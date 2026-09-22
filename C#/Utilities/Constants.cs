using System.Collections.Generic;

public static class Constants
{
    static Constants()
    {
        PopulateArabicLetterDotsDictionary();
    }

    public static string ORNATE_RIGHT_PARENTHESIS = "\uFD3F";  // ﴿
    public static string ORNATE_LEFT_PARENTHESIS = "\uFD3E";   // ﴾

    public static List<char> INITIAL_LETTERS = new List<char>()
    {
        'ا',
        'ح',
        'ر',
        'س',
        'ص',
        'ط',
        'ع',
        'ق',
        'ك',
        'ل',
        'م',
        'ن',
        'ه',
        'ي'
    };

    public static List<char> ARABIC_DIGITS = new List<char>()
    {
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9'
    };

    public static List<char> INDIAN_DIGITS = new List<char>()
    {
        '٠',
        '١',
        '٢',
        '٣',
        '٤',
        '٥',
        '٦',
        '٧',
        '٨',
        '٩'
    };

    public static List<char> ARABIC_LETTERS = new List<char>
    {
        'ء',
        'ا',
        'إ',
        'أ',
        'ٱ',
        'ب',
        'ت',
        'ث',
        'ج',
        'ح',
        'خ',
        'د',
        'ذ',
        'ر',
        'ز',
        'س',
        'ش',
        'ص',
        'ض',
        'ط',
        'ظ',
        'ع',
        'غ',
        'ف',
        'ق',
        'ك',
        'ل',
        'م',
        'ن',
        'ه',
        'ة',
        'و',
        'ؤ',
        'ى',
        'ي',
        'ئ'
    };

    public static List<char> ARABIC_LETTERS_UNDOTTED = new List<char>
    {
        'ء',
        'ا',
        'إ',
        'أ',
        'ٱ',
        'ح',
        'د',
        'ر',
        'س',
        'ص',
        'ط',
        'ع',
        'ك',
        'ل',
        'م',
        'ه',
        'و',
        'ؤ',
        'ى',
        'ئ'
    };

    public static List<char> ARABIC_LETTERS_DOTTED = new List<char>
    {
        'ب',
        'ت',
        'ث',
        'ج',
        'خ',
        'ذ',
        'ز',
        'ش',
        'ض',
        'ظ',
        'غ',
        'ف',
        'ق',
        'ن',
        'ة',
        'ي'
    };

    public static Dictionary<char, int> ARABIC_LETTERS_DOTS = null;
    private static void PopulateArabicLetterDotsDictionary()
    {
        ARABIC_LETTERS_DOTS = new Dictionary<char, int>();
        if (ARABIC_LETTERS_DOTS != null)
        {
            ARABIC_LETTERS_DOTS.Add('ء', 0);
            ARABIC_LETTERS_DOTS.Add('ا', 0);
            ARABIC_LETTERS_DOTS.Add('إ', 0);
            ARABIC_LETTERS_DOTS.Add('أ', 0);
            ARABIC_LETTERS_DOTS.Add('ٱ', 0);
            ARABIC_LETTERS_DOTS.Add('ح', 0);
            ARABIC_LETTERS_DOTS.Add('د', 0);
            ARABIC_LETTERS_DOTS.Add('ر', 0);
            ARABIC_LETTERS_DOTS.Add('س', 0);
            ARABIC_LETTERS_DOTS.Add('ص', 0);
            ARABIC_LETTERS_DOTS.Add('ط', 0);
            ARABIC_LETTERS_DOTS.Add('ع', 0);
            ARABIC_LETTERS_DOTS.Add('ك', 0);
            ARABIC_LETTERS_DOTS.Add('ل', 0);
            ARABIC_LETTERS_DOTS.Add('م', 0);
            ARABIC_LETTERS_DOTS.Add('ه', 0);
            ARABIC_LETTERS_DOTS.Add('و', 0);
            ARABIC_LETTERS_DOTS.Add('ؤ', 0);
            ARABIC_LETTERS_DOTS.Add('ى', 0);
            ARABIC_LETTERS_DOTS.Add('ئ', 0);
            ARABIC_LETTERS_DOTS.Add('ب', 1);
            ARABIC_LETTERS_DOTS.Add('ت', 2);
            ARABIC_LETTERS_DOTS.Add('ث', 3);
            ARABIC_LETTERS_DOTS.Add('ج', 1);
            ARABIC_LETTERS_DOTS.Add('خ', 1);
            ARABIC_LETTERS_DOTS.Add('ذ', 1);
            ARABIC_LETTERS_DOTS.Add('ز', 1);
            ARABIC_LETTERS_DOTS.Add('ش', 3);
            ARABIC_LETTERS_DOTS.Add('ض', 1);
            ARABIC_LETTERS_DOTS.Add('ظ', 1);
            ARABIC_LETTERS_DOTS.Add('غ', 1);
            ARABIC_LETTERS_DOTS.Add('ف', 1);
            ARABIC_LETTERS_DOTS.Add('ق', 2);
            ARABIC_LETTERS_DOTS.Add('ن', 1);
            ARABIC_LETTERS_DOTS.Add('ة', 2);
            ARABIC_LETTERS_DOTS.Add('ي', 2);
        }
    }

