using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

// callback functions to indicate progress.
public delegate void bottom_of_main_loop();
public delegate void done_pruning(uint recursion_level, List<BagAnagrams> pruned);
public delegate void found_anagram(List<string> words);

// each entry is a bag followed by words that can be made from that bag.
public class BagAnagrams : IComparable
{
    public Bag bag;
    public List<string> words;

    public BagAnagrams(Bag bag, List<string> words)
    {
        this.bag = bag;
        this.words = words;
    }

    #region IComparable Members

    public int CompareTo(object obj)
    {
        return this.bag.CompareTo(((BagAnagrams)obj).bag);
    }

    #endregion
}

public class Anagrams
{
    // given a list of words and a list of anagrams, make more
    // anagrams by combining the two.
    private static List<List<string>> Combine(List<string> ws, List<List<string>> ans)
    {
        List<List<string>> rv = new List<List<string>>();
        foreach (List<string> a in ans)
        {
            foreach (string word in ws)
            {
                List<string> bigger_anagram = new List<string>();
                bigger_anagram.InsertRange(0, a);
                bigger_anagram.Add(word);
                rv.Add(bigger_anagram);
            }
        }

        return rv;
    }

    // return a list that is like dictionary, but which contains only those items which can be made from the letters in bag.
    private static List<BagAnagrams> Prune(Bag bag, List<BagAnagrams> dictionary, done_pruning done_pruning_callback, uint recursion_level)
    {
        List<BagAnagrams> rv = new List<BagAnagrams>();
        foreach (BagAnagrams pair in dictionary)
        {
            Bag this_bag = pair.bag;
            if (bag.Subtract(this_bag) != null)
            {
                rv.Add(pair);
            }
        }
        done_pruning_callback(recursion_level, rv);
        return rv;
    }

    public static List<List<string>> anagrams(Bag bag,
        List<BagAnagrams> dictionary,
        uint recursion_level,
        bottom_of_main_loop bottom,
        done_pruning done_pruning_callback,
        found_anagram success_callback)
    {
        List<List<string>> rv = new List<List<string>>();
        List<BagAnagrams> pruned = Prune(bag,
            dictionary,
            done_pruning_callback,
            recursion_level);
        int pruned_initial_size = pruned.Count;
        while (pruned.Count > 0)
        {
            BagAnagrams entry = pruned[0];
            Bag this_bag = entry.bag;
            Bag diff = bag.Subtract(this_bag);
            if (diff != null)
            {
                if (diff.Empty())
                {
                    foreach (string w in entry.words)
                    {
                        List<string> loner = new List<string>();
                        loner.Add(w);
                        rv.Add(loner);
                        if (recursion_level == 0)
                            success_callback(loner);
                    }
                }
                else
                {
                    List<List<string>> from_smaller = anagrams(diff, pruned, recursion_level + 1,
                        bottom,
                        done_pruning_callback,
                        success_callback);
                    List<List<string>> combined = Combine(entry.words, from_smaller);
                    foreach (List<string> an in combined)
                    {
                        rv.Add(an);
                        if (recursion_level == 0)
                            success_callback(an);
                    }
                }
            }
            pruned.RemoveAt(0);
            if (recursion_level == 0)
                bottom();
        }
        return rv;
    }

    // Generate Anagrams from Word Dictionary and a list of letters
    private static List<BagAnagrams> LoadDictionary(string path)
    {
        List<BagAnagrams> result = new List<BagAnagrams>();
        using (StreamReader reader = File.OpenText(path))
        {
            try
            {
                string line = null;
                int linesRead = 0;
                Hashtable stringlists_by_bag = new Hashtable();
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.ToLower();

                    Bag bag = new Bag(line);
                    if (!stringlists_by_bag.ContainsKey(bag))
                    {
                        List<string> l = new List<string>();
                        l.Add(line);
                        stringlists_by_bag.Add(bag, l);
                    }
                    else
                    {
                        List<string> l = (List<string>)stringlists_by_bag[bag];
                        if (!l.Contains(line))
                            l.Add(line);
                    }
                    linesRead++;
                }

                result = new List<BagAnagrams>();
                foreach (DictionaryEntry de in stringlists_by_bag)
                {
                    result.Add(new BagAnagrams((Bag)de.Key, (List<string>)de.Value));
                }
                result.Sort();

                // Sort the list so that the biggest bags come first.
                // This might make more interesting anagrams appear first.
                BagAnagrams[] sort_me = new BagAnagrams[result.Count];
                result.CopyTo(sort_me);
                Array.Sort(sort_me);
                result.Clear();
                result.InsertRange(0, sort_me);
            }
            catch (Exception ex)
            {
                throw new Exception("Dictionary: " + ex.Message + ".\r\n\r\n" + ex.StackTrace);
            }
        }
        return result;
    }
    public static List<string> GenerateAnagrams(string path, string letters)
    {
        List<string> result = new List<string>();

        List<BagAnagrams> dictionary = LoadDictionary(path);
        Bag bag = new Bag(letters);
        Anagrams.anagrams(bag, dictionary, 0,

            // bottom of main loop
            delegate()
            {
                //ProgressBar.PerformStep();
                //Application.DoEvents();
            },

            // done pruning
            delegate(uint recursion_level, List<BagAnagrams> pruned_dictionary)
            {
                if (recursion_level == 0)
                {
                    //ProgressBar.Maximum = pruned_dictionary.Count;
                    //Application.DoEvents();
                }
            },

            // found a top-level anagram
            delegate(List<string> words)
            {
                string display_me = "";
                foreach (string s in words)
                {
                    if (display_me.Length > 0)
                        display_me += " ";
                    display_me += s;
                }

                result.Add(display_me);
            });

        return result;
    }
}
