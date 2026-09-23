using QuranCode.Core;
using QuranCode.Core.Content;
using QuranCode.Core.Search;
using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Text, root, related and similar searches. Every search produces a list of
/// <see cref="VerseHit"/>s; one page of them is turned into display verses with
/// their words marked, and the whole list into per-chapter counts.
/// </summary>
internal sealed partial class Handlers
{
    /// <summary>The original's default for similar verses (F6).</summary>
    public const double DefaultSimilarity = 0.7;

    /// <remarks>
    /// Text that is not all Arabic letters, marks, digits and symbols searches
    /// the translations instead (Features.txt #51, #67, #73). An Arabic
    /// search that finds nothing tries the standard-spelling text (#52), and
    /// the result says where it was found.
    /// </remarks>
    public SearchResultDto Search(SearchParams p)
    {
        RequireText(p.Term, "term");
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        if (!TranslationSearch.IsArabic(p.Term)) return SearchTranslations(p, textMode, counting);

        var query = new TextQuery(p.Term, ParseWordness(p.Wordness), ParseGrouping(p.Grouping), Scope(p.Scope));

        SearchResult result = _engine.Search(textMode, counting).Find(query);
        Segmentation segmentation = _engine.Segmentation(textMode, counting);
        CorpusView view = _engine.View(counting);

        VerseHit[] withWords = result.Words
            .GroupBy(m => m.VerseNumber)
            .Select(g => new VerseHit(
                g.Key,
                g.Select(m => m.WordIndex - segmentation.VerseFirstWord[view.IndexOf(g.Key)]).ToArray(),
                HitWords.Counted))
            .ToArray();
        VerseHit[] hits = Merge(result.Verses, withWords);

        if (hits.Length == 0)
        {
            IReadOnlyList<int> spelled = _engine.EmlaaeiSearch(p.Term, query.Wordness, textMode, counting, query.Scope);
            if (spelled.Count > 0)
            {
                VerseHit[] whole = spelled.Select(v => new VerseHit(v, [], HitWords.None)).ToArray();
                return Page(result.Term, whole, p.Offset, p.Limit, textMode, counting) with { FoundIn = "emlaaei" };
            }
        }

        return Page(result.Term, hits, p.Offset, p.Limit, textMode, counting);
    }

    /// <summary>
    /// Search in translations: those asked for, or the edition's own
    /// translations and transliteration. A pack's translations are searched
    /// when asked for by name, so a search does not read all of them at once.
    /// </summary>
    private SearchResultDto SearchTranslations(SearchParams p, string textMode, CountingOptions counting)
    {
        IReadOnlyList<TranslationInfo> chosen = p.Translations is { Count: > 0 } keys
            ? keys.Select(k => _engine.Translation(k) ?? throw RpcException.NotFound($"There is no translation named \"{Truncate(k)}\".")).ToArray()
            : _engine.Translations.Where(t => t.Source == 0 && t.Kind is "translation" or "transliteration").ToArray();
        if (chosen.Count == 0) throw RpcException.NotFound("This edition has no translations to search.");

        CorpusView view = _engine.View(counting);
        HashSet<int>? scope = Scope(p.Scope);
        IReadOnlyList<TranslationHit> found = TranslationSearch.Find(
            p.Term, ParseWordness(p.Wordness),
            chosen.Select(t => (t.Key, _engine.AllTranslationText(t))).ToArray(), scope);
        TranslationHit[] counted = found.Where(h => view.IndexOf(h.VerseNumber) >= 0).ToArray();

        Dictionary<int, TranslationHit> byVerse = counted.ToDictionary(h => h.VerseNumber);
        VerseHit[] hits = counted.Select(h => new VerseHit(h.VerseNumber, [], HitWords.None)).ToArray();
        SearchResultDto page = Page(p.Term.Trim(), hits, p.Offset, p.Limit, textMode, counting);

        // Each verse carries the lines that matched, with where.
        SearchVerseDto[] verses = page.Verses.Select(v => v with
        {
            MatchCount = byVerse[v.Number].Matches.Sum(m => m.Ranges.Count),
            Translations = byVerse[v.Number].Matches
                .Select(m => new TranslationMatchDto(m.Key, m.Text, m.Ranges.Select(r => (IReadOnlyList<int>)[r.Start, r.Length]).ToArray()))
                .ToArray(),
        }).ToArray();
        return page with { Verses = verses, WordCount = counted.Sum(h => h.Matches.Sum(m => m.Ranges.Count)), FoundIn = "translations" };
    }

