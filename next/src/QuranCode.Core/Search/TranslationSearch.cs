using System.Collections.Frozen;

namespace QuranCode.Core.Search;

/// <summary>One translation line a search matched, with where.</summary>
/// <param name="Ranges">Start and length of each match in the text.</param>
public sealed record TranslationMatch(string Key, string Text, IReadOnlyList<(int Start, int Length)> Ranges);

/// <summary>A verse found through one or more of its translations.</summary>
public sealed record TranslationHit(int VerseNumber, IReadOnlyList<TranslationMatch> Matches);

/// <summary>
/// Search in translations (Features.txt #51), and the rule that decides
/// whether typed text is searched in the Arabic or in the translations
/// (#67, #73).
/// </summary>
/// <remarks>
/// Follows the legacy <c>DoFindPhrases(translation, ...)</c>: runs of spaces
/// count as one, case is ignored, and there is no folding of accents. The
/// legacy builds a regular expression from the typed text; here it is matched
/// literally, so a typed "." or "(" means itself.
/// </remarks>
public static class TranslationSearch
{
    /// <summary>
    /// The characters the legacy <c>String.IsArabic</c> accepts: Indian digits,
    /// the 36 Quran letters, stop marks, Quran marks, diacritics and symbols
    /// (Utilities/Constants.cs), and آ, which the legacy list leaves out.
    /// </summary>
    private static readonly FrozenSet<char> ArabicCharacters = (
        // INDIAN_DIGITS
        "\u0660\u0661\u0662\u0663\u0664\u0665\u0666\u0667\u0668\u0669" +
        // ARABIC_LETTERS
        "\u0621\u0627\u0625\u0623\u0671\u0628\u062A\u062B\u062C\u062D\u062E\u062F\u0630\u0631\u0632\u0633\u0634\u0635\u0636\u0637\u0638\u0639\u063A\u0641\u0642\u0643\u0644\u0645\u0646\u0647\u0629\u0648\u0624\u0649\u064A\u0626" +
        // STOPMARKS
        "\u06D9\u06D6\u06DA\u06DB\u06D7\u06DC\u06D8" +
        // QURANMARKS
        "\u06DE\u06E9\u2302" +
        // DIACRITICS
        "\u0652\u064E\u0650\u064F\u0651\u064B\u064D\u064C\u0670\u0654\u06E5\u0653\u06DF\u06E6\u06E7\u06ED\u06E2\u06DC\u06E3\u06E0\u06E8\u06EA\u06EB\u06EC\u0640" +
        // SYMBOLS
        "\u007B\u007D\u005B\u005D\u003C\u003E\u0028\u0029\u002E\u002C\u003B\u0060\u0021\u003A\u003D\u002B\u002D\u002A\u002F\u0025\u005C\u0022\u0027\u007E\u0040\u0023\u0024\u005E\u0026\u005F\u007C\u003F\u2018\u00F7\u00D7\u061B\u0640\u2014\u060C\u0022\u2019\u002E\u061F" +
        "\u0622").ToFrozenSet();

    /// <summary>Whether text is searched in the Arabic: every character is Arabic, a mark, a digit, a symbol or a space.</summary>
    public static bool IsArabic(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0) return false;
        foreach (char c in text)
        {
            if (c is ' ' or '\r' or '\n' or '\t' || ArabicCharacters.Contains(c)) continue;
            return false;
        }
        return true;
    }

    /// <param name="translations">Each searched translation's key and its verse texts.</param>
    public static IReadOnlyList<TranslationHit> Find(
        string term, Wordness wordness, IReadOnlyList<(string Key, IReadOnlyDictionary<int, string> Text)> translations,
        IReadOnlySet<int>? scope = null)
    {
        ArgumentNullException.ThrowIfNull(term);
        ArgumentNullException.ThrowIfNull(translations);
        string needle = string.Join(' ', term.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (needle.Length == 0) return [];

        var byVerse = new SortedDictionary<int, List<TranslationMatch>>();
        foreach ((string key, IReadOnlyDictionary<int, string> texts) in translations)
        {
            foreach ((int verse, string raw) in texts)
            {
                if (scope is not null && !scope.Contains(verse)) continue;
                string text = string.Join(' ', raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                List<(int, int)> ranges = Matches(text, needle, wordness);
                if (ranges.Count == 0) continue;
                if (!byVerse.TryGetValue(verse, out List<TranslationMatch>? list)) byVerse[verse] = list = [];
                list.Add(new TranslationMatch(key, text, ranges));
            }
        }
        return byVerse.Select(v => new TranslationHit(v.Key, v.Value)).ToArray();
    }

    /// <summary>Where the needle occurs, ignoring case; whole word means no letter or digit on either side.</summary>
    public static List<(int Start, int Length)> Matches(string text, string needle, Wordness wordness)
    {
        var ranges = new List<(int, int)>();
        for (int at = text.IndexOf(needle, StringComparison.OrdinalIgnoreCase); at >= 0;
             at = text.IndexOf(needle, at + 1, StringComparison.OrdinalIgnoreCase))
        {
            int end = at + needle.Length;
            bool whole = (at == 0 || !char.IsLetterOrDigit(text[at - 1])) && (end == text.Length || !char.IsLetterOrDigit(text[end]));
            if (wordness == Wordness.WholeWord && !whole) continue;
            if (wordness == Wordness.PartOfWord && whole) continue;
            ranges.Add((at, needle.Length));
        }
        return ranges;
    }
}
