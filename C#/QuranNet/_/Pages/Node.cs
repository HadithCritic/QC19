using System;

namespace QuranNet
{
    public enum NodeCompareOrder { Ascending, Descending }
    public enum NodeCompareBy { Id, X, Y, Z, Text, Meaning, Root, Value }
    public class Node : IComparable<Node>
    {
        private static NodeCompareBy s_compare_by = NodeCompareBy.Id;
        public static NodeCompareBy CompareBy
        {
            get { return s_compare_by; }
            set { s_compare_by = value; }
        }
        private static NodeCompareOrder s_compare_order = NodeCompareOrder.Ascending;
        public static NodeCompareOrder CompareOrder
        {
            get { return s_compare_order; }
            set { s_compare_order = value; }
        }
        public int CompareTo(Node obj)
        {
            if (this == obj) return 0;

            if (s_compare_order == NodeCompareOrder.Ascending)
            {
                switch (s_compare_by)
                {
                    case NodeCompareBy.Id:
                        {
                            return this.Id.CompareTo(obj.Id);
                        }
                    case NodeCompareBy.X:
                        {
                            if (this.X.CompareTo(obj.X) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.X.CompareTo(obj.X);
                        }
                    case NodeCompareBy.Y:
                        {
                            if (this.Y.CompareTo(obj.Y) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Y.CompareTo(obj.Y);
                        }
                    case NodeCompareBy.Z:
                        {
                            if (this.Z.CompareTo(obj.Z) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Z.CompareTo(obj.Z);
                        }
                    case NodeCompareBy.Text:
                        {
                            if (this.Text.CompareTo(obj.Text) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Text.CompareTo(obj.Text);
                        }
                    case NodeCompareBy.Meaning:
                        {
                            if (this.Meaning.CompareTo(obj.Meaning) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Meaning.CompareTo(obj.Meaning);
                        }
                    case NodeCompareBy.Root:
                        {
                            if (this.Root.CompareTo(obj.Root) == 0)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Root.CompareTo(obj.Root);
                        }
                    case NodeCompareBy.Value:
                        {
                            if (this.Value.CompareTo(obj.Value) == 0L)
                            {
                                return this.Id.CompareTo(obj.Id);
                            }
                            return this.Value.CompareTo(obj.Value);
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
                    case NodeCompareBy.Id:
                        {
                            return obj.Id.CompareTo(this.Id);
                        }
                    case NodeCompareBy.X:
                        {
                            if (obj.X.CompareTo(this.X) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.X.CompareTo(this.X);
                        }
                    case NodeCompareBy.Y:
                        {
                            if (obj.Y.CompareTo(this.Y) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Y.CompareTo(this.Y);
                        }
                    case NodeCompareBy.Z:
                        {
                            if (obj.Z.CompareTo(this.Z) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Z.CompareTo(this.Z);
                        }
                    case NodeCompareBy.Text:
                        {
                            if (obj.Text.CompareTo(this.Text) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Text.CompareTo(this.Text);
                        }
                    case NodeCompareBy.Meaning:
                        {
                            if (obj.Meaning.CompareTo(this.Meaning) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Meaning.CompareTo(this.Meaning);
                        }
                    case NodeCompareBy.Root:
                        {
                            if (obj.Root.CompareTo(this.Root) == 0)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Root.CompareTo(this.Root);
                        }
                    case NodeCompareBy.Value:
                        {
                            if (obj.Value.CompareTo(this.Value) == 0L)
                            {
                                return obj.Id.CompareTo(this.Id);
                            }
                            return obj.Value.CompareTo(this.Value);
                        }
                    default:
                        {
                            return obj.Id.CompareTo(this.Id);
                        }
                }
            }
        }

        public long Id;
        public int X;
        public int Y;
        public int Z;
        public string Text;
        public string SimplifiedText; // cache for speed
        public string Meaning;
        public string Root;
        public long Value;

        public int InPorts;
        public int OutPorts;
    }
}
