using System;
using System.Collections.Generic;

// Find palindrome substrings in large strings in O(n) time
public class Manacher
{
    // p[i] = radius of longest palindrome centered at i in transformed string
    public int[] p;

    // transformed string with # and sentinels
    public string ms;

    // preprocess the string and run the algorithm
    public Manacher(string s)
    {
        ms = "@";
        foreach (char c in s)
        {
            ms += "#" + c;
        }
        ms += "#$";

        Run();
    }

    // returns length of longest palindrome centered at 'cen' in original string
    // 'odd' = 1 → check for odd-length, 'odd' = 0 → even-length
    private int GetLongestPalindrome(int cen, int odd)
    {
        int pos = 2 * cen + 2 + (odd == 0 ? 1 : 0);
        return p[pos];
    }
    private void Run()
    {
        int n = ms.Length;
        p = new int[n];
        int l = 0, r = 0;

        for (int i = 1; i < n - 1; ++i)
        {
            // mirror of i around center (l + r)/2
            int mirror = l + r - i;

            // initialize p[i] based on its mirror 
            // if within bounds
            if (i < r)
                p[i] = Math.Min(r - i, p[mirror]);

            // expand palindrome centered at i
            while (ms[i + 1 + p[i]] == ms[i - 1 - p[i]])
            {
                ++p[i];
            }

            // update [l, r] if the palindrome expands beyond current r
            if (i + p[i] > r)
            {
                l = i - p[i];
                r = i + p[i];
            }
        }
    }

    public bool IsPalindrome(int l, int r)
    {
        int len = r - l + 1;
        int cen = (l + r) / 2;
        return len <= GetLongestPalindrome(cen, len % 2);
    }
    public string LongestPalindrome(string s)
    {
        Manacher manacher = new Manacher(s);
        int n = s.Length;

        // maximum length found so far
        int maxLen = 1;

        // starting index of longest palindrome
        int bestStart = 0;

        for (int i = 0; i < n; i++)
        {
            // check for odd-length palindrome centered at i
            int oddLen = manacher.GetLongestPalindrome(i, 1);
            if (oddLen > maxLen)
            {
                maxLen = oddLen;
                bestStart = i - maxLen / 2;
            }

            // check for even-length palindrome centered between i and i+1
            int evenLen = manacher.GetLongestPalindrome(i, 0);
            if (evenLen > maxLen)
            {
                maxLen = evenLen;
                bestStart = i - maxLen / 2 + 1;
            }
        }

        // extract the longest palindromic substring
        return s.Substring(bestStart, maxLen);
    }
    public List<bool> ArePalindromes(string s, int[,] queries)
    {
        // preprocess the string using Manacher class
        Manacher manacher = new Manacher(s);
        List<bool> result = new List<bool>();

        // number of queries
        int qCount = queries.GetLength(0);

        for (int i = 0; i < qCount; i++)
        {
            int l = queries[i, 0];
            int r = queries[i, 1];

            // check if a[l..r] is a palindrome 
            // using O(1) query
            result.Add(manacher.IsPalindrome(l, r));
        }

        return result;
    }
}
