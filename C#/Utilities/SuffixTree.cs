using System;
using System.Text;
using System.Collections.Generic;
// Copies all substring half O(n^2) space. TOO BAD
public class SuffixTree
{
    private class Node
    {
        public string substring = null;              // a substring of the input string
        public List<int> children = new List<int>(); // vector of child nodes
        public Node()
        {
            substring = "";
        }
        public Node(string substring, params int[] children)
        {
            this.substring = substring;
            this.children.AddRange(children);
        }
    }

    readonly List<Node> nodes = new List<Node>();
    public SuffixTree(string text)
    {
        if (nodes != null)
        {
            nodes.Add(new Node());
            for (int i = 0; i < text.Length; i++)
            {
                AddSuffix(text.Substring(i));
            }
        }
    }
    private void AddSuffix(string suffix)
    {
        int n = 0;
        int i = 0;
        while (i < suffix.Length)
        {
            char b = suffix[i];
            int x2 = 0;
            int n2;
            while (true)
            {
                List<int> children = nodes[n].children;
                if (x2 == children.Count)
                {
                    // no matching child, remainder of suf becomes new node
                    n2 = nodes.Count;
                    nodes.Add(new Node(suffix.Substring(i)));
                    nodes[n].children.Add(n2);
                    return;
                }
                n2 = children[x2];
                if (nodes[n2].substring[0] == b)
                {
                    break;
                }
                x2++;
            }

            // find prefix of remaining suffix in common with child
            string sub2 = nodes[n2].substring;
            int j = 0;
            while (j < sub2.Length)
            {
                if (suffix[i + j] != sub2[j])
                {
                    // split n2
                    var n3 = n2;
                    // new node for the part in common
                    n2 = nodes.Count;
                    nodes.Add(new Node(sub2.Substring(0, j), n3));
                    nodes[n3].substring = sub2.Substring(j); // old node loses the part in common
                    nodes[n].children[x2] = n2;
                    break; // continue down the tree
                }
                j++;
            }
            i += j; // advance past part in common
            n = n2; // continue down the tree
        }
    }
    private StringBuilder m_str = new StringBuilder();
    private void SubVisualize(int index, string prefix)
    {
        List<int> children = nodes[index].children;
        if (children.Count == 0)
        {
            m_str.AppendLine(String.Format("- {0}", nodes[index].substring));
            return;
        }
        m_str.AppendLine(String.Format("+ {0}", nodes[index].substring));

        List<int>.Enumerator it = children.GetEnumerator();
        if (it.MoveNext())
        {
            do
            {
                var cit = it;
                if (!cit.MoveNext()) break;

                m_str.Append(String.Format("{0}+-", prefix));

                SubVisualize(it.Current, prefix + "| ");

            } while (it.MoveNext());
        }

        m_str.Append(String.Format("{0}+-", prefix));

        SubVisualize(children[children.Count - 1], prefix + "  ");
    }
    public string Visualize()
    {
        if (nodes.Count == 0)
        {
            return "";
        }

        SubVisualize(0, "");

        return m_str.ToString();
    }
}
