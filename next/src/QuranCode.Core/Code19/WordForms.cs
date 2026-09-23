namespace QuranCode.Core.Code19;

/// <summary>
/// Named sets of whole words a finding can count. Matching is always on the
/// whole normalized word, never a substring: a substring rule counts the name
/// of God 2,726 times instead of 2,698, catching ظلالها (their shadows),
/// خلاله (through it), اللهب (the flame), اللهو (the amusement), يضلله
/// (misleads him) and eleven other word types that are not the name.
/// </summary>
public static class WordForms
{
    /// <summary>
    /// The eleven written forms of the name of God, derived in ADR 0004 §7
    /// from a published index of all 2,698 occurrences. اللهم is not among
    /// them: it is a vocative, and including it breaks both of Appendix 1's
    /// totals. The legacy <c>AllahWords</c> list has twelve because it serves
    /// the original's research tables, not this count.
    /// </summary>
    public const string Allah = "allah";

    private static readonly Dictionary<string, HashSet<string>> Sets = new(StringComparer.Ordinal)
    {
        [Allah] = new(StringComparer.Ordinal)
        {
            "الله", "لله", "بالله", "والله", "ولله", "فالله", "فلله", "تالله", "وتالله", "ءالله", "ابالله",
        },
    };

    /// <summary>The named set, or null when no set has that name.</summary>
    public static IReadOnlySet<string>? Named(string name) =>
        Sets.TryGetValue(name, out HashSet<string>? set) ? set : null;

    /// <summary>Every set name.</summary>
    public static IReadOnlyCollection<string> Names => Sets.Keys;
}
