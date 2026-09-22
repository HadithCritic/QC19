using System;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// Complete meaningful list of words that may cross verse boundries
    /// <para>(Example Quran 30:2-4)</para>
    /// <para>غُلِبَتِ ٱلرُّومُ</para>
    /// <para>فِىٓ أَدْنَى ٱلْأَرْضِ وَهُم مِّنۢ بَعْدِ غَلَبِهِمْ سَيَغْلِبُونَ</para>
    /// <para>فِى بِضْعِ سِنِينَ</para>
    /// </summary>
    public class Sentence
    {
        private Verse first_verse = null;
        /// <summary>
        /// First verse in sentence
        /// </summary>
        public Verse FirstVerse
        {
            get { return first_verse; }
        }

        private int start_position = -1;
        /// <summary>
        /// Position in verse of first letter of first word in sentence
        /// </summary>
        public int StartPosition
        {
            get { return start_position; }
        }

        private Verse last_verse = null;
        /// <summary>
        /// Last verse in sentence (same as first verse if sentence within a single verse)
        /// </summary>
        public Verse LastVerse
        {
            get { return last_verse; }
        }

        private int end_position = -1;
        /// <summary>
        /// Position in verse of last letter of last word in sentence
        /// </summary>
        public int EndPosition
        {
            get { return end_position; }
        }

        private int chapter_count = 0;
        /// <summary>
        /// Number of chapters the sentence is sread over (default = 1)
        /// </summary>
        public int ChapterCount
        {
            get { return chapter_count; }
        }

        private int verse_count = 0;
        /// <summary>
        /// Number of verses the sentence is sread over (default = 1)
        /// </summary>
        public int VerseCount
        {
            get { return verse_count; }
        }

        private int word_count = 0;
        /// <summary>
        /// Number of words in the sentence text
        /// </summary>
        public int WordCount
        {
            get { return word_count; }
        }

        private int letter_count = 0;
        /// <summary>
        /// Number of letters in the sentence text
        /// </summary>
        public int LetterCount
        {
            get { return letter_count; }
        }

        private int unique_letter_count = 0;
        /// <summary>
        /// Number of letters the sentence text is using
        /// </summary>
        public int UniqueLetterCount
        {
            get { return unique_letter_count; }
        }

        private string text = null;
        /// <summary>
        /// Original sentence text
        /// </summary>
        public string Text
        {
            get { return text; }
        }
        public override string ToString()
        {
            return this.Text;
        }

        /// </summary>
        /// <param name="first_verse">First verse in sentence</param>
        /// <param name="start_position">Position in verse of first letter of first word in sentence</param>
        /// <param name="last_verse">Last verse in sentence</param>
        /// <param name="end_position">Position in verse of last letter of last word in sentence</param>
        /// <param name="text">Complete text of sentence within its verse or across multiple verses</param>
        public Sentence(Verse first_verse, int start_position, Verse last_verse, int end_position, string text)
        {
            this.first_verse = first_verse;
            this.start_position = start_position;
            this.last_verse = last_verse;
            this.end_position = end_position;
            this.text = text.Trim();

            this.chapter_count = last_verse.Chapter.SortedNumber - first_verse.Chapter.SortedNumber + 1;

            this.verse_count = last_verse.Number - first_verse.Number + 1;

            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }
            this.word_count = text.Split().Length;
            this.letter_count = text.Length - word_count + 1;

            List<char> unique_letters = new List<char>();
            foreach (char letter in text)
            {
                if (letter == ' ') continue;

                if (!unique_letters.Contains(letter))
                {
                    unique_letters.Add(letter);
                }
            }
            this.unique_letter_count = unique_letters.Count;
        }
    }
}
