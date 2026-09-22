using QuranCode.Core.Text;

namespace QuranCode.Core.Content;

/// <summary>
/// The corpus split into verses, words and letters, with the positional and
/// distance metadata the value modifiers consume.
/// </summary>
/// <remarks>
/// <para>
/// This is the replacement for the legacy object graph, and the reason it is a
/// separate type rather than part of <see cref="ContentRepository"/> is that
/// most work never needs it. A plain value calculation reads text and a letter
/// map; only the 21 <c>AddTo*</c> modifiers need to know where a letter sits or
/// how far it is from its previous occurrence.
/// </para>
/// <para>
/// Stored as parallel arrays rather than objects. The legacy engine allocated
/// 327,792 <c>Letter</c> instances carrying ~20 counters and a back-pointer
/// each, costing 3,804 ms and 136 MB. The same information here is a handful of
/// <c>int[]</c>, built in a single pass, and built only on demand.
/// </para>
/// <para>
/// Distances follow the legacy definition exactly, from
/// <c>Book.SetupDistancesToPrevious</c>: the first occurrence of a key gets
/// zero, and every later occurrence gets the difference from the <i>previous</i>
/// occurrence, not the first.
/// </para>
/// </remarks>
public sealed class Segmentation
{
    // -- verses -----------------------------------------------------------

    /// <summary>Chapter number of each verse, 1-based values, 0-based index.</summary>
    public int[] VerseChapter { get; }

    /// <summary>Position of each verse within its chapter.</summary>
    public int[] VerseNumberInChapter { get; }

    /// <summary>Index of each verse's first word.</summary>
    public int[] VerseFirstWord { get; }

    /// <summary>Word count of each verse.</summary>
    public int[] VerseWordCount { get; }

    // -- words ------------------------------------------------------------

    /// <summary>Verse index owning each word.</summary>
    public int[] WordVerse { get; }

    /// <summary>Position of each word within its verse.</summary>
    public int[] WordNumberInVerse { get; }

    /// <summary>Position of each word within its chapter.</summary>
    public int[] WordNumberInChapter { get; }

    /// <summary>Index of each word's first letter.</summary>
    public int[] WordFirstLetter { get; }

    /// <summary>Letter count of each word.</summary>
    public int[] WordLetterCount { get; }

    /// <summary>Distance in words to the previous identical word.</summary>
    public int[] WordDistanceW { get; }

    /// <summary>Distance in verses to the previous identical word.</summary>
    public int[] WordDistanceV { get; }

    /// <summary>Distance in chapters to the previous identical word.</summary>
    public int[] WordDistanceC { get; }

    // -- letters ----------------------------------------------------------

    /// <summary>The letter characters, in corpus order.</summary>
    public char[] LetterChars { get; }

    /// <summary>Word index owning each letter.</summary>
    public int[] LetterWord { get; }

    /// <summary>Position of each letter within its word.</summary>
    public int[] LetterNumberInWord { get; }

    /// <summary>Position of each letter within its verse.</summary>
    public int[] LetterNumberInVerse { get; }

    /// <summary>Position of each letter within its chapter.</summary>
    public int[] LetterNumberInChapter { get; }

    /// <summary>Distance in letters to the previous identical letter.</summary>
    public int[] LetterDistanceL { get; }

    /// <summary>Distance in words to the previous identical letter.</summary>
    public int[] LetterDistanceW { get; }

    /// <summary>Distance in verses to the previous identical letter.</summary>
    public int[] LetterDistanceV { get; }

    /// <summary>Distance in chapters to the previous identical letter.</summary>
    public int[] LetterDistanceC { get; }

    public int VerseCount => VerseChapter.Length;
    public int WordCount => WordVerse.Length;
    public int LetterCount => LetterChars.Length;

