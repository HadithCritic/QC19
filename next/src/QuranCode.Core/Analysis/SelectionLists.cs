using QuranCode.Core.Content;
using QuranCode.Core.Numbers;
using QuranCode.Core.Text;

namespace QuranCode.Core.Analysis;

/// <summary>A word and how many times it occurs.</summary>
public readonly record struct WordCount(string Word, int Count);

/// <summary>One row of the letter statistics (Features.txt #22).</summary>
/// <param name="Order">1-based order of first appearance.</param>
/// <param name="PositionSum">Sum of the letter's positions.</param>
/// <param name="DistanceSum">Sum of the distances from each occurrence to the one before; the first counts 0.</param>
public readonly record struct LetterStatistic(char Letter, int Order, int Count, long PositionSum, long DistanceSum);

/// <summary>Which position a letter's place is counted in.</summary>
public enum LetterPositionScope
{
    Book,
    Chapter,
    Verse,
    Word,
}

/// <summary>Sums of one quantity over a list, split by kind (Features.txt #12, #13).</summary>
/// <param name="Ratio">d/u: values that repeat, times how often, over values that occur once; null without any value that occurs once.</param>
public sealed record QuantitySums(double Sum, double Odd, double Even, double Prime, double Composite, double? Ratio);

/// <summary>The C, V, C+V, C−V, C×V and C÷V sums of a list of (C, V) pairs.</summary>
public sealed record CvSums(
    int Count,
    QuantitySums C,
    QuantitySums V,
    QuantitySums Plus,
    QuantitySums Minus,
    QuantitySums Times,
    QuantitySums Divided);

/// <summary>What front-back symmetry compares (Features.txt #8).</summary>
public enum SymmetryKind
{
    /// <summary>Letters per word.</summary>
    WordLetters,

    /// <summary>Words per verse.</summary>
    VerseWords,

    /// <summary>Letters per verse.</summary>
    VerseLetters,
}

/// <summary>A place where the running totals from the front and from the back agree.</summary>
/// <param name="Position">1-based count of units from each end.</param>
/// <param name="Total">The equal running total.</param>
/// <param name="PositionSum">1 + 2 + ... + Position.</param>
/// <param name="TotalSum">The running sum of the totals at the matching positions so far.</param>
public readonly record struct SymmetryPoint(int Position, long Total, long PositionSum, long TotalSum);

public sealed record SymmetryResult(int Units, IReadOnlyList<SymmetryPoint> Points, double Percent);

/// <summary>
/// Lists and sums over a selection that the original shows beside the text:
/// word and letter frequencies, C/V sums, and front-back symmetry.
/// </summary>
public static class SelectionLists
{
    /// <summary>
    /// Word frequencies (Features.txt #21), most frequent first and then in
    /// ordinal order of the text, so ties always list the same way (the
    /// original's order for ties is undefined).
    /// </summary>
    public static IReadOnlyList<WordCount> WordFrequencies(IEnumerable<string> words) =>
        words.GroupBy(w => w, StringComparer.Ordinal)
            .Select(g => new WordCount(g.Key, g.Count()))
            .OrderByDescending(w => w.Count)
            .ThenBy(w => w.Word, StringComparer.Ordinal)
            .ToArray();

    /// <summary>The words of a verse as written, marks in canonical order and stop marks left out.</summary>
    public static IEnumerable<string> WordsWithMarks(Verse verse) =>
        DisplayWords.Split(verse.Text)
            .Select(w => MarkOrder.Canonical(string.Join(' ', w.Split(' ').Where(t => t.Any(char.IsLetter)))))
            .Where(w => w.Length > 0);

