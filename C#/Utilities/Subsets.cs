using System;
using System.Collections.Generic;
using System.Diagnostics;

// Yorye Nathan on 03-May-2015
// https://stackoverflow.com/questions/30006497/find-all-k-size-subsets-with-sum-s-of-an-n-size-bag-of-duplicate-unsorted-positi/30012781#30012781
/*
Please note that this is required for a **C# .NET 2.0** project (**Linq not allowed**).

I know very similar questions have been asked here and I have already produce some working code (see below) but still would like an advice as to how to make the algorithm faster given k and s conditions.

This is what I've learnt so far:
Dynamic programming is the most efficient way to finding ONE (not all) subsets. Please correct me if I am wrong. And is there a way of repeatedly calling the DP code to produce newer subsets till the bag (set with duplicates) is exhausted?

If not, then is there a way that may speed up the backtracking recursive algorithm I have below which does produce what I need but runs in O(2^n), I think, by taking s and k into account?

Here is my fixed bag of numbers that will NEVER change with n=114 and number range from 3 to 286:

        int[] numbers = new int[]
        {
            7, 286, 200, 176, 120, 165, 206, 75, 129, 109,
            123, 111, 43, 52, 99, 128, 111, 110, 98, 135,
            112, 78, 118, 64, 77, 227, 93, 88, 69, 60,
            34, 30, 73, 54, 45, 83, 182, 88, 75, 85,
            54, 53, 89, 59, 37, 35, 38, 29, 18, 45,
            60, 49, 62, 55, 78, 96, 29, 22, 24, 13,
            14, 11, 11, 18, 12, 12, 30, 52, 52, 44,
            28, 28, 20, 56, 40, 31, 50, 40, 46, 42,
            29, 19, 36, 25, 22, 17, 19, 26, 30, 20,
            15, 21, 11, 8, 8, 19, 5, 8, 8, 11,
            11, 8, 3, 9, 5, 4, 7, 3, 6, 3,
            5, 4, 5, 6
        };


**Requirements**

 - Space limit to 2-3GB max but time should be O(n^x) not O(x^n).

 - The bag must not be sorted and duplicate must not be removed.

 - The result should be the indices of the numbers in the matching
   subset, not the numbers themselves (as we have duplicates).


**Dynamic Programming Attempt**

Here is the C# dynamic programming version adapted from an answer to a similar question here on stackoverflow.com:

    using System;
    using System.Collections.Generic;

    namespace Utilities
    {
        public static class Combinations
        {
            private static Dictionary<int, bool> m_memo = new Dictionary<int, bool>();
            private static Dictionary<int, KeyValuePair<int, int>> m_previous = new Dictionary<int, KeyValuePair<int, int>>();
            static Combinations()
            {
                m_memo.Clear();
                m_previous.Clear();
                m_memo[0] = true;
                m_previous[0] = new KeyValuePair<int, int>(-1, 0);

            }

            public static bool FindSubset(IList<int> set, int sum)
            {
                //m_memo.Clear();
                //m_previous.Clear();
                //m_memo[0] = true;
                //m_previous[0] = new KeyValuePair<int, int>(-1, 0);

                for (int i = 0; i < set.Count; ++i)
                {
                    int n = set[i];
                    for (int s = sum; s >= n; --s)
                    {
                        if (m_memo.ContainsKey(s - n) && m_memo[s - n] == true)
                        {
                            m_memo[s] = true;

                            if (!m_previous.ContainsKey(s))
                            {
                                m_previous[s] = new KeyValuePair<int, int>(i, n);
                            }
                        }
                    }
                }

                return m_memo.ContainsKey(sum) && m_memo[sum];
            }
            public static IEnumerable<int> GetLastIndex(int sum)
            {
                while (m_previous[sum].Key != -1)
                {
                    yield return m_previous[sum].Key;
                    sum -= m_previous[sum].Value;
                }
            }

            public static void SubsetSumMain(string[] args)
            {
                int[] numbers = new int[]
                {
                    7, 286, 200, 176, 120, 165, 206, 75, 129, 109,
                    123, 111, 43, 52, 99, 128, 111, 110, 98, 135,
                    112, 78, 118, 64, 77, 227, 93, 88, 69, 60,
                    34, 30, 73, 54, 45, 83, 182, 88, 75, 85,
                    54, 53, 89, 59, 37, 35, 38, 29, 18, 45,
                    60, 49, 62, 55, 78, 96, 29, 22, 24, 13,
                    14, 11, 11, 18, 12, 12, 30, 52, 52, 44,
                    28, 28, 20, 56, 40, 31, 50, 40, 46, 42,
                    29, 19, 36, 25, 22, 17, 19, 26, 30, 20,
                    15, 21, 11, 8, 8, 19, 5, 8, 8, 11,
                    11, 8, 3, 9, 5, 4, 7, 3, 6, 3,
                    5, 4, 5, 6
                };

                int sum = 400;
                //int m_size = 4; // don't know to use in dynamic programming

                // call dynamic programming
                if (Numbers.FindSubset(numbers, sum))
                {
                    foreach (int index in Numbers.GetLastIndex(sum))
                    {
                        Console.Write((index + 1) + "." + numbers[index] + "\t");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                Console.ReadKey();
            }
        }
    }

**Recursive Programming Attempt**

and Here is the C# recursive programming version adapted from an answer to a similar question here on stackoverflow.com:

    using System;
    using System.Collections.Generic;

    namespace Utilities
    {
        public static class Combinations
        {
            private static int s_count = 0;
            public static int CountSubsets(int[] numbers, int index, int current, int sum, int m_size, List<int> result)
            {
                if ((numbers.Length <= index) || (current > sum)) return 0;
                if (result == null) result = new List<int>();

                List<int> temp = new List<int>(result);
                if (current + numbers[index] == sum)
                {
                    temp.Add(index);
                    if ((m_size == 0) || (temp.Count == m_size))
                    {
                        s_count++;
                    }
                }
                else if (current + numbers[index] < sum)
                {
                    temp.Add(index);
                    CountSubsets(numbers, index + 1, current + numbers[index], sum, m_size, temp);
                }

                CountSubsets(numbers, index + 1, current, sum, m_size, result);
                return s_count;
            }

            private static List<List<int>> m_subsets = new List<List<int>>();
            public static List<List<int>> FindSubsets(int[] numbers, int index, int current, int sum, int m_size, List<int> result)
            {
                if ((numbers.Length <= index) || (current > sum)) return m_subsets;
                if (result == null) result = new List<int>();

                List<int> temp = new List<int>(result);
                if (current + numbers[index] == sum)
                {
                    temp.Add(index);
                    if ((m_size == 0) || (temp.Count == m_size))
                    {
                        m_subsets.Add(temp);
                    }
                }
                else if (current + numbers[index] < sum)
                {
                    temp.Add(index);
                    FindSubsets(numbers, index + 1, current + numbers[index], sum, m_size, temp);
                }

                FindSubsets(numbers, index + 1, current, sum, m_size, result);

                return m_subsets;
            }

            public static void SubsetSumMain(string[] args)
            {
                int[] numbers = new int[]
                {
                    7, 286, 200, 176, 120, 165, 206, 75, 129, 109,
                    123, 111, 43, 52, 99, 128, 111, 110, 98, 135,
                    112, 78, 118, 64, 77, 227, 93, 88, 69, 60,
                    34, 30, 73, 54, 45, 83, 182, 88, 75, 85,
                    54, 53, 89, 59, 37, 35, 38, 29, 18, 45,
                    60, 49, 62, 55, 78, 96, 29, 22, 24, 13,
                    14, 11, 11, 18, 12, 12, 30, 52, 52, 44,
                    28, 28, 20, 56, 40, 31, 50, 40, 46, 42,
                    29, 19, 36, 25, 22, 17, 19, 26, 30, 20,
                    15, 21, 11, 8, 8, 19, 5, 8, 8, 11,
                    11, 8, 3, 9, 5, 4, 7, 3, 6, 3,
                    5, 4, 5, 6
                };

                int sum = 17;
                int m_size = 2;

                // call backtracking recursive programming
                Console.WriteLine("CountSubsets");
                int count = Numbers.CountSubsets(numbers, 0, 0, sum, m_size, null);
                Console.WriteLine("Count = " + count);
                Console.WriteLine();

                // call backtracking recursive programming
                Console.WriteLine("FindSubsets");
                List<List<int>> subsets = Numbers.FindSubsets(numbers, 0, 0, sum, m_size, null);
                for (int i = 0; i < subsets.Count; i++)
                {
                    if (subsets[i] != null)
                    {
                        Console.Write((i + 1).ToString() + ":\t");
                        for (int j = 0; j < subsets[i].Count; j++)
                        {
                            int index = subsets[i][j];
                            Console.Write((index + 1) + "." + numbers[index] + " ");
                        }
                        Console.WriteLine();
                    }
                }
                Console.WriteLine("Count = " + subsets.Count);

                Console.ReadKey();
            }
        }
    }

Please let me know how to restrict the dynamic programming version to subset m_size k and if I can call it repeatedly so it returns different subsets on every call until there are no more matching subsets.

Also I am not sure where to initialize the memo of the DP algorithm. I did it in the static constructor which auto-runs when accessing any method. Is this the correct initialization place or does it need to be moved to inside the FindSunset() method [commented out]?

As for the recursive version, is it backtracking? and how can we speed it up. It works correctly and takes k and s into account but totally inefficient.

Let's make this thread the mother of all C# SubsetSum related questions!

Thank you :)
*/
// Benchmarking ...
//int m_size = 7;
//int sum = 313;
//Subsets found: 122131009
//00:00:30.2087779

