namespace QuranCode.Core.Numbers;

/// <summary>
/// The legacy number classification, as defined in the Readme and in
/// <c>Utilities/Numbers.cs</c> (<c>NumberType</c>).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="NumberTheory.Classify"/> only ever returns a leaf:
/// <see cref="None"/>, <see cref="Unit"/> or one of the four additive
/// subclasses. <see cref="Prime"/> and <see cref="Composite"/> are families,
/// used when counting or indexing across both of their subclasses.
/// </para>
/// <para>
/// "Additive" means the digit sum belongs to the same family. A composite with
/// digit sum 1 (such as 10 or 100) is therefore non-additive, because 1 is the
/// unit and not a composite.
/// </para>
/// </remarks>
public enum NumberClass
{
    /// <summary>Zero. The legacy calls this "not a number".</summary>
    None,

    /// <summary>One: indivisible.</summary>
    Unit,

    /// <summary>Any prime (family).</summary>
    Prime,

    /// <summary>Prime with a prime digit sum. Legacy code AP.</summary>
    AdditivePrime,

    /// <summary>Prime with a non-prime digit sum. Legacy code XP.</summary>
    NonAdditivePrime,

    /// <summary>Any composite (family).</summary>
    Composite,

    /// <summary>Composite with a composite digit sum. Legacy code AC.</summary>
    AdditiveComposite,

    /// <summary>Composite with a non-composite digit sum. Legacy code XC.</summary>
    NonAdditiveComposite,
}

public static class NumberClassExtensions
{
    /// <summary>The family a class belongs to: Prime, Composite, or itself.</summary>
    public static NumberClass Family(this NumberClass numberClass) => numberClass switch
    {
        NumberClass.AdditivePrime or NumberClass.NonAdditivePrime => NumberClass.Prime,
        NumberClass.AdditiveComposite or NumberClass.NonAdditiveComposite => NumberClass.Composite,
        _ => numberClass,
    };

    /// <summary>The short code the legacy UI prints: U, AP, XP, AC, XC.</summary>
    public static string Code(this NumberClass numberClass) => numberClass switch
    {
        NumberClass.Unit => "U",
        NumberClass.Prime => "P",
        NumberClass.AdditivePrime => "AP",
        NumberClass.NonAdditivePrime => "XP",
        NumberClass.Composite => "C",
        NumberClass.AdditiveComposite => "AC",
        NumberClass.NonAdditiveComposite => "XC",
        _ => "",
    };
}
