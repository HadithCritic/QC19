using System;
using System.Collections.Generic;

namespace Model
{
    public enum LetterCompareOrder { Ascending, Descending }
    public enum LetterCompareBy { Number, NumberInChapter, NumberInVerse, NumberInWord, Text, Value }
    public class Letter : IComparable<Letter>
    {
        private static LetterCompareBy s_compare_by = LetterCompareBy.Text;
        public static LetterCompareBy CompareBy
        {
            get { return s_compare_by; }
            set { s_compare_by = value; }
        }
        private static LetterCompareOrder s_compare_order = LetterCompareOrder.Ascending;
        public static LetterCompareOrder CompareOrder
        {
            get { return s_compare_order; }
            set { s_compare_order = value; }
        }
        public int CompareTo(Letter obj)
        {
            if (this == obj) return 0;

            if (s_compare_order == LetterCompareOrder.Ascending)
            {
                switch (s_compare_by)
                {
                    case LetterCompareBy.Number:
                        {
                            return this.Number.CompareTo(obj.Number);
                        }
                    case LetterCompareBy.NumberInChapter:
                        {
                            if (this.NumberInChapter.CompareTo(obj.NumberInChapter) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.NumberInChapter.CompareTo(obj.NumberInChapter);
                        }
                    case LetterCompareBy.NumberInVerse:
                        {
                            if (this.NumberInVerse.CompareTo(obj.NumberInVerse) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.NumberInVerse.CompareTo(obj.NumberInVerse);
                        }
                    case LetterCompareBy.NumberInWord:
                        {
                            if (this.NumberInWord.CompareTo(obj.NumberInWord) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.NumberInWord.CompareTo(obj.NumberInWord);
                        }
                    case LetterCompareBy.Text:
                        {
                            if (this.Text.CompareTo(obj.Text) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Text.CompareTo(obj.Text);
                        }
                    case LetterCompareBy.Value:
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
                    case LetterCompareBy.Number:
                        {
                            return obj.Number.CompareTo(this.Number);
                        }
                    case LetterCompareBy.NumberInChapter:
                        {
                            if (obj.NumberInChapter.CompareTo(this.NumberInChapter) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.NumberInChapter.CompareTo(this.NumberInChapter);
                        }
                    case LetterCompareBy.NumberInVerse:
                        {
                            if (obj.NumberInVerse.CompareTo(this.NumberInVerse) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.NumberInVerse.CompareTo(this.NumberInVerse);
                        }
                    case LetterCompareBy.NumberInWord:
                        {
                            if (obj.NumberInWord.CompareTo(this.NumberInWord) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.NumberInWord.CompareTo(this.NumberInWord);
                        }
                    case LetterCompareBy.Text:
                        {
                            if (obj.Text.CompareTo(this.Text) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Text.CompareTo(this.Text);
                        }
                    case LetterCompareBy.Value:
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

        private Word word = null;
        public Word Word
        {
            get { return word; }
        }

        private int number_in_word;
        public int NumberInWord
        {
            set { number_in_word = value; }
            get { return number_in_word; }
        }
        private int number_in_verse = 0;
        public int NumberInVerse
        {
            set { number_in_verse = value; }
            get { return number_in_verse; }
        }
        private int number_in_chapter = 0;
        public int NumberInChapter
        {
            set { number_in_chapter = value; }
            get { return number_in_chapter; }
        }
        private int number = 0;
        public int Number
        {
            set { number = value; }
            get { return number; }
        }

        public Distance DistanceToPrevious = new Distance();
        public Distance DistanceToNext = new Distance();

        private int frequency_in_word = 0;
        private int frequency_in_verse = 0;
        private int frequency_in_chapter = 0;
        private int frequency = 0;
        private int occurrence_in_word = 0;
        private int occurrence_in_verse = 0;
        private int occurrence_in_chapter = 0;
        private int occurrence = 0;
        public int FrequencyInWord
        {
            get { return frequency_in_word; }
            internal set { frequency_in_word = value; }
        }
        public int OccurrenceInWord
        {
            get { return occurrence_in_word; }
            internal set { occurrence_in_word = value; }
        }
        public int FrequencyInVerse
        {
            get { return frequency_in_verse; }
            internal set { frequency_in_verse = value; }
        }
        public int OccurrenceInVerse
        {
            get { return occurrence_in_verse; }
            internal set { occurrence_in_verse = value; }
        }
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
        public int OccurrencesBeforeInWord
        {
            get { return (occurrence_in_word - 1); }
        }
        public int OccurrencesBeforeInVerse
        {
            get { return (occurrence_in_verse - 1); }
        }
        public int OccurrencesBeforeInChapter
        {
            get { return (occurrence_in_chapter - 1); }
        }
        public int OccurrencesBefore
        {
            get { return (occurrence - 1); }
        }
        public int OccurrencesAfterInWord
        {
            get { return (frequency_in_word - occurrence_in_word); }
        }
        public int OccurrencesAfterInVerse
        {
            get { return (frequency_in_verse - occurrence_in_verse); }
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
                if (word != null)
                {
                    return (this.word.Address + ":" + number_in_word.ToString());
                }
                return "XXX:XXX:XXX:XXX";
            }
        }

        private char character;
        public char Character
        {
            get { return character; }
        }
        public string Text
        {
            get { return this.character.ToString(); }
        }

        public Letter(Word word, int number_in_word, char character)
        {
            this.word = word;
            //this.number = number; // to be filled by book.SetupNumbers
            this.number_in_word = number_in_word;
            //this.number_in_verse = number_in_verse; // to be filled by book.SetupNumbers
            //this.number_in_chapter = number_in_chapter; // to be filled by book.SetupNumbers
            this.character = character;
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
