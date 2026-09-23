using System.Globalization;
using System.Text;
using QuranCode.Core;
using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Search.Numbers;
using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Selection lists (word and letter frequencies, the Maths sums, symmetry,
/// the Allah statistics), the research word lists and ratio coloring.
/// </summary>
internal sealed partial class Handlers
{
    /// <summary>The research lists are paged; the whole table comes only as text.</summary>
    public const int MaxResearchRows = 1000;

    public WordFrequenciesDto SelectionWords(SelectionParams p)
    {
        VerseRange range = RequireRange(p.First, p.Last);
        IReadOnlyList<WordCount> words = _engine.WordFrequencies(range, SystemName(p.ValueSystem), Counting(p.Counting), p.WithMarks);
        return new WordFrequenciesDto(words.Sum(w => w.Count), words.Count, words.Select(w => new WordCountDto(w.Word, w.Count)).ToArray());
    }

    public IReadOnlyList<LetterStatisticDto> SelectionLetters(SelectionParams p)
    {
        VerseRange range = RequireRange(p.First, p.Last);
        LetterPositionScope scope = p.Scope switch
        {
            null or "book" => LetterPositionScope.Book,
            "chapter" => LetterPositionScope.Chapter,
            "verse" => LetterPositionScope.Verse,
            "word" => LetterPositionScope.Word,
            _ => throw RpcException.InvalidParams("scope must be book, chapter, verse or word."),
        };
        return _engine.LetterStatistics(range, SystemName(p.ValueSystem), Counting(p.Counting), scope)
            .Select(l => new LetterStatisticDto(l.Letter.ToString(), l.Order, l.Count, l.PositionSum, l.DistanceSum))
            .ToArray();
    }

    public MathsDto SelectionMaths(SelectionParams p)
    {
        VerseRange range = RequireRange(p.First, p.Last);
        CountingOptions counting = Counting(p.Counting);
        return new MathsDto(
            Cv(_engine.ChapterSums(range, counting, p.AbsoluteDifference, p.VOverC)),
            Cv(_engine.VerseSums(range, counting, p.AbsoluteDifference, p.VOverC)));
    }

    public SymmetryDto SelectionSymmetry(SelectionParams p)
    {
        VerseRange range = RequireRange(p.First, p.Last);
        SymmetryKind kind = p.Kind switch
        {
            null or "wordLetters" => SymmetryKind.WordLetters,
            "verseWords" => SymmetryKind.VerseWords,
            "verseLetters" => SymmetryKind.VerseLetters,
            _ => throw RpcException.InvalidParams("kind must be wordLetters, verseWords or verseLetters."),
        };
        SymmetryResult result = _engine.Symmetry(range, kind, p.Boundaries, SystemName(p.ValueSystem), Counting(p.Counting));
        return new SymmetryDto(
            result.Units,
            result.Points.Select(x => new SymmetryPointDto(x.Position, x.Total, x.PositionSum, x.TotalSum)).ToArray(),
            Math.Round(result.Percent, 3));
    }

    public AllahSummaryDto SelectionAllah(SelectionParams p)
    {
        AllahSummary s = _engine.AllahSummary(RequireRange(p.First, p.Last), SystemName(p.ValueSystem), Counting(p.Counting));
        return new AllahSummaryDto(s.Allah, s.WithAllah, s.WithLillah, s.Total);
    }

    public ResearchTableDto ResearchWordList(ResearchParams p)
    {
        WordListKind kind = p.Method switch
        {
            "allah" => WordListKind.Allah,
            "nonAllah" => WordListKind.NonAllah,
            "all" => WordListKind.All,
            "double" => WordListKind.Double,
            "repeated" => WordListKind.Repeated,
            _ => throw RpcException.InvalidParams("method must be allah, nonAllah, all, double or repeated."),
        };
        if (p.Gap is < 0 or > 100) throw RpcException.InvalidParams("gap must be between 0 and 100.");
        VerseRange? range = (p.First, p.Last) switch
        {
            (null, null) => null,
            (int first, int last) => RequireRange(first, last),
            _ => throw RpcException.InvalidParams("give both first and last, or neither for the whole book."),
        };

        ResearchTable table = _engine.WordList(kind, range, SystemName(p.ValueSystem), Counting(p.Counting), p.Gap);
        if (p.Tsv)
        {
            var text = new StringBuilder();
            text.AppendJoin('\t', table.Columns).Append('\n');
            foreach (IReadOnlyList<string> row in table.Rows) text.AppendJoin('\t', row).Append('\n');
            return new ResearchTableDto(table.Columns, table.Rows.Count, 0, [], text.ToString());
        }

        int offset = p.Offset ?? 0;
        int limit = p.Limit ?? DefaultSearchLimit;
        if (offset < 0) throw RpcException.InvalidParams("offset must not be negative.");
        if (limit is < 1 or > MaxResearchRows) throw RpcException.InvalidParams($"limit must be between 1 and {MaxResearchRows}.");
        return new ResearchTableDto(table.Columns, table.Rows.Count, offset, table.Rows.Skip(offset).Take(limit).ToArray(), null);
    }

    public IReadOnlyList<RatioUnitDto> RatioSplit(RatioParams p)
    {
        RequireChapter(p.Chapter);
        double ratio = p.Ratio ?? QuranCode.Core.Analysis.RatioSplit.GoldenRatio;
        if (ratio is < 0 or > 1 || double.IsNaN(ratio)) throw RpcException.InvalidParams("ratio must be between 0 and 1.");
        if (p.Length is "long") ratio = 1 - ratio;
        else if (p.Length is not (null or "short")) throw RpcException.InvalidParams("length must be short or long.");

        UnitKind? scope = p.Scope switch
        {
            null or "verse" => UnitKind.Verses,
            "chapter" => UnitKind.Chapters,
            "page" => UnitKind.Pages,
            "station" => UnitKind.Stations,
            "part" => UnitKind.Parts,
            "group" => UnitKind.Groups,
            "half" => UnitKind.Halves,
            "quarter" => UnitKind.Quarters,
            "bowing" => UnitKind.Bowings,
            "book" => null,
            _ => throw RpcException.InvalidParams("scope must be verse, chapter, page, station, part, group, half, quarter, bowing or book."),
        };
        RatioMeasure measure = p.Measure switch
        {
            null or "letters" => RatioMeasure.Letters,
            "value" => RatioMeasure.Value,
            _ => throw RpcException.InvalidParams("measure must be letters or value."),
        };
        RatioBoundary boundary = p.Boundary switch
        {
            null or "letter" => RatioBoundary.Letter,
            "word" => RatioBoundary.Word,
            "sentence" => RatioBoundary.Sentence,
            "verse" => RatioBoundary.Verse,
            "chapter" => RatioBoundary.Chapter,
            _ => throw RpcException.InvalidParams("boundary must be letter, word, sentence, verse or chapter."),
        };

        string system = SystemName(p.ValueSystem);
        CountingOptions counting = Counting(p.Counting);
        string textMode = RequireSystem(system).TextMode;
        IReadOnlyList<RatioSplitResult> splits = _engine.RatioSplits(p.Chapter, scope, ratio, measure, boundary, system, counting);

        Segmentation s = _engine.Segmentation(textMode, counting);
        CorpusView view = _engine.View(counting);
        CountingText text = _engine.CountingText(textMode, counting);
        return splits.Select(split => RatioUnit(split, s, view, text)).ToArray();
    }

    /// <summary>Where a split falls among a verse's displayed words.</summary>
    private RatioUnitDto RatioUnit(RatioSplitResult split, Segmentation s, CorpusView view, CountingText text)
    {
        int verseIndex = s.WordVerse[split.Word];
        Verse verse = view.Verses[verseIndex];
        VerseDisplay display = _engine.Display(verse);
        int inVerse = split.Word - s.VerseFirstWord[verseIndex];
        (int word, int letters) = (inVerse - display.WordOffset, split.LetterInWord);

        DisplaySpan[]? spans = DisplayWords.Align(display, s.VerseWords(verseIndex), text.NormalizeWord);
        if (spans is not null && inVerse >= display.WordOffset)
        {
            // Counted words sharing a display word (waw as a word) come before this one's letters.
            int remaining = split.LetterInWord;
            for (int j = inVerse - 1; j >= display.WordOffset && spans[j].First == spans[inVerse].First; j--)
            {
                remaining += s.WordLetterCount[s.VerseFirstWord[verseIndex] + j];
            }

            DisplaySpan span = spans[inVerse];
            (word, letters) = (span.First + span.Count - 1, remaining);
            for (int d = span.First; d < span.First + span.Count; d++)
            {
                int inDisplay = LetterCount(display.Words[d]);
                if (remaining <= inDisplay)
                {
                    (word, letters) = (d, remaining);
                    break;
                }
                remaining -= inDisplay;
            }
        }

        return new RatioUnitDto(
            view.Verses[split.FirstVerse].Number, view.Verses[split.LastVerse].Number, split.Colored,
            verse.Number, word, letters,
            split.FirstLetters, split.FirstValue.ToString(CultureInfo.InvariantCulture),
            split.SecondLetters, split.SecondValue.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Letters of a displayed word, not its marks or tatweel.</summary>
    private static int LetterCount(string displayWord) => displayWord.Count(c => char.IsLetter(c) && c != 'ـ');

    private static CvSumsDto Cv(CvSums s) => new(s.Count, Q(s.C), Q(s.V), Q(s.Plus), Q(s.Minus), Q(s.Times), Q(s.Divided));

    private static QuantitySumsDto Q(QuantitySums q) => new(q.Sum, q.Odd, q.Even, q.Prime, q.Composite, q.Ratio);

    private string SystemName(string? valueSystem) => RequireSystem(valueSystem).Name;
}
