using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class Selection
    {
        private Book book = null;
        public Book Book
        {
            get { return book; }
        }

        private SelectionScope scope = SelectionScope.Book;
        public SelectionScope Scope
        {
            get { return scope; }
        }

        private List<int> indexes = null;
        public List<int> Indexes
        {
            get { return indexes; }
        }

        private List<Page> pages = null;
        public List<Page> Pages
        {
            get
            {
                return pages;
            }
        }

        private List<Station> stations = null;
        public List<Station> Stations
        {
            get
            {
                return stations;
            }
        }

        private List<Part> parts = null;
        public List<Part> Parts
        {
            get
            {
                return parts;
            }
        }

        private List<Group> groups = null;
        public List<Group> Groups
        {
            get
            {
                return groups;
            }
        }

        private List<Half> halfs = null;
        public List<Half> Halfs
        {
            get
            {
                return halfs;
            }
        }

        private List<Quarter> quarters = null;
        public List<Quarter> Quarters
        {
            get
            {
                return quarters;
            }
        }

        private List<Bowing> bowings = null;
        public List<Bowing> Bowings
        {
            get
            {
                return bowings;
            }
        }

        private List<Chapter> chapters = null;
        public List<Chapter> Chapters
        {
            get
            {
                return chapters;
            }
        }

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get
            {
                return verses;
            }
        }

        private List<Word> words = null;
        public List<Word> Words
        {
            get
            {
                return words;
            }
        }

        private List<Letter> letters = null;
        public List<Letter> Letters
        {
            get
            {
                return letters;
            }
        }

        public Selection(Book book, SelectionScope scope, List<int> indexes)
        {
            this.book = book;
            this.scope = scope;
            if (indexes != null)
            {
                this.indexes = new List<int>(indexes);

                this.chapters = new List<Chapter>();
                this.pages = new List<Page>();
                this.stations = new List<Station>();
                this.parts = new List<Part>();
                this.groups = new List<Group>();
                this.halfs = new List<Half>();
                this.quarters = new List<Quarter>();
                this.bowings = new List<Bowing>();
                this.verses = new List<Verse>();
                this.words = new List<Word>();
                this.letters = new List<Letter>();

                switch (scope)
                {
                    case SelectionScope.Book:
                        {
                            if (book.Verses != null)
                            {
                                this.verses.AddRange(book.Verses);
                            }
                        }
                        break;
                    case SelectionScope.Page:
                        {
                            if (book.Pages != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Pages.Count))
                                    {
                                        this.verses.AddRange(book.Pages[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Station:
                        {
                            if (book.Stations != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Stations.Count))
                                    {
                                        this.verses.AddRange(book.Stations[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Part:
                        {
                            if (book.Parts != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Parts.Count))
                                    {
                                        this.verses.AddRange(book.Parts[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Group:
                        {
                            if (book.Groups != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Groups.Count))
                                    {
                                        this.verses.AddRange(book.Groups[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Half:
                        {
                            if (book.Halfs != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Halfs.Count))
                                    {
                                        this.verses.AddRange(book.Halfs[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Quarter:
                        {
                            if (book.Quarters != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Quarters.Count))
                                    {
                                        this.verses.AddRange(book.Quarters[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Bowing:
                        {
                            if (book.Bowings != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Bowings.Count))
                                    {
                                        this.verses.AddRange(book.Bowings[index].Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Chapter:
                        {
                            if (book.Chapters != null)
                            {
                                foreach (int index in indexes)
                                {
                                    foreach (Chapter chapter in book.Chapters)
                                    {
                                        if ((chapter.Number - 1) == index)
                                        {
                                            verses.AddRange(chapter.Verses);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Verse:
                        {
                            if (book.Verses != null)
                            {
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Verses.Count))
                                    {
                                        this.verses.Add(book.Verses[index]);
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Word:
                        {
                            if (book.Verses != null)
                            {
                                Verse verse = null;
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Words.Count))
                                    {
                                        Word word = book.Words[index];
                                        if (verse != word.Verse)
                                        {
                                            verse = word.Verse;
                                            this.verses.Add(verse);

                                            this.words.Add(word);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case SelectionScope.Letter:
                        {
                            if (book.Verses != null)
                            {
                                Verse verse = null;
                                foreach (int index in indexes)
                                {
                                    if ((index >= 0) && (index < book.Letters.Count))
                                    {
                                        Letter letter = book.Letters[index];
                                        if (verse != letter.Word.Verse)
                                        {
                                            verse = letter.Word.Verse;
                                            this.verses.Add(verse);

                                            this.letters.Add(letter);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                foreach (Verse verse in this.verses)
                {
                    if (verse != null)
                    {
                        if (!this.chapters.Contains(verse.Chapter))
                        {
                            this.chapters.Add(verse.Chapter);
                        }
                        if (!this.pages.Contains(verse.Page))
                        {
                            this.pages.Add(verse.Page);
                        }
                        if (!this.stations.Contains(verse.Station))
                        {
                            this.stations.Add(verse.Station);
                        }
                        if (!this.parts.Contains(verse.Part))
                        {
                            this.parts.Add(verse.Part);
                        }
                        if (!this.groups.Contains(verse.Group))
                        {
                            this.groups.Add(verse.Group);
                        }
                        if (!this.halfs.Contains(verse.Half))
                        {
                            this.halfs.Add(verse.Half);
                        }
                        if (!this.quarters.Contains(verse.Quarter))
                        {
                            this.quarters.Add(verse.Quarter);
                        }
                        if (!this.bowings.Contains(verse.Bowing))
                        {
                            this.bowings.Add(verse.Bowing);
                        }

                        this.words.AddRange(verse.Words);

                        this.letters.AddRange(verse.Letters);
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
