// Dr Haifeng Xu, Yangzhou University, China
// https://www.researchgate.net/profile/Haifeng_Xu3

using System;
using System.Text;

public static class DotBuilder
{
    public static string Build(int[] numbers)
    {
        return Build(numbers, null, false);
    }
    public static string Build(int[] numbers, string[] labels)
    {
        return Build(numbers, labels, false);
    }
    public static string Build(int[] numbers, bool zero_index)
    {
        return Build(numbers, null, zero_index);
    }
    public static string Build(int[] numbers, string[] labels, bool zero_index)
    {
        int nodes = 0;
        int solid_links = 0;
        int dashed_links = 0;

        StringBuilder str = new StringBuilder();

        str.AppendLine("digraph QuranGrid");
        str.AppendLine("{");

        int size = numbers.Length;
        for (int i = 0; i < size; i++)
        {
            int chapter1 = i + 1;
            int verses1 = numbers[i];

            int j = -1;
            if (zero_index) // Dr Haifeng Xu, Yangzhou University, China
            {
                j = (verses1 % size);
                if (j == verses1)
                {
                    str.AppendLine("\tedge [style=solid, color=black];");
                    solid_links++;
                }
                else
                {
                    str.AppendLine("\tedge [style=dashed, color=red];");
                    dashed_links++;
                }
            }
            else // natural connection method
            {
                j = ((verses1 - 1) % size);
                if (j == (verses1 - 1))
                {
                    str.AppendLine("\tedge [style=solid, color=black];");
                    solid_links++;
                }
                else
                {
                    str.AppendLine("\tedge [style=dashed, color=red];");
                    dashed_links++;
                }
            }

            if ((j >= 0) && (j < size))
            {
                int chapter2 = j + 1;
                int verses2 = numbers[j];

                if ((labels != null) && (labels.Length == size))
                {
                    string MathsVersesCCountLabel = labels[i];
                    string MathsVersesVCountLabel = labels[j];
                    str.AppendLine("\t" + i + " [label=\"" + MathsVersesCCountLabel + "\n" + (chapter1 + "." + verses1) + "\"" + "];");
                    str.AppendLine("\t" + j + " [label=\"" + MathsVersesVCountLabel + "\n" + (chapter2 + "." + verses2) + "\"" + "];");
                    str.AppendLine("\t" + i + " -> " + j + ";");
                }
                else
                {
                    str.AppendLine("\t" + (chapter1 + "." + verses1) + " -> " + (chapter2 + "." + verses2) + ";");
                }
                nodes++;
            }
        }

        str.AppendLine();
        str.AppendLine("\tfontcolor=blue fontsize=29 label=\"Nodes = " + nodes + "   Solid Links = " + solid_links + "   Dashed Links = " + dashed_links + "\"");
        str.AppendLine("}");
        return str.ToString();
    }
    public static string Sample()
    {
        StringBuilder str = new StringBuilder();

        str.AppendLine("digraph SAMPLE {");
        str.AppendLine("\ta [label=<&alpha;>]");
        str.AppendLine("\tb [label=<&beta;>]");
        str.AppendLine("\tc [label=<&gamma;>]");
        str.AppendLine("\ti [label=<&infin;>]");
        str.AppendLine("\tx [label=<&radic;>, fontname=\"bold\"]");
        str.AppendLine("\ty [label=<&int;>]");
        str.AppendLine("\tz [label=<&hearts;>, fontcolor=red, fontsize=24]");
        str.AppendLine("\t\ta -> b -> c -> b;");
        str.AppendLine("\t\ta -> {x y};");
        str.AppendLine("\t\tb [shape=box];");
        str.AppendLine("\t\td [label=\"&pi;\ne\n&phi;\", color=palegreen, fontsize=14, fontname=\"Palatino-Italic\", fontcolor=darkgreen, style=filled];");
        str.AppendLine("\t\ta -> z [label=\"MathML\", weight=100];");
        str.AppendLine("\t\tx -> z [label=\"multi-line\nlabel\"];");
        str.AppendLine("\t\tedge [style=dashed, color=red];");
        str.AppendLine("\t\tb -> x;");
        str.AppendLine("\t\t{rank=same; a b c}");
        str.AppendLine("\t\td -> {d i z};");
        str.AppendLine("\t}");
        str.AppendLine("");
        str.AppendLine("\tgraph GRAPH {");
        str.AppendLine("\t\ta -- b -- c;");
        str.AppendLine("\t\ta -- {x y};");
        str.AppendLine("\t\tx -- c [w=10.0];");
        str.AppendLine("\t\tx -- y [w=5.0, len=3];");
        str.AppendLine("\t}");

        return str.ToString();
    }
}
