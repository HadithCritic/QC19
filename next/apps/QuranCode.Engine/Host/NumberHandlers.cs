using System.Globalization;
using QuranCode.Core;
using QuranCode.Core.Content;
using QuranCode.Core.Search;
using QuranCode.Core.Search.Numbers;
using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Find by numbers and by letter frequency (Features.txt #14, #25, #26, #32
/// to #35, #54). One page of found units becomes references, measures and a
/// few of their verses; the whole result becomes per-chapter counts.
/// </summary>
internal sealed partial class Handlers
{
    /// <summary>Verses shown under each found unit.</summary>
    public const int PreviewVerses = 3;

    /// <summary>A criterion value beyond this is surely a typing slip.</summary>
    private const long MaxCriterionValue = 1_000_000_000_000_000;

    public UnitSearchResultDto SearchNumbers(NumberSearchParams p)
    {
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        string system = p.ValueSystem ?? QuranCodeEngine.DefaultValueSystem;
        var query = new NumberQuery(ParseUnit(p.Unit), ParseShape(p.Shape), p.Size, ParseNumberScope(p.NumberScope))
        {
            Number = ParseCriterion(p.Number, "number"),
            Verses = ParseCriterion(p.Verses, "verses"),
            Words = ParseCriterion(p.Words, "words"),
            Letters = ParseCriterion(p.Letters, "letters"),
            UniqueLetters = ParseCriterion(p.UniqueLetters, "uniqueLetters"),
            Value = ParseCriterion(p.Value, "value"),
            Frequency = ParseCriterion(p.Frequency, "frequency"),
            Occurrence = ParseCriterion(p.Occurrence, "occurrence"),
        };

        NumberSearch search = _engine.NumberSearch(system, counting);
        NumberSearchResult result = Run(() => search.Find(query, ScopeIndexes(p.Scope, counting)));
        return UnitPage(result, search, p.Offset, p.Limit, textMode, counting, frequency: false);
    }

    public UnitSearchResultDto SearchFrequency(FrequencySearchParams p)
    {
        RequireText(p.Phrase, "phrase");
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        string system = p.ValueSystem ?? QuranCodeEngine.DefaultValueSystem;

        // The phrase is counted in the same letters as the text.
        CountingText text = _engine.CountingText(textMode, counting);
        string phrase = string.Join("", p.Phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(text.NormalizeWord));

        LetterMatch? match = p.Match switch
        {
            null => null,
            "all" => LetterMatch.AllLettersOf,
            "any" => LetterMatch.AnyLetterOf,
            "only" => LetterMatch.OnlyLettersOf,
            "none" => LetterMatch.NoLetterOf,
            _ => throw RpcException.InvalidParams("match must be all, any, only or none."),
        };
        var query = new FrequencyQuery(
            ParseUnit(p.Unit), phrase, ParseShape(p.Shape), p.Size, p.UniqueLetters,
            p.Sum is null ? null : ParseCriterion(p.Sum, "sum"), match);

        NumberSearch search = _engine.NumberSearch(system, counting);
        NumberSearchResult result = Run(() => new FrequencySearch(search).Find(query, ScopeIndexes(p.Scope, counting)));
        return UnitPage(result, search, p.Offset, p.Limit, textMode, counting, frequency: true);
    }

    /// <summary>The engine's refusals are the caller's to fix.</summary>
    private static NumberSearchResult Run(Func<NumberSearchResult> search)
    {
        try
        {
            return search();
        }
        catch (ArgumentException ex)
        {
            throw RpcException.InvalidParams(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw RpcException.InvalidParams(ex.Message);
        }
    }

    /// <summary>The counted verses a scope names, as indexes in order; null for the whole book.</summary>
    private List<int>? ScopeIndexes(ScopeDto? scope, CountingOptions counting)
    {
        HashSet<int>? numbers = Scope(scope);
        if (numbers is null) return null;
        CorpusView view = _engine.View(counting);
        return numbers.Select(view.IndexOf).Where(i => i >= 0).Order().ToList();
    }

    private UnitSearchResultDto UnitPage(
        NumberSearchResult result, NumberSearch search, int? offset, int? limit,
        string textMode, CountingOptions counting, bool frequency)
    {
        int from = offset ?? 0;
        int take = limit ?? DefaultSearchLimit;
        if (from < 0) throw RpcException.InvalidParams("offset must not be negative.");
        if (take is < 1 or > MaxSearchLimit) throw RpcException.InvalidParams($"limit must be between 1 and {MaxSearchLimit}.");

        Segmentation segmentation = _engine.Segmentation(textMode, counting);
        CorpusView view = _engine.View(counting);
        CountingText text = _engine.CountingText(textMode, counting);

        var chapterCounts = new int[_engine.Chapters.Count];
        var allVerses = new SortedSet<int>();
        foreach (FoundUnit unit in result.Units)
        {
            IReadOnlyList<int> verses = CoveredVerses(unit, search, segmentation);
            chapterCounts[segmentation.VerseChapter[verses[0]] - 1]++;
            foreach (int v in verses) allVerses.Add(view.Verses[v].Number);
        }

        FoundUnitDto[] units = result.Units.Skip(from).Take(take)
            .Select(unit => UnitDto(unit, search, segmentation, view, text, frequency))
            .ToArray();
        return new UnitSearchResultDto(result.Units.Count, result.Truncated, from, units, chapterCounts, [.. allVerses]);
    }

    /// <summary>The counted verse indexes a unit covers, in order.</summary>
    private static IReadOnlyList<int> CoveredVerses(FoundUnit unit, NumberSearch search, Segmentation s)
    {
        switch (unit.Unit)
        {
            case UnitKind.Words:
                return unit.Items.Select(w => s.WordVerse[w]).Distinct().Order().ToArray();
            case UnitKind.Sentences:
                return Enumerable.Range(s.WordVerse[unit.Items[0]], s.WordVerse[unit.Items[1]] - s.WordVerse[unit.Items[0]] + 1).ToArray();
            case UnitKind.Verses:
                return unit.Items.Order().ToArray();
            default:
                Dictionary<int, Block> blocks = search.BlocksOf(unit.Unit).ToDictionary(b => b.Number);
                return unit.Items
                    .SelectMany(n => Enumerable.Range(blocks[n].FirstVerse, blocks[n].LastVerse - blocks[n].FirstVerse + 1))
                    .Distinct().Order().ToArray();
        }
    }

    private FoundUnitDto UnitDto(
        FoundUnit unit, NumberSearch search, Segmentation s, CorpusView view, CountingText text, bool frequency)
    {
        IReadOnlyList<int> verses = CoveredVerses(unit, search, s);
        int[] numbers = verses.Select(v => view.Verses[v].Number).ToArray();
        bool contiguous = numbers[^1] - numbers[0] + 1 == numbers.Length;

        // Words (and sentences) are marked in their verses; larger units show their verses plain.
        int[] words = unit.Unit switch
        {
            UnitKind.Words => [.. unit.Items],
            UnitKind.Sentences => Enumerable.Range(unit.Items[0], unit.Items[1] - unit.Items[0] + 1).ToArray(),
            _ => [],
        };
        SearchVerseDto[] preview = verses.Take(PreviewVerses).Select(v =>
        {
            int first = s.VerseFirstWord[v];
            int[] marked = words.Where(w => s.WordVerse[w] == v).Select(w => w - first).ToArray();
            var hit = new VerseHit(view.Verses[v].Number, marked, marked.Length > 0 ? HitWords.Counted : HitWords.None);
            return SearchVerse(hit, s, view, text);
        }).ToArray();

        Tally t = unit.Tally;
        return new FoundUnitDto(
            UnitReference(unit, s, view, numbers),
            numbers[0], numbers[^1], contiguous ? null : numbers,
            t.Verses, t.Words, t.Letters, t.UniqueLetters, Number(t.Value),
            frequency ? t.LetterFrequencySum.ToString(CultureInfo.InvariantCulture) : null,
            preview, verses.Count > PreviewVerses);
    }

    private string UnitReference(FoundUnit unit, Segmentation s, CorpusView view, int[] numbers)
    {
        string VerseRef(int v) => $"{s.VerseChapter[v]}:{s.VerseNumberInChapter[v]}";
        string WordRef(int w) => $"{VerseRef(s.WordVerse[w])} word {s.WordNumberInVerse[w]}";
        string Join(IEnumerable<string> parts)
        {
            string[] all = parts.ToArray();
            return all.Length <= 6 ? string.Join(", ", all) : string.Join(", ", all.Take(6)) + $" and {all.Length - 6} more";
        }

        switch (unit.Unit)
        {
            case UnitKind.Words or UnitKind.Sentences:
            {
                int first = unit.Items[0], last = unit.Unit == UnitKind.Sentences ? unit.Items[1] : unit.Items[^1];
                if (unit.Shape == UnitShape.Set) return Join(unit.Items.Select(WordRef));
                int verse = s.WordVerse[first];
                bool wholeVerse = s.WordVerse[last] == verse && first == s.VerseFirstWord[verse] &&
                                  last == s.VerseFirstWord[verse] + s.VerseWordCount[verse] - 1;
                if (wholeVerse && (unit.Unit == UnitKind.Sentences || first != last)) return VerseRef(verse);
                if (first == last) return WordRef(first);
                return s.WordVerse[first] == s.WordVerse[last]
                    ? $"{VerseRef(s.WordVerse[first])} words {s.WordNumberInVerse[first]}-{s.WordNumberInVerse[last]}"
                    : $"{WordRef(first)} to {WordRef(last)}";
            }
            case UnitKind.Verses:
            {
                if (unit.Shape == UnitShape.Set) return Join(unit.Items.Select(VerseRef));
                int first = unit.Items[0], last = unit.Items[^1];
                if (first == last) return VerseRef(first);
                return s.VerseChapter[first] == s.VerseChapter[last]
                    ? $"{VerseRef(first)}-{s.VerseNumberInChapter[last]}"
                    : $"{VerseRef(first)}-{VerseRef(last)}";
            }
            default:
            {
                string name = BlockName(unit.Unit);
                if (unit.Items.Count == 1) return $"{name} {unit.Items[0]}";
                string plural = name == "Half" ? "Halves" : name + "s";
                return unit.Shape == UnitShape.Range
                    ? $"{plural} {unit.Items[0]}-{unit.Items[^1]}"
                    : $"{plural} {string.Join(", ", unit.Items)}";
            }
        }
    }

    private static string BlockName(UnitKind unit) => unit switch
    {
        UnitKind.Chapters => "Chapter",
        UnitKind.Pages => "Page",
        UnitKind.Stations => "Station",
        UnitKind.Parts => "Part",
        UnitKind.Groups => "Group",
        UnitKind.Halves => "Half",
        UnitKind.Quarters => "Quarter",
        _ => "Bowing",
    };

    private static UnitKind ParseUnit(string unit) => unit switch
    {
        "words" => UnitKind.Words,
        "verses" => UnitKind.Verses,
        "chapters" => UnitKind.Chapters,
        "sentences" => UnitKind.Sentences,
        "pages" => UnitKind.Pages,
        "stations" => UnitKind.Stations,
        "parts" => UnitKind.Parts,
        "groups" => UnitKind.Groups,
        "halves" => UnitKind.Halves,
        "quarters" => UnitKind.Quarters,
        "bowings" => UnitKind.Bowings,
        _ => throw RpcException.InvalidParams(
            "unit must be words, verses, chapters, sentences, pages, stations, parts, groups, halves, quarters or bowings."),
    };

    private static UnitShape ParseShape(string? shape) => shape switch
    {
        null or "single" => UnitShape.Single,
        "range" => UnitShape.Range,
        "set" => UnitShape.Set,
        _ => throw RpcException.InvalidParams("shape must be single, range or set."),
    };

    private static NumberScope? ParseNumberScope(string? scope) => scope switch
    {
        null => null,
        "book" => NumberScope.Book,
        "chapter" => NumberScope.Chapter,
        "verse" => NumberScope.Verse,
        _ => throw RpcException.InvalidParams("numberScope must be book, chapter or verse."),
    };

    private static Criterion ParseCriterion(CriterionDto? dto, string name)
    {
        if (dto is null) return Criterion.Any;

        long value = 0;
        if (!string.IsNullOrWhiteSpace(dto.Value) &&
            (!long.TryParse(dto.Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value) ||
             Math.Abs(value) > MaxCriterionValue))
        {
            throw RpcException.InvalidParams($"{name}: the value must be a whole number.");
        }

        Comparison comparison = dto.Comparison switch
        {
            null or "eq" => Comparison.Equal,
            "ne" => Comparison.NotEqual,
            "lt" => Comparison.Less,
            "le" => Comparison.LessOrEqual,
            "gt" => Comparison.Greater,
            "ge" => Comparison.GreaterOrEqual,
            "div" => Comparison.DivisibleBy,
            "ndiv" => Comparison.NotDivisibleBy,
            "sum" => Comparison.EqualSum,
            _ => throw RpcException.InvalidParams($"{name}: comparison must be eq, ne, lt, le, gt, ge, div, ndiv or sum."),
        };

        NumberType type = dto.Type is null or "none"
            ? NumberType.None
            : Enum.TryParse(dto.Type, ignoreCase: true, out NumberType parsed) && Enum.IsDefined(parsed) && dto.Type.All(char.IsLetter)
                ? parsed
                : throw RpcException.InvalidParams($"{name}: there is no number kind \"{Truncate(dto.Type)}\".");

        int remainder = dto.Remainder ?? 0;
        if (remainder < -1) throw RpcException.InvalidParams($"{name}: the remainder is -1 (any) or more.");
        return new Criterion(value, comparison, type, remainder);
    }
}
