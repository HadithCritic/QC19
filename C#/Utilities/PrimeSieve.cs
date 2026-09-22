// ---------------------------------------------------------------------------
// Primes.cs : Dave's Garage Prime Sieve in C++
// ---------------------------------------------------------------------------
// https://github.com/davepl/Primes
// ---------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

public class PrimeSieve
{
    private int m_limit = 0;
    private BitArray m_candidates;
    private Dictionary<int, int> m_primes_under_n = new Dictionary<int, int> 
    { 
        { 10 , 1 },                 // Historical data for validating our results - the number of primes
        { 100 , 25 },               // to be found under some limit, such as 168 primes under 1000
        { 1000 , 168 },
        { 10000 , 1229 },
        { 100000 , 9592 },
        { 1000000 , 78498 },
        { 10000000 , 664579 },
        { 100000000 , 5761455 } 
    };

    public PrimeSieve(int limit)
    {
        m_limit = limit;
        m_candidates = new BitArray((int)((this.m_limit + 1) / 2), true);
    }
    private bool GetBit(int index)
    {
        if (index % 2 == 0)
            return false;
        return m_candidates[index / 2];
    }
    private void ClearBit(int index)
    {
        if (index % 2 == 0)
        {
            Console.WriteLine("You are setting even bits, which is sub-optimal");
            return;
        }
        m_candidates[index / 2] = false;
    }
    public void Run()
    {
        int factor = 3;
        int sqrt = (int)Math.Sqrt(this.m_limit);

        while (factor < sqrt)
        {
            for (int n = factor; n <= this.m_limit; n++)
            {
                if (GetBit(n))
                {
                    factor = n;
                    break;
                }
            }

            // if marking factor 3, you wouldn't mark 6 (it's a mult of 2) so start with the 3rd instance of this factor's multiple.
            // we can then step by factor * 2 because every second one is going to be even by definition
            for (int n = factor * 3; n <= this.m_limit; n += factor * 2)
                ClearBit(n);

            factor += 2;
        }
    }

    public void DisplayResult(bool show, TimeSpan duration, int passes)
    {
        if (show)
            Console.Write("2, ");

        int count = 1;
        for (int num = 3; num <= this.m_limit; num++)
        {
            if (GetBit(num))
            {
                if (show)
                    Console.Write(num + ", ");
                count++;
            }
        }
        if (show)
            Console.WriteLine("");
        Console.WriteLine("Passes: " + passes + ", Time: " + duration.TotalSeconds + ", Avg: " + (duration.TotalSeconds / passes) +
                        ", Limit: " + this.m_limit + ", Count: " + count + ", Valid: " + ValidateResult());
    }
    public bool ValidateResult()
    {
        if (m_primes_under_n.ContainsKey(this.m_limit))
            return this.m_primes_under_n[this.m_limit] == this.CountPrimes();
        return false;
    }
    public int CountPrimes()
    {
        int count = 0;
        for (int i = 0; i < this.m_candidates.Count; i++)
            if (m_candidates[i])
                count++;
        return count;
    }

    public static void MainXXX(string[] args)
    {
        DateTime start = DateTime.UtcNow;
        int passes = 0;
        PrimeSieve sieve = null;

        while ((DateTime.UtcNow - start).TotalSeconds < 10)
        {
            sieve = new PrimeSieve(1000000);
            sieve.Run();
            passes++;
        }

        TimeSpan duration = DateTime.UtcNow - start;
        if (sieve != null)
            sieve.DisplayResult(false, duration, passes);
    }
}

