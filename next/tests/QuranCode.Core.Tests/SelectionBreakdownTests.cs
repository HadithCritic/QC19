using QuranCode.Core.Content;
using QuranCode.Core.Text;
using QuranCode.Core.User;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Breakdowns, values across systems, and saved research selections.</summary>
[Collection(SharedEngine.Collection)]
public sealed class SelectionBreakdownTests
{
    private static QuranCodeEngine Engine => SharedEngine.Instance;

    private const string Gematria = "Simplified29_Abjad_Gematria";

    [Theory]
    [InlineData("1:1:w2:l2-1:3", BreakdownUnit.Verse)]
    [InlineData("1:1:w2:l2-1:3", BreakdownUnit.Word)]
    [InlineData("1:1:w2:l2-1:3", BreakdownUnit.Letter)]
    [InlineData("1:7:w9-2:1:w1", BreakdownUnit.Word)]   // crosses the header of 2:1
    [InlineData("1:7:w9-2:1:w1", BreakdownUnit.Letter)]
    [InlineData("2:255", BreakdownUnit.Word)]
    [InlineData("1", BreakdownUnit.Letter)]
    public void RowsAddUpToTheSelection(string address, BreakdownUnit unit)
    {
        QuranSelection selection = SelectionResolverTests.Parse(address);
        SelectionBreakdown breakdown = Engine.Breakdown(selection, unit, Gematria);
        SelectionAnalysis analysis = Engine.Analyze(selection, Gematria);

        Assert.NotEmpty(breakdown.Rows);
        Assert.Equal(analysis.Statistics!.LetterCount, breakdown.Rows.Sum(r => r.Letters));
        Assert.Equal(analysis.Statistics.Value, breakdown.Rows.Sum(r => r.Value));
    }

    [Fact]
    public void WordRowsAreDisplayWordsWithAddresses()
    {
        SelectionBreakdown breakdown = Engine.Breakdown(SelectionResolverTests.Parse("1:1:w2-1:1:w3"), BreakdownUnit.Word, Gematria);

        Assert.Equal(2, breakdown.Rows.Count);
        Assert.Equal(new QuranLocation(1, 1, 2), breakdown.Rows[0].Location);
        Assert.Equal(66, breakdown.Rows[0].Value); // الله
        Assert.Equal(4, breakdown.Rows[0].Letters);
    }

    [Fact]
    public void LetterRowsShowEachLetter()
    {
        SelectionBreakdown breakdown = Engine.Breakdown(SelectionResolverTests.Parse("1:1:w2"), BreakdownUnit.Letter, Gematria);

        Assert.Equal([1L, 30L, 30L, 5L], breakdown.Rows.Select(r => r.Value));
        Assert.Equal(new QuranLocation(1, 1, 2, 3), breakdown.Rows[2].Location);
    }

    [Fact]
    public void AnUncountedSelectionHasNoRows()
    {
        SelectionBreakdown breakdown = SubmissionEngine.Instance.Breakdown(
            SelectionResolverTests.Parse("2:0"), BreakdownUnit.Word, counting: new CountingOptions { IncludeBasmalas = false });
        Assert.Empty(breakdown.Rows);
    }

    [Fact]
    public void ValuesAcrossSystemsMatchTheAnalysis()
    {
        QuranSelection selection = SelectionResolverTests.Parse("2:255:w4:l2-2:257:w8:l3");
        string[] systems = [Gematria, "Simplified29_Alphabet_Primes1", QuranCodeEngine.DefaultValueSystem];

        IReadOnlyList<SystemSelectionValue> values = Engine.Values(selection, systems);

        Assert.Equal(systems, values.Select(v => v.ValueSystem));
        foreach (SystemSelectionValue value in values)
        {
            Assert.Null(value.Error);
            Assert.Equal(Engine.Analyze(selection, value.ValueSystem).Statistics!.Value, value.Value);
        }
    }

    [Fact]
    public void ALetterIsItsTextWithMarks()
    {
        Assert.Equal("لَّ", DisplayLetters.Letter("ٱللَّهِ", 3));
        Assert.Equal("ٱ", DisplayLetters.Letter("ٱللَّهِ", 1));
    }
}

[Collection(SubmissionEngine.Collection)]
public sealed class SubmissionBreakdownTests
{
    [Fact]
    public void TheJoinedWordsOf96_5AreTwoRows()
    {
        SelectionBreakdown breakdown = SubmissionEngine.Instance.Breakdown(
            SelectionResolverTests.Parse("96:5:w3-96:5:w4"), BreakdownUnit.Word);

        Assert.Equal([3, 4], breakdown.Rows.Select(r => r.Location.Word!.Value));
        Assert.All(breakdown.Rows, r => Assert.Equal(2, r.Letters));
    }
}

public sealed class ResearchSelectionStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"qurancode-research-{Guid.NewGuid():N}.db");
    private readonly UserStore _store;

    public ResearchSelectionStoreTests() => _store = new UserStore(_path);

    public void Dispose()
    {
        _store.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (string file in new[] { _path, _path + "-wal", _path + "-shm" }) File.Delete(file);
    }

    [Fact]
    public void SavesListsUpdatesAndDeletes()
    {
        var counting = new CountingOptions { IncludeBasmalas = false, WawAsWord = true };
        ResearchSelection saved = _store.SaveResearchSelection(
            null, "Throne verse", "positional test", "2:255:w4:l2-2:257:w8:l3", "Simplified29_Abjad_Gematria", counting)!;

        ResearchSelection listed = Assert.Single(_store.ResearchSelections());
        Assert.Equal(saved.Id, listed.Id);
        Assert.Equal("2:255:w4:l2-2:257:w8:l3", listed.Address);
        Assert.Equal(counting, listed.Counting);

        ResearchSelection? updated = _store.SaveResearchSelection(saved.Id, "Renamed", "", "2:255", null, null);
        Assert.Equal("Renamed", updated!.Title);
        Assert.Null(Assert.Single(_store.ResearchSelections()).Counting);

        Assert.True(_store.DeleteResearchSelection(saved.Id));
        Assert.Empty(_store.ResearchSelections());
    }

    [Fact]
    public void UpdatingAMissingSelectionGivesNull() =>
        Assert.Null(_store.SaveResearchSelection(999, "x", "", "1", null, null));

    [Theory]
    [InlineData("2:255:w4-5")]
    [InlineData("")]
    public void RefusesABadAddress(string address) =>
        Assert.Throws<ArgumentException>(() => _store.SaveResearchSelection(null, "x", "", address, null, null));

    [Fact]
    public void SurvivesReopening()
    {
        _store.SaveResearchSelection(null, "Kept", "", "96:1-96:5", null, null);
        using var reopened = new UserStore(_path);
        Assert.Equal("Kept", Assert.Single(reopened.ResearchSelections()).Title);
    }
}
