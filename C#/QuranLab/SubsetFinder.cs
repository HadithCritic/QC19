// Yorye Nathan on 03-May-2015
// https://stackoverflow.com/questions/30006497/find-all-k-m_size-subsets-with-sum-s-of-an-n-m_size-bag-of-duplicate-unsorted-positi/30012781#30012781
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Model;

public class SubsetFinder
{
    private readonly int[] m_tail_sums = null;

    private Client m_client = null;
    private Chapter[] m_chapters = null;
    public SubsetFinder(Client client, NumberQuery query)
    {
        m_client = client;
        m_query = query;

        if (m_client != null)
        {
            if (m_client.Book != null)
            {
                m_chapters = m_client.Book.Chapters.ToArray();
                int chapter_count = m_chapters.Length;

                // Sort Chapters descendingly by SortedNumber
                Array.Sort(m_chapters, (a, b) => b.SortedNumber.CompareTo(a.SortedNumber));

                // Save tail-sums to allow immediate access by index
                m_tail_sums = new int[chapter_count + 1];
                int sum = 0;
                for (int i = chapter_count - 1; i >= 0; i--)
                {
                    sum += m_chapters[i].SortedNumber;
                    m_tail_sums[i] = sum;
                }
            }
        }
    }

    private long m_loop = 0L;
    private const long BATCH_SIZE = 10000L;

    private long ms = 0L;
    /// <summary>
    /// Count all subsets where the sum of numbers in a subset equals the target sum.
    /// </summary>
    /// <param name="m_size">number of items in subsets to be found</param>
    /// <param name="sum">target sum</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public long Count(int size, long sum)
    {
        m_loop = 0L;
        ms = 0L;

        if (size > m_chapters.Length)
            return ms;

        if (m_unique_match_chapters == null)
        {
            m_unique_match_chapters = new List<Chapter>();
        }
        else
        {
            m_unique_match_chapters.Clear();
        }

        if (size > 0)
        {
            if (sum == -1)
            {
                this.Scan(0, size, new List<Chapter>(), null);
            }
            else
            {
                this.Scan(0, size, sum, new List<Chapter>(), null);
            }
        }

        return ms;
    }

    /// <summary>
    /// Find all subsets where the sum of numbers in a subset equals the target sum.
    /// </summary>
    /// <param name="sum">target sum</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public void Find(long sum, Action<Chapter[]> callback)
    {
        m_loop = 0L;

        for (int count = 1; count <= m_chapters.Length; count++)
        {
            this.Find(count, sum, callback);
        }
    }
    /// <summary>
    /// Find all subsets with specified count where the sum of numbers in a subset equals the target sum.
    /// </summary>
    /// <param name="m_size">number of items in subsets to be found</param>
    /// <param name="sum">target sum</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public void Find(int size, long sum, Action<Chapter[]> callback)
    {
        m_loop = 0L;

        if (size > m_chapters.Length)
            return;

        if (m_unique_match_chapters == null)
        {
            m_unique_match_chapters = new List<Chapter>();
        }
        else
        {
            m_unique_match_chapters.Clear();
        }

        if (size > 0)
        {
            if (sum == -1)
            {
                this.Scan(0, size, new List<Chapter>(), callback);
            }
            else
            {
                this.Scan(0, size, sum, new List<Chapter>(), callback);
            }
        }
    }
    /// <summary>
    /// Find all subsets with specified count regardless of sum.
    /// </summary>
    /// <param name="m_size">number of items in subsets to be found</param>
    /// <param name="subset">subset to evaluate</param>
    /// <param name="callback">method to call when a new subset is found</param>
    public void Find(int size, Action<Chapter[]> callback)
    {
        m_loop = 0L;

        if (size > m_chapters.Length)
            return;

        if (m_unique_match_chapters == null)
        {
            m_unique_match_chapters = new List<Chapter>();
        }
        else
        {
            m_unique_match_chapters.Clear();
        }
        this.Scan(0, size, new List<Chapter>(), callback);
    }

