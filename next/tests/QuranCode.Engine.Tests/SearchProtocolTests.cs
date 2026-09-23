using System.Text.Json;
using QuranCode.Core;
using QuranCode.Core.Tests;
using QuranCode.Engine.Host;
using Xunit;

namespace QuranCode.Engine.Tests;

/// <summary>Phase 4 over the protocol: grouping, scope, roots, related, similar and harakat searches.</summary>
public sealed class SearchProtocolTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly Dispatcher _dispatcher;

    public SearchProtocolTests() => _dispatcher = new Dispatcher(new Handlers(_engine), new StringWriter());

    public void Dispose() => _engine.Dispose();

    private JsonElement Call(string method, object parameters) =>
        JsonDocument.Parse(_dispatcher.Handle(JsonSerializer.Serialize(new { id = 1, method, @params = parameters }))).RootElement;

    private JsonElement Result(string method, object parameters)
    {
        JsonElement root = Call(method, parameters);
        Assert.True(root.TryGetProperty("result", out JsonElement result), root.ToString());
        return result;
    }

    private string ErrorCode(string method, object parameters) =>
        Call(method, parameters).GetProperty("error").GetProperty("code").GetString()!;

    private int Abs(int chapter, int verse) => _engine.Verse(chapter, verse).Number;

    private static int Verses(JsonElement result) => result.GetProperty("verseCount").GetInt32();

    [Fact]
    public void GroupingChangesHowTermsCombine()
    {
        int any = Verses(Result("search.text", new { term = "الله رب", wordness = "whole" }));
        int all = Verses(Result("search.text", new { term = "الله رب", wordness = "whole", grouping = "all" }));
        int phrase = Verses(Result("search.text", new { term = "رب العلمين", wordness = "whole", grouping = "phrase" }));
        Assert.True(any > all && all > 0 && phrase > 0);
        Assert.Equal("invalid_params", ErrorCode("search.text", new { term = "الله", grouping = "some" }));
    }

    [Fact]
    public void ScopeIsARangeOrAList()
    {
        int first = Abs(2, 1), last = Abs(2, 20);
        JsonElement range = Result("search.text", new { term = "الله", scope = new { first, last } });
        Assert.All(range.GetProperty("verses").EnumerateArray(), v => Assert.Equal(2, v.GetProperty("chapter").GetInt32()));

        JsonElement list = Result("search.text", new { term = "الله", scope = new { verses = new[] { 1, Abs(2, 255) } } });
        Assert.Equal(2, Verses(list));

        Assert.Equal("invalid_params", ErrorCode("search.text", new { term = "الله", scope = new { first } }));
        Assert.Equal("invalid_params", ErrorCode("search.text", new { term = "الله", scope = new { verses = new[] { 0 } } }));
    }

    [Fact]
    public void ChapterCountsCoverTheWholeResult()
    {
        JsonElement result = Result("search.text", new { term = "الله", limit = 5 });
        int[] counts = result.GetProperty("chapterCounts").EnumerateArray().Select(c => c.GetInt32()).ToArray();
        Assert.Equal(114, counts.Length);
        Assert.Equal(result.GetProperty("wordCount").GetInt32(), counts.Sum());
        Assert.Equal(5, result.GetProperty("verses").GetArrayLength());
    }

    [Fact]
    public void RootSearchReportsTheRootItUsed()
    {
        JsonElement result = Result("search.roots", new { term = "الرحمن" });
        JsonElement root = result.GetProperty("roots")[0];
        Assert.Equal("رحمان", root.GetProperty("root").GetString());
        Assert.True(Verses(result) > 100);

        JsonElement first = result.GetProperty("verses")[0];
        Assert.Equal(1, first.GetProperty("number").GetInt32());
        Assert.Equal([2], first.GetProperty("highlights").EnumerateArray().Select(h => h.GetInt32()));
    }

    [Fact]
    public void RelatedWordsComeFromTheClickedWord()
    {
        JsonElement result = Result("search.related", new { verse = 1, word = 2 });
        Assert.Equal("رحمان", result.GetProperty("roots")[0].GetProperty("root").GetString());
        Assert.Equal("invalid_params", ErrorCode("search.related", new { verse = 1, word = 9 }));
        Assert.Equal("invalid_params", ErrorCode("search.related", new { verse = 1 }));
    }

    [Fact]
    public void SimilarAndRelatedVersesStartFromAVerse()
    {
        int verse = Abs(55, 13);
        JsonElement exact = Result("search.similar", new { verse, method = "text", threshold = 1.0 });
        Assert.Equal(31, Verses(exact));
        Assert.Equal(1.0, exact.GetProperty("verses")[0].GetProperty("score").GetDouble());

        Assert.True(Verses(Result("search.similar", new { verse })) >= 31); // text at 70%
        Assert.True(Verses(Result("search.relatedVerses", new { verse })) >= 31);
        Assert.Equal("invalid_params", ErrorCode("search.similar", new { verse, threshold = 1.5 }));
        Assert.Equal("invalid_params", ErrorCode("search.similar", new { verse, method = "sound" }));
    }

    [Fact]
    public void AVerseZeroLeftOutCannotStartASearch() =>
        Assert.Equal("invalid_params", ErrorCode("search.similar", new { verse = Abs(2, 0), counting = new { includeBasmalas = false } }));

    [Fact]
    public void NumberSearchFindsUnitsWithReferences()
    {
        JsonElement seven = Result("search.numbers", new { unit = "chapters", verses = new { value = "7" } });
        JsonElement first = seven.GetProperty("units")[0];
        Assert.Equal("Chapter 1", first.GetProperty("reference").GetString());
        Assert.Equal(1, first.GetProperty("firstVerse").GetInt32());
        Assert.Equal(7, first.GetProperty("lastVerse").GetInt32());
        Assert.Equal(3, first.GetProperty("preview").GetArrayLength());
        Assert.True(first.GetProperty("morePreview").GetBoolean());

        // Two neighboring words with 19 letters between them; 3:17 has one such pair.
        JsonElement ranges = Result("search.numbers", new { unit = "words", shape = "range", size = 2, letters = new { value = "19" }, limit = 3 });
        Assert.Equal("3:17 words 5-6", ranges.GetProperty("units")[0].GetProperty("reference").GetString());
        Assert.Equal(ranges.GetProperty("unitCount").GetInt32(), ranges.GetProperty("chapterCounts").EnumerateArray().Sum(c => c.GetInt32()));
    }

    [Fact]
    public void NumberKindsAndTheLastVerse()
    {
        JsonElement last = Result("search.numbers", new { unit = "verses", number = new { value = "-1" }, limit = 500 });
        Assert.Equal(114, last.GetProperty("unitCount").GetInt32());

        JsonElement prime = Result("search.numbers", new { unit = "chapters", verses = new { type = "prime" } });
        Assert.All(prime.GetProperty("units").EnumerateArray(),
            u => Assert.True(Core.Numbers.NumberTheory.IsPrime(u.GetProperty("verses").GetInt32())));
    }

    [Fact]
    public void SentencesInTheSubmissionTextUseItsPauseMarks()
    {
        JsonElement result = Result("search.numbers", new { unit = "sentences", words = new { value = "4" }, scope = new { first = Abs(2, 2), last = Abs(2, 2) } });
        string[] references = result.GetProperty("units").EnumerateArray().Select(u => u.GetProperty("reference").GetString()!).ToArray();
        Assert.Contains("2:2 words 1-4", references); // ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ, up to the first ۛ
    }

    [Fact]
    public void FrequencySearchReportsTheSum()
    {
        JsonElement result = Result("search.frequency", new { unit = "verses", phrase = "الله", sum = new { value = "20" } });
        Assert.True(result.GetProperty("unitCount").GetInt32() > 0);
        Assert.Equal("20", result.GetProperty("units")[0].GetProperty("letterFrequencySum").GetString());

        JsonElement none = Result("search.frequency", new { unit = "words", phrase = "ا", match = "none", limit = 1 });
        Assert.True(none.GetProperty("unitCount").GetInt32() > 0);
    }

    [Theory]
    [InlineData("search.numbers", """{"unit":"verses"}""")]
    [InlineData("search.numbers", """{"unit":"lines","words":{"value":"3"}}""")]
    [InlineData("search.numbers", """{"unit":"verses","words":{"value":"three"}}""")]
    [InlineData("search.numbers", """{"unit":"verses","words":{"type":"perfect"}}""")]
    [InlineData("search.numbers", """{"unit":"verses","words":{"comparison":"about","value":"3"}}""")]
    [InlineData("search.numbers", """{"unit":"words","shape":"set","size":3,"letters":{"value":"3"}}""")]
    [InlineData("search.frequency", """{"unit":"verses","phrase":"  "}""")]
    [InlineData("search.frequency", """{"unit":"verses","phrase":"الله"}""")]
    [InlineData("search.frequency", """{"unit":"pages","phrase":"الله","sum":{"value":"3"}}""")]
    public void RejectsBadNumberSearches(string method, string parameters)
    {
        JsonElement root = JsonDocument.Parse(_dispatcher.Handle($$"""{"id":1,"method":"{{method}}","params":{{parameters}}}""")).RootElement;
        Assert.Equal("invalid_params", root.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void HarakatSearchComparesMarks()
    {
        // With its marks, ٱلرَّحْمَٰنِ (genitive) is not ٱلرَّحْمَٰنُ (nominative).
        int genitive = Verses(Result("search.harakat", new { term = "ٱلرَّحْمَٰنِ" }));
        int plain = Verses(Result("search.text", new { term = "الرحمن", wordness = "whole" }));
        Assert.True(genitive > 0 && genitive < plain);
    }
}
