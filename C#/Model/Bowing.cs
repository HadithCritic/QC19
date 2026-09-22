using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class Bowing
    {
        private Book book = null;
        public Book Book
        {
            get { return book; }
            set { book = value; }
        }

        private List<Chapter> chapters = null;
        public List<Chapter> Chapters
        {
            get
            {
                if (chapters == null)
                    return book.GetChapters(verses);
                else
                    return chapters;
            }
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
                                    this.letters.AddRange(word.Letters);
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

        public Bowing(Book book, int number, List<Verse> verses)
        {
            this.book = book;
            this.number = number;
            this.verses = verses;
            if (this.verses != null)
            {
                int verse_number = 1;
                foreach (Verse verse in this.verses)
                {
                    if (verse != null)
                    {
                        verse.Bowing = this;
                        verse.NumberInBowing = verse_number++;
                    }
                }
            }
        }

        public string Text
        {
            get
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
                return str.ToString();
            }
        }
        public override string ToString()
        {
            return this.Text;
        }
    }
}
