using Microsoft.Data.Sqlite;
using QuranCode.Core.Code19;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
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
    public void KhalifasTableCoversEveryInitial()
    {
        int expected = QuranicInitials.Chapters.Sum(c => c.Letters.Distinct().Count());
        Assert.Equal(expected, QuranicInitials.Published.Count);
        Assert.All(QuranicInitials.Chapters, c => Assert.All(c.Letters.Distinct(), l =>
            Assert.True(QuranicInitials.Published.ContainsKey((c.Chapter, l)), $"no published figure for {l} in {c.Chapter}")));
    }

    /// <summary>
    /// The text reproduces every one of Khalifa's per-letter figures except
    /// alif, and ل in chapters 11 and 30, where his printout has one ل fewer
    /// than the text. Anything else disagreeing is a regression.
    /// </summary>
    [Fact]
    public void OnlyTheKnownFiguresDisagree()
    {
        var disagree = QuranicInitials.AllCounts(_engine)
            .Where(c => !c.Matches).Select(c => $"{c.Chapter}:{c.Letter}").ToHashSet();
        var known = QuranicInitials.Chapters
            .Where(c => c.Letters.Contains('ا')).Select(c => $"{c.Chapter}:ا")
            .Append("11:ل").Append("30:ل").ToHashSet();
        Assert.True(known.SetEquals(disagree), $"disagreeing: {string.Join(" ", disagree.Order())}");
    }

    /// <summary>
    /// Khalifa's verse-by-verse alif counts, transcribed from Quran: Visual
    /// Presentation of the Miracle, cover every verse of the 13 alif chapters,
    /// verse 0 included, and add up to his published figure for each.
    /// </summary>
    [Fact]
    public void TheQvpAlifTableAddsUpToEachPublishedTotal()
    {
        string path = Path.Combine(TestPaths.GoldenDirectory, "..", "..", "data", "sources", "qvp", "alif.tsv");
        var rows = File.ReadLines(path)
            .Where(l => l.Length > 0 && char.IsDigit(l[0]))
            .Select(l => l.Split('\t').Select(int.Parse).ToArray())
            .ToArray();

        foreach (InitialedChapter chapter in QuranicInitials.Chapters.Where(c => c.Letters.Contains('ا')))
        {
            int[][] verses = [.. rows.Where(r => r[0] == chapter.Chapter)];
            Core.Content.Chapter c = _engine.Chapters[chapter.Chapter - 1];
            Assert.Equal(c.RowCount, verses.Length);
            Assert.Equal(Enumerable.Range(0, c.RowCount), verses.Select(r => r[1]));
            Assert.Equal(QuranicInitials.Published[(chapter.Chapter, 'ا')], verses.Sum(r => r[2]));
        }
    }

    private static readonly CountingOptions KhalifaHamza = new() { IncludeBasmalas = true, HamzaAboveLine = true };

    private long AlifOf(int chapter) =>
        FindingEvaluator.Evaluate(_engine, new Finding(
            "t", "t", 0, FindingMeasure.LetterOccurrences, new FindingScope(chapter), "ا",
            true, "Simplified29", RuleBasis.Stated, "t", "t", HamzaAsAlif: true)).Computed;

    /// <summary>
    /// Simplified29 with the hamza above a line kept and every hamza on no
    /// seat counted as alif gives Khalifa's alif total over the 13 chapters
    /// exactly, though each chapter is off by a few (docs/research/alif-counting.md).
    /// </summary>
    [Fact]
    public void KhalifasHamzaConventionGivesHisAlifTotal()
    {
        int[] chapters = [.. QuranicInitials.Chapters.Where(c => c.Letters.Contains('ا')).Select(c => c.Chapter)];
        Assert.Equal(17152, chapters.Sum(c => QuranicInitials.Published[(c, 'ا')]));
        Assert.Equal(17152, chapters.Sum(AlifOf));
        Assert.Equal(4504, AlifOf(2));
        Assert.Equal(605, AlifOf(13)); // exact
        Assert.Equal(493, AlifOf(15)); // exact
    }

    [Fact]
    public void KhalifasHamzaConventionMatchesHisPrintoutVerseByVerse()
    {
        string path = Path.Combine(TestPaths.GoldenDirectory, "..", "..", "data", "sources", "qvp", "alif.tsv");
        var khalifa = File.ReadLines(path)
            .Where(l => l.Length > 0 && char.IsDigit(l[0]))
            .Select(l => l.Split('\t').Select(int.Parse).ToArray())
            .ToDictionary(r => (r[0], r[1]), r => r[2]);

        Segmentation s = _engine.Segmentation("Simplified29", KhalifaHamza);
        int exact = 0;
        for (int v = 0; v < s.VerseCount; v++)
        {
            if (!khalifa.TryGetValue((s.VerseChapter[v], s.VerseNumberInChapter[v]), out int published)) continue;
            string text = string.Concat(s.VerseWords(v));
            if (text.Count(c => c is 'ا' or 'ء') == published) exact++;
        }
        Assert.Equal(1340, exact);
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
