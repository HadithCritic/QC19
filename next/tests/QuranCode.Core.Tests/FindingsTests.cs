using System.Text;
using Microsoft.Data.Sqlite;
using QuranCode.Core.Code19;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Every published Code 19 result in the catalog, checked against the text.
/// A finding that stops reproducing fails the build; ADR 0004 forbids editing
/// the expected number to agree.
/// </summary>
public sealed class FindingsTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);

    public void Dispose()
    {
        _engine.Dispose();
        SqliteConnection.ClearAllPools();
    }

    public static TheoryData<string> Ids()
    {
        var data = new TheoryData<string>();
        foreach (Finding finding in FindingCatalog.All) data.Add(finding.Id);
        return data;
    }

    [Theory]
    [MemberData(nameof(Ids))]
    public void EachFindingReproduces(string id)
    {
        Finding finding = FindingCatalog.All.Single(f => f.Id == id);
        FindingResult result = FindingEvaluator.Evaluate(_engine, finding);
        Assert.True(
            result.Holds,
            $"{finding.Id}: computed {result.Computed}, published {finding.Expected}. " +
            $"Rule ({finding.Basis}): {finding.Rule} Source: {finding.Source}");
    }

    [Fact]
    public void TheCatalogIsWellFormed()
    {
        Assert.NotEmpty(FindingCatalog.All);
        foreach (Finding f in FindingCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(f.Claim), $"{f.Id} has no claim");
            Assert.False(string.IsNullOrWhiteSpace(f.Rule), $"{f.Id} has no rule");
            Assert.False(string.IsNullOrWhiteSpace(f.Source), $"{f.Id} has no source");
            Assert.True(f.Expected > 0, $"{f.Id} expects {f.Expected}");
        }
    }

    /// <summary>
    /// The two Appendix 1 totals are the ones the Allah rule was derived from,
    /// so they are also the ones that would catch a change to it. Both are
    /// multiples of 19, which is the claim being checked.
    /// </summary>
    [Fact]
    public void TheAppendixOneTotalsAreMultiplesOf19()
    {
        FindingResult count = Evaluate("allah-count");
        Assert.Equal(2698, count.Computed);
        Assert.Equal(142, count.Multiple);

        FindingResult sum = Evaluate("allah-verse-number-sum");
        Assert.Equal(118123, sum.Computed);
        Assert.Equal(6217, sum.Multiple);
    }

    /// <summary>
    /// The rule matches whole words only. A substring rule gives 2,726, which
    /// is not a multiple of 19: 28 extra occurrences across 16 word types,
    /// none of them the name. اللهم is among them, which is why it has to be
    /// excluded explicitly rather than by spelling.
    /// </summary>
    [Fact]
    public void ASubstringRuleWouldOverCount()
    {
        var counting = new Core.Text.CountingOptions { IncludeBasmalas = false };
        Core.Content.Segmentation segmentation = _engine.Segmentation("Simplified29", counting);
        IReadOnlySet<string> forms = WordForms.Named(WordForms.Allah)!;

        int substring = 0;
        var extraTypes = new SortedSet<string>(StringComparer.Ordinal);
        for (int w = 0; w < segmentation.WordCount; w++)
        {
            string word = segmentation.WordText(w);
            if (!forms.Any(form => word.Contains(form, StringComparison.Ordinal))) continue;
            substring++;
            if (!forms.Contains(word)) extraTypes.Add(word);
        }

        Assert.Equal(2726, substring);
        Assert.Equal(28, substring - 2698);
        Assert.NotEqual(0, substring % 19);
        Assert.Equal(16, extraTypes.Count);
        Assert.Contains("اللهم", extraTypes);
        Assert.Contains("ظلله", extraTypes);   // shadow, not the name
        Assert.Contains("اللهب", extraTypes);  // the flame
    }

    /// <summary>
    /// The convention belongs to the finding (ADR 0004 §7). Counting the 112
    /// unnumbered Basmalahs changes the Appendix 1 totals, so a global setting
    /// could not serve both this finding and one that needs them.
    /// </summary>
    [Fact]
    public void TheBasmalahConventionChangesTheResult()
    {
        Finding stated = FindingCatalog.All.Single(f => f.Id == "allah-count");
        Assert.False(stated.IncludeBasmalas);

        FindingResult withThem = FindingEvaluator.Evaluate(_engine, stated with { IncludeBasmalas = true });
        Assert.NotEqual(2698, withThem.Computed);
    }

    /// <summary>An inferred rule must say so, so a reader can see what rests on an assumption.</summary>
    [Fact]
    public void InferredRulesAreMarked()
    {
        Assert.Equal(RuleBasis.Inferred, FindingCatalog.All.Single(f => f.Id == "allah-count").Basis);
        Assert.Equal(RuleBasis.Stated, FindingCatalog.All.Single(f => f.Id == "qaf-in-chapter-50").Basis);
    }

    [Fact]
    public void AnUnknownFormSetIsRejected()
    {
        Finding bad = FindingCatalog.All.First(f => f.Measure == FindingMeasure.WordFormOccurrences)
            with { Match = "nobody" };
        Assert.Throws<ArgumentException>(() => FindingEvaluator.Evaluate(_engine, bad));
    }

    [Fact]
    public void AMalformedCatalogRowIsRejected()
    {
        Assert.Throws<InvalidDataException>(() => Read("too\tfew\tfields"));
        Assert.Throws<InvalidDataException>(() =>
            Read("id\tclaim\t1\tnosuchmeasure\tbook\tallah\tno\tOriginal\tstated\trule\tsource"));
        Assert.Throws<InvalidDataException>(() =>
            Read("id\tclaim\t1\twords\tbook\t \tmaybe\tOriginal\tstated\trule\tsource"));
    }

    private static IReadOnlyList<Finding> Read(string line) =>
        FindingCatalog.Read(new MemoryStream(Encoding.UTF8.GetBytes(line)));

    private FindingResult Evaluate(string id) =>
        FindingEvaluator.Evaluate(_engine, FindingCatalog.All.Single(f => f.Id == id));
}
