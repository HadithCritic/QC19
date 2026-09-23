using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>Phase 5 over the protocol: number details, selection lists, research lists and ratio splits.</summary>
public sealed class StatisticsProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly Dispatcher _dispatcher;

    public StatisticsProtocolTests() => _dispatcher = new Dispatcher(new Handlers(_engine), new StringWriter());

    public void Dispose() => _engine.Dispose();

    private JsonElement Call(string method, object parameters) =>
        JsonDocument.Parse(_dispatcher.Handle(JsonSerializer.Serialize(new { id = 1, method, @params = parameters }))).RootElement;

    private JsonElement Result(string method, object parameters)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("result", out JsonElement result), root.ToString());
        return result;
    }

    [Fact]
    public void NumberDetailsOf619()
    {
        JsonElement d = Result("number.details", new { value = "619" });
        Assert.Equal("P114-C83-P23-P9-C4-C1", d.GetProperty("chain").GetProperty("text").GetString());
        Assert.Equal(2, d.GetProperty("divisorCount").GetInt32());
        Assert.Equal("4n-1", d.GetProperty("fourN").GetProperty("form").GetString());
        Assert.False(d.GetProperty("carmichael").GetBoolean());

        JsonElement carmichael = Result("number.details", new { value = "561" });
        Assert.True(carmichael.GetProperty("carmichael").GetBoolean());
        Assert.Equal(JsonValueKind.Null, Result("number.details", new { value = "0" }).GetProperty("chain").ValueKind);
    }

    [Fact]
    public void SelectionListsForAlFatiha()
    {
        JsonElement words = Result("selection.words", new { first = 1, last = 7 });
        Assert.Equal(29, words.GetProperty("total").GetInt32());

        JsonElement letters = Result("selection.letters", new { first = 1, last = 7, scope = "verse" });
        Assert.Equal(139, letters.EnumerateArray().Sum(l => l.GetProperty("count").GetInt32()));

        JsonElement maths = Result("selection.maths", new { first = 1, last = 7 });
        Assert.Equal(1, maths.GetProperty("chapters").GetProperty("count").GetInt32());
        Assert.Equal(28, maths.GetProperty("verses").GetProperty("v").GetProperty("sum").GetDouble());

        JsonElement symmetry = Result("selection.symmetry", new { first = 1, last = 7, kind = "verseWords", boundaries = true });
        Assert.Equal(7, symmetry.GetProperty("units").GetInt32());

        JsonElement allah = Result("selection.allah", new { first = 1, last = 7 });
        Assert.Equal(2, allah.GetProperty("total").GetInt32()); // ٱللَّهِ in 1:1 and لِلَّهِ in 1:2
        Assert.Equal(1, allah.GetProperty("allah").GetInt32());
    }

    [Fact]
    public void ResearchListsPageOrComeWhole()
    {
        JsonElement page = Result("research.words", new { method = "allah", limit = 5 });
        Assert.Equal(2815, page.GetProperty("rowCount").GetInt32());
        Assert.Equal(5, page.GetProperty("rows").GetArrayLength());

        JsonElement whole = Result("research.words", new { method = "repeated", tsv = true });
        string tsv = whole.GetProperty("tsv").GetString()!;
        Assert.StartsWith("#\tWord1\tWord2", tsv);
        Assert.Equal(whole.GetProperty("rowCount").GetInt32() + 1, tsv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Fact]
    public void RatioSplitsMapToDisplayedWords()
    {
        JsonElement units = Result("ratio.split", new { chapter = 1 });
        JsonElement first = units[0];
        Assert.Equal(7, units.GetArrayLength());
        Assert.True(first.GetProperty("colored").GetBoolean());
        Assert.Equal(2, first.GetProperty("splitWord").GetInt32());     // ٱلرَّحْمَٰنِ
        Assert.Equal(5, first.GetProperty("splitLetters").GetInt32());
    }

    [Fact]
    public void TranslationsListReadAndSearch()
    {
        JsonElement list = Result("translations.list", new { });
        // ADR 0004: Khalifa's English, the transliteration and the Emlaaei text.
        Assert.Equal(3, list.GetArrayLength());

        JsonElement text = Result("translations.text", new { keys = new[] { "submission.en", "submission.translit" }, first = 1, last = 7 });
        Assert.Equal(2, text.GetArrayLength());
        Assert.Equal(7, text[0].GetProperty("verses").GetArrayLength());

        JsonElement found = Result("search.text", new { term = "Most Merciful", limit = 3 });
        Assert.Equal("translations", found.GetProperty("foundIn").GetString());
        JsonElement line = found.GetProperty("verses")[0].GetProperty("translations")[0];
        Assert.Equal("submission.en", line.GetProperty("key").GetString());
        Assert.Equal(13, line.GetProperty("ranges")[0][1].GetInt32());

        JsonElement spelled = Result("search.text", new { term = "الكتاب", wordness = "whole" });
        Assert.Equal("emlaaei", spelled.GetProperty("foundIn").GetString());
        Assert.True(spelled.GetProperty("verseCount").GetInt32() > 100);

        JsonElement unknown = Call("search.text", new { term = "God", translations = new[] { "nobody.here" } });
        Assert.Equal("not_found", unknown.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void FindingsComeWithTheirProvenance()
    {
        JsonElement findings = Result("findings.list", new { });
        Assert.True(findings.GetArrayLength() >= 5);
        JsonElement[] gated = [.. findings.EnumerateArray().Where(f => f.GetProperty("check").GetString() == "gate")];
        Assert.NotEmpty(gated);
        Assert.All(gated, f => Assert.True(
            f.GetProperty("holds").GetBoolean(),
            $"{f.GetProperty("id").GetString()} computed {f.GetProperty("computed").GetInt64()}, " +
            $"published {f.GetProperty("expected").GetInt64()}"));

        JsonElement allah = findings.EnumerateArray().Single(f => f.GetProperty("id").GetString() == "allah-count");
        Assert.Equal(2698, allah.GetProperty("computed").GetInt64());
        Assert.True(allah.GetProperty("multipleOf19").GetBoolean());
        Assert.Equal(142, allah.GetProperty("multiple").GetInt64());
        // The rule was derived, not published, so the wire says so.
        Assert.Equal("inferred", allah.GetProperty("basis").GetString());
        Assert.Equal("numbered verses only", allah.GetProperty("convention").GetString());
        Assert.Equal("Appendix 1", allah.GetProperty("source").GetString());

        JsonElement alif = findings.EnumerateArray().Single(f => f.GetProperty("id").GetString() == "alm-2-alif");
        Assert.Equal("open", alif.GetProperty("check").GetString());
        Assert.False(alif.GetProperty("holds").GetBoolean());

        JsonElement qaf = findings.EnumerateArray().Single(f => f.GetProperty("id").GetString() == "qaf-in-chapter-50");
        Assert.Equal("stated", qaf.GetProperty("basis").GetString());
        Assert.Equal("chapter 50", qaf.GetProperty("scope").GetString());
    }

    [Fact]
    public void InitialsListTheTwentyNineChapters()
    {
        JsonElement chapters = Result("initials.list", new { });
        Assert.Equal(29, chapters.GetArrayLength());

        JsonElement qaf = chapters.EnumerateArray().Single(c => c.GetProperty("chapter").GetInt32() == 50);
        JsonElement count = qaf.GetProperty("counts")[0];
        Assert.Equal("ق", count.GetProperty("letter").GetString());
        Assert.Equal(57, count.GetProperty("count").GetInt64());
        Assert.Equal(57, count.GetProperty("published").GetInt64());
        Assert.True(count.GetProperty("multipleOf19").GetBoolean());

        JsonElement shura = chapters.EnumerateArray().Single(c => c.GetProperty("chapter").GetInt32() == 42);
        Assert.Equal(2, shura.GetProperty("verses").GetInt32());
        Assert.Equal(5, shura.GetProperty("counts").GetArrayLength());
    }

    [Fact]
    public void SweepListsTheTotalsOfASelection()
    {
        // Chapter 50, whatever its absolute verse numbers in this edition.
        JsonElement range = Result("reference.parse", new { text = "50" });
        JsonElement totals = Result("selection.sweep", new
        {
            first = range.GetProperty("first").GetInt32(),
            last = range.GetProperty("last").GetInt32(),
            valueSystem = "Simplified29_Alphabet_Primes1",
        });
        string Value(string label) => totals.EnumerateArray().Single(t => t.GetProperty("label").GetString() == label)
            .GetProperty("value").GetString()!;
        Assert.Equal("57", Value("ق"));
        Assert.Equal("50", Value("Sum of chapter numbers"));
        Assert.Contains(totals.EnumerateArray(), t => t.GetProperty("group").GetString() == "numbers");
    }

    [Fact]
    public void WordInfoGivesMeaningAndGrammar()
    {
        JsonElement word = Result("word.info", new { verse = 1, word = 2 });
        Assert.Equal("the Fountain of Mercy,", word.GetProperty("meaning").GetString());
        Assert.Contains("رحم", word.GetProperty("roots").EnumerateArray().Select(r => r.GetString()));
        Assert.True(word.GetProperty("parts").GetArrayLength() >= 2);
    }

    [Theory]
    [InlineData("translations.text", """{"keys":[],"first":1,"last":7}""")]
    [InlineData("translations.text", """{"keys":["submission.en"],"first":1,"last":2000}""")]
    [InlineData("word.info", """{"verse":1,"word":9}""")]
    [InlineData("selection.letters", """{"first":1,"last":7,"scope":"page"}""")]
    [InlineData("selection.symmetry", """{"first":1,"last":7,"kind":"sideways"}""")]
    [InlineData("research.words", """{"method":"allah","first":3}""")]
    [InlineData("research.words", """{"method":"favorites"}""")]
    [InlineData("ratio.split", """{"chapter":1,"ratio":2}""")]
    [InlineData("ratio.split", """{"chapter":1,"boundary":"comma"}""")]
    [InlineData("number.details", """{"value":"lots"}""")]
    public void RejectsBadParams(string method, string parameters)
    {
        JsonElement root = JsonDocument.Parse(_dispatcher.Handle($$"""{"id":1,"method":"{{method}}","params":{{parameters}}}""")).RootElement;
        Assert.Equal("invalid_params", root.GetProperty("error").GetProperty("code").GetString());
    }
}
