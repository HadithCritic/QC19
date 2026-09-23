using QuranCode.Core.Numbers;

namespace QuranCode.Core.Analysis;

/// <summary>A step of an index chain: the number's 1-based index among primes (P) or composites (C).</summary>
public readonly record struct ChainLink(long Index, bool Prime);

/// <summary>
/// Waleed's CP index chain (Features.txt #9): a number's index among primes or
/// composites, then that index's, down to 1.
/// </summary>
/// <param name="PrimesAsZero">The chain read as binary, P = 0 and C = 1, first link most significant.</param>
/// <param name="PrimesAsZeroReversed">The same bits read from the last link.</param>
/// <param name="PrimesAsOne">P = 1 and C = 0, first link most significant.</param>
/// <param name="PrimesAsOneReversed">The same bits read from the last link.</param>
public sealed record IndexChain(
    IReadOnlyList<ChainLink> Links,
    long PrimesAsZero,
    long PrimesAsZeroReversed,
    long PrimesAsOne,
    long PrimesAsOneReversed,
    long Sum)
{
    public string Text => string.Join("-", Links.Select(l => (l.Prime ? "P" : "C") + l.Index));
}

/// <summary>Where a number sits among the primes or composites of the form 4n+1 or 4n-1 (Features.txt #6, #7).</summary>
/// <param name="Plus">True for 4n+1, false for 4n-1.</param>
/// <param name="Ordinal">1-based position among the primes (or composites) of that form, or null when too far to count.</param>
public sealed record FourNForm(bool Plus, long N, long? Ordinal);

/// <summary>Ways to write a number as a sum or difference of two squares or two cubes.</summary>
public sealed record PowerSplits(
    IReadOnlyList<(long A, long B)> SquareSums,
    IReadOnlyList<(long A, long B)> SquareDifferences,
    IReadOnlyList<(long A, long B)> CubeSums,
    IReadOnlyList<(long A, long B)> CubeDifferences);

/// <summary>
/// The number facts of the original's value panel beyond the class and
/// factors: divisors, powers, Carmichael numbers, 4n±1 forms, square and
/// cube splits, and the CP index chain.
/// </summary>
public static class NumberForms
{
    /// <summary>The original computes square and cube splits only up to a million (Numbers.cs).</summary>
    public const long SplitLimit = 1_000_000;

    /// <summary>The original builds index chains only up to a trillion.</summary>
    public const long ChainLimit = 1_000_000_000_000;

    /// <summary>Counting 4n±1 numbers is done by scanning; beyond this it is left out.</summary>
    public const long FormOrdinalLimit = 10_000_000;

    /// <summary>All divisors in ascending order, from the prime factors.</summary>
    public static IReadOnlyList<long> Divisors(IReadOnlyList<long> factors)
    {
        ArgumentNullException.ThrowIfNull(factors);
        var divisors = new List<long> { 1 };
        foreach (IGrouping<long, long> group in factors.GroupBy(f => f))
        {
            int count = divisors.Count;
            long power = 1;
            for (int e = 0; e < group.Count(); e++)
            {
                power *= group.Key;
                for (int i = 0; i < count; i++) divisors.Add(divisors[i] * power);
            }
        }
        divisors.Sort();
        return divisors;
    }

    /// <summary>The highest power (2 to 10) the number is, or null. 1 counts as no power.</summary>
    public static int? Power(long value)
    {
        long n = Math.Abs(value);
        if (n < 4) return null;
        for (int k = 10; k >= 2; k--)
        {
            if (Search.Numbers.Criteria.IsPower(n, k)) return k;
        }
        return null;
    }

    /// <summary>Korselt's test: composite, square-free, and p − 1 divides n − 1 for every prime factor p.</summary>
    public static bool IsCarmichael(long value, IReadOnlyList<long> factors)
    {
        ArgumentNullException.ThrowIfNull(factors);
        long n = Math.Abs(value);
        if (factors.Count < 2 || factors.Distinct().Count() != factors.Count || n % 2 == 0) return false;
        return factors.All(p => (n - 1) % (p - 1) == 0);
    }

