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
        CountingOptions effective = Effective(textMode, counting);
        var resolver = new SelectionResolver(
            Chapters, Verses, View(effective), Segmentation(textMode, effective),
            Display, CountingText(textMode, effective).NormalizeWord, textMode);
        return resolver.Resolve(selection);
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
