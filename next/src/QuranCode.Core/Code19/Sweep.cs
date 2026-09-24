using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;

namespace QuranCode.Core.Code19;

/// <summary>Which part of a sweep a total belongs to.</summary>
public enum SweepGroup
{
    /// <summary>Chapters, verses, words and letters.</summary>
    Counts,

    /// <summary>The selection's value in the current letter-value system.</summary>
    Value,

    /// <summary>Sums of chapter and verse numbers.</summary>
    Numbers,

    /// <summary>How often each letter occurs.</summary>
    Letters,
}

/// <summary>One total of a selection.</summary>
public sealed record SweepTotal(SweepGroup Group, string Label, long Value);

/// <summary>
/// Every total of a selection that a Code 19 argument is built from, in one
/// list, so a reader can see at once which of them divide by 19: the counts,
/// the value, the sums of chapter and verse numbers, and each letter's
/// frequency. Divisibility is left to the reader's chosen divisor.
/// </summary>
public static class Sweep
{
    public static IReadOnlyList<SweepTotal> Of(
        QuranCodeEngine engine, VerseRange range, string valueSystem, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(engine);
        SelectionStatistics stats = engine.Statistics(range, valueSystem, counting: counting);
        return Totals(engine, stats, engine.View(counting).IndexRange(range), valueSystem, counting);
    }

    /// <summary>The same totals for an exact selection, resolved in the value system's text mode.</summary>
    /// <remarks>Chapter and verse number sums take every verse the selection touches.</remarks>
    public static IReadOnlyList<SweepTotal> Of(
        QuranCodeEngine engine, CountedSpan span, string valueSystem, CountingOptions? counting = null)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(span);
        ValueSystem system = engine.ValueSystem(valueSystem);
        SelectionStatistics stats = SelectionStatistics.Compute(
            engine.Segmentation(system.TextModeName, counting), engine.View(counting), engine.Chapters, span,
            system, CalculationProfile.Default, ModifierSet.None);
        return Totals(engine, stats, (span.FirstVerse, span.LastVerse), valueSystem, counting);
    }

    private static List<SweepTotal> Totals(
        QuranCodeEngine engine, SelectionStatistics stats, (int First, int Last)? verses,
        string valueSystem, CountingOptions? counting)
    {
        var totals = new List<SweepTotal>
        {
            new(SweepGroup.Counts, "Chapters", stats.ChapterCount),
            new(SweepGroup.Counts, "Verses", stats.VerseCount),
            new(SweepGroup.Counts, "Words", stats.WordCount),
            new(SweepGroup.Counts, "Letters", stats.LetterCount),
            new(SweepGroup.Counts, "Distinct letters", stats.DistinctLetterCount),
            new(SweepGroup.Value, "Value", stats.Value),
        };

        CorpusView view = engine.View(counting);
        if (verses is (int first, int last))
        {
            Segmentation segmentation = engine.Segmentation(engine.ValueSystem(valueSystem).TextModeName, counting);
            long chapterSum = 0, verseSum = 0, absoluteSum = 0;
            int previousChapter = 0;
            for (int v = first; v <= last; v++)
            {
                int chapter = segmentation.VerseChapter[v];
                if (chapter != previousChapter) chapterSum += chapter;
                previousChapter = chapter;
                verseSum += segmentation.VerseNumberInChapter[v];
                absoluteSum += view.Verses[v].Number;
            }
            totals.Add(new(SweepGroup.Numbers, "Sum of chapter numbers", chapterSum));
            totals.Add(new(SweepGroup.Numbers, "Sum of verse numbers", verseSum));
            totals.Add(new(SweepGroup.Numbers, "Sum of verse numbers in the book", absoluteSum));
            totals.Add(new(SweepGroup.Numbers, "Chapter numbers + verses", chapterSum + stats.VerseCount));
        }

        foreach (LetterFrequency f in stats.LetterFrequencies.OrderBy(f => f.Letter))
            totals.Add(new(SweepGroup.Letters, f.Letter.ToString(), f.Count));

        return totals;
    }
}