//int m_size = 9;
//int sum = 911;
//Subsets found: 766732123
//00:04:08.2344704

//int m_size = 10;
//int sum = 1000;
//Subsets found: 6323560004
//00:33:38.4460587

public class Subsets
{
    public sealed class Item
    {
        public readonly int Index;
        public readonly long Value;
        public Item(int index, long value)
        {
            this.Index = index;
            this.Value = value;
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}", this.Index + 1, this.Value);
        }
    }
    public readonly Item[] Items;
    private readonly int m_item_count;
    private readonly long[] m_tail_sums;

    /// <summary>
    /// Construct an object to store the number m_candidates
    /// </summary>
    /// <param name="numbers">the m_candidates of numbers to search for matching subsets</param>
    public Subsets(long[] numbers)
    {
        m_item_count = numbers.Length;
        this.Items = new Item[m_item_count];
        for (int i = 0; i < m_item_count; i++)
        {
            this.Items[i] = new Item(i, numbers[i]);
        }

        // Sort Items descendingly by their value
        Array.Sort(this.Items, (a, b) => b.Value.CompareTo(a.Value));

        // Save tail-sums to allow immediate access by index
        m_tail_sums = new long[m_item_count + 1];
        long sum = 0;
        for (int i = m_item_count - 1; i >= 0; i--)
        {
            sum += this.Items[i].Value;
            m_tail_sums[i] = sum;
        }
    }

    // find subsets
    /// <summary>
    /// Find all subsets where the sum of numbers in a subset equals the target sum.
    /// </summary>
    /// <param name="count">count of items in subsets to be found</param>
    /// <param name="sum">target sum</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public void Find(long sum, Action<Item[]> callback)
    {
        for (int count = 1; count <= m_item_count; count++)
            this.Find(count, sum, callback);
    }
    /// <summary>
    /// Find all subsets with specified count regardless of sum.
    /// </summary>
    /// <param name="count">count of items in subsets to be found</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public void Find(int count, Action<Item[]> callback)
    {
        if (count > m_item_count)
            return;

        this.Scan(0, count, new List<Item>(), callback);
    }
    /// <summary>
    /// Find all subsets with specified count where the sum of numbers in a subset equals the target sum.
    /// </summary>
    /// <param name="count">count of items in subsets to be found</param>
    /// <param name="sum">target sum</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public void Find(int count, long sum, Action<Item[]> callback)
    {
        if (count > m_item_count)
            return;

        this.Scan(0, count, sum, new List<Item>(), callback);
    }
    private void Scan(int index, int count, long sum, List<Item> subset, Action<Item[]> callback)
    {
        // No more Items to add, and current subset is guranteed to be valid
        if (count == 0)
        {
            // Callback with current subset
            callback(subset.ToArray());
            return;
        }

        // Save the smallest remaining sum
        int start_index = m_item_count - count;
        long tail_sum = m_tail_sums[start_index];

        // Smallest possible sum is greater than target sum,
        // so a valid subset cannot be found
        if (tail_sum > sum)
        {
            return;
        }

        // Find largest number that satisfies the condition that a valid subset can be found
        tail_sum -= this.Items[start_index].Value;
        // And remember the last index that satisfies the condition
        int last_index = start_index;
        while ((start_index > index) && (tail_sum + this.Items[start_index - 1].Value <= sum))
        {
            start_index--;
        }

        // Find the first number in the sorted items that is the largest number we've just found
        // (in case of duplicates)
        while ((start_index > index) && (Items[start_index] == Items[start_index - 1]))
        {
            start_index--;
        }

        // [start_index .. last_index] is the full range we must check in recursion
        for (int i = start_index; i <= last_index; i++)
        {
            // Find the largest possible sum, which is the target sum of the first k items
            // starting at current start_index
            long max_sum = m_tail_sums[i] - m_tail_sums[i + count];

            // The largest possible sum is less than the target sum, so a valid subset cannot be found
            if (max_sum < sum)
            {
                return;
            }

            // Add current item to the subset
            Item item = this.Items[i];
            subset.Add(item);

            // Recurse through the sub-problem to the right
            this.Scan(i + 1, count - 1, sum - item.Value, subset, callback);

            // Remove current Items and continue looping
            subset.RemoveAt(subset.Count - 1);
        }
    }
    private void Scan(int index, int count, List<Item> subset, Action<Item[]> callback)
    {
        // No more Items to add, and current subset is guranteed to be valid
        if (count == 0)
        {
            // Callback with current subset
            callback(subset.ToArray());
            return;
        }

        // Save the smallest remaining sum
        int start_index = m_item_count - count;

        // But remember the last index that satisfies the condition
        int last_index = start_index;

        while (start_index > index)
        {
            start_index--;
        }

        // Find the first number in the sorted items that is the largest number we've just found
        // (in case of duplicates)
        while ((start_index > index) && (Items[start_index] == Items[start_index - 1]))
        {
            start_index--;
        }

        // [start_index .. last_index] is the full range we must check in recursion
        for (int i = start_index; i <= last_index; i++)
        {
            // Add current Item to the subset
            Item item = this.Items[i];
            subset.Add(item);

            // Recurse through the sub-problem to the right
            this.Scan(i + 1, count - 1, subset, callback);

            // Remove current Items and continue looping
            subset.RemoveAt(subset.Count - 1);
        }
    }

    // count subsets
    private long m_count = 0L;
    public long Count(int count, long sum)
    {
        m_count = 0L;

        if (count > m_item_count)
            return m_count;

        if ((count > 0) && (sum > 0))
        {
            this.Scan(0, count, sum, new List<Item>());
        }
        else // 0 means any value
        {
            if ((count > 0) && (sum == 0))
            {
                this.Scan(0, count, new List<Item>());
            }
            else if ((count == 0) && (sum > 0))
            {
                for (int i = 0; i < m_item_count; i++)
                {
                    this.Scan(0, i + 1, new List<Item>());
                }
            }
            else if ((count == 0) && (sum == 0))
            {
                for (int i = 0; i < m_item_count; i++)
                {
                    this.Scan(0, i + 1, new List<Item>());
                }
            }
        }

        return m_count;
    }
    private void Scan(int index, int count, long sum, List<Item> subset)
    {
        // No more Items to add, and current subset is guranteed to be valid
        if (count == 0)
        {
            // Callback with current subset
            m_count++;
            return;
        }

        // Save the smallest remaining sum
        int start_index = m_item_count - count;
        long tail_sum = m_tail_sums[start_index];

        // Smallest possible sum is greater than target sum, so a valid subset cannot be found
        if (tail_sum > sum)
        {
            return;
        }

        // Find largest number that satisfies the condition that a valid subset can be found
        tail_sum -= this.Items[start_index].Value;
        // And remember the last index that satisfies the condition
        int last_index = start_index;
        while ((start_index > index) && (tail_sum + this.Items[start_index - 1].Value <= sum))
        {
            start_index--;
        }

        // Find the first number in the sorted items that is the largest number we've just found
        // (in case of duplicates)
        while ((start_index > index) && (Items[start_index] == Items[start_index - 1]))
        {
            start_index--;
        }

        // [start_index .. last_index] is the full range we must check in recursion
        for (int i = start_index; i <= last_index; i++)
        {
            // Find the largest possible sum, which is the target sum of the first k items
            // starting at current start_index
            long max_sum = m_tail_sums[i] - m_tail_sums[i + count];

            // The largest possible sum is less than the target sum, so a valid subset cannot be found
            if (max_sum < sum)
            {
                return;
            }

            // Add current item to the subset
            Item item = this.Items[i];
            subset.Add(item);

            // Recurse through the sub-problem to the right
            this.Scan(i + 1, count - 1, sum - item.Value, subset);

            // Remove current Items and continue looping
            subset.RemoveAt(subset.Count - 1);
        }
    }
    private void Scan(int index, int count, List<Item> subset)
    {
        // No more Items to add, and current subset is guranteed to be valid
        if (count == 0)
        {
            // Callback with current subset
            m_count++;
            return;
        }

        // Save the smallest remaining sum
        int start_index = m_item_count - count;

        // But remember the last index that satisfies the condition
        int last_index = start_index;

        while (start_index > index)
        {
            start_index--;
        }

        // Find the first number in the sorted items that is the largest number we've just found
        // (in case of duplicates)
        while ((start_index > index) && (Items[start_index] == Items[start_index - 1]))
        {
            start_index--;
        }

        // [start_index .. last_index] is the full range we must check in recursion
        for (int i = start_index; i <= last_index; i++)
        {
            // Add current Item to the subset
            Item item = this.Items[i];
            subset.Add(item);

            // Recurse through the sub-problem to the right
            this.Scan(i + 1, count - 1, subset);

            // Remove current Items and continue looping
            subset.RemoveAt(subset.Count - 1);
        }
    }
}
public class Test
{
    private static long m_count;
    private static void OnFound(Subsets.Item[] subset)
    {
        m_count++;

        //return; // Skip printing in benchmarking
        foreach (Subsets.Item item in subset)
        {
            Console.Write(item.ToString() + "\t");
        }
        Console.WriteLine();
    }
    private static void Main(string[] args)
    {
        long[] word_lengths = new long[]
        {
            3, 4, 6, 6,
            5, 3, 2, 7,
            6, 6,
            3, 3, 5,
            4, 4, 5, 6,
            5, 5, 8,
            3, 5, 5, 5, 3, 7, 5, 3, 7
        };
        Subsets subsets = new Subsets(word_lengths);

        int count = 7;
        long sum = 29;

        Stopwatch stopwatch = Stopwatch.StartNew();

        m_count = 0L;
        subsets.Find(count, sum, OnFound);
        stopwatch.Stop();

        Console.WriteLine("Subsets found: " + m_count);
        Console.WriteLine(stopwatch.Elapsed);
        Console.ReadKey();
    }
}

