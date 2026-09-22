using QuranCode.Core.Text;
using Xunit;
using Xunit.Abstractions;

namespace QuranCode.Core.Tests;

[Collection(SharedEngine.Collection)]
public sealed class DisplayWordsTests(ITestOutputHelper output)
{
    [Fact]
    public void AttachesPauseMarksToThePreviousWord()
    {
        string[] words = DisplayWords.Split("ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ ۛ فِيهِ ۛ هُدًى");
        Assert.Equal(["ذَٰلِكَ", "ٱلْكِتَٰبُ", "لَا", "رَيْبَ ۛ", "فِيهِ ۛ", "هُدًى"], words);
    }

    [Fact]
    public void AttachesALeadingMarkToTheNextWord()
    {
        // 56:75 opens with ۞; it belongs to the first word, not a word of its own.
        string[] words = DisplayWords.Split("۞ فَلَآ أُقْسِمُ");
        Assert.Equal(["۞ فَلَآ", "أُقْسِمُ"], words);
    }

    [Fact]
    public void KeepsMarksThatHaveNoWord() =>
        Assert.Equal(["۞"], DisplayWords.Split("۞"));

    [Theory]
    [InlineData(2, true)]
    [InlineData(95, true)]  // spelled with a shadda
    [InlineData(1, false)]  // the Bismillah is the verse
    [InlineData(9, false)]  // no Bismillah
    public void SeparatesTheBismillahHeader(int chapter, bool expectHeader)
    {
        Content.Verse verse = SharedEngine.Instance.Verse(chapter, 1);
        VerseDisplay display = DisplayWords.SplitVerse(verse.Text, chapter, 1);

        Assert.Equal(expectHeader, display.Bismillah is not null);
        Assert.Equal(expectHeader ? 4 : 0, display.WordOffset);
        if (expectHeader) Assert.Equal("بسم الله الرحمن الرحيم", ArabicNormalizer.Simplify29(display.Bismillah!));
    }

    [Fact]
    public void JoinedWordsMapToSeveralDisplayWords()
    {
        // 2:181: the rule stage joins "بَعْدَ مَا" into the single word "بعدما".
        QuranCodeEngine engine = SharedEngine.Instance;
        DisplaySpan[] spans = AlignVerse(engine, "Original", 188)!;

        Assert.Equal(13, spans.Length);
        Assert.Equal(new DisplaySpan(2, 2), spans[2]);
        Assert.Equal(new DisplaySpan(4, 1), spans[3]);
    }

    [Fact]
    public void BismillahWordsMapToAnEmptySpan()
    {
        DisplaySpan[] spans = AlignVerse(SharedEngine.Instance, "Original", 8)!; // 2:1

        Assert.Equal(5, spans.Length);
        Assert.All(spans[..4], span => Assert.Equal(0, span.Count));
        Assert.Equal(new DisplaySpan(0, 1), spans[4]);
    }

    [Theory]
    [InlineData("Original")]
    [InlineData("Simplified29")]
    [InlineData("SimplifiedDots")]
    public void EveryVerseAligns(string textMode)
    {
        QuranCodeEngine engine = SharedEngine.Instance;
        var unaligned = new List<int>();
        for (int v = 1; v <= engine.Verses.Count; v++)
        {
            if (AlignVerse(engine, textMode, v) is null) unaligned.Add(v);
        }

        output.WriteLine($"{textMode}: {unaligned.Count} unaligned: {string.Join(", ", unaligned.Take(20))}");
        Assert.Empty(unaligned);
    }

    private static DisplaySpan[]? AlignVerse(QuranCodeEngine engine, string textMode, int verseNumber)
    {
        Content.Segmentation segmentation = engine.Segmentation(textMode);
        Content.Verse verse = engine.Verse(verseNumber);
        string[] segmented = segmentation.VerseWords(verseNumber - 1);

        VerseDisplay display = DisplayWords.SplitVerse(verse.Text, verse.ChapterNumber, verse.NumberInChapter);
        return DisplayWords.Align(display, segmented, engine.Pipeline(textMode));
    }
}
