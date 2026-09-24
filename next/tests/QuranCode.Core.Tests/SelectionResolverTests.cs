using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Exact selections resolved to counted letters. Expectations are read off the
/// segmentation the verse-range statistics already use, so a selection that
/// covers whole verses must land on exactly the same units.
/// </summary>
[Collection(SharedEngine.Collection)]
public sealed class SelectionResolverTests
{
    private static QuranCodeEngine Engine => SharedEngine.Instance;

    [Theory]
    [InlineData("2:255", 262, 262)]
    [InlineData("1", 1, 7)]
    [InlineData("2-5", 8, 789)]
    [InlineData("1:7-2:2", 7, 9)]
    public void WholeVersesMatchTheVerseRange(string address, int first, int last)
    {
        CountedSpan span = Resolve(address);
        SelectionStatistics stats = Engine.Statistics(new VerseRange(first, last));

        Assert.True(span.IsVerseAligned);
        Assert.Equal(first - 1, span.FirstVerse);
        Assert.Equal(last - 1, span.LastVerse);
        Assert.Equal(stats.VerseCount, span.VerseCount);
        Assert.Equal(stats.WordCount, span.WordCount);
        Assert.Equal(stats.LetterCount, span.LetterCount);
    }

    [Fact]
    public void OneWordIsItsCountedLetters()
    {
        CountedSpan span = Resolve("1:1:w2");
        Segmentation s = Engine.Segmentation();

        Assert.Equal(1, span.WordCount);
        Assert.Equal(1, span.FirstWord);
        Assert.Equal("الله", s.WordText(span.FirstWord));
        Assert.Equal(4, span.LetterCount);
        Assert.False(span.IsVerseAligned);
    }

    [Fact]
    public void OneLetterIsOneCountedLetter()
    {
        CountedSpan span = Resolve("1:1:w2:l2");

        Assert.Equal(1, span.LetterCount);
        Assert.Equal(1, span.WordCount);
        Assert.Equal('ل', Engine.Segmentation().LetterChars[span.FirstLetter]);
        Assert.Equal(3 + 1, span.FirstLetter); // بسم, then the alif of الله
    }

    [Fact]
    public void SeveralWordsInOneVerse()
    {
        CountedSpan span = Resolve("1:1:w2-1:1:w3");
        Assert.Equal(2, span.WordCount);
        Assert.Equal(4 + 6, span.LetterCount);
    }

    [Fact]
    public void AllTheWordsOfAVerseAreTheVerse()
    {
        CountedSpan span = Resolve("1:1:w1-1:1:w4");
        Assert.True(span.IsVerseAligned);
        Assert.Equal(19, span.LetterCount);
    }

    [Fact]
    public void WordsAcrossVerses()
    {
        Segmentation s = Engine.Segmentation();
        CountedSpan span = Resolve("1:1:w3-1:2:w2");

        Assert.Equal(2, span.VerseCount);
        Assert.Equal(2 + 2, span.WordCount);
        Assert.Equal(2, span.FirstWord);
        Assert.Equal(s.VerseFirstWord[1] + 1, span.LastWord);
    }

    [Fact]
    public void WordsAcrossChaptersIncludeTheHeaderBetween()
    {
        // 1:7's last word through 2:1's first word: the classic text counts the
        // Bismillah header of 2:1 as four words before الٓمٓ.
        Segmentation s = Engine.Segmentation();
        int lastOf17 = s.VerseWordCount[6];
        CountedSpan span = Resolve($"1:7:w{lastOf17}-2:1:w1");

        Assert.Equal(2, span.ChapterCount);
        Assert.Equal(1 + 4 + 1, span.WordCount);
    }

    [Fact]
    public void StartingAtWordOneLeavesTheHeaderOut()
    {
        Segmentation s = Engine.Segmentation();
        CountedSpan span = Resolve("2:1:w1");

        Assert.Equal(s.VerseFirstWord[7] + 4, span.FirstWord);
        Assert.Equal(1, span.WordCount);
    }

