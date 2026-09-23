using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Search.Numbers;

namespace QuranCode.Core.Analysis;

/// <summary>Whether a unit is split by its letters or by its value (legacy ratio Type).</summary>
public enum RatioMeasure
{
    Letters,
    Value,
}

/// <summary>Where a split must fall for the unit to be colored (legacy ratio Delimiter).</summary>
public enum RatioBoundary
{
    /// <summary>Anywhere, even inside a word.</summary>
    Letter,

    /// <summary>At the end of a word.</summary>
    Word,

    /// <summary>After a word followed by a pause mark, or at a verse end.</summary>
    Sentence,

    /// <summary>At the end of a verse (chapter and larger units).</summary>
    Verse,

    /// <summary>At the end of a chapter (the book).</summary>
    Chapter,
}

/// <summary>
/// Where the ratio splits one unit: after <see cref="Letters"/> of its
/// counted letters, which end <see cref="LetterInWord"/> letters into counted
/// word <see cref="Word"/>. Not <see cref="Colored"/> when the split misses the
/// boundary asked for.
/// </summary>
public sealed record RatioSplitResult(
    int FirstVerse,
    int LastVerse,
    bool Colored,
    int Letters,
    int Word,
    int LetterInWord,
    int FirstLetters,
    long FirstValue,
    int SecondLetters,
    long SecondValue);

/// <summary>
/// Ratio coloring (Features.txt #24): each unit's letters (or value) are
/// split at a ratio, 1/φ by default, and the two parts colored when the split
/// falls on the boundary asked for. Follows the legacy
/// <c>Colorize*UserRatiosText</c>: the split is after round(letters × r)
/// letters, or, by value, after the leading letters whose running value is at
/// most round(value × r).
/// </summary>
public static class RatioSplit
{
    /// <summary>1/φ, the original's default ratio.</summary>
    public static readonly double GoldenRatio = (Math.Sqrt(5) - 1) / 2;

    /// <param name="units">Runs of counted verse indexes to split.</param>
    /// <param name="marks">The pause mark after each counted word, for the sentence boundary.</param>
    public static IReadOnlyList<RatioSplitResult> Split(
        Segmentation s, ValueSystem system, IReadOnlyList<(int First, int Last)> units,
        double ratio, RatioMeasure measure, RatioBoundary boundary, IReadOnlyList<Stopmark> marks)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(marks);
        if (ratio is < 0 or > 1 || double.IsNaN(ratio)) throw new ArgumentOutOfRangeException(nameof(ratio), "A ratio is between 0 and 1.");

        return units.Select(u => SplitOne(s, system, u.First, u.Last, ratio, measure, boundary, marks)).ToArray();
    }

    private static RatioSplitResult SplitOne(
        Segmentation s, ValueSystem system, int firstVerse, int lastVerse,
        double ratio, RatioMeasure measure, RatioBoundary boundary, IReadOnlyList<Stopmark> marks)
    {
        int firstWord = s.VerseFirstWord[firstVerse];
        int lastWord = s.VerseFirstWord[lastVerse] + s.VerseWordCount[lastVerse] - 1;
        int start = s.WordFirstLetter[firstWord];
        int end = s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord];
        int total = end - start;

        long totalValue = 0;
        for (int l = start; l < end; l++) totalValue += system[s.LetterChars[l]];

        int count;
        if (measure == RatioMeasure.Letters)
        {
            count = (int)Math.Round(total * ratio, MidpointRounding.AwayFromZero);
        }
        else
        {
            long target = (long)Math.Round(totalValue * ratio, MidpointRounding.AwayFromZero);
            long running = 0;
            count = 0;
            while (start + count < end && running + system[s.LetterChars[start + count]] <= target)
            {
                running += system[s.LetterChars[start + count]];
                count++;
            }
        }
        count = Math.Clamp(count, 0, total);

        // The letter the split follows, and its word.
        int splitLetter = start + Math.Max(count, 1) - 1;
        int word = s.LetterWord[splitLetter];
        int inWord = splitLetter - s.WordFirstLetter[word] + 1;
        bool wordEnd = inWord == s.WordLetterCount[word];
        int verse = s.WordVerse[word];
        bool verseEnd = wordEnd && word == s.VerseFirstWord[verse] + s.VerseWordCount[verse] - 1;
        bool chapterEnd = verseEnd && (verse == s.VerseCount - 1 || s.VerseChapter[verse + 1] != s.VerseChapter[verse]);

        bool colored = count > 0 && count < total && boundary switch
        {
            RatioBoundary.Letter => true,
            RatioBoundary.Word => wordEnd,
            RatioBoundary.Sentence => wordEnd && (verseEnd || marks[word] != Stopmark.None),
            RatioBoundary.Verse => verseEnd,
            _ => chapterEnd,
        };

        long firstValue = 0;
        for (int l = start; l < start + count; l++) firstValue += system[s.LetterChars[l]];

        return new RatioSplitResult(
            firstVerse, lastVerse, colored, count, word, inWord,
            count, firstValue, total - count, totalValue - firstValue);
    }
}
