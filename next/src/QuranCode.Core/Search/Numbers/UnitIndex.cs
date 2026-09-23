using System.Numerics;
using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Search.Numbers;

/// <summary>
/// Per-word facts the number and frequency searches measure, built once per
/// text mode, counting options and value system.
/// </summary>
public sealed class UnitIndex
{
    private IReadOnlyList<WordSpan>? _bookSentences;

    /// <param name="pauseMarks">Pause marks an edition stores apart from its text, by verse number and display word.</param>
    public UnitIndex(
        Segmentation segmentation, IReadOnlyList<Verse> verses, IReadOnlyList<long> wordValues,
        CountingText text, Func<Verse, VerseDisplay> display,
        IReadOnlyDictionary<int, Dictionary<int, string>>? pauseMarks = null)
    {
        ArgumentNullException.ThrowIfNull(segmentation);
        ArgumentNullException.ThrowIfNull(verses);
        ArgumentNullException.ThrowIfNull(wordValues);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(display);

        Segmentation = segmentation;
        Verses = verses;
        WordValues = wordValues;

        var letterIndex = new Dictionary<char, int>();
        foreach (char c in segmentation.LetterChars)
        {
            if (!letterIndex.ContainsKey(c)) letterIndex[c] = letterIndex.Count;
        }
        if (letterIndex.Count > 64)
        {
            throw new InvalidOperationException($"The text has {letterIndex.Count} distinct letters; at most 64 are supported.");
        }
        LetterIndex = letterIndex;

        int words = segmentation.WordCount;
        WordMasks = new ulong[words];
        WordTexts = new string[words];
        for (int w = 0; w < words; w++)
        {
            int first = segmentation.WordFirstLetter[w];
            for (int l = first; l < first + segmentation.WordLetterCount[w]; l++)
            {
                WordMasks[w] |= 1UL << letterIndex[segmentation.LetterChars[l]];
            }
            WordTexts[w] = segmentation.WordText(w);
        }

        WordMarks = new Stopmark[words];
        for (int v = 0; v < verses.Count; v++) MarkVerse(v, text, display(verses[v]), pauseMarks?.GetValueOrDefault(verses[v].Number));

        VerseTexts = new string[verses.Count];
        for (int v = 0; v < verses.Count; v++) VerseTexts[v] = string.Join(' ', segmentation.VerseWords(v));
    }

    public Segmentation Segmentation { get; }

    /// <summary>The counted verses, aligned with the segmentation.</summary>
    public IReadOnlyList<Verse> Verses { get; }

    public IReadOnlyList<long> WordValues { get; }

    /// <summary>Each distinct letter's bit.</summary>
    public IReadOnlyDictionary<char, int> LetterIndex { get; }

    /// <summary>The letters each word contains, one bit per distinct letter.</summary>
    public ulong[] WordMasks { get; }

    public string[] WordTexts { get; }

    /// <summary>Each verse's counted text, words joined by spaces.</summary>
    public string[] VerseTexts { get; }

    /// <summary>The stop mark after each counted word.</summary>
    public Stopmark[] WordMarks { get; }

    public static int UniqueLetters(ulong mask) => BitOperations.PopCount(mask);

    /// <summary>Sentences of the whole book; a scope splits its own verses.</summary>
    public IReadOnlyList<WordSpan> Sentences(IReadOnlyList<int>? verseIndexes = null)
    {
        if (verseIndexes is null) return _bookSentences ??= SplitWords(Enumerable.Range(0, Verses.Count).ToArray());
        return SplitWords(verseIndexes);
    }

    private IReadOnlyList<WordSpan> SplitWords(IReadOnlyList<int> verseIndexes)
    {
        // The words of the searched verses, in order, as the legacy concatenates them.
        var words = new List<int>();
        foreach (int v in verseIndexes)
        {
            int first = Segmentation.VerseFirstWord[v];
            for (int w = first; w < first + Segmentation.VerseWordCount[v]; w++) words.Add(w);
        }
        IReadOnlyList<WordSpan> local = Numbers.Sentences.Split(
            words.Select(w => WordMarks[w]).ToArray(),
            i => ArabicNormalizer.Simplify29(WordTexts[words[i]]));

        // Back to absolute word indexes; a scope's verses may not be contiguous,
        // and a sentence runs over the words as listed.
        return local.Select(s => new WordSpan(words[s.First], words[s.Last])).ToArray();
    }

    private void MarkVerse(int verse, CountingText text, VerseDisplay display, Dictionary<int, string>? stored)
    {
        int first = Segmentation.VerseFirstWord[verse];
        int count = Segmentation.VerseWordCount[verse];
        if (count == 0) return;

        Stopmark[] displayMarks = Numbers.Sentences.DisplayMarks(DisplayWords.Split(Verses[verse].Text));
        if (stored is not null)
        {
            foreach ((int word, string mark) in stored)
            {
                if (word < displayMarks.Length && displayMarks[word] is Stopmark.None or Stopmark.MustStop)
                {
                    Stopmark parsed = Numbers.Sentences.MarkOf(mark);
                    if (parsed != Stopmark.None) displayMarks[word] = parsed;
                }
            }
        }
        DisplaySpan[]? spans = DisplayWords.Align(display, Segmentation.VerseWords(verse), text.NormalizeWord);
        if (spans is not null)
        {
            for (int s = 0; s < count; s++)
            {
                int covered = s < display.WordOffset ? s : display.WordOffset + spans[s].First + spans[s].Count - 1;
                // When one display word splits into several counted words
                // (waw as a word), its mark belongs to the last of them.
                bool lastOfDisplayWord = s < display.WordOffset || s == count - 1 || spans[s + 1].First != spans[s].First;
                if (lastOfDisplayWord && covered >= 0 && covered < displayMarks.Length) WordMarks[first + s] = displayMarks[covered];
            }
        }

        // Every verse ends with a stop, whatever the marks say.
        if (WordMarks[first + count - 1] == Stopmark.None) WordMarks[first + count - 1] = Stopmark.MustStop;
    }
}
