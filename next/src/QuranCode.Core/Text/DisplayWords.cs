using QuranCode.Core.Content;

namespace QuranCode.Core.Text;

/// <summary>
/// A verse ready for display: an optional Bismillah header, then its words.
/// </summary>
/// <param name="WordOffset">
/// Segmented words the header accounts for. The engine counts the Bismillah as
/// the first four words of verse 1; <see cref="DisplayWords.Align"/> maps them
/// to an empty span.
/// </param>
public sealed record VerseDisplay(string? Bismillah, IReadOnlyList<string> Words, int WordOffset);

/// <summary>The display words, by index, that one segmented word covers.</summary>
public readonly record struct DisplaySpan(int First, int Count);

/// <summary>
/// Splits a verse's display text into the words a reader sees.
/// </summary>
/// <remarks>
/// <para>
/// The canonical text separates Quranic marks from words with spaces, so a
/// naive split yields 82,459 tokens against the engine's 77,878 words. A mark
/// is not a word: a pause mark (ۚ ۖ ۗ) joins the word before it, and a leading
/// mark such as ۞ joins the word after it, which is where each sits visually.
/// </para>
/// <para>
/// Rule stages can join or split words, so display word N is not always
/// segmented word N. <see cref="Align"/> computes the mapping per verse.
/// </para>
/// </remarks>
public static class DisplayWords
{
    private const int BismillahWordCount = 4;

    /// <summary>
    /// Splits a verse, separating the Bismillah that the source text prefixes to
    /// verse 1 of every chapter except 1 (where it is the verse) and 9 (which has
    /// none).
    /// </summary>
    /// <remarks>
    /// A mushaf prints it as a chapter header, so the UI does the same. Chapters
    /// 95 and 97 spell it with a shadda (بِّسْمِ), which is why this is
    /// positional rather than a string match. An edition that stores the
    /// Bismillah as its own verse 0 has nothing to separate.
    /// </remarks>
    public static VerseDisplay SplitVerse(
        string text, int chapterNumber, int numberInChapter, BasmalaMode mode = BasmalaMode.Prefix)
    {
        string[] words = Split(text);
        bool hasHeader = mode == BasmalaMode.Prefix
            && numberInChapter == 1 && chapterNumber is not (1 or 9) && words.Length > BismillahWordCount;
        if (!hasHeader) return new VerseDisplay(null, words, 0);

        return new VerseDisplay(
            string.Join(' ', words[..BismillahWordCount]),
            words[BismillahWordCount..],
            BismillahWordCount);
    }

    /// <summary>Splits text into words, attaching marks to their neighbors.</summary>
    public static string[] Split(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var words = new List<string>();
        string? leading = null;
        foreach (string token in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            bool isMark = !token.Any(char.IsLetter);
            if (isMark && words.Count > 0)
            {
                words[^1] = $"{words[^1]} {token}";
            }
            else if (isMark)
            {
                leading = leading is null ? token : $"{leading} {token}";
            }
            else
            {
                words.Add(leading is null ? token : $"{leading} {token}");
                leading = null;
            }
        }

        // Marks with no word at all: keep them rather than lose text.
        if (leading is not null) words.Add(leading);
        return [.. words];
    }

    /// <summary>Largest run of display words one segmented word may join.</summary>
    private const int MaxJoin = 3;

    /// <summary>
    /// Maps every segmented word of a verse to the display words it came from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rule stages join and split words: Original joins "بَعْدَ مَا" into
    /// "بعدما" in 2:181, 8:6 and 13:37, and other modes do more. Rather than
    /// encode each rule, this normalizes display words with the same pipeline
    /// and matches them greedily against the segmented words, allowing a
    /// segmented word to cover up to three display words or several segmented
    /// words to share one.
    /// </para>
    /// <para>
    /// Returns null when a verse cannot be aligned. The caller then highlights
    /// the verse rather than guessing at words, which is the honest failure.
    /// </para>
    /// </remarks>
    /// <param name="segmentedWords">The verse's words as the segmentation holds them, header included.</param>
    public static DisplaySpan[]? Align(VerseDisplay display, IReadOnlyList<string> segmentedWords, TextPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(display);
        ArgumentNullException.ThrowIfNull(segmentedWords);
        ArgumentNullException.ThrowIfNull(pipeline);

        var spans = new DisplaySpan[segmentedWords.Count];
        int s = 0;

        // The header is not displayed as words, so its segmented words map to
        // an empty span at the start.
        for (; s < display.WordOffset && s < spans.Length; s++) spans[s] = new DisplaySpan(0, 0);

        IReadOnlyList<string> words = display.Words;
        int d = 0;
        while (d < words.Count && s < segmentedWords.Count)
        {
            if (TryJoin(words, d, segmentedWords[s], pipeline, out int joined))
            {
                spans[s++] = new DisplaySpan(d, joined);
                d += joined;
                continue;
            }

            // One display word split into several segmented words.
            int split = TrySplit(words[d], segmentedWords, s, pipeline);
            if (split == 0) return null;

            for (int i = 0; i < split; i++) spans[s++] = new DisplaySpan(d, 1);
            d++;
        }

        return d == words.Count && s == segmentedWords.Count ? spans : null;
    }

    private static bool TryJoin(IReadOnlyList<string> words, int start, string target, TextPipeline pipeline, out int count)
    {
        string wanted = Compact(target);
        for (count = 1; count <= MaxJoin && start + count <= words.Count; count++)
        {
            string candidate = string.Join(' ', Enumerable.Range(start, count).Select(i => words[i]));
            if (Compact(pipeline.Normalize(candidate)) == wanted) return true;
        }
        count = 0;
        return false;
    }

    private static int TrySplit(string word, IReadOnlyList<string> segmented, int start, TextPipeline pipeline)
    {
        string wanted = Compact(pipeline.Normalize(word));
        string accumulated = "";
        for (int count = 1; count <= MaxJoin && start + count <= segmented.Count; count++)
        {
            accumulated += Compact(segmented[start + count - 1]);
            if (accumulated == wanted) return count;
            if (accumulated.Length >= wanted.Length) break;
        }
        return 0;
    }

    private static string Compact(string text) => text.Replace(" ", "", StringComparison.Ordinal);
}
