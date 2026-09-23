using QuranCode.Core.Content;

namespace QuranCode.Core.Search;

/// <summary>How F6 compares two verses (legacy <c>SimilarityMethod</c>).</summary>
public enum SimilarityMethod
{
    /// <summary>Edit-distance similarity of the whole verse text.</summary>
    Text,

    /// <summary>Every word of the verse has its own similar word in the other verse.</summary>
    Words,

    /// <summary>Enough words share their root with a word of the other verse.</summary>
    Roots,

    /// <summary>Enough words share their value with a word of the other verse.</summary>
    Values,
}

/// <summary>Which words of a found verse a hit list points at.</summary>
public enum HitWords
{
    /// <summary>The whole verse matched; no single words.</summary>
    None,

    /// <summary>Indexes among the verse's counted words (the segmentation).</summary>
    Counted,

    /// <summary>Indexes among the verse's display words.</summary>
    Display,
}

/// <summary>A found verse, the words that matched in it, and how close it came.</summary>
public sealed record VerseHit(int VerseNumber, IReadOnlyList<int> Words, HitWords Kind, double? Score = null);

/// <summary>
/// Related verses (F5, Features.txt #40) and similar verses (F6, #41).
/// </summary>
/// <remarks>
/// Follows the legacy <c>Server.FindRelatedVerses</c> and the current-verse
/// form of <c>Server.FindVerses(verse, method, percentage)</c>. The legacy
/// also offers an all-pairs form (every verse against every other, grouped);
/// it is not reproduced, see the Phase 4 log.
/// </remarks>
public sealed class VerseSimilarity
{
    /// <summary>The legacy threshold test allows for rounding in the percentage box.</summary>
    private const double Tolerance = 0.0005;

    private readonly Segmentation _segmentation;
    private readonly IReadOnlyList<Verse> _verses;
    private readonly IReadOnlyList<string> _wordTexts;
    private readonly WordRoots _roots;
    private readonly RootSearch _rootSearch;
    private readonly Dictionary<int, int> _indexOf;
    private string[]? _verseTexts;

    /// <param name="verses">The counted verses, aligned with the segmentation.</param>
    /// <param name="wordTexts">Counted word text, as <see cref="TextSearch.WordTexts"/>.</param>
    public VerseSimilarity(
        Segmentation segmentation, IReadOnlyList<Verse> verses, IReadOnlyList<string> wordTexts,
        WordRoots roots, RootSearch rootSearch)
    {
        _segmentation = segmentation;
        _verses = verses;
        _wordTexts = wordTexts;
        _roots = roots;
        _rootSearch = rootSearch;
        _indexOf = new Dictionary<int, int>(verses.Count);
        for (int i = 0; i < verses.Count; i++) _indexOf[verses[i].Number] = i;
    }

    /// <summary>
    /// Verses in which every word of the given verse pairs with its own word
    /// sharing a root. The verse finds itself.
    /// </summary>
    public IReadOnlyList<VerseHit> Related(int verseNumber, IReadOnlySet<int>? scope = null)
    {
        IReadOnlyList<int[]> wanted = _roots.OfVerse(verseNumber);
        var found = new List<VerseHit>();
        foreach (Verse verse in InScope(scope))
        {
            IReadOnlyList<int[]> candidate = _roots.OfVerse(verse.Number);
            int[]? pairs = Pair(wanted.Count, candidate.Count, (a, b) => wanted[a].Intersect(candidate[b]).Any());
            if (pairs is not null) found.Add(new VerseHit(verse.Number, pairs.Order().ToArray(), HitWords.Display));
        }
        return found;
    }

    /// <summary>Verses similar to one verse by a method, at or above a threshold between 0 and 1.</summary>
    /// <param name="wordValues">Counted word values; needed only for <see cref="SimilarityMethod.Values"/>.</param>
    public IReadOnlyList<VerseHit> Similar(
        int verseNumber, SimilarityMethod method, double threshold,
        IReadOnlyList<long>? wordValues = null, IReadOnlySet<int>? scope = null)
    {
        if (threshold is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(threshold), "A threshold is between 0 and 1.");
        if (!_indexOf.TryGetValue(verseNumber, out int current))
        {
            throw new ArgumentException("That verse is not counted under these options.", nameof(verseNumber));
        }
        if (method == SimilarityMethod.Values) ArgumentNullException.ThrowIfNull(wordValues);

        double least = threshold - Tolerance;
        var found = new List<VerseHit>();
        foreach (Verse verse in InScope(scope))
        {
            int target = _indexOf[verse.Number];
            VerseHit? hit = method switch
            {
                SimilarityMethod.Text => ByText(current, target, least),
                SimilarityMethod.Words => ByWords(current, target, least),
                SimilarityMethod.Roots => ByRoots(verseNumber, verse.Number, least),
                _ => ByValues(current, target, least, wordValues!),
            };
            if (hit is not null) found.Add(hit);
        }
        return found;
    }