    private Segmentation(
        int[] verseChapter, int[] verseNumberInChapter, int[] verseFirstWord, int[] verseWordCount,
        int[] wordVerse, int[] wordNumberInVerse, int[] wordNumberInChapter,
        int[] wordFirstLetter, int[] wordLetterCount,
        int[] wordDistW, int[] wordDistV, int[] wordDistC,
        char[] letterChars, int[] letterWord,
        int[] letterNumberInWord, int[] letterNumberInVerse, int[] letterNumberInChapter,
        int[] letterDistL, int[] letterDistW, int[] letterDistV, int[] letterDistC)
    {
        VerseChapter = verseChapter;
        VerseNumberInChapter = verseNumberInChapter;
        VerseFirstWord = verseFirstWord;
        VerseWordCount = verseWordCount;
        WordVerse = wordVerse;
        WordNumberInVerse = wordNumberInVerse;
        WordNumberInChapter = wordNumberInChapter;
        WordFirstLetter = wordFirstLetter;
        WordLetterCount = wordLetterCount;
        WordDistanceW = wordDistW;
        WordDistanceV = wordDistV;
        WordDistanceC = wordDistC;
        LetterChars = letterChars;
        LetterWord = letterWord;
        LetterNumberInWord = letterNumberInWord;
        LetterNumberInVerse = letterNumberInVerse;
        LetterNumberInChapter = letterNumberInChapter;
        LetterDistanceL = letterDistL;
        LetterDistanceW = letterDistW;
        LetterDistanceV = letterDistV;
        LetterDistanceC = letterDistC;
    }

    /// <summary>
    /// Segments the whole corpus under one text pipeline.
    /// </summary>
    /// <remarks>
    /// One pass assigns positions; a second assigns distances, which need the
    /// absolute numbers from the first.
    /// </remarks>
    /// <param name="distancesWithinChapters">
    /// Reset the distance search at each chapter boundary. Defaults to
    /// <c>true</c>, matching <c>NumericalSystem.AddDistancesWithinChapters</c>,
    /// which is initialized to <c>true</c> in the legacy model.
    /// <para>
    /// This matters more than it looks. The legacy engine computes distances
    /// once, while building the book, so the value of this flag at build time is
    /// baked into every later calculation and toggling it afterwards does
    /// nothing. Getting it wrong shifts distance-modifier results for every
    /// verse after the first chapter.
    /// </para>
    /// </param>
    public static Segmentation Build(
        IReadOnlyList<Verse> verses, TextPipeline pipeline, bool distancesWithinChapters = true)
    {
        ArgumentNullException.ThrowIfNull(verses);
        ArgumentNullException.ThrowIfNull(pipeline);

        int verseCount = verses.Count;
        var verseChapter = new int[verseCount];
        var verseNumberInChapter = new int[verseCount];
        var verseFirstWord = new int[verseCount];
        var verseWordCount = new int[verseCount];

        // Sized generously from the known corpus shape; List handles the rest.
        var wordVerse = new List<int>(80_000);
        var wordNumberInVerse = new List<int>(80_000);
        var wordNumberInChapter = new List<int>(80_000);
        var wordFirstLetter = new List<int>(80_000);
        var wordLetterCount = new List<int>(80_000);
        var wordTexts = new List<string>(80_000);

        var letterChars = new List<char>(340_000);
        var letterWord = new List<int>(340_000);
        var letterNumberInWord = new List<int>(340_000);
        var letterNumberInVerse = new List<int>(340_000);
        var letterNumberInChapter = new List<int>(340_000);

        int currentChapter = 0;
        int wordInChapter = 0;
        int letterInChapter = 0;

        for (int v = 0; v < verseCount; v++)
        {
            Verse verse = verses[v];
            if (verse.ChapterNumber != currentChapter)
            {
                currentChapter = verse.ChapterNumber;
                wordInChapter = 0;
                letterInChapter = 0;
            }

            verseChapter[v] = verse.ChapterNumber;
            verseNumberInChapter[v] = verse.NumberInChapter;
            verseFirstWord[v] = wordVerse.Count;

            string normalized = pipeline.Normalize(verse.Text);

            int wordInVerse = 0;
            int letterInVerse = 0;

            foreach (Range range in Split(normalized))
            {
                ReadOnlySpan<char> word = normalized.AsSpan()[range];
                if (word.IsEmpty) continue;

                wordInVerse++;
                wordInChapter++;

                wordVerse.Add(v);
                wordNumberInVerse.Add(wordInVerse);
                wordNumberInChapter.Add(wordInChapter);
                wordFirstLetter.Add(letterChars.Count);
                wordTexts.Add(new string(word));

                int wordIndex = wordVerse.Count - 1;
                int lettersHere = 0;

                for (int i = 0; i < word.Length; i++)
                {
                    lettersHere++;
                    letterInVerse++;
                    letterInChapter++;

                    letterChars.Add(word[i]);
                    letterWord.Add(wordIndex);
                    letterNumberInWord.Add(lettersHere);
                    letterNumberInVerse.Add(letterInVerse);
                    letterNumberInChapter.Add(letterInChapter);
                }
                wordLetterCount.Add(lettersHere);
            }
            verseWordCount[v] = wordVerse.Count - verseFirstWord[v];
        }

        char[] chars = [.. letterChars];
        int[] lWord = [.. letterWord];
        int[] wVerse = [.. wordVerse];

        (int[] letterDistL, int[] letterDistW, int[] letterDistV, int[] letterDistC) =
            LetterDistances(chars, lWord, wVerse, verseChapter, distancesWithinChapters);

        (int[] wordDistW, int[] wordDistV, int[] wordDistC) =
            WordDistances(wordTexts, wVerse, verseChapter, distancesWithinChapters);

        return new Segmentation(
            verseChapter, verseNumberInChapter, verseFirstWord, verseWordCount,
            wVerse, [.. wordNumberInVerse], [.. wordNumberInChapter],
            [.. wordFirstLetter], [.. wordLetterCount],
            wordDistW, wordDistV, wordDistC,
            chars, lWord,
            [.. letterNumberInWord], [.. letterNumberInVerse], [.. letterNumberInChapter],
            letterDistL, letterDistW, letterDistV, letterDistC);
    }

