using QuranCode.Core.Numbers;

namespace QuranCode.Core.Search.Numbers;

/// <summary>How a measured number is compared (legacy <c>ComparisonOperator</c>).</summary>
public enum Comparison
{
    Equal,
    NotEqual,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual,

    /// <summary>Leaves the given remainder when divided by the value; remainder -1 means any non-zero remainder.</summary>
    DivisibleBy,

    NotDivisibleBy,

    /// <summary>Σ: compares a sum of positions instead of a count, for equality.</summary>
    EqualSum,
}

/// <summary>A kind of number a measurement may be required to be (legacy <c>NumberType</c>).</summary>
public enum NumberType
{
    /// <summary>No kind: the value and comparison apply.</summary>
    None,

    /// <summary>#: compare against the item's own number (a verse with as many words as its number).</summary>
    Natural,

    Prime,
    AdditivePrime,
    NonAdditivePrime,
    Composite,
    AdditiveComposite,
    NonAdditiveComposite,
    Odd,
    Even,
    Fibonacci,
    Square,
    Cubic,
    Quartic,
    Quintic,
    Sextic,
    Septic,
    Octic,
    Nonic,
    Decic,
}

/// <summary>
/// One constraint of a number search: a value with a comparison, or a kind
/// of number. Ignored when the value is 0 and there is no kind, as in the
/// original.
/// </summary>
/// <param name="Remainder">For <see cref="Comparison.DivisibleBy"/>: the remainder wanted, or -1 for any non-zero one.</param>
public sealed record Criterion(long Value = 0, Comparison Comparison = Comparison.Equal, NumberType Type = NumberType.None, int Remainder = 0)
{
    public static readonly Criterion Any = new();

    public bool IsSet => Value != 0 || Type != NumberType.None;

    /// <summary>Whether the constraint asks for a sum of positions (Σ) rather than a count.</summary>
    public bool WantsSum => Comparison == Comparison.EqualSum && Type == NumberType.None;

    /// <summary>Tests a measurement; <paramref name="itemNumber"/> serves the Natural kind.</summary>
    public bool Accepts(long measured, long itemNumber)
    {
        return Type switch
        {
            NumberType.None => Value == 0 || Compare(measured, Value, Comparison, Remainder),
            NumberType.Natural => Compare(measured, itemNumber, Comparison, Remainder),
            _ => Criteria.IsOfType(measured, Type),
        };
    }

    /// <summary>The legacy <c>Numbers.Compare</c>.</summary>
    public static bool Compare(long a, long b, Comparison comparison, int remainder) => comparison switch
    {
        Comparison.Equal or Comparison.EqualSum => a == b,
        Comparison.NotEqual => a != b,
        Comparison.Less => a < b,
        Comparison.LessOrEqual => a <= b,
        Comparison.Greater => a > b,
        Comparison.GreaterOrEqual => a >= b,
        Comparison.DivisibleBy => b != 0 && (remainder == -1 ? a % b != 0 : a != 0 && Math.Abs(a % b) == remainder),
        Comparison.NotDivisibleBy => b != 0 && a != 0 && a % b != 0,
        _ => false,
    };
}

/// <summary>Number kinds (legacy <c>Numbers.IsNumberType</c>).</summary>
public static class Criteria
{
    private static readonly HashSet<long> Fibonacci = BuildFibonacci();

    public static bool IsOfType(long number, NumberType type)
    {
        long n = Math.Abs(number);
        if (n == 0) return false;
        return type switch
        {
            NumberType.Natural => true,
            NumberType.Prime => NumberTheory.IsPrime(n),
            NumberType.AdditivePrime => NumberTheory.IsMember(n, NumberClass.AdditivePrime),
            NumberType.NonAdditivePrime => NumberTheory.IsMember(n, NumberClass.NonAdditivePrime),
            NumberType.Composite => NumberTheory.IsComposite(n),
            NumberType.AdditiveComposite => NumberTheory.IsMember(n, NumberClass.AdditiveComposite),
            NumberType.NonAdditiveComposite => NumberTheory.IsMember(n, NumberClass.NonAdditiveComposite),
            NumberType.Odd => n % 2 == 1,
            NumberType.Even => n % 2 == 0,
            NumberType.Fibonacci => Fibonacci.Contains(n),
            >= NumberType.Square and <= NumberType.Decic => IsPower(n, type - NumberType.Square + 2),
            _ => false,
        };
    }

    /// <summary>Whether n is some whole number raised to the given power.</summary>
    public static bool IsPower(long n, int power)
    {
        long root = (long)Math.Round(Math.Pow(n, 1.0 / power));
        for (long r = Math.Max(1, root - 1); r <= root + 1; r++)
        {
            long value = 1;
            bool overflow = false;
            for (int i = 0; i < power && !overflow; i++)
            {
                if (value > long.MaxValue / r) overflow = true;
                else value *= r;
            }
            if (!overflow && value == n) return true;
        }
        return false;
    }

    private static HashSet<long> BuildFibonacci()
    {
        var set = new HashSet<long> { 1 };
        long a = 1, b = 2;
        while (b > 0 && b < long.MaxValue / 2)
        {
            set.Add(b);
            (a, b) = (b, a + b);
        }
        return set;
    }
}
