using System.Numerics;

namespace QuranCode.Core.Numbers;

/// <summary>
/// An odd-only, bit-packed sieve of Eratosthenes.
/// </summary>
/// <remarks>
/// One bit per odd number, so a sieve to 100 million costs 6.25 MB. That is
/// what makes it affordable to classify and index values as large as the whole
/// book's (about 19.6 million under the default system), which the legacy
/// engine could only do from its shipped tables.
/// </remarks>
internal sealed class PrimeSieve
{
    // Bit i set means the odd number 2i+1 is composite.
    private readonly ulong[] _composite;

    /// <summary>Largest value this sieve answers for.</summary>
    public long Limit { get; }

    public PrimeSieve(long limit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(limit);
        Limit = limit;

        long oddCount = (limit + 1) / 2;
        _composite = new ulong[(oddCount + 63) / 64];
        MarkBit(0); // 1 is not prime

        for (long p = 3; p * p <= limit; p += 2)
        {
            if (IsMarked(p / 2)) continue;
            for (long multiple = p * p; multiple <= limit; multiple += 2 * p)
            {
                MarkBit(multiple / 2);
            }
        }
    }

    /// <summary>Whether a non-negative value up to <see cref="Limit"/> is prime.</summary>
    public bool IsPrime(long value)
    {
        if (value < 2) return false;
        if (value == 2) return true;
        if ((value & 1) == 0) return false;
        return !IsMarked(value / 2);
    }

    /// <summary>Number of primes up to and including <paramref name="value"/>.</summary>
    public long CountPrimes(long value)
    {
        if (value < 2) return 0;

        long lastOdd = (value & 1) == 1 ? value : value - 1;
        long bits = lastOdd / 2 + 1; // odd numbers 1..lastOdd
        long fullWords = bits / 64;

        long composites = 0;
        for (long w = 0; w < fullWords; w++) composites += BitOperations.PopCount(_composite[w]);

        int remainder = (int)(bits % 64);
        if (remainder != 0)
        {
            ulong mask = (1UL << remainder) - 1;
            composites += BitOperations.PopCount(_composite[fullWords] & mask);
        }

        // Odd primes plus the prime 2.
        return bits - composites + 1;
    }

    private bool IsMarked(long bit) => (_composite[bit >> 6] & (1UL << (int)(bit & 63))) != 0;

    private void MarkBit(long bit) => _composite[bit >> 6] |= 1UL << (int)(bit & 63);
}