    public SearchResultDto SearchRoots(SearchParams p)
    {
        RequireText(p.Term, "term");
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        var query = new TextQuery(p.Term, Wordness.Any, ParseGrouping(p.Grouping), Scope(p.Scope));
        RootSearchResult result = _engine.RootSearch(counting).Find(query);
        return RootPage(result, p.Offset, p.Limit, textMode, counting);
    }

    public SearchResultDto SearchHarakat(SearchParams p)
    {
        RequireText(p.Term, "term");
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        IReadOnlyList<VerseHit> hits = HarakatSearch.Find(_engine.View(counting).Verses, p.Term, Scope(p.Scope));
        return Page(p.Term.Trim(), hits, p.Offset, p.Limit, textMode, counting);
    }

    public SearchResultDto SearchRelated(VerseSearchParams p)
    {
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        Verse verse = RequireCountedVerse(p.Verse, counting);
        int word = p.Word ?? throw RpcException.InvalidParams("word is required.");
        int words = DisplayWords.Split(verse.Text).Length;
        if (word < 0 || word >= words) throw RpcException.InvalidParams($"Verse {verse.ChapterNumber}:{verse.NumberInChapter} has words 0 to {words - 1}.");

        RootSearchResult result = _engine.RootSearch(counting).Related(verse.Number, word, Scope(p.Scope));
        return RootPage(result, p.Offset, p.Limit, textMode, counting);
    }

    public SearchResultDto SearchRelatedVerses(VerseSearchParams p)
    {
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        Verse verse = RequireCountedVerse(p.Verse, counting);
        IReadOnlyList<VerseHit> hits = _engine.Similarity(textMode, counting).Related(verse.Number, Scope(p.Scope));
        return Page(Reference(verse), hits, p.Offset, p.Limit, textMode, counting);
    }

    public SearchResultDto SearchSimilar(VerseSearchParams p)
    {
        (string textMode, CountingOptions counting) = SearchContext(p.ValueSystem, p.Counting);
        Verse verse = RequireCountedVerse(p.Verse, counting);
        double threshold = p.Threshold ?? DefaultSimilarity;
        if (threshold is < 0 or > 1 || double.IsNaN(threshold)) throw RpcException.InvalidParams("threshold must be between 0 and 1.");

        SimilarityMethod method = p.Method switch
        {
            null or "text" => SimilarityMethod.Text,
            "words" => SimilarityMethod.Words,
            "roots" => SimilarityMethod.Roots,
            "values" => SimilarityMethod.Values,
            _ => throw RpcException.InvalidParams("method must be text, words, roots or values."),
        };
        IReadOnlyList<long>? values = method == SimilarityMethod.Values
            ? _engine.WordValues(p.ValueSystem ?? QuranCodeEngine.DefaultValueSystem, counting)
            : null;

        IReadOnlyList<VerseHit> hits = _engine.Similarity(textMode, counting)
            .Similar(verse.Number, method, threshold, values, Scope(p.Scope));
        return Page(Reference(verse), hits, p.Offset, p.Limit, textMode, counting);
    }

    private SearchResultDto RootPage(RootSearchResult result, int? offset, int? limit, string textMode, CountingOptions counting)
    {
        VerseHit[] hits = result.Words
            .GroupBy(w => w.VerseNumber)
            .Select(g => new VerseHit(g.Key, g.Select(w => w.WordIndex).ToArray(), HitWords.Display))
            .ToArray();
        hits = Merge(result.Verses, hits);

        RootTermDto[] roots = result.Terms
            .Select(t => new RootTermDto(t.Term.Text, KindName(t.Term.Kind), t.Root))
            .ToArray();
        string term = string.Join(' ', result.Terms.Select(t => t.Root ?? t.Term.Text));
        return Page(term, hits, offset, limit, textMode, counting) with { Roots = roots };
    }

    /// <summary>Verses found with no marked words (a "-term" only search) still belong in the list.</summary>
    private static VerseHit[] Merge(IReadOnlyList<int> verses, IReadOnlyList<VerseHit> withWords)
    {
        Dictionary<int, VerseHit> byVerse = withWords.ToDictionary(h => h.VerseNumber);
        return verses.Select(v => byVerse.GetValueOrDefault(v) ?? new VerseHit(v, [], HitWords.None)).ToArray();
    }

