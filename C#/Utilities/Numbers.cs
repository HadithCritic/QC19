using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Security.Cryptography;

[Flags]
public enum Days
{
    None = 0x0000,
    Monday = 0x0001,
    Tuesday = 0x0002,
    Wednesday = 0x0004,
    Thursday = 0x0008,
    Friday = 0x0010,
    Saturday = 0x0020,
    Sunday = 0x0040,
    Workdays = Monday | Tuesday | Wednesday | Thursday | Friday,
    Weekend = Saturday | Sunday
}
// Days days = Days.None;                   // initialize
//if ((days & Days.Friday) == Days.Friday)  // has
//    days &= ~Days.Friday;                 // remove
//else
//    days |= Days.Friday;                  // add

[Flags]
public enum Directions
{
    None = 0x0000,
    North = 0x0001,
    South = 0x0002,
    East = 0x0004,
    West = 0x0008,
    NorthEast = 0x0010,
    NorthWest = 0x0020,
    SouthEast = 0x0040,
    SouthWest = 0x0080,
    All = North | South | East | West | NorthEast | NorthWest | SouthEast | SouthWest
    //Zigzag = 0x0100, // straight and zigzag (: عكرف لوي :)
}
// Directions directions = Directions.None;                 // initialize
//if ((directions & Directions.North) == Directions.North)  // has
//    directions &= ~Directions.North;                      // remove
//else
//    directions |= Directions.North;                       // add

public enum NumberType
{
    None,                   // not a number (eg infinity, -infinity)
    Unit,                   // indivisible by anything
    Prime,                  // divisible by self only (dividing by 1 doesn't divide into smaller parts and is misleading)
    AdditivePrime,          // prime with prime digit sum
    NonAdditivePrime,       // prime with non-prime digit sum
    Composite,              // divisible by self and other(s)
    AdditiveComposite,      // composite with composite digit sum
    NonAdditiveComposite,   // composite with non-composite digit sum
    Odd,                    // indivisible by 2
    Even,                   // divisible by 2
    Fibonacci,              // Fn+2 = Fn + Fn+1
    Square,                 // n*n
    Cubic,                  // n*n*n
    Quartic,                // n*n*n*n
    Quintic,                // n*n*n*n*n
    Sextic,                 // n*n*n*n*n*n          // also called hexic
    Septic,                 // n*n*n*n*n*n*n        // also called heptic
    Octic,                  // n*n*n*n*n*n*n*n
    Nonic,                  // n*n*n*n*n*n*n*n*n
    Decic,                  // n*n*n*n*n*n*n*n*n*n
    Natural                 // natural number from 1 to MaxValue
};

public enum IndexType { Any, Unit, Prime, Composite };
public enum IndexSubType { Any, Additive, NonAdditive };
public enum FactorsType { Any, Duplicate, Unique, SimilarPowers };

public enum NumberKind { Deficient, Perfect, Abundant };

public enum CVWLSumType { CVSum, CVWsSum, CVWsLsSum, CWsLsSum, CWsSum, CLsSum, VWsLsSum, VWsSum, VLsSum }

//https://en.wikipedia.org/wiki/Polygon
//----------------------------------------------------
//Ones		        Tens			Final suffix
//----------------------------------------------------
//1 hen-		    10 deca-		-gon 
//2 do-		        20 -cosa- 
//3 tri-		    30 triaconta- 
//4 tetra-	        40 tetraconta- 
//5 penta-	        50 pentaconta- 
//6 hexa-		    60 hexaconta- 
//7 hepta-	        70 heptaconta- 
//8 octa-		    80 octaconta- 
//9 ennea-/nona-	90 enneaconta-/nonaconta- 
//----------------------------------------------------
//
//Examples:
//7-gon  = hepta-                   -gon		= heptagon
//19-gon = nona-    deca-           -gon 		= nonadecagon 
//42-gon = do-      tetraconta-     -gon 		= dotetracontagon 
//50-gon =          pentaconta-     -gon		= pentacontagon 
//
//N-gon (for N > 99)			                = N-gon
//100-gon					                    = 100-gon
//----------------------------------------------------
public enum PolygonType
{
    Triangular,
    Square,
    Pentagonal,
    Hexagonal,
    Heptagonal,
    Octagonal,
    Nonagonal,
    Decagonal,
    Hendecagonal,
    Dodecagonal,
    Tridecagonal,
    Tetradecagonal,
    Pentadecagonal,
    Hexadecagonal,
    Heptadecagonal,
    Octadecagonal,
    Nonadecagonal,
    Icosagonal,
    Icosihenagonal,
    Icosidigonal,
    Icositrigonal,
    Icositetragonal
};

// Chemical polyhex hydrocarbons with 19 hexagons
// https://oeis.org/A120386
// Number of isomers of polyhex hydrocarbons with C_(2h) symmetry with nineteen hexagons
// 3, 17, 66, 189, 589, 1677, 3829, 7948, 15649, 25543, 26931, 15472 
// https://oeis.org/A120448
// Number of isomers of polyhex hydrocarbons with C_(2v) symmetry with nineteen hexagons
// 3, 17, 14, 92, 60, 316, 175, 814, 495, 2323, 1402, 6037, 3113, 12851, 6200, 24710, 11851, 46152, 18123, 72151, 18007, 74547, 8970, 40141
public enum PolyhexType
{
    C2hPolyhexHydrocarbon,
    C2vPolyhexHydrocarbon
};

// = ≠ < ≤ > ≥ ÷ !÷ Σ
public enum ComparisonOperator { Equal, NotEqual, LessThan, LessOrEqual, GreaterThan, GreaterOrEqual, DivisibleBy, IndivisibleBy, EqualSum, Reserved };

// + - * / %
public enum ArithmeticOperator { Plus, Minus, Multiply, Divide, Modulus };

public static class Numbers
{
    public const double ERROR_MARGIN = 0.0005D;

    public const int MAX_SERIES_LENGTH = 1024;
    public const int DEFAULT_RADIX = 10;
    public const int RADIX_NINETEEN = 19;
    public const int DEFAULT_DIVISOR = 19;
    public static Color DIVISOR_COLOR = Color.FromArgb(224, 255, 255);              // LightCyan

    public static int INTERESTINGX_COUNT =
        (Globals.EDITION == Edition.Standard) ? 2 :
        (Globals.EDITION == Edition.Research) ? 2 :
        (Globals.EDITION == Edition.Ultimate) ? 2 :
        (Globals.EDITION == Edition.BigNumbers) ? 3 : -1;
    public static Color INTERESTING_NUMBER_COLOR = Color.FromArgb(128, 255, 255);   // Cyan
    public static Color INTERESTINGD_NUMBER_COLOR = Color.FromArgb(224, 192, 255);  // LightPurple
    public static Color INTERESTINGX_NUMBER_COLOR = Color.FromArgb(192, 224, 224);  // DarkCyan
    public static Color CARMICHAEL_NUMBER_COLOR = Color.FromArgb(255, 255, 96);     // LightYellow
    public static Color CARMICHAELX_NUMBER_COLOR = Color.FromArgb(255, 255, 208);   // LightLightYellow
    public static Color SQUARE_NUMBER_COLOR = Color.FromArgb(192, 255, 192);        // LightLightGreen
    public static Color CUBIC_NUMBER_COLOR = Color.FromArgb(160, 255, 160);         // LightGreen
    public static Color QUINTIC_NUMBER_COLOR = Color.FromArgb(128, 255, 128);       // DimGreen
    public static Color SEPTIC_NUMBER_COLOR = Color.FromArgb(64, 255, 64);          // Green
    public static Color GetNumberBackColor(long value, int divisor, Color default_color)
    {
        Color color = (Numbers.Compare(value, divisor, ComparisonOperator.DivisibleBy, 0)) ? Numbers.DIVISOR_COLOR : default_color;

        // override
        if ((value >= 7 * 7) && (value.IsSquareNumber())) color = SQUARE_NUMBER_COLOR;                          // value = square number
        else if ((value >= 5 * 5 * 5) && (value.IsCubicNumber())) color = CUBIC_NUMBER_COLOR;                   // value = cubic number
        else if ((value >= 3 * 3 * 3 * 3 * 3) && (value.IsQuinticNumber())) color = QUINTIC_NUMBER_COLOR;       // value = quintic number
        else if ((value >= 2 * 2 * 2 * 2 * 2 * 2 * 2) && (value.IsSepticNumber())) color = SEPTIC_NUMBER_COLOR; // value = septic number

        // override
        if (value.IsInteresting()) color = INTERESTING_NUMBER_COLOR;                            // value = interesting number
        else if (value.IsInterestingD()) color = INTERESTINGD_NUMBER_COLOR;                     // value = one of the divisors is an interesting number
        else if (value.IsInterestingX(INTERESTINGX_COUNT)) color = INTERESTINGX_NUMBER_COLOR;   // value = sum/diff of interesting numbers

        // override
        if (value.IsCarmichael()) color = CARMICHAEL_NUMBER_COLOR;                              // value = Carmichael number
        else if (value.IsCarmichaelX()) color = CARMICHAELX_NUMBER_COLOR;                       // value = Carmichael number +/- index

        return color;
    }
    public static string GetNumberToolTipText(long value)
    {
        string interesting_sums = value.FindInterestingSums(INTERESTINGX_COUNT);
        if (!String.IsNullOrEmpty(interesting_sums))
        {
            interesting_sums = value + " = " + interesting_sums.Replace("\n", "\n" + value + " = ");
            if (interesting_sums.Length > 0)
            {
                int len = ("\n" + value + " = ").Length;
                int index = interesting_sums.Length - len;
                interesting_sums = interesting_sums.Remove(index, len);
            }
        }

        return (
                    Numbers.GetDivisors(value).Count + " = " + "{ " + Numbers.GetDivisorsString(value) + " }" + "\r\n" +
                    (value.IsComposite() ? ((value + " = " + (FactorizeToString(value))) + "\r\n") : "") +
                    interesting_sums
               );
    }

