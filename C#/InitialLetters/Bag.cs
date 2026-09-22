using System;
using System.Collections.Generic;
using System.Text;

public class Bag : IComparable
{
    static private string SubtractStrings(string s1, string s2)
    {
        Bag m = new Bag(s1);

        Bag s = new Bag(s2);
        Bag diff = m.Subtract(s);
        if (diff == null) return null;
        return diff.AsString();
    }

    private string contents;
    public Bag(string s)
    {
        Char[] chars = s.ToLower().ToCharArray();
        Array.Sort(chars);
        Char[] letters = Array.FindAll<char>(chars, Char.IsLetter);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Insert(0, letters);
        contents = sb.ToString();
    }
    public bool Empty()
    {
        return (contents.Length == 0);
    }
    public Bag Subtract(Bag bag)
    {
        string m = contents;
        string s = bag.contents;
        string difference = "";

        while (true)
        {
            if (s.Length == 0) return new Bag(difference + m);
            if (m.Length == 0) return null;
            {
                char s0 = s[0];
                char m0 = m[0];
                if (m0 > s0) return null;
                if (m0 < s0)
                {
                    m = m.Substring(1);
                    difference += m0;
                    continue;
                }
                System.Diagnostics.Trace.Assert(m0 == s0, "Internal error!");
                m = m.Substring(1);
                s = s.Substring(1);
            }
        }
    }
    public string AsString()
    {
        return contents;
    }
    public override int GetHashCode()
    {
        return contents.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        return contents.Equals(((Bag)obj).contents);
    }

    #region IComparable Members
    public int CompareTo(object obj)
    {
        Bag other = (Bag)obj;
        if (this.contents.Length > other.contents.Length)
            return -1;
        else if (this.contents.Length < other.contents.Length)
            return 1;
        else
            return 0;
    }
    #endregion
}
