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
/// Reproduces the legacy <c>Server.DoFindWords</c> (terms, wordness, any or all
/// words) and the Exact search (<c>DoFindPhrases</c>) over a verse scope.
/// </para>
/// <para>
/// <b>Normalization is the whole game.</b> The legacy engine simplifies both the
/// search term and the verse text before matching. The corpus side is the
/// segmentation, already in the text mode and counting options; the term goes
/// through the same word normalization, so a term typed with its marks is
/// changed the way the text was.
/// </para>
/// <para>
/// <b>Why not FTS5 here.</b> The corpus is 77,878 words and a linear scan over
/// a segmentation runs in single-digit milliseconds, so FTS earns nothing for
/// Arabic substring matching and would additionally impose its own tokenizer's
/// idea of a word boundary, which is not the legacy one.
/// </para>
/// </remarks>
public sealed class TextSearch
{
    private readonly Segmentation _segmentation;
    private readonly IReadOnlyList<Verse> _verses;
    private readonly string[] _wordTexts;
    private readonly Func<string, string> _normalizeTerm;
    private string[]? _verseTexts;

    /// <param name="normalizeTerm">How a typed term becomes corpus text; Simplify29 when omitted.</param>
    public TextSearch(
        Segmentation segmentation, IReadOnlyList<Verse> verses, TextPipeline pipeline,
        Func<string, string>? normalizeTerm = null)
    {
        ArgumentNullException.ThrowIfNull(segmentation);
        ArgumentNullException.ThrowIfNull(verses);
        ArgumentNullException.ThrowIfNull(pipeline);

        _segmentation = segmentation;
        _verses = verses;
        _normalizeTerm = normalizeTerm ?? ArabicNormalizer.Simplify29;

        // Materialize the normalized word text once. The segmentation keeps
        // letters, not words, so rebuilding a word's string per query would
        // dominate the search cost.
        _wordTexts = new string[segmentation.WordCount];
        for (int w = 0; w < segmentation.WordCount; w++) _wordTexts[w] = segmentation.WordText(w);
    }

    /// <summary>Words already normalized, in corpus order.</summary>
    public IReadOnlyList<string> WordTexts => _wordTexts;

    /// <summary>Finds words matching one term (or several, any of which may match).</summary>
    public SearchResult Find(string term, Wordness wordness = Wordness.Any)
    {
        ArgumentNullException.ThrowIfNull(term);
        return Find(new TextQuery(term, wordness));
    }

    public SearchResult Find(TextQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        IReadOnlyList<Term> terms = query.Terms(_normalizeTerm);

        var words = new List<WordMatch>();
        var verses = new List<int>();
        if (terms.Count > 0)
        {
            for (int v = 0; v < _segmentation.VerseCount; v++)
            {
                int verseNumber = _verses[v].Number;
                if (query.Scope is not null && !query.Scope.Contains(verseNumber)) continue;

                IReadOnlyList<int>? hits = query.Grouping == Grouping.Phrase
                    ? PhraseHits(v, terms, query.Wordness)
                    : TermHits(v, terms, query.Wordness, query.Grouping);
                if (hits is null) continue;

                verses.Add(verseNumber);
                foreach (int w in hits) words.Add(new WordMatch(w, verseNumber, _wordTexts[w]));
            }
        }

        return new SearchResult
        {
            Term = string.Join(' ', terms.Select(t => t.Kind switch
            {
                TermKind.Required => "+" + t.Text,
                TermKind.Excluded => "-" + t.Text,
                _ => t.Text,
            })),
            Wordness = query.Wordness,
            Words = words,
            Verses = verses,
        };
    }

    /// <summary>The verse's matching words, or null when the verse does not qualify.</summary>
    private List<int>? TermHits(int verse, IReadOnlyList<Term> terms, Wordness wordness, Grouping grouping)
    {
        int first = _segmentation.VerseFirstWord[verse];
        int end = first + _segmentation.VerseWordCount[verse];

        var hits = new SortedSet<int>();
        int plainTerms = 0, plainFound = 0;
        foreach (Term term in terms)
        {
            bool found = false;
            for (int w = first; w < end; w++)
            {
                if (!Matches(_wordTexts[w], term.Text, wordness)) continue;
                found = true;
                if (term.Kind == TermKind.Excluded) break;
                hits.Add(w);
            }

            switch (term.Kind)
            {
                case TermKind.Excluded when found:
                case TermKind.Required when !found:
                    return null;
                case TermKind.Plain:
                    plainTerms++;
                    if (found) plainFound++;
                    break;
            }
        }

        bool grouped = plainTerms == 0 || (grouping == Grouping.All ? plainFound == plainTerms : plainFound > 0);
        return grouped ? [.. hits] : null;
    }

    /// <summary>Words covered by each occurrence of the terms written in a row.</summary>
    private List<int>? PhraseHits(int verse, IReadOnlyList<Term> terms, Wordness wordness)
    {
        string text = VerseText(verse);
        string needle = string.Join(' ', terms.Select(t => t.Text));
        int first = _segmentation.VerseFirstWord[verse];
        int count = _segmentation.VerseWordCount[verse];

        List<int>? hits = null;
        for (int at = text.IndexOf(needle, StringComparison.Ordinal); at >= 0;
             at = text.IndexOf(needle, at + 1, StringComparison.Ordinal))
        {
            int end = at + needle.Length;
            bool whole = (at == 0 || text[at - 1] == ' ') && (end == text.Length || text[end] == ' ');
            if (wordness == Wordness.WholeWord && !whole) continue;
            if (wordness == Wordness.PartOfWord && whole) continue;

            hits ??= [];
            int start = 0;
            for (int w = 0; w < count; w++)
            {
                int stop = start + _wordTexts[first + w].Length;
                if (start < end && stop > at && !hits.Contains(first + w)) hits.Add(first + w);
                start = stop + 1;
            }
        }
        return hits;
    }

    private string VerseText(int verse)
    {
        _verseTexts ??= new string[_segmentation.VerseCount];
        return _verseTexts[verse] ??= string.Join(' ',
            _wordTexts, _segmentation.VerseFirstWord[verse], _segmentation.VerseWordCount[verse]);
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
