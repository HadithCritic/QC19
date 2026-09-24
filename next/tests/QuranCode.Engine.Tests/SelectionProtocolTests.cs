using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>Exact selections over the protocol, in the Submission edition.</summary>
public sealed class SelectionProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly Dispatcher _dispatcher;

    public SelectionProtocolTests() => _dispatcher = new Dispatcher(new Handlers(_engine), new StringWriter());

    public void Dispose() => _engine.Dispose();

    private const string Gematria = "Simplified29_Abjad_Gematria";

    private JsonElement Call(string method, object parameters) =>
        JsonDocument.Parse(_dispatcher.Handle(JsonSerializer.Serialize(new { id = 1, method, @params = parameters }))).RootElement;

    private JsonElement Result(string method, object parameters)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("result", out JsonElement result), root.ToString());
        return result;
    }

    private string ErrorCode(string method, object parameters)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("error", out JsonElement error), root.ToString());
        return error.GetProperty("code").GetString()!;
    }

    private static object Selection(object start, object end) => new { start, end };

    [Fact]
    public void AnalyzesAWordRange()
    {
        JsonElement a = Result("selection.analyze", new
        {
            selection = Selection(new { chapter = 1, verse = 1, word = 2 }, new { chapter = 1, verse = 1, word = 3 }),
            valueSystem = Gematria,
        });

        Assert.Equal("1:1:w2-1:1:w3", a.GetProperty("address").GetString());
        Assert.False(a.GetProperty("verseAligned").GetBoolean());
        Assert.Equal("2", a.GetProperty("words").GetProperty("value").GetString());
        Assert.Equal("1", a.GetProperty("verses").GetProperty("value").GetString());
        Assert.Equal(JsonValueKind.String, a.GetProperty("value").GetProperty("value").ValueKind);
        Assert.Equal("Simplified29", a.GetProperty("methodology").GetProperty("textMode").GetString());
        Assert.Equal(1, a.GetProperty("delta").GetProperty("words").GetInt32());
        Assert.Equal(2, a.GetProperty("start").GetProperty("wordInVerse").GetInt32());
    }

    [Fact]
    public void BackwardEndpointsComeBackInOrder()
    {
        JsonElement a = Result("selection.analyze", new
        {
            selection = Selection(new { chapter = 1, verse = 3 }, new { chapter = 1, verse = 1, word = 2, letter = 2 }),
        });
        Assert.Equal("1:1:w2:l2-1:3", a.GetProperty("address").GetString());
        Assert.Equal(2, a.GetProperty("selection").GetProperty("start").GetProperty("word").GetInt32());
    }

    [Fact]
    public void AWholeChapterIsVerseAlignedAndMatchesStats()
    {
        JsonElement a = Result("selection.analyze", new { selection = Selection(new { chapter = 1 }, new { chapter = 1 }), valueSystem = Gematria });
        JsonElement stats = Result("selection.stats", new { first = a.GetProperty("first").GetInt32(), last = a.GetProperty("last").GetInt32(), valueSystem = Gematria });

        Assert.True(a.GetProperty("verseAligned").GetBoolean());
        Assert.Equal(stats.GetProperty("value").GetProperty("value").GetString(), a.GetProperty("value").GetProperty("value").GetString());
        Assert.Equal(stats.GetProperty("letters").GetProperty("value").GetString(), a.GetProperty("letters").GetProperty("value").GetString());
    }

    [Fact]
    public void AnUncountedSelectionIsEmptyWithANote()
    {
        JsonElement a = Result("selection.analyze", new
        {
            selection = Selection(new { chapter = 2, verse = 0 }, new { chapter = 2, verse = 0 }),
            counting = new { includeBasmalas = false },
        });

        Assert.Equal(JsonValueKind.Null, a.GetProperty("first").ValueKind);
        Assert.Equal("0", a.GetProperty("letters").GetProperty("value").GetString());
        Assert.NotEqual(0, a.GetProperty("notes").GetArrayLength());
    }

    [Theory]
    [InlineData(115, 1, 1)]
    [InlineData(1, 1, 9)]
    public void RejectsWhatDoesNotExist(int chapter, int verse, int word) =>
        Assert.Equal("invalid_params", ErrorCode("selection.analyze", new
        {
            selection = Selection(new { chapter, verse, word }, new { chapter, verse, word }),
        }));

    [Fact]
    public void RejectsAnUnknownSystem() =>
        Assert.Equal("not_found", ErrorCode("selection.analyze", new
        {
            selection = Selection(new { chapter = 1 }, new { chapter = 1 }),
            valueSystem = "Nope",
        }));

    [Fact]
    public void RangeMethodsTakeASelection()
    {
        object sel = Selection(new { chapter = 1, verse = 1, word = 2, letter = 2 }, new { chapter = 1, verse = 1, word = 3, letter = 3 });

        JsonElement stats = Result("selection.stats", new { selection = sel, valueSystem = Gematria });
        Assert.Equal("6", stats.GetProperty("letters").GetProperty("value").GetString());

        JsonElement words = Result("selection.words", new { selection = sel, valueSystem = Gematria });
        Assert.Equal(2, words.GetProperty("total").GetInt32());

        JsonElement letters = Result("selection.letters", new { selection = sel, valueSystem = Gematria });
        Assert.Equal(6, letters.EnumerateArray().Sum(l => l.GetProperty("count").GetInt32()));

        JsonElement sweep = Result("selection.sweep", new { selection = sel, valueSystem = Gematria });
        Assert.Equal("6", sweep.EnumerateArray().Single(t => t.GetProperty("label").GetString() == "Letters").GetProperty("value").GetString());

        JsonElement maths = Result("selection.maths", new { selection = sel });
        Assert.Equal(1, maths.GetProperty("verses").GetProperty("count").GetInt32());

        JsonElement symmetry = Result("selection.symmetry", new { selection = sel, kind = "wordLetters" });
        Assert.Equal(2, symmetry.GetProperty("units").GetInt32());

        JsonElement allah = Result("selection.allah", new { selection = sel });
        Assert.Equal(1, allah.GetProperty("allah").GetInt32());

        JsonElement research = Result("research.words", new { method = "all", selection = sel });
        Assert.True(research.GetProperty("rowCount").GetInt32() > 0);
    }

    [Fact]
    public void AWholeVerseSelectionGivesTheVerseResult()
    {
        object sel = Selection(new { chapter = 2, verse = 255 }, new { chapter = 2, verse = 255 });
        JsonElement bySelection = Result("selection.sweep", new { selection = sel });
        int number = _engine.Verse(2, 255).Number;
        JsonElement byRange = Result("selection.sweep", new { first = number, last = number });

        Assert.Equal(byRange.ToString(), bySelection.ToString());
    }

    [Fact]
    public void WordsWithMarksNeedWholeVerses()
    {
        object sel = Selection(new { chapter = 1, verse = 1, word = 2 }, new { chapter = 1, verse = 1, word = 3 });
        Assert.Equal("invalid_params", ErrorCode("selection.words", new { selection = sel, withMarks = true }));
    }

    [Fact]
    public void ASelectionAndARangeTogetherAreRefused()
    {
        object sel = Selection(new { chapter = 1 }, new { chapter = 1 });
        Assert.Equal("invalid_params", ErrorCode("selection.stats", new { first = 1, last = 2, selection = sel }));
    }

    [Fact]
    public void StatsWithoutARangeOrSelectionAreRefused() =>
        Assert.Equal("invalid_params", ErrorCode("selection.stats", new { valueSystem = Gematria }));
}
