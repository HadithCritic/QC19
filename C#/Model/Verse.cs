using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public enum VerseCompareOrder { Ascending, Descending }
    public enum VerseCompareBy { Number, Chapter, NumberInChapter, Words, Letters, Text, Value }
    public class Verse : IComparable<Verse>
    {
        private static VerseCompareBy s_compare_by = VerseCompareBy.Text;
        public static VerseCompareBy CompareBy
        {
            get { return s_compare_by; }
            set { s_compare_by = value; }
        }
        private static VerseCompareOrder s_compare_order = VerseCompareOrder.Ascending;
        public static VerseCompareOrder CompareOrder
        {
            get { return s_compare_order; }
            set { s_compare_order = value; }
        }
        public int CompareTo(Verse obj)
        {
            if (this == obj) return 0;

            if (s_compare_order == VerseCompareOrder.Ascending)
            {
                switch (s_compare_by)
                {
                    case VerseCompareBy.Number:
                        {
                            return this.Number.CompareTo(obj.Number);
                        }
                    case VerseCompareBy.Chapter:
                        {
                            // sort by chapter and then by verse number
                            int primary = this.Chapter.SortedNumber.CompareTo(obj.Chapter.SortedNumber);
                            if (primary != 0)
                            {
                                return primary;
                            }
                            return this.Number.CompareTo(obj.Number);
                        }
                    case VerseCompareBy.NumberInChapter:
                        {
                            if (this.NumberInChapter.CompareTo(obj.NumberInChapter) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.NumberInChapter.CompareTo(obj.NumberInChapter);
                        }
                    case VerseCompareBy.Words:
                        {
                            if (this.Words.Count.CompareTo(obj.Words.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Words.Count.CompareTo(obj.Words.Count);
                        }
                    case VerseCompareBy.Letters:
                        {
                            if (this.Letters.Count.CompareTo(obj.Letters.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Letters.Count.CompareTo(obj.Letters.Count);
                        }
                    case VerseCompareBy.Text:
                        {
                            if (this.Text.CompareTo(obj.Text) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Text.CompareTo(obj.Text);
                        }
                    case VerseCompareBy.Value:
                        {
                            if (this.Value.CompareTo(obj.Value) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Value.CompareTo(obj.Value);
                        }
                    default:
                        {
                            return this.Number.CompareTo(obj.Number);
                        }
                }
            }
            else
            {
                switch (s_compare_by)
                {
                    case VerseCompareBy.Number:
                        {
                            return obj.Number.CompareTo(this.Number);
                        }
                    case VerseCompareBy.Chapter:
                        {
                            if (obj.Chapter.SortedNumber.CompareTo(this.Chapter.SortedNumber) == 0)
                            {
                                return obj.Chapter.SortedNumber.CompareTo(this.Chapter.SortedNumber);
                            }
                            return obj.Chapter.SortedNumber.CompareTo(this.Chapter.SortedNumber);
                        }
                    case VerseCompareBy.NumberInChapter:
                        {
                            if (obj.NumberInChapter.CompareTo(this.NumberInChapter) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.NumberInChapter.CompareTo(this.NumberInChapter);
                        }
                    case VerseCompareBy.Words:
                        {
                            if (obj.Words.Count.CompareTo(this.Words.Count) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Words.Count.CompareTo(this.Words.Count);
                        }
                    case VerseCompareBy.Letters:
                        {
                            if (obj.Letters.Count.CompareTo(this.Letters.Count) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Letters.Count.CompareTo(this.Letters.Count);
                        }
                    case VerseCompareBy.Text:
                        {
                            if (obj.Text.CompareTo(this.Text) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Text.CompareTo(this.Text);
                        }
                    case VerseCompareBy.Value:
                        {
                            if (obj.Value.CompareTo(this.Value) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Value.CompareTo(this.Value);
                        }
                    default:
                        {
                            return obj.Number.CompareTo(this.Number);
                        }
                }
            }
        }

        private Book book = null;
        public Book Book
        {
            get { return book; }
            set { book = value; }
        }

        private Chapter chapter = null;
        public Chapter Chapter
        {
            get { return chapter; }
            set { chapter = value; }
        }

        private int number = 0;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }
        private int number_in_chapter = 0;
        public int NumberInChapter
        {
            set { number_in_chapter = value; }
            get { return number_in_chapter; }
        }
        public int NumberInPage;
        public int NumberInStation;
        public int NumberInPart;
        public int NumberInGroup;
        public int NumberInHalf;
        public int NumberInQuarter;
        public int NumberInBowing;

        public Distance DistanceToPrevious = new Distance();
        public Distance DistanceToNext = new Distance();

        private int frequency_in_chapter = 0;
        private int frequency = 0;
        private int occurrence_in_chapter = 0;
        private int occurrence = 0;
        public int FrequencyInChapter
        {
            get { return frequency_in_chapter; }
            internal set { frequency_in_chapter = value; }
        }
        public int OccurrenceInChapter
        {
            get { return occurrence_in_chapter; }
            internal set { occurrence_in_chapter = value; }
        }
        public int Frequency
        {
            get { return frequency; }
            internal set { frequency = value; }
        }
        public int Occurrence
        {
            get { return occurrence; }
            internal set { occurrence = value; }
        }
        public int OccurrencesBeforeInChapter
        {
            get { return (occurrence_in_chapter - 1); }
        }
        public int OccurrencesBefore
        {
            get { return (occurrence - 1); }
        }
        public int OccurrencesAfterInChapter
        {
            get { return (frequency_in_chapter - occurrence_in_chapter); }
        }
        public int OccurrencesAfter
        {
            get { return (frequency - occurrence); }
        }

        public string Address
        {
            get
            {
                if (chapter != null)
                {
                    return (this.chapter.SortedNumber.ToString() + ":" + NumberInChapter.ToString());
                }
                return "0:0";
            }
        }
        public string PaddedAddress
        {
            get
            {
                if (chapter != null)
                {
                    return (this.chapter.SortedNumber.ToString("000") + ":" + NumberInChapter.ToString("000"));
                }
                return "000:000";
            }
        }
        public string ArabicAddress
        {
            get
            {
                if (chapter != null)
                {
                    return (this.chapter.SortedNumber.ToArabic() + "_" + NumberInChapter.ToArabic());
                }
                return "٠_٠";
            }
        }

        private Station station = null;
        public Station Station
        {
            get { return station; }
            set { station = value; }
        }

        private Part part = null;
        public Part Part
        {
            get { return part; }
            set { part = value; }
        }

        private Group group = null;
        public Group Group
        {
            get { return group; }
            set { group = value; }
        }

        private Half half = null;
        public Half Half
        {
            get { return half; }
            set { half = value; }
        }

        private Quarter quarter = null;
        public Quarter Quarter
        {
            get { return quarter; }
            set { quarter = value; }
        }

        private Bowing bowing = null;
        public Bowing Bowing
        {
            get { return bowing; }
            set { bowing = value; }
        }

        private Page page = null;
        public Page Page
        {
            get { return page; }
            set { page = value; }
        }

        private ProstrationType prostration_type = ProstrationType.None;
        public ProstrationType ProstrationType
        {
            get { return prostration_type; }
            internal set { prostration_type = value; }
        }

        private InitializationType initialization_type = InitializationType.NonInitialized;
        public InitializationType InitializationType
        {
            get { return initialization_type; }
            set { initialization_type = value; }
        }

        private List<Word> words = null;
        public List<Word> Words
        {
            get { return words; }
        }
        public bool HasRelatedWordsTo(Verse target)
        {
            if (target != null)
            {
                // make a copy to null used words
                List<Word> source_words = new List<Word>(this.Words);

                int common_word_count = 0;
                foreach (Word target_word in target.Words)
                {
                    for (int i = 0; i < source_words.Count; i++)
                    {
                        if (source_words[i] != null)
                        {
                            bool found = false;
                            if (source_words[i].Roots != null)
                            {
                                foreach (string root in source_words[i].Roots)
                                {
                                    if (target_word.Roots != null)
                                    {
                                        if (target_word.Roots.Contains(root))
                                        {
                                            source_words[i] = null; // remove it from list so not to be reused

                                            common_word_count++;
                                            found = true;
                                            break;
                                        }
                                    }
                                }

                                if (found)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (common_word_count >= this.Words.Count)
                {
                    return true;
                }
            }
            return false;
        }

        private List<Letter> letters = null;
        public List<Letter> Letters
        {
            get
            {
                if (this.letters == null)
                {
                    this.letters = new List<Letter>();
                    foreach (Word word in this.Words)
                    {
                        this.letters.AddRange(word.Letters);
                    }
                }
                return this.letters;
            }
        }

        private List<char> unique_letters = null;
        public List<char> UniqueLetters
        {
            get
            {
                if (unique_letters == null)
                {
                    unique_letters = new List<char>();
                    if (this.words != null)
                    {
                        foreach (Word word in this.words)
                        {
                            if (word.UniqueLetters != null)
                            {
                                foreach (char character in word.UniqueLetters)
                                {
                                    if (!unique_letters.Contains(character))
                                    {
                                        unique_letters.Add(character);
                                    }
                                }
                            }
                        }
                    }
                }
                return unique_letters;
            }
        }
        public int GetLetterFrequency(char character)
        {
            int result = 0;
            if (this.words != null)
            {
                foreach (Word word in this.words)
                {
                    if ((word.Letters != null) && (word.Letters.Count > 0))
                    {
                        foreach (Letter letter in word.Letters)
                        {
                            if (letter.Character == character)
                            {
                                result++;
                            }
                        }
                    }
                }
            }
            return result;
        }

        public string Endmark
        {
            get
            {
                if (s_include_number)
                {
                    return (" " + Constants.ORNATE_RIGHT_PARENTHESIS + NumberInChapter.ToArabic() + Constants.ORNATE_LEFT_PARENTHESIS + " ");
                }
                else
                {
                    return "\n"; // this is compatible with a RichTextBox
                }
            }
        }
        private static bool s_include_number = false;
        public static bool IncludeNumber
        {
            get { return s_include_number; }
            set { s_include_number = value; }
        }

        // assume all verses complete their sentences at end
        Stopmark stopmark = Stopmark.MustStop;
        public Stopmark Stopmark
        {
            get { return stopmark; }
        }

        public Verse(int number, string text, Stopmark stopmark)
        {
            this.number = number;
            this.text = text;
            this.stopmark = stopmark;

            // create words WITHOUT stopmarks
            this.words = new List<Word>();
            int word_number = 0;
            int word_position = 0;
            string[] word_texts = text.Split();
            for (int i = 0; i < word_texts.Length; i++)
            {
                string word_text = word_texts[i];
                if (word_text.Length > 0)
                {
                    // build new Words
                    if ((Constants.STOPMARKS.Contains(word_text[0])) || (Constants.QURANMARKS.Contains(word_text[0])))
                    {
                        // increment word position by stopmarks length in original text
                        word_position += 2; // 2 for stopmark and space after it

                        continue; // skip stopmarks/quranmarks
                    }
                    else // proper word
                    {
                        word_number++;
                        Word word = new Word(this, word_number, word_position, word_text);
                        if (word != null)
                        {
                            this.words.Add(word);
                        }
                    }

                    // in all cases
                    word_position += word_text.Length + 1; // 1 for space
                }
            }
        }
        public void ApplyWordStopmarks(string original_text)
        {
            int word_number = 0;

            string[] word_texts = original_text.Split();
            for (int i = 0; i < word_texts.Length; i++)
            {
                string word_text = word_texts[i];
                if (word_text.Length > 0)
                {
                    // build new Words
                    if ((word_text.Length == 1)
                        &&
                        ((word_text[0] == '۩') || (word_text[0] == '⌂'))
                        )
                    {
                        // add stopmark to previous word to stop it interfering with next verse or with chapters 8, 54, 97 as previous ones end with sijood
                        if (((i - 1) >= 0) && ((i - 1) < this.words.Count))
                        {
                            if (this.words[i - 1].Stopmark == Stopmark.None)
                            {
                                this.words[i - 1].Stopmark = Stopmark.MustStop;
                            }
                        }
                    }
                    else if ((Constants.STOPMARKS.Contains(word_text[0])) || (Constants.QURANMARKS.Contains(word_text[0])))
                    {
                        continue; // skip stopmarks/quranmarks
                    }
                    else // proper word
                    {
                        Stopmark word_stopmark = Stopmark.None;
                        word_number++;

                        // lookahead to determine word stopmark
                        // if not last word in verse
                        if (i < word_texts.Length - 1)
                        {
                            word_stopmark = StopmarkHelper.GetStopmark(word_texts[i + 1]);

                            // Quran 36:52 has another stopmark after MustPause, so use that instead
                            if (word_stopmark != Stopmark.None)
                            {
                                if (word_texts.Length > (i + 2))
                                {
                                    Stopmark next_word_stopmark = StopmarkHelper.GetStopmark(word_texts[i + 2]);
                                    if (next_word_stopmark != Stopmark.None)
                                    {
                                        word_stopmark = next_word_stopmark;
                                    }
                                }
                            }

                            // add stopmark.CanStop after بسم الله الرحمن الرحيم except 1:1 and 27:30
                            if (word_number == 4)
                            {
                                if ((word_text.Simplify29() == "الرحيم") || (word_text.Simplify29() == "الررحيم"))
                                {
                                    word_stopmark = Stopmark.CanStop;
                                }
                            }
                        }
                        else // last word in verse
                        {
                            // if no stopmark after word
                            if (word_stopmark == Stopmark.None)
                            {
                                word_stopmark = this.stopmark;
                            }
                        }

                        // apply word stopmark
                        int word_index = word_number - 1;
                        if ((word_index >= 0) && (word_index < this.words.Count))
                        {
                            this.words[word_number - 1].Stopmark = word_stopmark;
                        }
                    }
                }
            }
        }
        public void RecreateWordsApplyStopmarks(string waw_text)
        {
            this.text = waw_text;

            this.words = new List<Word>();

            int word_number = 0;
            int word_position = 0;

            string[] word_texts = waw_text.Split();
            for (int i = 0; i < word_texts.Length; i++)
            {
                string word_text = word_texts[i];
                if (word_text.Length > 0)
                {
                    // build new Words
                    if ((word_text.Length == 1)
                        &&
                        ((word_text[0] == '۩') || (word_text[0] == '⌂'))
                        )
                    {
                        // add stopmark to previous word to stop it interfering with next verse or with chapters 8, 54, 97 as previous ones end with sijood
                        if (((i - 1) >= 0) && ((i - 1) < this.words.Count))
                        {
                            if (this.words[i - 1].Stopmark == Stopmark.None)
                            {
                                this.words[i - 1].Stopmark = Stopmark.MustStop;
                            }
                        }
                    }
                    else if ((Constants.STOPMARKS.Contains(word_text[0])) || (Constants.QURANMARKS.Contains(word_text[0])))
                    {
                        // increment word position by stopmarks length in original text
                        word_position += 2; // 2 for stopmark and space after it

                        continue; // skip stopmarks/quranmarks
                    }
                    else // proper word
                    {
                        word_number++;
                        Stopmark word_stopmark = Stopmark.None;

                        // lookahead to determine word stopmark
                        // if not last word in verse
                        if (i < word_texts.Length - 1)
                        {
                            word_stopmark = StopmarkHelper.GetStopmark(word_texts[i + 1]);

                            // Quran 36:52 has another stopmark after MustPause, so use that instead
                            if (word_stopmark != Stopmark.None)
                            {
                                if (word_texts.Length > (i + 2))
                                {
                                    Stopmark next_word_stopmark = StopmarkHelper.GetStopmark(word_texts[i + 2]);
                                    if (next_word_stopmark != Stopmark.None)
                                    {
                                        word_stopmark = next_word_stopmark;
                                    }
                                }
                            }

                            // add stopmark.CanStop after بسم الله الرحمن الرحيم except 1:1 and 27:30
                            if (word_number == 4)
                            {
                                if ((word_text.Simplify29() == "الرحيم") || (word_text.Simplify29() == "الررحيم"))
                                {
                                    word_stopmark = Stopmark.CanStop;
                                }
                            }
                        }
                        else // last word in verse
                        {
                            // if no stopmark after word
                            if (word_stopmark == Stopmark.None)
                            {
                                // use verse stopmark
                                word_stopmark = this.stopmark;
                            }
                        }

                        Word word = new Word(this, word_number, word_position, word_text);
                        if (word != null)
                        {
                            word.Stopmark = word_stopmark;
                            this.words.Add(word);
                        }
                    }

                    // in all cases
                    word_position += word_text.Length + 1; // 1 for space
                }
            }
        }

        private string text = null;
        public string Text
        {
            get { return text; }
            internal set { text = value; }
        }

        // update value for CompareBy.Value
        private long value = 0L;
        public long Value
        {
            set { this.value = value; }
            get { return this.value; }
        }

        /// <summary>
        /// Language --> Text
        /// </summary>
        public Dictionary<string, string> Translations = new Dictionary<string, string>();
    }
}
