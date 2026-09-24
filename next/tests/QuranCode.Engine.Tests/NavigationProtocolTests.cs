using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Core.User;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>Phase 3 over the protocol: units, positions, chapters, distances, bookmarks and history.</summary>
public sealed class NavigationProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly string _userPath = Path.Combine(Path.GetTempPath(), $"qc-user-{Guid.NewGuid():N}.db");
    private readonly UserStore _store;
    private readonly Dispatcher _dispatcher;

    public NavigationProtocolTests()
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

    private (int, int) Range(string text)
    {
        JsonElement r = Result("reference.parse", new { text });
        return (r.GetProperty("first").GetInt32(), r.GetProperty("last").GetInt32());
    }

    private int Abs(int chapter, int verse) => _engine.Verse(chapter, verse).Number;

    [Fact]
    public void ParsesUnitsAndChapterRanges()
    {
        Assert.Equal((1, 7), Range("page 1"));
        Assert.Equal((Abs(3, 0), _engine.Chapters[3].LastVerse), Range("3-4"));
        Assert.Equal((Abs(3, 0), Abs(4, 19)), Range("3-4:19"));
        Assert.Equal((262, 262), Range("verse 262"));
        Assert.Equal((1, 1), Range("word 1"));
        Assert.Equal((7, 7), Range("letter 139")); // Al-Fatiha has 139 letters
        Assert.Equal(_engine.Chapters[113].LastVerse, Range("part 30").Item2);
        Assert.Equal("invalid_params", ErrorCode("reference.parse", new { text = "page 605" }));
    }

    [Fact]
    public void WordAndLetterUnitsFollowTheCountingOptions()
    {
        // Leaving out the Bismillahs shifts every later word number by 4 per chapter.
        JsonElement with = Result("reference.parse", new { text = "word 40" });
        JsonElement without = Result("reference.parse", new { text = "word 40", counting = new { includeBasmalas = false } });
        Assert.NotEqual(with.GetProperty("first").GetInt32(), without.GetProperty("first").GetInt32());
    }

    [Fact]
    public void StatsReportVersesBeforeAndAfter()
    {
        JsonElement position = Result("selection.stats", new { first = Abs(2, 255), last = Abs(2, 255) }).GetProperty("position");
        Assert.Equal(255, position.GetProperty("beforeInChapter").GetInt32()); // 2:0 to 2:254
        Assert.Equal(31, position.GetProperty("afterInChapter").GetInt32());
        Assert.Equal(Abs(2, 255) - 1, position.GetProperty("beforeInBook").GetInt32());

        JsonElement without = Result("selection.stats", new { first = Abs(2, 255), last = Abs(2, 255), counting = new { includeBasmalas = false } });
        Assert.Equal(254, without.GetProperty("position").GetProperty("beforeInChapter").GetInt32());
    }

    [Fact]
    public void ChapterStatsCoverEveryChapter()
    {
        JsonElement chapters = Result("chapters.stats");
        Assert.Equal(114, chapters.GetArrayLength());
        Assert.Equal("8317", chapters[0].GetProperty("value").GetString());
        Assert.Equal(29, chapters[0].GetProperty("words").GetInt32());
        Assert.Equal(287, chapters[1].GetProperty("verses").GetInt32());
    }

    [Fact]
    public void MeasuresDistanceBetweenWords()
    {
        // 1:1 word 0 (bism) to 1:2 word 0 (al-hamdu): 4 words, 19 letters, 1 verse.
        JsonElement d = Result("words.distance", new { from = new { verse = 1, word = 0 }, to = new { verse = 2, word = 0 } });
        Assert.Equal(0, d.GetProperty("chapters").GetInt32());
        Assert.Equal(1, d.GetProperty("verses").GetInt32());
        Assert.Equal(4, d.GetProperty("words").GetInt32());
        Assert.Equal(19, d.GetProperty("letters").GetInt32());

        Assert.Equal("not_found", ErrorCode("words.distance", new { from = new { verse = 1, word = 0 }, to = new { verse = 1, word = 99 } }));
    }

    [Fact]
    public void BookmarksRoundTripAsChapterAndVerse()
    {
        JsonElement saved = Result("bookmarks.save", new { first = Abs(2, 255), last = Abs(2, 255), note = "Ayat al-Kursi" });
        Assert.Equal("2:255", saved.GetProperty("reference").GetString());
        Assert.Equal(Abs(2, 255), saved.GetProperty("first").GetInt32());

        JsonElement list = Result("bookmarks.list");
        Assert.Equal(1, list.GetArrayLength());

        Assert.True(Result("bookmarks.delete", new { id = saved.GetProperty("id").GetInt64() }).GetBoolean());
        Assert.Equal(0, Result("bookmarks.list").GetArrayLength());
    }

    [Fact]
    public void RecordsBrowseAndFindHistory()
    {
        Result("history.add", new { kind = "browse", first = 1, last = 7 });
        Result("history.add", new { kind = "find", term = "الله", wordness = "any" });

        JsonElement browse = Result("history.list", new { kind = "browse" });
        Assert.Equal("1:1-7", browse[0].GetProperty("reference").GetString());
        Assert.Equal("الله", Result("history.list", new { kind = "find" })[0].GetProperty("term").GetString());

        Result("history.clear", new { kind = "find" });
        Assert.Equal(0, Result("history.list", new { kind = "find" }).GetArrayLength());
        Assert.Equal("invalid_params", ErrorCode("history.list", new { kind = "everything" }));
    }

    [Fact]
    public void TextModesAreSavedListedAndDeleted()
    {
        JsonElement listed = Result("textModes.list");
        Assert.Contains("Simplified29", listed.GetProperty("bases").EnumerateArray().Select(b => b.GetString()));
        Assert.Equal(0, listed.GetProperty("modes").GetArrayLength());

        var mode = new { name = "TaaAsHaa", @base = "Simplified30", rules = new[] { new { find = "ة", replace = "ه" } }, description = "" };
        Assert.Equal("TaaAsHaa", Result("textModes.save", mode).GetProperty("name").GetString());
        Assert.Equal("Simplified30", Result("textModes.list").GetProperty("modes")[0].GetProperty("base").GetString());
        Assert.Contains(Result("systems.list").EnumerateArray(), s => s.GetProperty("name").GetString()!.StartsWith("TaaAsHaa_", StringComparison.Ordinal));

        // A fresh engine over the same user file has it too.
        using var engine = new QuranCodeEngine(TestPaths.SubmissionDatabase);
        _ = new UserHandlers(engine, _store);
        Assert.True(engine.HasTextMode("TaaAsHaa"));

        Assert.True(Result("textModes.delete", new { name = "TaaAsHaa" }).GetBoolean());
        Assert.Equal(0, Result("textModes.list").GetProperty("modes").GetArrayLength());
        Assert.Empty(_store.TextModes());
    }

    [Fact]
    public void AnUnsoundTextModeIsRefused() =>
        Assert.Equal("invalid_params", ErrorCode("textModes.save",
            new { name = "Simplified29", @base = "Simplified29", rules = new[] { new { find = "ا", replace = "ا" } } }));

    [Fact]
    public void WithoutAUserFileTheMethodsSaySo()
    {
        var bare = new Dispatcher(new Handlers(_engine), new StringWriter());
        JsonElement root = JsonDocument.Parse(bare.Handle("""{"id":1,"method":"bookmarks.list"}""")).RootElement;
        Assert.Equal("unavailable", root.GetProperty("error").GetProperty("code").GetString());
    }
}
