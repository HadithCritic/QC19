using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;

namespace QuranCode.Core;

/// <summary>Exact selections, from a chapter down to a letter (docs/specs/research-selection.md).</summary>
public sealed partial class QuranCodeEngine
{
    /// <summary>
    /// The counted text a selection covers under a text mode and options, or why
    /// it cannot be resolved.
    /// </summary>
    public SelectionResolution Resolve(
        QuranSelection selection, string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return Resolver(textMode, counting).Resolve(selection);
    }

    private SelectionResolver Resolver(string textMode, CountingOptions? counting)
    {
        CountingOptions effective = Effective(textMode, counting);
        return new SelectionResolver(
            Chapters, Verses, View(effective), Segmentation(textMode, effective),
            Display, CountingText(textMode, effective).NormalizeWord, textMode);
    }

    /// <summary>
    /// A selection's value in each of several systems. Each text mode is
    /// resolved once; a mode in which the selection cannot be resolved (its
    /// letters do not match one by one there) gives that resolution's error.
    /// </summary>
    public IReadOnlyList<SystemSelectionValue> Values(
        QuranSelection selection, IEnumerable<string> valueSystems, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(valueSystems);

        var resolutions = new Dictionary<string, SelectionResolution>(StringComparer.Ordinal);
        var values = new List<SystemSelectionValue>();
        foreach (string name in valueSystems)
        {
            ValueSystem system = ValueSystem(name);
            string mode = system.TextModeName;
            if (!resolutions.TryGetValue(mode, out SelectionResolution? resolution))
            {
                resolutions[mode] = resolution = Resolve(selection, mode, counting);
            }

            long? value = resolution.Span is CountedSpan span
                ? SelectionStatistics.ValueOf(
                    Segmentation(mode, counting), View(Effective(mode, counting)), Chapters, span,
                    system, CalculationProfile.Default, ModifierSet.None)
                : null;
            values.Add(new SystemSelectionValue(name, resolution.Span?.LetterCount ?? 0, value, resolution.Error));
        }
        return values;
    }

    /// <summary>
    /// A selection listed verse by verse, word by word or letter by letter,
    /// each row with its counted letters and value.
    /// </summary>
    /// <remarks>
    /// Rows are valued as runs of letters, the way a partial selection is, so
    /// they add up to the selection's value. Only a Base system's single verse
    /// is valued differently on its own (see <see cref="SegmentedCalculator.ValueOfVerse"/>).
    /// A Bismillah header the classic edition counts inside verse 1 is a row of
    /// its own, so no counted letter is left out.
    /// </remarks>
    public SelectionBreakdown Breakdown(
        QuranSelection selection, BreakdownUnit unit, string valueSystem = DefaultValueSystem,
        CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ValueSystem system = ValueSystem(valueSystem);
        string mode = system.TextModeName;
        SelectionResolver resolver = Resolver(mode, counting);
        SelectionResolution resolution = resolver.Resolve(selection);
        if (resolution.Span is not CountedSpan span) return new SelectionBreakdown(resolution, []);

        Segmentation s = Segmentation(mode, counting);
        CorpusView view = View(Effective(mode, counting));
        var rows = new List<BreakdownRow>();
        void Add(QuranLocation location, string text, int first, int last)
        {
            (int a, int b) = (Math.Max(first, span.FirstLetter), Math.Min(last, span.LastLetter));
            long value = a > b
                ? 0
                : SegmentedCalculator.ValueOfLetters(s, a, b, system, CalculationProfile.Default, ModifierSet.None);
            rows.Add(new BreakdownRow(location, text, Math.Max(0, b - a + 1), value));
        }

        for (int v = span.FirstVerse; v <= span.LastVerse; v++)
        {
            BreakdownVerse(resolver, s, view.Verses[v], v, unit, span, Add);
        }
        return new SelectionBreakdown(resolution, rows);
    }

    private void BreakdownVerse(
        SelectionResolver resolver, Segmentation s, Verse verse, int index, BreakdownUnit unit, CountedSpan span,
        Action<QuranLocation, string, int, int> add)
    {
        VerseDisplay display = Display(verse);
        var at = new QuranLocation(verse.ChapterNumber, verse.NumberInChapter);
        int firstWord = s.VerseFirstWord[index], lastWord = firstWord + s.VerseWordCount[index] - 1;
        int verseStart = s.WordFirstLetter[firstWord];
        int verseEnd = s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord] - 1;