    [Fact]
    public void BackwardClicksGiveTheSameSpan()
    {
        Assert.Equal(Resolve("1:1:w3-1:2:w2"), Resolve("1:2:w2-1:1:w3"));
        Assert.Equal(Resolve("1:1:w2:l2-1:1:w3:l3"), Resolve("1:1:w3:l3-1:1:w2:l2"));
    }

    [Fact]
    public void PartialWordsAtBothEnds()
    {
        // الله from its second letter, through الرحمن to its third.
        CountedSpan span = Resolve("1:1:w2:l2-1:1:w3:l3");

        Assert.Equal(2, span.WordCount);
        Assert.Equal(3 + 3, span.LetterCount);
        Assert.Equal(4, span.FirstLetter);
        Assert.Equal(3 + 4 + 2, span.LastLetter);
    }

    [Fact]
    public void LettersInsideOneWord()
    {
        CountedSpan span = Resolve("1:1:w3:l2-1:1:w3:l4");
        Assert.Equal(3, span.LetterCount);
        Assert.Equal(1, span.WordCount);
    }

    [Fact]
    public void MixedLevelEndpoints()
    {
        CountedSpan wordToVerse = Resolve("1:1:w3-1:2");
        CountedSpan verseToLetter = Resolve("1:1-1:2:w1:l2");
        Segmentation s = Engine.Segmentation();

        Assert.Equal(s.VerseFirstWord[1] + s.VerseWordCount[1] - 1, wordToVerse.LastWord);
        Assert.Equal(0, verseToLetter.FirstLetter);
        Assert.Equal(s.WordFirstLetter[s.VerseFirstWord[1]] + 1, verseToLetter.LastLetter);
    }

    [Fact]
    public void AShaddaLetterSelectsBothItsLetters()
    {
        // Original allows no text options, so these use Simplified29.
        var counting = new CountingOptions { ShaddaAsLetter = true };
        CountedSpan span = Resolve("1:1:w2:l3", "Simplified29", counting);
        Assert.Equal(2, span.LetterCount);
    }

    [Fact]
    public void AWawWordSelectsBothItsCountedWords()
    {
        // 1:5 وَإِيَّاكَ is و and إياك when waw is counted as a word.
        var counting = new CountingOptions { WawAsWord = true };

        CountedSpan word = Resolve("1:5:w3", "Simplified29", counting);
        CountedSpan waw = Resolve("1:5:w3:l1", "Simplified29", counting);
        CountedSpan rest = Resolve("1:5:w3:l2-1:5:w3:l5", "Simplified29", counting);

        Assert.Equal(2, word.WordCount);
        Assert.Equal(1, waw.WordCount);
        Assert.Equal(1, waw.LetterCount);
        Assert.Equal(1, rest.WordCount);
        Assert.Equal(word.LastWord, rest.FirstWord);
    }