    // find/count subsets
    private void Scan(int index, int size, List<Chapter> chapters, Action<Chapter[]> callback)
    {
        m_loop++;

        // No more chapters to add.
        // Current subset is guranteed to be valid
        if (size == 0)
        {
            // Callback with current subset
            Chapter[] subset = chapters.ToArray();
            if (IsMatch(subset))
            {
                if (!MainForm.Running)
                {
                    return;
                }

                if (callback == null) // count only
                {
                    ms++;
                    if ((m_loop % BATCH_SIZE) == 0)
                    {
                        //MainForm.Progress = m_loop;
                        Application.DoEvents();
                    }
                }
                else // call back MainForm
                {
                    callback(subset);

                    //MainForm.Progress = m_loop;
                    Application.DoEvents();
                }
            }
            return;
        }

        // Find largest number that satisfies the condition that a valid subset can be found
        int start_index = m_chapters.Length - size;
        //  And remember the last index that satisfies the condition
        int last_index = start_index;
        while (start_index > index)
        {
            start_index--;
        }

        //// Find the first number in the sorted chapters that is the largest number we've just found
        //// (in case of duplicates)
        //while ((start_index > index) && (Chapters[start_index] == Chapters[start_index - 1]))
        //{
        //    start_index--;
        //}

        // [start_index .. last_index] is the full range we must check in recursion
        for (int i = start_index; i <= last_index; i++)
        {
            // Add current chapter to the subset
            Chapter chapter = m_chapters[i];
            chapters.Add(chapter);

            // Recurse through the sub-problem to the right
            this.Scan(i + 1, size - 1, chapters, callback);

            // Remove current chapter and continue looping
            chapters.RemoveAt(chapters.Count - 1);

            if (!MainForm.Running)
            {
                return;
            }
        }
    }
    private void Scan(int index, int size, long sum, List<Chapter> chapters, Action<Chapter[]> callback)
    {
        if (chapters != null)
        {
            m_loop++;

            // No more chapters to add.
            // Current subset is guranteed to be valid
            if (size == 0)
            {
                // Callback with current subset
                Chapter[] subset = chapters.ToArray();
                if (IsMatch(subset))
                {
                    if (callback == null) // count only
                    {
                        ms++;
                        if ((m_loop % BATCH_SIZE) == 0)
                        {
                            //MainForm.Progress = m_loop;
                            Application.DoEvents();
                        }
                    }
                    else // call back MainForm
                    {
                        callback(subset);

                        //MainForm.Progress = m_loop;
                        Application.DoEvents();
                    }
                }
                return;
            }

            // Save the smallest remaining sum
            int start_index = m_chapters.Length - size;
            int tail_sum = m_tail_sums[start_index];

            // Smallest possible sum is greater than target sum,
            // so a valid subset cannot be found
            if (tail_sum > sum)
            {
                return;
            }

            // Find largest number that satisfies the condition that a valid subset can be found
            tail_sum -= m_chapters[start_index].SortedNumber;
            // And remember the last index that satisfies the condition
            int last_index = start_index;
            while ((start_index > index) && (tail_sum + m_chapters[start_index - 1].SortedNumber <= sum))
            {
                start_index--;
            }

            //// Find the first number in the sorted chapters that is the largest number we've just found
            //// (in case of duplicates)
            //while ((start_index > index) && (Chapters[start_index] == Chapters[start_index - 1]))
            //{
            //    start_index--;
            //}

            // [start_index .. last_index] is the full range we must check in recursion
            for (int i = start_index; i <= last_index; i++)
            {
                // Find the largest possible sum, which is the sum of the first k chapters
                // starting at current start_index
                int max_sum = m_tail_sums[i] - m_tail_sums[i + size];

                // The largest possible sum is less than the sum, so a valid subset cannot be found
                if (max_sum < sum)
                {
                    return;
                }

                // Add current chapter to the subset
                Chapter chapter = m_chapters[i];
                if (chapter != null)
                {
                    chapters.Add(chapter);

                    // Recurse through the sub-problem to the right
                    this.Scan(i + 1, size - 1, sum - chapter.SortedNumber, chapters, callback);

                    // Remove current chapter and continue looping
                    chapters.RemoveAt(chapters.Count - 1);
                }

                if (!MainForm.Running)
                {
                    return;
                }
            }
        }
    }

    private readonly NumberQuery m_query;
    private bool IsMatch(Chapter[] chapters)
    {
        if (chapters == null) return false;

        int sum = 0;
        foreach (Chapter chapter in chapters)
        {
            sum += chapter.SortedNumber;
        }
        if ((m_query.ChapterSumNumberType == NumberType.None) || (m_query.ChapterSumNumberType == NumberType.Natural))
        {
            if (m_query.ChapterSum > -1)
            {
                if (sum != m_query.ChapterSum)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.ChapterSumNumberType))
            {
                return false;
            }
        }