    public static Color[] NUMBER_TYPE_COLORS =
    { 
        /* NumberType.None */                   Color.Black,
        /* NumberType.Unit */                   Color.DarkViolet,
        /* NumberType.Prime */                  Color.Black,
        /* NumberType.AdditivePrime */          Color.Blue,
        /* NumberType.NonAdditivePrime */       Color.Green,
        /* NumberType.Composite */              Color.Black,
        /* NumberType.AdditiveComposite */      Color.FromArgb(240, 32, 32),
        /* NumberType.NonAdditiveComposite */   Color.FromArgb(128, 32, 32),
        /* NumberType.Odd */                    Color.Black,
        /* NumberType.Even */                   Color.Black,
        /* NumberType.Square */                 Color.DarkBlue,
        /* NumberType.Cubic */                  Color.DarkBlue,
        /* NumberType.Quartic */                Color.DarkBlue,
        /* NumberType.Quintic */                Color.DarkBlue,
        /* NumberType.Sextic */                 Color.DarkBlue,
        /* NumberType.Septic */                 Color.DarkBlue,
        /* NumberType.Octic */                  Color.DarkBlue,
        /* NumberType.Nonic */                  Color.DarkBlue,
        /* NumberType.Decic */                  Color.DarkBlue,
        /* NumberType.Natural */                Color.DarkBlue
    };
    public static Color[] NUMBER_TYPE_BACKCOLORS =
    { 
        /* NumberType.None */                   Color.White,
        /* NumberType.Unit */                   Color.FromArgb(255, 248, 255),
        /* NumberType.Prime */                  Color.White,
        /* NumberType.AdditivePrime */          Color.FromArgb(208, 208, 255),
        /* NumberType.NonAdditivePrime */       Color.FromArgb(240, 255, 240),
        /* NumberType.Composite */              Color.White,
        /* NumberType.AdditiveComposite */      Color.FromArgb(255, 224, 224),
        /* NumberType.NonAdditiveComposite */   Color.FromArgb(232, 216, 216),
        /* NumberType.Odd */                    Color.White,
        /* NumberType.Even */                   Color.White,
        /* NumberType.Square */                 Color.White,
        /* NumberType.Cubic */                  Color.White,
        /* NumberType.Quartic */                Color.White,
        /* NumberType.Quintic */                Color.White,
        /* NumberType.Sextic */                 Color.White,
        /* NumberType.Septic */                 Color.White,
        /* NumberType.Octic */                  Color.White,
        /* NumberType.Nonic */                  Color.White,
        /* NumberType.Decic */                  Color.White,
        /* NumberType.Natural */                Color.White
    };
    public static Color GetNumberForeColor(long number)
    {
        if (number < 0L) number *= -1L;
        if (number > 1000000000000L) return Color.Black;

        return GetNumberForeColor(number.ToString(), Numbers.DEFAULT_RADIX);
    }
    public static Color GetNumberForeColor(string text, int radix)
    {
        if (text.Length > 16)
            return Color.Black;

        // if negative number, remove -ve sign
        if (text.StartsWith("-"))
            text = text.Remove(0, 1);

        if (IsUnit(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.Unit];
        }

        else if (IsNonAdditivePrime(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.NonAdditivePrime];
        }
        else if (IsAdditivePrime(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.AdditivePrime];
        }
        else if (IsPrime(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.Prime];
        }

        else if (IsNonAdditiveComposite(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.NonAdditiveComposite];
        }
        else if (IsAdditiveComposite(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.AdditiveComposite];
        }
        else if (IsComposite(text, radix))
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.Composite];
        }

        else
        {
            return NUMBER_TYPE_COLORS[(int)NumberType.None];
        }
    }

    public static Color[] NUMBER_KIND_BACKCOLORS =
    { 
        /* NumberKind.Deficient */          Color.FromArgb(255, 224, 255),
        /* NumberKind.Perfect */            Color.FromArgb(255, 128, 255),
        /* NumberKind.Abundant */           Color.FromArgb(255, 192, 255)
    };

    public static Color[] CALCULATION_MODE_COLORS =
    { 
        /* CalculationMode.SumOfLetterValues */                         Color.FromArgb(255, 255, 255),
        /* CalculationMode.SumOfLetterValueDigitSums */                 Color.FromArgb(255, 192, 192),
        /* CalculationMode.SumOfLetterValueDigitalRoots */              Color.FromArgb(255, 0, 0),
        /* CalculationMode.SumOfWordValueDigitSums */                   Color.FromArgb(192, 192, 255),
        /* CalculationMode.SumOfWordValueDigitalRoots */                Color.FromArgb(0, 0, 255),
        /* CalculationMode.SumOfVerseValueDigitSums */                  Color.FromArgb(192, 255, 192),
        /* CalculationMode.SumOfVerseValueDigitalRoots */               Color.FromArgb(0, 255, 0),
        /* CalculationMode.SumOfUniqueLetterValues */                   Color.FromArgb(255, 255, 0)
    };

    private static int s_max_size = Globals.MAX_NUMBERS;   // for generating big numbers in BigInteger Edition

    static Numbers()
    {
        if (!Directory.Exists(Globals.NUMBERS_FOLDER))
        {
            Directory.CreateDirectory(Globals.NUMBERS_FOLDER);
        }

        if (Globals.EDITION == Edition.BigNumbers)
        {
            GeneratePrimes(s_max_size);
            GenerateAdditivePrimes(s_max_size);
            GenerateNonAdditivePrimes(s_max_size);
            GenerateComposites(s_max_size / 2);
            GenerateAdditiveComposites(s_max_size / 2);
            GenerateNonAdditiveComposites(s_max_size / 2);
            GenerateDeficientNumbers(s_max_size / 16);
            GenerateAbundantNumbers(s_max_size / 16);
        }
        else
        {
            LoadPrimes();
            LoadAdditivePrimes();
            LoadNonAdditivePrimes();
            LoadComposites();
            LoadAdditiveComposites();
            LoadNonAdditiveComposites();
            LoadDeficientNumbers();
            LoadAbundantNumbers();
        }
        LoadPerfectNumbers();

        for (int sides = 3; sides <= MAX_SERIES_LENGTH; sides++)
        {
            GeneratePolygonalNumbers(sides);
            GenerateCenteredPolygonalNumbers(sides);
            GeneratePyramidalNumbers(sides);
        }

        LoadPrimeRepunits();
        LoadInterestingNumbers();
        LoadNumberDimensions();
        LoadDuplicateNumberDimensions();
        LoadUniqueNumberDimensions();
        LoadReptendPrimes();
        LoadPartitionNumbers();
        LoadSmithNumbers();
        LoadCarmichaelNumbers();
        LoadRamanujanPrimes();
    }

    // pi = circumference / diameter ~= 355/113
    public const double PI = 3.141592653589793238462643383279D;
    private static string s_pi_path = "pi.txt";
    private static int[] s_pi_digits = null;
    public static int[] PiDigits
    {
        get
        {
            if (s_pi_digits == null)
            {
                LoadPiDigits();
            }
            return s_pi_digits;
        }
    }
    private static void LoadPiDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_pi_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_pi_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_pi_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }
    // e = Euler's number = 0SUM∞(1/n!)
    private static string s_e_path = "e.txt";
    public const double E = 2.718281828459045235360287471352D;
    private static int[] s_e_digits = null;
    public static int[] EDigits
    {
        get
        {
            if (s_e_digits == null)
            {
                LoadEDigits();
            }
            return s_e_digits;
        }
    }
    private static void LoadEDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_e_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_e_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_e_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }
    // phi = golden ratio = (1+sqrt(5))/2
    private static string s_phi_path = "phi.txt";
    public const double PHI = 1.618033988749894848204586834365D;
    private static int[] s_phi_digits = null;
    public static int[] PhiDigits
    {
        get
        {
            if (s_phi_digits == null)
            {
                LoadPhiDigits();
            }
            return s_phi_digits;
        }
    }
    private static void LoadPhiDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_phi_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_phi_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_phi_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }
    // pi_e
    private static string s_pi_e_path = "pi_e.txt";
    private static int[] s_pi_e_digits = null;
    public static int[] PiEDigits
    {
        get
        {
            if (s_pi_e_digits == null)
            {
                LoadPiEDigits();
            }
            return s_pi_e_digits;
        }
    }
    private static void LoadPiEDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_pi_e_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_pi_e_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_pi_e_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }
    // pi_phi
    private static string s_pi_phi_path = "pi_phi.txt";
    private static int[] s_pi_phi_digits = null;
    public static int[] PiPhiDigits
    {
        get
        {
            if (s_pi_phi_digits == null)
            {
                LoadPiPhiDigits();
            }
            return s_pi_phi_digits;
        }
    }
    private static void LoadPiPhiDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_pi_phi_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_pi_phi_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_pi_phi_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }
    // e_phi
    private static string s_e_phi_path = "e_phi.txt";
    private static int[] s_e_phi_digits = null;
    public static int[] EPhiDigits
    {
        get
        {
            if (s_e_phi_digits == null)
            {
                LoadEPhiDigits();
            }
            return s_e_phi_digits;
        }
    }
    private static void LoadEPhiDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_e_phi_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_e_phi_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_e_phi_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }
    // pi_e_phi
    private static string s_pi_e_phi_path = "pi_e_phi.txt";
    private static int[] s_pi_e_phi_digits = null;
    public static int[] PiEPhiDigits
    {
        get
        {
            if (s_pi_e_phi_digits == null)
            {
                LoadPiEPhiDigits();
            }
            return s_pi_e_phi_digits;
        }
    }
    private static void LoadPiEPhiDigits()
    {
        string path = "Numbers" + Path.DirectorySeparatorChar + s_pi_e_phi_path;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();

                // remove integer value and keep decimal digits only
                int pos = content.IndexOf(".");
                content = content.Remove(0, pos + 1);

                s_pi_e_phi_digits = new int[content.Length];
                for (int i = 0; i < content.Length; i++)
                {
                    s_pi_e_phi_digits[i] = content[i] - 48; // char to int
                }
            }
        }
    }

    public const double HEART = 1.339D; // needs verification


    // interesting numbers // http://quranocode.com/114.txt
    public static List<long> InterestingNumbers = null;
    private static string s_interesting_numbers_path = Globals.INTERESTING_NUMBERS_FILENAME;
    public static void LoadInterestingNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_interesting_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            InterestingNumbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        char[] separators = { '\t' };
                        string[] parts = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            long number;
                            if (long.TryParse(parts[0], out number))
                            {
                                if (!InterestingNumbers.Contains(number))
                                {
                                    InterestingNumbers.Add(number);

                                    // include difference between interesting numbers
                                    InterestingNumbers.Add(-number);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    // source equals an interesting number
    public static bool IsInteresting(this int source)
    {
        return ((long)source).IsInteresting();
    }
    public static bool IsInteresting(this long source)
    {
        if (source < 0L) source *= -1L;

        foreach (long number in InterestingNumbers)
        {
            if ((source == number)) //|| (source.IsMultiple(number)))
            {
                return true;
            }
        }

        return false;
    }
    public static bool IsInteresting(this float source)
    {
        return IsInteresting((double)source, ERROR_MARGIN);
    }
    public static bool IsInteresting(this decimal source)
    {
        return IsInteresting((double)source, ERROR_MARGIN);
    }
    public static bool IsInteresting(this double source)
    {
        return IsInteresting(source, ERROR_MARGIN);
    }
    private static bool IsInteresting(this double source, double tolerance)
    {
        if (source < 0L) source *= -1D;
        return (
                (Math.Abs(source - (Numbers.PI * 2)) <= tolerance) ||
                (Math.Abs(source - Numbers.PI) <= tolerance) ||
                (Math.Abs(source - Numbers.E) <= tolerance) ||
                (Math.Abs(source - Numbers.PHI) <= tolerance) ||
                (Math.Abs(source - (Numbers.PI * Numbers.PI)) <= tolerance) ||
                (Math.Abs(source - ((Numbers.PI * 2) * (Numbers.PI * 2))) <= tolerance) ||
                (Math.Abs(source - (Numbers.E * Numbers.E)) <= tolerance) ||
                (Math.Abs(source - (Numbers.PHI * Numbers.PHI)) <= tolerance) //||
                                                                              //(source.IsMultiple(Numbers.PI)) ||
                                                                              //(source.IsMultiple(Numbers.E)) ||
                                                                              //(source.IsMultiple(Numbers.PHI))
               );
    }
    // source is a multiple of an interesting number
    // i.e. one of the source divisors is an interesting number
    public static bool IsInterestingD(this float source)
    {
        return ((long)source).IsInterestingD();
    }
    public static bool IsInterestingD(this decimal source)
    {
        return ((long)source).IsInterestingD();
    }
    public static bool IsInterestingD(this double source)
    {
        return ((long)source).IsInterestingD();
    }
    public static bool IsInterestingD(this int source)
    {
        return ((long)source).IsInterestingD();
    }
    public static bool IsInterestingD(this long source)
    {
        if (source < 0L) source *= -1L;

        List<long> divisors = GetDivisors(source);
        foreach (long divisor in divisors)
        {
            if (InterestingNumbers.Contains(divisor)) return true;
        }

        return false;
    }
    // source is the sum or diff of interesting numbers
    public static bool IsInterestingX(this float source, int count)
    {
        return ((long)source).IsInterestingX(count);
    }
    public static bool IsInterestingX(this decimal source, int count)
    {
        return ((long)source).IsInterestingX(count);
    }
    public static bool IsInterestingX(this double source, int count)
    {
        return ((long)source).IsInterestingX(count);
    }
    public static bool IsInterestingX(this int source, int count)
    {
        return ((long)source).IsInterestingX(count);
    }
    public static bool IsInterestingX(this long source, int count) // Subset Sum
    {
        if (source < 0L) source *= -1L;

        // the source is the sum or difference between interesting numbers
        if (source < 57L) return false;

        Subsets subsets = new Subsets(InterestingNumbers.ToArray());
        if (subsets != null)
        {
            if (subsets.Count(count, source) > 0L) return true;
        }

        return false;
    }

    private static int MIN_NUMBER_TO_FIND_INTERESTING_NUMBERS_SUM_OR_DIFF = 57;
    private static StringBuilder s_interesting_sums_str = new StringBuilder();
    public static string FindInterestingSums(this int source)
    {
        if (source < 0) source *= -1;

        // the source is the sum or difference between interesting numbers
        if (source < MIN_NUMBER_TO_FIND_INTERESTING_NUMBERS_SUM_OR_DIFF) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            subsets.Find((long)source, OnFoundInterestingSum);
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this long source)
    {
        if (source < 0L) source *= -1L;
        if (source < 57L) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            subsets.Find(source, OnFoundInterestingSum);
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this float source)
    {
        if (source < 0F) source *= -1F;
        if (source < 57F) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            subsets.Find((long)source, OnFoundInterestingSum);
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this double source)
    {
        if (source < 0D) source *= -1D;
        if (source < 57D) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            subsets.Find((long)source, OnFoundInterestingSum);
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this decimal source)
    {
        if (source < 0m) source *= -1m;
        if (source < 57m) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            subsets.Find((long)source, OnFoundInterestingSum);
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this int source, int count)
    {
        if (source < 0) source *= -1;

        // the source is the sum or difference between interesting numbers
        if (source < MIN_NUMBER_TO_FIND_INTERESTING_NUMBERS_SUM_OR_DIFF) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            if (count == 0) // find all sums
            {
                subsets.Find((long)source, OnFoundInterestingSum);
            }
            else // find sums up to count
            {
                for (int i = 2; i <= count; i++)
                {
                    subsets.Find(i, (long)source, OnFoundInterestingSum);
                }
            }
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this long source, int count)
    {
        if (source < 0L) source *= -1L;
        if (source < 57L) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            if (count == 0) // find all sums
            {
                subsets.Find((long)source, OnFoundInterestingSum);
            }
            else // find sums up to count
            {
                for (int i = 2; i <= count; i++)
                {
                    subsets.Find(i, (long)source, OnFoundInterestingSum);
                }
            }
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this float source, int count)
    {
        if (source < 0F) source *= -1F;
        if (source < 57F) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            if (count == 0) // find all sums
            {
                subsets.Find((long)source, OnFoundInterestingSum);
            }
            else // find sums up to count
            {
                for (int i = 2; i <= count; i++)
                {
                    subsets.Find(i, (long)source, OnFoundInterestingSum);
                }
            }
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this double source, int count)
    {
        if (source < 0D) source *= -1D;
        if (source < 57D) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            if (count == 0) // find all sums
            {
                subsets.Find((long)source, OnFoundInterestingSum);
            }
            else // find sums up to count
            {
                for (int i = 2; i <= count; i++)
                {
                    subsets.Find(i, (long)source, OnFoundInterestingSum);
                }
            }
        }

        return s_interesting_sums_str.ToString();
    }
    public static string FindInterestingSums(this decimal source, int count)
    {
        if (source < 0m) source *= -1m;
        if (source < 57m) return null;

        s_interesting_lines.Clear();
        s_interesting_sums_str.Length = 0;

        long[] numbers = InterestingNumbers.ToArray();
        Subsets subsets = new Subsets(numbers);
        if (subsets != null)
        {
            if (count == 0) // find all sums
            {
                subsets.Find((long)source, OnFoundInterestingSum);
            }
            else // find sums up to count
            {
                for (int i = 2; i <= count; i++)
                {
                    subsets.Find(i, (long)source, OnFoundInterestingSum);
                }
            }
        }

        return s_interesting_sums_str.ToString();
    }
    private static bool ContainsItem(List<Subsets.Item> items, long value)
    {
        foreach (Subsets.Item item in items)
        {
            if (item.Value == value)
            {
                return true;
            }
        }
        return false;
    }
    private static Subsets.Item GetItem(List<Subsets.Item> items, long value)
    {
        foreach (Subsets.Item item in items)
        {
            if (item.Value == value)
            {
                return item;
            }
        }
        return null;
    }
    private static List<string> s_interesting_lines = new List<string>();
    private static void OnFoundInterestingSum(Subsets.Item[] subset)
    {
        // remove any +N and -N interesting numbers if both exist in subset
        List<Subsets.Item> filtered = new List<Subsets.Item>(subset);
        foreach (Subsets.Item item in subset)
        {
            if (ContainsItem(filtered, -item.Value))
            {
                filtered.Remove(GetItem(filtered, -item.Value));
                filtered.Remove(item);
            }
        }

        if (filtered.Count > 1)
        {
            string xxx = "";
            foreach (Subsets.Item item in filtered)
            {
                xxx += item.Value + " + ";
            }
            xxx = xxx.Replace(" + -", " - ");
            xxx = xxx.Remove(xxx.Length - 3, 3);

            if (!s_interesting_lines.Contains(xxx))
            {
                s_interesting_lines.Add(xxx);
                s_interesting_sums_str.AppendLine(xxx);
            }
        }
    }

    public static bool IsCarmichael(this int source)
    {
        return IsCarmichael((long)source);
    }
    public static bool IsCarmichael(this long source)
    {
        return IsCarmichaelNumber(source);
    }
    public static bool IsCarmichael(this float source)
    {
        return IsCarmichael((double)source, ERROR_MARGIN);
    }
    public static bool IsCarmichael(this decimal source)
    {
        return IsCarmichael((double)source, ERROR_MARGIN);
    }
    public static bool IsCarmichael(this double source)
    {
        return IsCarmichael(source, ERROR_MARGIN);
    }
    private static bool IsCarmichael(this double source, double tolerance)
    {
        return (
                ((long)Math.Floor(source + 0.5D - tolerance)).IsCarmichael()
                ||
                ((long)Math.Floor(source + 0.5D + tolerance)).IsCarmichael()
               );
    }
    public static bool IsCarmichaelX(this int source)
    {
        return IsCarmichaelX((long)source);
    }
    public static bool IsCarmichaelX(this long source)
    {
        if (source < 0L)
            source *= -1L;
        for (int i = 0; i < CarmichaelNumbers.Count; i++)
        {
            if (
                //(source.IsMultiple(CarmichaelNumbers[i])) ||
                (source == (CarmichaelNumbers[i] + (i + 1))) ||
                (source == (CarmichaelNumbers[i] - (i + 1))) //||
                                                             //(source == (CarmichaelNumbers[i] * (i + 1))) ||
                                                             //(Math.Abs(source - ((double)CarmichaelNumbers[i] / (i + 1))) <= ERROR_MARGIN)
               )
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsCarmichaelX(this float source)
    {
        return IsCarmichaelX((double)source, ERROR_MARGIN);
    }
    public static bool IsCarmichaelX(this decimal source)
    {
        return IsCarmichaelX((double)source, ERROR_MARGIN);
    }
    public static bool IsCarmichaelX(this double source)
    {
        return IsCarmichaelX(source, ERROR_MARGIN);
    }
    private static bool IsCarmichaelX(this double source, double tolerance)
    {
        return (
                ((long)Math.Floor(source + 0.5D - tolerance)).IsCarmichaelX()
                ||
                ((long)Math.Floor(source + 0.5D + tolerance)).IsCarmichaelX()
               );
    }

    public static bool IsMultiple(this int source, int target)
    {
        if (source == 0)
            return false;
        if (target == 0)
            return false;
        return ((source % target) == 0);
    }
    public static bool IsMultiple(this long source, long target)
    {
        if (source == 0)
            return false;
        if (target == 0)
            return false;
        return ((source % target) == 0);
    }
    public static bool IsMultiple(this float source, float target)
    {
        return source.IsMultiple(target, (float)ERROR_MARGIN);
    }
    public static bool IsMultiple(this decimal source, decimal target)
    {
        return source.IsMultiple(target, (decimal)ERROR_MARGIN);
    }
    public static bool IsMultiple(this double source, double target)
    {
        return source.IsMultiple(target, ERROR_MARGIN);
    }
    public static bool IsMultiple(this float source, float target, float tolerance)
    {
        if (source == 0)
            return false;
        if (target == 0)
            return false;
        return ((Math.Abs(source) > tolerance) && (Math.Abs((source % target)) < tolerance));
    }
    public static bool IsMultiple(this decimal source, decimal target, decimal tolerance)
    {
        if (source == 0)
            return false;
        if (target == 0)
            return false;
        return ((Math.Abs(source) > tolerance) && (Math.Abs((source % target)) < tolerance));
    }
    public static bool IsMultiple(this double source, double target, double tolerance)
    {
        if (source == 0)
            return false;
        if (target == 0)
            return false;
        return ((Math.Abs(source) > tolerance) && (Math.Abs((source % target)) < tolerance));
    }

    public static NumberType GetNumberType(long number)
    {
        if (number < 0L) number *= -1L;
        if (number == 0L) return NumberType.None;

        if (IsUnit(number))
            return NumberType.Unit;
        else if (IsAdditivePrime(number))
            return NumberType.AdditivePrime;
        else if (IsNonAdditivePrime(number))
            return NumberType.NonAdditivePrime;
        if (IsAdditiveComposite(number))
            return NumberType.AdditiveComposite;
        else if (IsNonAdditiveComposite(number))
            return NumberType.NonAdditiveComposite;
        else
            return NumberType.None;
    }
    public static string GetNumberTypeAsString(long number)
    {
        if (number < 0L) number *= -1L;
        if (number == 0L) return "";

        if (IsUnit(number))
            return "U";
        else if (IsAdditivePrime(number))
            return "AP";
        else if (IsNonAdditivePrime(number))
            return "XP";
        if (IsAdditiveComposite(number))
            return "AC";
        else if (IsNonAdditiveComposite(number))
            return "XC";
        else
            return "";
    }
    public static bool IsNumberType(long number, NumberType number_type)
    {
        if (number < 0L) number *= -1L;
        if (number == 0L) return false;

        switch (number_type)
        {
            case NumberType.Natural:
                {
                    return true;
                }
            case NumberType.Prime:
                {
                    return (IsPrime(number));
                }
            case NumberType.AdditivePrime:
                {
                    return (IsAdditivePrime(number));
                }
            case NumberType.NonAdditivePrime:
                {
                    return (IsNonAdditivePrime(number));
                }
            case NumberType.Composite:
                {
                    return (IsComposite(number));
                }
            case NumberType.AdditiveComposite:
                {
                    return (IsAdditiveComposite(number));
                }
            case NumberType.NonAdditiveComposite:
                {
                    return (IsNonAdditiveComposite(number));
                }
            case NumberType.Odd:
                {
                    return (IsOdd(number));
                }
            case NumberType.Even:
                {
                    return (IsEven(number));
                }
            case NumberType.Fibonacci:
                {
                    return (IsFibonacciNumber(number));
                }
            case NumberType.Square:
                {
                    return (IsSquareNumber(number));
                }
            case NumberType.Cubic:
                {
                    return (IsCubicNumber(number));
                }
            case NumberType.Quartic:
                {
                    return (IsQuarticNumber(number));
                }
            case NumberType.Quintic:
                {
                    return (IsQuinticNumber(number));
                }
            case NumberType.Sextic:
                {
                    return (IsSexticNumber(number));
                }
            case NumberType.Septic:
                {
                    return (IsSepticNumber(number));
                }
            case NumberType.Octic:
                {
                    return (IsOcticNumber(number));
                }
            case NumberType.Nonic:
                {
                    return (IsNonicNumber(number));
                }
            case NumberType.Decic:
                {
                    return (IsDecicNumber(number));
                }
            case NumberType.None:
            default:
                {
                    return false;
                }
        }
    }
    /// <summary>
    /// Compare two numbers
    /// </summary>
    /// <param name="number1">first number</param>
    /// <param name="number2">second number</param>
    /// <param name="comparison_operator">operator for comparing the two numbers</param>
    /// <param name="remainder">remainder for the % operator. -1 means any remainder</param>
    /// <returns>returns comparison result</returns>
    public static bool Compare(long number1, long number2, ComparisonOperator comparison_operator, int remainder)
    {
        switch (comparison_operator)
        {
            case ComparisonOperator.Equal:
                {
                    return (number1 == number2);
                }
            case ComparisonOperator.NotEqual:
                {
                    return (number1 != number2);
                }
            case ComparisonOperator.LessThan:
                {
                    return (number1 < number2);
                }
            case ComparisonOperator.LessOrEqual:
                {
                    return (number1 <= number2);
                }
            case ComparisonOperator.GreaterThan:
                {
                    return (number1 > number2);
                }
            case ComparisonOperator.GreaterOrEqual:
                {
                    return (number1 >= number2);
                }
            case ComparisonOperator.DivisibleBy:
                {
                    if (number2 == 0)
                        return false;
                    if (remainder == -1) // means any remainder
                    {
                        return ((number1 % number2) != 0);
                    }
                    else
                    {
                        // ignore 0
                        return ((number1 != 0) && (Math.Abs((number1 % number2)) == remainder));
                    }
                }
            case ComparisonOperator.IndivisibleBy:
                {
                    // ignore 0
                    if (number2 == 0)
                        return false;
                    return ((number1 != 0) && (Math.Abs((number1 % number2)) != 0));
                }
            case ComparisonOperator.EqualSum:
                {
                    return (number1 == number2); //??? pass sum in number2
                }
            case ComparisonOperator.Reserved:
            default:
                {
                    return false;
                }
        }
    }
    public static int Reverse(int number)
    {
        int result = 0;
        while (number > 0)
        {
            result = (result * 10) + (number % 10);
            number /= 10;
        }
        return result;
    }
    public static long Reverse(long number)
    {
        long result = 0L;
        while (number > 0L)
        {
            result = (result * 10L) + (number % 10L);
            number /= 10L;
        }
        return result;
    }
    public static long Concatenate(long number1, long number2, bool left_to_right)
    {
        long result;

        string combination = "";
        string AAA = number1.ToString();
        string BBB = number2.ToString();
        if (left_to_right)
        {
            combination = AAA + BBB;
        }
        else // right_to_left
        {
            combination = BBB + AAA;
        }

        if (long.TryParse(combination, out result))
        {
            return result;
        }
        return -1L;
    }
    public static long Interlace(long number1, long number2, bool a_then_b, bool left_to_right)
    {
        long result;

        if (left_to_right)
        {
            number1 = Reverse(number1);
            number2 = Reverse(number2);
        }
        else // right_to_left
        {
            //number1 = number1;
            //number2 = number2;
        }

        if ((number1 != -1L) && (number2 != -1L))
        {
            string combination = "";
            string AAA = number1.ToString();
            string BBB = number2.ToString();
            if (!a_then_b)
            {
                string temp = AAA;
                AAA = BBB;
                BBB = temp;
            }

            int a = AAA.Length;
            int b = BBB.Length;
            int min = Math.Min(a, b);

            for (int d = 0; d < min; d++)
            {
                combination += AAA[d].ToString() + BBB[d].ToString();
            }
            if (a > min)
            {
                combination += AAA.Substring(min);
            }
            else
            {
                combination += BBB.Substring(min);
            }

            if (long.TryParse(combination, out result))
            {
                return result;
            }
            return -1L;
        }
        return -1L;
    }
    public static long CrossOver(long number1, long number2, bool a_then_b, bool left_to_right)
    {
        long result;

        string combination = "";
        string AAA = number1.ToString();
        string BBB = number2.ToString();
        if (!a_then_b)
        {
            string temp = AAA;
            AAA = BBB;
            BBB = temp;
        }

        int a = AAA.Length;
        int b = BBB.Length;
        if ((a > 1) && (b > 1))
        {
            int mid_a = a / 2;
            string AAAHalf1 = AAA.Substring(0, mid_a - 1);
            string AAAHalf2 = AAA.Substring(mid_a - 1);

            int mid_b = b / 2;
            string BBBHalf1 = AAA.Substring(0, mid_b - 1);
            string BBBHalf2 = AAA.Substring(mid_b - 1);

            if (left_to_right)
            {
                combination = AAAHalf1 + BBBHalf2 + AAAHalf2 + BBBHalf1;
            }
            else // right_to_left
            {
                combination = BBBHalf1 + AAAHalf2 + BBBHalf2 + AAAHalf1;
            }

            if (long.TryParse(combination, out result))
            {
                return result;
            }
            return -1L;
        }
        return -1L;
    }
    public static bool AreReverse(long number1, long number2)
    {
        return (number1 == Reverse(number2));
    }
    public static bool AreConsecutive(List<int> numbers)
    {
        if (numbers != null)
        {
            for (int i = 0; i < numbers.Count - 1; i++)
            {
                if (numbers[i + 1] != numbers[i] + 1)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    public static long SumOfNumbers(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfNumbers(number);
        }
        return 0L;
    }
    public static long SumOfNumbers(long number)
    {
        if (number < 0L) number *= -1L;

        long result = (number * (number + 1L)) / 2L;

        if (result < 0L) return -1L;
        else return result;
    }
    public static string GetNumbersString(long number)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append(i.ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    public static long SumOfSquareNumbers(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfSquareNumbers(number);
        }
        return 0L;
    }
    public static long SumOfSquareNumbers(long number)
    {
        if (number < 0L) number *= -1L;

        long result = 0L;
        for (int i = 1; i <= number; i++)
        {
            result += i * i;
        }

        if (result < 0L) return -1L;
        else return result;
    }
    public static string SumOfSquareNumbersString(long number)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((i * i).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    public static long SumOfCubicNumbers(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfCubicNumbers(number);
        }
        return 0L;
    }
    public static long SumOfCubicNumbers(long number)
    {
        if (number < 0L) number *= -1L;

        long result = 0L;
        for (int i = 1; i <= number; i++)
        {
            result += i * i * i;
        }

        if (result < 0L) return -1L;
        else return result;
    }
    public static string SumOfCubicNumbersString(long number)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((i * i * i).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }

    public static bool IsUnit(long number)
    {
        if (number < 0L) number *= -1L;

        return (number == 1L);
    }
    public static bool IsUnit(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsUnit(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsOdd(long number)
    {
        if (number < 0L) number *= -1L;

        return ((number % 2) == 1L);
    }
    public static bool IsOdd(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsOdd(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsEven(long number)
    {
        if (number < 0L) number *= -1L;

        return ((number % 2) == 0L);
    }
    public static bool IsEven(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsEven(number);
        }
        catch
        {
            return false;
        }
    }
    // https://digitalbush.com/2010/02/26/sieve-of-eratosthenes-in-csharp/

    //IList<int> FindPrimes(int max)
    //{
    //    var result = new List<int>((int)(max / (Math.Log(max) - 1.08366)));
    //    var maxSquareRoot = Math.Sqrt(max);
    //    var eliminated = new System.Collections.BitArray(max + 1);
    //    result.Add(2);
    //    for (int i = 3; i <= max; i += 2)
    //    {
    //        if (!eliminated[i])
    //        {
    //            if (i < maxSquareRoot)
    //            {
    //                for (int j = i * i; j <= max; j += 2 * i)
    //                    eliminated[j] = true;
    //            }
    //            result.Add(i);
    //        }
    //    }
    //    return result;
    //}

    // Algorithm Optimizations
    // I cut my work in half by treating the special case of '2'.
    // We know that 2 is prime and all even numbers thereafter are not.
    // So, we'll add two immediately and then start looping at 3 only checking odd numbers from there forward.

    // After we've found a prime, we only need to eliminate numbers from it's square and forward.
    // Let's say we want to find all prime numbers up to 100 and we've just identified 7 as a prime.
    // Per the algorithm, I'll need to eliminate 2*7, 3*7 ,4*7, 5*7, 6*7, 7*7 ,8*7 ,9*7, 10*7 ,11*7, 12*7 ,13*7 and 14*7.
    // None of the even multiples matter (even times an odd is always even) and none of the multiples
    // up to the square of the prime matter since we've already done those multiples in previous loops.
    // So really we only have to eliminate 7*7, 9*7, 11*7 and 13*7.
    // That's a 9 fewer iterations and those savings become more fruitful the deeper you go!

    // The last optimization is the square root calculation and check.
    // We know from above that we only need to start eliminating beginning at the square of the current prime.
    // Therefore it also makes sense that we can stop even trying once we get past the to square root of the max.
    // This saves a bunch more iterations.

    // Language Optimizations
    // Originally I had started by returning an IEnumerable<int>.
    // I wasn't using the list you see above and instead I was using yield return i.
    // I really like that syntax, but once I got to the VB.net version (Coming Soon!),
    // I didn't have a direct translation for the yield keyword.
    // I took the lazy route in the VB version and just stuffed it all into a list and returned that.
    // To my surprise it was faster! I went back and changed the C# version above and it performed better.
    // I'm not sure why, but I'm going with it.

    // What do you think that you get when do a sizeof(bool) in C#?
    // I was surprised to find out that my trusty booleans actually take up a whole byte instead of a single bit.
    // I speculate that there is a performance benefit that all of your types fit into a byte level offset in memory.
    // I was thrilled to find out that we have a BitArray class that is useful for situations above
    // when you need to store a lot of booleans and you need them to only take up a bit in memory.
    // I'm not sure it helped anything, but I feel better knowing I'm using the least amount of memory possible.

    // Conclusion
    // Despite the fact that I know C# really well, I'm very thrilled that I was able to learn a few things about the language.
    // Also, I'm really happy with the performance of this reference implementation.
    // On my machine (2.66 GHz Core2 Duo and 2 GB of RAM) I can find all of the primes under 1,000,000 in 19ms.
    // I think I've squeezed all I can out of this version.
    // Please let me know if you see something I missed or did wrong and I'll make adjustments.

    // EDIT: I just added one more optimization that's worth noting.
    // Instead of constructing my list with an empty constructor, I can save a several milliseconds 
    // off the larger sets by specifying a start m_size of the internal m_candidates structure behind the list.
    // If I set this m_size at or slightly above the end count of prime numbers,
    // then I avoid a lot of costly m_candidates copying as the m_candidates bounds keep getting hit.
    // It turns out that there is quite a bit of math involved in accurately predicting the number of primes underneath a given number.
    // I chose to cheat and just use Legendre's constant with the Prime Number Theorem which is close enough for my purposes.
    // I can now calculate all primes under 1,000,000 in 10ms on my machine. Neat!
    //private static List<int> GeneratePrimesUsingSieveOfEratosthenes(int limit)
    //{
    //    // guard against parameter out of range
    //    if (limit < 2)
    //    {
    //        return new List<int>();
    //    }

    //    // Legendre's constant to approximate the number of primes below N
    //    int max_primes = (int)Math.Ceiling((limit / (Math.Log(limit) - 1.08366)));
    //    if (max_primes < 1)
    //    {
    //        max_primes = 1;
    //    }
    //    List<int> primes = new List<int>(max_primes);

    //    // bit m_candidates to cross out multiples of primes successively
    //    BitArray candidates = new BitArray(limit + 1, true);

    //    // add number 2 as prime
    //    primes.Add(2);
    //    // and cross out all its multiples
    //    for (int j = 2 * 2; j <= limit; j += 2)
    //    {
    //        candidates[j] = false;
    //    }

    //    // get the ceiling of sqrt of N
    //    int limit_sqrt = (int)Math.Ceiling(Math.Sqrt(limit));

    //    // start from 3 and skip even numbers
    //    // don't go beyond limit or overflow into negative
    //    for (int i = 3; (i > 0 && i <= limit); i += 2)
    //    {
    //        if (candidates[i])
    //        {
    //            // add not-crossed out candidate
    //            primes.Add(i);

    //            // up to the sqrt of N
    //            if (i <= limit_sqrt)
    //            {
    //                // and cross out non-even multiples from i*i and skip even i multiples
    //                // don't go beyond limit, or overflow into negative
    //                for (int j = i * i; (j > 0 && j <= limit); j += 2 * i)
    //                {
    //                    candidates[j] = false;
    //                }
    //            }
    //        }
    //    }

    //    return primes;
    //}
    //private static List<int> GeneratePrimesUsingDivisionTrial(int limit)
    //{
    //    // guard against parameter out of range
    //    if (limit < 2)
    //    {
    //        return new List<int>();
    //    }

    //    // Legendre's constant to approximate the number of primes below N
    //    int max_primes = (int)Math.Ceiling((limit / (Math.Log(limit) - 1.08366)));
    //    if (max_primes < 1)
    //    {
    //        max_primes = 1;
    //    }
    //    List<int> primes = new List<int>(max_primes);

    //    primes.Add(2);

    //    for (int i = 3; i <= limit; i += 2)
    //    {
    //        bool is_prime = true;
    //        for (int j = 3; j <= (int)Math.Sqrt(i); j += 2)
    //        {
    //            if (i % j == 0)
    //            {
    //                is_prime = false;
    //                break;
    //            }
    //        }

    //        if (is_prime)
    //        {
    //            primes.Add(i);
    //        }
    //    }

    //    return primes;
    //}
    public static bool IsPrime(long number)
    {
        if (number < 0L) number *= -1L;

        if (number == 0L)        // 0 is neither prime nor composite
            return false;

        if (number == 1L)        // 1 is the unit, indivisible
            return false;        // NOT prime

        if (number == 2L)        // 2 is the first prime
            return true;

        if (number % 2L == 0L)   // exclude even numbers to speed up search
            return false;

        long sqrt = (long)Math.Round(Math.Sqrt(number));
        for (long i = 3L; i <= sqrt; i += 2L)
        {
            if ((number % i) == 0L)
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsPrime(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsPrime(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsAdditivePrime(long number)
    {
        if (IsPrime(number))
        {
            return IsPrime(DigitSum(number));
        }
        return false;
    }
    public static bool IsAdditivePrime(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsAdditivePrime(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsNonAdditivePrime(long number)
    {
        if (IsPrime(number))
        {
            return !IsPrime(DigitSum(number));
        }
        return false;
    }
    public static bool IsNonAdditivePrime(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsNonAdditivePrime(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsComposite(long number)
    {
        if (number < 0L) number *= -1L;

        if (number == 0L)        // 0 is NOT composite
            return false;

        if (number == 1L)        // 1 is the unit, indivisible
            return false;        // NOT composite

        if (number == 2L)        // 2 is the first prime
            return false;

        if (number % 2L == 0L)   // even numbers are composite
            return true;

        long sqrt = (long)Math.Round(Math.Sqrt(number));
        for (long i = 3L; i <= sqrt; i += 2L)
        {
            if ((number % i) == 0L)
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsComposite(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsComposite(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsAdditiveComposite(long number)
    {
        if (IsComposite(number))
        {
            return IsComposite(DigitSum(number));
        }
        return false;
    }
    public static bool IsAdditiveComposite(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsAdditiveComposite(number);
        }
        catch
        {
            return false;
        }
    }
    public static bool IsNonAdditiveComposite(long number)
    {
        if (IsComposite(number))
        {
            return !IsComposite(DigitSum(number));
        }
        return false;
    }
    public static bool IsNonAdditiveComposite(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return IsNonAdditiveComposite(number);
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Check if three numbers are additive primes and their L2R and R2L concatenations are additive primes too.
    /// <para>Example:</para>
    /// <para>Quran chapter The Key has:</para>
    /// <para>(7, 29, 139) are primes with primes digit sums (7=7, 2+9=11, 1+3+9=13)</para>
    /// <para>and 729139, 139297 primes with prime digit sum (1+3+9+2+9+7=31)</para>
    /// </summary>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <param name="n3"></param>
    /// <returns></returns>
    public static bool ArePrimeTriplets(string text1, string text2, string text3, int radix)
    {
        try
        {
            long number1 = Radix.Decode(text1, radix);
            long number2 = Radix.Decode(text2, radix);
            long number3 = Radix.Decode(text3, radix);
            return ArePrimeTriplets(number1, number2, number3);
        }
        catch
        {
            return false;
        }
    }
    public static bool ArePrimeTriplets(long number1, long number2, long number3)
    {
        if (IsAdditivePrime(number1) && IsAdditivePrime(number2) && IsAdditivePrime(number3))
        {
            long l2r;
            long r2l;
            if (long.TryParse(number1.ToString() + number2.ToString() + number3.ToString(), out l2r))
            {
                if (long.TryParse(number3.ToString() + number2.ToString() + number1.ToString(), out r2l))
                {
                    return (IsAdditivePrime(l2r) && IsAdditivePrime(r2l));
                }
            }
        }
        return false;
    }
    public static List<int> SieveOfEratosthenes(int limit)
    {
        // guard against parameter out of range
        if ((limit < 2) || (limit > (int)(int.MaxValue * 0.9999999)))
        {
            return new List<int>();
        }

        // Legendre's constant to approximate the number of primes below N
        int max_primes = (int)Math.Ceiling((limit / (Math.Log(limit) - 1.08366)));
        if (max_primes < 1)
        {
            max_primes = 1;
        }
        List<int> primes = new List<int>(max_primes);

        // bit m_candidates to cross out multiples of primes successively
        // from N^2, jumping 2N at a time (to skip even multiples)
        BitArray candidates = new BitArray(limit + 1, true);

        // add number 2 as prime
        primes.Add(2);
        //// no need to cross out evens as we are skipping them anyway
        //// and cross out all its multiples
        //for (int j = 2 * 2; j <= limit; j += 2)
        //{
        //    candidates[j] = false;
        //}

        // get the ceiling of sqrt of N
        int sqrt_of_limit = (int)Math.Ceiling(Math.Sqrt(limit));

        // start from 3 and skip even numbers
        // don't go beyond limit or overflow into negative
        for (int i = 3; (i > 0 && i <= limit); i += 2)
        {
            // if not-crossed out candidate yet
            if (candidates[i])
            {
                // add candidate
                primes.Add(i);

                // up to the sqrt of N
                if (i <= sqrt_of_limit)
                {
                    // and cross out non-even multiples from i*i and skip even i multiples
                    // don't go beyond limit, or overflow into negative
                    for (int j = i * i; (j > 0 && j <= limit); j += 2 * i)
                    {
                        candidates[j] = false;
                    }
                }
            }
        }
        return primes;
    }

    private static List<long> p_squares = null;
    private static List<long> p_ap_squares = null;
    private static List<long> p_xp_squares = null;
    public static List<long> P_Squares
    {
        get
        {
            if (p_squares == null)
            {
                p_squares = new List<long>();
                foreach (long n in Primes)
                {
                    int p = PrimeIndexOf(n) + 1;
                    if ((p > 0) && IsSquareNumber(p))
                    {
                        p_squares.Add(n);
                    }
                }
            }
            return p_squares;
        }
    }
    public static List<long> P_AP_Squares
    {
        get
        {
            if (p_ap_squares == null)
            {
                p_ap_squares = new List<long>();
                foreach (long n in Primes)
                {
                    int p = PrimeIndexOf(n) + 1;
                    if ((p > 0) && IsSquareNumber(p))
                    {
                        int ap = AdditivePrimeIndexOf(n) + 1;
                        if ((ap > 0) && IsSquareNumber(ap))
                        {
                            p_ap_squares.Add(n);
                        }
                    }
                }
            }
            return p_ap_squares;
        }
    }
    public static List<long> P_XP_Squares
    {
        get
        {
            if (p_xp_squares == null)
            {
                p_xp_squares = new List<long>();
                foreach (long n in Primes)
                {
                    int p = PrimeIndexOf(n) + 1;
                    if ((p > 0) && IsSquareNumber(p))
                    {
                        int xp = NonAdditivePrimeIndexOf(n) + 1;
                        if ((xp > 0) && IsSquareNumber(xp))
                        {
                            p_xp_squares.Add(n);
                        }
                    }
                }
            }
            return p_xp_squares;
        }
    }
    private static List<long> c_squares = null;
    private static List<long> c_ac_squares = null;
    private static List<long> c_xc_squares = null;
    public static List<long> C_Squares
    {
        get
        {
            if (c_squares == null)
            {
                c_squares = new List<long>();
                foreach (long n in Composites)
                {
                    int c = CompositeIndexOf(n) + 1;
                    if ((c > 0) && IsSquareNumber(c))
                    {
                        c_squares.Add(n);
                    }
                }
            }
            return c_squares;
        }
    }
    public static List<long> C_AC_Squares
    {
        get
        {
            if (c_ac_squares == null)
            {
                c_ac_squares = new List<long>();
                foreach (long n in Composites)
                {
                    int c = CompositeIndexOf(n) + 1;
                    if ((c > 0) && IsSquareNumber(c))
                    {
                        int ac = AdditiveCompositeIndexOf(n) + 1;
                        if ((ac > 0) && IsSquareNumber(ac))
                        {
                            c_ac_squares.Add(n);
                        }
                    }
                }
            }
            return c_ac_squares;
        }
    }
    public static List<long> C_XC_Squares
    {
        get
        {
            if (c_xc_squares == null)
            {
                c_xc_squares = new List<long>();
                foreach (long n in Composites)
                {
                    int c = CompositeIndexOf(n) + 1;
                    if ((c > 0) && IsSquareNumber(c))
                    {
                        int xc = NonAdditiveCompositeIndexOf(n) + 1;
                        if ((xc > 0) && IsSquareNumber(xc))
                        {
                            c_xc_squares.Add(n);
                        }
                    }
                }
            }
            return c_xc_squares;
        }
    }

    public static int GetDigitValue(char c)
    {
        int result = -1;
        if (Char.IsDigit(c)) // 0..9
        {
            result = (int)char.GetNumericValue(c);
        }
        else // A..Z
        {
            result = c.CompareTo('A') + 10;
        }
        return result;
    }
    public static List<int> GetDigits(long number)
    {
        if (number < 0L) number *= -1L;

        List<int> result = new List<int>();
        string str = number.ToString();
        for (int i = 0; i < str.Length; i++)
        {
            result.Add((int)Char.GetNumericValue(str[i]));
        }
        return result;
    }
    public static List<char> GetDigits(string text)
    {
        List<char> result = new List<char>();
        if (!String.IsNullOrEmpty(text))
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (Char.IsDigit(c))
                {
                    result.Add(text[i]);
                }
            }
        }
        return result;
    }
    public static int DigitCount(long number)
    {
        if (number < 0L) number *= -1L;

        return DigitCount(number.ToString());
    }
    public static int DigitCount(string text)
    {
        return DigitCount(text, Numbers.DEFAULT_RADIX);
        //int result = 0;
        //if (!String.IsNullOrEmpty(text))
        //{
        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        char c = text[i];
        //        if (Char.IsDigit(c))
        //        {
        //            result++;
        //        }
        //    }
        //}
        //return result;
    }
    public static int DigitCount(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        return DigitCount(number.ToString(), radix);
    }
    public static int DigitCount(string text, int radix)
    {
        int result = 0;
        if (!String.IsNullOrEmpty(text))
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (((c >= ('0')) && (c < ('9' - 9 + radix))) || ((c >= ('A')) && (c < ('A' - 10 + radix))))
                {
                    result++;
                }
            }
        }
        return result;
    }
    public static int DigitSum(long number)
    {
        if (number < 0L) number *= -1L;

        return DigitSum(number.ToString());
    }
    public static int DigitSum(string text)
    {
        int result = 0;
        if (!String.IsNullOrEmpty(text))
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (Char.IsDigit(c))
                {
                    result += GetDigitValue(c);
                }
            }
        }
        return result;
    }
    public static int DigitSum(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        return DigitSum(number.ToString(), radix);
    }
    public static int DigitSum(string text, int radix)
    {
        int result = 0;
        try
        {
            if (!String.IsNullOrEmpty(text))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    result += (int)Radix.Decode(text[i].ToString(), radix);
                }
            }
        }
        catch
        {
            // do nothing
        }
        return result;
    }

    public static long SumOfNumberDigitSums(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfNumberDigitSums(number, DEFAULT_RADIX);
    }
    public static long SumOfNumberDigitSums(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfNumberDigitSums(number, DEFAULT_RADIX);
        }
        return 0L;
    }
    public static long SumOfNumberDigitSums(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        //????? method works correctly for radix = 10 only
        // https://ideone.com/ik8iE6

        long result = 0L;
        long pos = 1L;
        long previous = 0L;
        long count = 0L;

        while (number > 0)
        {
            long r = number % radix;
            number /= radix;

            result += (r * (r - 1L) / 2L) * pos + r * (((radix - 1L) * (radix / 2L)) * count * pos / radix) + r * (previous + 1L);
            previous += pos * r;
            count++;

            pos *= radix;
        }
        return result;
    }
    public static long SumOfNumberDigitSums(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfNumberDigitSums(number, radix);
        }
        return 0L;
    }
    public static string SumOfDigitSumsString(long number, int radix)
    {
        if (number < 0L) number *= -1L;
        if (number > 1000000000000L) return "";

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((DigitSum(i, radix)).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    public static string GetNumberDigitSumsString(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfDigitSumsString(number, DEFAULT_RADIX);
    }
    public static long SumOfSquareDigitSums(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfSquareDigitSums(number, DEFAULT_RADIX);
    }
    public static long SumOfSquareDigitSums(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfSquareDigitSums(number, DEFAULT_RADIX);
        }
        return 0L;
    }
    public static string SumOfSquareDigitSumsString(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfSquareDigitSumsString(number, DEFAULT_RADIX);
    }
    public static long SumOfSquareDigitSums(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        long result = 0L;
        for (int i = 1; i <= number; i++)
        {
            result += (DigitSum(i, radix) * DigitSum(i, radix));
        }
        return result;
    }
    public static long SumOfSquareDigitSums(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfSquareDigitSums(number, radix);
        }
        return 0L;
    }
    public static string SumOfSquareDigitSumsString(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((DigitSum(i, radix) * DigitSum(i, radix)).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    public static long SumOfCubicDigitSums(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfCubicDigitSums(number, DEFAULT_RADIX);
    }
    public static long SumOfCubicDigitSums(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfCubicDigitSums(number, DEFAULT_RADIX);
        }
        return 0L;
    }
    public static string SumOfCubicDigitSumsString(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfCubicDigitSumsString(number, DEFAULT_RADIX);
    }
    public static long SumOfCubicDigitSums(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        long result = 0L;
        for (int i = 1; i <= number; i++)
        {
            result += (DigitSum(i, radix) * DigitSum(i, radix) * DigitSum(i, radix));
        }
        return result;
    }
    public static long SumOfCubicDigitSums(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfCubicDigitSums(number, radix);
        }
        return 0L;
    }
    public static string SumOfCubicDigitSumsString(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((DigitSum(i, radix) * DigitSum(i, radix) * DigitSum(i, radix)).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }

    public static int DigitalRoot(long number)
    {
        if (number < 0L) number *= -1L;

        return DigitalRoot(number, DEFAULT_RADIX);
    }
    public static int DigitalRoot(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return DigitalRoot(number);
        }
        return 0;
    }
    public static int DigitalRoot(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        return (int)(1L + (number - 1L) % (radix - 1));
    }
    public static int DigitalRoot(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return DigitalRoot(number, radix);
        }
        return 0;
    }
    public static long SumNumberDigitalRoots(long number)
    {
        if (number < 0L) number *= -1L;

        return SumNumberDigitalRoots(number, DEFAULT_RADIX);
    }
    public static long SumNumberDigitalRoots(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumNumberDigitalRoots(number);
        }
        return 0L;
    }
    public static long SumNumberDigitalRoots(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        long d = number / (radix - 1L);
        long r = number % (radix - 1L);
        long sum_1_to_radix_minus_1 = (radix - 1L) * (radix) / 2;
        return (d * sum_1_to_radix_minus_1 + r * (r + 1L) / 2L);
    }
    public static long SumNumberDigitalRoots(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumNumberDigitalRoots(number, radix);
        }
        return 0L;
    }
    public static string SumOfDigitalRootsString(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((DigitalRoot(i, radix)).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    public static string GetNumberDigitalRootsString(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfDigitalRootsString(number, DEFAULT_RADIX);
    }
    public static long SumOfSquareDigitalRoots(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfSquareDigitalRoots(number, DEFAULT_RADIX);
    }
    public static long SumOfSquareDigitalRoots(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfSquareDigitalRoots(number);
        }
        return 0L;
    }
    public static string SumOfSquareDigitalRootsString(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfSquareDigitalRootsString(number, DEFAULT_RADIX);
    }
    public static long SumOfSquareDigitalRoots(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        long result = 0L;
        for (int i = 1; i <= number; i++)
        {
            result += (DigitalRoot(i, radix) * DigitalRoot(i, radix));
        }
        return result;
    }
    public static long SumOfSquareDigitalRoots(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfSquareDigitalRoots(number, radix);
        }
        return 0L;
    }
    public static string SumOfSquareDigitalRootsString(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((DigitalRoot(i, radix) * DigitalRoot(i, radix)).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }
    public static long SumOfCubicDigitalRoots(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfCubicDigitalRoots(number, DEFAULT_RADIX);
    }
    public static long SumOfCubicDigitalRoots(string text)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfCubicDigitalRoots(number);
        }
        return 0L;
    }
    public static string SumOfCubicDigitalRootsString(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfCubicDigitalRootsString(number, DEFAULT_RADIX);
    }
    public static long SumOfCubicDigitalRoots(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        long result = 0L;
        for (int i = 1; i <= number; i++)
        {
            result += (DigitalRoot(i, radix) * DigitalRoot(i, radix) * DigitalRoot(i, radix));
        }
        return result;
    }
    public static long SumOfCubicDigitalRoots(string text, int radix)
    {
        long number;
        if (long.TryParse(text, out number))
        {
            return SumOfCubicDigitalRoots(number, radix);
        }
        return 0L;
    }
    public static string SumOfCubicDigitalRootsString(long number, int radix)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        for (int i = 1; i <= number; i++)
        {
            str.Append((DigitalRoot(i, radix) * DigitalRoot(i, radix) * DigitalRoot(i, radix)).ToString() + "+");
        }
        if (str.Length > 0)
        {
            str.Remove(str.Length - 1, 1);
        }
        return str.ToString();
    }

    public static bool IsDigitsOnly(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (!Char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }
    private static List<char> s_odd_digits = new List<char> { '1', '3', '5', '7', '9' };
    private static List<char> s_even_digits = new List<char> { '0', '2', '4', '6', '8' };
    private static List<char> s_prime_digits = new List<char> { '2', '3', '5', '7' };
    private static List<char> s_composite_digits = new List<char> { '4', '6', '8', '9' };
    public static List<char> OddDigits
    {
        get { return s_odd_digits; }
    }
    public static List<char> EvenDigits
    {
        get { return s_even_digits; }
    }
    public static List<char> PrimeDigits
    {
        get { return s_prime_digits; }
    }
    public static List<char> CompositeDigits
    {
        get { return s_composite_digits; }
    }
    public static bool IsOddDigits(long number)
    {
        if (number < 0L) number *= -1L;

        return IsOddDigits(number.ToString());
    }
    public static bool IsOddDigits(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (Char.IsDigit(c))
            {
                if (!s_odd_digits.Contains(c))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool IsEvenDigits(long number)
    {
        if (number < 0L) number *= -1L;

        return IsEvenDigits(number.ToString());
    }
    public static bool IsEvenDigits(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (Char.IsDigit(c))
            {
                if (!s_even_digits.Contains(c))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool IsPrimeDigits(long number)
    {
        if (number < 0L) number *= -1L;

        return IsPrimeDigits(number.ToString());
    }
    public static bool IsPrimeDigits(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (Char.IsDigit(c))
            {
                if (!s_prime_digits.Contains(c))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool IsPrimeOr1Digits(long number)
    {
        if (number < 0L) number *= -1L;

        return IsPrimeOr1Digits(number.ToString());
    }
    public static bool IsPrimeOr1Digits(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (Char.IsDigit(c))
            {
                if ((c != '1') && !s_prime_digits.Contains(c))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool IsCompositeDigits(long number)
    {
        if (number < 0L) number *= -1L;

        return IsCompositeDigits(number.ToString());
    }
    public static bool IsCompositeDigits(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (Char.IsDigit(c))
            {
                if (!s_composite_digits.Contains(c))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool IsCompositeOr0Digits(long number)
    {
        if (number < 0L) number *= -1L;

        return IsCompositeOr0Digits(number.ToString());
    }
    public static bool IsCompositeOr0Digits(string text)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        foreach (char c in text)
        {
            if (Char.IsDigit(c))
            {
                if ((c != '0') && !s_composite_digits.Contains(c))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static List<long> s_primes = null;
    private static List<long> s_additive_primes = null;
    private static List<long> s_non_additive_primes = null;
    public static List<long> Primes
    {
        get
        {
            if (s_primes == null)
            {
                GeneratePrimes(s_max_size);
            }
            return s_primes;
        }
    }
    public static List<long> AdditivePrimes
    {
        get
        {
            if (s_additive_primes == null)
            {
                GenerateAdditivePrimes(s_max_size);
            }
            return s_additive_primes;
        }
    }
    public static List<long> NonAdditivePrimes
    {
        get
        {
            if (s_non_additive_primes == null)
            {
                GenerateNonAdditivePrimes(s_max_size);
            }
            return s_non_additive_primes;
        }
    }
    public static int PrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsPrime(number))
        {
            if (s_primes == null)
            {
                GeneratePrimes(s_max_size);
            }
            return BinarySearch(s_primes, number);
        }
        return -1;
    }
    public static int AdditivePrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsAdditivePrime(number))
        {
            if (s_additive_primes == null)
            {
                GenerateAdditivePrimes(s_max_size);
            }
            return BinarySearch(s_additive_primes, number);
        }
        return -1;
    }
    public static int NonAdditivePrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsNonAdditivePrime(number))
        {
            if (s_non_additive_primes == null)
            {
                GenerateNonAdditivePrimes(s_max_size);
            }
            return BinarySearch(s_non_additive_primes, number);
        }
        return -1;
    }
    public static int PrimeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return PrimeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    public static int AdditivePrimeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return AdditivePrimeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    public static int NonAdditivePrimeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return NonAdditivePrimeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static void GeneratePrimes(int max)
    {
        //if (s_primes != null)
        //{
        //    int primes_upto_max = (int)(max / (Math.Log(max) + 1));
        //    if (s_primes.Count >= primes_upto_max)
        //    {
        //        return; // we already have a large list, no need to RE-generate new one
        //    }
        //}

        if (s_primes == null)
        {
            BitArray composites = new BitArray(max + 1);

            s_primes = new List<long>();

            s_primes.Add(2L);

            // process odd numbers // 3, 5, 7, 9, 11, ..., max
            long sqrt = (long)Math.Round(Math.Sqrt(max)) + 1L;
            for (int i = 3; i <= max; i += 2)
            {
                if (!composites[i])
                {
                    s_primes.Add(i);

                    // mark off multiples of i starting from i*i and skipping even "i"s
                    if (i < sqrt)
                    {
                        for (int j = i * i; j <= max; j += 2 * i)
                        {
                            composites[j] = true;
                        }
                    }
                }
            }
        }
    }
    private static void GenerateAdditivePrimes(int max)
    {
        //// re-generate for new max if larger
        //GeneratePrimes(max);

        if (s_additive_primes == null)
        {
            if (s_primes == null)
            {
                GeneratePrimes(max);
            }

            if (s_primes != null)
            {
                s_additive_primes = new List<long>();
                int count = s_primes.Count;
                for (int i = 0; i < count; i++)
                {
                    if (IsPrime(DigitSum(s_primes[i])))
                    {
                        s_additive_primes.Add(s_primes[i]);
                    }
                }
            }
        }
    }
    private static void GenerateNonAdditivePrimes(int max)
    {
        //// re-generate for new max if larger
        //GeneratePrimes(max);

        if (s_non_additive_primes == null)
        {
            if (s_primes == null)
            {
                GeneratePrimes(max);
            }

            if (s_primes != null)
            {
                s_non_additive_primes = new List<long>();
                int count = s_primes.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!IsPrime(DigitSum(s_primes[i])))
                    {
                        s_non_additive_primes.Add(s_primes[i]);
                    }
                }
            }
        }
    }
    private static string s_primes_path = "primes.txt";
    private static string s_additive_primes_path = "additive_primes.txt";
    private static string s_non_additive_primes_path = "non_additive_primes.txt";
    private static void LoadPrimes()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_primes_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_primes = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_primes.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void LoadAdditivePrimes()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_additive_primes_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_additive_primes = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_additive_primes.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void LoadNonAdditivePrimes()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_non_additive_primes_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_non_additive_primes = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_non_additive_primes.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void SavePrimes()
    {
        if (s_primes != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_primes_path;
                FileHelper.SaveValues(path, s_primes);
            }
        }
    }
    private static void SaveAdditivePrimes()
    {
        if (s_additive_primes != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_additive_primes_path;
                FileHelper.SaveValues(path, s_additive_primes);
            }
        }
    }
    private static void SaveNonAdditivePrimes()
    {
        if (s_non_additive_primes != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_non_additive_primes_path;
                FileHelper.SaveValues(path, s_non_additive_primes);
            }
        }
    }
    private static List<long> s_primes_4nplus1 = null;
    public static List<long> Primes4nPlus1
    {
        get
        {
            if (s_primes_4nplus1 == null)
            {
                LoadPrimes4nPlus1();
            }
            return s_primes_4nplus1;
        }
    }
    public static int Prime4nPlus1IndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsPrime(number))
        {
            if (s_primes_4nplus1 == null)
            {
                LoadPrimes4nPlus1();
            }
            return BinarySearch(s_primes_4nplus1, number);
        }
        return -1;
    }
    public static int Prime4nPlus1IndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return Prime4nPlus1IndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static string s_primes_4nplus1_path = "primes_4n+1.txt";
    private static void LoadPrimes4nPlus1()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_primes_4nplus1_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_primes_4nplus1 = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_primes_4nplus1.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static List<long> s_primes_4nminus1 = null;
    public static List<long> Primes4nMinus1
    {
        get
        {
            if (s_primes_4nminus1 == null)
            {
                LoadPrimes4nMinus1();
            }
            return s_primes_4nminus1;
        }
    }
    public static int Prime4nMinus1IndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsPrime(number))
        {
            if (s_primes_4nminus1 == null)
            {
                LoadPrimes4nMinus1();
            }
            return BinarySearch(s_primes_4nminus1, number);
        }
        return -1;
    }
    public static int Prime4nMinus1IndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return Prime4nMinus1IndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static string s_primes_4nminus1_path = "primes_4n-1.txt";
    private static void LoadPrimes4nMinus1()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_primes_4nminus1_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_primes_4nminus1 = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_primes_4nminus1.Add(number);
                        }
                    }
                }
            }
        }
    }

    private static List<long> s_composites = null;
    private static List<long> s_additive_composites = null;
    private static List<long> s_non_additive_composites = null;
    public static List<long> Composites
    {
        get
        {
            if (s_composites == null)
            {
                GenerateComposites(s_max_size / 2);
            }
            return s_composites;
        }
    }
    public static List<long> AdditiveComposites
    {
        get
        {
            if (s_additive_composites == null)
            {
                GenerateAdditiveComposites(s_max_size / 2);
            }
            return s_additive_composites;
        }
    }
    public static List<long> NonAdditiveComposites
    {
        get
        {
            if (s_non_additive_composites == null)
            {
                GenerateNonAdditiveComposites(s_max_size / 2);
            }
            return s_non_additive_composites;
        }
    }
    public static int CompositeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsComposite(number))
        {
            int max = s_max_size / 2;
            if (s_composites == null)
            {
                GenerateComposites(max);
            }
            return BinarySearch(s_composites, number);
        }
        return -1;
    }
    public static int AdditiveCompositeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsAdditiveComposite(number))
        {
            int max = s_max_size / 2;
            if (s_additive_composites == null)
            {
                GenerateAdditiveComposites(max);
            }
            return BinarySearch(s_additive_composites, number);
        }
        return -1;
    }
    public static int NonAdditiveCompositeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsNonAdditiveComposite(number))
        {
            int max = s_max_size / 2;
            if (s_non_additive_composites == null)
            {
                GenerateNonAdditiveComposites(max);
            }
            return BinarySearch(s_non_additive_composites, number);
        }
        return -1;
    }
    public static int CompositeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return CompositeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    public static int AdditiveCompositeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return AdditiveCompositeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    public static int NonAdditiveCompositeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return NonAdditiveCompositeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static void GenerateComposites(int max)
    {
        //if (s_composites != null)
        //{
        //    int primes_upto_max = (int)(max / (Math.Log(max) + 1));
        //    if (s_composites.Count >= (max - primes_upto_max))
        //    {
        //        return; // we already have a large list, no need to RE-generate new one
        //    }
        //}

        if (s_composites == null)
        {
            BitArray composites = new BitArray(max + 1);

            s_composites = new List<long>(max);

            for (int i = 4; i <= max; i += 2)
            {
                composites[i] = true;
            }

            // process odd numbers // 3, 5, 7, 9, 11, ..., max
            long sqrt = (long)Math.Round(Math.Sqrt(max)) + 1L;
            for (int i = 3; i <= max; i += 2)
            {
                if (!composites[i])
                {
                    // mark off multiples of i
                    if (i <= sqrt)
                    {
                        for (int j = i * i; j <= max; j += 2 * i)
                        {
                            composites[j] = true;
                        }
                    }
                }
            }

            for (int i = 4; i <= max; i++)
            {
                if (composites[i])
                {
                    s_composites.Add(i);
                }
            }
        }
    }
    private static void GenerateAdditiveComposites(int max)
    {
        //// re-generate for new max if larger
        //GenerateComposites(max);

        if (s_additive_composites == null)
        {
            if (s_composites == null)
            {
                GenerateComposites(max);
            }

            if (s_composites != null)
            {
                s_additive_composites = new List<long>();
                int count = s_composites.Count;
                for (int i = 0; i < count; i++)
                {
                    if (IsComposite(DigitSum(s_composites[i])))
                    {
                        s_additive_composites.Add(s_composites[i]);
                    }
                }
            }
        }
    }
    private static void GenerateNonAdditiveComposites(int max)
    {
        //// re-generate for new max if larger
        //GenerateComposites(max);

        if (s_non_additive_composites == null)
        {
            if (s_composites == null)
            {
                GenerateComposites(max);
            }

            if (s_composites != null)
            {
                s_non_additive_composites = new List<long>();
                int count = s_composites.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!IsComposite(DigitSum(s_composites[i])))
                    {
                        s_non_additive_composites.Add(s_composites[i]);
                    }
                }
            }
        }
    }
    private static string s_composites_path = "composites.txt";
    private static string s_additive_composites_path = "additive_composites.txt";
    private static string s_non_additive_composites_path = "non_additive_composites.txt";
    private static void LoadComposites()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_composites_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_composites = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_composites.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void LoadAdditiveComposites()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_additive_composites_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_additive_composites = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_additive_composites.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void LoadNonAdditiveComposites()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_non_additive_composites_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_non_additive_composites = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_non_additive_composites.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void SaveComposites()
    {
        if (s_composites != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_composites_path;
                FileHelper.SaveValues(path, s_composites);
            }
        }
    }
    private static void SaveAdditiveComposites()
    {
        if (s_additive_composites != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_additive_composites_path;
                FileHelper.SaveValues(path, s_additive_composites);
            }
        }
    }
    private static void SaveNonAdditiveComposites()
    {
        if (s_non_additive_composites != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_non_additive_composites_path;
                FileHelper.SaveValues(path, s_non_additive_composites);
            }
        }
    }
    private static List<long> s_composites_4nplus1 = null;
    public static List<long> Composites4nPlus1
    {
        get
        {
            if (s_composites_4nplus1 == null)
            {
                LoadComposites4nPlus1();
            }
            return s_composites_4nplus1;
        }
    }
    public static int Composite4nPlus1IndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsComposite(number))
        {
            if (s_composites_4nplus1 == null)
            {
                LoadComposites4nPlus1();
            }
            return BinarySearch(s_composites_4nplus1, number);
        }
        return -1;
    }
    public static int Composite4nPlus1IndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return Composite4nPlus1IndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static string s_composites_4nplus1_path = "composites_4n+1.txt";
    private static void LoadComposites4nPlus1()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_composites_4nplus1_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_composites_4nplus1 = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_composites_4nplus1.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static List<long> s_composites_4nminus1 = null;
    public static List<long> Composites4nMinus1
    {
        get
        {
            if (s_composites_4nminus1 == null)
            {
                LoadComposites4nMinus1();
            }
            return s_composites_4nminus1;
        }
    }
    public static int Composite4nMinus1IndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (IsComposite(number))
        {
            if (s_composites_4nminus1 == null)
            {
                LoadComposites4nMinus1();
            }
            return BinarySearch(s_composites_4nminus1, number);
        }
        return -1;
    }
    public static int Composite4nMinus1IndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return Composite4nMinus1IndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static string s_composites_4nminus1_path = "composites_4n-1.txt";
    private static void LoadComposites4nMinus1()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_composites_4nminus1_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_composites_4nminus1 = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_composites_4nminus1.Add(number);
                        }
                    }
                }
            }
        }
    }

    // Number Dimensions (Number of Factors): 1a, 2a, 3a, ..., 19a
    public static Dictionary<int, List<long>> NumberDimensions = null;
    public static bool IsNumberDimension(int dimension, long number)
    {
        if (number < 0L) number *= -1L;

        if (NumberDimensions.ContainsKey(dimension - 1))
        {
            return NumberDimensions[dimension - 1].Contains(number);
        }
        return false;
    }
    public static int NumberDimensionIndexOf(int dimension, long number)
    {
        if (number < 0L) number *= -1L;

        if (NumberDimensions.ContainsKey(dimension - 1))
        {
            return BinarySearch(NumberDimensions[dimension - 1], number);
        }
        return -1;
    }
    private static string s_number_dimensions_path = "a.txt";
    public static void LoadNumberDimensions()
    {
        NumberDimensions = new Dictionary<int, List<long>>();
        for (int i = 0; i < 19; i++)
        {
            string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + (i + 1).ToString() + s_number_dimensions_path;
            if (File.Exists(path))
            {
                FileHelper.WaitForFileReady(path);

                List<long> number_dimension = new List<long>();
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = null;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            if (line.Length == 0)
                                break;

                            long number;
                            if (long.TryParse(line, out number))
                            {
                                number_dimension.Add(number);
                            }
                        }
                    }
                }
                NumberDimensions.Add(i, number_dimension);
            }
        }
    }
    // Duplicate Number Dimensions (Number of Factors): 2d, 3d, ..., 19d
    public static Dictionary<int, List<long>> DuplicateNumberDimensions = null;
    public static bool IsDuplicateNumberDimension(int dimension, long number)
    {
        if (number < 0L) number *= -1L;

        if (DuplicateNumberDimensions.ContainsKey(dimension - 1))
        {
            return DuplicateNumberDimensions[dimension - 1].Contains(number);
        }
        return false;
    }
    public static int DuplicateNumberDimensionIndexOf(int dimension, long number)
    {
        if (number < 0L) number *= -1L;

        if (DuplicateNumberDimensions.ContainsKey(dimension - 1))
        {
            return BinarySearch(DuplicateNumberDimensions[dimension - 1], number);
        }
        return -1;
    }
    private static string s_duplicate_number_dimensions_path = "d.txt";
    public static void LoadDuplicateNumberDimensions()
    {
        DuplicateNumberDimensions = new Dictionary<int, List<long>>();
        for (int i = 1; i < 19; i++)
        {
            string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + (i + 1).ToString() + s_duplicate_number_dimensions_path;
            if (File.Exists(path))
            {
                FileHelper.WaitForFileReady(path);

                List<long> duplicate_number_dimension = new List<long>();
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = null;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            if (line.Length == 0)
                                break;

                            long number;
                            if (long.TryParse(line, out number))
                            {
                                duplicate_number_dimension.Add(number);
                            }
                        }
                    }
                }
                DuplicateNumberDimensions.Add(i, duplicate_number_dimension);
            }
        }
    }
    // Unique Number Dimensions (Number of Factors): 2u, 3u, ..., 7u
    public static Dictionary<int, List<long>> UniqueNumberDimensions = null;
    public static bool IsUniqueNumberDimension(int dimension, long number)
    {
        if (number < 0L) number *= -1L;

        if (UniqueNumberDimensions.ContainsKey(dimension - 1))
        {
            return UniqueNumberDimensions[dimension - 1].Contains(number);
        }
        return false;
    }
    public static int UniqueNumberDimensionIndexOf(int dimension, long number)
    {
        if (number < 0L) number *= -1L;

        if (UniqueNumberDimensions.ContainsKey(dimension - 1))
        {
            return BinarySearch(UniqueNumberDimensions[dimension - 1], number);
        }
        return -1;
    }
    private static string s_unique_number_dimensions_path = "u.txt";
    public static void LoadUniqueNumberDimensions()
    {
        UniqueNumberDimensions = new Dictionary<int, List<long>>();
        for (int i = 1; i < 7; i++)
        {
            string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + (i + 1).ToString() + s_unique_number_dimensions_path;
            if (File.Exists(path))
            {
                FileHelper.WaitForFileReady(path);

                List<long> unique_number_dimension = new List<long>();
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = null;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            if (line.Length == 0)
                                break;

                            long number;
                            if (long.TryParse(line, out number))
                            {
                                unique_number_dimension.Add(number);
                            }
                        }
                    }
                }
                UniqueNumberDimensions.Add(i, unique_number_dimension);
            }
        }
    }

    // prime repeated units 1111111.....1111111 numbers
    public static List<long> PrimeRepunits = null;
    public static bool IsPrimeRepunit(long number)
    {
        if (number < 0L) number *= -1L;

        return PrimeRepunits.Contains(number);
    }
    private static string s_prime_repunits_path = "prime_repunits.txt";
    public static void LoadPrimeRepunits()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_prime_repunits_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            PrimeRepunits = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        if (line.Length == 0)
                            break;

                        long number;
                        if (long.TryParse(line, out number))
                        {
                            PrimeRepunits.Add(number);
                        }
                    }
                }
            }
        }
    }

    // https://en.wikipedia.org/wiki/Full_reptend_prime
    // full reptend primes, full repetend primes, proper primes or long primes
    public static List<long> ReptendPrimes = null;
    public static bool IsReptendPrime(long number)
    {
        if (number < 0L) number *= -1L;

        return ReptendPrimes.Contains(number);
    }
    private static string s_reptend_primes_path = "reptend_primes.txt";
    public static void LoadReptendPrimes()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_reptend_primes_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            ReptendPrimes = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        if (line.Length == 0)
                            break;

                        long number;
                        if (long.TryParse(line, out number))
                        {
                            ReptendPrimes.Add(number);
                        }
                    }
                }
            }
        }
    }

    public static NumberKind GetNumberKind(long number)
    {
        if (number < 0L) number *= -1L;
        if (number == 0L) return NumberKind.Deficient;
        if (number == 1L) return NumberKind.Deficient;
        if (number > 1000000000000L) return NumberKind.Deficient;

        long sum_of_proper_divisors = SumOfProperDivisors(number);
        if (sum_of_proper_divisors < number)
        {
            return NumberKind.Deficient;
        }
        else if (sum_of_proper_divisors == number)
        {
            return NumberKind.Perfect;
        }
        else //if (sum_of_proper_divisors > number)
        {
            return NumberKind.Abundant;
        }
    }
    private static List<long> s_deficient_numbers;
    public static List<long> DeficientNumbers
    {
        get
        {
            if (s_deficient_numbers == null)
            {
                GenerateDeficientNumbers(s_max_size / 16);
            }
            return s_deficient_numbers;
        }
    }
    public static int DeficientNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_deficient_numbers == null)
        {
            GenerateDeficientNumbers(s_max_size / 16);
        }
        return BinarySearch(s_deficient_numbers, number);
    }
    private static void GenerateDeficientNumbers(int max)
    {
        s_deficient_numbers = new List<long>(max);
        for (int i = 0; i < max; i++)
        {
            long sum_of_proper_divisors = SumOfProperDivisors(i);
            if (sum_of_proper_divisors < i)
            {
                s_deficient_numbers.Add(i);
            }
        }
    }
    /// <summary>
    /// <para>Deficient number is a number with the sum of its proper divisors is less than itself</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsDeficientNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return DeficientNumbers.Contains(number);
    }
    private static string s_deficient_numbers_path = "deficient_numbers.txt";
    private static void LoadDeficientNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_deficient_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_deficient_numbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_deficient_numbers.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void SaveDeficientNumbers()
    {
        if (s_deficient_numbers != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_deficient_numbers_path;
                FileHelper.SaveValues(path, s_deficient_numbers);
            }
        }
    }

    private static List<long> s_perfect_numbers;
    public static List<long> PerfectNumbers
    {
        get
        {
            if (s_perfect_numbers == null)
            {
                GeneratePerfectNumbers(s_max_size / 16);
            }
            return s_perfect_numbers;
        }
    }
    public static int PerfectNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_perfect_numbers == null)
        {
            GeneratePerfectNumbers(s_max_size / 16);
        }
        return BinarySearch(s_perfect_numbers, number);
    }
    private static void GeneratePerfectNumbers(int max)
    {
        s_perfect_numbers = new List<long>(max) { 1 }; // 1 is 1st perfect number
        for (int i = 2; i < max; i++)
        {
            long sum_of_proper_divisors = SumOfProperDivisors(i);
            if (sum_of_proper_divisors < i)
            {
                s_perfect_numbers.Add(i);
            }
        }
    }
    /// <summary>
    /// <para>Perfect number is a number with the sum of its proper divisors equals itself</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsPerfectNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return PerfectNumbers.Contains(number);
    }
    private static string s_perfect_numbers_path = "perfect_numbers.txt";
    private static void LoadPerfectNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_perfect_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_perfect_numbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_perfect_numbers.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void SavePerfectNumbers()
    {
        if (s_perfect_numbers != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_perfect_numbers_path;
                FileHelper.SaveValues(path, s_perfect_numbers);
            }
        }
    }

    private static List<long> s_abundant_numbers;
    public static List<long> AbundantNumbers
    {
        get
        {
            if (s_abundant_numbers == null)
            {
                GenerateAbundantNumbers(s_max_size / 16);
            }
            return s_abundant_numbers;
        }
    }
    public static int AbundantNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_abundant_numbers == null)
        {
            GenerateAbundantNumbers(s_max_size / 16);
        }
        return BinarySearch(s_abundant_numbers, number);
    }
    private static void GenerateAbundantNumbers(int max)
    {
        s_abundant_numbers = new List<long>(max);
        for (int i = 0; i < max; i++)
        {
            long sum_of_proper_divisors = SumOfProperDivisors(i);
            if (sum_of_proper_divisors > i)
            {
                s_abundant_numbers.Add(i);
            }
        }
    }
    /// <summary>
    /// <para>Abundant number is a number with the sum of its proper divisors is greater than itself</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsAbundantNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return AbundantNumbers.Contains(number);
    }
    private static string s_abundant_numbers_path = "abundant_numbers.txt";
    private static void LoadAbundantNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_abundant_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_abundant_numbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_abundant_numbers.Add(number);
                        }
                    }
                }
            }
        }
    }
    private static void SaveAbundantNumbers()
    {
        if (s_abundant_numbers != null)
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_abundant_numbers_path;
                FileHelper.SaveValues(path, s_abundant_numbers);
            }
        }
    }

    public static bool IsSquareNumber(this long number)
    {
        return (IsPowerNumber(number, 2));
    }
    public static bool IsCubicNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 3));
    }
    public static bool IsQuarticNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 4));
    }
    public static bool IsQuinticNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 5));
    }
    public static bool IsSexticNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 6));
    }
    public static bool IsSepticNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 7));
    }
    public static bool IsOcticNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 8));
    }
    public static bool IsNonicNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 9));
    }
    public static bool IsDecicNumber(this long number)
    {
        if (number < 0L) number *= -1L;

        return (IsPowerNumber(number, 10));
    }
    public static bool IsPowerNumber(this long number, int power)
    {
        if (number < 0L) number *= -1L;

        long root = (long)Math.Round(Math.Pow(number, (1.0D / power)));
        long total = 1L;
        for (int i = 0; i < power; i++)
        {
            total *= root;
        }
        return (number == total);
    }
    public static List<long> PowerNumbers(int power)
    {
        List<long> result = new List<long>();
        for (int i = 1; i < MAX_SERIES_LENGTH; i++)
        {
            result.Add((long)Math.Pow(i, power));
        }
        return result;
    }
    public static List<long> SquareNumbers
    {
        get
        {
            return PowerNumbers(2);
        }
    }
    public static List<long> CubicNumbers
    {
        get
        {
            return PowerNumbers(3);
        }
    }
    public static List<long> QuarticNumbers
    {
        get
        {
            return PowerNumbers(4);
        }
    }
    public static List<long> QuinticNumbers
    {
        get
        {
            return PowerNumbers(5);
        }
    }
    public static List<long> SexticNumbers
    {
        get
        {
            return PowerNumbers(6);
        }
    }
    public static List<long> SepticNumbers
    {
        get
        {
            return PowerNumbers(7);
        }
    }
    public static List<long> OcticNumbers
    {
        get
        {
            return PowerNumbers(8);
        }
    }
    public static List<long> NonicNumbers
    {
        get
        {
            return PowerNumbers(9);
        }
    }
    public static List<long> DecicNumbers
    {
        get
        {
            return PowerNumbers(10);
        }
    }

    //https://en.wikipedia.org/wiki/Polygonal_number
    //ith number of Polygonal(sides=N) = ( (N - 2)*i*i - (N - 4)*i ) / 2
    //--------------------------------------------------------------------------------------------
    //N   Name            Formula            i = 1 2 3 4 5 6 7 8 9 10             oeis.org/A??????
    //--------------------------------------------------------------------------------------------
    //3   Triangular      ½(1n² + 1n)         1 3 6 10 15 21 28 36 45 55           A000217  1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 66, 78, 91, 105, 120, 136, 153, 171, 190, 210, 231, 253, 276, 300, 325, 351, 378, 406, 435, 465, 496, 528, 561, 595, 630, 666, 703, 741, 780, 820, 861, 903, 946, 990, 1035, 1081, 1128, 1176, 1225, 1275, 1326, 1378, 1431
    //4   Square          ½(2n² - 0n)         1 4 9 16 25 36 49 64 81 100          A000290  1, 4, 9, 16, 25, 36, 49, 64, 81, 100, 121, 144, 169, 196, 225, 256, 289, 324, 361, 400, 441, 484, 529, 576, 625, 676, 729, 784, 841, 900, 961, 1024, 1089, 1156, 1225, 1296, 1369, 1444, 1521, 1600, 1681, 1764, 1849, 1936, 2025, 2116, 2209, 2304, 2401, 2500
    //5   Pentagonal      ½(3n² - 1n)         1 5 12 22 35 51 70 92 117 145        A000326  1, 5, 12, 22, 35, 51, 70, 92, 117, 145, 176, 210, 247, 287, 330, 376, 425, 477, 532, 590, 651, 715, 782, 852, 925, 1001, 1080, 1162, 1247, 1335, 1426, 1520, 1617, 1717, 1820, 1926, 2035, 2147, 2262, 2380, 2501, 2625, 2752, 2882, 3015, 3151
    //6   Hexagonal       ½(4n² - 2n)         1 6 15 28 45 66 91 120 153 190       A000384  1, 6, 15, 28, 45, 66, 91, 120, 153, 190, 231, 276, 325, 378, 435, 496, 561, 630, 703, 780, 861, 946, 1035, 1128, 1225, 1326, 1431, 1540, 1653, 1770, 1891, 2016, 2145, 2278, 2415, 2556, 2701, 2850, 3003, 3160, 3321, 3486, 3655, 3828, 4005, 4186, 4371, 4560
    //7   Heptagonal      ½(5n² - 3n)         1 7 18 34 55 81 112 148 189 235      A000566  1, 7, 18, 34, 55, 81, 112, 148, 189, 235, 286, 342, 403, 469, 540, 616, 697, 783, 874, 970, 1071, 1177, 1288, 1404, 1525, 1651, 1782, 1918, 2059, 2205, 2356, 2512, 2673, 2839, 3010, 3186, 3367, 3553, 3744, 3940, 4141, 4347, 4558, 4774, 4995, 5221, 5452, 5688
    //8   Octagonal       ½(6n² - 4n)         1 8 21 40 65 96 133 176 225 280      A000567  1, 8, 21, 40, 65, 96, 133, 176, 225, 280, 341, 408, 481, 560, 645, 736, 833, 936, 1045, 1160, 1281, 1408, 1541, 1680, 1825, 1976, 2133, 2296, 2465, 2640, 2821, 3008, 3201, 3400, 3605, 3816, 4033, 4256, 4485, 4720, 4961, 5208, 5461
    //9   Nonagonal       ½(7n² - 5n)         1 9 24 46 75 111 154 204 261 325     A001106  1, 9, 24, 46, 75, 111, 154, 204, 261, 325, 396, 474, 559, 651, 750, 856, 969, 1089, 1216, 1350, 1491, 1639, 1794, 1956, 2125, 2301, 2484, 2674, 2871, 3075, 3286, 3504, 3729, 3961, 4200, 4446, 4699, 4959, 5226, 5500, 5781, 6069, 6364
    //10  Decagonal       ½(8n² - 6n)         1 10 27 52 85 126 175 232 297 370    A001107  1, 10, 27, 52, 85, 126, 175, 232, 297, 370, 451, 540, 637, 742, 855, 976, 1105, 1242, 1387, 1540, 1701, 1870, 2047, 2232, 2425, 2626, 2835, 3052, 3277, 3510, 3751, 4000, 4257, 4522, 4795, 5076, 5365, 5662, 5967, 6280, 6601, 6930, 7267, 7612, 7965, 8326
    //11  Hendecagonal    ½(9n² - 7n)         1 11 30 58 95 141 196 260 333 415    A051682  1, 11, 30, 58, 95, 141, 196, 260, 333, 415, 506, 606, 715, 833, 960, 1096, 1241, 1395, 1558, 1730, 1911, 2101, 2300, 2508, 2725, 2951, 3186, 3430, 3683, 3945, 4216, 4496, 4785, 5083, 5390, 5706, 6031, 6365, 6708, 7060, 7421, 7791, 8170
    //12  Dodecagonal     ½(10n² - 8n)        1 12 33 64 105 156 217 288 369 460   A051624  1, 12, 33, 64, 105, 156, 217, 288, 369, 460, 561, 672, 793, 924, 1065, 1216, 1377, 1548, 1729, 1920, 2121, 2332, 2553, 2784, 3025, 3276, 3537, 3808, 4089, 4380, 4681, 4992, 5313, 5644, 5985, 6336, 6697, 7068, 7449, 7840, 8241, 8652
    //13  Tridecagonal    ½(11n² - 9n)        1 13 36 70 115 171 238 316 405 505   A051865  1, 13, 36, 70, 115, 171, 238, 316, 405, 505, 616, 738, 871, 1015, 1170, 1336, 1513, 1701, 1900, 2110, 2331, 2563, 2806, 3060, 3325, 3601, 3888, 4186, 4495, 4815, 5146, 5488, 5841, 6205, 6580, 6966, 7363, 7771, 8190, 8620, 9061, 9513
    //14  Tetradecagonal  ½(12n² - 10n)       1 14 39 76 125 186 259 344 441 550   A051866  1, 14, 39, 76, 125, 186, 259, 344, 441, 550, 671, 804, 949, 1106, 1275, 1456, 1649, 1854, 2071, 2300, 2541, 2794, 3059, 3336, 3625, 3926, 4239, 4564, 4901, 5250, 5611, 5984, 6369, 6766, 7175, 7596, 8029, 8474, 8931, 9400, 9881, 10374
    //15  Pentadecagonal  ½(13n² - 11n)       1 15 42 82 135 201 280 372 477 595   A051867  1, 15, 42, 82, 135, 201, 280, 372, 477, 595, 726, 870, 1027, 1197, 1380, 1576, 1785, 2007, 2242, 2490, 2751, 3025, 3312, 3612, 3925, 4251, 4590, 4942, 5307, 5685, 6076, 6480, 6897, 7327, 7770, 8226, 8695, 9177, 9672, 10180, 10701
    //16  Hexadecagonal   ½(14n² - 12n)       1 16 45 88 145 216 301 400 513 640   A051868  1, 16, 45, 88, 145, 216, 301, 400, 513, 640, 781, 936, 1105, 1288, 1485, 1696, 1921, 2160, 2413, 2680, 2961, 3256, 3565, 3888, 4225, 4576, 4941, 5320, 5713, 6120, 6541, 6976, 7425, 7888, 8365, 8856, 9361, 9880, 10413, 10960, 11521
    //17  Heptadecagonal  ½(15n² - 13n)       1 17 48 94 155 231 322 428 549 685   A051869  1, 17, 48, 94, 155, 231, 322, 428, 549, 685, 836, 1002, 1183, 1379, 1590, 1816, 2057, 2313, 2584, 2870, 3171, 3487, 3818, 4164, 4525, 4901, 5292, 5698, 6119, 6555, 7006, 7472, 7953, 8449, 8960, 9486, 10027, 10583, 11154, 11740, 12341
    //18  Octadecagonal   ½(16n² - 14n)       1 18 51 100 165 246 343 456 585 730  A051870  1, 18, 51, 100, 165, 246, 343, 456, 585, 730, 891, 1068, 1261, 1470, 1695, 1936, 2193, 2466, 2755, 3060, 3381, 3718, 4071, 4440, 4825, 5226, 5643, 6076, 6525, 6990, 7471, 7968, 8481, 9010, 9555, 10116, 10693, 11286, 11895, 12520
    //19  Nonadecagonal   ½(17n² - 15n)       1 19 54 106 175 261 364 484 621 775  A051871  1, 19, 54, 106, 175, 261, 364, 484, 621, 775, 946, 1134, 1339, 1561, 1800, 2056, 2329, 2619, 2926, 3250, 3591, 3949, 4324, 4716, 5125, 5551, 5994, 6454, 6931, 7425, 7936, 8464, 9009, 9571, 10150, 10746, 11359, 11989, 12636, 13300
    //20  Icosagonal      ½(18n² - 16n)       1 20 57 112 185 276 385 512 657 820  A051872  1, 20, 57, 112, 185, 276, 385, 512, 657, 820, 1001, 1200, 1417, 1652, 1905, 2176, 2465, 2772, 3097, 3440, 3801, 4180, 4577, 4992, 5425, 5876, 6345, 6832, 7337, 7860, 8401, 8960, 9537, 10132, 10745, 11376, 12025, 12692, 13377, 14080
    //21  Icosihenagonal  ½(19n² - 17n)       1 21 60 118 195 291 406 540 693 865  A051873  1, 21, 60, 118, 195, 291, 406, 540, 693, 865, 1056, 1266, 1495, 1743, 2010, 2296, 2601, 2925, 3268, 3630, 4011, 4411, 4830, 5268, 5725, 6201, 6696, 7210, 7743, 8295, 8866, 9456, 10065, 10693, 11340, 12006, 12691, 13395, 14118
    //22  Icosidigonal    ½(20n² - 18n)       1 22 63 124 205 306 427 568 729 910  A051874  1, 22, 63, 124, 205, 306, 427, 568, 729, 910, 1111, 1332, 1573, 1834, 2115, 2416, 2737, 3078, 3439, 3820, 4221, 4642, 5083, 5544, 6025, 6526, 7047, 7588, 8149, 8730, 9331, 9952, 10593, 11254, 11935, 12636, 13357, 14098, 14859
    //23  Icositrigonal   ½(21n² - 19n)       1 23 66 130 215 321 448 596 765 955  A051875  1, 23, 66, 130, 215, 321, 448, 596, 765, 955, 1166, 1398, 1651, 1925, 2220, 2536, 2873, 3231, 3610, 4010, 4431, 4873, 5336, 5820, 6325, 6851, 7398, 7966, 8555, 9165, 9796, 10448, 11121, 11815, 12530, 13266, 14023, 14801, 15600
    //24  Icositetragonal ½(22n² - 20n)       1 24 69 136 225 336 469 624 801 1000 A051876  1, 24, 69, 136, 225, 336, 469, 624, 801, 1000, 1221, 1464, 1729, 2016, 2325, 2656, 3009, 3384, 3781, 4200, 4641, 5104, 5589, 6096, 6625, 7176, 7749, 8344, 8961, 9600, 10261, 10944, 11649, 12376, 13125, 13896, 14689, 15504, 16341
    //----------------------------------------------------------------------------------------
    private static Dictionary<int, List<long>> s_polygonal_numbers_dictionary = new Dictionary<int, List<long>>();
    public static List<long> PolygonalNumbers(int sides)
    {
        if (!s_polygonal_numbers_dictionary.ContainsKey(sides))
        {
            GeneratePolygonalNumbers(sides);
        }

        if (s_polygonal_numbers_dictionary.ContainsKey(sides))
        {
            return s_polygonal_numbers_dictionary[sides];
        }
        else
        {
            return null;
        }
    }
    private static void GeneratePolygonalNumbers(int sides)
    {
        List<long> polygonal_numbers = new List<long>(MAX_SERIES_LENGTH);
        for (int n = 1; n <= MAX_SERIES_LENGTH; n++)
        {
            long number = ((sides - 2) * n * n - (sides - 4) * n) / 2L;
            polygonal_numbers.Add(number);
        }
        s_polygonal_numbers_dictionary.Add(sides, polygonal_numbers);
    }
    public static bool IsPolygonalNumber(int sides, long number)
    {
        if (number < 0L) number *= -1L;

        if (s_polygonal_numbers_dictionary.ContainsKey(sides))
        {
            return (s_polygonal_numbers_dictionary[sides].Contains(number));
        }
        return false;
    }
    public static List<long> TriangularNumbers
    {
        get
        {
            return PolygonalNumbers(3);
        }
    }
    public static List<long> QuadlateralNumbers
    {
        get
        {
            return PolygonalNumbers(4);
        }
    }
    public static List<long> PentagonalNumbers
    {
        get
        {
            return PolygonalNumbers(5);
        }
    }
    public static List<long> HexagonalNumbers
    {
        get
        {
            return PolygonalNumbers(6);
        }
    }
    public static List<long> HeptagonalNumbers
    {
        get
        {
            return PolygonalNumbers(7);
        }
    }
    public static List<long> OctagonalNumbers
    {
        get
        {
            return PolygonalNumbers(8);
        }
    }
    public static List<long> NonagonalNumbers
    {
        get
        {
            return PolygonalNumbers(9);
        }
    }
    public static List<long> DecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(10);
        }
    }
    public static List<long> HendecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(11);
        }
    }
    public static List<long> DodecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(12);
        }
    }
    public static List<long> TridecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(13);
        }
    }
    public static List<long> TetradecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(14);
        }
    }
    public static List<long> PentadecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(15);
        }
    }
    public static List<long> HexadecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(16);
        }
    }
    public static List<long> HeptadecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(17);
        }
    }
    public static List<long> OctadecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(18);
        }
    }
    public static List<long> NonadecagonalNumbers
    {
        get
        {
            return PolygonalNumbers(19);
        }
    }
    public static List<long> IcosagonalNumbers
    {
        get
        {
            return PolygonalNumbers(20);
        }
    }
    public static List<long> IcosihenagonalNumbers
    {
        get
        {
            return PolygonalNumbers(21);
        }
    }
    public static List<long> IcosidigonalNumbers
    {
        get
        {
            return PolygonalNumbers(22);
        }
    }
    public static List<long> IcositrigonalNumbers
    {
        get
        {
            return PolygonalNumbers(23);
        }
    }
    public static List<long> IcositetragonalNumbers
    {
        get
        {
            return PolygonalNumbers(24);
        }
    }
    public static bool IsTriangularNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(3).Contains(number));
    }
    public static bool IsQuadlateralNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(4).Contains(number));
    }
    public static bool IsPentagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(5).Contains(number));
    }
    public static bool IsHexagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(6).Contains(number));
    }
    public static bool IsHeptagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(7).Contains(number));
    }
    public static bool IsOctagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(8).Contains(number));
    }
    public static bool IsNonagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(9).Contains(number));
    }
    public static bool IsDecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(10).Contains(number));
    }
    public static bool IsHendecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(11).Contains(number));
    }
    public static bool IsDodecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(12).Contains(number));
    }
    public static bool IsTridecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(13).Contains(number));
    }
    public static bool IsTetradecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(14).Contains(number));
    }
    public static bool IsPentadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(15).Contains(number));
    }
    public static bool IsHexadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(16).Contains(number));
    }
    public static bool IsHeptadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(17).Contains(number));
    }
    public static bool IsOctadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(18).Contains(number));
    }
    public static bool IsNonadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(19).Contains(number));
    }
    public static bool IsIcosagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(20).Contains(number));
    }
    public static bool IsIcosihenagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(21).Contains(number));
    }
    public static bool IsIcosidigonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(22).Contains(number));
    }
    public static bool IsIcositrigonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(23).Contains(number));
    }
    public static bool IsIcositetragonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PolygonalNumbers(24).Contains(number));
    }

    //https://en.wikipedia.org/wiki/Centered_polygonal_number
    // ith number of CenteredPolygonal(sides=N) = (((N * i)/2) * (i-1)) + 1
    // Whereas a prime number p cannot be a polygonal number, many centered polygonal numbers are primes.
    private static Dictionary<int, List<long>> s_centered_polygonal_numbers_dictionary = new Dictionary<int, List<long>>();
    public static List<long> CenteredPolygonalNumbers(int sides)
    {
        if (!s_centered_polygonal_numbers_dictionary.ContainsKey(sides))
        {
            GenerateCenteredPolygonalNumbers(sides);
        }

        if (s_centered_polygonal_numbers_dictionary.ContainsKey(sides))
        {
            return s_centered_polygonal_numbers_dictionary[sides];
        }
        else
        {
            return null;
        }
    }
    private static void GenerateCenteredPolygonalNumbers(int sides)
    {
        List<long> centered_polygonal_numbers = new List<long>(MAX_SERIES_LENGTH);
        for (int n = 1; n <= MAX_SERIES_LENGTH; n++)
        {
            long number = (int)(((sides * n) / 2.0D) * (n - 1)) + 1L;
            centered_polygonal_numbers.Add(number);
        }
        s_centered_polygonal_numbers_dictionary.Add(sides, centered_polygonal_numbers);
    }
    public static bool IsCenteredPolygonalNumber(int sides, long number)
    {
        if (s_centered_polygonal_numbers_dictionary.ContainsKey(sides))
        {
            return (s_centered_polygonal_numbers_dictionary[sides].Contains(number));
        }
        return false;
    }
    public static List<long> CenteredTriangularNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(3);
        }
    }
    public static List<long> CenteredSquareNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(4);
        }
    }
    public static List<long> CenteredPentagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(5);
        }
    }
    public static List<long> CenteredHexagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(6);
        }
    }
    public static List<long> CenteredHeptagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(7);
        }
    }
    public static List<long> CenteredOctagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(8);
        }
    }
    public static List<long> CenteredNonagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(9);
        }
    }
    public static List<long> CenteredDecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(10);
        }
    }
    public static List<long> CenteredHendecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(11);
        }
    }
    public static List<long> CenteredDodecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(12);
        }
    }
    public static List<long> CenteredTridecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(13);
        }
    }
    public static List<long> CenteredTetradecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(14);
        }
    }
    public static List<long> CenteredPentadecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(15);
        }
    }
    public static List<long> CenteredHexadecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(16);
        }
    }
    public static List<long> CenteredHeptadecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(17);
        }
    }
    public static List<long> CenteredOctadecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(18);
        }
    }
    public static List<long> CenteredNonadecagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(19);
        }
    }
    public static List<long> CenteredIcosagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(20);
        }
    }
    public static List<long> CenteredIcosihenagonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(21);
        }
    }
    public static List<long> CenteredIcosidigonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(22);
        }
    }
    public static List<long> CenteredIcositrigonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(23);
        }
    }
    public static List<long> CenteredIcositetragonalNumbers
    {
        get
        {
            return CenteredPolygonalNumbers(24);
        }
    }
    public static bool IsCenteredTriangularNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(3).Contains(number));
    }
    public static bool IsCenteredSquareNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(4).Contains(number));
    }
    public static bool IsCenteredPentagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(5).Contains(number));
    }
    public static bool IsCenteredHexagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(6).Contains(number));
    }
    public static bool IsCenteredHeptagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(7).Contains(number));
    }
    public static bool IsCenteredOctagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(8).Contains(number));
    }
    public static bool IsCenteredNonagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(9).Contains(number));
    }
    public static bool IsCenteredDecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(10).Contains(number));
    }
    public static bool IsCenteredHendecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(11).Contains(number));
    }
    public static bool IsCenteredDodecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(12).Contains(number));
    }
    public static bool IsCenteredTridecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(13).Contains(number));
    }
    public static bool IsCenteredTetradecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(14).Contains(number));
    }
    public static bool IsCenteredPentadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(15).Contains(number));
    }
    public static bool IsCenteredHexadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(16).Contains(number));
    }
    public static bool IsCenteredHeptadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(17).Contains(number));
    }
    public static bool IsCenteredOctadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(18).Contains(number));
    }
    public static bool IsCenteredNonadecagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(19).Contains(number));
    }
    public static bool IsCenteredIcosagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(20).Contains(number));
    }
    public static bool IsCenteredIcosihenagonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(21).Contains(number));
    }
    public static bool IsCenteredIcosidigonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(22).Contains(number));
    }
    public static bool IsCenteredIcositrigonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(23).Contains(number));
    }
    public static bool IsCenteredIcositetragonalNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredPolygonalNumbers(24).Contains(number));
    }

    //https://en.wikipedia.org/wiki/Pyramidal_number
    //r-gonal pyramidal numbers = a(r, n) = ( 3*n*n + n*n*n*(r-2) - n*(r-5) ) / 6
    //A000292    Triangular pyramidal numbers: a(n) = n*(n+1)*(n+2)/6
    //1, 4, 10, 20, 35, 56, 84, 120, 165, 220, 286, 364, 455, 560, 680, 816, 969, 1140, 1330, 1540, 1771, 2024, 2300, 2600, 2925, 3276, 3654, 4060, 4495, 4960, 5456, 5984, 6545, 7140, 7770, 8436, 9139, 9880, 10660, 11480, 12341, 13244, 14190, 15180
    //A000330    Square pyramidal numbers: a(n) = n*(n+1)*(2*n+1)/6
    //1, 5, 14, 30, 55, 91, 140, 204, 285, 385, 506, 650, 819, 1015, 1240, 1496, 1785, 2109, 2470, 2870, 3311, 3795, 4324, 4900, 5525, 6201, 6930, 7714, 8555, 9455, 10416, 11440, 12529, 13685, 14910, 16206, 17575, 19019, 20540, 22140, 23821, 25585, 27434, 29370
    //A002411    Pentagonal pyramidal numbers: a(n) = n^2*(n+1)/2
    //1, 6, 18, 40, 75, 126, 196, 288, 405, 550, 726, 936, 1183, 1470, 1800, 2176, 2601, 3078, 3610, 4200, 4851, 5566, 6348, 7200, 8125, 9126, 10206, 11368, 12615, 13950, 15376, 16896, 18513, 20230, 22050, 23976, 26011, 28158, 30420, 32800, 35301, 37926, 40678
    //A002412    Hexagonal pyramidal numbers: a(n) = n*(n+1)*(4n-1)/6 
    //1, 7, 22, 50, 95, 161, 252, 372, 525, 715, 946, 1222, 1547, 1925, 2360, 2856, 3417, 4047, 4750, 5530, 6391, 7337, 8372, 9500, 10725, 12051, 13482, 15022, 16675, 18445, 20336, 22352, 24497, 26775, 29190, 31746, 34447, 37297, 40300
    //A002413    Heptagonal pyramidal numbers: a(n) = n*(n+1)*(5*n-2)/6
    //1, 8, 26, 60, 115, 196, 308, 456, 645, 880, 1166, 1508, 1911, 2380, 2920, 3536, 4233, 5016, 5890, 6860, 7931, 9108, 10396, 11800, 13325, 14976, 16758, 18676, 20735, 22940, 25296, 27808, 30481, 33320, 36330, 39516, 42883, 46436, 50180, 54120
    //A002414    Octagonal pyramidal numbers: a(n) = n*(n+1)*(2*n-1)/2
    //1, 9, 30, 70, 135, 231, 364, 540, 765, 1045, 1386, 1794, 2275, 2835, 3480, 4216, 5049, 5985, 7030, 8190, 9471, 10879, 12420, 14100, 15925, 17901, 20034, 22330, 24795, 27435, 30256, 33264, 36465, 39865, 43470, 47286, 51319, 55575, 60060, 64780
    //A007584    Enneagonal pyramidal numbers: a(n) = n*(n+1)*(7*n-4)/6
    //1, 10, 34, 80, 155, 266, 420, 624, 885, 1210, 1606, 2080, 2639, 3290, 4040, 4896, 5865, 6954, 8170, 9520, 11011, 12650, 14444, 16400, 18525, 20826, 23310, 25984, 28855, 31930, 35216, 38720, 42449, 46410, 50610, 55056, 59755, 64714, 69940, 75440, 81221
    //A007585    Decagonal pyramidal numbers: a(n) = n*(n + 1)*(8*n - 5)/6
    //1, 11, 38, 90, 175, 301, 476, 708, 1005, 1375, 1826, 2366, 3003, 3745, 4600, 5576, 6681, 7923, 9310, 10850, 12551, 14421, 16468, 18700, 21125, 23751, 26586, 29638, 32915, 36425, 40176, 44176, 48433, 52955, 57750
    //A007586    Hendecagonal pyramidal numbers: n*(n+1)*(3*n-2)/2
    //1, 12, 42, 100, 195, 336, 532, 792, 1125, 1540, 2046, 2652, 3367, 4200, 5160, 6256, 7497, 8892, 10450, 12180, 14091, 16192, 18492, 21000, 23725, 26676, 29862, 33292, 36975, 40920, 45136, 49632, 54417, 59500, 64890, 70596, 76627, 82992, 89700, 96760, 104181
    //A007587    Dodecagonal pyramidal numbers: n*(n+1)*(10*n-7)/6
    //1, 13, 46, 110, 215, 371, 588, 876, 1245, 1705, 2266, 2938, 3731, 4655, 5720, 6936, 8313, 9861, 11590, 13510, 15631, 17963, 20516, 23300, 26325, 29601, 33138, 36946, 41035, 45415, 50096, 55088, 60401, 66045, 72030, 78366, 85063, 92131, 99580, 107420, 115661
    private static Dictionary<int, List<long>> s_pyramidal_numbers_dictionary = new Dictionary<int, List<long>>();
    public static List<long> PyramidalNumbers(int sides)
    {
        if (!s_pyramidal_numbers_dictionary.ContainsKey(sides))
        {
            GeneratePyramidalNumbers(sides);
        }

        if (s_pyramidal_numbers_dictionary.ContainsKey(sides))
        {
            return s_pyramidal_numbers_dictionary[sides];
        }
        else
        {
            return null;
        }
    }
    private static void GeneratePyramidalNumbers(int sides)
    {
        List<long> pyramidal_numbers = new List<long>(MAX_SERIES_LENGTH);
        for (int n = 1; n <= MAX_SERIES_LENGTH; n++)
        {
            long number = (3 * n * n + n * n * n * (sides - 2) - n * (sides - 5)) / 6;
            pyramidal_numbers.Add(number);
        }
        s_pyramidal_numbers_dictionary.Add(sides, pyramidal_numbers);
    }
    public static bool IsPyramidalNumber(int sides, long number)
    {
        if (number < 0L) number *= -1L;

        if (s_pyramidal_numbers_dictionary.ContainsKey(sides))
        {
            return (s_pyramidal_numbers_dictionary[sides].Contains(number));
        }
        return false;
    }
    public static List<long> TriangularPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(3);
        }
    }
    public static List<long> SquarePyramidNumbers
    {
        get
        {
            return PyramidalNumbers(4);
        }
    }
    public static List<long> PentagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(5);
        }
    }
    public static List<long> HexagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(6);
        }
    }
    public static List<long> HeptagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(7);
        }
    }
    public static List<long> OctagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(8);
        }
    }
    public static List<long> NonagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(9);
        }
    }
    public static List<long> DecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(10);
        }
    }
    public static List<long> HendecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(11);
        }
    }
    public static List<long> DodecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(12);
        }
    }
    public static List<long> TridecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(13);
        }
    }
    public static List<long> TetradecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(14);
        }
    }
    public static List<long> PentadecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(15);
        }
    }
    public static List<long> HexadecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(16);
        }
    }
    public static List<long> HeptadecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(17);
        }
    }
    public static List<long> OctadecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(18);
        }
    }
    public static List<long> NonadecagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(19);
        }
    }
    public static List<long> IcosagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(20);
        }
    }
    public static List<long> IcosihenagonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(21);
        }
    }
    public static List<long> IcosidigonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(22);
        }
    }
    public static List<long> IcositrigonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(23);
        }
    }
    public static List<long> IcositetragonalPyramidNumbers
    {
        get
        {
            return PyramidalNumbers(24);
        }
    }
    public static bool IsTriangularPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(3).Contains(number));
    }
    public static bool IsSquarePyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(4).Contains(number));
    }
    public static bool IsPentagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(5).Contains(number));
    }
    public static bool IsHexagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(6).Contains(number));
    }
    public static bool IsHeptagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(7).Contains(number));
    }
    public static bool IsOctagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(8).Contains(number));
    }
    public static bool IsNonagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(9).Contains(number));
    }
    public static bool IsDecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(10).Contains(number));
    }
    public static bool IsHendecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(11).Contains(number));
    }
    public static bool IsDodecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(12).Contains(number));
    }
    public static bool IsTridecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(13).Contains(number));
    }
    public static bool IsTetradecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(14).Contains(number));
    }
    public static bool IsPentadecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(15).Contains(number));
    }
    public static bool IsHexadecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(16).Contains(number));
    }
    public static bool IsHeptadecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(17).Contains(number));
    }
    public static bool IsOctadecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(18).Contains(number));
    }
    public static bool IsNonadecagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(19).Contains(number));
    }
    public static bool IsIcosagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(20).Contains(number));
    }
    public static bool IsIcosihenagonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(21).Contains(number));
    }
    public static bool IsIcosidigonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(22).Contains(number));
    }
    public static bool IsIcositrigonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(23).Contains(number));
    }
    public static bool IsIcositetragonalPyramidNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (PyramidalNumbers(24).Contains(number));
    }

    //https://en.wikipedia.org/wiki/Platonic_solid
    //https://en.wikipedia.org/wiki/Centered_polyhedral_number
    // 4 Faces: Centered tetrahedral  numbers
    // 6 Faces: Centered hexahedron   numbers / Centered cube numbers
    // 8 Faces: Centered octahedral   numbers
    //12 Faces: Centered dodecahedral numbers
    //20 Faces: Centered icosahedral  numbers

    //Centered Tetrahedral Numbers
    //https://en.wikipedia.org/wiki/Centered_tetrahedral_number
    //https://oeis.org/A005894
    //(2*n+1)*(n^2+n+3)/3
    //1, 5, 15, 35, 69, 121, 195, 295, 425, 589, 791, 1035, 1325, 1665, 2059, 2511, 3025, 3605, 4255, 4979, 5781, 6665, 7635, 8695, 9849, 11101, 12455, 13915, 15485, 17169, 18971, 20895, 22945, 25125, 27439, 29891, 32485, 35225, 38115, ...
    //Centered Hexahedron Numbers / Centered Cube Numbers
    //https://en.wikipedia.org/wiki/Centered_cube_number
    //https://oeis.org/A005898
    //n^3 + (n+1)^3
    //1, 9, 35, 91, 189, 341, 559, 855, 1241, 1729, 2331, 3059, 3925, 4941, 6119, 7471, 9009, 10745, 12691, 14859, 17261, 19909, 22815, 25991, 29449, 33201, 37259, 41635, 46341, 51389, 56791, 62559, 68705, 75241, 82179, 89531, 97309, 105525, ...
    //Centered Octahedral Numbers
    //https://en.wikipedia.org/wiki/Centered_octahedral_number
    //https://oeis.org/A001845
    //(2*n+1)*(2*n^2 + 2*n + 3)/3
    //1, 7, 25, 63, 129, 231, 377, 575, 833, 1159, 1561, 2047, 2625, 3303, 4089, 4991, 6017, 7175, 8473, 9919, 11521, 13287, 15225, 17343, 19649, 22151, 24857, 27775, 30913, 34279, 37881, 41727, 45825, 50183, 54809, 59711, 64897, 70375, 76153, 82239, ...
    //Centered Dodecahedral Numbers
    //https://en.wikipedia.org/wiki/Centered_dodecahedral_number
    //https://oeis.org/A005904
    //(2*n+1)*(5*n^2+5*n+1)
    //1, 33, 155, 427, 909, 1661, 2743, 4215, 6137, 8569, 11571, 15203, 19525, 24597, 30479, 37231, 44913, 53585, 63307, 74139, 86141, 99373, 113895, 129767, 147049, 165801, 186083, 207955, 231477, 256709, 283711, 312543, 343265, 375937, 410619, 447371, ...
    //Centered Icosahedral Numbers
    //https://en.wikipedia.org/wiki/Centered_icosahedral_number
    //https://oeis.org/A005902
    //(2*n+1)*(5*n^2+5*n+3)/3
    //1, 13, 55, 147, 309, 561, 923, 1415, 2057, 2869, 3871, 5083, 6525, 8217, 10179, 12431, 14993, 17885, 21127, 24739, 28741, 33153, 37995, 43287, 49049, 55301, 62063, 69355, 77197, 85609, 94611, 104223, 114465, 125357, 136919, 149171, ...
    private static List<long> s_polyhedral_4_faces = null;
    private static List<long> s_polyhedral_6_faces = null;
    private static List<long> s_polyhedral_8_faces = null;
    private static List<long> s_polyhedral_12_faces = null;
    private static List<long> s_polyhedral_20_faces = null;
    public static List<long> CenteredTetrahedralNumbers
    {
        get
        {
            if (s_polyhedral_4_faces == null)
            {
                s_polyhedral_4_faces = new List<long>(MAX_SERIES_LENGTH);
                for (int n = 0; n < MAX_SERIES_LENGTH; n++)
                {
                    long number = ((2 * n + 1) * ((n * n) + n + 3)) / 3;
                    s_polyhedral_4_faces.Add(number);
                }
            }
            return s_polyhedral_4_faces;
        }
    }
    public static List<long> CenteredHexahedronNumbers
    {
        get
        {
            if (s_polyhedral_6_faces == null)
            {
                s_polyhedral_6_faces = new List<long>(MAX_SERIES_LENGTH);
                for (int n = 0; n < MAX_SERIES_LENGTH; n++)
                {
                    long number = (n * n * n) + ((n + 1) * (n + 1) * (n + 1));
                    s_polyhedral_6_faces.Add(number);
                }
            }
            return s_polyhedral_6_faces;
        }
    }
    public static List<long> CenteredOctahedralNumbers
    {
        get
        {
            if (s_polyhedral_8_faces == null)
            {
                s_polyhedral_8_faces = new List<long>(MAX_SERIES_LENGTH);
                for (int n = 0; n < MAX_SERIES_LENGTH; n++)
                {
                    long number = (2 * n + 1) * (2 * (n * n) + 2 * n + 3) / 3;
                    s_polyhedral_8_faces.Add(number);
                }
            }
            return s_polyhedral_8_faces;
        }
    }
    public static List<long> CenteredDodecahedralNumbers
    {
        get
        {
            if (s_polyhedral_12_faces == null)
            {
                s_polyhedral_12_faces = new List<long>(MAX_SERIES_LENGTH);
                for (int n = 0; n < MAX_SERIES_LENGTH; n++)
                {
                    long number = (2 * n + 1) * (5 * (n * n) + 5 * n + 1);
                    s_polyhedral_12_faces.Add(number);
                }
            }
            return s_polyhedral_12_faces;
        }
    }
    public static List<long> CenteredIcosahedralNumbers
    {
        get
        {
            if (s_polyhedral_20_faces == null)
            {
                s_polyhedral_20_faces = new List<long>(MAX_SERIES_LENGTH);
                for (int n = 0; n < MAX_SERIES_LENGTH; n++)
                {
                    long number = (2 * n + 1) * (5 * (n * n) + 5 * n + 3) / 3;
                    s_polyhedral_20_faces.Add(number);
                }
            }
            return s_polyhedral_20_faces;
        }
    }
    public static bool IsCenteredTetrahedralNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredTetrahedralNumbers.Contains(number));
    }
    public static bool IsCenteredHexahedronNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredHexahedronNumbers.Contains(number));
    }
    public static bool IsCenteredOctahedralNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredOctahedralNumbers.Contains(number));
    }
    public static bool IsCenteredDodecahedralNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredDodecahedralNumbers.Contains(number));
    }
    public static bool IsCenteredIcosahedralNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CenteredIcosahedralNumbers.Contains(number));
    }

    // Chemical polyhex hydrocarbons C_(2h) with 19 hexagons
    // https://oeis.org/A120386
    // Number of isomers of polyhex hydrocarbons with C_(2h) symmetry with nineteen hexagons
    // 3, 17, 66, 189, 589, 1677, 3829, 7948, 15649, 25543, 26931, 15472 
    private static List<long> c2h_polyhex_hydrocarbon_numbers = null;
    public static List<long> C2hPolyhexHydrocarbonNumbers
    {
        get
        {
            if (c2h_polyhex_hydrocarbon_numbers == null)
            {
                GenerateC2hPolyhexHydrocarbonNumbers();
            }
            return c2h_polyhex_hydrocarbon_numbers;
        }
    }
    private static void GenerateC2hPolyhexHydrocarbonNumbers()
    {
        c2h_polyhex_hydrocarbon_numbers = new List<long>()
        {
            3, 17, 66, 189, 589, 1677, 3829, 7948, 15649, 25543, 26931, 15472
        };
    }
    public static bool IsC2hPolyhexHydrocarbonNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return C2hPolyhexHydrocarbonNumbers.Contains(number);
    }

    // Chemical polyhex hydrocarbons C_(2v) with 19 hexagons
    // https://oeis.org/A120448
    // Number of isomers of polyhex hydrocarbons with C_(2v) symmetry with nineteen hexagons
    // 3, 17, 14, 92, 60, 316, 175, 814, 495, 2323, 1402, 6037, 3113, 12851, 6200, 24710, 11851, 46152, 18123, 72151, 18007, 74547, 8970, 40141
    private static List<long> c2v_polyhex_hydrocarbon_numbers = null;
    public static List<long> C2vPolyhexHydrocarbonNumbers
    {
        get
        {
            if (c2v_polyhex_hydrocarbon_numbers == null)
            {
                GenerateC2vPolyhexHydrocarbonNumbers();
            }
            return c2v_polyhex_hydrocarbon_numbers;
        }
    }
    private static void GenerateC2vPolyhexHydrocarbonNumbers()
    {
        c2v_polyhex_hydrocarbon_numbers = new List<long>()
        {
            3, 17, 14, 92, 60, 316, 175, 814, 495, 2323, 1402, 6037, 3113, 12851, 6200, 24710, 11851, 46152, 18123, 72151, 18007, 74547, 8970, 40141
        };
    }
    public static bool IsC2vPolyhexHydrocarbonNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return C2vPolyhexHydrocarbonNumbers.Contains(number);
    }

    private static List<long> s_linear_numbers;
    public static List<long> Linear
    {
        get
        {
            if (s_linear_numbers == null)
            {
                GenerateLinearNumbers();
            }
            return s_linear_numbers;
        }
    }
    private static void GenerateLinearNumbers()
    {
        s_linear_numbers = new List<long>();
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_linear_numbers.Add(i + 1);
        };
    }
    /// <summary>
    /// Linear numbers are natural numbers from 1 to 1024
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsLinearNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (Linear.Contains(number));
    }

    //Collatz Conjecture: Starting with any positive integer n, do 3n+1 if n=Odd, do n/2 if n=Even, if continue this process, n must reach 1
    //Collatz Numbers = Squence of numbers with increasing steps to reach 1
    //1, 2, 3, 6, 7, 9, 18, 25, 27, 54, 73, 97, 129, 171, 231, 313, 327, 649, 703, 871, 1161, 2223, 2463, 2919, 3711, 6171, 10971, 13255, 17647, 23529, 26623, 34239, 35655, 52527, 77031, 106239, 142587, 156159, 216367, 230631, 410011, 511935, 626331, 837799, ...
    private static List<long> s_collatz_numbers;
    public static List<long> CollatzNumbers
    {
        get
        {
            if (s_collatz_numbers == null)
            {
                GenerateCollatzNumbers();
            }
            return s_collatz_numbers;
        }
    }
    private static void GenerateCollatzNumbers()
    {
        s_collatz_numbers = new List<long>()
        {
            1, 2, 3, 6, 7, 9, 18, 25, 27, 54, 73, 97, 129, 171, 231, 313,
            327, 649, 703, 871, 1161, 2223, 2463, 2919, 3711, 6171, 10971,
            13255, 17647, 23529, 26623, 34239, 35655, 52527, 77031, 106239,
            142587, 156159, 216367, 230631, 410011, 511935, 626331, 837799
        };
    }
    /// <summary>
    /// <para>Collatz Conjecture</para>
    /// <para>Starting with any positive integer n, do 3n+1 if n=Odd, do n/2 if n=Even, if continue this process, n must reach 1</para>
    /// <para>Collatz Numbers = Squence of numbers with increasing steps to reach 1</para>
    /// <para>1, 2, 3, 6, 7, 9, 18, 25, 27, 54, 73, 97, 129, 171, 231, 313, 327, 649, 703, 871, 1161, 2223, 2463, 2919, 3711, 6171, 10971,</para>
    /// <para>13255, 17647, 23529, 26623, 34239, 35655, 52527, 77031, 106239, 142587, 156159, 216367, 230631, 410011, 511935, 626331, 837799, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsCollatzNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return (CollatzNumbers.Contains(number));
    }

    private static List<long> s_gematria_number;
    public static List<long> GematriaNumbers
    {
        get
        {
            if (s_gematria_number == null)
            {
                GenerateGematriaNumbers();
            }
            return s_gematria_number;
        }
    }
    private static void GenerateGematriaNumbers()
    {
        s_gematria_number = new List<long>()
        {
                1,     2,     3,     4,     5,     6,     7,     8,     9,
                10,    20,    30,    40,    50,    60,    70,    80,    90,
                100,   200,   300,   400,   500,   600,   700,   800,   900,
                1000,  2000,  3000,  4000,  5000,  6000,  7000,  8000,  9000,
                10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000,
                100000
        };
    }
    /// <summary>
    /// <para>Gematria numbers are linear-then-log-10 integers</para>
    /// <para>1, 2, 3, 4, 5, 6, 7, 8, 9</para>
    /// <para>10, 20, 30, 40, 50, 60, 70, 80, 90</para>
    /// <para>100, 200, 300, 400, 500, 600, 700, 800, 900</para>
    /// <para>1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000</para>
    /// <para>10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000</para>
    /// <para>100000</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsGematriaNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return GematriaNumbers.Contains(number);
    }

    private static List<long> s_quran_numbers;
    public static List<long> QuranNumbers
    {
        get
        {
            if (s_quran_numbers == null)
            {
                GenerateQuranNumbers();
            }
            return s_quran_numbers;
        }
    }
    private static void GenerateQuranNumbers()
    {
        s_quran_numbers = new List<long>()
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 19, 20, 30, 40, 50, 60, 70, 80, 99, 100, 200, 300, 1000, 2000, 3000, 5000, 50000, 100000
        };
    }
    /// <summary>
    /// <para>Quran Number is a whole number used in the Quran</para>
    /// <para>1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 19,</para>
    /// <para>20, 30, 40, 50, 60, 70, 80, 99, 100, 200, 300,</para>
    /// <para>1000, 2000, 3000, 5000, 50000, 100000</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsQuranNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return QuranNumbers.Contains(number);
    }

    private static List<long> s_merciful_numbers;
    public static List<long> MercifulNumbers
    {
        get
        {
            if (s_merciful_numbers == null)
            {
                GenerateMercifulNumbers();
            }
            return s_merciful_numbers;
        }
    }
    private static void GenerateMercifulNumbers()
    {
        s_merciful_numbers = new List<long>()
        {
            13, 16, 18, 21, 23, 25, 28, 30, 32, 34, 36, 38, 40, 42, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75, 77
        };
    }
    /// <summary>
    /// <para>Merciful Number is one of 31 verse numbers of the most repeated verse in the Quran</para>
    /// <para>13, 16, 18, 21, 23, 25, 28, 30, 32, 34, 36, 38, 40, 42, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75, 77</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsMercifulNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return MercifulNumbers.Contains(number);
    }

    private static List<long> s_mersenne_prime_exponents;
    public static List<long> MersennePrimeExponents
    {
        get
        {
            if (s_mersenne_prime_exponents == null)
            {
                GenerateMersennePrimeExponents();
            }
            return s_mersenne_prime_exponents;
        }
    }
    private static void GenerateMersennePrimeExponents()
    {
        s_mersenne_prime_exponents = new List<long>()
        {
            2, 3, 5, 7, 13, 17, 19, 31, 61, 89, 107, 127, 521, 607, 1279, 2203, 2281, 3217, 4253, 4423, 9689, 9941, 11213, 19937, 21701, 23209, 44497, 86243, 110503, 132049, 216091, 756839, 859433, 1257787, 1398269, 2976221, 3021377, 6972593, 13466917, 20996011, 24036583, 25964951, 30402457, 32582657, 37156667, 42643801, 43112609, 57885161, 74207281, 77232917, 82589933
        };
    }
    /// <summary>
    /// <para>Mersenne Prime is 2^p - 1 for some prime p exponent power</para>
    /// <para>the first 51 known Mersenne powers in 2021 are:</para>
    /// <para>https://en.wikipedia.org/wiki/Mersenne_prime</para>
    /// <para>https://oeis.org/A000043</para>
    /// <para>2, 3, 5, 7, 13, 17, 19, 31, 61, 89, 107, 127, 521, 607, 1279, 2203, 2281, 3217,</para>
    /// <para>4253, 4423, 9689, 9941, 11213, 19937, 21701, 23209, 44497, 86243, 110503, 132049,</para>
    /// <para>216091, 756839, 859433, 1257787, 1398269, 2976221, 3021377, 6972593, 13466917,</para>
    /// <para>20996011, 24036583, 25964951, 30402457, 32582657, 37156667, 42643801, 43112609, 57885161,</para>
    /// <para>74207281, 77232917, 82589933</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsMersennePrimeExponent(long number)
    {
        if (number < 0L) number *= -1L;

        return MersennePrimeExponents.Contains(number);
    }

    private static List<long> s_partition_numbers = null;
    public static List<long> PartitionNumbers
    {
        get
        {
            if (s_partition_numbers == null)
            {
                LoadPartitionNumbers();
            }
            return s_partition_numbers;
        }
    }
    public static int PartitionNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_partition_numbers == null)
        {
            LoadPartitionNumbers();
        }
        return BinarySearch(s_partition_numbers, number);
    }
    public static int PartitionNumberIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return PartitionNumberIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    private static string s_partition_numbers_path = "partition_numbers.txt";
    private static void LoadPartitionNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_partition_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_partition_numbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_partition_numbers.Add(number);
                        }
                    }
                }
            }
        }
    }

    private static List<long> s_smith_numbers = null;
    public static List<long> SmithNumbers
    {
        get
        {
            if (s_smith_numbers == null)
            {
                LoadSmithNumbers();
            }
            return s_smith_numbers;
        }
    }
    public static int SmithNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_smith_numbers == null)
        {
            LoadSmithNumbers();
        }
        return BinarySearch(s_smith_numbers, number);
    }
    public static int SmithNumberIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return SmithNumberIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    /// <summary>
    /// Smith number is a composite number for which the sum of its digits is equal to the sum of the digits in its prime factors.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsSmithNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return SmithNumbers.Contains(number);
    }
    private static string s_smith_numbers_path = "smith_numbers.txt";
    private static void LoadSmithNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_smith_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_smith_numbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_smith_numbers.Add(number);
                        }
                    }
                }
            }
        }
    }

    private static List<long> s_carmichael_numbers = null;
    public static List<long> CarmichaelNumbers
    {
        get
        {
            if (s_carmichael_numbers == null)
            {
                LoadCarmichaelNumbers();
            }
            return s_carmichael_numbers;
        }
    }
    public static int CarmichaelNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_carmichael_numbers == null)
        {
            LoadCarmichaelNumbers();
        }
        return BinarySearch(s_carmichael_numbers, number);
    }
    public static int CarmichaelNumberIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return CarmichaelNumberIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    /// <summary>
    /// Carmichael number is a composite number n which satisfies the modular arithmetic congruence relation:
    /// b^(n-1) == 1 mod(n) for all integers b which are relatively prime to n.
    /// 561, 1105, 1729, 2465, 2821, 6601, 8911, 10585, 15841, 29341, 41041,
    /// 46657, 52633, 62745, 63973, 75361, 101101, 115921, 126217, 162401, 172081, 188461,
    /// 252601, 278545, 294409, 314821, 334153, 340561, 399001, 410041, 449065, 488881, 512461
    /// Note: Sum(1..286) = 41041 = 11th Carmichael number.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsCarmichaelNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return CarmichaelNumbers.Contains(number);
    }
    private static string s_carmichael_numbers_path = "carmichael_numbers.txt";
    private static void LoadCarmichaelNumbers()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_carmichael_numbers_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_carmichael_numbers = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_carmichael_numbers.Add(number);
                        }
                    }
                }
            }
        }
    }

    private static List<long> s_ramanujan_primes = null;
    public static List<long> RamanujanPrimes
    {
        get
        {
            if (s_ramanujan_primes == null)
            {
                LoadRamanujanPrimes();
            }
            return s_ramanujan_primes;
        }
    }
    public static int RamanujanPrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_ramanujan_primes == null)
        {
            LoadRamanujanPrimes();
        }
        return BinarySearch(s_ramanujan_primes, number);
    }
    public static int RamanujanPrimeIndexOf(string text, int radix)
    {
        try
        {
            long number = Radix.Decode(text, radix);
            return RamanujanPrimeIndexOf(number);
        }
        catch
        {
            return -1;
        }
    }
    /// <summary>
    /// <para>Ramanujan primes R_n: a(n) is the smallest number such that if x >= a(n),</para>
    /// <para>then pi(x) - pi(x/2) >= n, where pi(x) is the number of primes <= x.</para>
    /// <para>https://oeis.org/A104272</para>
    /// <para>2, 11, 17, 29, 41, 47, 59, 67, 71, 97, 101, 107, 127, 149, 151, 167, 179,</para>
    /// <para>181, 227, 229, 233, 239, 241, 263, 269, 281, 307, 311, 347, 349, 367, 373, 401,</para>
    /// <para>409, 419, 431, 433, 439, 461, 487, 491, 503, 569, 571, 587, 593, 599, 601, 607,</para>
    /// <para>641, 643, 647, 653, 659, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsRamanujanPrime(long number)
    {
        if (number < 0L) number *= -1L;

        return RamanujanPrimes.Contains(number);
    }
    private static string s_ramanujan_primes_path = "ramanujan_primes.txt";
    private static void LoadRamanujanPrimes()
    {
        string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + s_ramanujan_primes_path;
        if (File.Exists(path))
        {
            FileHelper.WaitForFileReady(path);

            s_ramanujan_primes = new List<long>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line = null;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        long number;
                        if (long.TryParse(line, out number))
                        {
                            s_ramanujan_primes.Add(number);
                        }
                    }
                }
            }
        }
    }

    private static List<long> s_fibonacci_numbers;
    public static List<long> FibonacciNumbers
    {
        get
        {
            if (s_fibonacci_numbers == null)
            {
                GenerateFibonacciNumbers();
            }
            return s_fibonacci_numbers;
        }
    }
    public static int FibonacciNumberIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_fibonacci_numbers == null)
        {
            GenerateFibonacciNumbers();
        }
        return BinarySearch(s_fibonacci_numbers, number);
    }
    private static void GenerateFibonacciNumbers()
    {
        s_fibonacci_numbers = new List<long>(MAX_SERIES_LENGTH);
        s_fibonacci_numbers.Add(1); // 0th item
        s_fibonacci_numbers.Add(1); // 1st item
        for (int i = 2; i < MAX_SERIES_LENGTH; i++)
        {
            long number = s_fibonacci_numbers[i - 1] + s_fibonacci_numbers[i - 2];
            s_fibonacci_numbers.Add(number);
        }
    }
    /// <summary>
    /// <para>Fibonacci number is the sum of previous two Fibonacci numbers</para>
    /// <para>1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsFibonacciNumber(long number)
    {
        if (number < 0L) number *= -1L;

        return FibonacciNumbers.Contains(number);
    }

    // number + index
    private static List<long> s_prime_plus_indexes;
    public static List<long> PrimePlusIndexes
    {
        get
        {
            if (s_prime_plus_indexes == null)
            {
                GeneratePrimePlusIndexes();
            }
            return s_prime_plus_indexes;
        }
    }
    private static void GeneratePrimePlusIndexes()
    {
        s_prime_plus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_prime_plus_indexes.Add(s_primes[i] + (i + 1));
        }
    }
    public static bool IsPrimePlusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return PrimePlusIndexes.Contains(number);
    }

    private static List<long> s_additive_prime_plus_indexes;
    public static List<long> AdditivePrimePlusIndexes
    {
        get
        {
            if (s_additive_prime_plus_indexes == null)
            {
                GenerateAdditivePrimePlusIndexes();
            }
            return s_additive_prime_plus_indexes;
        }
    }
    private static void GenerateAdditivePrimePlusIndexes()
    {
        s_additive_prime_plus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_additive_prime_plus_indexes.Add(s_additive_primes[i] + (i + 1));
        }
    }
    public static bool IsAdditivePrimePlusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return AdditivePrimePlusIndexes.Contains(number);
    }

    private static List<long> s_non_additive_prime_plus_indexes;
    public static List<long> NonAdditivePrimePlusIndexes
    {
        get
        {
            if (s_non_additive_prime_plus_indexes == null)
            {
                GenerateNonAdditivePrimePlusIndexes();
            }
            return s_non_additive_prime_plus_indexes;
        }
    }
    private static void GenerateNonAdditivePrimePlusIndexes()
    {
        s_non_additive_prime_plus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_non_additive_prime_plus_indexes.Add(s_non_additive_primes[i] + (i + 1));
        }
    }
    public static bool IsNonAdditivePrimePlusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return NonAdditivePrimePlusIndexes.Contains(number);
    }

    private static List<long> s_composite_plus_indexes;
    public static List<long> CompositePlusIndexes
    {
        get
        {
            if (s_composite_plus_indexes == null)
            {
                GenerateCompositePlusIndexes();
            }
            return s_composite_plus_indexes;
        }
    }
    private static void GenerateCompositePlusIndexes()
    {
        s_composite_plus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_composite_plus_indexes.Add(s_composites[i] + (i + 1));
        }
    }
    public static bool IsCompositePlusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return CompositePlusIndexes.Contains(number);
    }

    private static List<long> s_additive_composite_plus_indexes;
    public static List<long> AdditiveCompositePlusIndexes
    {
        get
        {
            if (s_additive_composite_plus_indexes == null)
            {
                GenerateAdditiveCompositePlusIndexes();
            }
            return s_additive_composite_plus_indexes;
        }
    }
    private static void GenerateAdditiveCompositePlusIndexes()
    {
        s_additive_composite_plus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_additive_composite_plus_indexes.Add(s_additive_composites[i] + (i + 1));
        }
    }
    public static bool IsAdditiveCompositePlusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return AdditiveCompositePlusIndexes.Contains(number);
    }

    private static List<long> s_non_additive_composite_plus_indexes;
    public static List<long> NonAdditiveCompositePlusIndexes
    {
        get
        {
            if (s_non_additive_composite_plus_indexes == null)
            {
                GenerateNonAdditiveCompositePlusIndexes();
            }
            return s_non_additive_composite_plus_indexes;
        }
    }
    private static void GenerateNonAdditiveCompositePlusIndexes()
    {
        s_non_additive_composite_plus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_non_additive_composite_plus_indexes.Add(s_non_additive_composites[i] + (i + 1));
        }
    }
    public static bool IsNonAdditiveCompositePlusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return NonAdditiveCompositePlusIndexes.Contains(number);
    }

    // number - index
    private static List<long> s_prime_minus_indexes;
    public static List<long> PrimeMinusIndexes
    {
        get
        {
            if (s_prime_minus_indexes == null)
            {
                GeneratePrimeMinusIndexes();
            }
            return s_prime_minus_indexes;
        }
    }
    private static void GeneratePrimeMinusIndexes()
    {
        s_prime_minus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_prime_minus_indexes.Add(s_primes[i] - (i + 1));
        }
    }
    public static bool IsPrimeMinusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return PrimeMinusIndexes.Contains(number);
    }

    private static List<long> s_additive_prime_minus_indexes;
    public static List<long> AdditivePrimeMinusIndexes
    {
        get
        {
            if (s_additive_prime_minus_indexes == null)
            {
                GenerateAdditivePrimeMinusIndexes();
            }
            return s_additive_prime_minus_indexes;
        }
    }
    private static void GenerateAdditivePrimeMinusIndexes()
    {
        s_additive_prime_minus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_additive_prime_minus_indexes.Add(s_additive_primes[i] - (i + 1));
        }
    }
    public static bool IsAdditivePrimeMinusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return AdditivePrimeMinusIndexes.Contains(number);
    }

    private static List<long> s_non_additive_prime_minus_indexes;
    public static List<long> NonAdditivePrimeMinusIndexes
    {
        get
        {
            if (s_non_additive_prime_minus_indexes == null)
            {
                GenerateNonAdditivePrimeMinusIndexes();
            }
            return s_non_additive_prime_minus_indexes;
        }
    }
    private static void GenerateNonAdditivePrimeMinusIndexes()
    {
        s_non_additive_prime_minus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_non_additive_prime_minus_indexes.Add(s_non_additive_primes[i] - (i + 1));
        }
    }
    public static bool IsNonAdditivePrimeMinusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return NonAdditivePrimeMinusIndexes.Contains(number);
    }

    private static List<long> s_composite_minus_indexes;
    public static List<long> CompositeMinusIndexes
    {
        get
        {
            if (s_composite_minus_indexes == null)
            {
                GenerateCompositeMinusIndexes();
            }
            return s_composite_minus_indexes;
        }
    }
    private static void GenerateCompositeMinusIndexes()
    {
        s_composite_minus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_composite_minus_indexes.Add(s_composites[i] - (i + 1));
        }
    }
    public static bool IsCompositeMinusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return CompositeMinusIndexes.Contains(number);
    }

    private static List<long> s_additive_composite_minus_indexes;
    public static List<long> AdditiveCompositeMinusIndexes
    {
        get
        {
            if (s_additive_composite_minus_indexes == null)
            {
                GenerateAdditiveCompositeMinusIndexes();
            }
            return s_additive_composite_minus_indexes;
        }
    }
    private static void GenerateAdditiveCompositeMinusIndexes()
    {
        s_additive_composite_minus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_additive_composite_minus_indexes.Add(s_additive_composites[i] - (i + 1));
        }
    }
    public static bool IsAdditiveCompositeMinusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return AdditiveCompositeMinusIndexes.Contains(number);
    }

    private static List<long> s_non_additive_composite_minus_indexes;
    public static List<long> NonAdditiveCompositeMinusIndexes
    {
        get
        {
            if (s_non_additive_composite_minus_indexes == null)
            {
                GenerateNonAdditiveCompositeMinusIndexes();
            }
            return s_non_additive_composite_minus_indexes;
        }
    }
    private static void GenerateNonAdditiveCompositeMinusIndexes()
    {
        s_non_additive_composite_minus_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            s_non_additive_composite_minus_indexes.Add(s_non_additive_composites[i] - (i + 1));
        }
    }
    public static bool IsNonAdditiveCompositeMinusIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return NonAdditiveCompositeMinusIndexes.Contains(number);
    }

    // concatenate index|number
    private static List<long> s_index_primes;
    public static List<long> IndexPrimes
    {
        get
        {
            if (s_index_primes == null)
            {
                GenerateIndexPrimes();
            }
            return s_index_primes;
        }
    }
    public static int IndexPrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_index_primes == null)
        {
            GenerateIndexPrimes();
        }
        return BinarySearch(s_index_primes, number);
    }
    private static void GenerateIndexPrimes()
    {
        s_index_primes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = (i + 1).ToString() + s_primes[i].ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_index_primes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Index|Prime number is concatenation of prime index and prime value.</para>
    /// <para>12, 23, 35, 47, 511, 613, 717, 819, 923, 1029, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsIndexPrime(long number)
    {
        if (number < 0L) number *= -1L;

        return IndexPrimes.Contains(number);
    }

    private static List<long> s_index_additive_primes;
    public static List<long> IndexAdditivePrimes
    {
        get
        {
            if (s_index_additive_primes == null)
            {
                GenerateIndexAdditivePrimes();
            }
            return s_index_additive_primes;
        }
    }
    public static int IndexAdditivePrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_index_additive_primes == null)
        {
            GenerateIndexAdditivePrimes();
        }
        return BinarySearch(s_index_additive_primes, number);
    }
    private static void GenerateIndexAdditivePrimes()
    {
        s_index_additive_primes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = (i + 1).ToString() + s_additive_primes[i].ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_index_additive_primes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Index|AdditivePrime number is concatenation of additive_prime index and additive_prime value.</para>
    /// <para>12, 23, 35, 47, 511, 623, 729, 841, 943, 1047, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsIndexAdditivePrime(long number)
    {
        if (number < 0L) number *= -1L;

        return IndexAdditivePrimes.Contains(number);
    }

    private static List<long> s_index_non_additive_primes;
    public static List<long> IndexNonAdditivePrimes
    {
        get
        {
            if (s_index_non_additive_primes == null)
            {
                GenerateIndexNonAdditivePrimes();
            }
            return s_index_non_additive_primes;
        }
    }
    public static int IndexNonAdditivePrimeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_index_non_additive_primes == null)
        {
            GenerateIndexNonAdditivePrimes();
        }
        return BinarySearch(s_index_non_additive_primes, number);
    }
    private static void GenerateIndexNonAdditivePrimes()
    {
        s_index_non_additive_primes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = (i + 1).ToString() + s_non_additive_primes[i].ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_index_non_additive_primes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Index|NonAdditivePrime number is concatenation of non_additive_prime index and non_additive_prime value.</para>
    /// <para>113, 217, 319, 431, 537, 653, 759, 871, 973, 1079, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsIndexNonAdditivePrime(long number)
    {
        if (number < 0L) number *= -1L;

        return IndexNonAdditivePrimes.Contains(number);
    }

    private static List<long> s_index_composites;
    public static List<long> IndexComposites
    {
        get
        {
            if (s_index_composites == null)
            {
                GenerateIndexComposites();
            }
            return s_index_composites;
        }
    }
    public static int IndexCompositeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_index_composites == null)
        {
            GenerateIndexComposites();
        }
        return BinarySearch(s_index_composites, number);
    }
    private static void GenerateIndexComposites()
    {
        s_index_composites = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = (i + 1).ToString() + s_composites[i].ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_index_composites.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Index|Composite number is concatenation of composite index and composite value.</para>
    /// <para>14, 26, 38, 49, 510, 612, 714, 815, 916, 1018, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsIndexComposite(long number)
    {
        if (number < 0L) number *= -1L;

        return IndexComposites.Contains(number);
    }

    private static List<long> s_index_additive_composites;
    public static List<long> IndexAdditiveComposites
    {
        get
        {
            if (s_index_additive_composites == null)
            {
                GenerateIndexAdditiveComposites();
            }
            return s_index_additive_composites;
        }
    }
    public static int IndexAdditiveCompositeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_index_additive_composites == null)
        {
            GenerateIndexAdditiveComposites();
        }
        return BinarySearch(s_index_additive_composites, number);
    }
    private static void GenerateIndexAdditiveComposites()
    {
        s_index_additive_composites = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = (i + 1).ToString() + s_additive_composites[i].ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_index_additive_composites.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Index|AdditiveComposite number is concatenation of additive_composite index and additive_composite value.</para>
    /// <para>14, 26, 38, 49, 515, 618, 722, 824, 926, 1027, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsIndexAdditiveComposite(long number)
    {
        if (number < 0L) number *= -1L;

        return IndexAdditiveComposites.Contains(number);
    }

    private static List<long> s_index_non_additive_composites;
    public static List<long> IndexNonAdditiveComposites
    {
        get
        {
            if (s_index_non_additive_composites == null)
            {
                GenerateIndexNonAdditiveComposites();
            }
            return s_index_non_additive_composites;
        }
    }
    public static int IndexNonAdditiveCompositeIndexOf(long number)
    {
        if (number < 0L) number *= -1L;

        if (s_index_non_additive_composites == null)
        {
            GenerateIndexNonAdditiveComposites();
        }
        return BinarySearch(s_index_non_additive_composites, number);
    }
    private static void GenerateIndexNonAdditiveComposites()
    {
        s_index_non_additive_composites = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = (i + 1).ToString() + s_non_additive_composites[i].ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_index_non_additive_composites.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Index|NonAdditiveComposite number is concatenation of non_additive_composite index and non_additive_composite value.</para>
    /// <para>110, 212, 314, 416, 520, 621, 725, 830, 932, 1034 = PV(bismAllah), ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsIndexNonAdditiveComposite(long number)
    {
        if (number < 0L) number *= -1L;

        return IndexNonAdditiveComposites.Contains(number);
    }

    // concatenate number|index
    private static List<long> s_prime_indexes;
    public static List<long> PrimeIndexes
    {
        get
        {
            if (s_prime_indexes == null)
            {
                GeneratePrimeIndexes();
            }
            return s_prime_indexes;
        }
    }
    private static void GeneratePrimeIndexes()
    {
        s_prime_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = s_primes[i].ToString() + (i + 1).ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_prime_indexes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Prime|Index number is concatenation of prime value and prime index.</para>
    /// <para>21, 32, 53, 74, 115, 136, 177, 198, 239, 2910, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsPrimeIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return PrimeIndexes.Contains(number);
    }

    private static List<long> s_additive_prime_indexes;
    public static List<long> AdditivePrimeIndexes
    {
        get
        {
            if (s_additive_prime_indexes == null)
            {
                GenerateAdditivePrimeIndexes();
            }
            return s_additive_prime_indexes;
        }
    }
    private static void GenerateAdditivePrimeIndexes()
    {
        s_additive_prime_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = s_additive_primes[i].ToString() + (i + 1).ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_additive_prime_indexes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>AdditivePrime|Index number is concatenation of additive_prime value and additive_prime index.</para>
    /// <para>21, 32, 53, 74, 115, 236, 297, 418, 439, 4710, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsAdditivePrimeIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return AdditivePrimeIndexes.Contains(number);
    }

    private static List<long> s_non_additive_prime_indexes;
    public static List<long> NonAdditivePrimeIndexes
    {
        get
        {
            if (s_non_additive_prime_indexes == null)
            {
                GenerateNonAdditivePrimeIndexes();
            }
            return s_non_additive_prime_indexes;
        }
    }
    private static void GenerateNonAdditivePrimeIndexes()
    {
        s_non_additive_prime_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = s_non_additive_primes[i].ToString() + (i + 1).ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_non_additive_prime_indexes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>NonAdditivePrime|Index number is concatenation of non_additive_prime value and non_additive_prime index.</para>
    /// <para>131, 172, 193, 314, 375, 536, 597, 718, 739, 7910, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsNonAdditivePrimeIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return NonAdditivePrimeIndexes.Contains(number);
    }

    private static List<long> s_composite_indexes;
    public static List<long> CompositeIndexes
    {
        get
        {
            if (s_composite_indexes == null)
            {
                GenerateCompositeIndexes();
            }
            return s_composite_indexes;
        }
    }
    private static void GenerateCompositeIndexes()
    {
        s_composite_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = s_composites[i].ToString() + (i + 1).ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_composite_indexes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>Composite|Index number is concatenation of composite value and composite index.</para>
    /// <para>41, 62, 83, 94, 105, 126, 147, 158, 169, 1810, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsCompositeIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return CompositeIndexes.Contains(number);
    }

    private static List<long> s_additive_composite_indexes;
    public static List<long> AdditiveCompositeIndexes
    {
        get
        {
            if (s_additive_composite_indexes == null)
            {
                GenerateAdditiveCompositeIndexes();
            }
            return s_additive_composite_indexes;
        }
    }
    private static void GenerateAdditiveCompositeIndexes()
    {
        s_additive_composite_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = s_additive_composites[i].ToString() + (i + 1).ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_additive_composite_indexes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>AdditiveComposite|Index number is concatenation of additive_composite value and additive_composite index.</para>
    /// <para>41, 62, 83, 94, 155, 186, 227, 248, 269, 2710, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsAdditiveCompositeIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return AdditiveCompositeIndexes.Contains(number);
    }

    private static List<long> s_non_additive_composite_indexes;
    public static List<long> NonAdditiveCompositeIndexes
    {
        get
        {
            if (s_non_additive_composite_indexes == null)
            {
                GenerateNonAdditiveCompositeIndexes();
            }
            return s_non_additive_composite_indexes;
        }
    }
    private static void GenerateNonAdditiveCompositeIndexes()
    {
        s_non_additive_composite_indexes = new List<long>(MAX_SERIES_LENGTH);
        for (int i = 0; i < MAX_SERIES_LENGTH; i++)
        {
            string text = s_non_additive_composites[i].ToString() + (i + 1).ToString();
            long number;
            if (long.TryParse(text, out number))
            {
                s_non_additive_composite_indexes.Add(number);
            }
        }
    }
    /// <summary>
    /// <para>NonAdditiveComposite|Index number is concatenation of non_additive_composite value and non_additive_composite index.</para>
    /// <para>101, 122, 143, 164, 205, 216, 257, 308, 329, 3410, ...</para>
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsNonAdditiveCompositeIndex(long number)
    {
        if (number < 0L) number *= -1L;

        return NonAdditiveCompositeIndexes.Contains(number);
    }


    /// <summary>
    /// Factorize a number into its prime factors.
    /// </summary>
    /// <param name="number">A number to factorize.</param>
    /// <returns>Return all prime factors (including repeated ones).</returns>
    public static List<long> Factorize(long number)
    {
        List<long> result = new List<long>();
        if (result != null)
        {
            if (number < 0L)
            {
                result.Add(-1L);
                number *= -1L;
            }

            if (number == 0)
            {
                return result;
            }

            if ((number > 0L) && (number <= 2L))
            {
                result.Add(number);
            }
            else // if (number > 2L)
            {
                // if number has a prime factor add it to factors,
                // number /= p,
                // reloop until  number == 1L
                while (number != 1L)
                {
                    if ((number % 2L) == 0L) // if even number
                    {
                        result.Add(2L);
                        number /= 2L;
                    }
                    else // trial divide by all primes up to sqrt(number)
                    {
                        long max = (long)Math.Round(Math.Sqrt(number)) + 1L;    // extra 1 for double calculation errors

                        bool is_factor_found = false;
                        for (long i = 3L; i <= max; i += 2L)
                        {
                            if ((number % i) == 0L)
                            {
                                is_factor_found = true;
                                result.Add(i);
                                number /= i;
                                break; // for loop, reloop while
                            }
                        }

                        // if no prime factor found the number must be prime in the first place
                        if (!is_factor_found)
                        {
                            result.Add(number);
                            break; // while loop
                        }
                    }
                }
            }

            s_factors = result;
        }
        return result;
    }
    /// <summary>
    /// Factorize a number into its prime factors.
    /// </summary>
    /// <param name="number">A number to factorize.</param>
    /// <returns>Return all prime factors and their powers.</returns>
    public static Dictionary<long, int> FactorizeByPowers(long number)
    {
        Dictionary<long, int> result = new Dictionary<long, int>();
        if (result != null)
        {
            if (number == 0L) return result;

            int power = 0;
            List<long> factors = Factorize(number);
            if (factors != null)
            {
                result.Add(factors[0], power);
                foreach (long factor in factors)
                {
                    if (!result.ContainsKey(factor)) // new factor
                    {
                        power = 1;
                        result.Add(factor, power);
                    }
                    else // update existing factor
                    {
                        power++;
                        result[factor] = power;
                    }
                }

                s_factor_powers = result;
            }
        }
        return result;
    }
    /// <summary>
    /// Get a multiplication string of a number's prime factors.
    /// </summary>
    /// <param name="number">A number to factorize.</param>
    /// <returns></returns>
    public static string FactorizeToString(long number)
    {
        StringBuilder str = new StringBuilder();
        List<long> factors = Factorize(number);
        if (factors != null)
        {
            if (factors.Count > 0)
            {
                foreach (long factor in factors)
                {
                    str.Append(factor.ToString() + "×");
                }
                if (str.Length > 1)
                {
                    str.Remove(str.Length - 1, 1);
                }
            }
        }
        return str.ToString();
    }

    private static List<long> s_factors = null;
    private static Dictionary<long, int> s_factor_powers = null;
    public static List<long> GetDivisors(long number)
    {
        if (number < 0L) number *= -1L;

        //if (s_factor_powers == null) 
        s_factor_powers = FactorizeByPowers(number);
        int factors_count = GetDivisorCount(number);
        List<long> result = new List<long>(factors_count);

        if (result != null)
        {
            if (number <= 0L) return result;

            result.Insert(0, 1L);
            if (number == 1L) return result;

            int count = 1;
            foreach (long key in s_factor_powers.Keys)
            {
                int count_so_far = count;
                long prime = key;
                int exponent = s_factor_powers[key];

                long multiplier = 1L;
                for (int j = 0; j < exponent; ++j)
                {
                    multiplier *= prime;
                    for (int i = 0; i < count_so_far; ++i)
                    {
                        result.Insert(count++, result[i] * multiplier);
                    }
                }
            }
            result.Sort();
            return result;
        }

        return new List<long>();
    }
    public static int GetDivisorCount(long number)
    {
        if (number < 0L) number *= -1L;

        int result = 1;
        s_factor_powers = FactorizeByPowers(number);
        if (s_factor_powers != null)
        {
            foreach (long key in s_factor_powers.Keys)
            {
                result *= (s_factor_powers[key] + 1);
            }
        }
        return result;
    }
    public static long SumOfDivisors(long number)
    {
        if (number < 0L) number *= -1L;
        if (number == 0) return 0L;

        long result = 0L;
        List<long> divisors = GetDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                result += divisor;
            }
        }
        return result;
    }
    public static long SumOfDivisorDigitSums(long number)
    {
        if (number < 0L) number *= -1L;
        if (number == 0) return 0L;

        long result = 0L;
        List<long> divisors = GetDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                result += DigitSum(divisor);
            }
        }
        return result;
    }
    public static long SumOfDivisorDigitalRoots(long number)
    {
        if (number < 0L) number *= -1L;
        if (number == 0) return 0L;

        long result = 0L;
        List<long> divisors = GetDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                result += DigitalRoot(divisor);
            }
        }
        return result;
    }
    public static string GetDivisorsString(long number)
    {
        if (number < 0L) number *= -1L;
        if (number > 1000000000000L) return "";

        StringBuilder str = new StringBuilder();
        List<long> divisors = GetDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                str.Append(divisor.ToString() + ",");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }
        }
        return str.ToString();
    }
    public static string GetDivisorDigitSumsString(long number)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        List<long> divisors = GetDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                str.Append(DigitSum(divisor).ToString() + "+");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }
        }
        return str.ToString();
    }
    public static string GetDivisorDigitalRootsString(long number)
    {
        if (number < 0L) number *= -1L;

        StringBuilder str = new StringBuilder();
        List<long> divisors = GetDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                str.Append(DigitalRoot(divisor).ToString() + "+");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }
        }
        return str.ToString();
    }

    /// <summary>
    /// Proper divisors are all divisors except the number itself.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static List<long> GetProperDivisors(long number)
    {
        if (number < 0L) number *= -1L;

        List<long> result = GetDivisors(number);
        if (result != null)
        {
            if (result.Count > 0)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }
        return new List<long>();
    }
    public static int GetProperDivisorCount(long number)
    {
        if (number < 0L) number *= -1L;

        return GetDivisorCount(number) - 1;
    }
    public static long SumOfProperDivisors(long number)
    {
        if (number < 0L) number *= -1L;

        return SumOfDivisors(number) - number;
    }
    public static string GetProperDivisorsString(long number)
    {
        if (number < 0L) number *= -1L;
        if (number > 1000000000000L) return "";

        StringBuilder str = new StringBuilder();
        List<long> divisors = GetProperDivisors(number);
        if (divisors != null)
        {
            foreach (long divisor in divisors)
            {
                str.Append(divisor.ToString() + "+");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }
        }
        return str.ToString();
    }

    // similar powers of prime factors
    public static string GetSimilarPowers(long number)
    {
        StringBuilder str = new StringBuilder();

        if ((number > 1L) && (number <= 1000000L))
        {
            Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
            List<int> powers = new List<int>();
            foreach (int value in factors_with_powers.Values)
            {
                powers.Add(value);
            }
            powers.Sort();

            foreach (int power in powers)
            {
                str.Append(power + "-");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1); // "-"
            }
        }

        return str.ToString();
    }
    public static int GetSimilarPowersIndex(long number)
    {
        int index = -1;

        if ((number > 1L) && (number <= 1000000L))
        {
            Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
            List<int> powers = new List<int>();
            foreach (int value in factors_with_powers.Values)
            {
                powers.Add(value);
            }
            powers.Sort();

            for (long n = 2L; n <= number; n++) // start with 2 to skip 1 which is a non-prime and non-composite
            {
                Dictionary<long, int> fps = Numbers.FactorizeByPowers(n);
                List<int> ps = new List<int>();
                foreach (int power in fps.Values)
                {
                    ps.Add(power);
                }
                ps.Sort();

                bool same_lists = true;
                if (powers.Count != ps.Count) // compare size
                {
                    same_lists = false;
                }
                else // compare elements
                {
                    for (int i = 0; i < factors_with_powers.Count; i++)
                    {
                        if (powers[i] != ps[i])
                        {
                            same_lists = false;
                            break;
                        }
                    }
                }

                if (same_lists)
                {
                    index++;
                }
            }
        }

        return index;
    }
    public static List<long> GetSimilarPowersNumbers(long number)
    {
        List<long> result = new List<long>();

        if ((number > 1L) && (number <= 1000000L))
        {
            Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
            List<int> powers = new List<int>();
            foreach (int value in factors_with_powers.Values)
            {
                powers.Add(value);
            }
            powers.Sort();

            for (long n = 2L; n <= number; n++) // start with 2 to skip 1 which is a non-prime and non-composite
            {
                Dictionary<long, int> fps = Numbers.FactorizeByPowers(n);
                List<int> ps = new List<int>();
                foreach (int power in fps.Values)
                {
                    ps.Add(power);
                }
                ps.Sort();

                bool same_lists = true;
                if (powers.Count != ps.Count) // compare size
                {
                    same_lists = false;
                }
                else // compare elements
                {
                    for (int i = 0; i < factors_with_powers.Count; i++)
                    {
                        if (powers[i] != ps[i])
                        {
                            same_lists = false;
                            break;
                        }
                    }
                }

                if (same_lists)
                {
                    result.Add(n);
                }
            }
        }

        return result;
    }
    public static long GetSimilarPowersNumber(long number, int index)
    {
        long result = 0L;

        if ((number > 1L) && (number <= 1000000L))
        {
            if ((index > 0L) && (index <= 100000L))
            {
                Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
                List<int> powers = new List<int>();
                foreach (int value in factors_with_powers.Values)
                {
                    powers.Add(value);
                }
                powers.Sort();

                int count = -1;
                for (long n = 2; n <= long.MaxValue; n++)   // start with 2 to skip 1 which is a non-prime and non-composite
                {
                    Dictionary<long, int> fps = Numbers.FactorizeByPowers(n);
                    List<int> ps = new List<int>();
                    foreach (int power in fps.Values)
                    {
                        ps.Add(power);
                    }
                    ps.Sort();

                    bool same_lists = true;
                    if (powers.Count != ps.Count) // compare size
                    {
                        same_lists = false;
                    }
                    else // compare elements
                    {
                        for (int i = 0; i < factors_with_powers.Count; i++)
                        {
                            if (powers[i] != ps[i])
                            {
                                same_lists = false;
                                break;
                            }
                        }
                    }

                    if (same_lists)
                    {
                        count++;
                        if (count == index)
                        {
                            result = n;
                            break;
                        }
                    }
                }
            }
        }

        return result;
    }
    public static long GetSimilarPowersNextNumber(long number)
    {
        long result = 0L;

        if ((number > 1L) && (number <= 1000000L))
        {
            Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
            List<int> powers = new List<int>();
            foreach (int value in factors_with_powers.Values)
            {
                powers.Add(value);
            }
            powers.Sort();

            for (long n = number + 1; n <= long.MaxValue; n++)
            {
                Dictionary<long, int> fps = Numbers.FactorizeByPowers(n);
                List<int> ps = new List<int>();
                foreach (int power in fps.Values)
                {
                    ps.Add(power);
                }
                ps.Sort();

                bool same_lists = true;
                if (powers.Count != ps.Count) // compare size
                {
                    same_lists = false;
                }
                else // compare elements
                {
                    for (int i = 0; i < factors_with_powers.Count; i++)
                    {
                        if (powers[i] != ps[i])
                        {
                            same_lists = false;
                            break;
                        }
                    }
                }

                if (same_lists)
                {
                    result = n;
                    break;
                }
            }
        }

        return result;
    }
    public static long GetSimilarPowersPriorNumber(long number)
    {
        long result = 0L;

        if ((number > 1L) && (number <= 1000000L))
        {
            Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
            List<int> powers = new List<int>();
            foreach (int value in factors_with_powers.Values)
            {
                powers.Add(value);
            }
            powers.Sort();

            for (long n = number - 1; n > 2; n--)
            {
                Dictionary<long, int> fps = Numbers.FactorizeByPowers(n);
                List<int> ps = new List<int>();
                foreach (int power in fps.Values)
                {
                    ps.Add(power);
                }
                ps.Sort();

                bool same_lists = true;
                if (powers.Count != ps.Count) // compare size
                {
                    same_lists = false;
                }
                else // compare elements
                {
                    for (int i = 0; i < factors_with_powers.Count; i++)
                    {
                        if (powers[i] != ps[i])
                        {
                            same_lists = false;
                            break;
                        }
                    }
                }

                if (same_lists)
                {
                    result = n;
                    break;
                }
            }
        }

        return result;
    }

    // sum/diff of two squares
    public static string Get4nPlus1EqualsSumOfTwoSquares(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number - 1L) / 4L;
        if (number == ((n * 4L) + 1L))
        {
            return GetTwoSquaresSummedToNumber(number);
        }

        return "";
    }
    public static string Get4nPlus1EqualsDiffOfTwoSquares(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number - 1L) / 4L;
        if (number == ((n * 4L) + 1L))
        {
            return GetTwoSquaresDiffedToNumber(number);
        }

        return "";
    }
    public static string Get4nMinus1EqualsSumOfTwoSquares(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number + 1L) / 4L;
        if (number == ((n * 4L) - 1L))
        {
            return GetTwoSquaresSummedToNumber(number);
        }

        return "";
    }
    public static string Get4nMinus1EqualsDiffOfTwoSquares(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number + 1L) / 4L;
        if (number == ((n * 4L) - 1L))
        {
            return GetTwoSquaresDiffedToNumber(number);
        }

        return "";
    }
    private static string GetTwoSquaresSummedToNumber(long number)
    {
        if (number < 0L) number *= -1L;

        string result = "";

        long a = 0L;
        long b = 0L;
        if (number > 1000000L) return "";

        long max = (number / 2L) + 1L;
        for (long i = 0L; i <= max; i++)
        {
            // remove squared i from number
            a = i * i;
            b = number - a;
            if (a > b) break;

            // if b is a perfect square, append new result
            long j = (long)Math.Sqrt(b + Numbers.ERROR_MARGIN);
            if (j * j == b)
            {
                result += i + "^2" + " + " + j + "^2" + " = ";
            }
        }
        if (result.Length > 3) // remove last " = "
        {
            result = result.Remove(result.Length - 3, 3);
        }

        return result;
    }
    private static string GetTwoSquaresDiffedToNumber(long number)
    {
        if (number < 0L) number *= -1L;

        string result = "";

        long a = 0L;
        long b = 0L;
        if (number > 1000000L) return "";

        long max = (number / 2L) + 1L;
        for (long i = 0L; i <= max; i++)
        {
            // add squared i to number
            a = i * i;
            b = number + a;
            if (a > b) break;

            // if b is a perfect square, append new result
            long j = (long)Math.Sqrt(b + Numbers.ERROR_MARGIN);
            if (j * j == b)
            {
                result += j + "^2" + " - " + i + "^2" + " = ";
            }
        }
        if (result.Length > 3) // remove last " = "
        {
            result = result.Remove(result.Length - 3, 3);
        }

        return result;
    }
    // sum/diff of two cubes
    public static string Get4nPlus1EqualsSumOfTwoCubes(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number - 1L) / 4L;
        if (number == ((n * 4L) + 1L))
        {
            return GetTwoCubesSummedToNumber(number);
        }

        return "";
    }
    public static string Get4nPlus1EqualsDiffOfTwoCubes(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number - 1L) / 4L;
        if (number == ((n * 4L) + 1L))
        {
            return GetTwoCubesDiffedToNumber(number);
        }

        return "";
    }
    public static string Get4nMinus1EqualsSumOfTwoCubes(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number + 1L) / 4L;
        if (number == ((n * 4L) - 1L))
        {
            return GetTwoCubesSummedToNumber(number);
        }

        return "";
    }
    public static string Get4nMinus1EqualsDiffOfTwoCubes(long number)
    {
        if (number < 0L) number *= -1L;

        long n = (number + 1L) / 4L;
        if (number == ((n * 4L) - 1L))
        {
            return GetTwoCubesDiffedToNumber(number);
        }

        return "";
    }
    private static string GetTwoCubesSummedToNumber(long number)
    {
        if (number < 0L) number *= -1L;

        string result = "";

        long a = 0L;
        long b = 0L;
        if (number > 1000000L) return "";

        long max = (number / 3L) + 1L;
        for (long i = 0L; i <= max; i++)
        {
            // remove cubed i from number
            a = i * i * i;
            b = number - a;
            if (a > b) break;

            // if b is a perfect cube, append new result
            long j = (long)Math.Pow(b + Numbers.ERROR_MARGIN, (1.0D / 3.0D));
            if (j * j * j == b)
            {
                result += i + "^3" + " + " + j + "^3" + " = ";
            }
        }
        if (result.Length > 3) // remove last " = "
        {
            result = result.Remove(result.Length - 3, 3);
        }

        return result;
    }
    private static string GetTwoCubesDiffedToNumber(long number)
    {
        if (number < 0L) number *= -1L;

        string result = "";

        long a = 0L;
        long b = 0L;
        if (number > 1000000L) return "";

        long max = (number / 3L) + 1L;
        for (long i = 0L; i <= max; i++)
        {
            // add cubed i to number
            a = i * i * i;
            b = number + a;
            if (a > b) break;

            // if b is a perfect cube, append new result
            long j = (long)Math.Pow(b + Numbers.ERROR_MARGIN, (1.0D / 3.0D));
            if (j * j * j == b)
            {
                result += j + "^3" + " - " + i + "^3" + " = ";
            }
        }
        if (result.Length > 3) // remove last " = "
        {
            result = result.Remove(result.Length - 3, 3);
        }

        return result;
    }

    public static int BinarySearch(IList<long> sorted_list, long number)
    {
        if (sorted_list == null)
            return -1;
        if (sorted_list.Count < 1)
            return -1;

        int min = 0;
        int max = sorted_list.Count - 1;
        int old_mid = -1;
        int mid;
        while ((mid = (min + max) / 2) != old_mid)
        {
            if (number == sorted_list[min])
            { return min; }

            if (number == sorted_list[max])
            { return max; }

            if (number == sorted_list[mid])
            { return mid; }
            else if (number < sorted_list[mid])
            { max = mid; }
            else /*if (number > sorted_list[mid])*/
            { min = mid; }

            old_mid = mid;
        }

        return -1;
    }
    public static void QuickSort(IList<long> list, int min, int max)
    {
        if (list == null)
            return;
        if (list.Count < 1)
            return;
        if (min > max)
            return;
        if ((min < 0) || (max >= list.Count))
            return;

        int lo = min;
        int hi = max;
        long mid = list[(lo + hi) / 2];	// uses copy constructor

        do
        {
            while (list[lo] < mid)		// uses comparison operator
                lo++;
            while (mid < list[hi])
                hi--;

            if (lo <= hi)
            {
                long temp = list[hi];
                list[hi] = list[lo];
                list[hi] = temp;
                lo++;
                hi--;
            }
        }
        while (lo <= hi);

        if (hi > min)
            QuickSort(list, min, hi);
        if (lo < max)
            QuickSort(list, lo, max);
    }

    // Permutations التباديل: P(n, k) = n! ⁄ (n−k)!
    //
    //          n!       
    // nPk = ---------
    //        (n-k)!
    //
    //       1 2 3 4 5 6 7 8 9
    // 9P4 = -----------------
    //       1 2 3 4 5    
    //
    //                         
    // 9P4 =           6 7 8 9 = multiply last k numbers
    //
    //
    public static BigInteger nPk(int n, int k)
    {
        BigInteger result = 0;
        if ((n > 0) && (k > 0))
        {
            if (k <= n)
            {
                // multiply last k numbers
                BigInteger numerator = 1;
                int r = n - k + 1;
                for (int i = r; i <= n; i++)
                {
                    numerator *= i;
                }

                result = numerator;
            }
            else // k > n
            {
                result = 0;
            }
        }
        return result;
    }
    // Combinations التوافيق: C(n, k) = n! ⁄ (k! (n−k)!)
    //
    //          n!
    // nCk = ---------
    //       k! (n-k)!
    //
    //       1 2 3 4 5 6 7 8 9
    // 9C4 = ---------------------
    //       1 2 3 4  *  1 2 3 4 5     
    //
    //                 6 7 8 9       multiply last k numbers
    // 9C4 = --------------------- = ------------------------
    //       1 2 3 4                 multiply first k numbers
    //
    //
    public static BigInteger nCk(int n, int k)
    {
        BigInteger result = 0;
        if ((n > 0) && (k > 0))
        {
            if (k <= n)
            {
                // multiply last k numbers
                BigInteger numerator = 1;
                int r = n - k + 1;
                for (int i = r; i <= n; i++)
                {
                    numerator *= i;
                }

                // multiply first k numbers
                BigInteger denominator = 1;
                for (int i = 1; i <= k; i++)
                {
                    denominator *= i;
                }

                result = numerator / denominator;
            }
            else // k > n
            {
                result = 0;
            }
        }
        return result;
    }

    // DO NOT USE as Microsoft provides a backdoor to the NSA
    // in System.Security.Cryptography for "national security".
    private static void GenerateRSAKeys()
    {
        GenerateRSAKeys(null);
    }
    private static void GenerateRSAKeys(string username)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss");
        if (!String.IsNullOrEmpty(username))
        {
            username = username.Replace(" ", "");
            username = username.Replace(".", "");
            username = username.Replace("\b", "");
            username = username.Replace("\t", "");
            username = username.Replace("\r", "");
            username = username.Replace("\n", "");
        }
        else
        {
            username = "";
        }

        int key_length = 16 * 1024; // in bits
        RSACryptoServiceProvider provider = new RSACryptoServiceProvider(key_length);

        if (Directory.Exists(Globals.NUMBERS_FOLDER))
        {
            string private_key_path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + timestamp + "_" + username + ((username.Length == 0) ? "" : "_") + "PrivateKey.xml";
            using (StreamWriter writer = new StreamWriter(private_key_path, false, Encoding.Unicode))
            {
                writer.WriteLine(provider.ToXmlString(true));
            }

            string public_key_path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + timestamp + "_" + username + ((username.Length == 0) ? "" : "_") + "PublicKey.xml";
            using (StreamWriter writer = new StreamWriter(public_key_path, false, Encoding.Unicode))
            {
                writer.WriteLine(provider.ToXmlString(false));
            }
        }
    }
    private static void GenerateRSAKeysTest()
    {
        var publicPrivateRsa = new RSACryptoServiceProvider
            (
                new CspParameters()
                {
                    KeyContainerName = "PublicPrivateKeys",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            )
        {
            PersistKeyInCsp = true,
        };

        var publicRsa = new RSACryptoServiceProvider(
                new CspParameters()
                {
                    KeyContainerName = "PublicKey",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            )
        {
            PersistKeyInCsp = true
        };


        //Export the key. 
        publicRsa.ImportParameters(publicPrivateRsa.ExportParameters(false));
        Console.WriteLine(publicRsa.ToXmlString(false));
        Console.WriteLine(publicPrivateRsa.ToXmlString(false));
        //Dispose those two CSPs. 
        using (publicRsa)
        {
            publicRsa.Clear();
        }
        using (publicPrivateRsa)
        {
            publicRsa.Clear();
        }


        //Retrieve keys
        publicPrivateRsa = new RSACryptoServiceProvider(
                new CspParameters()
                {
                    KeyContainerName = "PublicPrivateKeys",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            );

        publicRsa = new RSACryptoServiceProvider(
                new CspParameters()
                {
                    KeyContainerName = "PublicKey",
                    Flags = CspProviderFlags.UseMachineKeyStore
                    //Flags = CspProviderFlags.UseDefaultKeyContainer 
                }
            );
        Console.WriteLine(publicRsa.ToXmlString(false));
        Console.WriteLine(publicPrivateRsa.ToXmlString(false));
        using (publicRsa)
        {
            publicRsa.Clear();
        }
        using (publicPrivateRsa)
        {
            publicRsa.Clear();
        }
    }
}