    private IEnumerable<Verse> InScope(IReadOnlySet<int>? scope) =>
        scope is null ? _verses : _verses.Where(v => scope.Contains(v.Number));

    private VerseHit? ByText(int current, int target, double least)
    {
        string a = VerseText(current), b = VerseText(target);
        if (Similarity.UpperBound(a.Length, b.Length) < least) return null;
        double score = Similarity.Of(a, b);
        return score >= least ? new VerseHit(_verses[target].Number, [], HitWords.None, score) : null;
    }

    private VerseHit? ByWords(int current, int target, double least)
    {
        int a0 = _segmentation.VerseFirstWord[current], b0 = _segmentation.VerseFirstWord[target];
        int[]? pairs = Pair(
            _segmentation.VerseWordCount[current], _segmentation.VerseWordCount[target],
            (a, b) =>
            {
                string x = _wordTexts[a0 + a], y = _wordTexts[b0 + b];
                return Similarity.UpperBound(x.Length, y.Length) >= least && Similarity.Of(x, y) >= least;
            });
        return pairs is null ? null : new VerseHit(_verses[target].Number, pairs.Order().ToArray(), HitWords.Counted);
    }

    /// <summary>Counts words whose root equals a distinct word's root; roots compare ignoring case.</summary>
    private VerseHit? ByRoots(int current, int target, double least)
    {
        string[] a = Roots(current), b = Roots(target);
        List<int> matched = Count(a.Length, b.Length, (i, j) => string.Equals(a[i], b[j], StringComparison.OrdinalIgnoreCase));
        return matched.Count >= a.Length * least
            ? new VerseHit(target, matched.Order().ToArray(), HitWords.Display)
            : null;
    }

    private VerseHit? ByValues(int current, int target, double least, IReadOnlyList<long> values)
    {
        int a0 = _segmentation.VerseFirstWord[current], b0 = _segmentation.VerseFirstWord[target];
        int count = _segmentation.VerseWordCount[current];
        List<int> matched = Count(count, _segmentation.VerseWordCount[target], (i, j) => values[a0 + i] == values[b0 + j]);
        return matched.Count >= count * least
            ? new VerseHit(_verses[target].Number, matched.Order().ToArray(), HitWords.Counted)
            : null;
    }

    private string[] Roots(int verseNumber) =>
        Enumerable.Range(0, _roots.OfVerse(verseNumber).Count)
            .Select(w => _rootSearch.RootOf(verseNumber, w) ?? "")
            .ToArray();

    /// <summary>
    /// Greedy one-to-one pairing: each of the first list's items takes the
    /// first unused item of the second that fits. Returns the second list's
    /// paired indexes, or null when any item finds no partner.
    /// </summary>
    private static int[]? Pair(int first, int second, Func<int, int, bool> fits)
    {
        var used = new bool[second];
        var paired = new int[first];
        for (int a = 0; a < first; a++)
        {
            int partner = -1;
            for (int b = 0; b < second && partner < 0; b++)
            {
                if (!used[b] && fits(a, b)) partner = b;
            }
            if (partner < 0) return null;
            used[partner] = true;
            paired[a] = partner;
        }
        return paired;
    }

    /// <summary>The same greedy pairing, keeping the pairs that were found.</summary>
    private static List<int> Count(int first, int second, Func<int, int, bool> fits)
    {
        var used = new bool[second];
        var matched = new List<int>();
        for (int a = 0; a < first; a++)
        {
            for (int b = 0; b < second; b++)
            {
                if (used[b] || !fits(a, b)) continue;
                used[b] = true;
                matched.Add(b);
                break;
            }
        }
        return matched;
    }

    private string VerseText(int verse)
    {
        _verseTexts ??= new string[_segmentation.VerseCount];
        if (_verseTexts[verse] is { } cached) return cached;

        int first = _segmentation.VerseFirstWord[verse];
        var words = new string[_segmentation.VerseWordCount[verse]];
        for (int w = 0; w < words.Length; w++) words[w] = _wordTexts[first + w];
        return _verseTexts[verse] = string.Join(' ', words);
    }
}