    /// <summary>Letter statistics over a run of counted verses, in order of first appearance.</summary>
    public static IReadOnlyList<LetterStatistic> Letters(
        Segmentation s, int firstVerse, int lastVerse, LetterPositionScope scope)
    {
        ArgumentNullException.ThrowIfNull(s);
        int firstLetter = s.WordFirstLetter[s.VerseFirstWord[firstVerse]];
        int lastWord = s.VerseFirstWord[lastVerse] + s.VerseWordCount[lastVerse] - 1;
        return LettersBetween(s, firstLetter, s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord] - 1, scope);
    }

    /// <summary>Letter statistics over an inclusive run of counted letters, in order of first appearance.</summary>
    public static IReadOnlyList<LetterStatistic> LettersBetween(
        Segmentation s, int firstLetter, int lastLetter, LetterPositionScope scope)
    {
        ArgumentNullException.ThrowIfNull(s);
        var order = new List<char>();
        var counts = new Dictionary<char, (int Count, long PositionSum, long DistanceSum, long Last)>();

        for (int l = firstLetter; l <= lastLetter; l++)
        {
            char c = s.LetterChars[l];
            long position = scope switch
            {
                LetterPositionScope.Word => s.LetterNumberInWord[l],
                LetterPositionScope.Verse => s.LetterNumberInVerse[l],
                LetterPositionScope.Chapter => s.LetterNumberInChapter[l],
                _ => l + 1,
            };
            if (counts.TryGetValue(c, out var seen))
            {
                counts[c] = (seen.Count + 1, seen.PositionSum + position, seen.DistanceSum + position - seen.Last, position);
            }
            else
            {
                order.Add(c);
                counts[c] = (1, position, 0, position);
            }
        }

        return order.Select((c, i) => new LetterStatistic(c, i + 1, counts[c].Count, counts[c].PositionSum, counts[c].DistanceSum)).ToArray();
    }

    /// <summary>
    /// The Maths tab's sums (legacy DisplayMathsChapterSums and
    /// DisplayMathsVerseSums) over (C, V) pairs.
    /// </summary>
    /// <param name="absoluteDifference">|C − V| instead of C − V.</param>
    /// <param name="vOverC">V ÷ C instead of C ÷ V.</param>
    public static CvSums Cv(IReadOnlyList<(long C, long V)> pairs, bool absoluteDifference = false, bool vOverC = false)
    {
        ArgumentNullException.ThrowIfNull(pairs);
        QuantitySums Of(Func<(long C, long V), double> quantity) => Sums(pairs.Select(quantity).ToArray());
        return new CvSums(
            pairs.Count,
            Of(p => p.C),
            Of(p => p.V),
            Of(p => p.C + p.V),
            Of(p => absoluteDifference ? Math.Abs(p.C - p.V) : p.C - p.V),
            Of(p => (double)p.C * p.V),
            Of(p => vOverC ? (p.C == 0 ? 0 : (double)p.V / p.C) : (p.V == 0 ? 0 : (double)p.C / p.V)));
    }

    /// <summary>
    /// Sums of values by kind: odd, even, prime and composite test the whole
    /// part of the absolute value, and 1 is neither prime nor composite.
    /// </summary>
    public static QuantitySums Sums(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        double sum = 0, odd = 0, even = 0, prime = 0, composite = 0;
        foreach (double value in values)
        {
            long whole = Math.Abs((long)value);
            sum += value;
            if (whole % 2 == 1) odd += value;
            else even += value;
            if (NumberTheory.IsPrime(whole)) prime += value;
            else if (NumberTheory.IsComposite(whole)) composite += value;
        }

        // d/u keys a value by its whole part, as the original does for ÷.
        var frequency = values.GroupBy(v => (long)v).ToDictionary(g => g.Key, g => g.Count());
        double repeated = frequency.Where(f => f.Value > 1).Sum(f => (double)f.Key * f.Value);
        double single = frequency.Where(f => f.Value == 1).Sum(f => (double)f.Key);
        return new QuantitySums(sum, odd, even, prime, composite, single == 0 ? null : repeated / single);
    }

    /// <summary>
    /// Front-back symmetry (legacy symmetry tab): running totals of the units
    /// from the front and from the back, and every place they agree.
    /// </summary>
    /// <param name="withBoundaries">Also count the empty start and the full end, as the original's option does.</param>
    public static SymmetryResult Symmetry(IReadOnlyList<long> units, bool withBoundaries)
    {
        ArgumentNullException.ThrowIfNull(units);
        int n = units.Count;
        var points = new List<SymmetryPoint>();
        if (withBoundaries) points.Add(new SymmetryPoint(0, 0, 0, 0));

        long front = 0, back = 0, totalSum = 0;
        int last = withBoundaries ? n - 1 : n - 2;
        for (int i = 0; i <= last; i++)
        {
            front += units[i];
            back += units[n - 1 - i];
            if (front != back) continue;
            totalSum += front;
            points.Add(new SymmetryPoint(i + 1, front, (long)(i + 1) * (i + 2) / 2, totalSum));
        }

        double percent = n == 0 ? 0 : (points.Count - (withBoundaries ? 2 : 0)) * 100.0 / n;
        return new SymmetryResult(n, points, Math.Max(0, percent));
    }
}
