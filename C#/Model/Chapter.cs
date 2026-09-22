using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public enum ChapterCompareOrder { Ascending, Descending }
    public enum ChapterCompareBy { Compilation, Revelation, Verses, Words, Letters, CPlusV, CMinusV, CMultiplyV, CDivideV, Value }
    public class Chapter : IComparable<Chapter>
    {
        private static ChapterCompareBy s_compare_by = ChapterCompareBy.Compilation;
        public static ChapterCompareBy CompareBy
        {
            get { return s_compare_by; }
            set { s_compare_by = value; }
        }
        private static ChapterCompareOrder s_compare_order = ChapterCompareOrder.Ascending;
        public static ChapterCompareOrder CompareOrder
        {
            get { return s_compare_order; }
            set { s_compare_order = value; }
        }
        private static bool s_pin_chapter1 = true;
        public static bool PinChapter1
        {
            get { return s_pin_chapter1; }
            set { s_pin_chapter1 = value; }
        }
        public int CompareTo(Chapter obj)
        {
            if (this == obj) return 0;

            // don't pin chapter1 in compilation and revelation orders
            if ((s_compare_by != ChapterCompareBy.Compilation) && (s_compare_by != ChapterCompareBy.Revelation))
            {
                if ((Chapter.PinChapter1) && (this.Number == 1)) return -1;
                if ((Chapter.PinChapter1) && (obj.Number == 1)) return 1;
            }

            if (s_compare_order == ChapterCompareOrder.Ascending)
            {
                switch (s_compare_by)
                {
                    case ChapterCompareBy.Compilation:
                        {
                            return this.Number.CompareTo(obj.Number);
                        }
                    case ChapterCompareBy.Revelation:
                        {
                            return this.RevelationOrder.CompareTo(obj.RevelationOrder);
                        }
                    case ChapterCompareBy.Verses:
                        {
                            if (this.verses.Count.CompareTo(obj.Verses.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.verses.Count.CompareTo(obj.Verses.Count);
                        }
                    case ChapterCompareBy.Words:
                        {
                            if (this.Words.Count.CompareTo(obj.Words.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Words.Count.CompareTo(obj.Words.Count);
                        }
                    case ChapterCompareBy.Letters:
                        {
                            if (this.Letters.Count.CompareTo(obj.Letters.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Letters.Count.CompareTo(obj.Letters.Count);
                        }
                    case ChapterCompareBy.CPlusV:
                        {
                            int this_cv = this.Number + this.verses.Count;
                            int obj_cv = obj.Number + obj.verses.Count;

                            if (this_cv.CompareTo(obj_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this_cv.CompareTo(obj_cv);
                        }
                    case ChapterCompareBy.CMinusV:
                        {
                            int this_cv = this.Number - this.verses.Count;
                            int obj_cv = obj.Number - obj.verses.Count;

                            if (this_cv.CompareTo(obj_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this_cv.CompareTo(obj_cv);
                        }
                    case ChapterCompareBy.CMultiplyV:
                        {
                            int this_cv = this.Number * this.verses.Count;
                            int obj_cv = obj.Number * obj.verses.Count;

                            if (this_cv.CompareTo(obj_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this_cv.CompareTo(obj_cv);
                        }
                    case ChapterCompareBy.CDivideV:
                        {
                            int this_cv = (this.Number * 1000000) / this.verses.Count;
                            int obj_cv = (obj.Number * 1000000) / obj.verses.Count;

                            if (this_cv.CompareTo(obj_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this_cv.CompareTo(obj_cv);
                        }
                    case ChapterCompareBy.Value:
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
                    case ChapterCompareBy.Compilation:
                        {
                            return obj.Number.CompareTo(this.Number);
                        }
                    case ChapterCompareBy.Revelation:
                        {
                            return obj.RevelationOrder.CompareTo(this.RevelationOrder);
                        }
                    case ChapterCompareBy.Verses:
                        {
                            if (obj.Verses.Count.CompareTo(this.verses.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj.Verses.Count.CompareTo(this.verses.Count);
                        }
                    case ChapterCompareBy.Words:
                        {
                            if (obj.Words.Count.CompareTo(this.Words.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj.Words.Count.CompareTo(this.Words.Count);
                        }
                    case ChapterCompareBy.Letters:
                        {
                            if (obj.Letters.Count.CompareTo(this.Letters.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj.Letters.Count.CompareTo(this.Letters.Count);
                        }
                    case ChapterCompareBy.CPlusV:
                        {
                            int this_cv = this.Number + this.verses.Count;
                            int obj_cv = obj.Number + obj.verses.Count;

                            if (obj_cv.CompareTo(this_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj_cv.CompareTo(this_cv);
                        }
                    case ChapterCompareBy.CMinusV:
                        {
                            int this_cv = this.Number - this.verses.Count;
                            int obj_cv = obj.Number - obj.verses.Count;

                            if (obj_cv.CompareTo(this_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj_cv.CompareTo(this_cv);
                        }
                    case ChapterCompareBy.CMultiplyV:
                        {
                            int this_cv = this.Number * this.verses.Count;
                            int obj_cv = obj.Number * obj.verses.Count;

                            if (obj_cv.CompareTo(this_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj_cv.CompareTo(this_cv);
                        }
                    case ChapterCompareBy.CDivideV:
                        {
                            int this_cv = (this.Number * 1000000) / this.verses.Count;
                            int obj_cv = (obj.Number * 1000000) / obj.verses.Count;

                            if (obj_cv.CompareTo(this_cv) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return obj_cv.CompareTo(this_cv);
                        }
                    case ChapterCompareBy.Value:
                        {
                            if (obj.Value.CompareTo(this.Value) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
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

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get { return verses; }
        }

        private int number = 0;
        public int Number
        {
            get { return number; }
        }

        private int sorted_number = 0;
        public int SortedNumber
        {
            get { return sorted_number; }
            set { sorted_number = value; }
        }

        private string name = null;
        public string Name
        {
            get { return name; }
        }

        private string transliterated_name = null;
        public string TransliteratedName
        {
            get { return transliterated_name; }
        }

        private string english_name = null;
        public string EnglishName
        {
            get { return english_name; }
        }

        private RevelationPlace revelation_place = RevelationPlace.Both;
        public RevelationPlace RevelationPlace
        {
            get { return revelation_place; }
        }

        private int revelation_order = 0;
        public int RevelationOrder
        {
            get { return revelation_order; }
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
            get
            {
                if (this.words == null)
                {
                    this.words = new List<Word>();
                    if (this.verses != null)
                    {
                        foreach (Verse verse in this.verses)
                        {
                            if (verse != null)
                            {
                                this.words.AddRange(verse.Words);
                            }
                        }
                    }
                }
                return this.words;
            }
        }

        private List<Letter> letters = null;
        public List<Letter> Letters
        {
            get
            {
                if (this.letters == null)
                {
                    this.letters = new List<Letter>();
                    if (this.verses != null)
                    {
                        foreach (Verse verse in this.verses)
                        {
                            if (verse != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        this.letters.AddRange(word.Letters);
                                    }
                                }
                            }
                        }
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
                    this.unique_letters = new List<char>();
                    if (this.verses != null)
                    {
                        foreach (Verse verse in this.verses)
                        {
                            if (verse != null)
                            {
                                if (verse.Words != null)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        if (word != null)
                                        {
                                            if (word.UniqueLetters != null)
                                            {
                                                foreach (char character in word.UniqueLetters)
                                                {
                                                    if (!this.unique_letters.Contains(character))
                                                    {
                                                        this.unique_letters.Add(character);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return this.unique_letters;
            }
        }
        public int GetLetterFrequency(char character)
        {
            int result = 0;
            if (this.verses != null)
            {
                foreach (Verse verse in this.verses)
                {
                    if (verse != null)
                    {
                        if (verse.Words != null)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    if ((word.Letters != null) && (word.Letters.Count > 0))
                                    {
                                        foreach (Letter letter in word.Letters)
                                        {
                                            if (letter != null)
                                            {
                                                if (letter.Character == character)
                                                {
                                                    result++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public Chapter(Book book,
                       int number,
                       string name,
                       string transliterated_name,
                       string english_name,
                       RevelationPlace revelation_place,
                       int revelation_order,
                       int bowing_count,
                       List<Verse> verses)
        {
            this.book = book;
            this.number = number;
            this.sorted_number = number;
            this.name = name;
            this.transliterated_name = transliterated_name;
            this.english_name = english_name;
            this.revelation_place = revelation_place;
            this.revelation_order = revelation_order;
            this.verses = verses;
            if (this.verses != null)
            {
                int verse_number = 1;
                foreach (Verse verse in this.verses)
                {
                    if (verse != null)
                    {
                        verse.Chapter = this;
                        verse.NumberInChapter = verse_number++;
                    }
                }
            }
        }

        string text = null;
        public string Text
        {
            get
            {
                if (String.IsNullOrEmpty(text))
                {
                    StringBuilder str = new StringBuilder();
                    if (this.verses != null)
                    {
                        if (this.verses.Count > 0)
                        {
                            foreach (Verse verse in this.verses)
                            {
                                if (verse != null)
                                {
                                    str.AppendLine(verse.Text);
                                }
                            }
                            if (str.Length > 2)
                            {
                                str.Remove(str.Length - 2, 2);
                            }
                        }
                    }

                    this.text = str.ToString();
                }

                return this.text;
            }
        }

        // update value for CompareBy.Value
        private long value = 0L;
        public long Value
        {
            set { this.value = value; }
            get { return this.value; }
        }
    }
}
