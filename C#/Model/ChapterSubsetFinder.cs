using System;
using System.Text;
using System.Collections.Generic;
using Model;

public class ChapterSubsetFinder
{
    private List<Chapter> m_items = null;
    private List<List<Chapter>> m_subsets = null;
    public ChapterSubsetFinder(List<Chapter> Chapters)
    {
        m_items = new List<Chapter>(Chapters);
    }

    public List<List<Chapter>> Find(int number_of_chapters, int number_of_verses)
    {
        m_subsets = new List<List<Chapter>>();

        long[] chapter_verse_counts = new long[m_items.Count];
        for (int i = 0; i < m_items.Count; i++)
        {
            chapter_verse_counts[i] = m_items[i].Verses.Count;
        }

        Subsets subsets = new Subsets(chapter_verse_counts);
        subsets.Find(number_of_chapters, number_of_verses, OnFound);

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

        List<Chapter> subset = new List<Chapter>();
        foreach (Subsets.Item item in yyy)
        {
            subset.Add(m_items[item.Index]);
        }
        m_subsets.Add(subset);
    }
}
