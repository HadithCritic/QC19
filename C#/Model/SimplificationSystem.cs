using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class SimplificationSystem
    {
        // Primalogy System ©2008 Ali Adams - www.heliwave.com
        //public const string DEFAULT_NAME = "Simplified29";
        public const string DEFAULT_NAME = "Original";

        /// <summary>
        /// Name = TextMode only
        /// </summary>
        private string name = null;
        public string Name
        {
            get { return name; }
        }

        private List<SimplificationRule> rules = null;
        public List<SimplificationRule> Rules
        {
            get { return rules; }
        }

        public string Simplify(string text)
        {
            foreach (SimplificationRule rule in rules)
            {
                text = text.Replace(rule.Text, rule.Replacement);
            }
            return text;
        }

        public SimplificationSystem()
            : this(DEFAULT_NAME)
        {
        }

        public SimplificationSystem(string name)
        {
            this.name = name;
            this.rules = new List<SimplificationRule>();
        }

        public SimplificationSystem(SimplificationSystem simplification_system)
            : this(simplification_system.Name)
        {
            if (simplification_system != null)
            {
                this.rules = new List<SimplificationRule>(simplification_system.Rules);
            }
        }
    }
}
