using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class NumericalSystem
    {
        // Primalogy System ©2008 Ali Adams - www.heliwave.com
        //public const string DEFAULT_NAME = "Simplified29_Alphabet_Primes1";
        // Based on the name of surat Al-Fatiha to mean The Opener or The Key and
        // the fact that prime numbers are used in cryptography as private keys and
        // the structure of surat Al-Fatiha is built upon special kind of prime numbers
        // where the digit sum is prime too. They are called Additive prime numbers.
        // Surat Al-Fatiha = [7 verses] = [29 words] = [139 letters], all are prime numbers
        // and their digit sums (7=7, 2+9=11 and 1+3+9=13) are prime numbers too.
        // The Primalogy Systems itself produces additive prime numbers too:
        //   1. Al-Fatiha (8317 and 8+3+1+7=19)
        //   2. Al-Ikhlaas with BismAllah (4021 and 4+0+2+1=7) and
        //      Al-Ikhlaas without BismAllah (3167 and 3+1+6+7=17),
        //   3. Ayat Al-Kursi (11261 with 1+1+2+6+1=11)
        //   4. Ayat Ar-Rahmaan (683 with 6+8+3=17) [Fabiayyi aalaai Rabbikuma tukathibaan] where
        //      683 = 124th prime number = 4 * 31 and the aya has 4 words and is repeated 31 times
        //   5. The word "Allah" (269 with 2+6+9=17)
        // See Help\Primalogy.pdf for full details in English and
        // See Help\Primalogy_AR.pdf for full details in Arabic.
        public const string DEFAULT_NAME = "Original_Alphabet_Primes1";
        public const string DEFAULT_SUB_NAME = "Alphabet_Primes1";

        //private NumericalSystemScope scope = NumericalSystemScope.Book;
        //public NumericalSystemScope Scope
        //{
        //    get { return scope; }
        //    set
        //    {
        //        scope = value;
        //        //TODO update letter_order and letter_values
        //    }
        //}

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

        private long letter_values_sum = 0L;
        public long LetterValuesSum
        {
            get { return letter_values_sum; }
        }
        private Dictionary<char, long> letter_values = null;
        public Dictionary<char, long> LetterValues
        {
            get { return letter_values; }
        }
        public long this[char letter]
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
        public void Add(char letter, long value)
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
        public Dictionary<char, long>.KeyCollection Keys
        {
            get { return letter_values.Keys; }
        }
        public Dictionary<char, long>.ValueCollection Values
        {
            get { return letter_values.Values; }
        }
        public bool ContainsKey(char letter)
        {
            return letter_values.ContainsKey(letter);
        }

        public bool AbsolutePositions = false;
        public bool AddToLetterLNumber = true;
        public bool AddToLetterWNumber = true;
        public bool AddToLetterVNumber = true;
        public bool AddToLetterCNumber = true;
        public bool AddToLetterLDistance = true;
        public bool AddToLetterWDistance = true;
        public bool AddToLetterVDistance = true;
        public bool AddToLetterCDistance = true;
        public bool AddToWordWNumber = true;
        public bool AddToWordVNumber = true;
        public bool AddToWordCNumber = true;
        public bool AddToWordWDistance = true;
        public bool AddToWordVDistance = true;
        public bool AddToWordCDistance = true;
        public bool AddToVerseVNumber = true;
        public bool AddToVerseCNumber = true;
        public bool AddToVerseVDistance = true;
        public bool AddToVerseCDistance = true;
        public bool AddToChapterCNumber = true;

        public bool AddPositions = false;
        public bool AddDistancesToPrevious = false;
        public bool AddDistancesToNext = false;
        public bool AddDistancesWithinChapters = true;

        public NumericalSystem()
            : this(DEFAULT_NAME)
        {
        }

        public NumericalSystem(string name)
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

                    this.letter_values = new Dictionary<char, long>();
                }
                else
                {
                    throw new Exception("Invalid numerical system name." + "\r\n" +
                                        "Name must be in this format:" + "\r\n" +
                                        "TextMode_LetterOrder_LetterValue");
                }
            }
            else
            {
                throw new Exception("Numerical system name cannot be empty");
            }
        }

        public NumericalSystem(string name, Dictionary<char, long> letter_values)
            : this(name)
        {
            this.letter_values = new Dictionary<char, long>(letter_values);
        }

        public NumericalSystem(NumericalSystem numerical_system)
            : this(numerical_system.Name)
        {
            if (numerical_system != null)
            {
                letter_values_sum = 0L;
                this.letter_values.Clear();
                if (letter_values != null)
                {
                    foreach (char key in numerical_system.Keys)
                    {
                        long value = numerical_system[key];
                        letter_values_sum += value;
                        this.letter_values.Add(key, value);
                    }
                }
            }
        }

        public long CalculateValue(char character)
        {
            if (letter_values == null) return 0L;
            if (!letter_values.ContainsKey(character)) return 0L;

            return letter_values[character];
        }

        public long CalculateValue(string text)
        {
            if (String.IsNullOrEmpty(text)) return 0L;
            if (letter_values == null) return 0L;

            text = text.Simplify(text_mode);

            if (!text.IsArabic())  // eg English
            {
                text = text.ToUpper();
            }

            long result = 0L;
            if (letter_value.StartsWith("Base"))
            {
                string radix_str = "";
                int pos = 4;
                while (Char.IsDigit(letter_value[pos]))
                {
                    radix_str += letter_value[pos];
                    pos++;
                }
                int radix;
                if (int.TryParse(radix_str, out radix))
                {
                    StringBuilder str = new StringBuilder();
                    string[] words = text.Split();
                    foreach (string word in words)
                    {
                        for (int i = 0; i < word.Length; i++)
                        {
                            if (letter_values.ContainsKey(word[i]))
                            {
                                str.Insert(0, letter_values[word[i]]);
                            }
                        }
                        result += Radix.Decode(str.ToString(), radix);
                        str.Length = 0;
                    }
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (letter_values.ContainsKey(text[i]))
                    {
                        result += letter_values[text[i]];
                    }
                }
            }
            return result;
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

            if (
                 (AddPositions && AddToLetterLNumber) ||
                 (AddPositions && AddToLetterWNumber) ||
                 (AddPositions && AddToLetterVNumber) ||
                 (AddPositions && AddToLetterCNumber) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToLetterLDistance) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToLetterWDistance) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToLetterVDistance) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToLetterCDistance)
               )
            {
                str.Append("Add to each letter  value");
                str.Append("\t"); if (AddPositions && AddToLetterLNumber) str.Append("L");
                str.Append("\t"); if (AddPositions && AddToLetterWNumber) str.Append("W");
                str.Append("\t"); if (AddPositions && AddToLetterVNumber) str.Append("V");
                str.Append("\t"); if (AddPositions && AddToLetterCNumber) str.Append("C");
                str.Append("\t"); if (AddDistancesToPrevious && AddToLetterLDistance) str.Append("←∆L");
                str.Append("\t"); if (AddDistancesToPrevious && AddToLetterWDistance) str.Append("←∆W");
                str.Append("\t"); if (AddDistancesToPrevious && AddToLetterVDistance) str.Append("←∆V");
                str.Append("\t"); if (AddDistancesToPrevious && AddToLetterCDistance) str.Append("←∆C");
                str.Append("\t"); if (AddDistancesToNext && AddToLetterLDistance) str.Append("∆L→");
                str.Append("\t"); if (AddDistancesToNext && AddToLetterWDistance) str.Append("∆W→");
                str.Append("\t"); if (AddDistancesToNext && AddToLetterVDistance) str.Append("∆V→");
                str.Append("\t"); if (AddDistancesToNext && AddToLetterCDistance) str.Append("∆C→");
                str.AppendLine();
            }


            if (
                 (AddPositions && AddToWordWNumber) ||
                 (AddPositions && AddToWordVNumber) ||
                 (AddPositions && AddToWordCNumber) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToWordWDistance) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToWordVDistance) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToWordCDistance)
               )
            {
                str.Append("Add to each word    value");
                str.Append("\t");
                str.Append("\t"); if (AddPositions && AddToWordWNumber) str.Append("W");
                str.Append("\t"); if (AddPositions && AddToWordVNumber) str.Append("V");
                str.Append("\t"); if (AddPositions && AddToWordCNumber) str.Append("C");
                str.Append("\t");
                str.Append("\t"); if (AddDistancesToPrevious && AddToWordWDistance) str.Append("←∆W");
                str.Append("\t"); if (AddDistancesToPrevious && AddToWordVDistance) str.Append("←∆V");
                str.Append("\t"); if (AddDistancesToPrevious && AddToWordCDistance) str.Append("←∆C");
                str.Append("\t");
                str.Append("\t"); if (AddDistancesToNext && AddToWordWDistance) str.Append("∆W→");
                str.Append("\t"); if (AddDistancesToNext && AddToWordVDistance) str.Append("∆V→");
                str.Append("\t"); if (AddDistancesToNext && AddToWordCDistance) str.Append("∆C→");
                str.AppendLine();
            }


            if (
                 (AddPositions && AddToVerseVNumber) ||
                 (AddPositions && AddToVerseCNumber) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToVerseVDistance) ||
                 ((AddDistancesToPrevious || AddDistancesToNext) && AddToVerseCDistance)
               )
            {
                str.Append("Add to each verse   value");
                str.Append("\t");
                str.Append("\t");
                str.Append("\t"); if (AddPositions && AddToVerseVNumber) str.Append("V");
                str.Append("\t"); if (AddPositions && AddToVerseCNumber) str.Append("C");
                str.Append("\t");
                str.Append("\t");
                str.Append("\t"); if (AddDistancesToPrevious && AddToVerseVDistance) str.Append("←∆V");
                str.Append("\t"); if (AddDistancesToPrevious && AddToVerseCDistance) str.Append("←∆C");
                str.Append("\t");
                str.Append("\t");
                str.Append("\t"); if (AddDistancesToNext && AddToVerseVDistance) str.Append("∆V→");
                str.Append("\t"); if (AddDistancesToNext && AddToVerseCDistance) str.Append("∆C→");
                str.AppendLine();
            }

            if (
                 (AddPositions && AddToChapterCNumber)
               )
            {
                str.Append("Add to each chapter value");
                str.Append("\t");
                str.Append("\t");
                str.Append("\t");
                str.Append("\t"); if (AddPositions && AddToChapterCNumber) str.Append("C");
                str.AppendLine();
            }

            str.AppendLine("AddPositions               = " + AddPositions);
            str.AppendLine("AddDistancesToPrevious     = " + AddDistancesToPrevious);
            str.AppendLine("AddDistancesToNext         = " + AddDistancesToNext);
            str.AppendLine("AddDistancesWithinChapters = " + AddDistancesWithinChapters);
            str.AppendLine();

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
        public string ToTabbedString()
        {
            return (TextMode +
            "\t" + LetterOrder +
            "\t" + LetterValue +
            "\t" + (AddToLetterLNumber ? "L" : "") +
            "\t" + (AddToLetterWNumber ? "W" : "") +
            "\t" + (AddToLetterVNumber ? "V" : "") +
            "\t" + (AddToLetterCNumber ? "C" : "") +
            "\t" + (AddToLetterLDistance ? "∆L" : "") +
            "\t" + (AddToLetterWDistance ? "∆W" : "") +
            "\t" + (AddToLetterVDistance ? "∆V" : "") +
            "\t" + (AddToLetterCDistance ? "∆C" : "") +
            "\t" + (AddToWordWNumber ? "W" : "") +
            "\t" + (AddToWordVNumber ? "V" : "") +
            "\t" + (AddToWordCNumber ? "C" : "") +
            "\t" + (AddToWordWDistance ? "∆W" : "") +
            "\t" + (AddToWordVDistance ? "∆V" : "") +
            "\t" + (AddToWordCDistance ? "∆C" : "") +
            "\t" + (AddToVerseVNumber ? "V" : "") +
            "\t" + (AddToVerseCNumber ? "C" : "") +
            "\t" + (AddToVerseVDistance ? "∆V" : "") +
            "\t" + (AddToVerseCDistance ? "∆C" : "") +
            "\t" + (AddToChapterCNumber ? "C" : "") +
            "\t" + (AddPositions) +
            "\t" + (AddDistancesToPrevious) +
            "\t" + (AddDistancesToNext) +
            "\t" + (AddDistancesWithinChapters)
            );
        }
    }
}
