using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Search;

/// <summary>A display word: its verse and its index among the verse's display words.</summary>
public readonly record struct DisplayHit(int VerseNumber, int WordIndex);

/// <summary>Words found by their roots.</summary>
public sealed class RootSearchResult
{
    /// <summary>The root each term resolved to, or null for a term with no root.</summary>
    public required IReadOnlyList<(Term Term, string? Root)> Terms { get; init; }
    public required IReadOnlyList<DisplayHit> Words { get; init; }
    public required IReadOnlyList<int> Verses { get; init; }
}

/// <summary>
/// Root search (Features.txt #53) and the same-root lookup behind Ctrl+Click
/// and F4 (#1, #39).
/// </summary>
/// <remarks>
/// Follows <c>Server.FindWords(roots)</c>: each term is a root, or is resolved
/// to its best root the way <c>Book.GetBestRoot</c> does (exact text, then the
/// same text after each simplification, then the root of a word spelled that
/// way, then the most similar root that contains the term). The legacy form
/// never parses <c>+</c> and <c>-</c>; they are implemented here as in text
/// search.
/// </remarks>
public sealed class RootSearch
{
    private static readonly Func<string, string>[] Simplifications =
    [
        ArabicNormalizer.Simplify36, ArabicNormalizer.Simplify31, ArabicNormalizer.Simplify30,
        ArabicNormalizer.Simplify29, ArabicNormalizer.Simplify28,
    ];

    private readonly WordRoots _roots;
    private readonly IReadOnlyList<Verse> _verses;
    private Dictionary<string, int>? _rootOfWordText;

    /// <param name="verses">The counted verses, in order; words outside them are never found.</param>
    public RootSearch(WordRoots roots, IReadOnlyList<Verse> verses)
    {
        ArgumentNullException.ThrowIfNull(roots);
        ArgumentNullException.ThrowIfNull(verses);
        _roots = roots;
        _verses = verses;
    }

    /// <summary>The longest root of a display word; the first listed wins a tie (legacy <c>Word.Root</c>).</summary>
    public string? RootOf(int verseNumber, int wordIndex)
    {
        IReadOnlyList<int> ids = _roots.Of(verseNumber, wordIndex);
        string? best = null;
        foreach (int id in ids)
        {
            string root = _roots.Text(id);
            if (best is null || root.Length > best.Length) best = root;
        }
        return best;
    }

    /// <summary>Resolves typed text to a root, or null.</summary>
    public string? BestRoot(string text)
    {
        string term = ArabicNormalizer.Simplify36(text).Trim();
        if (term.Length == 0) return null;
        if (_roots.IdOf(term) is not null) return term;

        foreach (Func<string, string> simplify in Simplifications)
        {
            string wanted = simplify(term);
            foreach (string root in _roots.All)
            {
                if (simplify(root) == wanted) return root;
            }
        }

        if (RootOfWordText().TryGetValue(ArabicNormalizer.Simplify29(term), out int byWord)) return _roots.Text(byWord);

        string? closest = null;
        double best = -1;
        string plain = ArabicNormalizer.Simplify29(term);
        foreach (string root in _roots.All)
        {
            string candidate = ArabicNormalizer.Simplify29(root);
            if (!candidate.Contains(plain, StringComparison.Ordinal)) continue;
            double similarity = Similarity.Of(candidate, plain);
            if (similarity > best)
            {
                best = similarity;
                closest = root;
            }
        }
        return closest;
    }

    /// <summary>Finds the words whose roots the query names.</summary>
    public RootSearchResult Find(TextQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        IReadOnlyList<Term> terms = query.Terms(t => ArabicNormalizer.Simplify36(t));
        var resolved = terms.Select(t => (Term: t, Root: BestRoot(t.Text))).ToArray();
        var ids = resolved.Select(r => r.Root is null ? (int?)null : _roots.IdOf(r.Root)).ToArray();

        var words = new List<DisplayHit>();
        var verses = new List<int>();
        bool anyPlain = resolved.Any(r => r.Term.Kind == TermKind.Plain);

        foreach (Verse verse in _verses)
        {
            if (query.Scope is not null && !query.Scope.Contains(verse.Number)) continue;
            IReadOnlyList<int[]> wordRoots = _roots.OfVerse(verse.Number);

            var hits = new SortedSet<int>();
            int plainFound = 0, plainTerms = 0;
            bool rejected = false;
            for (int t = 0; t < resolved.Length && !rejected; t++)
            {
                int? id = ids[t];
                bool found = false;
                for (int w = 0; w < wordRoots.Count; w++)
                {
                    if (id is null || Array.IndexOf(wordRoots[w], id.Value) < 0) continue;
                    found = true;
                    if (resolved[t].Term.Kind != TermKind.Excluded) hits.Add(w);
                }

                switch (resolved[t].Term.Kind)
                {
                    case TermKind.Excluded when found:
                    case TermKind.Required when !found:
                        rejected = true;
                        break;
                    case TermKind.Plain:
                        plainTerms++;
                        if (found) plainFound++;
                        break;
                }
            }
            if (rejected) continue;

            bool grouped = !anyPlain || (query.Grouping == Grouping.All ? plainFound == plainTerms : plainFound > 0);
            if (!grouped || (hits.Count == 0 && anyPlain)) continue;

            verses.Add(verse.Number);
            words.AddRange(hits.Select(w => new DisplayHit(verse.Number, w)));
        }

        return new RootSearchResult { Terms = resolved, Words = words, Verses = verses };
    }

    /// <summary>Words sharing the longest root of one word (Ctrl+Click, F4).</summary>
    public RootSearchResult Related(int verseNumber, int wordIndex, IReadOnlySet<int>? scope = null)
    {
        string? root = RootOf(verseNumber, wordIndex);
        return root is null
            ? new RootSearchResult { Terms = [], Words = [], Verses = [] }
            : Find(new TextQuery(root, Scope: scope));
    }

    /// <summary>The most frequent root among the words spelled each way.</summary>
    private Dictionary<string, int> RootOfWordText()
    {
        if (_rootOfWordText is not null) return _rootOfWordText;

        var counts = new Dictionary<string, Dictionary<int, int>>(StringComparer.Ordinal);
        foreach (Verse verse in _verses)
        {
            string[] words = DisplayWords.Split(verse.Text);
            IReadOnlyList<int[]> roots = _roots.OfVerse(verse.Number);
            for (int w = 0; w < words.Length && w < roots.Count; w++)
            {
                string key = ArabicNormalizer.Simplify29(words[w]).Replace(" ", "", StringComparison.Ordinal);
                if (!counts.TryGetValue(key, out Dictionary<int, int>? byRoot)) counts[key] = byRoot = [];
                int longest = roots[w].OrderByDescending(id => _roots.Text(id).Length).FirstOrDefault(-1);
                if (longest >= 0) byRoot[longest] = byRoot.GetValueOrDefault(longest) + 1;
            }
        }
        _rootOfWordText = counts
            .Where(pair => pair.Value.Count > 0)
            .ToDictionary(pair => pair.Key, pair => pair.Value.MaxBy(c => c.Value).Key, StringComparer.Ordinal);
        return _rootOfWordText;
    }
}
