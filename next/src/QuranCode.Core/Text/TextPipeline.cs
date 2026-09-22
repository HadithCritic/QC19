namespace QuranCode.Core.Text;

/// <summary>
/// The complete normalization a text goes through before it is valued.
/// </summary>
/// <remarks>
/// <para>
/// The legacy engine normalizes in two places that never reference each other,
/// and both stages affect the result:
/// </para>
/// <list type="number">
///   <item><description>
///   <b>Rule stage.</b> <c>Rules/&lt;method&gt;/&lt;mode&gt;.txt</c>, applied by
///   <c>SimplificationSystem.Simplify</c> while building the book. Ordered
///   find/replace, largely word segmentation and orthographic fixes such as
///   joining "بعد ما" into one word.
///   </description></item>
///   <item><description>
///   <b>Letter stage.</b> the hardcoded <c>Simplify28..36</c> methods in
///   <c>Utilities/Extensions.cs</c>, applied by <c>Server.CalculateValue</c>.
///   Strips diacritics and folds letter forms.
///   </description></item>
/// </list>
/// <para>
/// Because the stages live far apart, it is easy to implement one and believe
/// the job is done. Doing so gives Al-Fatiha 8283 instead of 8317, and leaves
/// 273 of 6,236 verses valued incorrectly by small amounts. Both figures came
/// from golden comparison, not from reading the code.
/// </para>
/// <para>
/// This type is the answer to brief §24: one named, ordered, testable pipeline
/// instead of normalization scattered across unrelated functions. The order is
/// not negotiable, and the rule stage must run first.
/// </para>
/// </remarks>
public sealed class TextPipeline
{
    private readonly TextMode _rules;

    /// <summary>Text mode name, for example <c>Original</c>.</summary>
    public string TextModeName { get; }

    public TextPipeline(TextMode rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules;
        TextModeName = rules.Name;
    }

    /// <summary>
    /// Applies the rule stage, then the letter stage.
    /// </summary>
    /// <remarks>
    /// The result is what the legacy engine actually values, verified against
    /// all 6,236 verses.
    /// </remarks>
    public string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        string segmented = _rules.Simplify(text);
        return ArabicNormalizer.Simplify(segmented, TextModeName);
    }

    /// <summary>
    /// Applies only the rule stage, giving the text as the book stores it.
    /// </summary>
    /// <remarks>
    /// This is what the legacy <c>Verse.Text</c> holds, and what word counts
    /// and display are based on. It keeps diacritics; valuation does not use it
    /// directly.
    /// </remarks>
    public string Segment(string text) =>
        string.IsNullOrEmpty(text) ? "" : _rules.Simplify(text);
}
