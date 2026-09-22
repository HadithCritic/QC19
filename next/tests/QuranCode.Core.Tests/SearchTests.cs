using QuranCode.Core.Content;
using QuranCode.Core.Search;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Checks text search against results captured from the legacy engine.
/// </summary>
/// <remarks>
/// Both the counts and the exact verse sets are compared. Counts alone would let
/// a search that finds the right number of wrong verses pass.
/// </remarks>
public sealed class SearchTests : IDisposable
{
    private readonly ContentRepository _content;
    private readonly TextSearch _search;

    public SearchTests()
    {
        _content = new ContentRepository(TestPaths.ContentDatabase);
        var pipeline = new TextPipeline(_content.GetTextMode("Original"));
        Segmentation segmentation = Segmentation.Build(_content.Verses, pipeline);
        _search = new TextSearch(segmentation, _content.Verses, pipeline);
    }

    public void Dispose() => _content.Dispose();

    private static Wordness ParseWordness(string value) => value switch
    {
        "WholeWord" => Wordness.WholeWord,
        "PartOfWord" => Wordness.PartOfWord,
        _ => Wordness.Any,
    };

    [Fact]
    public void SearchCountsMatchLegacy()
    {
        var mismatches = new List<string>();

        foreach (string[] row in TestPaths.ReadRows("search-summary.tsv"))
        {
            string name = row[0];
            string term = row[1];
            Wordness wordness = ParseWordness(row[2]);
            int expectedWords = int.Parse(row[3]);
            int expectedVerses = int.Parse(row[4]);

            SearchResult result = _search.Find(term, wordness);

            if (result.WordCount != expectedWords)
            {
                mismatches.Add($"{name}: words expected {expectedWords}, got {result.WordCount}");
            }
            if (result.VerseCount != expectedVerses)
            {
                mismatches.Add($"{name}: verses expected {expectedVerses}, got {result.VerseCount}");
            }
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} search count mismatches:\n  " +
            string.Join("\n  ", mismatches.Take(20)));
    }

    [Fact]
    public void SearchVerseSetsMatchLegacy()
    {
        // Group the golden verse list by query.
        var expected = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        foreach (string[] row in TestPaths.ReadRows("search-verses.tsv"))
        {
            if (!expected.TryGetValue(row[0], out List<int>? list))
            {
                list = [];
                expected[row[0]] = list;
            }
            list.Add(int.Parse(row[1]));
        }

        var options = new Dictionary<string, (string Term, Wordness Wordness)>(StringComparer.Ordinal);
        foreach (string[] row in TestPaths.ReadRows("search-summary.tsv"))
        {
            options[row[0]] = (row[1], ParseWordness(row[2]));
        }

        var mismatches = new List<string>();

        foreach ((string name, List<int> goldenVerses) in expected)
        {
            (string term, Wordness wordness) = options[name];
            SearchResult result = _search.Find(term, wordness);

            var actual = result.Verses.ToList();
            if (actual.SequenceEqual(goldenVerses)) continue;

            var missing = goldenVerses.Except(actual).Take(5).ToList();
            var extra = actual.Except(goldenVerses).Take(5).ToList();
            mismatches.Add(
                $"{name}: {goldenVerses.Count} expected, {actual.Count} actual" +
                (missing.Count > 0 ? $"; missing {string.Join(",", missing)}" : "") +
                (extra.Count > 0 ? $"; extra {string.Join(",", extra)}" : ""));
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} search verse-set mismatches:\n  " +
            string.Join("\n  ", mismatches.Take(10)));
    }

    [Fact]
    public void EmptyTermFindsNothing()
    {
        Assert.Equal(0, _search.Find("").WordCount);
        Assert.Equal(0, _search.Find("   ").WordCount);
    }

    /// <summary>
    /// A raw term with diacritics must find the same words as its normalized
    /// form, because the search normalizes the term the way the corpus was.
    /// </summary>
    [Fact]
    public void RawAndNormalizedTermsAgree()
    {
        SearchResult raw = _search.Find("ٱللَّه");
        SearchResult normalized = _search.Find("الله");

        Assert.Equal(normalized.WordCount, raw.WordCount);
        Assert.Equal(normalized.VerseCount, raw.VerseCount);
    }
}
