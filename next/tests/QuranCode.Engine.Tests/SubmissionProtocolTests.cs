using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>The protocol over the Submission edition, verse 0 and its exclusion.</summary>
public sealed class SubmissionProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly Dispatcher _dispatcher;

    public SubmissionProtocolTests() => _dispatcher = new Dispatcher(new Handlers(_engine), new StringWriter());

    public void Dispose() => _engine.Dispose();

    private JsonElement Result(string method, object? parameters = null)
    {
        string response = _dispatcher.Handle(JsonSerializer.Serialize(new { id = 1, method, @params = parameters }));
        JsonElement root = JsonDocument.Parse(response).RootElement;
        Assert.True(root.TryGetProperty("result", out JsonElement result), response);
        return result;
    }

    private int Absolute(int chapter, int verse) => _engine.Verse(chapter, verse).Number;

    [Fact]
    public void InfoNamesTheEdition()
    {
        JsonElement info = Result("engine.info");
        Assert.Equal("submission", info.GetProperty("edition").GetString());
        Assert.Equal("verse-zero", info.GetProperty("basmala").GetString());
        Assert.Equal(6234, info.GetProperty("verseCount").GetInt32());
        Assert.Equal(6346, info.GetProperty("rowCount").GetInt32());
    }

    [Fact]
    public void ChapterVersesStartWithVerseZero()
    {
        JsonElement verses = Result("chapter.verses", new { chapter = 2 });
        Assert.Equal(287, verses.GetArrayLength());

        JsonElement zero = verses[0];
        Assert.Equal(0, zero.GetProperty("numberInChapter").GetInt32());
        Assert.True(zero.GetProperty("isBasmala").GetBoolean());
        Assert.Equal(JsonValueKind.Null, zero.GetProperty("bismillah").ValueKind);
        Assert.Equal(1, verses[1].GetProperty("numberInChapter").GetInt32());
    }

    [Fact]
    public void ExcludedBismillahHasNoValue()
    {
        JsonElement counted = Result("chapter.values", new { chapter = 2 });
        JsonElement excluded = Result("chapter.values", new { chapter = 2, includeBasmalas = false });

        Assert.Equal(JsonValueKind.String, counted[0].GetProperty("value").ValueKind);
        Assert.Equal(JsonValueKind.Null, excluded[0].GetProperty("value").ValueKind);
        Assert.Equal(counted[1].GetProperty("value").GetString(), excluded[1].GetProperty("value").GetString());
    }

    [Fact]
    public void StatsFollowTheBismillahChoice()
    {
        int first = Absolute(2, 0), last = Absolute(2, 286);
        JsonElement with = Result("selection.stats", new { first, last });
        JsonElement without = Result("selection.stats", new { first, last, includeBasmalas = false });

        Assert.Equal("287", with.GetProperty("verses").GetProperty("value").GetString());
        Assert.Equal("286", without.GetProperty("verses").GetProperty("value").GetString());
    }

    [Fact]
    public void NinetySixOneToFiveIsNineteenWords()
    {
        JsonElement stats = Result("selection.stats", new { first = Absolute(96, 1), last = Absolute(96, 5) });
        Assert.Equal("19", stats.GetProperty("words").GetProperty("value").GetString());
    }

    [Fact]
    public void SearchHighlightsVerseZeroAndSkipsItWhenExcluded()
    {
        JsonElement with = Result("search.text", new { term = "الرحمن", wordness = "whole", limit = 10 });
        JsonElement without = Result("search.text", new { term = "الرحمن", wordness = "whole", limit = 10, includeBasmalas = false });

        Assert.Equal(112, with.GetProperty("verseCount").GetInt32() - without.GetProperty("verseCount").GetInt32());

        JsonElement zero = with.GetProperty("verses").EnumerateArray().First(v => v.GetProperty("isBasmala").GetBoolean());
        Assert.Equal([2], zero.GetProperty("highlights").EnumerateArray().Select(h => h.GetInt32()));
    }
}
