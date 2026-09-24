using System.Globalization;

namespace QuranCode.Core.Text;

/// <summary>
/// The letters of a display word, and the counted letters each one produces.
/// </summary>
/// <remarks>
/// <para>
/// A display letter is one Arabic letter (Unicode category Lo, tatweel
/// excluded) with the marks that follow it. Everything else is a mark of the
/// letter before it: harakat, shadda, the superscript alif, the small waw and
/// yaa, pause marks. Marks before the first letter, such as ۞, belong to the
/// first letter. These are fixed by the display text, so a letter's number
/// does not depend on what is being counted.
/// </para>
/// <para>
/// What a letter counts as does depend on it. Shadda as a letter makes a
/// letter count twice, the superscript alif can become a letter of its own, a
/// text mode can drop or fold a letter. Rather than encode each rule,
/// <see cref="Map"/> normalizes each prefix of the word with the same
/// pipeline and takes what each letter adds. The answer is trusted only when
/// every prefix normalizes to a prefix of the counted letters; otherwise a
/// rule reacted to where the word was cut, and the letters are not addressable
/// one by one under that mode.
/// </para>
/// </remarks>
public static class DisplayLetters
{
    private const char Tatweel = 'ـ';

    /// <summary>How many letters a display word has.</summary>
    public static int Count(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        int count = 0;
        foreach (char c in word) if (IsLetter(c)) count++;
        return count;
    }

    /// <summary>
    /// The word up to and including letter <paramref name="letters"/> with its
    /// marks; the whole word when that is its last letter.
    /// </summary>
    public static string Prefix(string word, int letters)
    {
        ArgumentNullException.ThrowIfNull(word);
        if (letters <= 0) return "";

        int seen = 0;
        for (int i = 0; i < word.Length; i++)
        {
            if (!IsLetter(word[i])) continue;
            if (seen++ == letters) return word[..i].TrimEnd(' ');
        }
        return word;
    }

    /// <summary>Letter <paramref name="letter"/> (1-based) of a display word with its marks.</summary>
    public static string Letter(string word, int letter)
    {
        ArgumentNullException.ThrowIfNull(word);
        string through = Prefix(word, letter);
        string before = Prefix(word, letter - 1);
        return through[before.Length..].Trim();
    }

    /// <summary>
    /// Counted letters produced up to each display letter of a run of display
    /// words that became <paramref name="counted"/>: element [j][k] is the
    /// total through letter k + 1 of word j. Null when the letters cannot be
    /// matched one by one.
    /// </summary>
    /// <param name="words">Display words that align together: one word, or several joined into one counted word.</param>
    /// <param name="counted">Their counted letters, concatenated, without spaces.</param>
    /// <param name="normalizeWord">The normalization the segmentation used for these words.</param>
    public static int[][]? Map(IReadOnlyList<string> words, string counted, Func<string, string> normalizeWord)
    {
        ArgumentNullException.ThrowIfNull(words);
        ArgumentNullException.ThrowIfNull(counted);
        ArgumentNullException.ThrowIfNull(normalizeWord);

        var ends = new int[words.Count][];
        string before = "";
        int previous = 0;
        for (int j = 0; j < words.Count; j++)
        {
            int letters = Count(words[j]);
            ends[j] = new int[letters];
            for (int k = 1; k <= letters; k++)
            {
                string prefix = Compact(normalizeWord(before + Prefix(words[j], k)));
                if (prefix.Length < previous || !counted.StartsWith(prefix, StringComparison.Ordinal)) return null;
                ends[j][k - 1] = previous = prefix.Length;
            }
            before += words[j] + " ";
        }

        // The last prefix is every word whole, so it must be all of the counted letters.
        return previous == counted.Length ? ends : null;
    }

    private static bool IsLetter(char c) =>
        c != Tatweel && char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter;

    private static string Compact(string text) => text.Replace(" ", "", StringComparison.Ordinal);
}
