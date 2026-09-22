using System;
using System.Text;
using System.Collections.Generic;
using Model;

public class WordSubsetFinder
{
    private List<Word> m_items = null;
    private List<List<Word>> m_subsets = null;
    public WordSubsetFinder(List<Word> words)
    {
        m_items = new List<Word>(words);
    }

    public List<List<Word>> Find(int number_of_words, int number_of_letters)
    {
        m_subsets = new List<List<Word>>();

        long[] word_lengths = new long[m_items.Count];
        for (int i = 0; i < m_items.Count; i++)
        {
            word_lengths[i] = m_items[i].Letters.Count;
        }

        Subsets subsets = new Subsets(word_lengths);
        subsets.Find(number_of_words, number_of_letters, OnFound);

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

        List<Word> subset = new List<Word>();
        foreach (Subsets.Item item in yyy)
        {
            subset.Add(m_items[item.Index]);
        }
        m_subsets.Add(subset);
    }
}
