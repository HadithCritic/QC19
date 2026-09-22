using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class SimplificationRule
    {
        private string text = null;
        public string Text
        {
            get { return text; }
        }
        private string replacement = null;
        public string Replacement
        {
            get { return replacement; }
        }

        public SimplificationRule(string text, string replacement)
        {
            this.text = text;
            this.replacement = replacement;
        }
    }
}
