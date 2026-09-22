using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class Phrase
    {
        public Verse Verse = null;
        public List<Word> Words = null;
        public List<Letter> Letters = null;

        public int Position = 0;
        public string Text = null;
        public override string ToString()
        {
            return this.Text;
        }

        public Phrase(Verse verse, int position, string text)
        {
            this.Verse = verse;
            this.Position = position;
            this.Text = text;
            this.Words = new List<Word>();
            this.Letters = new List<Letter>();

            if (Globals.EDITION != Edition.Standard)
            {
                if (!this.Verse.Text.IsArabicWithDiacritics())
                {
                    FillWordsAndLetters(this.Verse, this.Text);
                }
            }
        }

        // AI-generated and modified by Ali Adams 2026-06-12 for B67
        private void FillWordsAndLetters(Verse verse, string text)
        {
            if (verse == null) return;
            if (this.Words == null) return;
            if (this.Letters == null) return;
            if (String.IsNullOrEmpty(verse.Text)) return;
            if (String.IsNullOrEmpty(text)) return;

            int index = 0;

            while ((index = verse.Text.Simplify36().IndexOf(text, index, StringComparison.Ordinal)) != -1)
            {
                int match_start = index;
                int match_end = index + text.Length - 1;

                foreach (Word word in verse.Words)
                {
                    int word_start = word.Position;
                    int word_end = word_start + word.Text.Simplify36().Length - 1;

                    if (word_start <= match_end && word_end >= match_start)
                    {
                        if (!this.Words.Contains(word))
                        {
                            this.Words.Add(word);
                        }

                        for (int i = 0; i < word.Letters.Count; i++)
                        {
                            int letter_position = word_start + i;

                            if ((letter_position >= match_start) && (letter_position <= match_end))
                            {
                                Letter letter = word.Letters[i];

                                if (!this.Letters.Contains(letter))
                                {
                                    this.Letters.Add(letter);
                                }
                            }
                        }
                    }
                }

                // move forward by exactly 1 character to catch overlapping matches
                index++;
            }
        }
        //TODO too slow AND doesn't work correctly in Original text mode
    }
}