// Sam Meldrum on 17-Sep-2008
// https://stackoverflow.com/questions/83547/algorithm-to-find-which-numbers-from-a-list-of-size-n-sum-to-another-number
public class SubsetSum
{
    private static List<List<long>> subsets;
    public static List<List<long>> FindAll(List<long> numbers, long sum)
    {
        subsets = new List<List<long>>();
        RecursiveFind(sum, 0L, new List<long>(), numbers, 0);
        return subsets;
    }
    private static void RecursiveFind(long sum, long current_sum, List<long> included, List<long> not_included, int start)
    {
        for (int i = start; i < not_included.Count; i++)
        {
            long next = not_included[i];
            if (current_sum + next == sum)
            {
                List<long> result = new List<long>(included);
                result.Add(next);
                subsets.Add(result);
            }
            else if (current_sum + next < sum)
            {
                List<long> next_included = new List<long>(included);
                next_included.Add(next);
                List<long> next_not_included = new List<long>(not_included);
                next_not_included.Remove(next);
                RecursiveFind(sum, current_sum + next, next_included, next_not_included, start++);
            }
        }
    }
    public static List<List<long>> FindAny(List<long> numbers, long sum)
    {
        subsets = new List<List<long>>();
        RecursiveFind(sum, 0L, new List<long>(), numbers, 0);
        return subsets;
    }
}
public class Program
{
    static void Main(string[] args)
    {
        List<long> numbers = new List<long> { 12, 3, 89, 12, 0, -7 };
        long sum = 229;

        List<List<long>> results = SubsetSum.FindAll(numbers, sum);
        foreach (List<long> result in results)
        {
            foreach (int value in result)
            {
                Console.Write("{0}\t", value);
            }
            Console.WriteLine();
        }

        Console.ReadLine();
    }
    //public static string GetInterestingSums(List<long> numbers, long sum)
    //{
    //    List<List<long>> subsets = SubsetSum.FindAll(numbers, sum);
    //    StringBuilder str = new StringBuilder();
    //    foreach (List<long> subset in subsets)
    //    {
    //        foreach (int number in subset)
    //        {
    //            str.Append(number + "\t");
    //        }
    //        str.AppendLine();
    //    }
    //    return str.ToString();
    //}
}
