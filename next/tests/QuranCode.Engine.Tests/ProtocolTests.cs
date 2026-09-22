using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>
/// The wire contract, exercised line in, line out, exactly as the Rust bridge
/// sees it.
/// </summary>
public sealed class ProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.ContentDatabase);
    private readonly StringWriter _log = new();
    private readonly Dispatcher _dispatcher;

    public ProtocolTests() => _dispatcher = new Dispatcher(new Handlers(_engine), _log);

    public void Dispose() => _engine.Dispose();

    private JsonElement Call(string method, object? parameters = null, long id = 1)
    {
        string request = JsonSerializer.Serialize(new { id, method, @params = parameters });
        string response = _dispatcher.Handle(request);
        Assert.DoesNotContain('\n', response); // one response per line, always

        JsonElement root = JsonDocument.Parse(response).RootElement;
        Assert.Equal(id, root.GetProperty("id").GetInt64());
        return root;
    }

    private JsonElement Result(string method, object? parameters = null)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("result", out JsonElement result), root.ToString());
        return result;
    }

    private (string Code, string Message) Error(string method, object? parameters = null)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("error", out JsonElement error), root.ToString());
        return (error.GetProperty("code").GetString()!, error.GetProperty("message").GetString()!);
    }

    [Fact]
    public void InfoDescribesTheCorpus()
    {
        JsonElement info = Result("engine.info");
        Assert.Equal(114, info.GetProperty("chapterCount").GetInt32());
        Assert.Equal(6236, info.GetProperty("verseCount").GetInt32());
        Assert.Equal(QuranCodeEngine.DefaultValueSystem, info.GetProperty("defaultValueSystem").GetString());
    }

    [Fact]
    public void ListsChaptersAndSystems()
    {
        Assert.Equal(114, Result("chapters.list").GetArrayLength());

        JsonElement systems = Result("systems.list");
        Assert.Equal(407, systems.GetArrayLength());
        Assert.Contains(systems.EnumerateArray(), s => s.GetProperty("researchOnly").GetBoolean());
    }

    [Fact]
    public void ChapterVersesCarryTheBismillahSeparately()
    {
        JsonElement verses = Result("chapter.verses", new { chapter = 2 });
        Assert.Equal(286, verses.GetArrayLength());

        JsonElement first = verses[0];
        Assert.Equal(JsonValueKind.String, first.GetProperty("bismillah").ValueKind);
        Assert.Equal(1, first.GetProperty("words").GetArrayLength()); // الٓمٓ
        Assert.Equal(JsonValueKind.Null, verses[1].GetProperty("bismillah").ValueKind);
    }

    [Fact]
    public void ChapterValuesMatchTheSingleVersePath()
    {
        JsonElement values = Result("chapter.values", new { chapter = 1 });
        Assert.Equal(7, values.GetArrayLength());

        long sum = values.EnumerateArray().Sum(v => long.Parse(v.GetProperty("value").GetString()!));
        Assert.Equal(8317, sum); // under the default system the verses sum to the chapter
        Assert.Equal(_engine.ValueOfVerse(1).ToString(), values[0].GetProperty("value").GetString());
    }

    [Fact]
    public void StatsForAlFatiha()
    {
        JsonElement stats = Result("selection.stats", new { first = 1, last = 7 });

        Assert.Equal("7", stats.GetProperty("verses").GetProperty("value").GetString());
        Assert.Equal("29", stats.GetProperty("words").GetProperty("value").GetString());
        Assert.Equal("139", stats.GetProperty("letters").GetProperty("value").GetString());

        JsonElement value = stats.GetProperty("value");
        Assert.Equal("8317", value.GetProperty("value").GetString());
        Assert.Equal("AP", value.GetProperty("code").GetString());
        Assert.Equal(19, value.GetProperty("digitSum").GetInt64());
    }

    [Fact]
    public void ParsesReferences()
    {
        JsonElement range = Result("reference.parse", new { text = "2:255" });
        Assert.Equal(262, range.GetProperty("first").GetInt32());
        Assert.Equal(262, range.GetProperty("last").GetInt32());

        (string code, string message) = Error("reference.parse", new { text = "2:300" });
        Assert.Equal("invalid_params", code);
        Assert.Contains("286", message);
    }

    [Fact]
    public void AnalyzesNumbersIncludingOnesBeyondJavaScriptPrecision()
    {
        JsonElement n = Result("number.analyze", new { value = "619" });
        Assert.Equal("XP", n.GetProperty("code").GetString()); // digit sum 16
        Assert.Equal(114, n.GetProperty("familyOrdinal").GetInt64());

        // 2^53 + 1 would round in a JS number; the value travels as a string.
        JsonElement big = Result("number.analyze", new { value = "9007199254740993" });
        Assert.Equal("9007199254740993", big.GetProperty("value").GetString());

        Assert.Equal("invalid_params", Error("number.analyze", new { value = "12x" }).Code);
    }

    [Fact]
    public void ValuesTextAcrossChosenSystems()
    {
        JsonElement rows = Result("text.values", new
        {
            text = "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",
            valueSystems = new[] { QuranCodeEngine.DefaultValueSystem },
        });

        Assert.Equal(1, rows.GetArrayLength());
        Assert.Equal(19, rows[0].GetProperty("letterCount").GetInt32());
    }

    [Fact]
    public void SearchHighlightsTheMatchingDisplayWords()
    {
        JsonElement result = Result("search.text", new { term = "بعدما", wordness = "whole", limit = 500 });
        Assert.True(result.GetProperty("verseCount").GetInt32() > 0);

        // 2:181 joins "بَعْدَ مَا" into one searchable word; both display words light up.
        JsonElement verse = result.GetProperty("verses").EnumerateArray()
            .Single(v => v.GetProperty("number").GetInt32() == 188);
        Assert.Equal([2, 3], verse.GetProperty("highlights").EnumerateArray().Select(h => h.GetInt32()));
    }

    [Fact]
    public void SearchReportsMatchesInsideTheBismillah()
    {
        JsonElement result = Result("search.text", new { term = "الرحمن", wordness = "whole", limit = 20 });

        // 2:1 is "الٓمٓ" alone; its match is the Bismillah the engine counts as its first words.
        JsonElement verse = result.GetProperty("verses").EnumerateArray()
            .Single(v => v.GetProperty("number").GetInt32() == 8);
        Assert.Equal([2], verse.GetProperty("bismillahHighlights").EnumerateArray().Select(h => h.GetInt32()));
        Assert.True(verse.GetProperty("aligned").GetBoolean());
        Assert.Equal(0, verse.GetProperty("highlights").GetArrayLength());

        JsonElement fatiha = result.GetProperty("verses")[0];
        Assert.Equal(0, fatiha.GetProperty("bismillahHighlights").GetArrayLength()); // 1:1 is the Bismillah itself
    }

    [Fact]
    public void SearchPagesResults()
    {
        JsonElement all = Result("search.text", new { term = "الله", limit = 500 });
        JsonElement page = Result("search.text", new { term = "الله", offset = 10, limit = 5 });

        Assert.Equal(all.GetProperty("verseCount").GetInt32(), page.GetProperty("verseCount").GetInt32());
        Assert.Equal(5, page.GetProperty("verses").GetArrayLength());
        Assert.Equal(
            all.GetProperty("verses")[10].GetProperty("number").GetInt32(),
            page.GetProperty("verses")[0].GetProperty("number").GetInt32());
    }

    [Theory]
    [InlineData("selection.stats", """{"first":0,"last":7}""")]
    [InlineData("selection.stats", """{"first":7,"last":1}""")]
    [InlineData("chapter.verses", """{"chapter":115}""")]
    [InlineData("search.text", """{"term":"  "}""")]
    [InlineData("search.text", """{"term":"الله","limit":0}""")]
    [InlineData("search.text", """{"term":"الله","wordness":"sometimes"}""")]
    [InlineData("chapter.verses", """{"chapter":"two"}""")]
    [InlineData("chapter.verses", """{"chapter":1,"extra":true}""")]
    [InlineData("chapter.verses", "{}")]
    public void RejectsBadParams(string method, string parameters)
    {
        string response = _dispatcher.Handle($$"""{"id":3,"method":"{{method}}","params":{{parameters}}}""");
        JsonElement error = JsonDocument.Parse(response).RootElement.GetProperty("error");
        Assert.Equal("invalid_params", error.GetProperty("code").GetString());
    }

    [Fact]
    public void MissingParamsObjectIsInvalid() =>
        Assert.Equal("invalid_params", Error("chapter.verses").Code);

    [Fact]
    public void UnknownValueSystemIsNotFound() =>
        Assert.Equal("not_found", Error("selection.stats", new { first = 1, last = 1, valueSystem = "Nope" }).Code);

    [Fact]
    public void UnknownMethod() => Assert.Equal("unknown_method", Error("verses.delete").Code);

    [Fact]
    public void ParamsMayBeOmitted()
    {
        string response = _dispatcher.Handle("""{"id":9,"method":"engine.info"}""");
        Assert.True(JsonDocument.Parse(response).RootElement.TryGetProperty("result", out _), response);
    }

    [Fact]
    public void ArabicIsWrittenAsUtf8NotEscapes()
    {
        string response = _dispatcher.Handle("""{"id":9,"method":"chapter.verses","params":{"chapter":1}}""");
        string alifWasla = ((char)0x0671).ToString();
        Assert.Contains(alifWasla, response);
        Assert.DoesNotContain("\\u0671", response, StringComparison.OrdinalIgnoreCase); // no escape sequence
    }

    [Fact]
    public void MalformedLineIsAParseErrorWithNullId()
    {
        JsonElement root = JsonDocument.Parse(_dispatcher.Handle("{not json")).RootElement;
        Assert.Equal(JsonValueKind.Null, root.GetProperty("id").ValueKind);
        Assert.Equal("parse_error", root.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void HostAnswersEveryLineAndStopsAtEndOfInput()
    {
        var input = new StringReader(
            """
            {"id":1,"method":"engine.info"}

            {"id":2,"method":"nope"}
            """);
        var output = new StringWriter();

        Assert.Equal(0, StdioHost.Run(input, output, _dispatcher));

        string[] lines = output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length); // the blank line is skipped, not answered
    }
}
