namespace QuranCode.Core.Numbers;

/// <summary>
/// Number-theory sequences, computed rather than shipped.
/// </summary>
/// <remarks>
/// <para>
/// The legacy install carries 63 MB of these as newline-delimited decimal ASCII
/// under <c>Numbers/</c> (6.65 million lines across 77 files), parsed
/// line-by-line into <c>List&lt;long&gt;</c> at startup.
/// </para>
/// <para>
/// A sieve to the same limit takes milliseconds. Measured on this machine, the
/// 78,499 primes below 1,000,003 that <c>primes.txt</c> holds regenerate in
/// about 10 ms, against 7.2 MB of file that has to be read and parsed. The
/// tables are not a cache of something expensive; they are a cache of something
/// nearly free.
/// </para>
/// <para>
/// Results are memoized per limit, so a caller that asks repeatedly pays once.
/// </para>
/// </remarks>
public static class NumberTheory
{
    private static readonly Lock Gate = new();
    private static long[]? _primes;
    private static int _primeLimit;

    /// <summary>
    /// Every prime up to and including <paramref name="limit"/>.
    /// </summary>
    /// <remarks>
    /// Plain sieve of Eratosthenes over a <see cref="bool"/> array. A segmented
    /// sieve would use less memory, but at these limits the simple version is
    /// already fast and is far easier to verify against the shipped tables.
    /// </remarks>
    public static ReadOnlySpan<long> Primes(int limit = 1_000_003)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(limit);

        lock (Gate)
        {
            if (_primes is not null && _primeLimit >= limit)
            {
                // Trim to the requested limit without recomputing.
                int count = _primes.Length;
                while (count > 0 && _primes[count - 1] > limit) count--;
                return _primes.AsSpan(0, count);
            }

            _primes = Sieve(limit);
            _primeLimit = limit;
            return _primes;
        }
    }

    private static long[] Sieve(int limit)
    {
        if (limit < 2) return [];

        var composite = new bool[limit + 1];
        for (int i = 2; (long)i * i <= limit; i++)
        {
            if (composite[i]) continue;
            for (long j = (long)i * i; j <= limit; j += i) composite[j] = true;
        }

        int count = 0;
        for (int i = 2; i <= limit; i++) if (!composite[i]) count++;

        var primes = new long[count];
        int index = 0;
        for (int i = 2; i <= limit; i++) if (!composite[i]) primes[index++] = i;
        return primes;
    }

    /// <summary>Whether a number is prime.</summary>
    public static bool IsPrime(long value)
    {
        if (value < 2) return false;
        if (value < 4) return true;
        if (value % 2 == 0) return false;

        for (long i = 3; i * i <= value; i += 2)
        {
            if (value % i == 0) return false;
        }
        return true;
    }

    /// <summary>
    /// Whether a number is an additive prime: prime, and with a prime digit sum.
    /// </summary>
    /// <remarks>
    /// Central to the Primalogy system. Al-Fatiha's 7 verses, 29 words and 139
    /// letters are all additive primes, as is its value 8317 (8+3+1+7 = 19).
    /// </remarks>
    public static bool IsAdditivePrime(long value) => IsPrime(value) && IsPrime(DigitSum(value));

    /// <summary>Every additive prime up to <paramref name="limit"/>.</summary>
    public static List<long> AdditivePrimes(int limit = 1_000_033)
    {
        var result = new List<long>();
        foreach (long prime in Primes(limit))
        {
            if (IsPrime(DigitSum(prime))) result.Add(prime);
        }
        return result;
    }

    /// <summary>Composite numbers up to <paramref name="limit"/>, starting at 4.</summary>
    public static List<long> Composites(int limit = 1_000_000)
    {
        var isPrime = new bool[limit + 1];
        foreach (long prime in Primes(limit)) isPrime[prime] = true;

        var result = new List<long>();
        for (int i = 4; i <= limit; i++)
        {
            if (!isPrime[i]) result.Add(i);
        }
        return result;
    }

    /// <summary>Primes congruent to 1 mod 4.</summary>
    /// <remarks>
    /// These are exactly the odd primes expressible as a sum of two squares,
    /// which is the property the legacy "4n+1" display uses.
    /// </remarks>
    public static List<long> Primes4NPlus1(int limit)
    {
        var result = new List<long>();
        foreach (long prime in Primes(limit))
        {
            if (prime % 4 == 1) result.Add(prime);
        }
        return result;
    }

    /// <summary>Primes congruent to 3 mod 4.</summary>
    public static List<long> Primes4NMinus1(int limit)
    {
        var result = new List<long>();
        foreach (long prime in Primes(limit))
        {
            if (prime % 4 == 3) result.Add(prime);
        }
        return result;
    }

    /// <summary>Sum of the decimal digits.</summary>
    public static long DigitSum(long value)
    {
        value = Math.Abs(value);
        long sum = 0;
        while (value > 0)
        {
            sum += value % 10;
            value /= 10;
        }
        return sum;
    }

    /// <summary>Repeated digit sum until one digit remains.</summary>
    public static long DigitalRoot(long value)
    {
        value = Math.Abs(value);
        return value == 0 ? 0 : 1 + (value - 1) % 9;
    }

    /// <summary>Prime factorization, ascending, with repeats.</summary>
    public static List<long> Factorize(long value)
    {
        var factors = new List<long>();
        if (value < 2) return factors;

        while (value % 2 == 0) { factors.Add(2); value /= 2; }
        for (long i = 3; i * i <= value; i += 2)
        {
            while (value % i == 0) { factors.Add(i); value /= i; }
        }
        if (value > 1) factors.Add(value);
        return factors;
    }

    /// <summary>The nth triangular number.</summary>
    public static long Triangular(long n) => n * (n + 1) / 2;
}