    /// <summary>The 4n±1 form of an odd prime or composite; null for others.</summary>
    public static FourNForm? FourN(long value)
    {
        long n = Math.Abs(value);
        if (n < 3 || n % 2 == 0) return null;
        bool prime = NumberTheory.IsPrime(n);
        if (!prime && !NumberTheory.IsComposite(n)) return null;

        bool plus = n % 4 == 1;
        long k = plus ? (n - 1) / 4 : (n + 1) / 4;
        long? ordinal = null;
        if (n <= FormOrdinalLimit)
        {
            long count = 0;
            for (long m = plus ? 5 : 3; m <= n; m += 4)
            {
                if (prime ? NumberTheory.IsPrime(m) : NumberTheory.IsComposite(m)) count++;
            }
            ordinal = count;
        }
        return new FourNForm(plus, k, ordinal);
    }

    /// <summary>Every a² + b², b² − a², a³ + b³ and b³ − a³ equal to the number (a ≤ b, zero allowed), up to a million.</summary>
    public static PowerSplits? Splits(long value)
    {
        long v = Math.Abs(value);
        if (v < 1 || v > SplitLimit) return null;

        var squareSums = new List<(long, long)>();
        for (long a = 0; 2 * a * a <= v; a++)
        {
            long rest = v - a * a;
            long b = (long)Math.Round(Math.Sqrt(rest));
            if (b * b == rest) squareSums.Add((a, b));
        }

        // b² − a² = v, a from 0 up to v/2 + 1 as in the original.
        var squareDiffs = new List<(long, long)>();
        for (long a = 0; a <= v / 2 + 1; a++)
        {
            long total = v + a * a;
            long b = (long)Math.Round(Math.Sqrt(total));
            if (b * b == total) squareDiffs.Add((a, b));
        }

        var cubeSums = new List<(long, long)>();
        for (long a = 0; 2 * a * a * a <= v; a++)
        {
            long rest = v - a * a * a;
            long b = (long)Math.Round(Math.Cbrt(rest));
            if (b * b * b == rest) cubeSums.Add((a, b));
        }

        // b³ − a³ ≥ 3a² + 3a + 1 when b > a, which bounds a.
        var cubeDiffs = new List<(long, long)>();
        for (long a = 0; 3 * a * a + 3 * a + 1 <= v; a++)
        {
            long total = v + a * a * a;
            long b = (long)Math.Round(Math.Cbrt(total));
            if (b * b * b == total) cubeDiffs.Add((a, b));
        }

        return new PowerSplits(squareSums, squareDiffs, cubeSums, cubeDiffs);
    }

    /// <summary>The index chain, or null when the number is 1 or less, or beyond <see cref="ChainLimit"/>.</summary>
    public static IndexChain? Chain(long value)
    {
        long n = Math.Abs(value);
        if (n <= 1 || n > ChainLimit) return null;

        var links = new List<ChainLink>();
        while (n > 1)
        {
            bool prime = NumberTheory.IsPrime(n);
            long? index = NumberTheory.TryOrdinal(n, prime ? NumberClass.Prime : NumberClass.Composite);
            if (index is null) break; // the chain so far is kept, as in the original
            links.Add(new ChainLink(index.Value, prime));
            n = index.Value;
        }
        if (links.Count == 0) return null;

        long Bits(bool primeBit, bool reversed)
        {
            IEnumerable<ChainLink> order = reversed ? Enumerable.Reverse(links) : links;
            long bits = 0;
            foreach (ChainLink link in order) bits = bits * 2 + (link.Prime == primeBit ? 1 : 0);
            return bits;
        }

        return new IndexChain(
            links,
            PrimesAsZero: Bits(primeBit: false, reversed: false),
            PrimesAsZeroReversed: Bits(primeBit: false, reversed: true),
            PrimesAsOne: Bits(primeBit: true, reversed: false),
            PrimesAsOneReversed: Bits(primeBit: true, reversed: true),
            Sum: links.Sum(l => l.Index));
    }
}
