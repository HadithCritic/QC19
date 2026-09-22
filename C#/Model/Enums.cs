using System;

namespace Model
{
    public enum CalculationMode
    {
        SumOfLetterValues,                              // White
        SumOfLetterValueDigitSums,                      // Light Red
        SumOfLetterValueDigitalRoots,                   //       Red
        SumOfWordValueDigitSums,                        // Light Blue
        SumOfWordValueDigitalRoots,                     //       Blue
        SumOfVerseValueDigitSums,                       // Light Green
        SumOfVerseValueDigitalRoots,                    //       Green
        SumOfUniqueLetterValues,                        //       Yellow
    };

    /// <summary>
    /// Source of letters used in building a numerical system dynamically
    /// </summary>
    public enum NumericalSystemScope { Book, Selection, HighlightedText };

    public enum LanguageType { RightToLeft, LeftToRight };

    public enum SelectionScope { Book, Station, Part, Group, Half, Quarter, Bowing, Chapter, Verse, Word, Letter, Page };

    public enum SearchScope { Book, Selection, Result };
    public enum SearchType { Text, Numbers, Similarity, Frequency, Sum, Grammar, Repetition };

    public enum TextSearchType { Exact, Proximity, Root };
    public enum TextSearchBlockSize { Verse, Chapter, Page, Station, Part, Group, Half, Quarter, Bowing };
    public enum TextLocationInChapter { Any, AtStart, AtMiddle, AtEnd };
    public enum TextLocationInVerse { Any, AtStart, AtMiddle, AtEnd };
    public enum TextLocationInWord { Any, AtStart, AtMiddle, AtEnd };
    public enum TextWordGrouping { Or, And };
    public enum TextWordness { Any, WholeWord, PartOfWord };

    public enum SimilaritySearchSource { CurrentVerse, AllVerses };
    public enum SimilarityMethod { SimilarText, SimilarWords, SameWordRoots, SameWordValues };

    public enum NumberScope { Number, NumberInChapter, NumberInVerse, NumberInWord };
    public enum NumbersResultType
    {
        Letters, Words, Sentences, Verses, Chapters, Pages, Stations, Parts, Groups, Halfs, Quarters, Bowings,
        LetterRanges, WordRanges, VerseRanges, ChapterRanges, PageRanges, StationRanges, PartRanges, GroupRanges, HalfRanges, QuarterRanges, BowingRanges,
        LetterSets, WordSets, VerseSets, ChapterSets, PageSets, StationSets, PartSets, GroupSets, HalfSets, QuarterSets, BowingSets
    };

    public enum RevelationPlace { Makkah, Medina, Both };

    public enum ProstrationType { None, Recommended, Obligatory };

    public enum InitializationType { Key, PartiallyInitialized, FullyInitialized, DoublyInitialized, NonInitialized };
    public enum ChapterSelection
    {
        Any,
        Key,
        Makkah, Medina,
        Initialized, PartiallyInitialized, FullyInitialized, DoublyInitialized, NonInitialized,
        Even, Odd, Any_E, Any_O, E_E, E_O, O_O, O_E, E_E_and_O_O, E_O_and_O_E,
        Composite, Prime, Any_C, Any_P, C_C, C_P, P_P, P_C, C_C_and_P_P, C_P_and_P_C,
        AC, AP, Any_AC, Any_AP, AC_AC, AC_AP, AP_AP, AP_AC, AC_AC_and_AP_AP, AC_AP_and_AP_AC,
        XC, XP, Any_XC, Any_XP, XC_XC, XC_XP, XP_XP, XP_XC, XC_XC_and_XP_XP, XC_XP_and_XP_XC,
        MersennePrime,
        Heavy, Light,
        All
    };

    public enum FrequencyMatchingType { AllLettersOf, AnyLetterOf, OnlyLettersOf, NoLetterOf };
    public enum FrequencySearchType { DuplicateLetters, UniqueLetters };
    public enum FrequencyResultType { Words, Sentences, Verses, Chapters, WordRanges, VerseRanges, ChapterRanges };

    public enum UserRatioScope { Verse, Chapter, Page, Station, Part, Group, Half, Quarter, Bowing, Book };
    public enum UserRatioDelimiter { None, Letter, Word, Sentence, Verse, Chapter };
    public enum UserRatioType { Text, Value };
    public enum UserRatioLength { Short, Long };