        IReadOnlyList<DisplayWordLetters>? words = unit == BreakdownUnit.Verse ? null : resolver.WordsOf(index);
        if (words is null)
        {
            add(at, string.Join(' ', display.Words), verseStart, verseEnd);
            return;
        }

        // The classic header's counted words come before the first display word.
        int headerEnd = words.Count > 0 ? words[0].FirstLetter - 1 : verseEnd;
        if (display.Bismillah is string header && headerEnd >= Math.Max(verseStart, span.FirstLetter) && verseStart <= span.LastLetter)
        {
            add(at, header, verseStart, headerEnd);
        }

        foreach (DisplayWordLetters word in words)
        {
            if (word.LastLetter < span.FirstLetter || word.FirstLetter > span.LastLetter) continue;
            string text = string.Join(' ', display.Words.Skip(word.Word).Take(word.Count));
            QuranLocation location = at with { Word = word.Word + 1 };
            if (unit == BreakdownUnit.Word || word.LetterEnds is null)
            {
                add(location, text, word.FirstLetter, word.LastLetter);
                continue;
            }

            for (int k = 0; k < word.LetterEnds.Length; k++)
            {
                int from = word.FirstLetter + (k == 0 ? 0 : word.LetterEnds[k - 1]);
                int to = word.FirstLetter + word.LetterEnds[k] - 1;
                // A letter that produces nothing counts where it stands; others must overlap the span.
                bool inside = from <= to
                    ? to >= span.FirstLetter && from <= span.LastLetter
                    : from >= span.FirstLetter && from <= span.LastLetter;
                if (inside) add(location with { Letter = k + 1 }, DisplayLetters.Letter(text, k + 1), from, to);
            }
        }
    }

    /// <summary>
    /// A selection resolved in a value system's own text mode, which is what
    /// every span passed to the span overloads below must be.
    /// </summary>
    public SelectionResolution ResolveFor(
        QuranSelection selection, string valueSystem = DefaultValueSystem, CountingOptions? counting = null) =>
        Resolve(selection, ValueSystem(valueSystem).TextModeName, counting);

    /// <summary>
    /// Everything the inspector shows for a selection: its statistics, distinct
    /// words, and where it starts and ends in the counted text.
    /// </summary>
    public SelectionAnalysis Analyze(
        QuranSelection selection, string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        SelectionResolution resolution = ResolveFor(selection, valueSystem, counting);
        if (resolution.Span is not CountedSpan span) return new SelectionAnalysis(resolution, null, 0, null, null);

        ValueSystem system = ValueSystem(valueSystem);
        Segmentation segmentation = Segmentation(system.TextModeName, counting);
        CorpusView view = View(Effective(system.TextModeName, counting));
        SelectionStatistics statistics = SelectionStatistics.Compute(
            segmentation, view, Chapters, span, system, CalculationProfile.Default, ModifierSet.None);

        return new SelectionAnalysis(
            resolution,
            statistics,
            WordFrequencies(span, valueSystem, counting).Count,
            CountedPosition.Of(segmentation, view, span.FirstLetter),
            CountedPosition.Of(segmentation, view, span.LastLetter));
    }

    /// <summary>Word frequencies of the counted words a span touches.</summary>
    public IReadOnlyList<WordCount> WordFrequencies(
        CountedSpan span, string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(span);
        IReadOnlyList<string> words = Search(ValueSystem(valueSystem).TextModeName, counting).WordTexts;
        return SelectionLists.WordFrequencies(Enumerable.Range(span.FirstWord, span.WordCount).Select(w => words[w]));
    }

    /// <summary>Letter statistics of exactly the letters of a span.</summary>
    public IReadOnlyList<LetterStatistic> LetterStatistics(
        CountedSpan span, string valueSystem = DefaultValueSystem, CountingOptions? counting = null,
        LetterPositionScope scope = LetterPositionScope.Book)
    {
        ArgumentNullException.ThrowIfNull(span);
        return SelectionLists.LettersBetween(
            Segmentation(ValueSystem(valueSystem).TextModeName, counting), span.FirstLetter, span.LastLetter, scope);
    }

    /// <summary>Front-back symmetry of a span; a word or verse cut by the span counts only its selected part.</summary>
    public SymmetryResult Symmetry(
        CountedSpan span, SymmetryKind kind, bool withBoundaries,
        string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(span);
        Segmentation s = Segmentation(ValueSystem(valueSystem).TextModeName, counting);

        long Letters(int firstLetter, int count) =>
            Math.Max(0, Math.Min(firstLetter + count - 1, span.LastLetter) - Math.Max(firstLetter, span.FirstLetter) + 1);
        long WordLetters(int w) => Letters(s.WordFirstLetter[w], s.WordLetterCount[w]);
        IEnumerable<int> WordsOf(int v) =>
            Enumerable.Range(s.VerseFirstWord[v], s.VerseWordCount[v]).Where(w => w >= span.FirstWord && w <= span.LastWord);

        IEnumerable<int> verses = Enumerable.Range(span.FirstVerse, span.VerseCount);
        long[] units = kind switch
        {
            SymmetryKind.WordLetters => Enumerable.Range(span.FirstWord, span.WordCount).Select(WordLetters).ToArray(),
            SymmetryKind.VerseWords => verses.Select(v => (long)WordsOf(v).Count()).ToArray(),
            _ => verses.Select(v => WordsOf(v).Sum(WordLetters)).ToArray(),
        };
        return SelectionLists.Symmetry(units, withBoundaries);
    }

    /// <summary>The Allah statistics of the counted words a span touches.</summary>
    public AllahSummary AllahSummary(
        CountedSpan span, string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(span);
        IReadOnlyList<string> words = Search(ValueSystem(valueSystem).TextModeName, counting).WordTexts;
        return ResearchWords.Allah(Enumerable.Range(span.FirstWord, span.WordCount).Select(w => words[w]));
    }

    /// <summary>The absolute verses a span touches, for analyses that work on whole verses.</summary>
    public VerseRange VersesOf(CountedSpan span, string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(span);
        CorpusView view = View(Effective(ValueSystem(valueSystem).TextModeName, counting));
        return new VerseRange(view.Verses[span.FirstVerse].Number, view.Verses[span.LastVerse].Number);
    }
}

