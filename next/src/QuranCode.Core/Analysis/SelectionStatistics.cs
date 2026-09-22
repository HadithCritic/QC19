using QuranCode.Core.Content;
using QuranCode.Core.Numerology;

namespace QuranCode.Core.Analysis;

/// <summary>How often one letter occurs in a selection.</summary>
public readonly record struct LetterFrequency(char Letter, int Count);

/// <summary>
/// Live statistics for a selection of verses: the panel the legacy app keeps
/// beside the text.
/// </summary>
public sealed record SelectionStatistics(
    VerseRange Range,
    string ValueSystem,
    int ChapterCount,
    int VerseCount,
    int WordCount,
    int LetterCount,
    int DistinctLetterCount,
    long Value,
    IReadOnlyList<LetterFrequency> LetterFrequencies)
{
    /// <summary>
    /// Counted verses before the selection and after it, within its chapter
    /// and within the book: Features.txt #11.
    /// </summary>
    public SelectionPosition Position { get; init; } = SelectionPosition.None;

    /// <summary>
    /// Computes statistics over a segmentation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value follows the path the legacy takes for the same selection,
    /// because the paths disagree in the word-level digit modes (see
    /// <see cref="SegmentedCalculator.ValueOfVerses"/>): one verse uses the
    /// single-verse path, exactly one whole chapter uses the chapter path, and
    /// anything else uses the aggregate path.
    /// </para>
    /// <para>
    /// Everything else is counted straight off the segmentation arrays, so the
    /// whole book takes one linear pass over 327,792 letters.
    /// </para>
    /// <para>
    /// The range is in absolute verse numbers; verses the view excludes (verse-0
    /// Bismillahs the user chose not to count) are skipped. A range holding only
    /// excluded verses gives empty statistics rather than an error.
    /// </para>
    /// </remarks>
    public static SelectionStatistics Compute(
        Segmentation segmentation,
        CorpusView view,
        IReadOnlyList<Chapter> chapters,
        VerseRange range,
        ValueSystem system,
        CalculationProfile profile,
        ModifierSet modifiers)
    {
        ArgumentNullException.ThrowIfNull(segmentation);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(chapters);
        ArgumentNullException.ThrowIfNull(system);
        int total = chapters[^1].LastVerse;
        if (range.First < 1 || range.Last > total || range.Last < range.First)
        {
            throw new ArgumentOutOfRangeException(nameof(range), $"verses run from 1 to {total}");
        }

        if (view.IndexRange(range) is not (int firstIndex, int lastIndex))
        {
            return new SelectionStatistics(range, system.Name, 0, 0, 0, 0, 0, 0, []);
        }

        SelectionPosition position = SelectionPosition.Of(segmentation, view, chapters, firstIndex, lastIndex);

        int firstWord = segmentation.VerseFirstWord[firstIndex];
        int endWord = segmentation.VerseFirstWord[lastIndex] + segmentation.VerseWordCount[lastIndex];
        int firstLetter = segmentation.WordFirstLetter[firstWord];
        int endLetter = endWord == firstWord
            ? firstLetter
            : segmentation.WordFirstLetter[endWord - 1] + segmentation.WordLetterCount[endWord - 1];

        var counts = new Dictionary<char, int>();
        for (int l = firstLetter; l < endLetter; l++)
        {
            char c = segmentation.LetterChars[l];
            counts[c] = counts.GetValueOrDefault(c) + 1;
        }

        LetterFrequency[] frequencies = counts
            .Select(p => new LetterFrequency(p.Key, p.Value))
            .OrderByDescending(f => f.Count)
            .ThenBy(f => f.Letter)
            .ToArray();

        int firstChapter = segmentation.VerseChapter[firstIndex];
        int lastChapter = segmentation.VerseChapter[lastIndex];

        return new SelectionStatistics(
            range,
            system.Name,
            lastChapter - firstChapter + 1,
            lastIndex - firstIndex + 1,
            endWord - firstWord,
            endLetter - firstLetter,
            frequencies.Length,
            ValueOf(segmentation, view, chapters, firstIndex, lastIndex, system, profile, modifiers),
            frequencies)
        {
            Position = position,
        };
    }

    private static long ValueOf(
        Segmentation segmentation, CorpusView view, IReadOnlyList<Chapter> chapters, int firstIndex, int lastIndex,
        ValueSystem system, CalculationProfile profile, ModifierSet modifiers)
    {
        int count = lastIndex - firstIndex + 1;
        if (count == 1)
        {
            return SegmentedCalculator.ValueOfVerse(segmentation, firstIndex, system, profile, modifiers);
        }

        // "Whole chapter" means every verse of it the view counts.
        Chapter chapter = chapters[segmentation.VerseChapter[firstIndex] - 1];
        (int First, int Last)? rows = view.IndexRange(new VerseRange(chapter.FirstVerse, chapter.LastVerse));
        if (rows == (firstIndex, lastIndex))
        {
            return SegmentedCalculator.ValueOfChapter(segmentation, chapter.Number, system, profile, modifiers);
        }

        return SegmentedCalculator.ValueOfVerses(segmentation, firstIndex, count, system, profile, modifiers);
    }
}
