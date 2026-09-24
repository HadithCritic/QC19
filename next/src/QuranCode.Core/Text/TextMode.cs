namespace QuranCode.Core.Text;

/// <summary>
/// A named text normalization, expressed as an ordered list of find/replace rules.
/// </summary>
/// <remarks>
/// This mirrors the legacy <c>SimplificationSystem</c> exactly, deliberately.
/// The rules ship as data in <c>Rules/&lt;word-count-method&gt;/&lt;mode&gt;.txt</c>
/// and the legacy engine applies them with a plain sequential
/// <c>string.Replace</c> loop.
///
/// The order is significant: later rules operate on the output of earlier ones,
/// so this is a pipeline, not a set. Reordering changes results.
///
/// Nothing here is cleverer than the original on purpose. This is the code path
/// every letter count and every value depends on, so it reproduces the legacy
/// behavior character for character rather than improving on it.
/// </remarks>
public sealed class TextMode
{
    /// <summary>Mode name, for example <c>Original</c> or <c>Simplified29</c>.</summary>
    public string Name { get; }

    /// <summary>
    /// Segmentation variant the rules belong to. The legacy engine ships two,
    /// 77878 and 77880, named for the word count each produces.
    /// </summary>
    public int WordCountMethod { get; }

    private readonly (string Find, string Replace)[] _rules;

    public TextMode(string name, int wordCountMethod, IReadOnlyList<(string Find, string Replace)> rules)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(rules);

        Name = name;
        WordCountMethod = wordCountMethod;
        _rules = rules.ToArray();
    }

    /// <summary>Number of rules in the pipeline.</summary>
    public int RuleCount => _rules.Length;

    /// <summary>The rules in order, for a derived mode to build on.</summary>
    public IReadOnlyList<(string Find, string Replace)> Rules => _rules;

    /// <summary>
    /// Applies every rule in order. Equivalent to the legacy
    /// <c>SimplificationSystem.Simplify</c>.
    /// </summary>
    public string Simplify(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // The legacy engine normalizes line endings before simplifying, because
        // several rule sets match on "\n" as a verse boundary.
        text = text.Replace("\r\n", "\n");

        foreach ((string find, string replace) in _rules)
        {
            text = text.Replace(find, replace, StringComparison.Ordinal);
        }
        return text;
    }
}
