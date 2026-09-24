using Microsoft.Data.Sqlite;
using QuranCode.Core.Code19;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Derived text modes (Features.txt #72).</summary>
public sealed class TextModeTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);

    public void Dispose()
    {
        _engine.Dispose();
        SqliteConnection.ClearAllPools();
    }

    private long Count(string textMode, int chapter, string letters) =>
        FindingEvaluator.Evaluate(_engine, new Finding(
            "t", "t", 0, FindingMeasure.LetterOccurrences, new FindingScope(chapter), letters,
            true, textMode, RuleBasis.Stated, "t", "t")).Computed;

    [Fact]
    public void AReaderCanDefineAndRemoveAMode()
    {
        // Count the taa marbuta with the haa, as some counts do, on top of Simplified30.
        var mode = new DerivedTextMode("TaaAsHaa", "Simplified30", [new TextRule("ة", "ه")]);
        _engine.DefineTextMode(mode);
        Assert.Equal("Simplified30", _engine.BaseOf("TaaAsHaa"));
        Assert.Equal(0, Count("TaaAsHaa", 2, "ة"));
        Assert.Equal(Count("Simplified30", 2, "ه") + Count("Simplified30", 2, "ة"), Count("TaaAsHaa", 2, "ه"));

        // Redefining replaces it, and what was built for it is rebuilt.
        _engine.DefineTextMode(mode with { Rules = [new TextRule("ب", "ت")] });
        Assert.Equal(0, Count("TaaAsHaa", 2, "ب"));

        Assert.True(_engine.RemoveTextMode("TaaAsHaa"));
        Assert.False(_engine.HasTextMode("TaaAsHaa"));
    }

    [Theory]
    [InlineData("", "Simplified29", "needs a name")]
    [InlineData("Simplified29", "Simplified29", "stock text mode")]
    [InlineData("Mine", "Simplified99", "not a stock text mode")]
    public void AnUnsoundModeIsRefused(string name, string baseMode, string problem)
    {
        var mode = new DerivedTextMode(name, baseMode, [new TextRule("ا", "ا")]);
        Assert.Contains(problem, mode.Problem());
        Assert.Throws<ArgumentException>(() => _engine.DefineTextMode(mode));
    }

    [Fact]
    public void AModeNeedsRules()
    {
        Assert.Contains("at least one rule", new DerivedTextMode("Mine", "Simplified29", []).Problem());
        Assert.Contains("something to find", new DerivedTextMode("Mine", "Simplified29", [new TextRule("", "x")]).Problem());
    }

    [Fact]
    public void AModeHasEveryValueSystemOfItsBase()
    {
        _engine.DefineTextMode(new DerivedTextMode("TaaAsHaa", "Simplified30", [new TextRule("ة", "ه")]));
        string[] stock = [.. _engine.ValueSystems().Where(n => n.StartsWith("Simplified30_", StringComparison.Ordinal))];
        string[] derived = [.. _engine.ValueSystems().Where(n => n.StartsWith("TaaAsHaa_", StringComparison.Ordinal))];
        Assert.NotEmpty(stock);
        Assert.Equal(stock.Select(n => "TaaAsHaa" + n["Simplified30".Length..]), derived);
        Assert.Contains(_engine.ValueSystemSummaries(), s => s.Name == derived[0] && s.TextMode == "TaaAsHaa");

        var system = _engine.ValueSystem("TaaAsHaa_Alphabet_QuranNumbers");
        Assert.Equal("TaaAsHaa", system.TextModeName);
        Assert.Equal(_engine.ValueSystem("Simplified30_Alphabet_QuranNumbers").Values, system.Values);

        // Every ة now values as ه, so the book's value moves.
        Assert.NotEqual(
            _engine.ValueOfBook("Simplified30_Alphabet_QuranNumbers", "Simplified30"),
            _engine.ValueOfBook("TaaAsHaa_Alphabet_QuranNumbers", "TaaAsHaa"));

        _engine.RemoveTextMode("TaaAsHaa");
        Assert.DoesNotContain(_engine.ValueSystems(), n => n.StartsWith("TaaAsHaa_", StringComparison.Ordinal));
    }

    [Fact]
    public void AModeNameIsLettersAndDigits() =>
        Assert.Contains("letters and digits", new DerivedTextMode("My_Mode", "Simplified29", [new TextRule("ا", "ا")]).Problem());
}
