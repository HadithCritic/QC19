namespace QuranCode.Core.Text;

/// <summary>One find-and-replace rule of a text mode, applied to every occurrence.</summary>
public sealed record TextRule(string Find, string Replace);

/// <summary>
/// A text mode defined on top of a stock one (Features.txt #72): the stock
/// mode's rules, then these, then the stock mode's letter stage.
/// </summary>
/// <remarks>
/// <para>
/// The original let a reader add a <c>Rules/SimplifiedXX.txt</c> file. That is
/// only half of a text mode here: the letter stage that strips marks and folds
/// letter forms is chosen by name, and an unknown name would keep every mark.
/// A derived mode therefore names its base, and inherits the base's letter
/// stage and its counting options. Its own rules run after the base's, so the
/// base's word joins (which match the text as written) still see the text they
/// expect.
/// </para>
/// <para>
/// A text mode changes letter counts, and so changes Code 19 results. That is
/// the reason to keep one: to count a letter the way a source did.
/// </para>
/// </remarks>
/// <param name="Name">Unique name. It may not reuse a stock mode's name.</param>
/// <param name="Base">The stock mode it starts from, such as Simplified29.</param>
/// <param name="Description">What it changes and why, for a reader.</param>
public sealed record DerivedTextMode(string Name, string Base, IReadOnlyList<TextRule> Rules, string Description = "")
{
    /// <summary>The stock modes a derived mode may start from.</summary>
    public static readonly IReadOnlyList<string> StockModes =
        ["Original", "Simplified28", "Simplified29", "Simplified30", "Simplified31", "Simplified36", "SimplifiedDots", "SimplifiedMarks"];

    /// <summary>The first problem with this definition, or null when it is sound.</summary>
    public string? Problem()
    {
        if (string.IsNullOrWhiteSpace(Name)) return "A text mode needs a name.";
        if (Name.Length > 64) return "A text mode's name is at most 64 characters.";
        if (StockModes.Contains(Name, StringComparer.OrdinalIgnoreCase)) return $"\"{Name}\" is a stock text mode.";
        if (!StockModes.Contains(Base, StringComparer.Ordinal)) return $"\"{Base}\" is not a stock text mode.";
        if (Rules.Count == 0) return "A text mode needs at least one rule.";
        if (Rules.Count > 500) return "A text mode has at most 500 rules.";
        foreach (TextRule rule in Rules)
            if (string.IsNullOrEmpty(rule.Find)) return "A rule needs something to find.";
        return null;
    }
}