    /// <summary>
    /// <para>    : None</para>
    /// <para>Laaa: MustContinue</para>
    /// <para>Sala: ShouldContinue</para>
    /// <para>Jeem: CanStop</para>
    /// <para>Dots: CanStopAtEither</para>
    /// <para>Qala: ShouldStop</para>
    /// <para>Seen: MustPause</para>
    /// <para>Meem: MustStop</para>
    /// </summary>
    //                     None,   Stop0%,       Stop25%,      Stop50%, Stop50%AtEither,  Stop75%,   Pause100%, Stop100%
    public enum Stopmark { None, MustContinue, ShouldContinue, CanStop, CanStopAtEither, ShouldStop, MustPause, MustStop };
    public class StopmarkHelper
    {
        //MustPause Occurrences:
        //Stop  	١٨_١	بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ ٱلْحَمْدُ لِلَّهِ ٱلَّذِىٓ أَنزَلَ عَلَىٰ عَبْدِهِ ٱلْكِتَٰبَ وَلَمْ يَجْعَل لَّهُۥ عِوَجَا ۜ
        //StopSTOP 	٣٦_٥٢	قَالُوا۟ يَٰوَيْلَنَا مَنۢ بَعَثَنَا مِن مَّرْقَدِنَا ۜ ۗ هَٰذَا مَا وَعَدَ ٱلرَّحْمَٰنُ وَصَدَقَ ٱلْمُرْسَلُونَ
        //Stop  	٦٩_٢٨	مَآ أَغْنَىٰ عَنِّى مَالِيَهْ ۜ
        //Continue 	٧٥_٢٧	وَقِيلَ مَنْ ۜ رَاقٍۢ
        //Continue 	٨٣_١٤	كَلَّا ۖ بَلْ ۜ رَانَ عَلَىٰ قُلُوبِهِم مَّا كَانُوا۟ يَكْسِبُونَ
        public static Stopmark GetStopmark(string stopmark_text)
        {
            Stopmark stopmark = Stopmark.None;
            if (!String.IsNullOrEmpty(stopmark_text))
            {
                switch (stopmark_text)
                {
                    case "":
                        stopmark = Stopmark.None;
                        break;
                    case "ۙ": // Laaa
                        stopmark = Stopmark.MustContinue;
                        break;
                    case "ۖ": // Sala
                        stopmark = Stopmark.ShouldContinue;
                        break;
                    case "ۚ": // Jeem
                        stopmark = Stopmark.CanStop;
                        break;
                    case "ۛ": // Dots
                        stopmark = Stopmark.CanStopAtEither;
                        break;
                    case "ۗ": // Qala
                        stopmark = Stopmark.ShouldStop;
                        break;
                    case "ۜ": // Seen
                        stopmark = Stopmark.MustPause;
                        break;
                    case "ۘ": // Meem
                        stopmark = Stopmark.MustStop;
                        break;
                    default: // Quran word
                        stopmark = Stopmark.None;
                        break;
                }
            }
            return stopmark;
        }
        public static string GetStopmarkText(Stopmark stopmark)
        {
            string stopmark_text = "";
            switch (stopmark)
            {
                case Stopmark.None: // none
                    stopmark_text = "";
                    break;
                case Stopmark.MustContinue:
                    stopmark_text = "ۙ"; // Laaa
                    break;
                case Stopmark.ShouldContinue:
                    stopmark_text = "ۖ"; // Sala
                    break;
                case Stopmark.CanStop:
                    stopmark_text = "ۚ"; // Jeem
                    break;
                case Stopmark.CanStopAtEither:
                    stopmark_text = "ۛ"; // Dots
                    break;
                case Stopmark.ShouldStop:
                    stopmark_text = "ۗ"; // Qala
                    break;
                case Stopmark.MustPause:
                    stopmark_text = "ۜ"; // Seen
                    break;
                case Stopmark.MustStop:
                    stopmark_text = "ۘ"; // Meem
                    break;
                default:
                    stopmark_text = "ۘ"; // Meem;
                    break;
            }
            return stopmark_text;
        }
    }
}