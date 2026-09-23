using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Analysis;

/// <summary>The word lists of the original's research methods (Features.txt #3, #10).</summary>
public enum WordListKind
{
    /// <summary>The twelve written forms of الله (legacy AllahWords).</summary>
    Allah,

    /// <summary>Fifteen words that contain the letters of الله but are not it (legacy NonAllahWords).</summary>
    NonAllah,

    /// <summary>Every word.</summary>
    All,

    /// <summary>Two neighboring words that are the same, across verse ends (legacy DoubleWords).</summary>
    Double,

    /// <summary>The same word again within a verse, a given number of words later (legacy RepeatedWords).</summary>
    Repeated,
}

/// <summary>A table of results, as the original writes them to its Research folder.</summary>
public sealed record ResearchTable(IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string>> Rows);

/// <summary>How the verses count الله by the original's statistics drawing.</summary>
/// <param name="Allah">Words that are exactly الله.</param>
/// <param name="WithAllah">Other words containing الله (not اللهو or اللهب).</param>
/// <param name="WithLillah">Words containing لله but not الله and none of the look-alikes.</param>
public sealed record AllahSummary(int Allah, int WithAllah, int WithLillah)
{
    public int Total => Allah + WithAllah + WithLillah;
}

/// <summary>
/// The research word lists and the Allah statistics, over the counted words
/// of a run of verses. Matching of الله always uses Simplify29, whatever the
/// text mode, as the original does.
/// </summary>
public static class ResearchWords
{
    private static readonly HashSet<string> AllahForms = new(StringComparer.Ordinal)
    {
        "الله", "ءالله", "ابالله", "اللهم", "بالله", "تالله", "فالله", "والله", "وتالله", "لله", "فلله", "ولله",
    };

    private static readonly HashSet<string> NonAllahForms = new(StringComparer.Ordinal)
    {
        "الضلله", "الكلله", "خلله", "خللها", "خللهما", "سلله", "ضلله", "ظلله", "ظللها", "كلله", "للهدي", "وظللهم",
        "يضلله", "اللهب", "اللهو",
    };

    private static readonly string[] NotWithLillah = ["اللهو", "اللهب", "ضلله", "ظلله", "كلله", "خلله", "سلله", "للهدي"];

    private static readonly string[] WordColumns = ["#", "Number", "InChapter", "Chapter", "Verse", "Word", "Text", "Order", "Total", "Value"];

    /// <param name="words">Counted word text of the whole book, in order.</param>
    /// <param name="values">Each word's value in the current system.</param>
    /// <param name="gap">For <see cref="WordListKind.Repeated"/>: words between the two (0 for neighbors).</param>
    public static ResearchTable List(
        WordListKind kind, Segmentation s, IReadOnlyList<string> words, IReadOnlyList<long> values,
        int firstVerse, int lastVerse, int gap = 0)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(words);
        ArgumentNullException.ThrowIfNull(values);

        // Order and Total count the same text across the whole book, as the original's Book does.
        var total = new Dictionary<string, int>(StringComparer.Ordinal);
        var order = new int[words.Count];
        for (int w = 0; w < words.Count; w++)
        {
            total[words[w]] = total.GetValueOrDefault(words[w]) + 1;
            order[w] = total[words[w]];
        }

        int first = s.VerseFirstWord[firstVerse];
        int end = s.VerseFirstWord[lastVerse] + s.VerseWordCount[lastVerse];
        var rows = new List<IReadOnlyList<string>>();

        IReadOnlyList<string> WordRow(int index, int w) =>
        [
            N(index), N(w + 1), N(s.WordNumberInChapter[w]), N(s.VerseChapter[s.WordVerse[w]]),
            N(s.VerseNumberInChapter[s.WordVerse[w]]), N(s.WordNumberInVerse[w]), words[w],
            N(order[w]), N(total[words[w]]), N(values[w]),
        ];

        switch (kind)
        {
            case WordListKind.Allah or WordListKind.NonAllah or WordListKind.All:
                HashSet<string>? forms = kind switch
                {
                    WordListKind.Allah => AllahForms,
                    WordListKind.NonAllah => NonAllahForms,
                    _ => null,
                };
                for (int w = first; w < end; w++)
                {
                    if (forms is null || forms.Contains(ArabicNormalizer.Simplify29(words[w]))) rows.Add(WordRow(rows.Count + 1, w));
                }
                return new ResearchTable(WordColumns, rows);

            case WordListKind.Double:
                int pair = 0;
                for (int w = first; w + 1 < end; w++)
                {
                    if (words[w] != words[w + 1]) continue;
                    pair++;
                    rows.Add(WordRow(pair, w));
                    rows.Add(WordRow(pair, w + 1));
                }
                return new ResearchTable(WordColumns, rows);

            default:
                if (gap < 0) throw new ArgumentOutOfRangeException(nameof(gap), "The gap is 0 or more words.");
                for (int v = firstVerse; v <= lastVerse; v++)
                {
                    int w0 = s.VerseFirstWord[v], count = s.VerseWordCount[v];
                    for (int i = 0; i + gap + 1 < count; i++)
                    {
                        int a = w0 + i, b = w0 + i + gap + 1;
                        if (words[a] != words[b]) continue;
                        rows.Add([
                            N(rows.Count + 1), words[a], words[b], N(v + 1), N(s.VerseChapter[v]),
                            N(s.VerseNumberInChapter[v]), N(i + 1), N(i + gap + 2),
                        ]);
                    }
                }
                return new ResearchTable(["#", "Word1", "Word2", "VerseNumber", "Chapter", "Verse", "W1", "W2"], rows);
        }
    }

    /// <summary>The original's Allah statistics drawing over the words given.</summary>
    public static AllahSummary Allah(IEnumerable<string> words)
    {
        ArgumentNullException.ThrowIfNull(words);
        int allah = 0, withAllah = 0, withLillah = 0;
        foreach (string word in words)
        {
            string text = ArabicNormalizer.Simplify29(word);
            if (text == "الله") allah++;
            else if (text.Contains("الله", StringComparison.Ordinal) &&
                     !text.Contains("اللهو", StringComparison.Ordinal) && !text.Contains("اللهب", StringComparison.Ordinal)) withAllah++;
            else if (text.Contains("لله", StringComparison.Ordinal) && !NotWithLillah.Any(x => text.Contains(x, StringComparison.Ordinal))) withLillah++;
        }
        return new AllahSummary(allah, withAllah, withLillah);
    }

    private static string N(long value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
