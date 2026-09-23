using QuranCode.Core.Search;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Phase 4: several terms, + and -, phrases, scope, roots and similar verses.</summary>
public sealed class SearchQueryTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);

    public void Dispose() => _engine.Dispose();

    private HashSet<int> Verses(string text, Grouping grouping = Grouping.Any, Wordness wordness = Wordness.WholeWord, IReadOnlySet<int>? scope = null) =>
        [.. _engine.Search().Find(new TextQuery(text, wordness, grouping, scope)).Verses];

    private int Abs(int chapter, int verse) => _engine.Verse(chapter, verse).Number;

    [Fact]
    public void AnyWordIsTheUnionAndAllWordsTheIntersection()
    {
        HashSet<int> allah = Verses("الله"), lord = Verses("رب");
        Assert.Equal(allah.Union(lord).Count(), Verses("الله رب").Count);
        Assert.Equal(allah.Intersect(lord).Count(), Verses("الله رب", Grouping.All).Count);
    }

    [Fact]
    public void PlusRequiresAndMinusExcludes()
    {
        HashSet<int> allah = Verses("الله"), mercy = Verses("الرحمن");
        Assert.Equal(allah.Except(mercy).Count(), Verses("الله -الرحمن").Count);
        Assert.Equal(allah.Intersect(mercy).Count(), Verses("الله +الرحمن").Count);
    }

    [Fact]
    public void APhraseNeedsItsWordsInOrder()
    {
        HashSet<int> phrase = Verses("الرحمن الرحيم", Grouping.Phrase);
        HashSet<int> reversed = Verses("الرحيم الرحمن", Grouping.Phrase);
        Assert.Contains(Abs(1, 3), phrase);
        Assert.Contains(Abs(2, 0), phrase);
        Assert.DoesNotContain(Abs(1, 3), reversed);
        Assert.True(phrase.IsSubsetOf(Verses("الرحمن الرحيم", Grouping.All)));
    }

    [Fact]
    public void ScopeLimitsTheSearch()
    {
        HashSet<int> chapter2 = [.. Enumerable.Range(_engine.Chapters[1].FirstVerse, _engine.Chapters[1].RowCount)];
        HashSet<int> found = Verses("الله", scope: chapter2);
        Assert.NotEmpty(found);
        Assert.True(found.IsSubsetOf(chapter2));
    }

    [Fact]
    public void TypedShaddaFollowsShaddaAsLetter()
    {
        var counting = new Text.CountingOptions { ShaddaAsLetter = true };
        TextSearch search = _engine.Search(counting: counting);
        Assert.True(search.Find("ٱلرَّحْمَٰنِ", Wordness.WholeWord).VerseCount > 100);
    }

    [Fact]
    public void RootSearchFindsEveryWordOfTheRoot()
    {
        RootSearchResult result = _engine.RootSearch().Find(new TextQuery("رحم"));
        Assert.Contains(new DisplayHit(1, 2), result.Words);
        Assert.Contains(new DisplayHit(1, 3), result.Words);
        Assert.Equal("رحم", result.Terms[0].Root);
    }

    [Fact]
    public void AWordResolvesToItsRoot()
    {
        RootSearch roots = _engine.RootSearch();
        Assert.Equal("رحمان", roots.BestRoot("الرحمن"));
        Assert.Equal("علم", roots.BestRoot("علم"));
        Assert.Null(roots.BestRoot("ثثثثث"));
    }

    [Fact]
    public void RelatedWordsShareTheLongestRoot()
    {
        RootSearchResult related = _engine.RootSearch().Related(1, 2); // ٱلرَّحْمَٰنِ: رحم|رحمان
        Assert.Equal("رحمان", related.Terms[0].Root);
        Assert.Contains(1, related.Verses);
        Assert.DoesNotContain(new DisplayHit(1, 3), related.Words); // ٱلرَّحِيمِ is رحيم, not رحمان
    }

    [Fact]
    public void TheRepeatedVerseOf55IsFoundAtEveryRepetition()
    {
        int verse = Abs(55, 13);
        VerseSimilarity similarity = _engine.Similarity();

        Assert.Equal(31, similarity.Similar(verse, SimilarityMethod.Text, 1.0).Count);
        Assert.Equal(31, similarity.Similar(verse, SimilarityMethod.Words, 1.0).Count);
        Assert.True(similarity.Similar(verse, SimilarityMethod.Values, 1.0, _engine.WordValues()).Count >= 31);
        Assert.True(similarity.Similar(verse, SimilarityMethod.Roots, 1.0).Count >= 31);
        Assert.True(similarity.Related(verse).Count >= 31);
    }

    [Fact]
    public void LowerThresholdsFindMore()
    {
        VerseSimilarity similarity = _engine.Similarity();
        int verse = Abs(2, 5);
        int strict = similarity.Similar(verse, SimilarityMethod.Text, 0.9).Count;
        int loose = similarity.Similar(verse, SimilarityMethod.Text, 0.6).Count;
        Assert.True(loose >= strict && strict >= 1, $"{strict} at 90%, {loose} at 60%");
    }

    [Fact]
    public void LevenshteinSimilarityMatchesTheDefinition()
    {
        Assert.Equal(3, Similarity.Distance("kitten", "sitting"));
        Assert.Equal(1 - 3.0 / 7, Similarity.Of("kitten", "sitting"), 6);
        Assert.Equal(1.0, Similarity.Of("", ""));
    }
}
