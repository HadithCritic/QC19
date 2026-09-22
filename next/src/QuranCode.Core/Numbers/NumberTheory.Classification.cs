namespace QuranCode.Core.Numbers;

public static partial class NumberTheory
{
    /// <summary>
    /// Largest magnitude <see cref="Ordinal"/> and <see cref="CountUpTo"/>
    /// answer for.
    /// </summary>
    /// <remarks>
    /// Counting needs a sieve to the value itself. 100 million costs 6.25 MB and
    /// well under a second, and covers every chapter and book value under the
    /// shipped systems. Larger values still classify, by trial division; they
    /// just have no ordinal.
    /// </remarks>
    public const long OrdinalLimit = 100_000_000;

    private static readonly Lock SieveGate = new();
    private static PrimeSieve? _sieve;

    /// <summary>
    /// Classifies a value the way the legacy <c>GetNumberType</c> does.
    /// </summary>
    /// <remarks>
    /// Negative values classify by magnitude, because sign alternation can make
    /// a total negative and the legacy colors those by their magnitude too.
    /// </remarks>
    public static NumberClass Classify(long value)
    {
        ulong magnitude = Magnitude(value);
        if (magnitude == 0) return NumberClass.None;
        if (magnitude == 1) return NumberClass.Unit;

        long digitSum = DigitSum(value);
        if (IsPrimeMagnitude(magnitude))
        {
            return IsPrime(digitSum) ? NumberClass.AdditivePrime : NumberClass.NonAdditivePrime;
        }
        return IsComposite(digitSum) ? NumberClass.AdditiveComposite : NumberClass.NonAdditiveComposite;
    }

    /// <summary>Whether a value is composite: greater than one in magnitude and not prime.</summary>
    public static bool IsComposite(long value)
    {
        ulong magnitude = Magnitude(value);
        return magnitude > 1 && !IsPrimeMagnitude(magnitude);
    }

    /// <summary>Whether a value belongs to a class or to a family.</summary>
    public static bool IsMember(long value, NumberClass numberClass)
    {
        NumberClass actual = Classify(value);
        return actual == numberClass || actual.Family() == numberClass;
    }

    /// <summary>
    /// How many positive integers up to <paramref name="limit"/> belong to a class.
    /// </summary>
    /// <remarks>
    /// This is the figure the Readme quotes as, for example, "there are 16
    /// additive primes up to 114".
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The limit exceeds <see cref="OrdinalLimit"/>, or the class is <see cref="NumberClass.None"/>.
    /// </exception>
    public static long CountUpTo(NumberClass numberClass, long limit)
    {
        if (numberClass == NumberClass.None)
        {
            throw new ArgumentOutOfRangeException(nameof(numberClass), "zero has no class to count");
        }
        ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, OrdinalLimit);
        if (limit < 1) return 0;

        PrimeSieve sieve = SieveTo(limit);
        return numberClass switch
        {
            NumberClass.Unit => 1,
            NumberClass.Prime => sieve.CountPrimes(limit),
            NumberClass.Composite => limit < 4 ? 0 : limit - 1 - sieve.CountPrimes(limit),
            _ => CountSubclass(sieve, numberClass, limit),
        };
    }

    /// <summary>
    /// A value's 1-based position within a class, or zero if it is not a member.
    /// </summary>
    /// <remarks>
    /// The legacy stores 0-based indices and every screen adds one before
    /// displaying (<c>Numbers/MainForm.cs</c>: <c>PrimeIndexOf(number) + 1</c>).
    /// The 1-based form is what people read, so it is what this returns.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The magnitude exceeds <see cref="OrdinalLimit"/>.</exception>
    public static long Ordinal(long value, NumberClass numberClass) =>
        TryOrdinal(value, numberClass)
        ?? throw new ArgumentOutOfRangeException(nameof(value), $"ordinals are only indexed up to {OrdinalLimit:N0}");

    /// <summary>
    /// As <see cref="Ordinal"/>, but null for values too large to index
    /// instead of throwing.
    /// </summary>
    public static long? TryOrdinal(long value, NumberClass numberClass)
    {
        ulong magnitude = Magnitude(value);
        if (magnitude > OrdinalLimit) return null;
        if (numberClass == NumberClass.None || !IsMember(value, numberClass)) return 0;
        return CountUpTo(numberClass, (long)magnitude);
    }

    private static long CountSubclass(PrimeSieve sieve, NumberClass numberClass, long limit)
    {
        bool wantPrime = numberClass.Family() == NumberClass.Prime;
        long count = 0;

        for (long n = wantPrime ? 2 : 4; n <= limit; n++)
        {
            if (sieve.IsPrime(n) != wantPrime) continue;

            long digitSum = DigitSum(n);
            bool additive = wantPrime ? IsPrime(digitSum) : IsComposite(digitSum);
            NumberClass actual = wantPrime
                ? (additive ? NumberClass.AdditivePrime : NumberClass.NonAdditivePrime)
                : (additive ? NumberClass.AdditiveComposite : NumberClass.NonAdditiveComposite);

            if (actual == numberClass) count++;
        }
        return count;
    }

    private static PrimeSieve SieveTo(long limit)
    {
        lock (SieveGate)
        {
            if (_sieve is not null && _sieve.Limit >= limit) return _sieve;

            // Grow geometrically so a sequence of rising requests does not
            // rebuild the sieve every time.
            long target = Math.Min(OrdinalLimit, Math.Max(limit, Math.Max(1_000_000, (_sieve?.Limit ?? 0) * 2)));
            _sieve = new PrimeSieve(target);
            return _sieve;
        }
    }

    /// <summary>
    /// Magnitude without overflow: <c>Math.Abs(long.MinValue)</c> throws.
    /// </summary>
    internal static ulong Magnitude(long value) =>
        value >= 0 ? (ulong)value : (ulong)(-(value + 1)) + 1;

    private static bool IsPrimeMagnitude(ulong magnitude)
    {
        if (magnitude < 2) return false;
        if (magnitude < 4) return true;
        if (magnitude % 2 == 0) return false;

        for (ulong i = 3; i <= magnitude / i; i += 2)
        {
            if (magnitude % i == 0) return false;
        }
        return true;
    }
}
