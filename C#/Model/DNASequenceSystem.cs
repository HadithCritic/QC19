using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class DNASequenceSystem
    {
        // PrimeDNASequence System ©2014 Belkacem Meghzouchene, Algeria
        public const string DEFAULT_NAME = "Simplified29_DNA_A29T29C19G23";
        // Using a large DNA sequence sample with ~29% A, ~29% T, ~19% C, and ~23% G,
        // the 29 Arabic letters were grouped into four groups such that the sum of
        // letter frequencies in each group was approximately 29%, 29%, 19% and 23%.
        // See Help\DNA_Systems.xls.

        /// <summary>
        /// Name = TextMode_LetterOrder_LetterValue
        /// </summary>
        private string name = null;
        public string Name
        {
            get { return name; }
        }

        private string text_mode = null;
        public string TextMode
        {
            get { return text_mode; }
        }

        private string letter_order = null;
        public string LetterOrder
        {
            get { return letter_order; }
        }

        private string letter_value = null;
        public string LetterValue
        {
            get { return letter_value; }
        }

        private Dictionary<char, char> letter_values = null;
        public Dictionary<char, char> LetterValues
        {
            get { return letter_values; }
        }
        public char this[char letter]
        {
            get
            {
                if (letter_values.ContainsKey(letter))
                {
                    return letter_values[letter];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                if (letter_values.ContainsKey(letter))
                {
                    letter_values[letter] = value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }
        public void Clear()
        {
            letter_values.Clear();
        }
        public int Count
        {
            get { return letter_values.Count; }
        }
        public void Add(char letter, char value)
        {
            if (letter_values.ContainsKey(letter))
            {
                throw new ArgumentException();
            }
            else
            {
                letter_values.Add(letter, value);
            }
        }
        public Dictionary<char, char>.KeyCollection Keys
        {
            get { return letter_values.Keys; }
        }
        public Dictionary<char, char>.ValueCollection Values
        {
            get { return letter_values.Values; }
        }
        public bool ContainsKey(char letter)
        {
            return letter_values.ContainsKey(letter);
        }

        public DNASequenceSystem()
            : this(DEFAULT_NAME)
        {
        }

        public DNASequenceSystem(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                string[] parts = name.Split('_');
                if (parts.Length == 3)
                {
                    this.name = name;
                    this.text_mode = parts[0];
                    this.letter_order = parts[1];
                    this.letter_value = parts[2];

                    this.letter_values = new Dictionary<char, char>();
                }
                else
                {
                    throw new Exception("Invalid DNA sequence system name." + "\r\n" +
                                        "Name must be in this format:" + "\r\n" +
                                        "TextMode_LetterOrder_LetterValue");
                }
            }
            else
            {
                throw new Exception("DNA sequence system name cannot be empty");
            }
        }

        public DNASequenceSystem(string name, Dictionary<char, char> letter_values)
            : this(name)
        {
            this.letter_values = letter_values;
        }

        public DNASequenceSystem(DNASequenceSystem dna_sequence_system)
            : this(dna_sequence_system.Name)
        {
            if (dna_sequence_system != null)
            {
                LetterValues.Clear();
                foreach (char key in dna_sequence_system.Keys)
                {
                    LetterValues.Add(key, dna_sequence_system[key]);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine(Name);
            str.AppendLine(this.ToOverview());

            return str.ToString();
        }
        public string ToOverview()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("----------------------------------------");
            str.AppendLine("Letter" + "\t" + "Value");
            str.AppendLine("----------------------------------------");
            foreach (char letter in LetterValues.Keys)
            {
                str.AppendLine(letter.ToString() + "\t" + LetterValues[letter].ToString());
            }
            str.AppendLine("----------------------------------------");

            return str.ToString();
        }
    }
}
