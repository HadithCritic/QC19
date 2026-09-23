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
    public void HarakatSearchComparesMarks()
    {
        // With its marks, ٱلرَّحْمَٰنِ (genitive) is not ٱلرَّحْمَٰنُ (nominative).
        int genitive = Verses(Result("search.harakat", new { term = "ٱلرَّحْمَٰنِ" }));
        int plain = Verses(Result("search.text", new { term = "الرحمن", wordness = "whole" }));
        Assert.True(genitive > 0 && genitive < plain);
    }
}
