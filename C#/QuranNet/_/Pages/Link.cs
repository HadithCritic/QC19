using System;
using System.Collections.Generic;

namespace QuranNet
{
    public enum LinkCompareOrder { Ascending, Descending }
    public enum LinkCompareBy { Id, Source, Target, Label, Range, NumberInRange, VerseLink }
    public class Link : IComparable<Link>
    {
        private static LinkCompareBy s_compare_by = LinkCompareBy.Id;
        public static LinkCompareBy CompareBy
        {
            get { return s_compare_by; }
            set { s_compare_by = value; }
        }
        private static LinkCompareOrder s_compare_order = LinkCompareOrder.Ascending;
        public static LinkCompareOrder CompareOrder
        {
            get { return s_compare_order; }
            set { s_compare_order = value; }
        }
        public int CompareTo(Link obj)
        {
            if (this == obj) return 0;

            if (s_compare_order == LinkCompareOrder.Ascending)
            {
                switch (s_compare_by)
                {
                    case LinkCompareBy.Id:
                        {
                            return this.Id.CompareTo(obj.Id);
                        }
                    case LinkCompareBy.Source:
                        {
                            if (this.Source.CompareTo(obj.Source) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Source.CompareTo(obj.Source);
                        }
                    case LinkCompareBy.Target:
                        {
                            if (this.Target.CompareTo(obj.Target) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Target.CompareTo(obj.Target);
                        }
                    case LinkCompareBy.Label:
                        {
                            if (this.Label.CompareTo(obj.Label) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Label.CompareTo(obj.Label);
                        }
                    case LinkCompareBy.Range:
                        {
                            if (this.Range.CompareTo(obj.Range) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Range.CompareTo(obj.Range);
                        }
                    case LinkCompareBy.NumberInRange:
                        {
                            if (this.NumberInRange.CompareTo(obj.NumberInRange) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.NumberInRange.CompareTo(obj.NumberInRange);
                        }
                    case LinkCompareBy.VerseLink:
                        {
                            if (this.VerseLink.CompareTo(obj.VerseLink) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.VerseLink.CompareTo(obj.VerseLink);
                        }
                    default:
                        {
                            return this.Id.CompareTo(obj.Id);
                        }
                }
            }
            else
            {
                switch (s_compare_by)
                {
                    case LinkCompareBy.Id:
                        {
                            return obj.Id.CompareTo(this.Id);
                        }
                    case LinkCompareBy.Source:
                        {
                            if (obj.Source.CompareTo(this.Source) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Source.CompareTo(this.Source);
                        }
                    case LinkCompareBy.Target:
                        {
                            if (obj.Target.CompareTo(this.Target) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Target.CompareTo(this.Target);
                        }
                    case LinkCompareBy.Label:
                        {
                            if (obj.Label.CompareTo(this.Label) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Label.CompareTo(this.Label);
                        }
                    case LinkCompareBy.Range:
                        {
                            if (obj.Range.CompareTo(this.Range) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Range.CompareTo(this.Range);
                        }
                    case LinkCompareBy.NumberInRange:
                        {
                            if (obj.NumberInRange.CompareTo(this.NumberInRange) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.NumberInRange.CompareTo(this.NumberInRange);
                        }
                    case LinkCompareBy.VerseLink:
                        {
                            if (obj.VerseLink.CompareTo(this.VerseLink) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.VerseLink.CompareTo(this.VerseLink);
                        }
                    default:
                        {
                            return obj.Id.CompareTo(this.Id);
                        }
                }
            }
        }

        public long Id;
        public Node Source;
        public Node Target;
        public string Label;
        public int Range;
        public int NumberInRange;
        public bool VerseLink;
    }

    // Usage: m_links.Sort(new LinkComparer());
    public class LinkRangeComparer : IComparer<Link>
    {
        public int Compare(Link x, Link y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (Link.CompareOrder == LinkCompareOrder.Descending)
            {
                int result = y.Range.CompareTo(x.Range);
                return result != 0 ? result : x.NumberInRange.CompareTo(y.NumberInRange);
            }
            else
            {
                int result = x.Range.CompareTo(y.Range);
                return result != 0 ? result : y.NumberInRange.CompareTo(x.NumberInRange);
            }
        }
    }
}
