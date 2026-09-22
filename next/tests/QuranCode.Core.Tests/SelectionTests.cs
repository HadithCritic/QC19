using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numbers;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// References, selection statistics and number analysis. Structural figures are
/// the ones the legacy Readme publishes.
/// </summary>
[Collection(SharedEngine.Collection)]
public sealed class SelectionTests
{
    [Theory]
    [InlineData("1", 1, 7)]
    [InlineData("2:255", 262, 262)]
    [InlineData(" 2 : 255 ", 262, 262)]
    [InlineData("2:255-257", 262, 264)]
    [InlineData("1:7-2:2", 7, 9)]
    [InlineData("114", 6231, 6236)]
    [InlineData("١١٤", 6231, 6236)] // Arabic-Indic digits
    public void ParsesReferences(string text, int first, int last)
    {
        ReferenceParseResult result = ReferenceParser.Parse(text, Structure());
        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(new VerseRange(first, last), result.Range);
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData("0", "chapter")]
    [InlineData("115", "chapter")]
    [InlineData("1:8", "verse")]
    [InlineData("2:0", "verse")]
    [InlineData("2:10-5", "before")]
    [InlineData("abc", "format")]
    [InlineData("1:2:3", "format")]
    public void RejectsBadReferencesWithAReason(string text, string reasonFragment)
    {
        ReferenceParseResult result = ReferenceParser.Parse(text, Structure());
        Assert.False(result.IsSuccess);
        Assert.Contains(reasonFragment, result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AlFatihaIsSevenVersesTwentyNineWordsAndOneThirtyNineLetters()
    {
        SelectionStatistics stats = Engine().Statistics(new VerseRange(1, 7));

        Assert.Equal(1, stats.ChapterCount);
        Assert.Equal(7, stats.VerseCount);
        Assert.Equal(29, stats.WordCount);
        Assert.Equal(139, stats.LetterCount);
        Assert.Equal(8317, stats.Value);
    }

    [Fact]
    public void FirstAndLastSevenVersesAreTwentyNineWords()
    {
        QuranCodeEngine engine = Engine();
        Assert.Equal(29, engine.Statistics(new VerseRange(1, 7)).WordCount);
        Assert.Equal(29, engine.Statistics(new VerseRange(6230, 6236)).WordCount);
    }

    [Fact]
    public void WholeBookMatchesTheCorpusTotals()
    {
        SelectionStatistics stats = Engine().Statistics(new VerseRange(1, 6236));

        Assert.Equal(114, stats.ChapterCount);
        Assert.Equal(6236, stats.VerseCount);
        Assert.Equal(77878, stats.WordCount);
        Assert.Equal(327792, stats.LetterCount);
        Assert.Equal(19628315, stats.Value);
    }

    [Fact]
    public void LetterFrequenciesSumToTheLetterCount()
    {
        SelectionStatistics stats = Engine().Statistics(new VerseRange(1, 7));

        Assert.Equal(stats.LetterCount, stats.LetterFrequencies.Sum(f => f.Count));
        Assert.Equal(stats.DistinctLetterCount, stats.LetterFrequencies.Count);
        Assert.True(stats.LetterFrequencies.Zip(stats.LetterFrequencies.Skip(1)).All(p => p.First.Count >= p.Second.Count));
    }

    [Fact]
    public void SingleVerseUsesTheSingleVersePath()
    {
        // The single-verse and aggregate paths differ in the legacy engine; a
        // one-verse selection must agree with ValueOfVerse, not with the range.
        QuranCodeEngine engine = Engine();
        Assert.Equal(engine.ValueOfVerse(262), engine.Statistics(new VerseRange(262, 262)).Value);
    }

    [Fact]
    public void WholeChapterUsesTheChapterPath()
    {
        QuranCodeEngine engine = Engine();
        Assert.Equal(engine.ValueOfChapter(112), engine.Statistics(new VerseRange(6222, 6225)).Value);
    }

    [Fact]
    public void AnalyzesAlFatihaValue()
    {
        NumberAnalysis a = NumberAnalysis.Of(8317);

        Assert.Equal(NumberClass.AdditivePrime, a.Class);
        Assert.Equal("AP", a.Code);
        Assert.Equal(19, a.DigitSum);
        Assert.Equal(1, a.DigitalRoot);
        Assert.Equal([8317L], a.Factors);
        Assert.Equal(NumberTheory.Ordinal(8317, NumberClass.Prime), a.FamilyOrdinal);
        Assert.Equal(NumberTheory.Ordinal(8317, NumberClass.AdditivePrime), a.ClassOrdinal);
    }

    [Fact]
    public void AnalyzesCompositeWithFactors()
    {
        NumberAnalysis a = NumberAnalysis.Of(114);
        Assert.Equal(NumberClass.AdditiveComposite, a.Class);
        Assert.Equal([2L, 3L, 19L], a.Factors);
        Assert.Equal(114 - 1 - 30, a.FamilyOrdinal); // 30 primes up to 114
    }

    [Fact]
    public void AnalysisOfZeroAndNegatives()
    {
        Assert.Equal(NumberClass.None, NumberAnalysis.Of(0).Class);
        Assert.Empty(NumberAnalysis.Of(0).Factors!);

        NumberAnalysis negative = NumberAnalysis.Of(-114);
        Assert.Equal(NumberClass.AdditiveComposite, negative.Class);
        Assert.Equal([2L, 3L, 19L], negative.Factors);
    }

    [Fact]
    public void HugeValuesClassifyWithoutOrdinals()
    {
        NumberAnalysis a = NumberAnalysis.Of(long.MaxValue);
        Assert.Null(a.FamilyOrdinal);
        Assert.Null(a.ClassOrdinal);
        Assert.Null(a.Factors); // too large to factor promptly
        Assert.NotEqual(NumberClass.None, a.Class);
    }

    private static QuranCodeEngine Engine() => SharedEngine.Instance;

    private static IReadOnlyList<Chapter> Structure() => SharedEngine.Instance.Chapters;
}

/// <summary>
/// One engine for the whole test run; building segmentations is the slow part.
/// The engine's caches are not thread-safe, so every user shares one xUnit
/// collection and runs serially.
/// </summary>
internal static class SharedEngine
{
    public const string Collection = "shared-engine";

    public static readonly QuranCodeEngine Instance = new(TestPaths.ContentDatabase);
}

[Collection(SharedEngine.Collection)]
public sealed class ValueSystemCatalogTests
{
    [Fact]
    public void ResearchOnlyFollowsTheLegacyExtraSystemsList()
    {
        IReadOnlyList<Content.ValueSystemSummary> systems = SharedEngine.Instance.ValueSystemSummaries();

        Assert.Equal(407, systems.Count);
        Assert.False(systems.Single(s => s.Name == QuranCodeEngine.DefaultValueSystem).ResearchOnly);
        Assert.All(systems.Where(s => s.Name.Contains("RamanujanPrimes")), s => Assert.True(s.ResearchOnly));
        Assert.All(systems.Where(s => s.TextMode == "SimplifiedMarks"), s => Assert.True(s.ResearchOnly));
        Assert.All(systems, s => Assert.Equal(s.Name, $"{s.TextMode}_{s.LetterOrder}_{s.LetterValue}"));
    }
}
