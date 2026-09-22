using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Composites
{
    class Program
    {
        public static void Main(string[] args)
        {
            StreamWriter file = null;
            try
            {
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Composite Number Generator - ©2009-2026 Ali Adams");
                Console.WriteLine("-------------------------------------------------");

                ConsoleKeyInfo exit;
                do
                {
                    Console.WriteLine();
                    Console.WriteLine("factors = -1  ==>  unit, primes and composites");
                    Console.WriteLine("factors =  0  ==>  composites");
                    Console.WriteLine("factors =  1  ==>  primes");
                    Console.WriteLine("factors =  n  ==>  composites with n factors");
                    Console.WriteLine();
                    Console.Write("How many prime factors-per-number to find?    ");
                    string n_str = Console.ReadLine();
                    int n = 0;
                    if (int.TryParse(n_str, out n))
                    {
                        string folder = "Composites";
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        string path = folder + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".txt";
                        file = File.CreateText(path);

                        if (n >= 0)
                        {
                            GenerateComposites(file, n);
                        }
                        else
                        {
                            GenerateComposites(file, -1);
                        }
                    }
                    else
                    {
                        break;
                    }

                    Console.WriteLine();
                    Console.Write("Exit? (y/n)");
                    exit = Console.ReadKey();
                    Console.WriteLine();

                } while ((exit.KeyChar != 'Y') && (exit.KeyChar != 'y'));
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }
        private static void GenerateComposites(StreamWriter file, int n)
        {
            if (file != null)
            {
                int count = 0;
                StringBuilder str = new StringBuilder();

                file.WriteLine();
                file.WriteLine("How many prime factors-per-number to find?    " + n.ToString());

                Console.Write("Duplicate, unique, or any factors (d/u/a)?    ");
                string factors_type = Console.ReadLine();
                file.WriteLine("Duplicate factors, unique, or any (d/u/a)?    " + factors_type);

                Console.Write("Maximum number?                               ");
                string max_str = Console.ReadLine();
                file.WriteLine("Maximum number?                               " + max_str);

                Console.WriteLine();

                Console.Write("Digit sum    (O/E/P/AP/XP/C/AC/XC/Any=ENTER)? ");
                string digit_sum_str = Console.ReadLine().ToUpper();
                file.WriteLine("Digit sum    (O/E/P/AP/XP/C/AC/XC/Any=ENTER)? " + digit_sum_str);

                Console.Write("Digital root (O/E/P/AP/XP/C/AC/XC/Any=ENTER)? ");
                string digital_root_str = Console.ReadLine().ToUpper();
                file.WriteLine("Digital root (O/E/P/AP/XP/C/AC/XC/Any=ENTER)? " + digital_root_str);

                bool numbers_only = false;
                Console.Write("Display numbers only without details   (y/n)? ");
                string numbers_only_str = Console.ReadLine();
                file.WriteLine("Display numbers only without details   (y/n)? " + numbers_only_str);
                numbers_only = ((numbers_only_str == "Y") || (numbers_only_str == "y"));
                if (numbers_only)
                {
                    int max = 0;
                    if (int.TryParse(max_str, out max))
                    {
                        Console.WriteLine();
                        file.WriteLine();

                        int min = (n == -1) ? 1 : 2;
                        for (int i = min; i <= max; i++)
                        {
                            int digit_sum = Numbers.DigitSum(i);
                            if (
                                (digit_sum_str == "") ||
                                ((digit_sum_str == "O") && (Numbers.IsOdd(digit_sum))) ||
                                ((digit_sum_str == "E") && (Numbers.IsEven(digit_sum))) ||
                                ((digit_sum_str == "P") && (Numbers.IsPrime(digit_sum))) ||
                                ((digit_sum_str == "AP") && (Numbers.IsAdditivePrime(digit_sum))) ||
                                ((digit_sum_str == "XP") && (Numbers.IsNonAdditivePrime(digit_sum))) ||
                                ((digit_sum_str == "C") && (Numbers.IsComposite(digit_sum))) ||
                                ((digit_sum_str == "AC") && (Numbers.IsAdditiveComposite(digit_sum))) ||
                                ((digit_sum_str == "XC") && (Numbers.IsNonAdditiveComposite(digit_sum)))
                               )
                            {
                                int digital_root = Numbers.DigitalRoot(i);
                                if (
                                    (digital_root_str == "") ||
                                    ((digital_root_str == "O") && (Numbers.IsOdd(digital_root))) ||
                                    ((digital_root_str == "E") && (Numbers.IsEven(digital_root))) ||
                                    ((digital_root_str == "P") && (Numbers.IsPrime(digital_root))) ||
                                    ((digital_root_str == "AP") && (Numbers.IsAdditivePrime(digital_root))) ||
                                    ((digital_root_str == "XP") && (Numbers.IsNonAdditivePrime(digital_root))) ||
                                    ((digital_root_str == "C") && (Numbers.IsComposite(digital_root))) ||
                                    ((digital_root_str == "AC") && (Numbers.IsAdditiveComposite(digital_root))) ||
                                    ((digital_root_str == "XC") && (Numbers.IsNonAdditiveComposite(digital_root)))
                                   )
                                {
                                    List<long> factors = Numbers.Factorize(i);

                                    // COMPOSITES
                                    if ((n == -1) || ((n == 0) && (factors.Count > 1)) || ((n > 1) && (factors.Count == n)))
                                    {
                                        bool all_are_duplicate = false;
                                        bool all_are_unique = true;
                                        if (factors_type.ToLower() == "d")
                                        {
                                            for (int j = 1; j < factors.Count; j++)
                                            {
                                                all_are_duplicate = (factors[0] == factors[j]);
                                            }
                                        }
                                        else if (factors_type.ToLower() == "u")
                                        {
                                            for (int j = 0; j < factors.Count; j++)
                                            {
                                                for (int k = j + 1; k < factors.Count; k++)
                                                {
                                                    if (factors[j] == factors[k])
                                                    {
                                                        all_are_unique = false;
                                                        break;
                                                    }
                                                }
                                                if (!all_are_unique)
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        if (
                                            ((factors_type.ToLower() == "d") && all_are_duplicate)
                                            ||
                                            ((factors_type.ToLower() == "u") && all_are_unique)
                                            ||
                                            ((factors_type.ToLower() != "d") && (factors_type.ToLower() != "u"))
                                           )
                                        {
                                            count++;

                                            str.Length = 0;
                                            foreach (long factor in factors)
                                            {
                                                str.Append(factor + "\t");
                                            }
                                            str.Remove(str.Length - 1, 1);

                                            Console.WriteLine(i.ToString());
                                            file.WriteLine(i.ToString());
                                        }
                                    }
                                    else if ((n == 1) && (factors.Count == n)) // PRIMES
                                    {
                                        count++;

                                        Console.WriteLine(i.ToString());
                                        file.WriteLine(i.ToString());
                                    }
                                }
                            }
                        } // for
                    }
                    if (count == 0)
                    {
                        Console.WriteLine("No matches were found!");
                        file.WriteLine("No matches were found!");
                    }

                    file.Flush();
                }
                else // verbose mode
                {
                    int max = 0;
                    if (int.TryParse(max_str, out max))
                    {
                        Console.WriteLine();
                        file.WriteLine();
                        if (n == 1) // PRIMES
                        {
                            Console.WriteLine("#\tN\tP\tAP\tXP\tFactors");
                            file.WriteLine("#\tN\tP\tAP\tXP\tFactors");
                        }
                        else // COMPOSITES
                        {
                            Console.WriteLine("#\tN\tC\tAC\tXC\tFactors");
                            file.WriteLine("#\tN\tC\tAC\tXC\tFactors");
                        }

                        int min = (n == -1) ? 1 : 2;
                        for (int i = min; i <= max; i++)
                        {
                            int digit_sum = Numbers.DigitSum(i);
                            if (
                                (digit_sum_str == "") ||
                                ((digit_sum_str == "O") && (Numbers.IsOdd(digit_sum))) ||
                                ((digit_sum_str == "E") && (Numbers.IsEven(digit_sum))) ||
                                ((digit_sum_str == "P") && (Numbers.IsPrime(digit_sum))) ||
                                ((digit_sum_str == "AP") && (Numbers.IsAdditivePrime(digit_sum))) ||
                                ((digit_sum_str == "XP") && (Numbers.IsNonAdditivePrime(digit_sum))) ||
                                ((digit_sum_str == "C") && (Numbers.IsComposite(digit_sum))) ||
                                ((digit_sum_str == "AC") && (Numbers.IsAdditiveComposite(digit_sum))) ||
                                ((digit_sum_str == "XC") && (Numbers.IsNonAdditiveComposite(digit_sum)))
                               )
                            {
                                int digital_root = Numbers.DigitalRoot(i);
                                if (
                                    (digital_root_str == "") ||
                                    ((digital_root_str == "O") && (Numbers.IsOdd(digital_root))) ||
                                    ((digital_root_str == "E") && (Numbers.IsEven(digital_root))) ||
                                    ((digital_root_str == "P") && (Numbers.IsPrime(digital_root))) ||
                                    ((digital_root_str == "AP") && (Numbers.IsAdditivePrime(digital_root))) ||
                                    ((digital_root_str == "XP") && (Numbers.IsNonAdditivePrime(digital_root))) ||
                                    ((digital_root_str == "C") && (Numbers.IsComposite(digital_root))) ||
                                    ((digital_root_str == "AC") && (Numbers.IsAdditiveComposite(digital_root))) ||
                                    ((digital_root_str == "XC") && (Numbers.IsNonAdditiveComposite(digital_root)))
                                   )
                                {
                                    List<long> factors = Numbers.Factorize(i);

                                    // COMPOSITES
                                    if ((n == -1) || ((n == 0) && (factors.Count > 1)) || ((n > 1) && (factors.Count == n)))
                                    {
                                        bool all_are_duplicate = false;
                                        bool all_are_unique = true;
                                        if (factors_type.ToLower() == "d")
                                        {
                                            for (int j = 1; j < factors.Count; j++)
                                            {
                                                all_are_duplicate = (factors[0] == factors[j]);
                                            }
                                        }
                                        else if (factors_type.ToLower() == "u")
                                        {
                                            for (int j = 0; j < factors.Count; j++)
                                            {
                                                for (int k = j + 1; k < factors.Count; k++)
                                                {
                                                    if (factors[j] == factors[k])
                                                    {
                                                        all_are_unique = false;
                                                        break;
                                                    }
                                                }
                                                if (!all_are_unique)
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        if (
                                            ((factors_type.ToLower() == "d") && all_are_duplicate)
                                            ||
                                            ((factors_type.ToLower() == "u") && all_are_unique)
                                            ||
                                            ((factors_type.ToLower() != "d") && (factors_type.ToLower() != "u"))
                                           )
                                        {
                                            count++;

                                            str.Length = 0;
                                            foreach (long factor in factors)
                                            {
                                                str.Append(factor + "\t");
                                            }
                                            str.Remove(str.Length - 1, 1);


                                            int C = Numbers.CompositeIndexOf(i) + 1;
                                            int AC = Numbers.AdditiveCompositeIndexOf(i) + 1;
                                            int XC = Numbers.NonAdditiveCompositeIndexOf(i) + 1;

                                            Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", count, i, (C <= 0) ? "" : C.ToString(), (AC <= 0) ? "" : AC.ToString(), (XC <= 0) ? "" : XC.ToString(), str.ToString());
                                            file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", count, i, (C <= 0) ? "" : C.ToString(), (AC <= 0) ? "" : AC.ToString(), (XC <= 0) ? "" : XC.ToString(), str.ToString());
                                        }
                                    }
                                    else if ((n == 1) && (factors.Count == n)) // PRIMES
                                    {
                                        count++;
                                        int P = Numbers.PrimeIndexOf(i) + 1;
                                        int AP = Numbers.AdditivePrimeIndexOf(i) + 1;
                                        int XP = Numbers.NonAdditivePrimeIndexOf(i) + 1;

                                        Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", count, i, (P <= 0) ? "" : P.ToString(), (AP <= 0) ? "" : AP.ToString(), (XP <= 0) ? "" : XP.ToString(), factors[0].ToString());
                                        file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", count, i, (P <= 0) ? "" : P.ToString(), (AP <= 0) ? "" : AP.ToString(), (XP <= 0) ? "" : XP.ToString(), factors[0].ToString());
                                    }
                                }
                            }
                        } // for
                    }
                    if (count == 0)
                    {
                        Console.WriteLine("No matches were found!");
                        file.WriteLine("No matches were found!");
                    }

                    file.Flush();
                }
            }
        }
    }
}
