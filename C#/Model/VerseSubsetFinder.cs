using System;
using System.Text;
using System.Collections.Generic;
using Model;

public class VerseSubsetFinder
{
    private List<Verse> m_items = null;
    private List<List<Verse>> m_subsets = null;
    public VerseSubsetFinder(List<Verse> Verses)
    {
        m_items = new List<Verse>(Verses);
    }

    public List<List<Verse>> Find(int number_of_verses, int number_of_words)
    {
        m_subsets = new List<List<Verse>>();

        long[] verse_word_counts = new long[m_items.Count];
        for (int i = 0; i < m_items.Count; i++)
        {
            verse_word_counts[i] = m_items[i].Words.Count;
        }

        Subsets subsets = new Subsets(verse_word_counts);
        subsets.Find(number_of_verses, number_of_words, OnFound);

        return m_subsets;
    }

    private void OnFound(Subsets.Item[] items)
    {
        // sort items in ascending order
        List<Subsets.Item> xxx = new List<Subsets.Item>(items);
        List<Subsets.Item> yyy = new List<Subsets.Item>();
        while (xxx.Count > 0)
        {
            int min = 0;
            for (int i = 0; i < xxx.Count; i++)
            {
                if (xxx[min].Index > xxx[i].Index)
                {
                    min = i;
                }
            }
            yyy.Add(xxx[min]);
            xxx.Remove(xxx[min]);
        };

        List<Verse> subset = new List<Verse>();
        foreach (Subsets.Item item in yyy)
        {
            subset.Add(m_items[item.Index]);
        }
        m_subsets.Add(subset);
    }
}
