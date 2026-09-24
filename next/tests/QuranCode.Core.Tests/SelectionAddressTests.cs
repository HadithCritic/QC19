using QuranCode.Core.Content;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Exact selection addresses: parsing, formatting and ordering.</summary>
public sealed class SelectionAddressTests
{
    [Theory]
    [InlineData("2", 2, null, null, null)]
    [InlineData("2:255", 2, 255, null, null)]
    [InlineData("2:0", 2, 0, null, null)]
    [InlineData("2:255:w4", 2, 255, 4, null)]
    [InlineData("2:255:w4:l2", 2, 255, 4, 2)]
    [InlineData(" 2 : 255 : W4 : L2 ", 2, 255, 4, 2)]
    [InlineData("٢:٢٥٥:w٤", 2, 255, 4, null)] // Arabic-Indic digits
    public void ParsesOneLocation(string text, int chapter, int? verse, int? word, int? letter)
    {
        SelectionParseResult result = SelectionAddress.Parse(text);

        Assert.True(result.IsSuccess, result.Error);
        var expected = new QuranLocation(chapter, verse, word, letter);
        Assert.Equal(new QuranSelection(expected, expected), result.Selection);
    }

    [Fact]
    public void ParsesARangeWithMixedLevels()
    {
        SelectionParseResult result = SelectionAddress.Parse("2:255:w4:l2-2:260");

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(new QuranLocation(2, 255, 4, 2), result.Selection!.Start);
        Assert.Equal(new QuranLocation(2, 260), result.Selection.End);
    }

    [Fact]
    public void ParsesAChapterRange()
    {
        SelectionParseResult result = SelectionAddress.Parse("2-5");

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(new QuranSelection(new QuranLocation(2), new QuranLocation(5)), result.Selection);
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData("2:255:w4-5", "in full")]   // word 5 or chapter 5?
    [InlineData("2:255-257", "in full")]    // the verse-range shorthand belongs to ReferenceParser
    [InlineData("2:255:l2", "format")]      // a letter needs a word
    [InlineData("2:255:w0", "format")]
    [InlineData("2:255:w4:l0", "format")]
    [InlineData("0", "format")]
    [InlineData("2:w4", "format")]
    [InlineData("1-2-3", "format")]
    [InlineData("abc", "format")]
    public void RejectsWithAReason(string text, string reasonFragment)
    {
        SelectionParseResult result = SelectionAddress.Parse(text);

        Assert.False(result.IsSuccess);
        Assert.Contains(reasonFragment, result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("2:255")]
    [InlineData("2:255:w4")]
    [InlineData("2:255:w4:l2")]
    [InlineData("2:255:w4:l2-2:257:w8:l3")]
    [InlineData("2:255-2:260")]
    [InlineData("2-5")]
    public void FormatsBackToTheSameAddress(string text) =>
        Assert.Equal(text, SelectionAddress.Format(SelectionAddress.Parse(text).Selection!));

    [Fact]
    public void OrdersEndpointsInQuranOrder()
    {
        var later = new QuranLocation(2, 257, 8);
        var earlier = new QuranLocation(2, 255, 4, 2);

        QuranSelection ordered = new QuranSelection(later, earlier).Ordered();

        Assert.Equal(earlier, ordered.Start);
        Assert.Equal(later, ordered.End);
    }

    [Fact]
    public void KeepsOrderedEndpointsAsTheyAre()
    {
        var selection = new QuranSelection(new QuranLocation(2, 255), new QuranLocation(2, 255, 3));
        Assert.Equal(selection, selection.Ordered());
    }

    [Fact]
    public void AWholeVerseStartsBeforeItsWords()
    {
        // Ordering compares where each endpoint begins: 2:255 begins before 2:255:w3.
        var selection = new QuranSelection(new QuranLocation(2, 255, 3), new QuranLocation(2, 255));
        Assert.Equal(new QuranLocation(2, 255), selection.Ordered().Start);
    }

    [Theory]
    [InlineData(2, null, null, null, SelectionLevel.Chapter)]
    [InlineData(2, 255, null, null, SelectionLevel.Verse)]
    [InlineData(2, 255, 4, null, SelectionLevel.Word)]
    [InlineData(2, 255, 4, 2, SelectionLevel.Letter)]
    public void KnowsItsLevel(int chapter, int? verse, int? word, int? letter, SelectionLevel level) =>
        Assert.Equal(level, new QuranLocation(chapter, verse, word, letter).Level);
}
