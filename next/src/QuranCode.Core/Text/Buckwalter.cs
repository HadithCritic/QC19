using System.Collections.Frozen;
using System.Text;

namespace QuranCode.Core.Text;

/// <summary>
/// The Quranic Arabic Corpus's extended Buckwalter transliteration, back to
/// Arabic script, for showing the corpus's word parts as the original does.
/// </summary>
public static class Buckwalter
{
    private static readonly FrozenDictionary<char, char> Map = new Dictionary<char, char>
    {
        ['\''] = 'ء', ['|'] = 'آ', ['>'] = 'أ', ['&'] = 'ؤ', ['<'] = 'إ', ['}'] = 'ئ',
        ['A'] = 'ا', ['b'] = 'ب', ['p'] = 'ة', ['t'] = 'ت', ['v'] = 'ث', ['j'] = 'ج',
        ['H'] = 'ح', ['x'] = 'خ', ['d'] = 'د', ['*'] = 'ذ', ['r'] = 'ر', ['z'] = 'ز',
        ['s'] = 'س', ['$'] = 'ش', ['S'] = 'ص', ['D'] = 'ض', ['T'] = 'ط', ['Z'] = 'ظ',
        ['E'] = 'ع', ['g'] = 'غ', ['_'] = 'ـ', ['f'] = 'ف', ['q'] = 'ق', ['k'] = 'ك',
        ['l'] = 'ل', ['m'] = 'م', ['n'] = 'ن', ['h'] = 'ه', ['w'] = 'و', ['Y'] = 'ى',
        ['y'] = 'ي', ['F'] = 'ً', ['N'] = 'ٌ', ['K'] = 'ٍ', ['a'] = 'َ', ['u'] = 'ُ',
        ['i'] = 'ِ', ['~'] = 'ّ', ['o'] = 'ْ', ['^'] = 'ٓ', ['#'] = 'ٔ', ['`'] = 'ٰ',
        ['{'] = 'ٱ', [':'] = 'ۜ', ['@'] = '۟', ['"'] = '۠', ['['] = 'ۢ', [';'] = 'ۣ',
        [','] = 'ۥ', ['.'] = 'ۦ', ['!'] = 'ۨ', ['-'] = '۪', ['+'] = '۫', ['%'] = '۬',
        [']'] = 'ۭ',
    }.ToFrozenDictionary();

    /// <summary>Arabic script for a Buckwalter string; characters outside the scheme are kept.</summary>
    public static string ToArabic(string buckwalter)
    {
        ArgumentNullException.ThrowIfNull(buckwalter);
        var text = new StringBuilder(buckwalter.Length);
        foreach (char c in buckwalter) text.Append(Map.GetValueOrDefault(c, c));
        return text.ToString();
    }
}
