using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
using Xunit;
using Xunit.Abstractions;

namespace QuranCode.Core.Tests;

/// <summary>
/// The Submission edition: chapter 9 without 9:128-129, verse-0 Bismillahs
/// that can be excluded, and its own orthography and word boundaries.
/// </summary>
[Collection(SubmissionEngine.Collection)]
public sealed class SubmissionEditionTests(ITestOutputHelper output)
{
    private static QuranCodeEngine Engine => SubmissionEngine.Instance;

    private static string[] Words(int chapter, int verse, string textMode = "Original") =>
        Engine.Segmentation(textMode).VerseWords(Engine.Verse(chapter, verse).Number - 1);

    [Fact]
    public void DescribesItself()
    {
        Assert.Equal("submission", Engine.Corpus.Edition);
        Assert.Equal(BasmalaMode.VerseZero, Engine.Corpus.Basmala);
    }

    [Fact]
    public void HasVerseZeroBismillahsEverywhereButChaptersOneAndNine()
    {
        Assert.Equal(6346, Engine.Verses.Count);
        Assert.Equal(112, Engine.Verses.Count(v => v.IsBasmala));
        Assert.All(Engine.Verses.Where(v => v.IsBasmala), v => Assert.Equal(0, v.NumberInChapter));

        Assert.False(Engine.Chapters[0].HasVerseZero);
        Assert.False(Engine.Chapters[8].HasVerseZero);
        Assert.True(Engine.Chapters[1].HasVerseZero);
        Assert.True(Engine.Verse(2, 0).IsBasmala);
        Assert.False(Engine.Verse(1, 1).IsBasmala); // chapter 1's Bismillah is verse 1
    }

    [Fact]
    public void ChapterNineEndsAt127()
    {
        Assert.Equal(127, Engine.Chapters[8].VerseCount);
        Assert.Equal(6234, Engine.Chapters.Sum(c => c.VerseCount));

        ReferenceParseResult missing = Engine.ParseReference("9:128");
        Assert.False(missing.IsSuccess);
        Assert.Contains("1 to 127", missing.Error);
    }

    [Fact]
    public void VerseZeroIsAReference()
    {
        ReferenceParseResult zero = Engine.ParseReference("2:0");
        Assert.True(zero.IsSuccess, zero.Error);
        Assert.True(Engine.Verse(zero.Range.First).IsBasmala);

        Assert.False(Engine.ParseReference("1:0").IsSuccess);
        Assert.False(Engine.ParseReference("9:0").IsSuccess);

        ReferenceParseResult chapter = Engine.ParseReference("2");
        Assert.Equal(Engine.Verse(2, 0).Number, chapter.Range.First);
        Assert.Equal(Engine.Verse(2, 286).Number, chapter.Range.Last);
    }

    [Fact]
    public void SeventySixtyNineIsSpelledWithSin() =>
        Assert.Contains("بسطه", Words(7, 69));

    [Fact]
    public void SixtyEightOneOpensWithNunSpelledOut() =>
        Assert.Equal("نون", Words(68, 1)[0]);

    [Fact]
    public void MaLamIsOneWordInNinetySixFive()
    {
        Assert.Equal(["علم", "الانسن", "مالم", "يعلم"], Words(96, 5));

        VerseRange first5 = new(Engine.Verse(96, 1).Number, Engine.Verse(96, 5).Number);
        Assert.Equal(19, Engine.Statistics(first5).WordCount);
    }

    [Fact]
    public void MaLamStaysTwoWordsElsewhere() =>
        Assert.Contains(Words(2, 236), w => w == "لم");

    [Fact]
    public void ClassicWordJoinsAreNotApplied()
    {
        // The classic rule joins "بعد ما" in 2:181; this edition keeps its own spacing.
        string[] words = Words(2, 181);
        Assert.Contains("بعد", words);
        Assert.DoesNotContain("بعدما", words);
    }

    [Fact]
    public void AlFatihaKeepsItsStructure()
    {
        SelectionStatistics stats = Engine.Statistics(new VerseRange(1, 7));
        output.WriteLine($"Al-Fatiha value: {stats.Value}");
        Assert.Equal(7, stats.VerseCount);
        Assert.Equal(29, stats.WordCount);
        Assert.Equal(139, stats.LetterCount);
    }

