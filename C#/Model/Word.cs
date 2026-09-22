using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public enum WordCompareOrder { Ascending, Descending }
    public enum WordCompareBy { Number, NumberInChapter, NumberInVerse, Letters, Text, Root, Value }
    public class Word : IComparable<Word>
    {
        private static WordCompareBy s_compare_by = WordCompareBy.Text;
        public static WordCompareBy CompareBy
        {
            get { return s_compare_by; }
            set { s_compare_by = value; }
        }
        private static WordCompareOrder s_compare_order = WordCompareOrder.Ascending;
        public static WordCompareOrder CompareOrder
        {
            get { return s_compare_order; }
            set { s_compare_order = value; }
        }
        public int CompareTo(Word obj)
        {
            if (this == obj) return 0;

            if (s_compare_order == WordCompareOrder.Ascending)
            {
                switch (s_compare_by)
                {
                    case WordCompareBy.Number:
                        {
                            return this.Number.CompareTo(obj.Number);
                        }
                    case WordCompareBy.NumberInChapter:
                        {
                            if (this.NumberInChapter.CompareTo(obj.NumberInChapter) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.NumberInChapter.CompareTo(obj.NumberInChapter);
                        }
                    case WordCompareBy.NumberInVerse:
                        {
                            if (this.NumberInVerse.CompareTo(obj.NumberInVerse) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.NumberInVerse.CompareTo(obj.NumberInVerse);
                        }
                    case WordCompareBy.Letters:
                        {
                            if (this.Letters.Count.CompareTo(obj.Letters.Count) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Letters.Count.CompareTo(obj.Letters.Count);
                        }
                    case WordCompareBy.Text:
                        {
                            if (this.Text.CompareTo(obj.Text) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Text.CompareTo(obj.Text);
                        }
                    case WordCompareBy.Root:
                        {
                            if (this.Root.CompareTo(obj.Root) == 0)
                            {
                                return this.Number.CompareTo(obj.Number);
                            }
                            return this.Root.CompareTo(obj.Root);
                        }
                    case WordCompareBy.Value:
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
                    case WordCompareBy.Number:
                        {
                            return obj.Number.CompareTo(this.Number);
                        }
                    case WordCompareBy.NumberInChapter:
                        {
                            if (obj.NumberInChapter.CompareTo(this.NumberInChapter) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.NumberInChapter.CompareTo(this.NumberInChapter);
                        }
                    case WordCompareBy.NumberInVerse:
                        {
                            if (obj.NumberInVerse.CompareTo(this.NumberInVerse) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.NumberInVerse.CompareTo(this.NumberInVerse);
                        }
                    case WordCompareBy.Letters:
                        {
                            if (obj.Letters.Count.CompareTo(this.Letters.Count) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Letters.Count.CompareTo(this.Letters.Count);
                        }
                    case WordCompareBy.Text:
                        {
                            if (obj.Text.CompareTo(this.Text) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Text.CompareTo(this.Text);
                        }
                    case WordCompareBy.Root:
                        {
                            if (obj.Root.CompareTo(this.Root) == 0)
                            {
                                return obj.Number.CompareTo(this.Number);
                            }
                            return obj.Root.CompareTo(this.Root);
                        }
                    case WordCompareBy.Value:
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

        private Verse verse = null;
        public Verse Verse
        {
            get { return verse; }
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

        private int frequency_in_verse = 0;
        private int frequency_in_chapter = 0;
        private int frequency = 0;
        private int frequency_by_root = 0;
        private int frequency_by_value = 0;
        private int occurrence_in_verse = 0;
        private int occurrence_in_chapter = 0;
        private int occurrence = 0;
        private int occurrence_by_root = 0;
        private int occurrence_by_value = 0;
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
        public int FrequencyByRoot
        {
            get { return frequency_by_root; }
            internal set { frequency_by_root = value; }
        }
        public int FrequencyByValue
        {
            get { return frequency_by_value; }
            internal set { frequency_by_value = value; }
        }
        public int Occurrence
        {
            get { return occurrence; }
            internal set { occurrence = value; }
        }
        public int OccurrenceByRoot
        {
            get { return occurrence_by_root; }
            internal set { occurrence_by_root = value; }
        }
        public int OccurrenceByValue
        {
            get { return occurrence_by_value; }
            internal set { occurrence_by_value = value; }
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
                if (verse != null)
                {
                    if (verse.Chapter != null)
                    {
                        return (this.verse.Chapter.SortedNumber.ToString() + ":" + verse.NumberInChapter.ToString() + ":" + number_in_verse.ToString());
                    }
                }
                return "XXX:XXX:XXX";
            }
        }

        private string transliteration = null;
        public string Transliteration
        {
            get
            {
                if (transliteration == null)
                {
                    if (this.text == "و")
                    {
                        transliteration = "wa";
                    }
                    else
                    {
                        if (this.verse != null)
                        {
                            if (this.verse.Translations.ContainsKey("en.transliteration"))
                            {
                                string verse_transliteration = this.verse.Translations["en.transliteration"];
                                string[] parts = verse_transliteration.Split();

                                int index = this.number_in_verse - 1;

                                if (!this.verse.Book.WithBismAllah)
                                {
                                    if (this.verse.NumberInChapter == 1)
                                    {
                                        if ((this.verse.Chapter.Number != 1) && (this.verse.Chapter.Number != 9))
                                        {
                                            index += 4;
                                        }
                                    }
                                }

                                for (int i = 0; i < this.number_in_verse; i++)
                                {
                                    if ((i >= 0) && (i < this.verse.Words.Count))
                                    {
                                        if (this.verse.Words[i].Text == "و")
                                        {
                                            index--;
                                        }
                                    }
                                }

                                if ((index >= 0) && (index < parts.Length))
                                {
                                    // remove wa from words following wa
                                    int w = this.number_in_verse - 1;
                                    if (((w - 1) >= 0) && ((w - 1) < this.verse.Words.Count))
                                    {
                                        if (this.verse.Words[w - 1].Text == "و")
                                        {
                                            parts[index] = parts[index].Substring(2);
                                        }
                                    }

                                    transliteration = parts[index];
                                }
                            }
                        }
                    }
                }
                if (transliteration != null)
                {
                    return transliteration.Replace("\"", "").Replace("\'", "");
                }
                return null;
            }
        }

        private string meaning = null;
        public string Meaning
        {
            set { meaning = value; }
            get
            {
                if (meaning != null)
                {
                    return meaning.Replace("\"", "").Replace("\'", "");
                }
                return null;
            }
        }

        private List<string> roots = null;
        public List<string> Roots
        {
            set { roots = value; }
            get { return roots; }
        }
        private string root = null;
        public string Root
        {
            get
            {
                if (String.IsNullOrEmpty(root))
                {
                    root = "";
                    if (this.Roots != null)
                    {
                        if (this.Roots.Count > 0)
                        {
                            foreach (string r in this.Roots)
                            {
                                if (root.Length < r.Length)
                                {
                                    root = r;
                                }
                            }
                        }
                    }
                }
                return root;
            }
        }

        private List<WordPart> parts = null;
        public List<WordPart> Parts
        {
            get { return parts; }
        }

        private List<WordPartGrammar> grammar = null;
        public List<WordPartGrammar> Grammar
        {
            get
            {
                if (grammar == null)
                {
                    grammar = new List<WordPartGrammar>();
                    foreach (WordPart word_part in this.Parts)
                    {
                        grammar.Add(word_part.Grammar);
                    }
                }
                return grammar;
            }
        }
        private string grammar_text = null;
        public string GrammarText
        {
            get
            {
                if (String.IsNullOrEmpty(grammar_text))
                {
                    StringBuilder result = new StringBuilder();
                    StringBuilder str = new StringBuilder();
                    string previous_word_address = null;
                    if (this.Parts != null)
                    {
                        for (int i = 0; i < this.Parts.Count; i++)
                        {
                            if (previous_word_address == this.Parts[i].Word.Address)
                            {
                                // continue with current part
                                str.Append
                                (
                                    this.Parts[i].Text + "\t" + this.Parts[i].Buckwalter + "\t" +
                                    ((this.Parts[i].Grammar.Position != null) ? (this.Parts[i].Grammar.Position + "\t") : "") +
                                    ((this.Parts[i].Grammar.Attribute != null) ? (this.Parts[i].Grammar.Attribute + "\t") : "") +
                                    ((this.Parts[i].Grammar.Position == "V") ? "\t" + ((this.Parts[i].Grammar.Qualifier != null) ? (this.Parts[i].Grammar.Qualifier + "\t") : "") : "") +
                                    ((this.Parts[i].Grammar.PersonDegree != null) ? (this.Parts[i].Grammar.PersonDegree + "\t") : "") +
                                    ((this.Parts[i].Grammar.PersonGender != null) ? (this.Parts[i].Grammar.PersonGender + "\t") : "") +
                                    ((this.Parts[i].Grammar.PersonNumber != null) ? (this.Parts[i].Grammar.PersonNumber + "\t") : "") +
                                    ((this.Parts[i].Grammar.Mood != null) ? (this.Parts[i].Grammar.Mood + "\t") : "") //+
                                                                                                                      //((this.Parts[i].Grammar.Lemma != null) ? (this.Parts[i].Grammar.Lemma.ToArabic() + "\t") : "") +
                                                                                                                      //((this.Parts[i].Grammar.Root != null) ? (this.Parts[i].Grammar.Root.ToArabic() + "\t") : "") +
                                                                                                                      //((this.Parts[i].Grammar.SpecialGroup != null) ? (this.Parts[i].Grammar.SpecialGroup.ToArabic() + "\t") : "")
                                );
                                str.Remove(str.Length - 1, 1); // remove last "\t"
                                str.AppendLine();
                            }
                            else // new part
                            {
                                previous_word_address = this.Parts[i].Word.Address;

                                // finish up previous part
                                if (str.Length > 2)
                                {
                                    str.Remove(str.Length - 2, 2); // "\r\n"
                                    result.Append(str.ToString());
                                    // clear str for new part
                                    str.Length = 0;
                                }

                                // continue with current part
                                str.Append
                                (
                                    this.Parts[i].Text + "\t" + this.Parts[i].Buckwalter + "\t" +
                                    ((this.Parts[i].Grammar.Position != null) ? (this.Parts[i].Grammar.Position + "\t") : "") +
                                    ((this.Parts[i].Grammar.Attribute != null) ? (this.Parts[i].Grammar.Attribute + "\t") : "") +
                                    ((this.Parts[i].Grammar.Position == "V") ? "\t" + ((this.Parts[i].Grammar.Qualifier != null) ? (this.Parts[i].Grammar.Qualifier + "\t") : "") : "") +
                                    ((this.Parts[i].Grammar.PersonDegree != null) ? (this.Parts[i].Grammar.PersonDegree + "\t") : "") +
                                    ((this.Parts[i].Grammar.PersonGender != null) ? (this.Parts[i].Grammar.PersonGender + "\t") : "") +
                                    ((this.Parts[i].Grammar.PersonNumber != null) ? (this.Parts[i].Grammar.PersonNumber + "\t") : "") +
                                    ((this.Parts[i].Grammar.Mood != null) ? (this.Parts[i].Grammar.Mood + "\t") : "") //+
                                                                                                                      //((this.Parts[i].Grammar.Lemma != null) ? (this.Parts[i].Grammar.Lemma.ToArabic() + "\t") : "") +
                                                                                                                      //((this.Parts[i].Grammar.Root != null) ? (this.Parts[i].Grammar.Root.ToArabic() + "\t") : "") +
                                                                                                                      //((this.Parts[i].Grammar.SpecialGroup != null) ? (this.Parts[i].Grammar.SpecialGroup.ToArabic() + "\t") : "")
                                );
                                str.Remove(str.Length - 1, 1); // remove last "\t"
                                str.AppendLine();
                            }

                            if (i == this.Parts.Count - 1)
                            {
                                // finish up last part
                                if (str.Length > 2)
                                {
                                    str.Remove(str.Length - 2, 2); // "\r\n"
                                    result.Append(str.ToString());
                                }
                            }
                        }
                    }
                    grammar_text = result.ToString();
                }
                return grammar_text;
            }
        }

        private List<Letter> letters = null;
        public List<Letter> Letters
        {
            get { return letters; }
        }

        private List<char> unique_letters = null;
        public List<char> UniqueLetters
        {
            get
            {
                if (unique_letters == null)
                {
                    unique_letters = new List<char>();
                    if (this.letters != null)
                    {
                        foreach (Letter letter in this.letters)
                        {
                            if (!unique_letters.Contains(letter.Character))
                            {
                                unique_letters.Add(letter.Character);
                            }
                        }
                    }
                }
                return unique_letters;
            }
        }

        private int position = -1;
        public int Position
        {
            get { return position; }
        }

        private string text = null;
        public string Text
        {
            get { return text; }
        }
        public override string ToString()
        {
            return this.Text;
        }

        private Stopmark stopmark = Stopmark.None;
        public Stopmark Stopmark
        {
            get { return stopmark; }
            set { stopmark = value; }
        }

        public Word(Verse verse, int number_in_verse, int position, string text)
        {
            this.verse = verse;
            //this.number = number;                         // to be filled by book.SetupNumbers
            this.number_in_verse = number_in_verse;
            //this.number_in_chapter = number_in_chapter;   // to be filled by book.SetupNumbers
            this.position = position;
            this.text = text;

            string simplified_text = null;
            if (text != null)
            {
                if (text.IsArabicWithDiacritics())
                {
                    simplified_text = text.Simplify("Original");
                }
                else
                {
                    simplified_text = text;
                }
            }

            this.parts = new List<WordPart>();

            this.letters = new List<Letter>();
            if (this.letters != null)
            {
                int letter_number_in_word = 0;
                foreach (char character in simplified_text)
                {
                    letter_number_in_word++;

                    Letter letter = new Letter(this, letter_number_in_word, character);
                    this.letters.Add(letter);
                }
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
