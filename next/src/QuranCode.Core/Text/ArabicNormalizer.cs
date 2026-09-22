using System.Collections.Frozen;
using System.Text;

namespace QuranCode.Core.Text;

/// <summary>
/// Letter-level normalization, ported from the legacy <c>Simplify*</c>
/// extension methods in <c>Utilities/Extensions.cs</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>The legacy engine normalizes in two unrelated places.</b> This was found
/// by golden testing, not by reading the code, and it matters:
/// </para>
/// <list type="number">
///   <item>
///     <description>
///     <c>Rules/&lt;method&gt;/&lt;mode&gt;.txt</c> drives
///     <c>SimplificationSystem.Simplify</c>, used when <b>building the book</b>.
///     Those rules are mostly about word segmentation, for example joining
///     "بعد ما" into one word.
///     </description>
///   </item>
///   <item>
///     <description>
///     Hardcoded C# in <c>string.Simplify(text_mode)</c>, used when
///     <b>calculating values</b>. This is what strips diacritics and folds
///     letter forms, and it never consults the rule files.
///     </description>
///   </item>
/// </list>
/// <para>
/// This type reproduces the second mechanism. Treating the rule files as the
/// whole story produced 8283 for Al-Fatiha instead of 8317.
/// </para>
/// <para>
/// It also explains a result in the golden data that looked like a coincidence:
/// <c>Original</c> and <c>Simplified29</c> give the identical book value
/// 19,628,315 because <c>Simplify("Original")</c> literally calls
/// <c>Simplify29()</c>. "Original" is a display mode, not a valuation mode.
/// </para>
/// <para>
/// The brief (§24) asks that normalization stop being scattered across
/// unrelated functions. That is exactly the defect here, and the fix is to name
/// it and centralize it rather than to reproduce the scattering.
/// </para>
/// </remarks>
public static class ArabicNormalizer
{
    // Extracted verbatim from Utilities/Constants.cs.
    private static readonly FrozenSet<char> Removed = new[]
    {
        // ARABIC_DIGITS
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        // INDIAN_DIGITS
        '٠', '١', '٢', '٣', '٤',
        '٥', '٦', '٧', '٨', '٩',
        // DIACRITICS
        'ْ', 'َ', 'ِ', 'ُ', 'ّ', 'ً', 'ٍ',
        'ٌ', 'ٰ', 'ٔ', 'ۥ', 'ٓ', '۟', 'ۦ',
        'ۧ', 'ۭ', 'ۢ', 'ۜ', 'ۣ', '۠', 'ۨ',
        '۪', '۫', '۬', 'ـ',
        // STOPMARKS
        'ۙ', 'ۖ', 'ۚ', 'ۛ', 'ۗ', 'ۜ', 'ۘ',
        // QURANMARKS
        '۞', '۩', '⌂',
        // Ornate parentheses
        '﴿', '﴾',
    }.ToFrozenSet();

    /// <summary>
    /// Strips digits, diacritics, stopmarks, Quran marks and ornate
    /// parentheses, then collapses runs of spaces.
    /// </summary>
    public static string Simplify36(string source)
    {
        if (string.IsNullOrEmpty(source)) return source;

        var builder = new StringBuilder(source.Length);
        bool lastWasSpace = false;

        foreach (char character in source)
        {
            if (Removed.Contains(character)) continue;

            // The legacy code appends everything, then loops replacing "  "
            // with " " until none remain. Collapsing inline is equivalent and
            // avoids repeated full-string passes.
            if (character == ' ')
            {
                if (lastWasSpace) continue;
                lastWasSpace = true;
            }
            else
            {
                lastWasSpace = false;
            }
            builder.Append(character);
        }
        return builder.ToString();
    }

    /// <summary>Simplify36 plus folding of hamza-carrying forms.</summary>
    public static string Simplify31(string source)
    {
        if (string.IsNullOrEmpty(source)) return source;

        string result = Simplify36(source);
        var builder = new StringBuilder(result.Length);

        foreach (char character in result)
        {
            builder.Append(character switch
            {
                'إ' or 'أ' or 'ٱ' or 'آ' => 'ا',
                'ؤ' => 'و',
                'ئ' => 'ي',
                _ => character,
            });
        }
        return builder.ToString();
    }

    /// <summary>Simplify31 with hamza removed.</summary>
    public static string Simplify30(string source) =>
        string.IsNullOrEmpty(source) ? source : Simplify31(source).Replace("ء", "");

    /// <summary>Simplify31 with taa marbuta folded to haa and alef maqsura to yaa.</summary>
    public static string Simplify29(string source)
    {
        if (string.IsNullOrEmpty(source)) return source;

        string result = Simplify31(source);
        result = result.Replace("ة", "ه");
        return result.Replace("ى", "ي");
    }

    /// <summary>Simplify29 with hamza removed.</summary>
    public static string Simplify28(string source) =>
        string.IsNullOrEmpty(source) ? source : Simplify29(source).Replace("ء", "");

    /// <summary>
    /// Normalization for a named text mode, matching
    /// <c>string.Simplify(text_mode)</c>.
    /// </summary>
    /// <remarks>
    /// Note that <c>Original</c> and <c>SimplifiedMarks</c> both map to
    /// <see cref="Simplify29"/>, and <c>SimplifiedDots</c> maps to
    /// <see cref="Simplify36"/>. These are the legacy mappings, preserved
    /// deliberately; the legacy source carries a "BUT final ي --> ى" comment on
    /// the SimplifiedDots branch that its code does not implement, and that
    /// discrepancy is preserved too rather than silently corrected.
    /// </remarks>
    public static string Simplify(string source, string textMode)
    {
        if (string.IsNullOrEmpty(source)) return "";

        return textMode switch
        {
            "Simplified28" => Simplify28(source),
            "Simplified29" => Simplify29(source),
            "Simplified30" => Simplify30(source),
            "Simplified31" => Simplify31(source),
            "Simplified36" => Simplify36(source),
            "SimplifiedDots" => Simplify36(source),
            "Original" or "SimplifiedMarks" => Simplify29(source),
            _ => source,
        };
    }
}
