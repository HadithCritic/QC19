namespace QuranCode.Core.Numerology;

/// <summary>
/// How letter values are combined into a total.
/// </summary>
/// <remarks>
/// Mirrors the legacy <c>CalculationMode</c> enum in <c>Model/Enums.cs</c>.
/// Names are kept identical so golden data and specifications line up.
/// </remarks>
public enum CalculationMode
{
    SumOfLetterValues,
    SumOfLetterValueDigitSums,
    SumOfLetterValueDigitalRoots,
    SumOfWordValueDigitSums,
    SumOfWordValueDigitalRoots,
}

/// <summary>
/// The settings that decide what a value means, gathered into one object.
/// </summary>
/// <remarks>
/// In the legacy engine these were static mutable fields on <c>Server</c>
/// (<c>CalculationMode</c>, <c>AlternateLetterValues</c> and siblings) combined
/// with 21 flags on <c>NumericalSystem</c>. Any code could change them, and a
/// result carried no record of what produced it.
///
/// <para>
/// Making the profile an explicit immutable value is what lets a research result
/// cite the settings that produced it, which the brief requires for
/// reproducibility (§20, §38). It also means valuation is a pure function of
/// (text, value system, profile), so it is safe to run in parallel.
/// </para>
///
/// <para>
/// <b>Scope.</b> Only the sign-alternation and calculation-mode settings are
/// implemented so far. The 21 position and distance modifiers
/// (<c>AddToLetterLNumber</c> and siblings) require per-letter positional
/// metadata and are not yet wired up; <see cref="RequiresPositionalMetadata"/>
/// reports whether a profile needs them. Constructing such a profile is allowed,
/// but valuation will reject it rather than silently return a wrong number.
/// </para>
/// </remarks>
public sealed record CalculationProfile
{
    /// <summary>The legacy defaults: a plain sum of letter values, no alternation.</summary>
    public static readonly CalculationProfile Default = new();

    public CalculationMode Mode { get; init; } = CalculationMode.SumOfLetterValues;

    /// <summary>Flip the sign at each letter boundary.</summary>
    public bool AlternateLetterValues { get; init; }

    /// <summary>Flip the sign at each word boundary.</summary>
    public bool AlternateWordValues { get; init; }

    /// <summary>Flip the sign at each verse boundary.</summary>
    public bool AlternateVerseValues { get; init; }

    /// <summary>Flip the sign at each chapter boundary.</summary>
    public bool AlternateChapterValues { get; init; }

    /// <summary>Add letter/word/verse/chapter positions to the total.</summary>
    public bool AddPositions { get; init; }

    /// <summary>Add distances to the previous identical element to the total.</summary>
    public bool AddDistancesToPrevious { get; init; }

    /// <summary>Add distances to the next identical element to the total.</summary>
    public bool AddDistancesToNext { get; init; }

    /// <summary>
    /// Whether this profile needs per-element positional metadata that the
    /// current engine does not yet materialize.
    /// </summary>
    public bool RequiresPositionalMetadata =>
        AddPositions || AddDistancesToPrevious || AddDistancesToNext;
}
