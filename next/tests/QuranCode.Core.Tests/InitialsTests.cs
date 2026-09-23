using Microsoft.Data.Sqlite;
using QuranCode.Core.Code19;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// The Quranic Initials: that the stated data matches the text, and the
/// counts the sources state.
/// </summary>
public sealed class InitialsTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);

    public void Dispose()
    {
        _engine.Dispose();
        SqliteConnection.ClearAllPools();
    }

    [Fact]
    public void ThereAre29ChaptersAnd14Letters()
    {
        Assert.Equal(29, QuranicInitials.Chapters.Count);
        Assert.Equal(14, QuranicInitials.Letters.Count);
    }

    /// <summary>
    /// Every chapter the data calls initialed is one the edition marks as
    /// initialed, and no other chapter is. This is what keeps the stated list
    /// honest against the text.
    /// </summary>
    [Fact]
    public void TheListMatchesTheEditionsOwnMarking()
    {
        int[] marked =
        [
            .. _engine.Chapters
                .Where(c => c.Initialization is not ("none" or "key"))
                .Select(c => c.Number),
        ];
        Assert.Equal(marked, QuranicInitials.Chapters.Select(c => c.Chapter));
    }

    /// <summary>
    /// Each chapter's opening verses begin with its stated initials. Two
    /// chapters are conventions rather than plain readings and are asserted
    /// separately below.
    /// </summary>
    [Theory]
    [MemberData(nameof(PlainChapters))]
    public void EachChapterOpensWithItsInitials(int number)
    {
        InitialedChapter chapter = QuranicInitials.Chapters.Single(c => c.Chapter == number);
        Assert.Equal(chapter.Letters, FirstWordOf(number, 1));
    }

    public static TheoryData<int> PlainChapters()
    {
        var data = new TheoryData<int>();
        foreach (InitialedChapter c in QuranicInitials.Chapters)
            if (c.Chapter is not (42 or 68)) data.Add(c.Chapter);
        return data;
    }

    /// <summary>Chapter 42 is the only one whose initials run over two verses.</summary>
    [Fact]
    public void Chapter42CarriesItsInitialsOverTwoVerses()
    {
        InitialedChapter chapter = QuranicInitials.Chapters.Single(c => c.Chapter == 42);
        Assert.Equal(2, chapter.Verses);
        Assert.Equal("حم", FirstWordOf(42, 1));
        Assert.Equal("عسق", FirstWordOf(42, 2));
        Assert.Equal("حمعسق", chapter.Letters);
    }

    /// <summary>
    /// Chapter 68's initial is the single letter ن, written out in the text as
    /// the word نون. Counting نون as three letters would make the initial ن و ن
    /// and give 15 distinct initials rather than 14, so the convention matters.
    /// </summary>
    [Fact]
    public void Chapter68WritesItsSingleInitialOut()
    {
        InitialedChapter chapter = QuranicInitials.Chapters.Single(c => c.Chapter == 68);
        Assert.Equal("ن", chapter.Letters);
        Assert.Equal("نون", FirstWordOf(68, 1));
        Assert.DoesNotContain('و', QuranicInitials.Letters);
    }

    /// <summary>
    /// The two Qaf-initialed chapters hold the same number of Qafs, and the
    /// two together make 114, the number of chapters (Mysterious Alphabets,
    /// printed p. 70). Both rows are also findings in the catalog.
    /// </summary>
    [Fact]
    public void BothQafChaptersHold57Qafs()
    {
        Assert.Equal([50, 42], QuranicInitials.With('ق').Select(c => c.Chapter).OrderByDescending(n => n));

        long fifty = CountOf(50, 'ق');
        long fortyTwo = CountOf(42, 'ق');
        Assert.Equal(57, fifty);
        Assert.Equal(57, fortyTwo);
        Assert.Equal(114, fifty + fortyTwo);
        Assert.Equal(0, (fifty + fortyTwo) % 19);
    }

    /// <summary>
    /// Appendix 1, simple facts 15 to 17: facts about the initials data
    /// itself rather than the text, so they check the stated list.
    /// </summary>
    [Fact]
    public void TheAppendixOneFactsAboutTheInitialsHold()
    {
        int sets = QuranicInitials.Chapters.Select(c => c.Letters).Distinct().Count();
        Assert.Equal(14, sets);
        Assert.Equal(57, QuranicInitials.Letters.Count + sets + QuranicInitials.Chapters.Count); // 14 + 14 + 29

        int chapterSum = QuranicInitials.Chapters.Sum(c => c.Chapter);
        Assert.Equal(822, chapterSum);
        Assert.Equal(19 * 44, chapterSum + sets);

        var initialed = QuranicInitials.Chapters.Select(c => c.Chapter).ToHashSet();
        int between = Enumerable.Range(2, 68 - 2 + 1).Count(n => !initialed.Contains(n));
        Assert.Equal(38, between);
    }

    [Fact]
    public void CountsCoverEveryInitialOfEveryChapter()
    {
        IReadOnlyList<InitialCount> all = QuranicInitials.AllCounts(_engine);
        Assert.Equal(QuranicInitials.Chapters.Sum(c => c.Letters.Distinct().Count()), all.Count);
        Assert.All(all, c => Assert.True(c.Count > 0, $"chapter {c.Chapter} has no {c.Letter}"));
    }

    private long CountOf(int chapter, char letter) =>
        QuranicInitials.CountsIn(_engine, QuranicInitials.Chapters.Single(c => c.Chapter == chapter))
            .Single(c => c.Letter == letter).Count;

    /// <summary>The first word of a verse, in Simplified29, which strips the marks.</summary>
    private string FirstWordOf(int chapter, int verseInChapter)
    {
        Core.Content.Segmentation segmentation = _engine.Segmentation("Simplified29");
        for (int v = 0; v < segmentation.VerseCount; v++)
        {
            if (segmentation.VerseChapter[v] != chapter) continue;
            if (segmentation.VerseNumberInChapter[v] != verseInChapter) continue;
            return segmentation.WordText(segmentation.VerseFirstWord[v]);
        }
        throw new InvalidOperationException($"{chapter}:{verseInChapter} is not in this edition");
    }
}
