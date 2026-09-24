using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Core.User;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>Values across systems, breakdowns, exact references and saved research selections.</summary>
public sealed class ResearchSelectionProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly string _userPath = Path.Combine(Path.GetTempPath(), $"qc-research-{Guid.NewGuid():N}.db");
    private readonly UserStore _store;
    private readonly Dispatcher _dispatcher;

    private const string Gematria = "Simplified29_Abjad_Gematria";

    public ResearchSelectionProtocolTests()
    {
        _store = new UserStore(_userPath);
        _dispatcher = new Dispatcher(new Handlers(_engine), new UserHandlers(_engine, _store), new StringWriter());
    }

    public void Dispose()
    {
        _store.Dispose();
        _engine.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (string file in new[] { _userPath, _userPath + "-wal", _userPath + "-shm" })
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }

    private JsonElement Call(string method, object? parameters = null) =>
        JsonDocument.Parse(_dispatcher.Handle(JsonSerializer.Serialize(new { id = 1, method, @params = parameters }))).RootElement;

    private JsonElement Result(string method, object? parameters = null)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("result", out JsonElement result), root.ToString());
        return result;
    }

    private string ErrorCode(string method, object? parameters = null) =>
        Call(method, parameters).GetProperty("error").GetProperty("code").GetString()!;

    private static readonly object Words = new
    {
        start = new { chapter = 1, verse = 1, word = 2 },
        end = new { chapter = 1, verse = 1, word = 3 },
    };

    [Fact]
    public void ValuesASelectionInTheGivenSystems()
    {
        JsonElement values = Result("selection.values", new { selection = Words, valueSystems = new[] { Gematria } });
        JsonElement analysis = Result("selection.analyze", new { selection = Words, valueSystem = Gematria });

        JsonElement only = Assert.Single(values.EnumerateArray());
        Assert.Equal(
            analysis.GetProperty("value").GetProperty("value").GetString(),
            only.GetProperty("value").GetProperty("value").GetString());
        Assert.Equal(JsonValueKind.Null, only.GetProperty("error").ValueKind);
    }

    [Fact]
    public void ValuesInEverySystemWhenNoneAreNamed()
    {
        JsonElement values = Result("selection.values", new { selection = Words });
        Assert.Equal(_engine.ValueSystems().Count, values.GetArrayLength());
    }

    [Fact]
    public void BreaksDownByWordAndPages()
    {
        JsonElement page = Result("selection.breakdown", new { selection = Words, by = "letter", valueSystem = Gematria, limit = 3 });

        Assert.Equal(10, page.GetProperty("rowCount").GetInt32()); // الله and الرحمن
        Assert.Equal(3, page.GetProperty("rows").GetArrayLength());
        JsonElement first = page.GetProperty("rows")[0];
        Assert.Equal("1:1:w2:l1", first.GetProperty("address").GetString());
        Assert.Equal("1", first.GetProperty("value").GetString());
    }

    [Theory]
    [InlineData("sideways", 10)]
    [InlineData("word", 0)]
    public void RejectsABadBreakdown(string by, int limit) =>
        Assert.Equal("invalid_params", ErrorCode("selection.breakdown", new { selection = Words, by, limit }));

    [Fact]
    public void RefusesALetterBreakdownOfAWholeChapter() =>
        Assert.Equal("invalid_params", ErrorCode("selection.breakdown", new
        {
            selection = new { start = new { chapter = 2 }, end = new { chapter = 2 } },
            by = "letter",
        }));

    [Fact]
    public void ParsesAnExactReference()
    {
        JsonElement range = Result("reference.parse", new { text = "2:255:w4-2:257:w8" });

        Assert.Equal(_engine.Verse(2, 255).Number, range.GetProperty("first").GetInt32());
        Assert.Equal(_engine.Verse(2, 257).Number, range.GetProperty("last").GetInt32());
        Assert.Equal(4, range.GetProperty("selection").GetProperty("start").GetProperty("word").GetInt32());
    }

    [Fact]
    public void AVerseReferenceHasNoSelection() =>
        Assert.Equal(JsonValueKind.Null, Result("reference.parse", new { text = "2:255" }).GetProperty("selection").ValueKind);

    [Fact]
    public void RejectsAnExactReferenceThatDoesNotExist() =>
        Assert.Equal("invalid_params", ErrorCode("reference.parse", new { text = "1:1:w9" }));

    [Fact]
    public void SavesListsAndDeletesResearchSelections()
    {
        JsonElement saved = Result("selections.save", new
        {
            selection = new { start = new { chapter = 2, verse = 257, word = 8 }, end = new { chapter = 2, verse = 255, word = 4, letter = 2 } },
            note = "backwards on purpose",
            valueSystem = Gematria,
            counting = new { includeBasmalas = false },
        });

        Assert.Equal("2:255:w4:l2-2:257:w8", saved.GetProperty("address").GetString());
        Assert.Equal("2:255:w4:l2-2:257:w8", saved.GetProperty("title").GetString());
        Assert.False(saved.GetProperty("counting").GetProperty("includeBasmalas").GetBoolean());

        JsonElement listed = Assert.Single(Result("selections.list").EnumerateArray());
        Assert.Equal(2, listed.GetProperty("selection").GetProperty("start").GetProperty("chapter").GetInt32());

        long id = saved.GetProperty("id").GetInt64();
        JsonElement renamed = Result("selections.save", new
        {
            id,
            title = "Throne verse",
            selection = new { start = new { chapter = 2, verse = 255 }, end = new { chapter = 2, verse = 255 } },
        });
        Assert.Equal("Throne verse", renamed.GetProperty("title").GetString());

        Assert.True(Result("selections.delete", new { id }).GetBoolean());
        Assert.Equal(0, Result("selections.list").GetArrayLength());
    }

    [Fact]
    public void RefusesToSaveWhatDoesNotExist()
    {
        Assert.Equal("invalid_params", ErrorCode("selections.save", new
        {
            selection = new { start = new { chapter = 1, verse = 9 }, end = new { chapter = 1, verse = 9 } },
        }));
        Assert.Equal("not_found", ErrorCode("selections.save", new
        {
            id = 999,
            selection = new { start = new { chapter = 1 }, end = new { chapter = 1 } },
        }));
    }
}
