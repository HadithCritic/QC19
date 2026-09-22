using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class WordPartGrammar
    {
        public static Dictionary<string, string> Language = null;

        public static List<string> Keys = new List<string>()
        {
            "INL",
            "DET",
            "Al+",
            "DEF",
            "INDEF",
            "1",
            "2",
            "3",
            "M",
            "F",
            "S",
            "D",
            "P",
            "PRON",
            "+PRON",
            "ADJ",
            "T",
            "LOC",
            "N",
            "PN",
            "ACT_PCPL",
            "PASS_PCPL",
            "VN",
            "IMPN",
            "NOM",
            "ACC",
            "GEN",
            "DEM",
            "REL",
            "V",
            "PERF",
            "IMPF",
            "IMPV",
            "JUS",
            "ACT",
            "PASS",
            "SUBJ",
            "IND",
            "(I)",
            "(II)",
            "(III)",
            "(IV)",
            "(V)",
            "(VI)",
            "(VII)",
            "(VIII)",
            "(IX)",
            "(X)",
            "(XI)",
            "(XII)",
            "AC",
            "AMD",
            "ANS",
            "AVR",
            "CERT",
            "EXH",
            "EXL",
            "EXP",
            "NEG",
            "PREV",
            "PRO",
            "RES",
            "RET",
            "SUB",
            "SUR",
            "INC",
            "INTG",
            "A_INTG+",
            "CAUS",
            "f_CAUS+",
            "COND",
            "RSLT",
            "f_RSLT+",
            "INT",
            "l_PRP+",
            "l_IMPV+",
            "PP",
            "bi+",
            "ka+",
            "ta+",
            "l_PP+",
            "+l_PP",
            "w_PP+",
            "EQ",
            "A_EQ+",
            "COM",
            "w_COM+",
            "CONJ",
            "f_CONJ+",
            "w_CONJ+",
            "EMPH",
            "l_EMPH+",
            "+n_EMPH",
            "VOC",
            "ya+",
            "ha+",
            "+VOC",
            "REM",
            "f_REM+",
            "w_REM+",
            "FUT",
            "sa+",
            "PART",
            "b_PART+",
            "CIRC",
            "w_CIRC+",
            "SUP",
            "f_SUP+",
            "w_SUP+",
            "+A_SILENT"
        };

        public bool Contains(string key)
        {
            return (
                    (type == key) ||
                    (position == key) ||
                    (attribute == key) ||
                    (qualifier == key) ||
                    (person_degree == key) ||
                    (person_gender == key) ||
                    (person_number == key) ||
                    (mood == key) ||
                    (lemma == key) ||
                    (root == key) ||
                    (special_group == key)
                   );
        }

        private WordPart word_part = null;
        public WordPart WordPart
        {
            get { return word_part; }
        }

        private string type = null;
        public string Type
        {
            get { return type; }
        }

        private string position = null;
        public string Position
        {
            get { return position; }
        }

        private string attribute = null;
        public string Attribute
        {
            get { return attribute; }
        }

        private string qualifier = null;
        public string Qualifier
        {
            get { return qualifier; }
        }

        private string person_degree = null;
        public string PersonDegree
        {
            get { return person_degree; }
        }

        private string person_gender = null;
        public string PersonGender
        {
            get { return person_gender; }
        }

        private string person_number = null;
        public string PersonNumber
        {
            get { return person_number; }
        }

        private string mood = null;
        public string Mood
        {
            get { return mood; }
        }

        private string lemma = null;
        public string Lemma
        {
            get { return lemma; }
        }

        private string root = null;
        public string Root
        {
            get { return root; }
        }

        private string special_group = null;
        public string SpecialGroup
        {
            get { return special_group; }
        }

        // instance constructor
        //Type	Position	Attribute	Qualifier	PersonDegree	PersonGender	PersonNumber	Mood	Lemma	Root	_SpecialGroup
        public WordPartGrammar(WordPart word_part, List<string> grammar)
        {
            if (word_part != null)
            {
                this.word_part = word_part;

                if (grammar != null)
                {
                    if (grammar.Count > 1)
                    {
                        this.type = grammar[0];

                        switch (this.type)
                        {
                            case "PREFIX":
                                {
                                    //Al+
                                    //bi+
                                    //bip+
                                    //ka+
                                    //ta+
                                    //sa+
                                    //ya+
                                    //ha+
                                    //A_INTG+
                                    //A_EQ+"
                                    //b_PART+
                                    //f_REM+
                                    //f_CONJ+
                                    //f_RSLT+
                                    //f_SUP+
                                    //f_CAUS+
                                    //l_PP+
                                    //l_EMPH+
                                    //l_PRP+
                                    //l_IMPV+
                                    //w_CONJ+
                                    //w_REM+
                                    //w_CIRC+
                                    //w_SUP+
                                    //w_PP+
                                    //w_COM+
                                    if (grammar.Count == 2)
                                    {
                                        this.position = grammar[1];
                                    }
                                    else
                                    {
                                        throw new Exception("WordPartGrammar: Invalide PREFIX at word part " + word_part.Address);
                                    }
                                }
                                break;
                            case "STEM":
                                {
                                    for (int i = 1; i < grammar.Count; i++)
                                    {
                                        string[] parts = grammar[i].Split(':');

                                        if (parts.Length == 2)
                                        {
                                            if (parts[0] == "POS")
                                            {
                                                //INL
                                                //N
                                                //PN
                                                //V
                                                //ADJ
                                                //IMPN
                                                //AC
                                                //AMD
                                                //ANS
                                                //AVR
                                                //CERT
                                                //COND
                                                //EXH
                                                //EXL
                                                //EXP
                                                //FUT
                                                //INC
                                                //INT
                                                //INTG
                                                //NEG
                                                //PREV
                                                //PRO
                                                //RES
                                                //RET
                                                //SUP
                                                //SUR
                                                //PP
                                                //CONJ
                                                //SUB
                                                //EQ
                                                //REM
                                                //CIRC
                                                //COM
                                                //RSLT
                                                //CAUS
                                                //EMPH
                                                //PRP
                                                //IMPV
                                                //PRON
                                                //DEM
                                                //REL
                                                //T
                                                //LOC
                                                this.position = parts[1];
                                            }
                                            else if (parts[0] == "LEM")
                                            {
                                                this.lemma = parts[1];
                                            }
                                            else if (parts[0] == "ROOT")
                                            {
                                                this.root = parts[1];
                                            }
                                            else if (parts[0] == "SP")
                                            {
                                                this.special_group = parts[1];
                                            }
                                            else if (parts[0] == "MOOD")
                                            {
                                                this.mood = parts[1];
                                            }
                                            else
                                            {
                                                throw new Exception("WordPartGrammar: Invalide STEM at word part " + word_part.Address);
                                            }
                                        }
                                        else if (parts.Length == 1) // attribute or qualifier
                                        {
                                            switch (parts[0])
                                            {
                                                case "1":
                                                case "2":
                                                case "3":
                                                    this.person_degree = parts[0][0].ToString();
                                                    break;
                                                case "M":
                                                case "F":
                                                    this.person_gender = parts[0][0].ToString();
                                                    break;
                                                case "S":
                                                case "D":
                                                case "P":
                                                    this.person_number = parts[0][0].ToString();
                                                    break;
                                                case "1M":
                                                case "2M":
                                                case "3M":
                                                case "1F":
                                                case "2F":
                                                case "3F":
                                                    this.person_degree = parts[0][0].ToString();
                                                    this.person_gender = parts[0][1].ToString();
                                                    break;
                                                case "1S":
                                                case "2S":
                                                case "3S":
                                                case "1D":
                                                case "2D":
                                                case "3D":
                                                case "1P":
                                                case "2P":
                                                case "3P":
                                                    this.person_degree = parts[0][0].ToString();
                                                    this.person_number = parts[0][1].ToString();
                                                    break;
                                                case "MS":
                                                case "FS":
                                                case "MD":
                                                case "FD":
                                                case "MP":
                                                case "FP":
                                                    this.person_gender = parts[0][0].ToString();
                                                    this.person_number = parts[0][1].ToString();
                                                    break;
                                                case "1MS":
                                                case "2MS":
                                                case "3MS":
                                                case "1FS":
                                                case "2FS":
                                                case "3FS":
                                                case "1MD":
                                                case "2MD":
                                                case "3MD":
                                                case "1FD":
                                                case "2FD":
                                                case "3FD":
                                                case "1MP":
                                                case "2MP":
                                                case "3MP":
                                                case "1FP":
                                                case "2FP":
                                                case "3FP":
                                                    this.person_degree = parts[0][0].ToString();
                                                    this.person_gender = parts[0][1].ToString();
                                                    this.person_number = parts[0][2].ToString();
                                                    break;
                                                default:
                                                    if (String.IsNullOrEmpty(this.attribute))
                                                    {
                                                        //VN
                                                        //ACT_PCPL
                                                        //PASS_PCPL
                                                        //NOM
                                                        //ACC
                                                        //GEN
                                                        //DEF
                                                        //INDEF
                                                        //PERF
                                                        //IMPF
                                                        //IMPV
                                                        this.attribute = parts[0];
                                                    }
                                                    else
                                                    {
                                                        //NOM
                                                        //ACC
                                                        //ACT
                                                        //PASS
                                                        //(I)
                                                        //(II)
                                                        //(III)
                                                        //(IV)
                                                        //(V)
                                                        //(VI)
                                                        //(VII)
                                                        //(VIII)
                                                        //(IX)
                                                        //(X)
                                                        //(XI)
                                                        //(XII)
                                                        this.qualifier = parts[0];
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case "SUFFIX":
                                {
                                    if (grammar.Count == 2)
                                    {
                                        //SUFFIX|+n_EMPH
                                        //SUFFIX|+VOC
                                        //SUFFIX|+l_PP
                                        //SUFFIX|+PRON:2MP
                                        //SUFFIX|+A_SILENT

                                        string[] parts = grammar[1].Split(':');

                                        if (parts.Length == 1)
                                        {
                                            this.position = parts[0];
                                        }
                                        else if (parts.Length == 2)
                                        {
                                            if (parts[0] == "+PRON")
                                            {
                                                this.position = parts[0];

                                                if (parts[1].Length == 3)
                                                {
                                                    this.person_degree = parts[1][0].ToString();
                                                    this.person_gender = parts[1][1].ToString();
                                                    this.person_number = parts[1][2].ToString();
                                                }
                                                else if (parts[1].Length == 2)
                                                {
                                                    if ((parts[1][0] == '1') || (parts[1][0] == '2') || (parts[1][0] == '3'))
                                                    {
                                                        this.person_degree = parts[1][0].ToString();
                                                    }
                                                    else if ((parts[1][0] == 'M') || (parts[1][0] == 'F'))
                                                    {
                                                        this.person_gender = parts[1][0].ToString();
                                                    }
                                                    else if ((parts[1][0] == 'S') || (parts[1][0] == 'D') || (parts[1][0] == 'P'))
                                                    {
                                                        this.person_number = parts[1][0].ToString();
                                                    }

                                                    if ((parts[1][1] == 'M') || (parts[1][1] == 'F'))
                                                    {
                                                        this.person_gender = parts[1][1].ToString();
                                                    }
                                                    else if ((parts[1][1] == 'S') || (parts[1][1] == 'D') || (parts[1][1] == 'P'))
                                                    {
                                                        this.person_number = parts[1][1].ToString();
                                                    }
                                                }
                                                else if (parts[1].Length == 1)
                                                {
                                                    if ((parts[1][0] == '1') || (parts[1][0] == '2') || (parts[1][0] == '3'))
                                                    {
                                                        this.person_degree = parts[1][0].ToString();
                                                    }
                                                    else if ((parts[1][0] == 'M') || (parts[1][0] == 'F'))
                                                    {
                                                        this.person_gender = parts[1][0].ToString();
                                                    }
                                                    else if ((parts[1][0] == 'S') || (parts[1][0] == 'D') || (parts[1][0] == 'P'))
                                                    {
                                                        this.person_number = parts[1][0].ToString();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("WordPartGrammar: SUFFIX|+PRON expected at word part " + word_part.Address);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("WordPartGrammar: Invalide SUFFIX at word part " + word_part.Address);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("WordPartGrammar: Invalide SUFFIX at word part " + word_part.Address);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        // copy constructor
        //Type	Position	Attribute	Qualifier	PersonDegree	PersonGender	PersonNumber	Mood	Lemma	Root	_SpecialGroup
        public WordPartGrammar(WordPartGrammar grammar)
        {
            if (grammar != null)
            {
                this.word_part = grammar.WordPart;
                this.type = grammar.Type;
                this.position = grammar.Position;
                this.attribute = grammar.Attribute;
                this.qualifier = grammar.Qualifier;
                this.person_degree = grammar.PersonDegree;
                this.person_gender = grammar.PersonGender;
                this.person_number = grammar.PersonNumber;
                this.mood = grammar.Mood;
                this.lemma = grammar.Lemma;
                this.root = grammar.Root;
                this.special_group = grammar.SpecialGroup;
            }
        }

        ////Type	Position	Attribute	Qualifier	PersonDegree	PersonGender	PersonNumber    Mood	Lemma	Root	_SpecialGroup
        //public string ToTable()
        //{
        //    StringBuilder str = new StringBuilder();
        //    //str.Append(Type + "\t");
        //    str.Append(Position + "\t");
        //    str.Append(Attribute + "\t");
        //    str.Append(Qualifier + "\t");
        //    str.Append(PersonDegree + "\t");
        //    str.Append(PersonGender + "\t");
        //    str.Append(PersonNumber + "\t");
        //    str.Append(Mood + "\t");
        //    //str.Append(Lemma + "\t");
        //    //str.Append(Root + "\t");
        //    //str.Append(_SpecialGroup + "\t");
        //    if (str.Length > 1)
        //    {
        //        str.Remove(str.Length - 1, 1); // remove last "\t"
        //    }
        //    return str.ToString();
        //}
    }
}
