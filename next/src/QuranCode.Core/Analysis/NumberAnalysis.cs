using QuranCode.Core.Numbers;

namespace QuranCode.Core.Analysis;

/// <summary>
/// Everything the UI shows about a single number.
/// </summary>
/// <param name="Value">The number itself.</param>
/// <param name="Class">Leaf classification: Unit, AP, XP, AC, XC or None.</param>
/// <param name="FamilyOrdinal">
/// 1-based position among all primes or all composites; null when too large to
/// index, and null for the unit and zero, which belong to no sequence.
/// </param>
/// <param name="ClassOrdinal">1-based position within the leaf class; null as above.</param>
/// <param name="Factors">Prime factors ascending with repeats; null when too large to factor promptly.</param>
public sealed record NumberAnalysis(
    long Value,
    NumberClass Class,
    long DigitSum,
    long DigitalRoot,
    long? FamilyOrdinal,
    long? ClassOrdinal,
    IReadOnlyList<long>? Factors)
{
    /// <summary>
    /// Largest magnitude factored. Trial division to its square root is ten
    /// million steps, a few milliseconds, which keeps analysis interactive.
    /// </summary>
    public const long FactorLimit = 100_000_000_000_000;

    public string Code => Class.Code();

    public static NumberAnalysis Of(long value)
    {
        NumberClass numberClass = NumberTheory.Classify(value);
        bool indexed = numberClass is not (NumberClass.None or NumberClass.Unit);
        ulong magnitude = NumberTheory.Magnitude(value);

        return new NumberAnalysis(
            value,
            numberClass,
            NumberTheory.DigitSum(value),
            NumberTheory.DigitalRoot(value),
            indexed ? NumberTheory.TryOrdinal(value, numberClass.Family()) : null,
            indexed ? NumberTheory.TryOrdinal(value, numberClass) : null,
            magnitude <= FactorLimit ? NumberTheory.Factorize((long)magnitude) : null);
    }
}
