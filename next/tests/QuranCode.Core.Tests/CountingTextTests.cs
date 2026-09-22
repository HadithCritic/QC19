using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

public sealed class CountingTextTests
{
    private static string Chars(params int[] codes) => string.Concat(codes.Select(c => (char)c));

    [Fact]
    public void ShaddaBecomesACopyOfTheLetterBeforeIt()
    {
        // lam, shadda, fatha -> lam, lam, fatha
        Assert.Equal(Chars(0x0644, 0x0644, 0x064E), CountingText.DoubleShadda(Chars(0x0644, 0x0651, 0x064E)));
        Assert.Equal("abc", CountingText.DoubleShadda("abc"));
    }

    [Fact]
    public void AHamzaOnATatweelIsMovedBeforeItsVowel()
    {
        // tatweel, fatha, hamza above -> tatweel, hamza above, fatha
        string stored = Chars(0x0644, 0x0652, 0x0640, 0x064E, 0x0654, 0x0627);
        string expected = Chars(0x0644, 0x0652, 0x0640, 0x0654, 0x064E, 0x0627);
        Assert.Equal(expected, CountingText.CanonicalizeMarks(stored));
        Assert.Equal(expected, CountingText.CanonicalizeMarks(expected));
    }

    [Fact]
    public void TextWithoutATatweelIsUntouched()
    {
        string text = Chars(0x0627, 0x064E, 0x0654);
        Assert.Same(text, CountingText.CanonicalizeMarks(text));
    }
}

[Collection(SharedEngine.Collection)]
public sealed class SubmissionMarkOrderTests
{
    [Theory]
    [InlineData(7, 58)]
    [InlineData(10, 101)]
    [InlineData(15, 92)]
    public void TheThreeReorderedHamzasCountAsInTheClassicText(int chapter, int verse)
    {
        // The Submission text stores these hamzas with the vowel first; once
        // the marks are reordered they count exactly as the classic text does.
        using var submission = new QuranCodeEngine(TestPaths.SubmissionDatabase);
        QuranCodeEngine classic = SharedEngine.Instance;

        string[] expected = classic.Segmentation().VerseWords(classic.Verse(chapter, verse).Number - 1);
        string[] actual = submission.Segmentation().VerseWords(submission.Verse(chapter, verse).Number - 1);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CountingOptionsApplyToTheSubmissionEdition()
    {
        using var engine = new QuranCodeEngine(TestPaths.SubmissionDatabase);
        var all = new Content.VerseRange(1, engine.Verses.Count);
        const string system = "Simplified29_Alphabet_Primes1";

        long plain = engine.Statistics(all, system).LetterCount;
        long hamza = engine.Statistics(all, system, counting: new CountingOptions { HamzaAboveLine = true }).LetterCount;
        long words = engine.Statistics(all, system).WordCount;
        long waw = engine.Statistics(all, system, counting: new CountingOptions { WawAsWord = true }).WordCount;

        Assert.True(hamza > plain, "hamza above a line adds letters");
        Assert.True(waw > words, "waw as a word adds words");
    }

    [Fact]
    public void TheOriginalTextModeIgnoresTextOptions()
    {
        // The original disables these options in the Original text mode.
        using var engine = new QuranCodeEngine(TestPaths.SubmissionDatabase);
        var all = new Content.VerseRange(1, engine.Verses.Count);
        var options = new CountingOptions { WawAsWord = true, ShaddaAsLetter = true, HamzaAboveLine = true };

        Assert.Equal(engine.Statistics(all).LetterCount, engine.Statistics(all, counting: options).LetterCount);
    }
}