/// <summary>How a breakdown lists a selection.</summary>
public enum BreakdownUnit
{
    Verse,
    Word,
    Letter,
}

/// <summary>One row of a breakdown: where it is, the display text, and its counted letters and value.</summary>
public sealed record BreakdownRow(QuranLocation Location, string Text, int Letters, long Value);

public sealed record SelectionBreakdown(SelectionResolution Resolution, IReadOnlyList<BreakdownRow> Rows);

/// <summary>A selection's value in one system; null with an error when it cannot be resolved there.</summary>
public sealed record SystemSelectionValue(string ValueSystem, int Letters, long? Value, string? Error);

/// <summary>
/// Where a counted letter sits: display chapter and verse, and counted
/// positions of its word and itself. Absolute word and letter numbers are
/// 1-based in the counted text, so they change with the options.
/// </summary>
public sealed record CountedPosition(
    int Chapter,
    int Verse,
    int AbsoluteVerse,
    int WordInVerse,
    int WordInChapter,
    int AbsoluteWord,
    int LetterInWord,
    int LetterInVerse,
    int LetterInChapter,
    int AbsoluteLetter)
{
    public static CountedPosition Of(Segmentation segmentation, CorpusView view, int letter)
    {
        ArgumentNullException.ThrowIfNull(segmentation);
        ArgumentNullException.ThrowIfNull(view);
        int word = segmentation.LetterWord[letter];
        int verse = segmentation.WordVerse[word];
        return new CountedPosition(
            segmentation.VerseChapter[verse],
            segmentation.VerseNumberInChapter[verse],
            view.Verses[verse].Number,
            segmentation.WordNumberInVerse[word],
            segmentation.WordNumberInChapter[word],
            word + 1,
            segmentation.LetterNumberInWord[letter],
            segmentation.LetterNumberInVerse[letter],
            segmentation.LetterNumberInChapter[letter],
            letter + 1);
    }
}

/// <summary>
/// A selection's analysis: its resolution, and when anything in it is
/// counted, its statistics and endpoints.
/// </summary>
public sealed record SelectionAnalysis(
    SelectionResolution Resolution,
    SelectionStatistics? Statistics,
    int DistinctWords,
    CountedPosition? Start,
    CountedPosition? End);