    [Theory]
    [InlineData("115", "Chapters run from 1 to 114")]
    [InlineData("1:8", "verses 1 to 7")]
    [InlineData("2:0", "verses 1 to 286")]
    [InlineData("1:1:w5", "words 1 to 4")]
    [InlineData("1:1:w2:l5", "letters 1 to 4")]
    public void RejectsWhatDoesNotExist(string address, string reasonFragment)
    {
        SelectionResolution result = Engine.Resolve(Parse(address));
        Assert.False(result.IsSuccess);
        Assert.Contains(reasonFragment, result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsALetterWithoutAWord()
    {
        var bad = new QuranLocation(1, 1, null, 2);
        SelectionResolution result = Engine.Resolve(new QuranSelection(bad, bad));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void KeepsTheOrderedSelection()
    {
        SelectionResolution result = Engine.Resolve(Parse("1:2-1:1:w3"));
        Assert.Equal(new QuranLocation(1, 1, 3), result.Selection.Start);
        Assert.Equal(new QuranLocation(1, 2), result.Selection.End);
    }

    internal static QuranSelection Parse(string address)
    {
        SelectionParseResult parsed = SelectionAddress.Parse(address);
        Assert.True(parsed.IsSuccess, parsed.Error);
        return parsed.Selection!;
    }

    private static CountedSpan Resolve(string address, string textMode = "Original", CountingOptions? counting = null)
    {
        SelectionResolution result = Engine.Resolve(Parse(address), textMode, counting);
        Assert.True(result.IsSuccess, result.Error);
        Assert.NotNull(result.Span);
        return result.Span!;
    }
}

/// <summary>The Submission edition: verse 0, chapter 9 and the 96:5 word rule.</summary>
[Collection(SubmissionEngine.Collection)]
public sealed class SubmissionSelectionResolverTests
{
    private static QuranCodeEngine Engine => SubmissionEngine.Instance;

    private static readonly CountingOptions NoBasmalas = new() { IncludeBasmalas = false };

    [Fact]
    public void MaAloneIsTheJoinedWordButOnlyItsLetters()
    {
        // 96:5 عَلَّمَ ٱلْإِنسَٰنَ مَا لَمْ يَعْلَمْ: the edition counts ما لم as one word.
        CountedSpan ma = Resolve("96:5:w3");
        CountedSpan lam = Resolve("96:5:w4");
        CountedSpan both = Resolve("96:5:w3-96:5:w4");

        Assert.Equal(1, ma.WordCount);
        Assert.Equal(2, ma.LetterCount);
        Assert.Equal(1, lam.WordCount);
        Assert.Equal(2, lam.LetterCount);
        Assert.Equal(ma.FirstWord, lam.FirstWord);
        Assert.Equal(1, both.WordCount);
        Assert.Equal(4, both.LetterCount);
    }

    [Fact]
    public void VerseZeroIsSelectableWhenCounted()
    {
        CountedSpan span = Resolve("2:0");
        Assert.True(span.IsVerseAligned);
        Assert.Equal(4, span.WordCount);
    }

    [Fact]
    public void AnUncountedVerseZeroResolvesToNothingWithANote()
    {
        SelectionResolution result = Engine.Resolve(SelectionResolverTests.Parse("2:0"), "Original", NoBasmalas);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Null(result.Span);
        Assert.Contains(result.Notes, n => n.Contains("2:0", StringComparison.Ordinal));
    }

    [Fact]
    public void AnUncountedVerseZeroAtTheStartIsSkippedNotMoved()
    {
        SelectionResolution result = Engine.Resolve(SelectionResolverTests.Parse("2:0-2:1"), "Original", NoBasmalas);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(new QuranLocation(2, 0), result.Selection.Start);
        Assert.Equal(1, result.Span!.VerseCount);
        Assert.Equal(Engine.View(NoBasmalas).IndexOf(Engine.Verse(2, 1).Number), result.Span.FirstVerse);
        Assert.Contains(result.Notes, n => n.Contains("2:0", StringComparison.Ordinal));
    }

    [Fact]
    public void ChapterNineHasNoVerseZero()
    {
        Assert.True(Resolve("9").IsVerseAligned);
        SelectionResolution result = Engine.Resolve(SelectionResolverTests.Parse("9:0"));
        Assert.False(result.IsSuccess);
        Assert.Contains("verses 1 to 127", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void AWholeChapterMatchesTheVerseRange()
    {
        Chapter chapter = Engine.Chapters[1];
        CountedSpan span = Resolve("2", NoBasmalas);
        SelectionStatistics stats = Engine.Statistics(
            new VerseRange(chapter.FirstVerse, chapter.LastVerse), counting: NoBasmalas);

        Assert.Equal(stats.VerseCount, span.VerseCount);
        Assert.Equal(stats.WordCount, span.WordCount);
        Assert.Equal(stats.LetterCount, span.LetterCount);
    }

    private static CountedSpan Resolve(string address, CountingOptions? counting = null)
    {
        SelectionResolution result = Engine.Resolve(SelectionResolverTests.Parse(address), "Original", counting);
        Assert.True(result.IsSuccess, result.Error);
        Assert.NotNull(result.Span);
        return result.Span!;
    }
}
