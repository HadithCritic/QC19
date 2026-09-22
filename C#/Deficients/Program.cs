using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deficients
{
    class Program
    {
        public static void Main(string[] args)
        {
            StreamWriter file = null;
            try
            {
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Deficient/Abundant Numbers - ©2009-2026 Ali Adams");
                Console.WriteLine("-------------------------------------------------");

                string folder = "Deficients";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string path = folder + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".txt";
                file = File.CreateText(path);

                //ConsoleKeyInfo exit;
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(" 1. Deficient Index = Prime Index");
                    Console.WriteLine(" 2. Abundant  Index = Prime Index");
                    Console.WriteLine(" 3. Deficient Index = Composite Index");
                    Console.WriteLine(" 4. Abundant  Index = Composite Index");
                    Console.WriteLine(" 5. Deficient Index = Sum of Proper Divisors");
                    Console.WriteLine(" 6. Abundant  Index = Sum of Proper Divisors");
                    Console.WriteLine(" 7. Deficient Index = n Dimensions Number Index");
                    Console.WriteLine(" 8. Abundant  Index = n Dimensions Number Index");
                    Console.WriteLine(" 9. Similar Prime Factor Powers");
                    Console.WriteLine(" 0. Quit");
                    Console.WriteLine();

                    Console.Write("Choose a function: ");
                    string method = Console.ReadLine();
                    if (String.IsNullOrEmpty(method)) return;

                    switch (method)
                    {
                        case "0": return;
                        case "1":
                            {
                                Find_DF_EQ_P(file);
                                Find_DF_EQ_AP(file);
                                Find_DF_EQ_XP(file);
                            }
                            break;
                        case "2":
                            {
                                Find_AB_EQ_P(file);
                                Find_AB_EQ_AP(file);
                                Find_AB_EQ_XP(file);
                            }
                            break;
                        case "3":
                            {
                                Find_DF_EQ_C(file);
                                Find_DF_EQ_AC(file);
                                Find_DF_EQ_XC(file);
                            }
                            break;
                        case "4":
                            {
                                Find_AB_EQ_C(file);
                                Find_AB_EQ_AC(file);
                                Find_AB_EQ_XC(file);
                            }
                            break;
                        case "5":
                            {
                                Find_DF_EQ_SPD(file);
                            }
                            break;
                        case "6":
                            {
                                Find_AB_EQ_SPD(file);
                            }
                            break;
                        case "7":
                            {
                                Find_DF_EQ_nD(file, 1);
                                Find_DF_EQ_nD(file, 2);
                                Find_DF_EQ_nD(file, 3);
                                Find_DF_EQ_nD(file, 4);
                                Find_DF_EQ_nD(file, 5);
                                Find_DF_EQ_nD(file, 6);
                                Find_DF_EQ_nD(file, 7);
                                Find_DF_EQ_nD(file, 8);
                                Find_DF_EQ_nD(file, 9);
                                Find_DF_EQ_nD(file, 10);
                            }
                            break;
                        case "8":
                            {
                                Find_AB_EQ_nD(file, 1);
                                Find_AB_EQ_nD(file, 2);
                                Find_AB_EQ_nD(file, 3);
                                Find_AB_EQ_nD(file, 4);
                                Find_AB_EQ_nD(file, 5);
                                Find_AB_EQ_nD(file, 6);
                                Find_AB_EQ_nD(file, 7);
                                Find_AB_EQ_nD(file, 8);
                                Find_AB_EQ_nD(file, 9);
                                Find_AB_EQ_nD(file, 10);
                            }
                            break;
                        case "9":
                            {
                                Find_SimilarPrimeFactorPowers(file);
                            }
                            break;
                    }

                    //Console.WriteLine();
                    //Console.Write("Exit? (y/n)");
                    //exit = Console.ReadKey();
                    //Console.WriteLine();

                } while (true); //((exit.KeyChar != 'Y') && (exit.KeyChar != 'y'));
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }

        private static void Find_DF_EQ_P(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\tP");
                file.WriteLine("#\tN\tDF\tP");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int p = Numbers.PrimeIndexOf(n) + 1;

                    if (df == p)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + p);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + p);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_DF_EQ_AP(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\tAP");
                file.WriteLine("#\tN\tDF\tAP");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int ap = Numbers.AdditivePrimeIndexOf(n) + 1;

                    if (df == ap)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + ap);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + ap);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_DF_EQ_XP(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\tXP");
                file.WriteLine("#\tN\tDF\tXP");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int xp = Numbers.NonAdditivePrimeIndexOf(n) + 1;

                    if (df == xp)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + xp);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + xp);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_P(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\tP");
                file.WriteLine("#\tN\tAB\tP");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int p = Numbers.PrimeIndexOf(n) + 1;

                    if (ab == p)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + p);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + p);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_AP(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\tAP");
                file.WriteLine("#\tN\tAB\tAP");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int ap = Numbers.AdditivePrimeIndexOf(n) + 1;

                    if (ab == ap)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + ap);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + ap);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_XP(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\tXP");
                file.WriteLine("#\tN\tAB\tXP");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int xp = Numbers.NonAdditivePrimeIndexOf(n) + 1;

                    if (ab == xp)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + xp);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + xp);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }

        private static void Find_DF_EQ_C(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\tC");
                file.WriteLine("#\tN\tDF\tC");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int c = Numbers.CompositeIndexOf(n) + 1;

                    if (df == c)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + c);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + c);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_DF_EQ_AC(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\tAC");
                file.WriteLine("#\tN\tDF\tAC");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int ac = Numbers.AdditiveCompositeIndexOf(n) + 1;

                    if (df == ac)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + ac);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + ac);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_DF_EQ_XC(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\tXC");
                file.WriteLine("#\tN\tDF\tXC");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int xc = Numbers.NonAdditiveCompositeIndexOf(n) + 1;

                    if (df == xc)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + xc);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + xc);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_C(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\tC");
                file.WriteLine("#\tN\tAB\tC");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int c = Numbers.CompositeIndexOf(n) + 1;

                    if (ab == c)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + c);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + c);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_AC(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\tAC");
                file.WriteLine("#\tN\tAB\tAC");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int ac = Numbers.AdditiveCompositeIndexOf(n) + 1;

                    if (ab == ac)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + ac);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + ac);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_XC(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\tXC");
                file.WriteLine("#\tN\tAB\tXC");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int xc = Numbers.NonAdditiveCompositeIndexOf(n) + 1;

                    if (ab == xc)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + xc);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + xc);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }

        private static void Find_DF_EQ_SPD(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\t∑pDivisors");
                file.WriteLine("#\tN\tDF\t∑pDivisors");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int spd = (int)Numbers.SumOfProperDivisors(n);

                    if (df == spd)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + spd);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + spd);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_SPD(StreamWriter file)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\t∑pDivisors");
                file.WriteLine("#\tN\tAB\t∑pDivisors");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int spd = (int)Numbers.SumOfProperDivisors(n);

                    if (ab == spd)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + spd);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + spd);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }

        private static void Find_DF_EQ_nD(StreamWriter file, int dimension)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tDF\t" + dimension.ToString() + "D");
                file.WriteLine("#\tN\tDF\t" + dimension.ToString() + "D");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.DeficientNumbers.Count - 1) break;

                    long n = Numbers.DeficientNumbers[i];
                    if (n > max) break;

                    int df = i + 1;
                    int d3 = Numbers.NumberDimensionIndexOf(dimension, n) + 1;

                    if (df == d3)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + df + "\t" + d3);
                        file.WriteLine(count + "\t" + n + "\t" + df + "\t" + d3);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
        private static void Find_AB_EQ_nD(StreamWriter file, int dimension)
        {
            if (file != null)
            {
                int max = 100000000;
                //Console.Write("Max number     : ");
                //string max_str = Console.ReadLine();
                //if (!String.IsNullOrEmpty(max_str))
                {
                    //if (int.TryParse(max_str, out max))
                    {
                    };
                }

                StringBuilder str = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine("#\tN\tAB\t" + dimension.ToString() + "D");
                file.WriteLine("#\tN\tAB\t" + dimension.ToString() + "D");

                int count = 0;
                for (int i = 0; i < max; i++)
                {
                    if (i > Numbers.AbundantNumbers.Count - 1) break;

                    long n = Numbers.AbundantNumbers[i];
                    if (n > max) break;

                    int ab = i + 1;
                    int d3 = Numbers.NumberDimensionIndexOf(dimension, n) + 1;

                    if (ab == d3)
                    {
                        ++count;
                        Console.WriteLine(count + "\t" + n + "\t" + ab + "\t" + d3);
                        file.WriteLine(count + "\t" + n + "\t" + ab + "\t" + d3);
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }

        private static void Find_SimilarPrimeFactorPowers(StreamWriter file)
        {
            if (file != null)
            {
                long number = 0L;
                Console.Write("Similar prime factor powers to number: ");
                string number_str = Console.ReadLine();
                if (!String.IsNullOrEmpty(number_str))
                {
                    if (long.TryParse(number_str, out number))
                    {
                        StringBuilder str = new StringBuilder();
                        Console.WriteLine();
                        Console.WriteLine("#\tN\tFactors");
                        file.WriteLine("#\tN\tFactors");

                        Dictionary<long, int> factors_with_powers = Numbers.FactorizeByPowers(number);
                        List<int> powers = new List<int>();
                        foreach (int power in factors_with_powers.Values)
                        {
                            powers.Add(power);
                        }
                        powers.Sort();

                        int count = 0;
                        for (long n = 2L; n <= number; n++) // start with 2 to skip 1 which is a non-prime and non-composite
                        {
                            Dictionary<long, int> fps = Numbers.FactorizeByPowers(n);
                            List<int> ps = new List<int>();
                            foreach (int power in fps.Values)
                            {
                                ps.Add(power);
                            }
                            ps.Sort();

                            bool same_lists = true;
                            if (powers.Count != ps.Count) // compare size
                            {
                                same_lists = false;
                            }
                            else // compare elements
                            {
                                for (int i = 0; i < factors_with_powers.Count; i++)
                                {
                                    if (powers[i] != ps[i])
                                    {
                                        same_lists = false;
                                        break;
                                    }
                                }
                            }

                            if (same_lists)
                            {
                                ++count;
                                Console.WriteLine(count + "\t" + n + "\t" + Numbers.FactorizeToString(n));
                                file.WriteLine(count + "\t" + n + "\t" + Numbers.FactorizeToString(n));
                            }
                        }

                        Console.WriteLine();
                        file.WriteLine();

                        file.Flush();
                    }
                }
            }
        }
    }
}
