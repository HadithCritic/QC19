using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Primes
{
    class Program
    {
        public static void Main(string[] args)
        {
            StreamWriter file = null;
            try
            {
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Primes Number Generator - ©2009-2026 Ali Adams");
                Console.WriteLine("----------------------------------------------");

                string folder = "Primes";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string path = null;

                Console.WriteLine();
                Console.Write("Find prime numbers ending in: ");
                string end_str = Console.ReadLine();
                int end = -1;
                if (int.TryParse(end_str, out end)) // "" makes end = 0 and returns false
                {
                    if (end < 0)
                    {
                        end = -1; // generate all primes
                    }
                    path = folder + Path.DirectorySeparatorChar + "primes_end_in_" + end_str + ".txt";
                }
                else
                {
                    end = -1; // in case of "" which makes end = 0
                    path = folder + Path.DirectorySeparatorChar + "primes_end_in_any" + ".txt";
                }

                file = File.CreateText(path);
                GeneratePrimes(file, end);

                Console.WriteLine();
                Console.Write("Press any key to exit.");
                Console.ReadKey();
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }
        private static void GeneratePrimes(StreamWriter file, int end)
        {
            if (file != null)
            {
                StringBuilder str = new StringBuilder();
                Console.WriteLine("#\tN\tP\tAP\tXP");
                file.WriteLine("#\tN\tP\tAP\tXP");

                int count = 0;
                int max = Numbers.Primes.Count;
                if (end == -1)
                {
                    foreach (long n in Numbers.Primes)
                    {
                        count++;
                        int p = Numbers.PrimeIndexOf(n) + 1;
                        int ap = Numbers.AdditivePrimeIndexOf(n) + 1;
                        int xp = Numbers.NonAdditivePrimeIndexOf(n) + 1;
                        Console.WriteLine(count + "\t" + n + "\t" + p + "\t" + ((ap > 0) ? ap.ToString() : "") + "\t" + ((xp > 0) ? xp.ToString() : "") + "\t");
                        file.WriteLine(count + "\t" + n + "\t" + p + "\t" + ((ap > 0) ? ap.ToString() : "") + "\t" + ((xp > 0) ? xp.ToString() : "") + "\t");
                    }
                }
                else
                {
                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        int end_length = Numbers.DigitCount(end);
                        long n = (long)(i * Math.Pow(10, end_length) + end);
                        if (n > Numbers.Primes[Numbers.Primes.Count - 1]) break;

                        int p = Numbers.PrimeIndexOf(n) + 1;
                        if (p > 0)
                        {
                            count++;
                            int ap = Numbers.AdditivePrimeIndexOf(n) + 1;
                            int xp = Numbers.NonAdditivePrimeIndexOf(n) + 1;
                            Console.WriteLine(count + "\t" + n + "\t" + p + "\t" + ((ap > 0) ? ap.ToString() : "") + "\t" + ((xp > 0) ? xp.ToString() : "") + "\t");
                            file.WriteLine(count + "\t" + n + "\t" + p + "\t" + ((ap > 0) ? ap.ToString() : "") + "\t" + ((xp > 0) ? xp.ToString() : "") + "\t");
                        }
                    }
                }

                Console.WriteLine();
                file.WriteLine();

                file.Flush();
            }
        }
    }
}
