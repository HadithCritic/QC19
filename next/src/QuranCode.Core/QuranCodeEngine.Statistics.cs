using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Search.Numbers;
using QuranCode.Core.Text;

namespace QuranCode.Core;

/// <summary>Selection lists and research word lists (Features.txt #3, #8, #10, #12, #13, #21, #22).</summary>
public sealed partial class QuranCodeEngine
{
    /// <summary>The counted verses a range covers, as segmentation indexes; null when none are counted.</summary>
    private (int First, int Last)? CountedRange(VerseRange range, CountingOptions? counting) => View(counting).IndexRange(range);

    /// <summary>Word frequencies in the counted text, or with marks as written (Original and SimplifiedMarks only in the original).</summary>
    public IReadOnlyList<WordCount> WordFrequencies(
        VerseRange range, string valueSystem = DefaultValueSystem, CountingOptions? counting = null, bool withMarks = false)
    {
        string textMode = ValueSystem(valueSystem).TextModeName;
        if (CountedRange(range, counting) is not var (first, last)) return [];

        if (withMarks)
        {
            IReadOnlyList<Verse> verses = View(counting).Verses;
            return SelectionLists.WordFrequencies(Enumerable.Range(first, last - first + 1).SelectMany(v => SelectionLists.WordsWithMarks(verses[v])));
        }
        Segmentation s = Segmentation(textMode, counting);
        IReadOnlyList<string> words = Search(textMode, counting).WordTexts;
        int start = s.VerseFirstWord[first];
        int end = s.VerseFirstWord[last] + s.VerseWordCount[last];
        return SelectionLists.WordFrequencies(Enumerable.Range(start, end - start).Select(w => words[w]));
    }

    public IReadOnlyList<LetterStatistic> LetterStatistics(
        VerseRange range, string valueSystem = DefaultValueSystem, CountingOptions? counting = null,
        LetterPositionScope scope = LetterPositionScope.Book)
    {
        string textMode = ValueSystem(valueSystem).TextModeName;
        return CountedRange(range, counting) is var (first, last)
            ? SelectionLists.Letters(Segmentation(textMode, counting), first, last, scope)
            : [];
    }

    /// <summary>C = chapter number and V = its counted verses, for every chapter the range touches (legacy chapter sums).</summary>
    public CvSums ChapterSums(VerseRange range, CountingOptions? counting = null, bool absoluteDifference = false, bool vOverC = false)
    {
        CorpusView view = View(counting);
        var pairs = Chapters
            .Where(c => c.FirstVerse <= range.Last && c.LastVerse >= range.First)
            .Select(c => ((long)c.Number, (long)view.Verses.Count(v => v.ChapterNumber == c.Number)))
            .ToArray();
        return SelectionLists.Cv(pairs, absoluteDifference, vOverC);
    }

    /// <summary>C = chapter number and V = verse number, for every counted verse in the range (legacy verse sums).</summary>
    public CvSums VerseSums(VerseRange range, CountingOptions? counting = null, bool absoluteDifference = false, bool vOverC = false)
    {
        if (CountedRange(range, counting) is not var (first, last)) return SelectionLists.Cv([], absoluteDifference, vOverC);
        IReadOnlyList<Verse> verses = View(counting).Verses;
        var pairs = Enumerable.Range(first, last - first + 1)
            .Select(v => ((long)verses[v].ChapterNumber, (long)verses[v].NumberInChapter))
            .ToArray();
        return SelectionLists.Cv(pairs, absoluteDifference, vOverC);
    }

    public SymmetryResult Symmetry(
        VerseRange range, SymmetryKind kind, bool withBoundaries,
        string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        string textMode = ValueSystem(valueSystem).TextModeName;
        if (CountedRange(range, counting) is not var (first, last)) return SelectionLists.Symmetry([], withBoundaries);

        Segmentation s = Segmentation(textMode, counting);
        long VerseLetters(int v) =>
            Enumerable.Range(s.VerseFirstWord[v], s.VerseWordCount[v]).Sum(w => (long)s.WordLetterCount[w]);

        long[] units = kind switch
        {
            SymmetryKind.WordLetters => Enumerable.Range(s.VerseFirstWord[first], s.VerseFirstWord[last] + s.VerseWordCount[last] - s.VerseFirstWord[first])
                .Select(w => (long)s.WordLetterCount[w]).ToArray(),
            SymmetryKind.VerseWords => Enumerable.Range(first, last - first + 1).Select(v => (long)s.VerseWordCount[v]).ToArray(),
            _ => Enumerable.Range(first, last - first + 1).Select(VerseLetters).ToArray(),
        };
        return SelectionLists.Symmetry(units, withBoundaries);
    }

    /// <summary>One of the research word lists over a range (the whole book when null).</summary>
    public ResearchTable WordList(
        WordListKind kind, VerseRange? range = null, string valueSystem = DefaultValueSystem,
        CountingOptions? counting = null, int gap = 0)
    {
        string textMode = ValueSystem(valueSystem).TextModeName;
        Segmentation s = Segmentation(textMode, counting);
        (int First, int Last)? span = range is { } r ? CountedRange(r, counting) : (0, s.VerseCount - 1);
        if (span is not var (first, last)) return new ResearchTable([], []);
        return ResearchWords.List(kind, s, Search(textMode, counting).WordTexts, WordValues(valueSystem, counting), first, last, gap);
    }

    /// <summary>
    /// Ratio splits (Features.txt #24) of the units a chapter shows: its verses,
    /// the chapter, the partitions that overlap it, or the whole book.
    /// </summary>
    /// <param name="scope">Verses, Chapters, a partition kind, or null for the whole book.</param>
    public IReadOnlyList<RatioSplitResult> RatioSplits(
        int chapter, UnitKind? scope, double ratio, RatioMeasure measure, RatioBoundary boundary,
        string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ValueSystem system = ValueSystem(valueSystem);
        Segmentation s = Segmentation(system.TextModeName, counting);
        NumberSearch numbers = NumberSearch(valueSystem, counting);

        Chapter c = Chapters[chapter - 1];
        if (CountedRange(new VerseRange(c.FirstVerse, c.LastVerse), counting) is not var (first, last)) return [];

        IReadOnlyList<(int, int)> units = scope switch
        {
            null => [(0, s.VerseCount - 1)],
            UnitKind.Verses => Enumerable.Range(first, last - first + 1).Select(v => (v, v)).ToArray(),
            UnitKind.Chapters => [(first, last)],
            UnitKind kind => numbers.BlocksOf(kind)
                .Where(b => b.FirstVerse <= last && b.LastVerse >= first)
                .Select(b => (b.FirstVerse, b.LastVerse)).ToArray(),
        };
        return Analysis.RatioSplit.Split(s, system, units, ratio, measure, boundary, numbers.Index.WordMarks);
    }

    /// <summary>The Allah statistics of the original's drawing, over a range.</summary>
    public AllahSummary AllahSummary(VerseRange range, string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        string textMode = ValueSystem(valueSystem).TextModeName;
        if (CountedRange(range, counting) is not var (first, last)) return new AllahSummary(0, 0, 0);
        Segmentation s = Segmentation(textMode, counting);
        IReadOnlyList<string> words = Search(textMode, counting).WordTexts;
        int start = s.VerseFirstWord[first];
        int end = s.VerseFirstWord[last] + s.VerseWordCount[last];
        return ResearchWords.Allah(Enumerable.Range(start, end - start).Select(w => words[w]));
    }
}