    public static List<char> STOPMARKS = new List<char>
    {
        'ۙ',    // Laaa         MustContinue
        'ۖ',    // Sala         ShouldContinue
        'ۚ',    // Jeem         CanStop
        'ۛ',    // Dots         CanStopAtEither
        'ۗ',    // Qala         ShouldStop
        'ۜ',    // Seen         MustPause
        'ۘ'     // Meem         MustStop
    };

    public static List<char> QURANMARKS = new List<char>
    {
        '۞',    // Partition mark
        '۩',    // Prostration mark (Obligatory)
        '⌂'     // Prostration mark (Recommended)
    };

    public static List<char> DIACRITICS = new List<char>
    {
        'ْ',
        'َ',
        'ِ',
        'ُ',
        'ّ',
        'ً',
        'ٍ',
        'ٌ',
        'ٰ',
        'ٔ',
        'ۥ',
        'ٓ',
        '۟',
        'ۦ',
        'ۧ',
        'ۭ',
        'ۢ',
        'ۜ',
        'ۣ',
        '۠',
        'ۨ',
        '۪',
        '۫',
        '۬',
        'ـ'
    };

    public static List<char> SYMBOLS = new List<char>
    {
        '{',
        '}',
        '[',
        ']',
        '<',
        '>',
        '(',
        ')',
        '.',
        ',',
        ';',
        '`',
        '!',
        ':',
        '=',
        '+',
        '-',
        '*',
        '/',
        '%',
        '\\',
        '"',
        '\'',
        '~',
        '@',
        '#',
        '$',
        '^',
        '&',
        '_',
        '|',
        '?',
        '‘',
        '÷',
        '×',
        '؛',
        'ـ',
        '—',
        '،',
        '"',
        '’',
        '.',
        '؟'
    };

    public static List<char> ENGLISH_LETTERS = new List<char>
    {
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'I',
        'J',
        'K',
        'L',
        'M',
        'N',
        'O',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'U',
        'V',
        'W',
        'X',
        'Y',
        'Z',
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'i',
        'j',
        'k',
        'l',
        'm',
        'n',
        'o',
        'p',
        'q',
        'r',
        's',
        't',
        'u',
        'v',
        'w',
        'x',
        'y',
        'z'
    };
    public static List<char> CAPITAL_ENGLISH_LETTERS = new List<char>
    {
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'I',
        'J',
        'K',
        'L',
        'M',
        'N',
        'O',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'U',
        'V',
        'W',
        'X',
        'Y',
        'Z'
    };
    public static List<char> SMALL_ENGLISH_LETTERS = new List<char>
    {
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'i',
        'j',
        'k',
        'l',
        'm',
        'n',
        'o',
        'p',
        'q',
        'r',
        's',
        't',
        'u',
        'v',
        'w',
        'x',
        'y',
        'z'
    };
}
