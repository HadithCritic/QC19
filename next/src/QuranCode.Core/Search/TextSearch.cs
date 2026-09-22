using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Search;

/// <summary>How a term must sit within a word.</summary>
public enum Wordness
{
    /// <summary>Anywhere: the term may be the word or part of it.</summary>
    Any,

    /// <summary>The term must be the entire word.</summary>
    WholeWord,

    /// <summary>The term must appear inside a longer word.</summary>
    PartOfWord,
}

/// <summary>One matching word.</summary>
public readonly record struct WordMatch(int WordIndex, int VerseNumber, string Text);

/// <summary>A completed search.</summary>
public sealed class SearchResult
{
    public required string Term { get; init; }
    public required Wordness Wordness { get; init; }
    public required IReadOnlyList<WordMatch> Words { get; init; }
    public required IReadOnlyList<int> Verses { get; init; }

    public int WordCount => Words.Count;
    public int VerseCount => Verses.Count;
}

/// <summary>
/// Arabic text search over the corpus.
/// </summary>
/// <remarks>
/// <para>
/// Reproduces the behavior of the legacy <c>Server.DoFindWords</c> chain for the
/// case the UI actually issues: a term, a wordness filter, whole-book scope, no
/// multiplicity constraint.
/// </para>
/// <para>
/// <b>Normalization is the whole game.</b> The legacy engine simplifies both the
/// search term and the verse text with <c>Simplify29</c> before matching, when
/// the active text mode is <c>Original</c> or <c>SimplifiedMarks</c> and
/// diacritics are off. Matching raw text against a normalized term finds
/// nothing, which is exactly what happens if this step is skipped.
/// </para>
/// <para>
/// <b>Why not FTS5 here.</b> The corpus is 77,878 words and a linear scan over
/// a segmentation runs in single-digit milliseconds, so FTS earns nothing for
/// Arabic substring matching and would additionally impose its own tokenizer's
/// idea of a word boundary, which is not the legacy one. FTS5 is reserved for
/// translation text, where documents are long, prose is natural language, and
/// ranking matters.
/// </para>
/// </remarks>
public sealed class TextSearch
{
    private readonly Segmentation _segmentation;
    private readonly IReadOnlyList<Verse> _verses;
    private readonly string[] _wordTexts;

    public TextSearch(Segmentation segmentation, IReadOnlyList<Verse> verses, TextPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(segmentation);
        ArgumentNullException.ThrowIfNull(verses);
        ArgumentNullException.ThrowIfNull(pipeline);

        _segmentation = segmentation;
        _verses = verses;

        // Materialize the normalized word text once. The segmentation keeps
        // letters, not words, so rebuilding a word's string per query would
        // dominate the search cost.
        _wordTexts = new string[segmentation.WordCount];
        for (int w = 0; w < segmentation.WordCount; w++)
        {
            int first = segmentation.WordFirstLetter[w];
            int count = segmentation.WordLetterCount[w];
            _wordTexts[w] = new string(segmentation.LetterChars, first, count);
        }
    }

    /// <summary>Words already normalized, in corpus order.</summary>
    public IReadOnlyList<string> WordTexts => _wordTexts;

    /// <summary>
    /// Finds words matching a term.
    /// </summary>
    /// <param name="term">
    /// Search text. Normalized the same way the corpus was, so a caller may pass
    /// either raw or simplified Arabic.
    /// </param>
    public SearchResult Find(string term, Wordness wordness = Wordness.Any)
    {
        ArgumentNullException.ThrowIfNull(term);

        // Match the legacy: normalize the term exactly as the corpus was, and
        // collapse internal whitespace.
        string needle = ArabicNormalizer.Simplify29(term).Trim();
        while (needle.Contains("  ", StringComparison.Ordinal))
        {
            needle = needle.Replace("  ", " ", StringComparison.Ordinal);
        }

        var words = new List<WordMatch>();
        var verses = new List<int>();
        int lastVerse = -1;

        if (needle.Length != 0)
        {
            for (int w = 0; w < _wordTexts.Length; w++)
            {
                string text = _wordTexts[w];
                if (!Matches(text, needle, wordness)) continue;

                int verseIndex = _segmentation.WordVerse[w];
                int verseNumber = _verses[verseIndex].Number;

                words.Add(new WordMatch(w, verseNumber, text));

                // Words are scanned in corpus order, so verse numbers arrive
                // sorted and ascending: a single comparison deduplicates them
                // without a set.
                if (verseNumber != lastVerse)
                {
                    verses.Add(verseNumber);
                    lastVerse = verseNumber;
                }
            }
        }

        return new SearchResult
        {
            Term = needle,
            Wordness = wordness,
            Words = words,
            Verses = verses,
        };
    }

    private static bool Matches(string word, string needle, Wordness wordness) => wordness switch
    {
        Wordness.WholeWord => string.Equals(word, needle, StringComparison.Ordinal),

        // "Part of a word" means the term occurs but the word is not the term,
        // so an exact match is excluded.
        Wordness.PartOfWord =>
            word.Contains(needle, StringComparison.Ordinal) &&
            !string.Equals(word, needle, StringComparison.Ordinal),

        _ => word.Contains(needle, StringComparison.Ordinal),
    };
}
