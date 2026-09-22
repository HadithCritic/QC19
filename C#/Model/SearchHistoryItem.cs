using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class SearchHistoryItem
    {
        public SearchType SearchType = SearchType.Text;
        public NumbersResultType NumbersResultType = NumbersResultType.Verses;
        public string Text = null;
        public string Header = null;
        public LanguageType LanguageType = LanguageType.RightToLeft;
        public string Translation = null;
        public List<Verse> Verses = new List<Verse>();
        public List<Phrase> Phrases = new List<Phrase>();
        public List<Word> Words = new List<Word>();
        public List<Letter> Letters = new List<Letter>();
    }
}