    /// <summary>
    /// Distance from each letter to the previous occurrence of the same
    /// character. First occurrences get zero, matching the legacy engine.
    /// </summary>
    private static (int[], int[], int[], int[]) LetterDistances(
        char[] chars, int[] letterWord, int[] wordVerse, int[] verseChapter,
        bool withinChapters)
    {
        int n = chars.Length;
        var distL = new int[n];
        var distW = new int[n];
        var distV = new int[n];
        var distC = new int[n];

        var lastLetter = new Dictionary<char, (int L, int W, int V, int C)>();
        int currentChapter = 0;

        for (int i = 0; i < n; i++)
        {
            int wordNumber = letterWord[i] + 1;
            int verseNumber = wordVerse[letterWord[i]] + 1;
            int chapterNumber = verseChapter[wordVerse[letterWord[i]]];

            if (withinChapters && chapterNumber != currentChapter)
            {
                currentChapter = chapterNumber;
                lastLetter.Clear();
            }

            if (lastLetter.TryGetValue(chars[i], out var previous))
            {
                distL[i] = (i + 1) - previous.L;
                distW[i] = wordNumber - previous.W;
                distV[i] = verseNumber - previous.V;
                distC[i] = chapterNumber - previous.C;
            }
            lastLetter[chars[i]] = (i + 1, wordNumber, verseNumber, chapterNumber);
        }
        return (distL, distW, distV, distC);
    }

    /// <summary>
    /// Distance from each word to the previous occurrence of the same text.
    /// </summary>
    private static (int[], int[], int[]) WordDistances(
        List<string> wordTexts, int[] wordVerse, int[] verseChapter, bool withinChapters)
    {
        int n = wordTexts.Count;
        var distW = new int[n];
        var distV = new int[n];
        var distC = new int[n];

        var last = new Dictionary<string, (int W, int V, int C)>(StringComparer.Ordinal);
        int currentChapter = 0;

        for (int i = 0; i < n; i++)
        {
            int verseNumber = wordVerse[i] + 1;
            int chapterNumber = verseChapter[wordVerse[i]];

            if (withinChapters && chapterNumber != currentChapter)
            {
                currentChapter = chapterNumber;
                last.Clear();
            }

            if (last.TryGetValue(wordTexts[i], out var previous))
            {
                distW[i] = (i + 1) - previous.W;
                distV[i] = verseNumber - previous.V;
                distC[i] = chapterNumber - previous.C;
            }
            last[wordTexts[i]] = (i + 1, verseNumber, chapterNumber);
        }
        return (distW, distV, distC);
    }

    private static SplitRanges Split(string text) => new(text);

    private ref struct SplitRanges(string text)
    {
        private readonly ReadOnlySpan<char> _text = text.AsSpan();
        private int _position;

        public Range Current { get; private set; }

        public readonly SplitRanges GetEnumerator() => this;

        public bool MoveNext()
        {
            while (_position < _text.Length && (_text[_position] == ' ' || _text[_position] == '\n')) _position++;
            if (_position >= _text.Length) return false;

            int start = _position;
            while (_position < _text.Length && _text[_position] != ' ' && _text[_position] != '\n') _position++;

            Current = start.._position;
            return true;
        }
    }
}