    private SearchResultDto Page(
        string term, IReadOnlyList<VerseHit> hits, int? offset, int? limit, string textMode, CountingOptions counting)
    {
        int from = offset ?? 0;
        int take = limit ?? DefaultSearchLimit;
        if (from < 0) throw RpcException.InvalidParams("offset must not be negative.");
        if (take is < 1 or > MaxSearchLimit) throw RpcException.InvalidParams($"limit must be between 1 and {MaxSearchLimit}.");

        Segmentation segmentation = _engine.Segmentation(textMode, counting);
        CorpusView view = _engine.View(counting);
        CountingText text = _engine.CountingText(textMode, counting);

        var chapterCounts = new int[_engine.Chapters.Count];
        int wordCount = 0;
        foreach (VerseHit hit in hits)
        {
            wordCount += hit.Words.Count;
            chapterCounts[_engine.Verse(hit.VerseNumber).ChapterNumber - 1] += Math.Max(1, hit.Words.Count);
        }

        SearchVerseDto[] verses = hits.Skip(from).Take(take)
            .Select(hit => SearchVerse(hit, segmentation, view, text))
            .ToArray();
        int[] numbers = hits.Select(h => h.VerseNumber).ToArray();
        return new SearchResultDto(term, wordCount, hits.Count, from, verses, chapterCounts, numbers);
    }

    private SearchVerseDto SearchVerse(VerseHit hit, Segmentation segmentation, CorpusView view, CountingText text)
    {
        Verse verse = _engine.Verse(hit.VerseNumber);
        VerseDisplay display = _engine.Display(verse);
        int[] highlights, bismillah;
        bool aligned = true;

        switch (hit.Kind)
        {
            case HitWords.Counted:
                DisplaySpan[]? spans = DisplayWords.Align(display, segmentation.VerseWords(view.IndexOf(hit.VerseNumber)), text.NormalizeWord);

                // An unalignable verse gets no word highlights; the UI marks the
                // whole verse instead of pointing at the wrong words.
                aligned = spans is not null;
                highlights = spans is null
                    ? []
                    : hit.Words.SelectMany(w => Enumerable.Range(spans[w].First, spans[w].Count)).Distinct().Order().ToArray();
                bismillah = hit.Words.Where(w => w < display.WordOffset).Distinct().Order().ToArray();
                break;

            case HitWords.Display:
                // Display indexes count the Bismillah header an edition prefixes to verse 1.
                highlights = hit.Words.Where(w => w >= display.WordOffset).Select(w => w - display.WordOffset).Distinct().Order().ToArray();
                bismillah = hit.Words.Where(w => w < display.WordOffset).Distinct().Order().ToArray();
                break;

            default:
                highlights = [];
                bismillah = [];
                break;
        }

        return new SearchVerseDto(
            verse.Number, verse.ChapterNumber, verse.NumberInChapter, verse.IsBasmala,
            display.Bismillah, display.Words, highlights, aligned, bismillah, hit.Words.Count,
            hit.Score is double score ? Math.Round(score, 4) : null);
    }

    private (string TextMode, CountingOptions Counting) SearchContext(string? valueSystem, CountingDto? counting) =>
        (RequireSystem(valueSystem).TextMode, Counting(counting));

    private Verse RequireCountedVerse(int number, CountingOptions counting)
    {
        if (number < 1 || number > _engine.Verses.Count)
        {
            throw RpcException.InvalidParams($"Verses run from 1 to {_engine.Verses.Count}.");
        }
        if (_engine.View(counting).IndexOf(number) < 0)
        {
            throw RpcException.InvalidParams("That verse is a Bismillah the counting options leave out.");
        }
        return _engine.Verse(number);
    }

    /// <summary>The verses a search may look at, or null for the whole book.</summary>
    private HashSet<int>? Scope(ScopeDto? scope)
    {
        if (scope is null) return null;
        int count = _engine.Verses.Count;
        if (scope.Verses is { } list)
        {
            if (scope.First is not null || scope.Last is not null) throw RpcException.InvalidParams("scope takes a range or a list of verses, not both.");
            if (list.Count > count) throw RpcException.InvalidParams($"scope lists at most {count} verses.");
            if (list.Any(v => v < 1 || v > count)) throw RpcException.InvalidParams($"scope verses run from 1 to {count}.");
            return [.. list];
        }
        if (scope.First is int first && scope.Last is int last)
        {
            VerseRange range = RequireRange(first, last);
            return [.. Enumerable.Range(range.First, range.Last - range.First + 1)];
        }
        throw RpcException.InvalidParams("scope needs first and last, or verses.");
    }

    private static Grouping ParseGrouping(string? value) => value switch
    {
        null or "any" => Grouping.Any,
        "all" => Grouping.All,
        "phrase" => Grouping.Phrase,
        _ => throw RpcException.InvalidParams("grouping must be any, all or phrase."),
    };

    private static string KindName(TermKind kind) => kind switch
    {
        TermKind.Required => "required",
        TermKind.Excluded => "excluded",
        _ => "plain",
    };

    private static string Reference(Verse verse) => $"{verse.ChapterNumber}:{verse.NumberInChapter}";
}