    [Fact]
    public void ExcludingBismillahsRemovesThemFromEveryCount()
    {
        Chapter baqara = Engine.Chapters[1];
        var range = new VerseRange(baqara.FirstVerse, baqara.LastVerse);

        SelectionStatistics with = Engine.Statistics(range);
        SelectionStatistics without = Engine.Statistics(range, includeBasmalas: false);

        Assert.Equal(287, with.VerseCount);
        Assert.Equal(286, without.VerseCount);
        Assert.Equal(4, with.WordCount - without.WordCount);
        Assert.Equal(19, with.LetterCount - without.LetterCount);
        Assert.Equal(Engine.ValueOfVerse(baqara.FirstVerse), with.Value - without.Value);
    }

    [Fact]
    public void WholeBookWithAndWithoutBismillahs()
    {
        var all = new VerseRange(1, Engine.Verses.Count);
        SelectionStatistics with = Engine.Statistics(all);
        SelectionStatistics without = Engine.Statistics(all, includeBasmalas: false);

        output.WriteLine($"with: {with.VerseCount} verses, {with.WordCount} words, {with.LetterCount} letters, value {with.Value}");
        output.WriteLine($"without: {without.VerseCount} verses, {without.WordCount} words, {without.LetterCount} letters, value {without.Value}");
        Assert.Equal(6346, with.VerseCount);
        Assert.Equal(6234, without.VerseCount);
        Assert.Equal(112 * 4, with.WordCount - without.WordCount);
    }

    [Fact]
    public void AnExcludedBismillahAloneCountsAsNothing()
    {
        int zero = Engine.Verse(2, 0).Number;
        SelectionStatistics stats = Engine.Statistics(new VerseRange(zero, zero), includeBasmalas: false);

        Assert.Equal(0, stats.VerseCount);
        Assert.Equal(0, stats.Value);
        Assert.Null(Engine.ValueOfVerse(zero, includeBasmalas: false));
    }

    [Fact]
    public void SearchSkipsExcludedBismillahs()
    {
        int with = Engine.Search().Find("الرحمن", Search.Wordness.WholeWord).VerseCount;
        int without = Engine.Search(includeBasmalas: false).Find("الرحمن", Search.Wordness.WholeWord).VerseCount;
        Assert.Equal(112, with - without);
    }

    [Theory]
    [InlineData("Original_Alphabet_Primes1")]
    [InlineData("Simplified29_Alphabet_Primes1")]
    [InlineData("Original_Abjad_Gematria")]
    public void EveryNormalizedLetterHasAValue(string systemName)
    {
        // A character the letter stage fails to fold would silently value as
        // zero. This edition encodes marks differently from Tanzil, so check.
        Numerology.ValueSystem system = Engine.ValueSystem(systemName);
        Segmentation segmentation = Engine.Segmentation(system.TextModeName);

        char[] unvalued = segmentation.LetterChars.Distinct().Where(c => !system.Contains(c)).ToArray();
        Assert.True(unvalued.Length == 0,
            $"unvalued characters: {string.Join(" ", unvalued.Select(c => $"U+{(int)c:X4}"))}");
    }

    [Fact]
    public void EveryVerseAlignsForHighlighting()
    {
        TextPipeline pipeline = Engine.Pipeline();
        Segmentation segmentation = Engine.Segmentation();
        var unaligned = Engine.Verses
            .Where(v => DisplayWords.Align(Engine.Display(v), segmentation.VerseWords(v.Number - 1), pipeline) is null)
            .Select(v => $"{v.ChapterNumber}:{v.NumberInChapter}")
            .ToList();

        Assert.True(unaligned.Count == 0, string.Join(", ", unaligned.Take(20)));
    }

    [Fact]
    public void VerseOneIsNotSplitForAHeader()
    {
        VerseDisplay display = Engine.Display(Engine.Verse(2, 1));
        Assert.Null(display.Bismillah);
        Assert.Single(display.Words); // الٓمٓ
    }
}

internal static class SubmissionEngine
{
    public const string Collection = "submission-engine";

    public static readonly QuranCodeEngine Instance = new(TestPaths.SubmissionDatabase);
}
