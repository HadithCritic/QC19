using System;
using System.Text;
using System.Collections.Generic;

public static class CharExtensions
{
    public static bool IsDiacritic(this char source)
    {
        return source >= '\u064B' && source <= '\u065F';
    }
}

public static class StringExtensions
{
    public static string Reverse(this string source)
    {
        char[] array = source.ToCharArray();
        Array.Reverse(array);
        return new string(array);
    }
    public static string RemoveDuplicates(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        string result = "";
        foreach (char c in source)
        {
            if (!result.Contains(c.ToString()))
            {
                result += c;
            }
        }
        return result;
    }
    public static string RemovePunctuations(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        string result = source;
        foreach (char character in Constants.SYMBOLS)
        {
            result = result.Replace(character.ToString(), "");
        }
        return result;
    }
    public static string ToNth(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        string nth = "th";
        long number = 0L;
        if (long.TryParse(source, out number))
        {
            if (source.EndsWith("11")) nth = "th";
            else if (source.EndsWith("12")) nth = "th";
            else if (source.EndsWith("13")) nth = "th";
            else if (source.EndsWith("1")) nth = "st";
            else if (source.EndsWith("2")) nth = "nd";
            else if (source.EndsWith("3")) nth = "rd";
            else nth = "th";
        }

        return source + nth;
    }
    public static string UnitsTensHundredsEtc(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        string result = "";

        long number = 0L;
        if (long.TryParse(source, out number))
        {
            long i = 10;
            while (number >= i / 10)
            {
                result += (number % i - number % (i / 10)).ToString() + ",";
                i *= 10;
            }
            if (result.Length > 0)
            {
                result = result.Remove(result.Length - 1, 1);
            }
        }

        return result;
    }

    //https://codeproject.com/Articles/2270/Inside-C-Second-Edition-String-Handling-and-Regula
    public static string ToTitleCase(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        string result = source.ToLower();
        foreach (string words in result.Split())
        {
            result += char.ToUpper(words[0]);
            result += (words.Substring(1, words.Length - 1) + ' ');
        }
        return result;
    }

    //https://codeproject.com/Articles/2270/Inside-C-Second-Edition-String-Handling-and-Regula
    public static bool IsPalindrome(this string source)
    {
        if (String.IsNullOrEmpty(source)) return false;

        int fulllength = source.Length - 1;
        int halflength = fulllength / 2;
        for (int i = 0; i <= halflength; i++)
        {
            if (source.Substring(i, 1) != source.Substring(fulllength - i, 1))
            {
                return false;
            }
        }
        return true;
    }
    public static List<string> Palindromes(this string source, int length)
    {
        List<string> palindromes = new List<string>();
        if (string.IsNullOrEmpty(source)) return palindromes;

        for (int i = 0; i < source.Length; i++)
        {
            // Case 1: Odd-length palindromes (single character center, e.g., "aba")
            ExpandAroundCenter(source, i, i, length, palindromes);

            // Case 2: Even-length palindromes (center is between two chars, e.g., "abba")
            ExpandAroundCenter(source, i, i + 1, length, palindromes);
        }

        return palindromes;
    }
    private static void ExpandAroundCenter(string s, int left, int right, int length, List<string> palindromes)
    {
        while (left >= 0 && right < s.Length && s[left] == s[right])
        {
            int palindromes_length = right - left + 1;
            if (palindromes_length >= length)
            {
                palindromes.Add(s.Substring(left, palindromes_length));
            }
            left--;
            right++;
        }
    }