        sum = 0;
        foreach (Chapter chapter in chapters)
        {
            sum += chapter.Verses.Count;
        }
        if ((m_query.VersesNumberType == NumberType.None) || (m_query.VersesNumberType == NumberType.Natural))
        {
            if (m_query.Verses > -1)
            {
                if (sum != m_query.Verses)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.VersesNumberType))
            {
                return false;
            }
        }

        sum = 0;
        foreach (Chapter chapter in chapters)
        {
            sum += chapter.Words.Count;
        }
        if ((m_query.WordsNumberType == NumberType.None) || (m_query.WordsNumberType == NumberType.Natural))
        {
            if (m_query.Words > -1)
            {
                if (sum != m_query.Words)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.WordsNumberType))
            {
                return false;
            }
        }

        sum = 0;
        foreach (Chapter chapter in chapters)
        {
            sum += chapter.Letters.Count;
        }
        if ((m_query.LettersNumberType == NumberType.None) || (m_query.LettersNumberType == NumberType.Natural))
        {
            if (m_query.Letters > -1)
            {
                if (sum != m_query.Letters)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.LettersNumberType))
            {
                return false;
            }
        }

        long value = 0L;
        foreach (Chapter chapter in chapters)
        {
            value += chapter.Value;
        }
        if ((m_query.ValueNumberType == NumberType.None) || (m_query.ValueNumberType == NumberType.Natural))
        {
            if (m_query.Value > -1)
            {
                if (value != m_query.Value)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(value, m_query.ValueNumberType))
            {
                return false;
            }
        }

        sum = 0;
        foreach (Chapter chapter in chapters)
        {
            sum += (chapter.SortedNumber + chapter.Verses.Count);
        }
        if ((m_query.CPlusVSumNumberType == NumberType.None) || (m_query.CPlusVSumNumberType == NumberType.Natural))
        {
            if (m_query.CPlusVSum > -1)
            {
                if (sum != m_query.CPlusVSum)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.CPlusVSumNumberType))
            {
                return false;
            }
        }

        sum = 0;
        foreach (Chapter chapter in chapters)
        {
            if (MainForm.AbsCMinusVSum)
            {
                sum += Math.Abs((chapter.SortedNumber - chapter.Verses.Count)); // Absolute
            }
            else
            {
                sum += (chapter.SortedNumber - chapter.Verses.Count);
            }
        }
        if ((m_query.CMinusVSumNumberType == NumberType.None) || (m_query.CMinusVSumNumberType == NumberType.Natural))
        {
            if (m_query.CMinusVSum > -1)
            {
                if (sum != m_query.CMinusVSum)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.CMinusVSumNumberType))
            {
                return false;
            }
        }

        sum = 0;
        foreach (Chapter chapter in chapters)
        {
            sum += (chapter.SortedNumber * chapter.Verses.Count);
        }
        if ((m_query.CTimesVSumNumberType == NumberType.None) || (m_query.CTimesVSumNumberType == NumberType.Natural))
        {
            if (m_query.CTimesVSum > -1)
            {
                if (sum != m_query.CTimesVSum)
                {
                    return false;
                }
            }
        }
        else
        {
            if (!Numbers.IsNumberType(sum, m_query.CTimesVSumNumberType))
            {
                return false;
            }
        }

        sum = 0;
        if (chapters.Length == 2)
        {
            if (MainForm.AbsC2MinusC1)
            {
                sum += Math.Abs((chapters[1].SortedNumber - chapters[0].SortedNumber)); // Absolute
            }
            else
            {
                sum += (chapters[0].SortedNumber - chapters[1].SortedNumber);
            }

            if ((m_query.C2MinusC1NumberType == NumberType.None) || (m_query.C2MinusC1NumberType == NumberType.Natural))
            {
                if (m_query.C2MinusC1 > -1)
                {
                    if (sum != m_query.C2MinusC1)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, m_query.C2MinusC1NumberType))
                {
                    return false;
                }
            }
        }

        sum = 0;
        if (chapters.Length == 2)
        {
            if (MainForm.AbsV2MinusV1)
            {
                sum += Math.Abs((chapters[1].Verses.Count - chapters[0].Verses.Count)); // Absolute
            }
            else
            {
                sum += (chapters[0].Verses.Count - chapters[1].Verses.Count);
            }

            if ((m_query.V2MinusV1NumberType == NumberType.None) || (m_query.V2MinusV1NumberType == NumberType.Natural))
            {
                if (m_query.V2MinusV1 > -1)
                {
                    if (sum != m_query.V2MinusV1)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, m_query.V2MinusV1NumberType))
                {
                    return false;
                }
            }
        }

        sum = 0;
        if (chapters.Length == 2)
        {
            if (MainForm.AbsW2MinusW1)
            {
                sum += Math.Abs((chapters[1].Words.Count - chapters[0].Words.Count)); // Absolute
            }
            else
            {
                sum += (chapters[0].Words.Count - chapters[1].Words.Count);
            }

            if ((m_query.W2MinusW1NumberType == NumberType.None) || (m_query.W2MinusW1NumberType == NumberType.Natural))
            {
                if (m_query.W2MinusW1 > -1)
                {
                    if (sum != m_query.W2MinusW1)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, m_query.W2MinusW1NumberType))
                {
                    return false;
                }
            }
        }

        sum = 0;
        if (chapters.Length == 2)
        {
            if (MainForm.AbsL2MinusL1)
            {
                sum += Math.Abs((chapters[1].Letters.Count - chapters[0].Letters.Count)); // Absolute
            }
            else
            {
                sum += (chapters[0].Letters.Count - chapters[1].Letters.Count);
            }

            if ((m_query.L2MinusL1NumberType == NumberType.None) || (m_query.L2MinusL1NumberType == NumberType.Natural))
            {
                if (m_query.L2MinusL1 > -1)
                {
                    if (sum != m_query.L2MinusL1)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, m_query.L2MinusL1NumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        // store unique matching chapters
        foreach (Chapter chapter in chapters)
        {
            if (!m_unique_match_chapters.Contains(chapter))
            {
                m_unique_match_chapters.Add(chapter);
            }
        }
        return true;
    }

    private List<Chapter> m_unique_match_chapters = null;
    public List<Chapter> UniqueMatchChapters
    {
        get { return m_unique_match_chapters; }
    }
}
