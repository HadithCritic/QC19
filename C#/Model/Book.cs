using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Model
{
    public class Book
    {
        private int number = 0;
        public int Number
        {
            get { return number; }
        }

        private string text_mode = null;
        public string TextMode
        {
            get { return text_mode; }
            set { text_mode = value; }
        }

        private bool with_bism_Allah = true;
        public bool WithBismAllah
        {
            get { return with_bism_Allah; }
            set { with_bism_Allah = value; }
        }

        private bool waw_as_word = false;
        public bool WawAsWord
        {
            get { return waw_as_word; }
            set { waw_as_word = value; }
        }

        private bool shadda_as_letter = false;
        public bool ShaddaAsLetter
        {
            get { return shadda_as_letter; }
            set { shadda_as_letter = value; }
        }

        private bool hamza_above_horizontal_line_as_letter = true;
        public bool HamzaAboveHorizontalLineAsLetter
        {
            get { return hamza_above_horizontal_line_as_letter; }
            set { hamza_above_horizontal_line_as_letter = value; }
        }

        private bool elf_above_horizontal_line_as_letter = false;
        public bool ElfAboveHorizontalLineAsLetter
        {
            get { return elf_above_horizontal_line_as_letter; }
            set { elf_above_horizontal_line_as_letter = value; }
        }

        private bool yaa_above_horizontal_line_as_letter = false;
        public bool YaaAboveHorizontalLineAsLetter
        {
            get { return yaa_above_horizontal_line_as_letter; }
            set { yaa_above_horizontal_line_as_letter = value; }
        }

        private bool noon_above_horizontal_line_as_letter = false;
        public bool NoonAboveHorizontalLineAsLetter
        {
            get { return noon_above_horizontal_line_as_letter; }
            set { noon_above_horizontal_line_as_letter = value; }
        }

        private List<Chapter> chapters = null;
        public List<Chapter> Chapters
        {
            get { return chapters; }
        }

        private List<Station> stations = null;
        public List<Station> Stations
        {
            get { return stations; }
        }

        private List<Part> parts = null;
        public List<Part> Parts
        {
            get { return parts; }
        }

        private List<Group> groups = null;
        public List<Group> Groups
        {
            get { return groups; }
        }

        private List<Half> halfs = null;
        public List<Half> Halfs
        {
            get { return halfs; }
        }

        private List<Quarter> quarters = null;
        public List<Quarter> Quarters
        {
            get { return quarters; }
        }

        private List<Bowing> bowings = null;
        public List<Bowing> Bowings
        {
            get { return bowings; }
        }

        private List<Page> pages = null;
        public List<Page> Pages
        {
            get { return pages; }
        }

        private List<Verse> verses = null;
        public List<Verse> Verses
        {
            get { return verses; }
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

        public int GetVerseNumber(int chapter_number, int verse_number_in_chapter)
        {
            if (this.chapters != null)
            {
                foreach (Chapter chapter in this.chapters)
                {
                    if (chapter != null)
                    {
                        if (chapter.SortedNumber == chapter_number)
                        {
                            if (chapter.Verses != null)
                            {
                                foreach (Verse verse in chapter.Verses)
                                {
                                    if (verse != null)
                                    {
                                        if (verse.NumberInChapter == verse_number_in_chapter)
                                        {
                                            return verse.Number;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
        public Verse GetVerseByAddress(int chapter_number, int verse_number_in_chapter)
        {
            if (this.chapters != null)
            {
                if ((chapter_number - 1 >= 0) && (chapter_number - 1 < this.chapters.Count))
                {
                    Chapter chapter = this.chapters[chapter_number - 1];
                    if (chapter != null)
                    {
                        if ((verse_number_in_chapter - 1 >= 0) && (verse_number_in_chapter - 1 < chapter.Verses.Count))
                        {
                            return chapter.Verses[verse_number_in_chapter - 1];
                        }
                    }
                }
            }
            return null;
        }
        public Word GetWordByAddress(int chapter_number, int verse_number_in_chapter, int word_number_in_verse)
        {
            if (this.chapters != null)
            {
                if ((chapter_number - 1 >= 0) && (chapter_number - 1 < this.chapters.Count))
                {
                    Chapter chapter = this.chapters[chapter_number - 1];
                    if (chapter != null)
                    {
                        if ((verse_number_in_chapter - 1 >= 0) && (verse_number_in_chapter - 1 < chapter.Verses.Count))
                        {
                            Verse verse = chapter.Verses[verse_number_in_chapter - 1];
                            if (verse != null)
                            {
                                if ((word_number_in_verse - 1 >= 0) && (word_number_in_verse - 1 < verse.Words.Count))
                                {
                                    return verse.Words[word_number_in_verse - 1];
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Letter GetLetterByAddress(int chapter_number, int verse_number_in_chapter, int word_number_in_verse, int letter_number_in_word)
        {
            if (this.chapters != null)
            {
                if ((chapter_number - 1 >= 0) && (chapter_number - 1 < this.chapters.Count))
                {
                    Chapter chapter = this.chapters[chapter_number - 1];
                    if (chapter != null)
                    {
                        if ((verse_number_in_chapter - 1 >= 0) && (verse_number_in_chapter - 1 < chapter.Verses.Count))
                        {
                            Verse verse = chapter.Verses[verse_number_in_chapter - 1];
                            if (verse != null)
                            {
                                if ((word_number_in_verse - 1 >= 0) && (word_number_in_verse - 1 < verse.Words.Count))
                                {
                                    Word word = verse.Words[word_number_in_verse - 1];
                                    if (word != null)
                                    {
                                        if ((letter_number_in_word - 1 >= 0) && (letter_number_in_word - 1 < word.Letters.Count))
                                        {
                                            return word.Letters[letter_number_in_word - 1];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Verse GetVerseByNumber(int number)
        {
            if (number <= 0) return null;

            if (this.verses != null)
            {
                if (this.verses.Count > 0)
                {
                    int n = (number % this.verses.Count);
                    if (n == 0) n = this.verses.Count;

                    if ((n > 0) && (n <= this.verses.Count))
                    {
                        return this.verses[n - 1];
                    }
                }
            }
            return null;
        }
        public Word GetWordByNumber(int number)
        {
            if (number <= 0) return null;

            if (this.Words.Count > 0)
            {
                int n = (number % this.Words.Count);
                if (n == 0) n = this.Words.Count;

                if ((n > 0) && (n <= this.Words.Count))
                {
                    if (this.chapters != null)
                    {
                        foreach (Chapter chapter in this.chapters)
                        {
                            if (chapter != null)
                            {
                                if (chapter.Verses != null)
                                {
                                    foreach (Verse verse in chapter.Verses)
                                    {
                                        if (verse != null)
                                        {
                                            if (verse.Words != null)
                                            {
                                                int word_count = verse.Words.Count;
                                                if (n > word_count)
                                                {
                                                    n -= word_count;
                                                }
                                                else
                                                {
                                                    return verse.Words[n - 1];
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
            return null;
        }
        public Letter GetLetterByNumber(int number)
        {
            if (number <= 0) return null;

            if (this.Letters.Count > 0)
            {
                int n = (number % this.Letters.Count);
                if (n == 0) n = this.Letters.Count;

                if ((n > 0) && (n <= this.Letters.Count))
                {
                    if (this.chapters != null)
                    {
                        foreach (Chapter chapter in this.chapters)
                        {
                            if (chapter != null)
                            {
                                if (chapter.Verses != null)
                                {
                                    foreach (Verse verse in chapter.Verses)
                                    {
                                        if (verse != null)
                                        {
                                            if (verse.Words != null)
                                            {
                                                foreach (Word word in verse.Words)
                                                {
                                                    if ((word.Letters != null) && (word.Letters.Count > 0))
                                                    {
                                                        int letter_count = word.Letters.Count;
                                                        if (n > letter_count)
                                                        {
                                                            n -= letter_count;
                                                        }
                                                        else
                                                        {
                                                            return word.Letters[n - 1];
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
                }
            }
            return null;
        }
        public Verse GetVerseByAddress(string address)
        {
            try
            {
                string[] address_parts = address.Split(':');
                if (address_parts.Length == 2)
                {
                    int chapter_number = int.Parse(address_parts[0]);
                    int verse_number_in_chapter = int.Parse(address_parts[1]);
                    return GetVerseByAddress(chapter_number, verse_number_in_chapter);
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
        public Word GetWordByAddress(string address)
        {
            try
            {
                string[] address_parts = address.Split(':');
                if (address_parts.Length == 3)
                {
                    int chapter_number = int.Parse(address_parts[0]);
                    int verse_number_in_chapter = int.Parse(address_parts[1]);
                    int word_number_in_verse = int.Parse(address_parts[2]);
                    return GetWordByAddress(chapter_number, verse_number_in_chapter, word_number_in_verse);
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
        public Letter GetLetterByAddress(string address)
        {
            try
            {
                string[] address_parts = address.Split(':');
                if (address_parts.Length == 4)
                {
                    int chapter_number = int.Parse(address_parts[0]);
                    int verse_number_in_chapter = int.Parse(address_parts[1]);
                    int word_number_in_verse = int.Parse(address_parts[2]);
                    int letter_number_in_word = int.Parse(address_parts[3]);
                    return GetLetterByAddress(chapter_number, verse_number_in_chapter, word_number_in_verse, letter_number_in_word);
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        private static int id = 0;
        public Book(string text_mode, List<Verse> verses, bool distances_within_chapters, bool with_diacritics)
        {
            this.number = id++;
            this.text_mode = text_mode;

            SetupPartitions(verses);
            SetupNumbers();
            SetupFrequenciesAndOccurrences(with_diacritics);
            SetupDistances(distances_within_chapters, with_diacritics);
        }
        private void SetupPartitions(List<Verse> verses)
        {
            if (verses != null)
            {
                this.verses = verses;
                // already set by Verse constructor
                //int verse_number = 1;
                foreach (Verse verse in this.verses)
                {
                    if (verse != null)
                    {
                        verse.Book = this;
                        //verse.Number = verse_number++;
                    }
                }

                this.chapters = new List<Chapter>();
                this.stations = new List<Station>();
                this.parts = new List<Part>();
                this.groups = new List<Group>();
                this.halfs = new List<Half>();
                this.quarters = new List<Quarter>();
                this.bowings = new List<Bowing>();
                this.pages = new List<Page>();

                this.min_words = 1;
                int word_count = 0;
                foreach (Verse verse in this.verses)
                {
                    if (verse != null)
                    {
                        if (verse.Words != null)
                        {
                            word_count += verse.Words.Count;
                        }
                    }
                }
                this.max_words = word_count;

                this.min_letters = 1;
                this.max_letters = int.MaxValue; // verse.Letters is not populated yet

                if (s_quran_metadata == null)
                {
                    LoadQuranMetadata();
                }

                if (s_quran_metadata != null)
                {
                    // setup Chapters
                    for (int i = 0; i < s_quran_metadata.Chapters.Count; i++)
                    {
                        int number = s_quran_metadata.Chapters[i].Number;
                        int verse_count = s_quran_metadata.Chapters[i].Verses;
                        int first_verse = s_quran_metadata.Chapters[i].FirstVerse;
                        int last_verse = first_verse + verse_count;
                        string name = s_quran_metadata.Chapters[i].Name;
                        string transliterated_name = s_quran_metadata.Chapters[i].TransliteratedName;
                        string english_name = s_quran_metadata.Chapters[i].EnglishName;
                        RevelationPlace revelation_place = s_quran_metadata.Chapters[i].RevelationPlace;
                        int revelation_order = s_quran_metadata.Chapters[i].RevelationOrder;
                        int bowing_count = s_quran_metadata.Chapters[i].Bowings;

                        List<Verse> chapter_verses = new List<Verse>();
                        if (this.verses != null)
                        {
                            for (int j = first_verse; j < last_verse; j++)
                            {
                                int index = j - 1;
                                if ((index >= 0) && (index < this.verses.Count))
                                {
                                    Verse verse = this.verses[index];
                                    chapter_verses.Add(verse);
                                }
                            }

                            Chapter chapter = new Chapter(this, number, name, transliterated_name, english_name, revelation_place, revelation_order, bowing_count, chapter_verses);
                            this.chapters.Add(chapter);
                        }
                    }

                    // setup Stations
                    for (int i = 0; i < s_quran_metadata.Stations.Count; i++)
                    {
                        int number = s_quran_metadata.Stations[i].Number;
                        int start_chapter = s_quran_metadata.Stations[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Stations[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_station_first_verse = 0;
                        if (i < s_quran_metadata.Stations.Count - 1)
                        {
                            int next_station_start_chapter = s_quran_metadata.Stations[i + 1].StartChapter;
                            int next_station_start_chapter_verse = s_quran_metadata.Stations[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_station_start_chapter - 1; j++)
                            {
                                next_station_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_station_first_verse += next_station_start_chapter_verse;
                        }
                        else
                        {
                            next_station_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_station_first_verse;

                        List<Verse> station_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                station_verses.Add(verse);
                            }
                        }

                        Station station = new Station(this, number, station_verses);
                        this.stations.Add(station);
                    }

                    // setup Parts
                    for (int i = 0; i < s_quran_metadata.Parts.Count; i++)
                    {
                        int number = s_quran_metadata.Parts[i].Number;
                        int start_chapter = s_quran_metadata.Parts[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Parts[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_part_first_verse = 0;
                        if (i < s_quran_metadata.Parts.Count - 1)
                        {
                            int next_part_start_chapter = s_quran_metadata.Parts[i + 1].StartChapter;
                            int next_part_start_chapter_verse = s_quran_metadata.Parts[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_part_start_chapter - 1; j++)
                            {
                                next_part_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_part_first_verse += next_part_start_chapter_verse;
                        }
                        else
                        {
                            next_part_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_part_first_verse;

                        List<Verse> part_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                part_verses.Add(verse);
                            }
                        }

                        Part part = new Part(this, number, part_verses);
                        this.parts.Add(part);
                    }

                    // setup Group
                    for (int i = 0; i < s_quran_metadata.Groups.Count; i++)
                    {
                        int number = s_quran_metadata.Groups[i].Number;
                        int start_chapter = s_quran_metadata.Groups[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Groups[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_group_first_verse = 0;
                        if (i < s_quran_metadata.Groups.Count - 1)
                        {
                            int next_group_start_chapter = s_quran_metadata.Groups[i + 1].StartChapter;
                            int next_group_start_chapter_verse = s_quran_metadata.Groups[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_group_start_chapter - 1; j++)
                            {
                                next_group_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_group_first_verse += next_group_start_chapter_verse;
                        }
                        else
                        {
                            next_group_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_group_first_verse;

                        List<Verse> group_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                group_verses.Add(verse);
                            }
                        }

                        Group group = new Group(this, number, group_verses);
                        this.groups.Add(group);
                    }

                    // setup Halfs
                    for (int i = 0; i < s_quran_metadata.Halfs.Count; i++)
                    {
                        int number = s_quran_metadata.Halfs[i].Number;
                        int start_chapter = s_quran_metadata.Halfs[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Halfs[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_half_first_verse = 0;
                        if (i < s_quran_metadata.Halfs.Count - 1)
                        {
                            int next_half_start_chapter = s_quran_metadata.Halfs[i + 1].StartChapter;
                            int next_half_start_chapter_verse = s_quran_metadata.Halfs[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_half_start_chapter - 1; j++)
                            {
                                next_half_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_half_first_verse += next_half_start_chapter_verse;
                        }
                        else
                        {
                            next_half_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_half_first_verse;

                        List<Verse> half_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                half_verses.Add(verse);
                            }
                        }

                        Half half = new Half(this, number, half_verses);
                        this.halfs.Add(half);
                    }

                    // setup Quarters
                    for (int i = 0; i < s_quran_metadata.Quarters.Count; i++)
                    {
                        int number = s_quran_metadata.Quarters[i].Number;
                        int start_chapter = s_quran_metadata.Quarters[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Quarters[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_quarter_first_verse = 0;
                        if (i < s_quran_metadata.Quarters.Count - 1)
                        {
                            int next_quarter_start_chapter = s_quran_metadata.Quarters[i + 1].StartChapter;
                            int next_quarter_start_chapter_verse = s_quran_metadata.Quarters[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_quarter_start_chapter - 1; j++)
                            {
                                next_quarter_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_quarter_first_verse += next_quarter_start_chapter_verse;
                        }
                        else
                        {
                            next_quarter_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_quarter_first_verse;

                        List<Verse> quarter_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                quarter_verses.Add(verse);
                            }
                        }

                        Quarter quarter = new Quarter(this, number, quarter_verses);
                        this.quarters.Add(quarter);
                    }

                    // setup Bowings
                    for (int i = 0; i < s_quran_metadata.Bowings.Count; i++)
                    {
                        int number = s_quran_metadata.Bowings[i].Number;
                        int start_chapter = s_quran_metadata.Bowings[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Bowings[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_bowing_first_verse = 0;
                        if (i < s_quran_metadata.Bowings.Count - 1)
                        {
                            int next_bowing_start_chapter = s_quran_metadata.Bowings[i + 1].StartChapter;
                            int next_bowing_start_chapter_verse = s_quran_metadata.Bowings[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_bowing_start_chapter - 1; j++)
                            {
                                next_bowing_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_bowing_first_verse += next_bowing_start_chapter_verse;
                        }
                        else
                        {
                            next_bowing_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_bowing_first_verse;

                        List<Verse> bowing_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                bowing_verses.Add(verse);
                            }
                        }

                        Bowing bowing = new Bowing(this, number, bowing_verses);
                        this.bowings.Add(bowing);
                    }

                    // setup Pages
                    for (int i = 0; i < s_quran_metadata.Pages.Count; i++)
                    {
                        int number = s_quran_metadata.Pages[i].Number;
                        int start_chapter = s_quran_metadata.Pages[i].StartChapter;
                        int start_chapter_verse = s_quran_metadata.Pages[i].StartChapterVerse;

                        int first_verse = 0;
                        for (int j = 0; j < start_chapter - 1; j++)
                        {
                            first_verse += this.chapters[j].Verses.Count;
                        }
                        first_verse += start_chapter_verse;

                        int next_page_first_verse = 0;
                        if (i < s_quran_metadata.Pages.Count - 1)
                        {
                            int next_page_start_chapter = s_quran_metadata.Pages[i + 1].StartChapter;
                            int next_page_start_chapter_verse = s_quran_metadata.Pages[i + 1].StartChapterVerse;
                            for (int j = 0; j < next_page_start_chapter - 1; j++)
                            {
                                next_page_first_verse += this.chapters[j].Verses.Count;
                            }
                            next_page_first_verse += next_page_start_chapter_verse;
                        }
                        else
                        {
                            next_page_first_verse = this.verses.Count + 1; // beyond end
                        }

                        int last_verse = next_page_first_verse;

                        List<Verse> page_verses = new List<Verse>();
                        for (int j = first_verse; j < last_verse; j++)
                        {
                            int index = j - 1;
                            if ((index >= 0) && (index < this.verses.Count))
                            {
                                Verse verse = this.verses[index];
                                page_verses.Add(verse);
                            }
                        }

                        Page page = new Page(this, number, page_verses);
                        this.pages.Add(page);
                    }

                    // setup Prostration
                    for (int i = 0; i < s_quran_metadata.Prostrations.Count; i++)
                    {
                        int number = s_quran_metadata.Prostrations[i].Number;
                        int chapter = s_quran_metadata.Prostrations[i].Chapter;
                        int chapter_verse = s_quran_metadata.Prostrations[i].ChapterVerse;
                        ProstrationType type = s_quran_metadata.Prostrations[i].Type;

                        int index = -1;
                        for (int j = 0; j < chapter - 1; j++)
                        {
                            index += this.chapters[j].Verses.Count;
                        }
                        index += chapter_verse;

                        if ((index > 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verse.ProstrationType = type;
                            if (verse.ProstrationType == ProstrationType.Recommended)
                            {
                                verse.Text = verse.Text.Replace("۩", "⌂");
                            }
                        }
                    }

                    // setup Initialization
                    for (int i = 0; i < s_quran_metadata.Initializations.Count; i++)
                    {
                        int number = s_quran_metadata.Initializations[i].Number;
                        int chapter = s_quran_metadata.Initializations[i].Chapter;
                        int chapter_verse = s_quran_metadata.Initializations[i].ChapterVerse;
                        InitializationType type = s_quran_metadata.Initializations[i].Type;

                        int index = -1;
                        for (int j = 0; j < chapter - 1; j++)
                        {
                            index += this.chapters[j].Verses.Count;
                        }
                        index += chapter_verse;

                        if ((index > 0) && (index < this.verses.Count))
                        {
                            Verse verse = this.verses[index];
                            verse.InitializationType = type;

                            if (verse.Chapter.Number == 1)
                            {
                                this.chapters[chapter - 1].InitializationType = InitializationType.Key;
                            }
                            else if (verse.Chapter.Number == 42)
                            {
                                this.chapters[chapter - 1].InitializationType = InitializationType.DoublyInitialized;
                            }
                            else
                            {
                                this.chapters[chapter - 1].InitializationType = type;
                            }
                        }
                    }
                }
            }
        }
        public void SetupNumbers()
        {
            int chapter_number = 1;
            int verse_number = 1;
            int word_number = 1;
            int letter_number = 1;

            if (this.chapters != null)
            {
                foreach (Chapter chapter in this.chapters)
                {
                    if (chapter != null)
                    {
                        chapter.SortedNumber = chapter_number++;

                        int verse_number_in_chapter = 1;
                        int word_number_in_chapter = 1;
                        int letter_number_in_chapter = 1;
                        foreach (Verse verse in chapter.Verses)
                        {
                            if (verse != null)
                            {
                                verse.Number = verse_number++;
                                verse.NumberInChapter = verse_number_in_chapter++;

                                int word_number_in_verse = 1;
                                int letter_number_in_verse = 1;
                                if (verse.Words != null)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        if (word != null)
                                        {
                                            word.Number = word_number++;
                                            word.NumberInChapter = word_number_in_chapter++;
                                            word.NumberInVerse = word_number_in_verse++;

                                            int letter_number_in_word = 1;
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter.Number = letter_number++;
                                                    letter.NumberInChapter = letter_number_in_chapter++;
                                                    letter.NumberInVerse = letter_number_in_verse++;
                                                    letter.NumberInWord = letter_number_in_word++;
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
        }
        public void SetupFrequenciesAndOccurrences(bool with_diacritics)
        {
            Dictionary<int, Dictionary<string, int>> letter_frequenciess = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> word_frequenciess = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> verse_frequenciess = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> letter_frequencies_in_chapters = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> word_frequencies_in_chapters = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> verse_frequencies_in_chapters = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> letter_frequencies_in_verses = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> word_frequencies_in_verses = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> letter_frequencies_in_words = new Dictionary<int, Dictionary<string, int>>();

            Dictionary<string, int> letter_frequencies = new Dictionary<string, int>();
            Dictionary<string, int> word_frequencies = new Dictionary<string, int>();
            Dictionary<string, int> verse_frequencies = new Dictionary<string, int>();
            letter_frequenciess.Add(this.Number, letter_frequencies);
            word_frequenciess.Add(this.Number, word_frequencies);
            verse_frequenciess.Add(this.Number, verse_frequencies);
            foreach (Chapter chapter in this.Chapters)
            {
                if (chapter != null)
                {
                    Dictionary<string, int> letter_frequencies_in_chapter = new Dictionary<string, int>();
                    Dictionary<string, int> word_frequencies_in_chapter = new Dictionary<string, int>();
                    Dictionary<string, int> verse_frequencies_in_chapter = new Dictionary<string, int>();
                    letter_frequencies_in_chapters.Add(chapter.SortedNumber, letter_frequencies_in_chapter);
                    word_frequencies_in_chapters.Add(chapter.SortedNumber, word_frequencies_in_chapter);
                    verse_frequencies_in_chapters.Add(chapter.SortedNumber, verse_frequencies_in_chapter);
                    foreach (Verse verse in chapter.Verses)
                    {
                        if (verse != null)
                        {
                            string verse_text = verse.Text;
                            if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                            {
                                verse_text = verse_text.Simplify29();
                            }
                            verse_text = verse_text.Trim();

                            if (verse_frequencies_in_chapter.ContainsKey(verse_text))
                            {
                                verse_frequencies_in_chapter[verse_text]++;
                                verse.OccurrenceInChapter = verse_frequencies_in_chapter[verse_text];
                            }
                            else
                            {
                                verse_frequencies_in_chapter.Add(verse_text, 1);
                                verse.OccurrenceInChapter = 1;
                            }

                            if (verse_frequencies.ContainsKey(verse_text))
                            {
                                verse_frequencies[verse_text]++;
                                verse.Occurrence = verse_frequencies[verse_text];
                            }
                            else
                            {
                                verse_frequencies.Add(verse_text, 1);
                                verse.Occurrence = 1;
                            }

                            Dictionary<string, int> letter_frequencies_in_verse = new Dictionary<string, int>();
                            Dictionary<string, int> word_frequencies_in_verse = new Dictionary<string, int>();
                            letter_frequencies_in_verses.Add(verse.Number, letter_frequencies_in_verse);
                            word_frequencies_in_verses.Add(verse.Number, word_frequencies_in_verse);
                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    string word_text = word.Text;
                                    if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                    {
                                        word_text = word_text.Simplify29();
                                    }
                                    word_text = word_text.Trim();

                                    if (word_frequencies_in_verse.ContainsKey(word_text))
                                    {
                                        word_frequencies_in_verse[word_text]++;
                                        word.OccurrenceInVerse = word_frequencies_in_verse[word_text];
                                    }
                                    else
                                    {
                                        word_frequencies_in_verse.Add(word_text, 1);
                                        word.OccurrenceInVerse = 1;
                                    }

                                    if (word_frequencies_in_chapter.ContainsKey(word_text))
                                    {
                                        word_frequencies_in_chapter[word_text]++;
                                        word.OccurrenceInChapter = word_frequencies_in_chapter[word_text];
                                    }
                                    else
                                    {
                                        word_frequencies_in_chapter.Add(word_text, 1);
                                        word.OccurrenceInChapter = 1;
                                    }

                                    if (word_frequencies.ContainsKey(word_text))
                                    {
                                        word_frequencies[word_text]++;
                                        word.Occurrence = word_frequencies[word_text];
                                    }
                                    else
                                    {
                                        word_frequencies.Add(word_text, 1);
                                        word.Occurrence = 1;
                                    }

                                    Dictionary<string, int> letter_frequencies_in_word = new Dictionary<string, int>();
                                    letter_frequencies_in_words.Add(word.Number, letter_frequencies_in_word);
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            string letter_text = letter.Text;
                                            if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                            {
                                                letter_text = letter_text.Simplify29();
                                            }
                                            letter_text = letter_text.Trim();

                                            if (letter_frequencies_in_word.ContainsKey(letter_text))
                                            {
                                                letter_frequencies_in_word[letter_text]++;
                                                letter.OccurrenceInWord = letter_frequencies_in_word[letter_text];
                                            }
                                            else
                                            {
                                                letter_frequencies_in_word.Add(letter_text, 1);
                                                letter.OccurrenceInWord = 1;
                                            }

                                            if (letter_frequencies_in_verse.ContainsKey(letter_text))
                                            {
                                                letter_frequencies_in_verse[letter_text]++;
                                                letter.OccurrenceInVerse = letter_frequencies_in_verse[letter_text];
                                            }
                                            else
                                            {
                                                letter_frequencies_in_verse.Add(letter_text, 1);
                                                letter.OccurrenceInVerse = 1;
                                            }

                                            if (letter_frequencies_in_chapter.ContainsKey(letter_text))
                                            {
                                                letter_frequencies_in_chapter[letter_text]++;
                                                letter.OccurrenceInChapter = letter_frequencies_in_chapter[letter_text];
                                            }
                                            else
                                            {
                                                letter_frequencies_in_chapter.Add(letter_text, 1);
                                                letter.OccurrenceInChapter = 1;
                                            }

                                            if (letter_frequencies.ContainsKey(letter_text))
                                            {
                                                letter_frequencies[letter_text]++;
                                                letter.Occurrence = letter_frequencies[letter_text];
                                            }
                                            else
                                            {
                                                letter_frequencies.Add(letter_text, 1);
                                                letter.Occurrence = 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (Chapter chapter in this.Chapters)
            {
                if (chapter != null)
                {
                    foreach (Verse verse in chapter.Verses)
                    {
                        if (verse != null)
                        {
                            string verse_text = verse.Text;
                            if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                            {
                                verse_text = verse_text.Simplify29();
                            }
                            verse_text = verse_text.Trim();

                            if (verse_frequencies_in_chapters[chapter.SortedNumber].ContainsKey(verse_text))
                            {
                                verse.FrequencyInChapter = verse_frequencies_in_chapters[chapter.SortedNumber][verse_text];
                            }
                            if (verse_frequenciess[this.Number].ContainsKey(verse_text))
                            {
                                verse.Frequency = verse_frequenciess[this.Number][verse_text];
                            }

                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    string word_text = word.Text;
                                    if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                    {
                                        word_text = word_text.Simplify29();
                                    }
                                    word_text = word_text.Trim();

                                    if (word_frequencies_in_verses[verse.Number].ContainsKey(word_text))
                                    {
                                        word.FrequencyInVerse = word_frequencies_in_verses[verse.Number][word_text];
                                    }
                                    if (word_frequencies_in_chapters[chapter.SortedNumber].ContainsKey(word_text))
                                    {
                                        word.FrequencyInChapter = word_frequencies_in_chapters[chapter.SortedNumber][word_text];
                                    }
                                    if (word_frequenciess[this.Number].ContainsKey(word_text))
                                    {
                                        word.Frequency = word_frequenciess[this.Number][word_text];
                                    }

                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            string letter_text = letter.Text;
                                            if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                            {
                                                letter_text = letter_text.Simplify29();
                                            }
                                            letter_text = letter_text.Trim();

                                            if (letter_frequencies_in_words[word.Number].ContainsKey(letter_text))
                                            {
                                                letter.FrequencyInWord = letter_frequencies_in_words[word.Number][letter_text];
                                            }
                                            if (letter_frequencies_in_verses[verse.Number].ContainsKey(letter_text))
                                            {
                                                letter.FrequencyInVerse = letter_frequencies_in_verses[verse.Number][letter_text];
                                            }
                                            if (letter_frequencies_in_chapters[chapter.SortedNumber].ContainsKey(letter_text))
                                            {
                                                letter.FrequencyInChapter = letter_frequencies_in_chapters[chapter.SortedNumber][letter_text];
                                            }
                                            if (letter_frequenciess[this.Number].ContainsKey(letter_text))
                                            {
                                                letter.Frequency = letter_frequenciess[this.Number][letter_text];
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
        // faster code called on NumericalSystem change
        public void SetupWordFrequenciesAndOccurrences(bool with_diacritics)
        {
            Dictionary<int, Dictionary<string, int>> word_frequenciess = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> word_frequenciess_by_root = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<long, int>> word_frequenciess_by_value = new Dictionary<int, Dictionary<long, int>>();

            Dictionary<string, int> word_frequencies = new Dictionary<string, int>();
            Dictionary<string, int> word_frequencies_by_root = new Dictionary<string, int>();
            Dictionary<long, int> word_frequencies_by_value = new Dictionary<long, int>();

            word_frequenciess.Add(this.Number, word_frequencies);
            word_frequenciess_by_root.Add(this.Number, word_frequencies_by_root);
            word_frequenciess_by_value.Add(this.Number, word_frequencies_by_value);
            foreach (Chapter chapter in this.Chapters)
            {
                if (chapter != null)
                {
                    foreach (Verse verse in chapter.Verses)
                    {
                        if (verse != null)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    string word_text = word.Text;
                                    if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                    {
                                        word_text = word_text.Simplify29();
                                    }
                                    word_text = word_text.Trim();

                                    if (word_frequencies.ContainsKey(word_text))
                                    {
                                        word_frequencies[word_text]++;
                                        word.Occurrence = word_frequencies[word_text];
                                    }
                                    else
                                    {
                                        word_frequencies.Add(word_text, 1);
                                        word.Occurrence = 1;
                                    }

                                    string word_root = word.Root;
                                    if (word_frequencies_by_root.ContainsKey(word_root))
                                    {
                                        word_frequencies_by_root[word_root]++;
                                        word.OccurrenceByRoot = word_frequencies_by_root[word_root];
                                    }
                                    else
                                    {
                                        word_frequencies_by_root.Add(word_root, 1);
                                        word.OccurrenceByRoot = 1;
                                    }

                                    long word_value = word.Value;
                                    if (word_frequencies_by_value.ContainsKey(word_value))
                                    {
                                        word_frequencies_by_value[word_value]++;
                                        word.OccurrenceByValue = word_frequencies_by_value[word_value];
                                    }
                                    else
                                    {
                                        word_frequencies_by_value.Add(word_value, 1);
                                        word.OccurrenceByValue = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (Chapter chapter in this.Chapters)
            {
                if (chapter != null)
                {
                    foreach (Verse verse in chapter.Verses)
                    {
                        if (verse != null)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    string word_text = word.Text;
                                    string word_root = word.Root;
                                    long word_value = word.Value;
                                    if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                    {
                                        word_text = word_text.Simplify29();
                                    }
                                    word_text = word_text.Trim();

                                    if (word_frequenciess[this.Number].ContainsKey(word_text))
                                    {
                                        word.Frequency = word_frequenciess[this.Number][word_text];
                                    }
                                    if (word_frequenciess_by_root[this.Number].ContainsKey(word_root))
                                    {
                                        word.FrequencyByRoot = word_frequenciess_by_root[this.Number][word_root];
                                    }
                                    if (word_frequenciess_by_value[this.Number].ContainsKey(word_value))
                                    {
                                        word.FrequencyByValue = word_frequenciess_by_value[this.Number][word_value];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void SetupDistances(bool distances_within_chapters, bool with_diacritics)
        {
            SetupDistancesToPrevious(distances_within_chapters, with_diacritics);
            SetupDistancesToNext(distances_within_chapters, with_diacritics);
        }
        private void SetupDistancesToPrevious(bool distances_within_chapters, bool with_diacritics)
        {
            // foreach chapter: no repeated chapters, no distances to previous occurrence

            // foreach verse: calculate distance to previous occurrence
            Dictionary<string, int> verse_previous_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> verse_previous_chapter_numbers = new Dictionary<string, int>();

            // foreach word: calculate distance to previous occurrence
            Dictionary<string, int> word_previous_word_numbers = new Dictionary<string, int>();
            Dictionary<string, int> word_previous_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> word_previous_chapter_numbers = new Dictionary<string, int>();

            // foreach letter: calculate distance to previous occurrence
            Dictionary<string, int> letter_previous_letter_numbers = new Dictionary<string, int>();
            Dictionary<string, int> letter_previous_word_numbers = new Dictionary<string, int>();
            Dictionary<string, int> letter_previous_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> letter_previous_chapter_numbers = new Dictionary<string, int>();

            if (chapters != null)
            {
                for (int c = 0; c < chapters.Count; c++)
                {
                    if (chapters[c] != null)
                    {
                        if (distances_within_chapters)
                        {
                            verse_previous_verse_numbers.Clear();
                            verse_previous_chapter_numbers.Clear();
                            word_previous_word_numbers.Clear();
                            word_previous_verse_numbers.Clear();
                            word_previous_chapter_numbers.Clear();
                            letter_previous_letter_numbers.Clear();
                            letter_previous_word_numbers.Clear();
                            letter_previous_verse_numbers.Clear();
                            letter_previous_chapter_numbers.Clear();
                        }

                        if (chapters[c].Verses != null)
                        {
                            List<Verse> verses = chapters[c].Verses;
                            for (int v = 0; v < verses.Count; v++)
                            {
                                if (verses[v] != null)
                                {
                                    string verse_key = verses[v].Text;
                                    if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                    {
                                        verse_key = verse_key.Simplify29();
                                    }
                                    verse_key = verse_key.Trim();

                                    if (!verse_previous_verse_numbers.ContainsKey(verse_key))
                                    {
                                        verses[v].DistanceToPrevious.dL = -1; // non-applicable
                                        verses[v].DistanceToPrevious.dW = -1; // non-applicable
                                        verses[v].DistanceToPrevious.dV = 0;
                                        verses[v].DistanceToPrevious.dC = 0;

                                        verse_previous_verse_numbers.Add(verse_key, verses[v].Number);
                                        verse_previous_chapter_numbers.Add(verse_key, chapters[c].SortedNumber);
                                    }
                                    else
                                    {
                                        verses[v].DistanceToPrevious.dL = -1; // non-applicable
                                        verses[v].DistanceToPrevious.dW = -1; // non-applicable
                                        verses[v].DistanceToPrevious.dV = verses[v].Number - verse_previous_verse_numbers[verse_key];
                                        verses[v].DistanceToPrevious.dC = chapters[c].SortedNumber - verse_previous_chapter_numbers[verse_key];

                                        // save latest chapter and verse numbers for next iteration
                                        verse_previous_verse_numbers[verse_key] = verses[v].Number;
                                        verse_previous_chapter_numbers[verse_key] = chapters[c].SortedNumber;
                                    }

                                    if (verses[v].Words != null)
                                    {
                                        List<Word> words = verses[v].Words;
                                        for (int w = 0; w < words.Count; w++)
                                        {
                                            if (words[w] != null)
                                            {
                                                string word_key = words[w].Text;
                                                if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                                {
                                                    word_key = word_key.Simplify29();
                                                }
                                                word_key = word_key.Trim();

                                                if (!word_previous_word_numbers.ContainsKey(word_key))
                                                {
                                                    words[w].DistanceToPrevious.dL = -1; // non-applicable
                                                    words[w].DistanceToPrevious.dW = 0;
                                                    words[w].DistanceToPrevious.dV = 0;
                                                    words[w].DistanceToPrevious.dC = 0;

                                                    word_previous_word_numbers.Add(word_key, words[w].Number);
                                                    word_previous_verse_numbers.Add(word_key, verses[v].Number);
                                                    word_previous_chapter_numbers.Add(word_key, chapters[c].SortedNumber);
                                                }
                                                else
                                                {
                                                    words[w].DistanceToPrevious.dL = -1; // non-applicable
                                                    words[w].DistanceToPrevious.dW = words[w].Number - word_previous_word_numbers[word_key];
                                                    words[w].DistanceToPrevious.dV = verses[v].Number - word_previous_verse_numbers[word_key];
                                                    words[w].DistanceToPrevious.dC = chapters[c].SortedNumber - word_previous_chapter_numbers[word_key];

                                                    // save latest chapter, verse and word numbers for next iteration
                                                    word_previous_word_numbers[word_key] = words[w].Number;
                                                    word_previous_verse_numbers[word_key] = verses[v].Number;
                                                    word_previous_chapter_numbers[word_key] = chapters[c].SortedNumber;
                                                }

                                                if (words[w].Letters != null)
                                                {
                                                    List<Letter> letters = words[w].Letters;
                                                    for (int l = 0; l < letters.Count; l++)
                                                    {
                                                        if (letters[l] != null)
                                                        {
                                                            string letter_key = letters[l].Text;
                                                            if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                                            {
                                                                letter_key = letter_key.Simplify29();
                                                            }
                                                            letter_key = letter_key.Trim();

                                                            if (!letter_previous_letter_numbers.ContainsKey(letter_key))
                                                            {
                                                                letters[l].DistanceToPrevious.dL = 0;
                                                                letters[l].DistanceToPrevious.dW = 0;
                                                                letters[l].DistanceToPrevious.dV = 0;
                                                                letters[l].DistanceToPrevious.dC = 0;

                                                                letter_previous_letter_numbers.Add(letter_key, letters[l].Number);
                                                                letter_previous_word_numbers.Add(letter_key, words[w].Number);
                                                                letter_previous_verse_numbers.Add(letter_key, verses[v].Number);
                                                                letter_previous_chapter_numbers.Add(letter_key, chapters[c].SortedNumber);
                                                            }
                                                            else
                                                            {
                                                                letters[l].DistanceToPrevious.dL = letters[l].Number - letter_previous_letter_numbers[letter_key];
                                                                letters[l].DistanceToPrevious.dW = words[w].Number - letter_previous_word_numbers[letter_key];
                                                                letters[l].DistanceToPrevious.dV = verses[v].Number - letter_previous_verse_numbers[letter_key];
                                                                letters[l].DistanceToPrevious.dC = chapters[c].SortedNumber - letter_previous_chapter_numbers[letter_key];

                                                                // save latest chapter, verse, word and letter numbers for next iteration
                                                                letter_previous_letter_numbers[letter_key] = letters[l].Number;
                                                                letter_previous_word_numbers[letter_key] = words[w].Number;
                                                                letter_previous_verse_numbers[letter_key] = verses[v].Number;
                                                                letter_previous_chapter_numbers[letter_key] = chapters[c].SortedNumber;
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
                    }
                }
            }
        }
        private void SetupDistancesToNext(bool distances_within_chapters, bool with_diacritics)
        {
            // foreach chapter: no repeated chapters, no distances to next occurrence

            // foreach verse: calculate distance to next occurrence
            Dictionary<string, int> verse_next_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> verse_next_chapter_numbers = new Dictionary<string, int>();

            // foreach word: calculate distance to next occurrence
            Dictionary<string, int> word_next_word_numbers = new Dictionary<string, int>();
            Dictionary<string, int> word_next_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> word_next_chapter_numbers = new Dictionary<string, int>();

            // foreach letter: calculate distance to next occurrence
            Dictionary<string, int> letter_next_letter_numbers = new Dictionary<string, int>();
            Dictionary<string, int> letter_next_word_numbers = new Dictionary<string, int>();
            Dictionary<string, int> letter_next_verse_numbers = new Dictionary<string, int>();
            Dictionary<string, int> letter_next_chapter_numbers = new Dictionary<string, int>();

            if (chapters != null)
            {
                for (int c = chapters.Count - 1; c >= 0; c--)
                {
                    if (chapters[c] != null)
                    {
                        if (distances_within_chapters)
                        {
                            verse_next_verse_numbers.Clear();
                            verse_next_chapter_numbers.Clear();
                            word_next_word_numbers.Clear();
                            word_next_verse_numbers.Clear();
                            word_next_chapter_numbers.Clear();
                            letter_next_letter_numbers.Clear();
                            letter_next_word_numbers.Clear();
                            letter_next_verse_numbers.Clear();
                            letter_next_chapter_numbers.Clear();
                        }

                        if (chapters[c].Verses != null)
                        {
                            List<Verse> verses = chapters[c].Verses;
                            for (int v = verses.Count - 1; v >= 0; v--)
                            {
                                if (verses[v] != null)
                                {
                                    string verse_key = verses[v].Text;
                                    if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                    {
                                        verse_key = verse_key.Simplify29();
                                    }
                                    verse_key = verse_key.Trim();

                                    if (!verse_next_verse_numbers.ContainsKey(verse_key))
                                    {
                                        verses[v].DistanceToNext.dL = -1; // non-applicable
                                        verses[v].DistanceToNext.dW = -1; // non-applicable
                                        verses[v].DistanceToNext.dV = 0;
                                        verses[v].DistanceToNext.dC = 0;

                                        verse_next_verse_numbers.Add(verse_key, verses[v].Number);
                                        verse_next_chapter_numbers.Add(verse_key, chapters[c].SortedNumber);
                                    }
                                    else
                                    {
                                        verses[v].DistanceToNext.dL = -1; // non-applicable
                                        verses[v].DistanceToNext.dW = -1; // non-applicable
                                        verses[v].DistanceToNext.dV = -1 * (verses[v].Number - verse_next_verse_numbers[verse_key]);
                                        verses[v].DistanceToNext.dC = -1 * (chapters[c].SortedNumber - verse_next_chapter_numbers[verse_key]);

                                        // save latest chapter and verse numbers for next iteration
                                        verse_next_verse_numbers[verse_key] = verses[v].Number;
                                        verse_next_chapter_numbers[verse_key] = chapters[c].SortedNumber;
                                    }

                                    if (verses[v].Words != null)
                                    {
                                        List<Word> words = verses[v].Words;
                                        for (int w = words.Count - 1; w >= 0; w--)
                                        {
                                            if (words[w] != null)
                                            {
                                                string word_key = words[w].Text;
                                                if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                                {
                                                    word_key = word_key.Simplify29();
                                                }
                                                word_key = word_key.Trim();

                                                if (!word_next_word_numbers.ContainsKey(word_key))
                                                {
                                                    words[w].DistanceToNext.dL = -1; // non-applicable
                                                    words[w].DistanceToNext.dW = 0;
                                                    words[w].DistanceToNext.dV = 0;
                                                    words[w].DistanceToNext.dC = 0;

                                                    word_next_word_numbers.Add(word_key, words[w].Number);
                                                    word_next_verse_numbers.Add(word_key, verses[v].Number);
                                                    word_next_chapter_numbers.Add(word_key, chapters[c].SortedNumber);
                                                }
                                                else
                                                {
                                                    words[w].DistanceToNext.dL = -1; // non-applicable
                                                    words[w].DistanceToNext.dW = -1 * (words[w].Number - word_next_word_numbers[word_key]);
                                                    words[w].DistanceToNext.dV = -1 * (verses[v].Number - word_next_verse_numbers[word_key]);
                                                    words[w].DistanceToNext.dC = -1 * (chapters[c].SortedNumber - word_next_chapter_numbers[word_key]);

                                                    // save latest chapter, verse and word numbers for next iteration
                                                    word_next_word_numbers[word_key] = words[w].Number;
                                                    word_next_verse_numbers[word_key] = verses[v].Number;
                                                    word_next_chapter_numbers[word_key] = chapters[c].SortedNumber;
                                                }

                                                if (words[w].Letters != null)
                                                {
                                                    List<Letter> letters = words[w].Letters;
                                                    for (int l = letters.Count - 1; l >= 0; l--)
                                                    {
                                                        if (letters[l] != null)
                                                        {
                                                            string letter_key = letters[l].Text;
                                                            if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                                            {
                                                                letter_key = letter_key.Simplify29();
                                                            }
                                                            letter_key = letter_key.Trim();

                                                            if (!letter_next_letter_numbers.ContainsKey(letter_key))
                                                            {
                                                                letters[l].DistanceToNext.dL = 0;
                                                                letters[l].DistanceToNext.dW = 0;
                                                                letters[l].DistanceToNext.dV = 0;
                                                                letters[l].DistanceToNext.dC = 0;

                                                                letter_next_letter_numbers.Add(letter_key, letters[l].Number);
                                                                letter_next_word_numbers.Add(letter_key, words[w].Number);
                                                                letter_next_verse_numbers.Add(letter_key, verses[v].Number);
                                                                letter_next_chapter_numbers.Add(letter_key, chapters[c].SortedNumber);
                                                            }
                                                            else
                                                            {
                                                                letters[l].DistanceToNext.dL = -1 * (letters[l].Number - letter_next_letter_numbers[letter_key]);
                                                                letters[l].DistanceToNext.dW = -1 * (words[w].Number - letter_next_word_numbers[letter_key]);
                                                                letters[l].DistanceToNext.dV = -1 * (verses[v].Number - letter_next_verse_numbers[letter_key]);
                                                                letters[l].DistanceToNext.dC = -1 * (chapters[c].SortedNumber - letter_next_chapter_numbers[letter_key]);

                                                                // save latest chapter, verse, word and letter numbers for next iteration
                                                                letter_next_letter_numbers[letter_key] = letters[l].Number;
                                                                letter_next_word_numbers[letter_key] = words[w].Number;
                                                                letter_next_verse_numbers[letter_key] = verses[v].Number;
                                                                letter_next_chapter_numbers[letter_key] = chapters[c].SortedNumber;
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
                    }
                }
            }
        }

        private class QuranMetadataChapter
        {
            public int Number;
            public int Verses;
            public int FirstVerse;
            public string Name;
            public string TransliteratedName;
            public string EnglishName;
            public RevelationPlace RevelationPlace;
            public int RevelationOrder;
            public int Bowings;
        }
        private class QuranMetadataStation
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataPart
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataGroup
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataHalf
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataQuarter
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataBowing
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataPage
        {
            public int Number;
            public int StartChapter;
            public int StartChapterVerse;
        }
        private class QuranMetadataProstration
        {
            public int Number;
            public int Chapter;
            public int ChapterVerse;
            public ProstrationType Type;
        }
        private class QuranMetadataInitialization
        {
            public int Number;
            public int Chapter;
            public int ChapterVerse;
            public InitializationType Type;
        }
        private class QuranMetadata
        {
            public List<QuranMetadataChapter> Chapters = new List<QuranMetadataChapter>();
            public List<QuranMetadataStation> Stations = new List<QuranMetadataStation>();
            public List<QuranMetadataPart> Parts = new List<QuranMetadataPart>();
            public List<QuranMetadataGroup> Groups = new List<QuranMetadataGroup>();
            public List<QuranMetadataHalf> Halfs = new List<QuranMetadataHalf>();
            public List<QuranMetadataQuarter> Quarters = new List<QuranMetadataQuarter>();
            public List<QuranMetadataBowing> Bowings = new List<QuranMetadataBowing>();
            public List<QuranMetadataPage> Pages = new List<QuranMetadataPage>();
            public List<QuranMetadataProstration> Prostrations = new List<QuranMetadataProstration>();
            public List<QuranMetadataInitialization> Initializations = new List<QuranMetadataInitialization>();
        }
        private static QuranMetadata s_quran_metadata = null;
        private static string END_OF_SECTION = "";
        private static void LoadQuranMetadata()
        {
            if (Directory.Exists(Globals.DATA_FOLDER))
            {
                string path = Globals.DATA_FOLDER + Path.DirectorySeparatorChar + "quran-metadata.txt";
                if (File.Exists(path))
                {
                    try
                    {
                        s_quran_metadata = new QuranMetadata();

                        using (StreamReader reader = File.OpenText(path))
                        {
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                switch (line)
                                {
                                    case "chapter\tverses\tfirst_verse\tname\ttransliterated_name\tenglish_name\trevelation_place\trevelation_order\tbowings":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 9) throw new Exception("Invalid Chapter metadata in " + path);

                                                QuranMetadataChapter info = new QuranMetadataChapter();
                                                info.Number = int.Parse(parts[0]);
                                                info.Verses = int.Parse(parts[1]);
                                                info.FirstVerse = int.Parse(parts[2]);
                                                info.Name = parts[3];
                                                info.TransliteratedName = parts[4];
                                                info.EnglishName = parts[5];
                                                info.RevelationPlace = (RevelationPlace)Enum.Parse(typeof(RevelationPlace), parts[6]);
                                                info.RevelationOrder = int.Parse(parts[7]);
                                                info.Bowings = int.Parse(parts[8]);
                                                s_quran_metadata.Chapters.Add(info);
                                            }
                                        }
                                        break;
                                    case "station\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Station metadata in " + path);

                                                QuranMetadataStation info = new QuranMetadataStation();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Stations.Add(info);
                                            }
                                        }
                                        break;
                                    case "part\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Part metadata in " + path);

                                                QuranMetadataPart info = new QuranMetadataPart();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Parts.Add(info);
                                            }
                                        }
                                        break;
                                    case "group\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Group metadata in " + path);

                                                QuranMetadataGroup info = new QuranMetadataGroup();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Groups.Add(info);
                                            }
                                        }
                                        break;
                                    case "half\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Half metadata in " + path);

                                                QuranMetadataHalf info = new QuranMetadataHalf();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Halfs.Add(info);
                                            }
                                        }
                                        break;
                                    case "quarter\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Quarter metadata in " + path);

                                                QuranMetadataQuarter info = new QuranMetadataQuarter();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Quarters.Add(info);
                                            }
                                        }
                                        break;
                                    case "bowing\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Bowing metadata in " + path);

                                                QuranMetadataBowing info = new QuranMetadataBowing();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Bowings.Add(info);
                                            }
                                        }
                                        break;
                                    case "page\tstart_chapter\tstart_chapter_verse":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 3) throw new Exception("Invalid Page metadata in " + path);

                                                QuranMetadataPage info = new QuranMetadataPage();
                                                info.Number = int.Parse(parts[0]);
                                                info.StartChapter = int.Parse(parts[1]);
                                                info.StartChapterVerse = int.Parse(parts[2]);
                                                s_quran_metadata.Pages.Add(info);
                                            }
                                        }
                                        break;
                                    case "prostration\tchapter\tchapter_verse\ttype":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 4) throw new Exception("Invalid Prostration metadata in " + path);

                                                QuranMetadataProstration info = new QuranMetadataProstration();
                                                info.Number = int.Parse(parts[0]);
                                                info.Chapter = int.Parse(parts[1]);
                                                info.ChapterVerse = int.Parse(parts[2]);
                                                info.Type = (ProstrationType)Enum.Parse(typeof(ProstrationType), parts[3]);
                                                s_quran_metadata.Prostrations.Add(info);
                                            }
                                        }
                                        break;
                                    case "initialization\tchapter\tchapter_verse\ttype":
                                        {
                                            while ((line = reader.ReadLine()) != null)
                                            {
                                                if (line == END_OF_SECTION) break;

                                                string[] parts = line.Split('\t');
                                                if (parts.Length != 4) throw new Exception("Invalid Initialization metadata in " + path);

                                                QuranMetadataInitialization info = new QuranMetadataInitialization();
                                                info.Number = int.Parse(parts[0]);
                                                info.Chapter = int.Parse(parts[1]);
                                                info.ChapterVerse = int.Parse(parts[2]);
                                                info.Type = (InitializationType)Enum.Parse(typeof(InitializationType), parts[3]);
                                                s_quran_metadata.Initializations.Add(info);
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            continue;
                                        }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("LoadQuranMetadata: " + ex.Message + ".\r\n\r\n" + ex.StackTrace);
                    }
                }
            }
        }

        public List<Word> GetWords(List<Letter> letters)
        {
            if (letters == null) return null;

            List<Word> result = new List<Word>();
            Word word = null;
            foreach (Letter letter in letters)
            {
                if (letter != null)
                {
                    if (word != letter.Word)
                    {
                        word = letter.Word;
                        if (!result.Contains(word))
                        {
                            result.Add(word);
                        }
                    }
                }
            }
            return result;
        }
        public List<Verse> GetVerses(List<Word> words)
        {
            if (words == null) return null;

            List<Verse> result = new List<Verse>();
            Verse verse = null;
            foreach (Word word in words)
            {
                if (word != null)
                {
                    if (verse != word.Verse)
                    {
                        verse = word.Verse;
                        if (!result.Contains(verse))
                        {
                            result.Add(verse);
                        }
                    }
                }
            }
            return result;
        }
        public List<Chapter> GetChapters(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Chapter> result = new List<Chapter>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Chapter))
                    {
                        result.Add(verse.Chapter);
                    }
                }
            }
            return result;
        }
        public List<Page> GetPages(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Page> result = new List<Page>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Page))
                    {
                        result.Add(verse.Page);
                    }
                }
            }
            return result;
        }
        public List<Station> GetStations(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Station> result = new List<Station>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Station))
                    {
                        result.Add(verse.Station);
                    }
                }
            }
            return result;
        }
        public List<Part> GetParts(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Part> result = new List<Part>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Part))
                    {
                        result.Add(verse.Part);
                    }
                }
            }
            return result;
        }
        public List<Group> GetGroups(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Group> result = new List<Group>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Group))
                    {
                        result.Add(verse.Group);
                    }
                }
            }
            return result;
        }
        public List<Half> GetHalfs(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Half> result = new List<Half>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Half))
                    {
                        result.Add(verse.Half);
                    }
                }
            }
            return result;
        }
        public List<Quarter> GetQuarters(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Quarter> result = new List<Quarter>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Quarter))
                    {
                        result.Add(verse.Quarter);
                    }
                }
            }
            return result;
        }
        public List<Bowing> GetBowings(List<Verse> verses)
        {
            if (verses == null) return null;

            List<Bowing> result = new List<Bowing>();
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    if (!result.Contains(verse.Bowing))
                    {
                        result.Add(verse.Bowing);
                    }
                }
            }
            return result;
        }

        public List<Word> GetCompleteWords(List<Letter> letters)
        {
            if (letters == null) return null;
            if (letters.Count == 0) return null;

            List<Word> result = new List<Word>();

            for (int i = 0; i < letters.Count; i++)
            {
                bool complete = true;
                Word word = letters[i].Word;
                foreach (Letter letter in word.Letters)
                {
                    if (!letters.Contains(letter))
                    {
                        complete = false;
                        break;
                    }
                }
                if (complete)
                {
                    if (!result.Contains(word))
                    {
                        result.Add(word);
                    }
                    i += word.Letters.Count;
                }
            }

            return result;
        }
        public List<Word> GetCompleteWords(Sentence sentence)
        {
            if (sentence == null) return null;
            if (String.IsNullOrEmpty(sentence.Text)) return null;

            List<Word> result = new List<Word>();

            if (sentence.FirstVerse.Number == sentence.LastVerse.Number)
            {
                foreach (Word word in sentence.FirstVerse.Words)
                {
                    if (word != null)
                    {
                        if ((word.Position >= sentence.StartPosition) && (word.Position < sentence.EndPosition))
                        {
                            result.Add(word);
                        }
                    }
                }
            }
            else // multi-verse
            {
                // first verse
                foreach (Word word in sentence.FirstVerse.Words)
                {
                    if (word != null)
                    {
                        if (word.Position >= sentence.StartPosition)
                        {
                            result.Add(word);
                        }
                    }
                }

                // middle verses
                int after_first_index = (sentence.FirstVerse.Number + 1) - 1;
                int before_last_index = (sentence.LastVerse.Number - 1) - 1;
                if (after_first_index <= before_last_index)
                {
                    for (int i = after_first_index; i <= before_last_index; i++)
                    {
                        result.AddRange(sentence.FirstVerse.Book.Verses[i].Words);
                    }
                }

                // last verse
                foreach (Word word in sentence.LastVerse.Words)
                {
                    if (word != null)
                    {
                        if (word.Position < sentence.EndPosition) // not <= because EndPosition is after the start of the last word in the sentence
                        {
                            result.Add(word);
                        }
                    }
                }
            }

            return result;
        }
        public List<Verse> GetCompleteVerses(List<Letter> letters)
        {
            if (letters == null) return null;
            if (letters.Count == 0) return null;

            List<Word> words = GetCompleteWords(letters);
            if (words != null)
            {
                return GetCompleteVerses(words);
            }

            return null;
        }
        public List<Verse> GetCompleteVerses(List<Word> words)
        {
            if (words == null) return null;
            if (words.Count == 0) return null;

            List<Verse> result = new List<Verse>();

            for (int i = 0; i < words.Count; i++)
            {
                bool complete = true;
                Verse verse = words[i].Verse;
                foreach (Word word in verse.Words)
                {
                    if (word != null)
                    {
                        if (!words.Contains(word))
                        {
                            complete = false;
                            break;
                        }
                    }
                }
                if (complete)
                {
                    if (!result.Contains(verse))
                    {
                        result.Add(verse);
                    }
                    i += verse.Words.Count;
                }
            }

            return result;
        }
        public List<Verse> GetCompleteVerses(Sentence sentence)
        {
            if (sentence == null) return null;
            if (String.IsNullOrEmpty(sentence.Text)) return null;

            List<Verse> result = new List<Verse>();

            if (sentence.FirstVerse.Number == sentence.LastVerse.Number)
            {
                if ((sentence.StartPosition == 0) && (sentence.EndPosition == sentence.Text.Length - 1))
                {
                    result.Add(sentence.FirstVerse);
                }
            }
            else // multi-verse
            {
                // first verse
                if (sentence.StartPosition == 0)
                {
                    result.Add(sentence.FirstVerse);
                }

                // middle verses
                int after_first_index = (sentence.FirstVerse.Number + 1) - 1;
                int before_last_index = (sentence.LastVerse.Number - 1) - 1;
                if (after_first_index <= before_last_index)
                {
                    for (int i = after_first_index; i <= before_last_index; i++)
                    {
                        result.Add(sentence.FirstVerse.Book.Verses[i]);
                    }
                }

                // last verse
                if (sentence.EndPosition == sentence.LastVerse.Text.Length - 1)
                {
                    result.Add(sentence.LastVerse);
                }
            }

            return result;
        }
        public List<Chapter> GetCompleteChapters(List<Letter> letters)
        {
            if (letters == null) return null;
            if (letters.Count == 0) return null;

            List<Chapter> result = new List<Chapter>();

            List<Word> words = GetCompleteWords(letters);
            if (words != null)
            {
                List<Verse> verses = GetCompleteVerses(words);
                if (verses != null)
                {
                    return GetCompleteChapters(verses);
                }
            }

            return result;
        }
        public List<Chapter> GetCompleteChapters(List<Word> words)
        {
            if (words == null) return null;
            if (words.Count == 0) return null;

            List<Chapter> result = new List<Chapter>();

            List<Verse> verses = GetCompleteVerses(words);
            if (verses != null)
            {
                return GetCompleteChapters(verses);
            }

            return result;
        }
        public List<Chapter> GetCompleteChapters(List<Verse> verses)
        {
            if (verses == null) return null;
            if (verses.Count == 0) return null;

            List<Chapter> result = new List<Chapter>();

            for (int i = 0; i < verses.Count; i++)
            {
                if (verses[i] != null)
                {
                    bool complete = true;
                    Chapter chapter = verses[i].Chapter;
                    if (chapter != null)
                    {
                        foreach (Verse verse in chapter.Verses)
                        {
                            if (verse != null)
                            {
                                if (!verses.Contains(verse))
                                {
                                    complete = false;
                                    break;
                                }
                            }
                        }
                        if (complete)
                        {
                            if (!result.Contains(chapter))
                            {
                                result.Add(chapter);
                            }
                            i += chapter.Verses.Count;
                        }
                    }
                }
            }

            return result;
        }
        public List<Chapter> GetCompleteChapters(List<Verse> verses, Letter start_letter, Letter end_letter)
        {
            if (verses == null) return null;
            if (verses.Count == 0) return null;

            List<Chapter> result = new List<Chapter>();

            List<Verse> copy_verses = new List<Verse>(verses); // make a copy so we don't change the passed verses
            if (copy_verses != null)
            {
                if (copy_verses.Count > 0)
                {
                    Verse first_verse = copy_verses[0];
                    if (first_verse != null)
                    {
                        if (start_letter.NumberInVerse > 1)
                        {
                            copy_verses.Remove(first_verse);
                        }
                    }

                    if (copy_verses.Count > 0) // check again after removing a verse
                    {
                        Verse last_verse = copy_verses[copy_verses.Count - 1];
                        if (last_verse != null)
                        {
                            if (end_letter.NumberInVerse < last_verse.Letters.Count)
                            {
                                copy_verses.Remove(last_verse);
                            }
                        }
                    }

                    if (copy_verses.Count > 0) // check again after removing a verse
                    {
                        foreach (Chapter chapter in this.Chapters)
                        {
                            if (chapter != null)
                            {
                                bool include_chapter = true;
                                foreach (Verse v in chapter.Verses)
                                {
                                    if (v != null)
                                    {
                                        if (!copy_verses.Contains(v))
                                        {
                                            include_chapter = false;
                                            break;
                                        }
                                    }
                                }

                                if (include_chapter)
                                {
                                    result.Add(chapter);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public void SortChapters(ChapterCompareBy compare_by, ChapterCompareOrder compare_order, bool pin_chapter1, bool distances_within_chapters, bool with_diacritics)
        {
            if (this.chapters != null)
            {
                Chapter.CompareBy = compare_by;
                Chapter.CompareOrder = compare_order;
                Chapter.PinChapter1 = pin_chapter1;

                // sort chapters using Chapter.CompareTo()
                this.chapters.Sort();

                // update chapter numbers in new sort order
                for (int i = 0; i < this.chapters.Count; i++)
                {
                    if (chapters[i] != null)
                    {
                        this.chapters[i].SortedNumber = i + 1;
                    }
                }

                // update verse order in new sort order
                List<Verse> verses = new List<Verse>();
                foreach (Chapter chapter in this.chapters)
                {
                    if (chapter != null)
                    {
                        verses.AddRange(chapter.Verses);
                    }
                }
                this.verses = verses;

                SetupNumbers();
                SetupFrequenciesAndOccurrences(with_diacritics);
                SetupDistances(distances_within_chapters, with_diacritics);
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

        private int min_words;
        public int MinWords
        {
            get { return min_words; }
        }
        private int max_words;
        public int MaxWords
        {
            get { return max_words; }
        }
        private int min_letters;
        public int MinLetters
        {
            get { return min_letters; }
        }
        private int max_letters;
        public int MaxLetters
        {
            get { return max_letters; }
        }

        // root words
        private SortedDictionary<string, List<Word>> root_words = null;
        public SortedDictionary<string, List<Word>> RootWords
        {
            get
            {
                if (root_words == null)
                {
                    PopulateRootWords();
                }
                return root_words;
            }
            set { root_words = value; }
        }
        private void PopulateRootWords()
        {
            root_words = new SortedDictionary<string, List<Word>>();
            if (root_words != null)
            {
                List<string> roots = new List<string>();
                foreach (Verse verse in this.Verses)
                {
                    if (verse != null)
                    {
                        if (verse.Words != null)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    if (word.Roots != null)
                                    {
                                        foreach (string root in word.Roots)
                                        {
                                            if (!root_words.ContainsKey(root))
                                            {
                                                root_words.Add(root, new List<Word>());
                                            }
                                            root_words[root].Add(word);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // root word-frequencies
        private SortedDictionary<string, Dictionary<Word, int>> root_word_frequencies = null;
        private SortedDictionary<string, Dictionary<Word, int>> RootWordFrequencies
        {
            get
            {
                if (root_word_frequencies == null)
                {
                    PopulateRootWordFrequencies();
                }
                return root_word_frequencies;
            }
            set { root_word_frequencies = value; }
        }
        private void PopulateRootWordFrequencies()
        {
            root_word_frequencies = new SortedDictionary<string, Dictionary<Word, int>>();
            if (root_word_frequencies != null)
            {
                List<string> roots = new List<string>();
                foreach (Verse verse in this.Verses)
                {
                    if (verse != null)
                    {
                        if (verse.Words != null)
                        {
                            foreach (Word word in verse.Words)
                            {
                                if (word != null)
                                {
                                    if (word.Roots != null)
                                    {
                                        foreach (string root in word.Roots)
                                        {
                                            if (!root_word_frequencies.ContainsKey(root))
                                            {
                                                Dictionary<Word, int> words_dictionary = new Dictionary<Word, int>();
                                                words_dictionary.Add(word, 1);
                                                root_word_frequencies.Add(root, words_dictionary);
                                            }
                                            else // root already exist
                                            {
                                                if (!root_word_frequencies[root].ContainsKey(word))
                                                {
                                                    root_word_frequencies[root].Add(word, 1);
                                                }
                                                else // word already exist
                                                {
                                                    root_word_frequencies[root][word]++;
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
        }

        // translation infos
        private Dictionary<string, TranslationInfo> translation_infos = null;
        public Dictionary<string, TranslationInfo> TranslationInfos
        {
            get { return translation_infos; }
            set { translation_infos = value; }
        }

        // recitation infos
        private Dictionary<string, RecitationInfo> recitation_infos = null;
        public Dictionary<string, RecitationInfo> RecitationInfos
        {
            get { return recitation_infos; }
            set { recitation_infos = value; }
        }

        // get verse range
        public List<Verse> GetVerses(int start, int end)
        {
            List<Verse> result = new List<Verse>();
            if (
                (start >= end)
                &&
                (start >= 1 && start <= this.verses.Count)
                &&
                (end >= start && end <= this.verses.Count)
                )
            {
                if (this.verses != null)
                {
                    foreach (Verse verse in this.verses)
                    {
                        if (verse != null)
                        {
                            if ((verse.Number >= start) && (verse.Number <= end))
                            {
                                result.Add(verse);
                            }
                        }
                    }
                }
            }
            return result;
        }
        // get words
        public Word GetLastWord(string text, Word first_word)
        {
            if (first_word == null) return null;

            if (!String.IsNullOrEmpty(text))
            {
                text = text.Trim();
                text = text.Replace("\r", "");
                text = text.Replace("\n", " ");
                text = text.Replace("\t", "");
                text = text.Replace("_", "");
                //text = text.Replace(" ", "");
                text = text.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
                text = text.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
                foreach (char character in Constants.INDIAN_DIGITS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.ARABIC_DIGITS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.SYMBOLS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.STOPMARKS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.QURANMARKS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.DIACRITICS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                if (!String.IsNullOrEmpty(text))
                {
                    int count = 0;
                    int pos = text.IndexOf(" ");
                    while (pos != -1)
                    {
                        count++;

                        text = text.Remove(pos, 1);
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                        pos = text.IndexOf(" ");
                    }

                    int last_word_index = (first_word.Number - 1) + count;
                    if ((last_word_index > 0) && (last_word_index < Words.Count))
                    {
                        return Words[last_word_index];
                    }
                }
            }

            return first_word;
        }
        public Dictionary<string, int> GetWordsWith(List<Verse> verses, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool with_diacritics)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (verses != null)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (!text.Contains(" "))
                    {
                        text = text.Simplify(this.text_mode).Trim();

                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                string verse_text = verse.Text;
                                if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                {
                                    verse_text = verse_text.Simplify29();
                                }
                                verse_text = verse_text.Trim();

                                verse_text = RemoveQuranmarksAndStopmarks(text, verse_text);

                                while (verse_text.Contains("  "))
                                {
                                    verse_text = verse_text.Replace("  ", " ");
                                }

                                string[] verse_words = verse_text.Split();
                                for (int i = 0; i < verse_words.Length; i++)
                                {
                                    bool break_loop = false;
                                    switch (text_location_in_verse)
                                    {
                                        case TextLocationInVerse.Any:
                                            {
                                                // do nothing
                                            }
                                            break;
                                        case TextLocationInVerse.AtStart:
                                            {
                                                if (i > 0) break_loop = true;
                                            }
                                            break;
                                        case TextLocationInVerse.AtMiddle:
                                            {
                                                if (i == 0) continue;
                                                if (i == verse_words.Length - 1) continue;
                                            }
                                            break;
                                        case TextLocationInVerse.AtEnd:
                                            {
                                                if (i < verse_words.Length - 1) continue;
                                            }
                                            break;
                                    }
                                    if (break_loop) break;

                                    switch (text_wordness)
                                    {
                                        case TextWordness.WholeWord:
                                            {
                                                if (verse_words[i] == text)
                                                {
                                                    if (!result.ContainsKey(verse_words[i]))
                                                    {
                                                        result.Add(verse_words[i], 1);
                                                    }
                                                    else
                                                    {
                                                        result[verse_words[i]]++;
                                                    }
                                                }
                                            }
                                            break;
                                        case TextWordness.PartOfWord:
                                            {
                                                if ((verse_words[i] != text) && (verse_words[i].Contains(text)))
                                                {
                                                    if (!result.ContainsKey(verse_words[i]))
                                                    {
                                                        result.Add(verse_words[i], 1);
                                                    }
                                                    else
                                                    {
                                                        result[verse_words[i]]++;
                                                    }
                                                }
                                            }
                                            break;
                                        case TextWordness.Any:
                                            {
                                                switch (text_location_in_word)
                                                {
                                                    case TextLocationInWord.AtStart:
                                                        {
                                                            if (verse_words[i].StartsWith(text))
                                                            {
                                                                if (!result.ContainsKey(verse_words[i]))
                                                                {
                                                                    result.Add(verse_words[i], 1);
                                                                }
                                                                else
                                                                {
                                                                    result[verse_words[i]]++;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case TextLocationInWord.AtMiddle:
                                                        {
                                                            if (verse_words[i].ContainsInside(text))
                                                            {
                                                                if (!result.ContainsKey(verse_words[i]))
                                                                {
                                                                    result.Add(verse_words[i], 1);
                                                                }
                                                                else
                                                                {
                                                                    result[verse_words[i]]++;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case TextLocationInWord.AtEnd:
                                                        {
                                                            if (verse_words[i].EndsWith(text))
                                                            {
                                                                if (!result.ContainsKey(verse_words[i]))
                                                                {
                                                                    result.Add(verse_words[i], 1);
                                                                }
                                                                else
                                                                {
                                                                    result[verse_words[i]]++;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case TextLocationInWord.Any:
                                                        {
                                                            if (verse_words[i].Contains(text))
                                                            {
                                                                if (!result.ContainsKey(verse_words[i]))
                                                                {
                                                                    result.Add(verse_words[i], 1);
                                                                }
                                                                else
                                                                {
                                                                    result[verse_words[i]]++;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        public Dictionary<string, int> GetCurrentWords(List<Verse> verses, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool with_diacritics)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (verses != null)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    while (text.Contains("+"))
                    {
                        text = text.Replace("+", "");
                    }
                    while (text.Contains("-"))
                    {
                        text = text.Replace("-", "");
                    }
                    text = text.Simplify(this.text_mode).Trim();

                    string[] text_words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (text_words.Length > 0)
                    {
                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                string verse_text = verse.Text;
                                if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                {
                                    verse_text = verse_text.Simplify29();
                                }
                                verse_text = verse_text.Trim();

                                verse_text = RemoveQuranmarksAndStopmarks(text, verse_text);

                                while (verse_text.Contains("  "))
                                {
                                    verse_text = verse_text.Replace("  ", " ");
                                }

                                string[] verse_words = verse_text.Split();
                                if (verse_words.Length >= text_words.Length)
                                {
                                    for (int i = 0; i < verse_words.Length; i++)
                                    {
                                        bool break_loop = false;
                                        switch (text_location_in_verse)
                                        {
                                            case TextLocationInVerse.Any:
                                                {
                                                    // do nothing
                                                }
                                                break;
                                            case TextLocationInVerse.AtStart:
                                                {
                                                    if (i > 0) break_loop = true;
                                                }
                                                break;
                                            case TextLocationInVerse.AtMiddle:
                                                {
                                                    if (i == 0) continue;
                                                    if (i == verse_words.Length - 1) continue;
                                                }
                                                break;
                                            case TextLocationInVerse.AtEnd:
                                                {
                                                    if (i < verse_words.Length - 1) continue;
                                                }
                                                break;
                                        }
                                        if (break_loop) break;

                                        int match_count = 0;
                                        if (text_words.Length == 1) // 1 word search term
                                        {
                                            switch (text_wordness)
                                            {
                                                case TextWordness.WholeWord:
                                                    {
                                                        if (verse_words[i] == text_words[0])
                                                        {
                                                            match_count = 1;
                                                        }
                                                    }
                                                    break;
                                                case TextWordness.PartOfWord:
                                                    {
                                                        if ((verse_words[i] != text_words[0]) && (verse_words[i].Contains(text_words[0])))
                                                        {
                                                            switch (text_location_in_word)
                                                            {
                                                                case TextLocationInWord.AtStart:
                                                                    {
                                                                        if (verse_words[i].StartsWith(text_words[0]))
                                                                        {
                                                                            match_count = 1;
                                                                        }
                                                                    }
                                                                    break;
                                                                case TextLocationInWord.AtMiddle:
                                                                    {
                                                                        if (verse_words[i].ContainsInside(text_words[0]))
                                                                        {
                                                                            MatchCollection matches = Regex.Matches(verse_words[i], text_words[0]);
                                                                            match_count = matches.Count;

                                                                            if (match_count > 0)
                                                                            {
                                                                                if (verse_words[i].StartsWith(text_words[0]))
                                                                                {
                                                                                    match_count--;
                                                                                }

                                                                                if (verse_words[i].EndsWith(text_words[0]))
                                                                                {
                                                                                    match_count--;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                                case TextLocationInWord.AtEnd:
                                                                    {
                                                                        if (verse_words[i].EndsWith(text_words[0]))
                                                                        {
                                                                            match_count = 1;
                                                                        }
                                                                    }
                                                                    break;
                                                                case TextLocationInWord.Any:
                                                                    {
                                                                        if (verse_words[i].Contains(text_words[0]))
                                                                        {
                                                                            MatchCollection matches = Regex.Matches(verse_words[i], text_words[0]);
                                                                            match_count = matches.Count;
                                                                        }
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case TextWordness.Any:
                                                    {
                                                        switch (text_location_in_word)
                                                        {
                                                            case TextLocationInWord.AtStart:
                                                                {
                                                                    if (verse_words[i].StartsWith(text_words[0]))
                                                                    {
                                                                        match_count = 1;
                                                                    }
                                                                }
                                                                break;
                                                            case TextLocationInWord.AtMiddle:
                                                                {
                                                                    if (verse_words[i].ContainsInside(text_words[0]))
                                                                    {
                                                                        MatchCollection matches = Regex.Matches(verse_words[i], text_words[0]);
                                                                        match_count = matches.Count;

                                                                        if (match_count > 0)
                                                                        {
                                                                            if (verse_words[i].StartsWith(text_words[0]))
                                                                            {
                                                                                match_count--;
                                                                            }

                                                                            if (verse_words[i].EndsWith(text_words[0]))
                                                                            {
                                                                                match_count--;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                            case TextLocationInWord.AtEnd:
                                                                {
                                                                    if (verse_words[i].EndsWith(text_words[0]))
                                                                    {
                                                                        match_count = 1;
                                                                    }
                                                                }
                                                                break;
                                                            case TextLocationInWord.Any:
                                                                {
                                                                    if (verse_words[i].Contains(text_words[0]))
                                                                    {
                                                                        MatchCollection matches = Regex.Matches(verse_words[i], text_words[0]);
                                                                        match_count = matches.Count;
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        else if (text_words.Length > 1) // multiple words search term
                                        {
                                            switch (text_location_in_word)
                                            {
                                                case TextLocationInWord.AtStart:
                                                    {
                                                        if (verse_words[i].StartsWith(text_words[0]))
                                                        {
                                                            if (verse_text.Contains(text))
                                                            {
                                                                match_count = 1;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case TextLocationInWord.AtMiddle:
                                                    {
                                                        if (verse_words[i].ContainsInside(text_words[0]))
                                                        {
                                                            if (verse_text.Contains(text))
                                                            {
                                                                MatchCollection matches = Regex.Matches(verse_words[i], text_words[0]);
                                                                match_count = matches.Count;

                                                                if (match_count > 0)
                                                                {
                                                                    if (verse_words[i].StartsWith(text_words[0]))
                                                                    {
                                                                        match_count--;
                                                                    }

                                                                    if (verse_words[i].EndsWith(text_words[0]))
                                                                    {
                                                                        match_count--;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case TextLocationInWord.AtEnd:
                                                    {
                                                        if (verse_words[i].EndsWith(text_words[0]))
                                                        {
                                                            if (verse_text.Contains(text))
                                                            {
                                                                match_count = 1;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case TextLocationInWord.Any:
                                                    {
                                                        if (verse_words[i].EndsWith(text_words[0]))
                                                        {
                                                            if (verse_words.Length >= (i + text_words.Length))
                                                            {
                                                                // match text minus last word
                                                                bool match_found_minus_last_word = true;
                                                                for (int j = 1; j < text_words.Length - 1; j++)
                                                                {
                                                                    if (verse_words[j + i] != text_words[j])
                                                                    {
                                                                        match_found_minus_last_word = false;
                                                                        break;
                                                                    }
                                                                }

                                                                // is still true, check the last word
                                                                if (match_found_minus_last_word)
                                                                {
                                                                    int last_j = text_words.Length - 1;
                                                                    if (verse_words[last_j + i].StartsWith(text_words[last_j])) // last text_word
                                                                    {
                                                                        match_count = 1;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }

                                        if (match_count > 0)
                                        {
                                            // skip all text but not found good_word in case it followed by good_word too
                                            i += text_words.Length - 1;

                                            // get last word variation
                                            if (i < verse_words.Length)
                                            {
                                                string matching_word = verse_words[i];
                                                if (!result.ContainsKey(matching_word))
                                                {
                                                    result.Add(matching_word, match_count);
                                                }
                                                else
                                                {
                                                    result[matching_word] += match_count;
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
        public Dictionary<string, int> GetNextWords(List<Verse> verses, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool with_diacritics)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (verses != null)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    while (text.Contains("  "))
                    {
                        text = text.Replace("  ", " ");
                    }
                    while (text.Contains("+"))
                    {
                        text = text.Replace("+", "");
                    }
                    while (text.Contains("-"))
                    {
                        text = text.Replace("-", "");
                    }
                    text = text.Simplify(this.text_mode).Trim();

                    string[] text_words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (text_words.Length > 0)
                    {
                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                string verse_text = verse.Text;
                                if (((this.text_mode == "Original") || (this.text_mode == "SimplifiedMarks")) && (!with_diacritics))
                                {
                                    verse_text = verse_text.Simplify29();
                                }
                                verse_text = verse_text.Trim();

                                verse_text = RemoveQuranmarksAndStopmarks(text, verse_text);

                                while (verse_text.Contains("  "))
                                {
                                    verse_text = verse_text.Replace("  ", " ");
                                }

                                string[] verse_words = verse_text.Split();
                                if (verse_words.Length >= text_words.Length)
                                {
                                    for (int i = 0; i < verse_words.Length; i++)
                                    {
                                        bool break_loop = false;
                                        switch (text_location_in_verse)
                                        {
                                            case TextLocationInVerse.Any:
                                                {
                                                    // do nothing
                                                }
                                                break;
                                            case TextLocationInVerse.AtStart:
                                                {
                                                    if (i > 0) break_loop = true;
                                                }
                                                break;
                                            case TextLocationInVerse.AtMiddle:
                                                {
                                                    if (i == 0) continue;
                                                    if (i == verse_words.Length - 1) continue;
                                                }
                                                break;
                                            case TextLocationInVerse.AtEnd:
                                                {
                                                    if (i < verse_words.Length - 1) continue;
                                                }
                                                break;
                                        }
                                        if (break_loop) break;

                                        //bool start_found = false;
                                        //switch (text_location_in_word)
                                        //{
                                        //    case TextLocationInWord.AtStart:
                                        //        {
                                        //            start_found = verse_words[i].Equals(text_words[0]);
                                        //        }
                                        //        break;
                                        //    case TextLocationInWord.AtMiddle:
                                        //        {
                                        //            start_found = verse_words[i].EndsWith(text_words[0]);
                                        //        }
                                        //        break;
                                        //    case TextLocationInWord.AtEnd:
                                        //        {
                                        //            start_found = verse_words[i].EndsWith(text_words[0]);
                                        //        }
                                        //        break;
                                        //    case TextLocationInWord.Any:
                                        //        {
                                        //            start_found = verse_words[i].EndsWith(text_words[0]);
                                        //        }
                                        //        break;
                                        //}

                                        bool found = false;
                                        switch (text_wordness)
                                        {
                                            case TextWordness.WholeWord:
                                                {
                                                    if (verse_words[i] == text_words[0])
                                                    {
                                                        found = true;
                                                    }
                                                }
                                                break;
                                            case TextWordness.PartOfWord:
                                                {
                                                    if ((verse_words[i] != text_words[0]) && (verse_words[i].EndsWith(text_words[0])))
                                                    {
                                                        found = true;
                                                    }
                                                }
                                                break;
                                            case TextWordness.Any:
                                                {
                                                    if (verse_words[i].EndsWith(text_words[0]))
                                                    {
                                                        found = true;
                                                    }
                                                    break;
                                                }
                                        }

                                        if (found)
                                        {
                                            if (verse_words.Length >= (i + text_words.Length))
                                            {
                                                // check rest of text_words if matching
                                                bool match_found = true;
                                                for (int j = 1; j < text_words.Length; j++)
                                                {
                                                    if (!verse_words[j + i].StartsWith(text_words[j]))
                                                    {
                                                        match_found = false;
                                                        break;
                                                    }
                                                }

                                                if (match_found)
                                                {
                                                    i += text_words.Length;

                                                    // add next word to result (if not added already)
                                                    if (i < verse_words.Length)
                                                    {
                                                        string matching_word = verse_words[i];
                                                        if (!result.ContainsKey(matching_word))
                                                        {
                                                            result.Add(matching_word, 1);
                                                        }
                                                        else
                                                        {
                                                            result[matching_word]++;
                                                        }
                                                    }

                                                    i--; // check following word in case it contains a match too
                                                         // Example: search for " ير" to not miss يطير in طاير يطير
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
        public string RemoveQuranmarksAndStopmarks(string search_term, string body)
        {
            // if search term includes quranmarks or stopmarks, do nothing
            foreach (char c in search_term)
            {
                if (Constants.QURANMARKS.Contains(c)) return body;
                if (Constants.STOPMARKS.Contains(c)) return body;
            }

            // if body starts with a quranmark, remove it
            if (Constants.QURANMARKS.Contains(body[0]))
            {
                body = body.Substring(2, body.Length - 2);
            }

            // if body ends with a quranmark, remove it
            if (Constants.QURANMARKS.Contains(body[body.Length - 1]))
            {
                body = body.Substring(0, body.Length - 2);
            }

            // if body ends with a stopmark, remove it
            if (Constants.STOPMARKS.Contains(body[body.Length - 1]))
            {
                body = body.Substring(0, body.Length - 2);
            }

            return body;
        }
        // get roots
        public List<string> GetRoots()
        {
            List<string> result = new List<string>();
            foreach (string key in this.RootWords.Keys)
            {
                result.Add(key);
            }
            return result;
        }
        public Dictionary<string, int> GetRoots(string text)
        {
            return GetRoots(this.Verses, text, TextLocationInWord.Any);
        }
        public Dictionary<string, int> GetRoots(List<Verse> verses, string text)
        {
            return GetRoots(verses, text, TextLocationInWord.Any);
        }
        public Dictionary<string, int> GetRoots(List<Verse> verses, string text, TextLocationInWord text_location_in_word)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (verses != null)
            {
                if (text != null)
                {
                    SortedDictionary<string, List<Word>> root_word_frequencies = this.RootWords;
                    if (root_word_frequencies != null)
                    {
                        if (root_word_frequencies.Keys != null)
                        {
                            foreach (string root in root_word_frequencies.Keys)
                            {
                                if (root_word_frequencies.ContainsKey(root))
                                {
                                    int count = 0;
                                    List<Word> root_words = root_word_frequencies[root];
                                    if (verses.Count == this.Verses.Count)
                                    {
                                        count = root_words.Count;
                                    }
                                    else
                                    {
                                        foreach (Word root_word in root_words)
                                        {
                                            if (root_word != null)
                                            {
                                                if (verses.Contains(root_word.Verse))
                                                {
                                                    count++;
                                                }
                                            }
                                        }
                                    }

                                    foreach (Word root_word in root_words)
                                    {
                                        if (root_word != null)
                                        {
                                            if (verses.Contains(root_word.Verse))
                                            {
                                                switch (text_location_in_word)
                                                {
                                                    case TextLocationInWord.AtStart:
                                                        {
                                                            if (text.Length == 0)
                                                            {
                                                                result.Add(root, count);
                                                            }
                                                            else
                                                            {
                                                                text = text.Simplify36();
                                                                if (root.StartsWith(text))
                                                                {
                                                                    result.Add(root, count);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case TextLocationInWord.AtMiddle:
                                                        {
                                                            if (text.Length == 0)
                                                            {
                                                                result.Add(root, count);
                                                            }
                                                            else
                                                            {
                                                                text = text.Simplify36();
                                                                if (root.ContainsInside(text))
                                                                {
                                                                    result.Add(root, count);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case TextLocationInWord.AtEnd:
                                                        {
                                                            if (text.Length == 0)
                                                            {
                                                                result.Add(root, count);
                                                            }
                                                            else
                                                            {
                                                                text = text.Simplify36();
                                                                if (root.EndsWith(text))
                                                                {
                                                                    result.Add(root, count);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case TextLocationInWord.Any:
                                                        {
                                                            if (text.Length == 0)
                                                            {
                                                                result.Add(root, count);
                                                            }
                                                            else
                                                            {
                                                                text = text.Simplify36();
                                                                if (root.Contains(text))
                                                                {
                                                                    result.Add(root, count);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                }
                                                break;
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
        public string GetBestRoot(string text)
        {
            foreach (Verse verse in this.Verses)
            {
                foreach (Word word in verse.Words)
                {
                    if (word != null)
                    {
                        if (text == word.Text)
                        {
                            return word.Root;
                        }
                    }
                }
            }
            foreach (Verse verse in this.Verses)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            if (text.Simplify36() == word.Text.Simplify36())
                            {
                                return word.Root;
                            }
                        }
                    }
                }
            }
            foreach (Verse verse in this.Verses)
            {
                foreach (Word word in verse.Words)
                {
                    if (word != null)
                    {
                        if (text.Simplify31() == word.Text.Simplify31())
                        {
                            return word.Root;
                        }
                    }
                }
            }
            foreach (Verse verse in this.Verses)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            if (text.Simplify30() == word.Text.Simplify30())
                            {
                                return word.Root;
                            }
                        }
                    }
                }
            }
            foreach (Verse verse in this.Verses)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            if (text.Simplify29() == word.Text.Simplify29())
                            {
                                return word.Root;
                            }
                        }
                    }
                }
            }
            foreach (Verse verse in this.Verses)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            if (text.Simplify28() == word.Text.Simplify28())
                            {
                                return word.Root;
                            }
                        }
                    }
                }
            }

            // if not found
            return GetApproximateRoot(text);
        }
        private string GetApproximateRoot(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                string simple_word_text = text.Simplify36();

                // special case:
                if (
                     (simple_word_text == "بسم") ||
                     (simple_word_text == "باسماء") ||
                     (simple_word_text == "باسمايهم") ||
                     (simple_word_text == "اسمه") ||
                     (simple_word_text == "مسمي") ||
                     (simple_word_text == "سميتها") ||
                     (simple_word_text == "اسم") ||
                     (simple_word_text == "اسماء") ||
                     (simple_word_text == "سميتموها") ||
                     (simple_word_text == "اسميه") ||
                     (simple_word_text == "سموهم") ||
                     (simple_word_text == "للمتوسمين") ||
                     (simple_word_text == "سميا") ||
                     (simple_word_text == "الاسماء") ||
                     (simple_word_text == "سميكم") ||
                     (simple_word_text == "الاسم") ||
                     (simple_word_text == "ليسمون") ||
                     (simple_word_text == "تسميه") ||
                     (simple_word_text == "باسم") ||
                     (simple_word_text == "سنسمه") ||
                     (simple_word_text == "تسمي")
                   )
                {
                    return "وسم"; // instead of "سمو"
                }

                // try all roots in case word_text is a root
                SortedDictionary<string, List<Word>> root_word_frequencies = this.RootWords;
                if (root_word_frequencies != null)
                {
                    foreach (string root in root_word_frequencies.Keys)
                    {
                        if (root != null)
                        {
                            if (
                                    (root.Length >= 3)
                                    ||
                                    (root.Length == simple_word_text.Length - 1)
                                    ||
                                    (root.Length == simple_word_text.Length)
                                    ||
                                    (root.Length == simple_word_text.Length + 1)
                               )
                            {
                                List<Word> root_words = root_word_frequencies[root];
                                foreach (Word root_word in root_words)
                                {
                                    int verse_index = root_word.Verse.Number - 1;
                                    if ((verse_index >= 0) && (verse_index < this.verses.Count))
                                    {
                                        Verse verse = this.verses[verse_index];
                                        int word_index = root_word.NumberInVerse - 1;
                                        if ((word_index >= 0) && (word_index < verse.Words.Count))
                                        {
                                            Word verse_word = verse.Words[word_index];
                                            if (verse_word.Text.Simplify36() == simple_word_text)
                                            {
                                                return root;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // get most similar root to word_text
                string best_root = null;
                double max_similirity = double.MinValue;
                Dictionary<string, int> roots = GetRoots(text);
                foreach (string root in roots.Keys)
                {
                    double similirity = root.SimilarityTo(simple_word_text);
                    if (similirity > max_similirity)
                    {
                        max_similirity = similirity;
                        best_root = root;
                    }
                }
                return best_root;
            }
            return null;
        }
        // get related words and verses
        public Dictionary<string, int> GetUniqueRelatedWords(Word word)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (word != null)
            {
                SortedDictionary<string, Dictionary<Word, int>> root_word_frequencies = this.RootWordFrequencies;
                if (root_word_frequencies != null)
                {
                    // try all roots in case word_text is a root
                    string simplified_word_text = word.Text.Simplify36();
                    if (root_word_frequencies.ContainsKey(simplified_word_text))
                    {
                        Dictionary<Word, int> root_words = root_word_frequencies[simplified_word_text];
                        foreach (Word root_word in root_words.Keys)
                        {
                            if (root_word != null)
                            {
                                int verse_index = root_word.Verse.Number - 1;
                                if ((verse_index >= 0) && (verse_index < this.verses.Count))
                                {
                                    Verse verse = this.verses[verse_index];
                                    int word_index = root_word.NumberInVerse - 1;
                                    if ((word_index >= 0) && (word_index < verse.Words.Count))
                                    {
                                        Word verse_word = verse.Words[word_index];
                                        if (!result.ContainsKey(verse_word.Text))
                                        {
                                            result.Add(verse_word.Text, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                    {
                        string root = word.Root;
                        if (!String.IsNullOrEmpty(root))
                        {
                            if (root_word_frequencies.ContainsKey(root))
                            {
                                Dictionary<Word, int> words_dictionary = root_word_frequencies[root];
                                foreach (Word w in words_dictionary.Keys)
                                {
                                    if (w != null)
                                    {
                                        int verse_index = w.Verse.Number - 1;
                                        if ((verse_index >= 0) && (verse_index < this.verses.Count))
                                        {
                                            Verse verse = this.verses[verse_index];
                                            int word_index = w.NumberInVerse - 1;
                                            if ((word_index >= 0) && (word_index < verse.Words.Count))
                                            {
                                                Word verse_word = verse.Words[word_index];
                                                if (!result.ContainsKey(verse_word.Text))
                                                {
                                                    result.Add(verse_word.Text, 1);
                                                }
                                                else
                                                {
                                                    result[verse_word.Text]++;
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
        public Dictionary<Word, int> GetRelatedWords(Word word)
        {
            Dictionary<Word, int> result = new Dictionary<Word, int>();
            if (word != null)
            {
                SortedDictionary<string, Dictionary<Word, int>> root_word_frequencies = this.RootWordFrequencies;
                if (root_word_frequencies != null)
                {
                    // try all roots in case word_text is a root
                    string simplified_word_text = word.Text.Simplify36();
                    if (root_word_frequencies.ContainsKey(simplified_word_text))
                    {
                        Dictionary<Word, int> root_words = root_word_frequencies[simplified_word_text];
                        foreach (Word root_word in root_words.Keys)
                        {
                            if (root_word != null)
                            {
                                int frequency = root_words[root_word];
                                result.Add(root_word, frequency);
                            }
                        }
                    }
                    else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                    {
                        string root = word.Root;
                        if (!String.IsNullOrEmpty(root))
                        {
                            if (root_word_frequencies.ContainsKey(root))
                            {
                                Dictionary<Word, int> words_dictionary = root_word_frequencies[root];
                                foreach (Word w in words_dictionary.Keys)
                                {
                                    if (w != null)
                                    {
                                        int frequency = words_dictionary[w];
                                        int verse_index = w.Verse.Number - 1;
                                        if ((verse_index >= 0) && (verse_index < this.verses.Count))
                                        {
                                            Verse verse = this.verses[verse_index];
                                            int word_index = w.NumberInVerse - 1;
                                            if ((word_index >= 0) && (word_index < verse.Words.Count))
                                            {
                                                Word verse_word = verse.Words[word_index];
                                                if (!result.ContainsKey(verse_word))
                                                {
                                                    result.Add(verse_word, frequency);
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
        public List<Verse> GetRelatedVerses(Word word)
        {
            List<Verse> result = new List<Verse>();
            if (word != null)
            {
                SortedDictionary<string, List<Word>> root_word_frequencies = this.RootWords;
                if (root_word_frequencies != null)
                {
                    // try all roots in case word_text is a root
                    if (root_word_frequencies.ContainsKey(word.Text))
                    {
                        List<Word> root_words = root_word_frequencies[word.Text];
                        foreach (Word root_word in root_words)
                        {
                            if (root_word != null)
                            {
                                int verse_index = root_word.Verse.Number - 1;
                                if ((verse_index >= 0) && (verse_index < this.verses.Count))
                                {
                                    Verse verse = this.verses[verse_index];
                                    if (!result.Contains(verse))
                                    {
                                        result.Add(verse);
                                    }
                                }
                            }
                        }
                    }
                    else // if no such root, search for the matching root_word by its verse position and get its root and then get all root_words
                    {
                        string root = word.Root;
                        if (!String.IsNullOrEmpty(root))
                        {
                            if (root_word_frequencies.ContainsKey(root))
                            {
                                List<Word> root_words = root_word_frequencies[root];
                                foreach (Word w in root_words)
                                {
                                    if (w != null)
                                    {
                                        int verse_index = w.Verse.Number - 1;
                                        if ((verse_index >= 0) && (verse_index < this.verses.Count))
                                        {
                                            Verse verse = this.verses[verse_index];
                                            if (!result.Contains(verse))
                                            {
                                                result.Add(verse);
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
    }
}