    public static bool IsDigitsOnly(this string source)
    {
        foreach (char c in source)
        {
            if (!Char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsBinary(this string source)
    {
        foreach (char c in source)
        {
            if ((c != ' ') && (c != '0') && (c != '1'))
            {
                return false;
            }
        }
        return true;
    }
    public static long BinaryToDecimal(this string source)
    {
        long value = 0L;

        if (source.IsBinary())
        {
            string[] word_texts = source.Split();

            foreach (string word_text in word_texts)
            {
                long word_value = Radix.Decode(word_text, 2);
                value += word_value;
            }
        }

        return value;
    }
    // ConvertTo binary text into decimal words/line values
    public static string BinaryToDecimalString(this string source)
    {
        StringBuilder str = new StringBuilder();

        if (source.IsBinary())
        {
            long verse_value = 0L;
            string[] word_texts = source.Split();

            foreach (string word_text in word_texts)
            {
                long word_value = Radix.Decode(word_text, 2);
                verse_value += word_value;
                str.Append(word_value.ToString() + "+");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }

            str.Append("\t" + verse_value.ToString());
        }

        return str.ToString();
    }

    /// <summary>
    /// returns an Arabic letter-by-letter de-transliteration of the source buckwater string
    /// </summary>
    /// <returns></returns>
    public static string ToArabic(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        Dictionary<char, char> dictionary = new Dictionary<char, char>()
        {
            { '\'', 'ء' },
            { '>', 'أ' },
            { '&', 'ؤ' },
            { '<', 'إ' },
            { '}', 'ئ' },
            { 'A', 'ا' },
            { 'b', 'ب' },
            { 'p', 'ة' },
            { 't', 'ت' },
            { 'v', 'ث' },
            { 'j', 'ج' },
            { 'H', 'ح' },
            { 'x', 'خ' },
            { 'd', 'د' },
            { '*', 'ذ' },
            { 'r', 'ر' },
            { 'z', 'ز' },
            { 's', 'س' },
            { '$', 'ش' },
            { 'S', 'ص' },
            { 'D', 'ض' },
            { 'T', 'ط' },
            { 'Z', 'ظ' },
            { 'E', 'ع' },
            { 'g', 'غ' },
            { '_', 'ـ' },
            { 'f', 'ف' },
            { 'q', 'ق' },
            { 'k', 'ك' },
            { 'l', 'ل' },
            { 'm', 'م' },
            { 'n', 'ن' },
            { 'h', 'ه' },
            { 'w', 'و' },
            { 'Y', 'ى' },
            { 'y', 'ي' },
            { 'F', 'ً' },
            { 'N', 'ٌ' },
            { 'K', 'ٍ' },
            { 'a', 'َ' },
            { 'u', 'ُ' },
            { 'i', 'ِ' },
            { '~', 'ّ' },
            { 'o', 'ْ' },
            { '^', 'ٓ' },
            { '#', 'ٔ' },
            { '`', 'ٰ' },
            { '{', 'ٱ' },
            { ':', 'ۜ' },
            { '@', '۟' },
            { '"', '۠' },
            { '[', 'ۢ' },
            { ';', 'ۣ' },
            { ',', 'ۥ' },
            { '.', 'ۧ' },
            { '!', 'ۨ' },
            { '-', '۪' },
            { '+', '۫' },
            { '%', '۬' },
            { ']', 'ۭ' }
        };

        StringBuilder str = new StringBuilder();
        if (dictionary != null)
        {
            foreach (char c in source)
            {
                if (dictionary.ContainsKey(c))
                {
                    str.Append(dictionary[c]);
                }
                else
                {
                    str.Append(c);
                }
            }
        }
        return str.ToString();
    }
    /// <summary>
    /// returns a Buckwalter letter-by-letter transliteration of the source arabic string
    /// </summary>
    /// <returns></returns>
    public static string ToBuckwalter(this string source)
    {
        if (String.IsNullOrEmpty(source)) return "";

        Dictionary<char, char> dictionary = new Dictionary<char, char>()
        {
            {'ء', '\'' },
            { 'أ', '>' },
            { 'ؤ', '&' },
            { 'إ', '<' },
            { 'ئ', '}' },
            { 'ا', 'A' },
            { 'ب', 'b' },
            { 'ة', 'p' },
            { 'ت', 't' },
            { 'ث', 'v' },
            { 'ج', 'j' },
            { 'ح', 'H' },
            { 'خ', 'x' },
            { 'د', 'd' },
            { 'ذ', '*' },
            { 'ر', 'r' },
            { 'ز', 'z' },
            { 'س', 's' },
            { 'ش', '$' },
            { 'ص', 'S' },
            { 'ض', 'D' },
            { 'ط', 'T' },
            { 'ظ', 'Z' },
            { 'ع', 'E' },
            { 'غ', 'g' },
            { 'ـ', '_' },
            { 'ف', 'f' },
            { 'ق', 'q' },
            { 'ك', 'k' },
            { 'ل', 'l' },
            { 'م', 'm' },
            { 'ن', 'n' },
            { 'ه', 'h' },
            { 'و', 'w' },
            { 'ى', 'Y' },
            { 'ي', 'y' },
            { 'ً', 'F' },
            { 'ٌ', 'N' },
            { 'ٍ', 'K' },
            { 'َ', 'a' },
            { 'ُ', 'u' },
            { 'ِ', 'i' },
            { 'ّ', '~' },
            { 'ْ', 'o' },
            { 'ٓ', '^' },
            { 'ٔ', '#' },
            { 'ٰ', '`' },
            { 'ٱ', '{' },
            { 'ۜ', ':' },
            { '۟', '@' },
            { '۠', '"' },
            { 'ۢ', '[' },
            { 'ۣ', ';' },
            { 'ۥ', ',' },
            { 'ۧ', '.' },
            { 'ۨ', '!' },
            { '۪', '-' },
            { '۫', '+' },
            { '۬', '%' },
            { 'ۭ', ']' }
        };

        StringBuilder str = new StringBuilder();
        if (dictionary != null)
        {
            foreach (char c in source)
            {
                if (dictionary.ContainsKey(c))
                {
                    str.Append(dictionary[c]);
                }
                else
                {
                    str.Append(c);
                }
            }
        }
        return str.ToString();
    }

    /// <summary>
    /// Is source made up of any combination of:
    /// Arabic letters with or without diacritics (harakat), stopmarks, common symbols, and/or Indian numbers?
    /// </summary>
    public static bool IsArabic(this string source)
    {
        if (String.IsNullOrEmpty(source)) return false;

        foreach (char character in source)
        {
            if (character == ' ') continue;
            if (character == '\r') continue;
            if (character == '\n') continue;
            if (character == '\t') continue;
            if (Constants.ARABIC_LETTERS.Contains(character)) continue;
            if (Constants.INDIAN_DIGITS.Contains(character)) continue;
            if (Constants.DIACRITICS.Contains(character)) continue;
            if (Constants.STOPMARKS.Contains(character)) continue;
            if (Constants.QURANMARKS.Contains(character)) continue;
            if (Constants.SYMBOLS.Contains(character)) continue;

            return false;
        }

        return true;
    }
    /// <summary>
    /// Is source made up of any combination of:
    /// Arabic letters, stopmarks, common symbols, and/or Indian numbers
    /// AND
    /// diacritics (harakat)?
    /// </summary>
    public static bool IsArabicWithDiacritics(this string source)
    {
        if (String.IsNullOrEmpty(source)) return false;

        if (source.IsArabic())
        {
            string simplified_text = source.Simplify36();
            return (source.Length > simplified_text.Length);
        }

        return false;
    }
    public static string GetDiacritics(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        StringBuilder str = new StringBuilder();
        foreach (char character in source)
        {
            if (
                Constants.ARABIC_DIGITS.Contains(character) ||
                Constants.INDIAN_DIGITS.Contains(character) ||
                (Constants.ORNATE_RIGHT_PARENTHESIS[0] == character) ||
                (Constants.ORNATE_LEFT_PARENTHESIS[0] == character) ||
                Constants.STOPMARKS.Contains(character) ||
                Constants.QURANMARKS.Contains(character) ||
                Constants.DIACRITICS.Contains(character)
               )
            {
                str.Append(character);
            }
        }

        return str.ToString();
    }
    public static bool IsEnglish(this string source)
    {
        if (String.IsNullOrEmpty(source)) return false;

        foreach (char character in source)
        {
            if (
                  !(character == ' ') &&
                  !(character == '\r') &&
                  !(character == '\n') &&
                  !(character == '\t') &&
                  !((character >= 'A') && (character <= 'Z')) &&
                  !((character >= 'a') && (character <= 'z')) &&
                  !Constants.SYMBOLS.Contains(character)
               )
            {
                return false;
            }
        }

        return true;
    }

    public static string Simplify28(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        string result = source.Simplify29();

        return result.Replace("ء", "");
    }
    public static string Simplify29(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        string result = source.Simplify31();

        result = result.Replace("ة", "ه");
        return result.Replace("ى", "ي");
    }
    public static string Simplify30(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        string result = source.Simplify31();

        return result.Replace("ء", "");
    }
    public static string Simplify31(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        string result = source.Simplify36();

        StringBuilder str = new StringBuilder();
        foreach (char character in result)
        {
            switch (character)
            {
                case 'إ': str.Append("ا"); break;
                case 'أ': str.Append("ا"); break;
                case 'ٱ': str.Append("ا"); break;
                case 'آ': str.Append("ا"); break;
                case 'ؤ': str.Append("و"); break;
                case 'ئ': str.Append("ي"); break;
                default: str.Append(character); break;
            }
        }
        return str.ToString();
    }
    public static string Simplify36(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        // FASTER than Replace // 10s QuranCode startup vs 30s using Replace
        StringBuilder str = new StringBuilder();
        foreach (char character in source)
        {
            if (
                  !Constants.ARABIC_DIGITS.Contains(character) &&
                  !Constants.INDIAN_DIGITS.Contains(character) &&
                  !Constants.DIACRITICS.Contains(character) &&
                  !(Constants.ORNATE_RIGHT_PARENTHESIS[0] == character) &&
                  !(Constants.ORNATE_LEFT_PARENTHESIS[0] == character) &&
                  !Constants.STOPMARKS.Contains(character) &&
                  !Constants.QURANMARKS.Contains(character)
               )
            {
                str.Append(character);
            }
        }

        string result = str.ToString();
        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }

        //result = result.Replace("آ", "ءا");
        return result;
    }
    public static string SimplifyDots(this string source)
    {
        if (String.IsNullOrEmpty(source)) return source;

        // FASTER than Replace // 10s QuranCode startup vs 30s using Replace
        StringBuilder str = new StringBuilder();
        foreach (char character in source)
        {
            if (
                  !Constants.ARABIC_DIGITS.Contains(character) &&
                  !Constants.INDIAN_DIGITS.Contains(character) &&
                  !Constants.DIACRITICS.Contains(character) &&
                  !(Constants.ORNATE_RIGHT_PARENTHESIS[0] == character) &&
                  !(Constants.ORNATE_LEFT_PARENTHESIS[0] == character) &&
                  !Constants.STOPMARKS.Contains(character) &&
                  !Constants.QURANMARKS.Contains(character)
               )
            {
                str.Append(character);
            }
        }

        string result = str.ToString();
        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }

        //result = result.Replace("آ", "ءا");
        return result;
    }
    public static string Simplify(this string source, string text_mode)
    {
        if (String.IsNullOrEmpty(source)) return "";

        if (text_mode == "Simplified28")
        {
            return source.Simplify28();
        }
        else if (text_mode == "Simplified29")
        {
            return source.Simplify29();
        }
        else if (text_mode == "Simplified30")
        {
            return source.Simplify30();
        }
        else if (text_mode == "Simplified31")
        {
            return source.Simplify31();
        }
        else if (text_mode == "Simplified36")
        {
            return source.Simplify36();
        }
        else if (text_mode == "SimplifiedDots")
        {
            return source.Simplify36(); // BUT final ي --> ى
        }
        else if ((text_mode == "Original") || (text_mode == "SimplifiedMarks"))
        {
            //if (Globals.EDITION == Edition.Standard)
            //{
            //    return source.Simplify29();
            //}
            //return source;

            return source.Simplify29();
        }

        return source;
    }

    public static Dictionary<char, List<int>> LetterPositions(this string source)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterPositions(false, false);
        }
        return null;
    }
    public static Dictionary<char, List<int>> LetterPositions(this string source, bool case_sensitive)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterPositions(case_sensitive, false);
        }
        return null;
    }
    public static Dictionary<char, List<int>> LetterPositions(this string source, bool case_sensitive, bool ignore_diacritics)
    {
        Dictionary<char, List<int>> result = new Dictionary<char, List<int>>();
        if (!String.IsNullOrEmpty(source))
        {
            string temp = null;
            if (!case_sensitive) temp = source.ToUpper();
            if (ignore_diacritics && temp.IsArabicWithDiacritics()) temp = temp.Simplify29(); // ignore_diacritics in Original text_mode

            int whitespaces = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                if ((temp[i] == '\0') || (temp[i] == ' ') || (temp[i] == '\t') || (temp[i] == '\r') || (temp[i] == '\n'))
                {
                    whitespaces++;
                }
                else
                {
                    char character = temp[i];
                    int position = i + 1 - whitespaces;
                    if (result.ContainsKey(character)) // update existing entry
                    {
                        result[character].Add(position);
                    }
                    else // create new entry
                    {
                        result.Add(character, new List<int>() { position });
                    }
                }
            }
        }
        return result;
    }
    public static long LetterPositionsSum(this string source)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterPositionsSum(false, false);
        }
        return 0L;
    }
    public static long LetterPositionsSum(this string source, bool case_sensitive)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterPositionsSum(case_sensitive, false);
        }
        return 0L;
    }
    public static long LetterPositionsSum(this string source, bool case_sensitive, bool ignore_diacritics)
    {
        long result = 0L;
        if (!String.IsNullOrEmpty(source))
        {
            Dictionary<char, List<int>> positions = LetterPositions(source, case_sensitive, ignore_diacritics);
            if (positions != null)
            {
                foreach (char key in positions.Keys)
                {
                    foreach (int position in positions[key])
                    {
                        result += position;
                    }
                }
            }
        }
        return result;
    }
    public static Dictionary<char, List<int>> LetterDistances(this string source)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterDistances(false, false);
        }
        return null;
    }
    public static Dictionary<char, List<int>> LetterDistances(this string source, bool case_sensitive)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterDistances(case_sensitive, false);
        }
        return null;
    }
    public static Dictionary<char, List<int>> LetterDistances(this string source, bool case_sensitive, bool ignore_diacritics)
    {
        Dictionary<char, List<int>> result = new Dictionary<char, List<int>>();
        if (!String.IsNullOrEmpty(source))
        {
            string temp = null;
            if (!case_sensitive) temp = source.ToUpper();
            if (ignore_diacritics && temp.IsArabicWithDiacritics()) temp = temp.Simplify29(); // ignore_diacritics in Original text_mode

            int whitespaces = 0;
            Dictionary<char, int> last_position = new Dictionary<char, int>();
            for (int i = 0; i < temp.Length; i++)
            {
                if ((temp[i] == '\0') || (temp[i] == ' ') || (temp[i] == '\t') || (temp[i] == '\r') || (temp[i] == '\n'))
                {
                    whitespaces++;
                }
                else
                {
                    char character = temp[i];
                    int position = i + 1 - whitespaces;
                    int distance = 0;
                    if (result.ContainsKey(character)) // update existing entry
                    {
                        if (last_position.ContainsKey(character))
                        {
                            distance = position - last_position[character];
                            result[character].Add(distance);
                            last_position[character] = position;
                        }
                    }
                    else // create new empty entry
                    {
                        last_position.Add(character, position);
                        result.Add(character, new List<int>());
                    }
                }
            }
        }
        return result;
    }
    public static long LetterDistancesSum(this string source)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterDistancesSum(false, false);
        }
        return 0L;
    }
    public static long LetterDistancesSum(this string source, bool case_sensitive)
    {
        if (!String.IsNullOrEmpty(source))
        {
            return source.LetterDistancesSum(case_sensitive, false);
        }
        return 0L;
    }
    public static long LetterDistancesSum(this string source, bool case_sensitive, bool ignore_diacritics)
    {
        long result = 0L;
        if (!String.IsNullOrEmpty(source))
        {
            Dictionary<char, List<int>> distances = LetterDistances(source, case_sensitive, ignore_diacritics);
            if (distances != null)
            {
                foreach (char key in distances.Keys)
                {
                    foreach (int distance in distances[key])
                    {
                        result += distance;
                    }
                }
            }
        }
        return result;
    }

    public static bool Contains(this string source, string value, StringComparison string_comparison)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(value)) return false;

        return source.IndexOf(value, string_comparison) != -1;
    }
    public static bool Contains(this string source, string value)
    {
        return Contains(source, value, false);
    }
    public static bool Contains(this string source, string value, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(value)) return false;

        return case_sensitive ? source.Contains(value, StringComparison.InvariantCulture) : source.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }
    public static string Left(this string source, int length)
    {
        if (String.IsNullOrEmpty(source)) return "";

        if ((length > 0) && (length <= source.Length))
        {
            return source.Substring(0, length);
        }
        else
        {
            throw new ArgumentException();
        }
    }
    public static string Mid(this string source, int start, int end)
    {
        if (String.IsNullOrEmpty(source)) return "";

        if ((start >= end)
            && (start >= 0) && (start < source.Length)
            && (end >= 0) && (end < source.Length)
            )
        {
            return source.Substring(start, end - start);
        }
        else
        {
            throw new ArgumentException();
        }
    }
    public static string Right(this string source, int length)
    {
        if (String.IsNullOrEmpty(source)) return "";

        if ((length > 0) && (length <= source.Length))
        {
            return source.Substring(source.Length - 1 - length);
        }
        else
        {
            throw new ArgumentException();
        }
    }
    /// <summary>
    /// right trims source string at length if too long, or right space-pad source to length if too short.
    /// <para>Examples:</para>
    /// <para>"en.english".Pad(13)         returns  "en.english   "</para>
    /// <para>"en.transliteration".Pad(13) returns  "en.transliter"</para>
    /// </summary>
    public static string Pad(this string source, int length)
    {
        if (String.IsNullOrEmpty(source)) return "";

        if (source.Length >= length)
        {
            return source.Substring(0, length);
        }
        else
        {
            return source.PadRight(length);
        }
    }
    /// <summary>
    /// rigth trims source string from start to length if too long, or right space-pad source to length if too short.
    /// <para>Examples:</para>
    /// <para>"en.english".Pad(3, 10)         returns  "english   "</para>
    /// <para>"en.transliteration".Pad(3, 10) returns  "transliter"</para>
    /// </summary>
    public static string Pad(this string source, int start, int length)
    {
        if (String.IsNullOrEmpty(source)) return "";

        if (source.Length > start)
        {
            // right trim
            string result = source.Substring(start);

            if (result.Length >= length)
            {
                return result.Substring(0, length);
            }
            else
            {
                return result.PadRight(length);
            }
        }
        else
        {
            throw new ArgumentException();
        }
    }

    /// <summary>
    /// truncate source string to maximum length if too long.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="length">maximum length</param>
    /// <returns></returns>
    public static string Truncate(this string source, int length)
    {
        if (string.IsNullOrEmpty(source)) return source;
        return source.Length <= length ? source : source.Substring(0, length - 3) + "...";
    }

    /// <summary> 
    /// returns true if source contains target inside it not at start or end, else false. 
    /// </summary>
    public static bool ContainsInside(this string source, string target)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;
        if (source.Length < target.Length + 2) return false; // ensure at least 1 extra character at both ends of source

        // remove first character
        string temp = source.Remove(0, 1);
        // remove last character
        temp = temp.Remove(temp.Length - 1, 1);

        return temp.Contains(target);
    }

    /// <summary> 
    /// returns true if source contains any word of target, else false. 
    /// </summary>
    public static bool ContainsWord(this string source, string target)
    {
        return ContainsWord(source, target, false);
    }
    public static bool ContainsWord(this string source, string target, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;
        if (source == target) return true;

        string[] source_words = source.Split();
        string[] target_words = target.Split();
        int source_word_count = source_words.Length;
        int target_word_count = target_words.Length;

        for (int j = 0; j < target_word_count; j++)
        {
            for (int i = 0; i < source_word_count; i++)
            {
                if (case_sensitive)
                {
                    if (source_words[i] == target_words[j])
                    {
                        return true;
                    }
                }
                else
                {
                    if (source_words[i].ToLower() == target_words[j].ToLower())
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    /// <summary> 
    /// returns true if source contains word_count words of target in any order, else false. 
    /// </summary>
    public static bool ContainsWords(this string source, string target, int word_count)
    {
        return ContainsWords(source, target, word_count, false);
    }
    public static bool ContainsWords(this string source, string target, int word_count, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;
        if (source == target) return true;

        string[] source_words = source.Split();
        string[] target_words = target.Split();
        int source_word_count = source_words.Length;
        int target_word_count = target_words.Length;

        int common_word_count = 0;
        for (int j = 0; j < target_word_count; j++)
        {
            for (int i = 0; i < source_word_count; i++)
            {
                if (case_sensitive)
                {
                    if (source_words[i] != null)
                    {
                        if (source_words[i] == target_words[j])
                        {
                            source_words[i] = null; // remove it from list so not to be reused
                            common_word_count++;
                            break;
                        }
                    }
                }
                else
                {
                    if (source_words[i] != null)
                    {
                        if (source_words[i].ToLower() == target_words[j].ToLower())
                        {
                            source_words[i] = null; // remove it from list so not to be reused
                            common_word_count++;
                            break;
                        }
                    }
                }
            }
        }
        if (common_word_count >= word_count)
        {
            return true;
        }
        return false;
    }
    /// <summary> 
    /// returns true if source contains all words of target in any order, else false. 
    /// </summary>
    public static bool ContainsWords(this string source, string target)
    {
        return ContainsWords(source, target, false);
    }
    public static bool ContainsWords(this string source, string target, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;
        if (source == target) return true;

        string[] target_words = target.Split();
        int target_word_count = target_words.Length;

        return ContainsWords(source, target, target_word_count, case_sensitive);
    }

    /// <summary> 
    /// returns true if source contains any word of target_words, else false. 
    /// </summary>
    public static bool ContainsWord(this string source, List<string> target_words)
    {
        return ContainsWord(source, target_words, false);
    }
    public static bool ContainsWord(this string source, List<string> target_words, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if ((target_words == null) || (target_words.Count == 0)) return false;

        string[] source_words = source.Split();
        int source_word_count = source_words.Length;
        int target_word_count = target_words.Count;

        for (int j = 0; j < target_word_count; j++)
        {
            for (int i = 0; i < source_word_count; i++)
            {
                if (case_sensitive)
                {
                    if (source_words[i].Contains(target_words[j]))
                    {
                        return true;
                    }
                }
                else
                {
                    if (source_words[i].ToLower().Contains(target_words[j].ToLower()))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    /// <summary> 
    /// returns true if source contains word_count words of target_words in any order, else false. 
    /// </summary>
    public static bool ContainsWords(this string source, List<string> target_words, int word_count)
    {
        return ContainsWords(source, target_words, word_count, false);
    }
    public static bool ContainsWords(this string source, List<string> target_words, int word_count, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if ((target_words == null) || (target_words.Count == 0)) return false;

        string[] source_words = source.Split();
        int source_word_count = source_words.Length;
        int target_word_count = target_words.Count;

        int common_word_count = 0;
        for (int j = 0; j < target_word_count; j++)
        {
            for (int i = 0; i < source_word_count; i++)
            {
                if (case_sensitive)
                {
                    if (source_words[i] != null)
                    {
                        if (source_words[i].Contains(target_words[j]))
                        {
                            source_words[i] = null; // remove it from list so not to be reused
                            common_word_count++;
                            break;
                        }
                    }
                }
                else
                {
                    if (source_words[i] != null)
                    {
                        if (source_words[i].ToLower().Contains(target_words[j].ToLower()))
                        {
                            source_words[i] = null; // remove it from list so not to be reused
                            common_word_count++;
                            break;
                        }
                    }
                }
            }
        }

        if (common_word_count >= word_count)
        {
            return true;
        }
        return false;
    }
    /// <summary> 
    /// returns true if source contains all words of target_words in any order, else false. 
    /// </summary>
    public static bool ContainsWords(this string source, List<string> target_words)
    {
        return ContainsWords(source, target_words, false);
    }
    public static bool ContainsWords(this string source, List<string> target_words, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if ((target_words == null) || (target_words.Count == 0)) return false;

        int target_word_count = target_words.Count;

        return ContainsWords(source, target_words, target_word_count, case_sensitive);
    }

    /// <summary> 
    /// returns true if target contains similar words to source words in any order, else false. 
    /// </summary>
    public static bool HasSimilarWordsTo(this string source, string target, double similarity_percentage)
    {
        return HasSimilarWordsTo(source, target, similarity_percentage, false, false);
    }
    public static bool HasSimilarWordsTo(this string source, string target, double similarity_percentage, bool or_greater)
    {
        return HasSimilarWordsTo(source, target, similarity_percentage, or_greater, false);
    }
    public static bool HasSimilarWordsTo(this string source, string target, double similarity_percentage, bool or_greater, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;

        if (source == target) return true;

        string[] source_words = source.Split();
        string[] target_words = target.Split();
        int source_word_count = source_words.Length;
        int target_word_count = target_words.Length;

        int match_count = 0;
        for (int j = 0; j < target_word_count; j++)
        {
            for (int i = 0; i < source_word_count; i++)
            {
                if (source_words[i] != null)
                {
                    if (source_words[i].IsSimilarTo(target_words[j], similarity_percentage, case_sensitive))
                    {
                        source_words[i] = null; // remove it from list so not to be used again
                        match_count++;
                        break;
                    }
                }
            }
        }

        return (match_count >= target.Length);
    }
    /// <summary> 
    /// returns true if target contains similarity_percentage of same words to source words in any order, else false. 
    /// </summary>
    public static bool HasSameWordsTo(this string source, string target, double similarity_percentage)
    {
        return HasSameWordsTo(source, target, similarity_percentage, false, false);
    }
    public static bool HasSameWordsTo(this string source, string target, double similarity_percentage, bool or_greater)
    {
        return HasSimilarWordsTo(source, target, similarity_percentage, or_greater, false);
    }
    public static bool HasSameWordsTo(this string source, string target, double similarity_percentage, bool or_greater, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;

        if (source == target) return true;

        string[] source_words = source.Split();
        string[] target_words = target.Split();
        int source_word_count = source_words.Length;
        int target_word_count = target_words.Length;

        int match_count = 0;
        for (int j = 0; j < target_word_count; j++)
        {
            for (int i = 0; i < source_word_count; i++)
            {
                if (source_words[i] != null)
                {
                    if (String.Equals(source_words[i], target_words[j]))
                    {
                        source_words[i] = null; // remove it from list so not to be used again
                        match_count++;
                        break;
                    }
                }
            }
        }

        return (match_count >= (target.Length * similarity_percentage));
    }
    /// <summary> 
    /// returns true if target contains similar words to source words in any order, else false. 
    /// </summary>
    public static bool HasSimilarWordsTo(this List<string> source, List<string> target, double similarity_percentage)
    {
        return HasSimilarWordsTo(source, target, similarity_percentage, false, false);
    }
    public static bool HasSimilarWordsTo(this List<string> source, List<string> target, double similarity_percentage, bool or_greater)
    {
        return HasSimilarWordsTo(source, target, similarity_percentage, or_greater, false);
    }
    public static bool HasSimilarWordsTo(this List<string> source, List<string> target, double similarity_percentage, bool or_greater, bool case_sensitive)
    {
        if (source == null) return false;
        if (target == null) return false;
        if (source.Count == 0) return false;
        if (target.Count == 0) return false;

        if (source == target) return true;

        List<string> target_copy = new List<string>(target);
        int match_count = 0;
        for (int i = 0; i < target_copy.Count; i++)
        {
            for (int j = 0; j < source.Count; j++)
            {
                if (target_copy[i] != null)
                {
                    if (target_copy[i].IsSimilarTo(source[j], similarity_percentage, or_greater, case_sensitive))
                    {
                        target_copy[i] = null; // remove it from list so not to be used again
                        match_count++;
                        break;
                    }
                }
            }
        }
        return (match_count >= target.Count);
    }
    /// <summary> 
    /// returns true if target contains similarity_percentage of same words to source words in any order, else false. 
    /// </summary>
    public static bool HasSameWordsTo(this List<string> source, List<string> target, double similarity_percentage)
    {
        return HasSameWordsTo(source, target, similarity_percentage, false, false);
    }
    public static bool HasSameWordsTo(this List<string> source, List<string> target, double similarity_percentage, bool or_greater)
    {
        return HasSameWordsTo(source, target, similarity_percentage, or_greater, false);
    }
    public static bool HasSameWordsTo(this List<string> source, List<string> target, double similarity_percentage, bool or_greater, bool case_sensitive)
    {
        if (source == null) return false;
        if (target == null) return false;
        if (source.Count == 0) return false;
        if (target.Count == 0) return false;

        if (source == target) return true;

        List<string> target_copy = new List<string>(target);
        int match_count = 0;
        if (case_sensitive)
        {
            for (int i = 0; i < target_copy.Count; i++)
            {
                for (int j = 0; j < source.Count; j++)
                {
                    if (target_copy[i] != null)
                    {
                        if (String.Equals(target_copy[i], source[j]))
                        {
                            target_copy[i] = null; // remove it from list so not to be used again
                            match_count++;
                            break;
                        }
                    }
                }
            }
        }
        else // case insensitive
        {
            for (int i = 0; i < target_copy.Count; i++)
            {
                for (int j = 0; j < source.Count; j++)
                {
                    if (target_copy[i] != null)
                    {
                        if (String.Equals(target_copy[i].ToUpper(), source[j].ToUpper()))
                        {
                            target_copy[i] = null; // remove it from list so not to be used again
                            match_count++;
                            break;
                        }
                    }
                }
            }
        }
        return (match_count >= (target.Count * similarity_percentage));
    }
    /// <summary> 
    /// returns true if target contains similarity_percentage of same values to source values in any order, else false. 
    /// </summary>
    public static bool HasSameValuesTo(this List<long> source, List<long> target, double similarity_percentage)
    {
        return HasSameValuesTo(source, target, similarity_percentage, false, false);
    }
    public static bool HasSameValuesTo(this List<long> source, List<long> target, double similarity_percentage, bool or_greater)
    {
        return HasSameValuesTo(source, target, similarity_percentage, or_greater, false);
    }
    public static bool HasSameValuesTo(this List<long> source, List<long> target, double similarity_percentage, bool or_greater, bool case_sensitive)
    {
        if (source == null) return false;
        if (target == null) return false;
        if (source.Count == 0) return false;
        if (target.Count == 0) return false;

        if (source == target) return true;

        List<long> target_copy = new List<long>(target);
        int match_count = 0;
        for (int i = 0; i < target_copy.Count; i++)
        {
            for (int j = 0; j < source.Count; j++)
            {
                if (target_copy[i] != -1)
                {
                    if (target_copy[i] == source[j])
                    {
                        target_copy[i] = -1; // remove it from list so not to be used again
                        match_count++;
                        break;
                    }
                }
            }
        }
        return (match_count >= (target.Count * similarity_percentage));
    }
    //Levenshtein's Edit Distance - Converges slowly but with good matches
    /// <summary> 
    /// returns true if source and target strings are similar to a given percentage, else false. 
    /// </summary>
    public static bool IsSimilarTo(this string source, string target, double similarity_percentage)
    {
        return source.IsSimilarTo(target, similarity_percentage, false, false);
    }
    /// <summary> 
    /// returns true if source and target strings are similar to a given percentage (or greater), else false. 
    /// </summary>
    public static bool IsSimilarTo(this string source, string target, double similarity_percentage, bool or_greater)
    {
        return source.IsSimilarTo(target, similarity_percentage, or_greater, false);
    }
    private static bool IsSimilarTo(this string source, string target, double similarity_percentage, bool or_greater, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;

        if (similarity_percentage == 1.0D) return (source == target);

        double similarity = 1.0D;
        if (source != target)
        {
            similarity = case_sensitive ? source.SimilarityTo(target) : source.ToLower().SimilarityTo(target.ToLower());
        }

        if (or_greater)
        {
            return (similarity >= (similarity_percentage - Numbers.ERROR_MARGIN));
        }
        else // exact percentage only
        {
            return ((similarity >= (similarity_percentage - Numbers.ERROR_MARGIN)) && (similarity <= (similarity_percentage + Numbers.ERROR_MARGIN)));
        }
    }
    /// <summary> 
    /// returns true if source and target strings are similar to a given percentage range (from ... to), else false. 
    /// </summary>
    public static bool IsSimilarTo(this string source, string target, double from_similarity_percentage, double to_similarity_percentage)
    {
        return source.IsSimilarTo(target, from_similarity_percentage, to_similarity_percentage, false);
    }
    private static bool IsSimilarTo(this string source, string target, double from_similarity_percentage, double to_similarity_percentage, bool case_sensitive)
    {
        if (String.IsNullOrEmpty(source)) return false;
        if (String.IsNullOrEmpty(target)) return false;

        if (from_similarity_percentage == 1.0D) return (source == target);

        double similarity = 1.0D;
        if (source != target)
        {
            similarity = case_sensitive ? source.SimilarityTo(target) : source.ToLower().SimilarityTo(target.ToLower());
        }

        return ((similarity >= (from_similarity_percentage - Numbers.ERROR_MARGIN)) && (similarity <= (to_similarity_percentage + Numbers.ERROR_MARGIN)));
    }
    /// <summary> 
    /// returns the percentage similarity between source and target strings. 
    /// </summary>
    public static double SimilarityTo(this string source, string target)
    {
        if (String.IsNullOrEmpty(source)) return 0.0D;
        if (String.IsNullOrEmpty(target)) return 0.0D;

        if (source == target) return 1.0D;

        int steps_to_the_same = ComputeLevenshteinDistance(source, target);
        return (1.0D - ((double)steps_to_the_same / (double)Math.Max(source.Length, target.Length)));
    }
    /// <summary>
    /// returns the number of steps required to transform the source string into the target string. 
    /// Re: https://dotnetperls.com/levenshtein
    /// </summary>
    private static int ComputeLevenshteinDistance(string source, string target)
    {
        if (String.IsNullOrEmpty(source)) return 0;
        if (String.IsNullOrEmpty(target)) return 0;

        if (source == target) return source.Length;

        int source_word_count = source.Length;
        int target_word_count = target.Length;
        int[,] distance = new int[source_word_count + 1, target_word_count + 1];

        // Step 1
        if (source_word_count == 0)
        {
            return target_word_count;
        }
        if (target_word_count == 0)
        {
            return source_word_count;
        }

        // Step 2
        for (int i = 0; i <= source_word_count; distance[i, 0] = i++)
        {
        }
        for (int j = 0; j <= target_word_count; distance[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= source_word_count; i++)
        {
            //Step 4
            for (int j = 1; j <= target_word_count; j++)
            {
                // Step 5
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                // Step 6
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        // Step 7
        return distance[source_word_count, target_word_count];
    }

    // VERY SLOW - ONLY SUITABLE FOR SHORT STRINGS
    // Video: https://coursera.org/lecture/cs-algorithms-theory-machines/longest-repeated-substring-hkJBt
    public static string LongestRepeatedSubstring(this string text)
    {
        int len = text.Length;
        string[] suffixes = new string[len];
        for (int i = 0; i < len; i++)
        {
            suffixes[i] = text.Substring(i, len - i);
        }

        Array.Sort(suffixes);

        string result = "";
        for (int i = 0; i < len - 1; i++)
        {
            string longest_prefix = LongestCommonString(suffixes[i], suffixes[i + 1]);
            if (longest_prefix.Length > result.Length)
            {
                result = longest_prefix;
            }
        }

        return result;
    }
    public static string LongestCommonString(string a, string b)
    {
        int min = Math.Min(a.Length, b.Length);
        for (int i = 0; i < min; i++)
        {
            if (a[i] != b[i])
            {
                return a.Substring(0, i);
            }
        }
        return a.Substring(0, min);
    }

    // VERY FAST - SUITABLE FOR LRGE STRINGS but in Javascript :(
    // Online Demo: https://daniel-hug.github.io/longest-repeated-substring/

    //4n+1 HashKey Generation
    public static long GetHashKey(this string source, int level)
    {
        long key = -1L;
        if (source == null) return key;
        if (source.Length < 5) return key;
        if ((level <= 0) || (level > 3)) return key;

        long odd = source.GetHashCode() * 2L + 1L;
        long even = source.GetHashCode() * 2L;
        switch (level)
        {
            case 0: // Lite Edition
                {
                    key = (4L * even) - 1L;
                }
                break;
            case 1: // Standard Edition
                {
                    key = (4L * odd) - 1L;
                }
                break;
            case 2: // Professional Edition
                {
                    key = (4L * even) + 1L;
                }
                break;
            case 3: // Ultimate Edition
                {
                    key = (4L * odd) + 1L;
                }
                break;
        }

        return key;
    }
    public static int GetHashKeyLevel(this string source, long key)
    {
        int level = -1;
        if (source == null) return level;
        if (source.Length < 5) return level;
        long odd = source.GetHashCode() * 2L + 1L;
        long even = source.GetHashCode() * 2L;
        if (key == (4L * even) - 1L) level = 0;         // Lite Edition
        else if (key == (4L * odd) - 1L) level = 1;     // Standard Edition
        else if (key == (4L * even) + 1L) level = 2;    // Professional Edition
        else if (key == (4L * odd) + 1L) level = 3;     // Ultimate Edition
        else level = -1;

        return level;
    }
}

public static class NumberExtensions
{
    public static string ToArabic(this int source)
    {
        StringBuilder str = new StringBuilder();
        try
        {
            int number = source;
            string text = number.ToString();
            for (int i = 0; i < text.Length; i++)
            {
                char digit = Constants.INDIAN_DIGITS[number % 10];
                str.Insert(0, digit);
                number /= 10;
            }
            return str.ToString();
        }
        catch
        {
            throw new ArgumentException();
        }
    }
    public static int DigitSum(this int source)
    {
        if (source < 0) source *= -1;

        int result = 0;
        int remainder = 0;
        do
        {
            remainder = source % 10;
            result += remainder;
            source = source / 10;
        } while (source != 0);
        return result;
    }
    public static int DigitalRoot(this int source)
    {
        if (source < 0) source *= -1;

        int result = DigitSum(source);
        while (result > 9)
        {
            result = DigitSum(result);
        }
        return result;
    }
    public static List<int> UnitsTensHundredsEtc(this int source)
    {
        List<int> result = new List<int>();

        int i = 10;
        while (source >= i / 10)
        {
            result.Add(source % i - source % (i / 10));
            i *= 10;
        }

        return result;
    }
    public static bool IsPrime(this int source)
    {
        if (source < 0) source *= -1;

        if (source == 0)        // 0 is neither prime nor composite
            return false;

        if (source == 1)        // 1 is the unit, indivisible
            return false;       // NOT prime

        if (source == 2)        // 2 is the first prime
            return true;

        if (source % 2 == 0)    // exclude even numbers to speed up search
            return false;

        int sqrt = (int)Math.Round(Math.Sqrt(source));
        for (int i = 3; i <= sqrt; i += 2)
        {
            if ((source % i) == 0)
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsAdditivePrime(this int source)
    {
        if (IsPrime(source))
        {
            return IsPrime(DigitSum(source));
        }
        return false;
    }
    public static bool IsNonAdditivePrime(this int source)
    {
        if (IsPrime(source))
        {
            return !IsPrime(DigitSum(source));
        }
        return false;
    }
    public static bool IsComposite(this int source)
    {
        if (source < 0) source *= -1;

        if (source == 0)        // 0 is NOT composite
            return false;

        if (source == 1)        // 1 is the unit, indivisible
            return false;       // NOT composite

        if (source == 2)        // 2 is the first prime
            return false;

        if (source % 2 == 0)    // even numbers are composite
            return true;

        int sqrt = (int)Math.Round(Math.Sqrt(source));
        for (int i = 3; i <= sqrt; i += 2)
        {
            if ((source % i) == 0)
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsAdditiveComposite(this int source)
    {
        if (IsComposite(source))
        {
            return IsComposite(DigitSum(source));
        }
        return false;
    }
    public static bool IsNonAdditiveComposite(this int source)
    {
        if (IsComposite(source))
        {
            return !IsComposite(DigitSum(source));
        }
        return false;
    }
    public static List<int> Factorize(int source)
    {
        List<int> result = new List<int>();

        if (source < 0)
        {
            result.Add(-1);
            source *= -1;
        }

        if ((source >= 0) && (source <= 2))
        {
            result.Add(source);
        }
        else // if (source > 2)
        {
            // if source has a prime factor add it to factors,
            // source /= p,
            // reloop until  source == 1
            while (source != 1)
            {
                if ((source % 2) == 0) // if even source
                {
                    result.Add(2);
                    source /= 2;
                }
                else // trial divide by all primes up to sqrt(source)
                {
                    int max = (int)Math.Round(Math.Sqrt(source)) + 1;	// extra 1 for double calculation errors

                    bool is_factor_found = false;
                    for (int i = 3; i <= max; i += 2)
                    {
                        if ((source % i) == 0)
                        {
                            is_factor_found = true;
                            result.Add(i);
                            source /= i;
                            break; // for loop, reloop while
                        }
                    }

                    // if no prime factor found the source must be prime in the first place
                    if (!is_factor_found)
                    {
                        result.Add(source);
                        break; // while loop
                    }
                }
            }
        }

        return result;
    }

    public static int DigitSum(this long source)
    {
        if (source < 0L) source *= -1L;

        int result = 0;
        int remainder = 0;
        do
        {
            remainder = (int)(source % 10L);
            result += remainder;
            source = source / 10L;
        } while (source != 0);
        return result;
    }
    public static int DigitalRoot(this long source)
    {
        if (source < 0L) source *= -1L;

        int result = DigitSum(source);
        while (result > 9)
        {
            result = DigitSum(result);
        }
        return result;
    }
    public static List<long> UnitsTensHundredsEtc(this long source)
    {
        List<long> result = new List<long>();

        long i = 10;
        while (source >= i / 10)
        {
            result.Add(source % i - source % (i / 10));
            i *= 10;
        }

        return result;
    }
    public static bool IsPrime(this long source)
    {
        if (source < 0L) source *= -1L;

        if (source == 0L)        // 0 is neither prime nor composite
            return false;

        if (source == 1L)        // 1 is the unit, indivisible
            return false;        // NOT prime

        if (source == 2L)        // 2 is the first prime
            return true;

        if (source % 2L == 0L)   // exclude even numbers to speed up search
            return false;

        long sqrt = (long)Math.Round(Math.Sqrt(source));
        for (long i = 3L; i <= sqrt; i += 2L)
        {
            if ((source % i) == 0L)
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsAdditivePrime(this long source)
    {
        if (IsPrime(source))
        {
            return IsPrime(DigitSum(source));
        }
        return false;
    }
    public static bool IsNonAdditivePrime(this long source)
    {
        if (IsPrime(source))
        {
            return !IsPrime(DigitSum(source));
        }
        return false;
    }
    public static bool IsComposite(this long source)
    {
        if (source < 0L) source *= -1L;

        if (source == 0L)        // 0 is NOT composite
            return false;

        if (source == 1L)        // 1 is the unit, indivisible
            return false;        // NOT composite

        if (source == 2L)        // 2 is the first prime
            return false;

        if (source % 2L == 0L)   // even numbers are composite
            return true;

        long sqrt = (long)Math.Round(Math.Sqrt(source));
        for (long i = 3L; i <= sqrt; i += 2L)
        {
            if ((source % i) == 0L)
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsAdditiveComposite(this long source)
    {
        if (IsComposite(source))
        {
            return IsComposite(DigitSum(source));
        }
        return false;
    }
    public static bool IsNonAdditiveComposite(this long source)
    {
        if (IsComposite(source))
        {
            return !IsComposite(DigitSum(source));
        }
        return false;
    }
    public static List<long> Factorize(long source)
    {
        List<long> result = new List<long>();

        if (source < 0L)
        {
            result.Add(-1L);
            source *= -1L;
        }

        if ((source >= 0L) && (source <= 2L))
        {
            result.Add(source);
        }
        else // if (source > 2L)
        {
            // if source has a prime factor add it to factors,
            // source /= p,
            // reloop until  source == 1L
            while (source != 1L)
            {
                if ((source % 2L) == 0L) // if even source
                {
                    result.Add(2L);
                    source /= 2L;
                }
                else // trial divide by all primes up to sqrt(source)
                {
                    long max = (long)Math.Round(Math.Sqrt(source)) + 1L;	// extra 1 for double calculation errors

                    bool is_factor_found = false;
                    for (long i = 3L; i <= max; i += 2L)
                    {
                        if ((source % i) == 0L)
                        {
                            is_factor_found = true;
                            result.Add(i);
                            source /= i;
                            break; // for loop, reloop while
                        }
                    }

                    // if no prime factor found the source must be prime in the first place
                    if (!is_factor_found)
                    {
                        result.Add(source);
                        break; // while loop
                    }
                }
            }
        }

        return result;
    }
}

public static class ListExtensions
{
    public static bool ItemEquals<T>(this List<T> source, string value)
    {
        return source.ItemEquals(value, StringComparison.Ordinal);
    }
    public static bool ItemEquals<T>(this List<T> source, string value, StringComparison string_comparison)
    {
        foreach (T item in source)
        {
            if (item.ToString() == value)
            {
                return true;
            }
        }
        return false;
    }

    public static bool ItemContains<T>(this List<T> source, string value)
    {
        return source.ItemContains(value, StringComparison.Ordinal);
    }
    public static bool ItemContains<T>(this List<T> source, string value, StringComparison string_comparison)
    {
        foreach (T item in source)
        {
            if (item.ToString().IndexOf(value, string_comparison) != -1)
            {
                return true;
            }
        }
        return false;
    }

    public static bool ItemStartsWith<T>(this List<T> source, string value)
    {
        return source.ItemStartsWith(value, StringComparison.Ordinal);
    }
    public static bool ItemStartsWith<T>(this List<T> source, string value, StringComparison string_comparison)
    {
        foreach (T item in source)
        {
            if (item.ToString().StartsWith(value, string_comparison))
            {
                return true;
            }
        }
        return false;
    }

    public static List<T> RemoveDuplicates<T>(this List<T> source)
    {
        return source.RemoveDuplicates(StringComparison.Ordinal);
    }
    public static List<T> RemoveDuplicates<T>(this List<T> source, StringComparison string_comparison)
    {
        List<T> result = new List<T>();
        foreach (T item in source)
        {
            if (!result.ItemEquals(item.ToString(), string_comparison))
            {
                result.Add(item);
            }
        }
        return result;
    }

    public static List<T> Intersect<T>(this List<T> source, List<T> target)
    {
        List<T> result = new List<T>();
        foreach (T item in source)
        {
            if (target.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }
    public static List<T> Union<T>(this List<T> source, List<T> target)
    {
        List<T> result = new List<T>(source);
        foreach (T item in target)
        {
            if (!source.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }
    public static List<T> Minus<T>(this List<T> source, List<T> target)
    {
        List<T> result = new List<T>();
        foreach (T item in source)
        {
            if (!target.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }
    public static List<T> ToSet<T>(this List<T> source)
    {
        List<T> result = new List<T>();
        foreach (T item in source)
        {
            if (!result.Contains(item))
            {
                result.Add(item);
            }
        }
        return result;
    }
}
