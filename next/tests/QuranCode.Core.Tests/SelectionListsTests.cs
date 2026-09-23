using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Phase 5: word and letter lists, C/V sums, symmetry and the research word lists.</summary>
public sealed class SelectionListsTests : IDisposable
{
    private readonly QuranCodeEngine _classic = new(TestPaths.ContentDatabase);
    private readonly QuranCodeEngine _submission = new(TestPaths.SubmissionDatabase);

    public void Dispose()
    {
        _classic.Dispose();
        _submission.Dispose();
    }

    private static VerseRange Chapter(QuranCodeEngine e, int n) => new(e.Chapters[n - 1].FirstVerse, e.Chapters[n - 1].LastVerse);

    private static VerseRange Book(QuranCodeEngine e) => new(1, e.Verses.Count);

    [Fact]
    public void WordFrequenciesCountEveryWordOnce()
    {
        IReadOnlyList<WordCount> words = _submission.WordFrequencies(Chapter(_submission, 1));
        Assert.Equal(29, words.Sum(w => w.Count));
        Assert.Equal(words.Select(w => w.Count).OrderDescending(), words.Select(w => w.Count));
        Assert.Contains(words, w => w.Count == 2); // الرحمن and الرحيم, twice each

        IReadOnlyList<WordCount> marked = _submission.WordFrequencies(Chapter(_submission, 1), withMarks: true);
        Assert.Equal(29, marked.Sum(w => w.Count));
        Assert.True(marked.Count >= words.Count);
    }

    [Fact]
    public void LetterStatisticsFollowTheLetters()
    {
        IReadOnlyList<LetterStatistic> letters = _submission.LetterStatistics(Chapter(_submission, 1));
        Assert.Equal(139, letters.Sum(l => l.Count));
        Assert.Equal(Enumerable.Range(1, letters.Count), letters.Select(l => l.Order));

        // The first letter is at position 1 of the chapter's text; a letter seen
        // once has no distance, and a distance sum never exceeds the span.
        Assert.Equal(1, _submission.LetterStatistics(Chapter(_submission, 1), scope: LetterPositionScope.Chapter)[0].PositionSum > 0 ? 1 : 0);
        Assert.All(letters.Where(l => l.Count == 1), l => Assert.Equal(0, l.DistanceSum));
        Assert.All(letters, l => Assert.InRange(l.DistanceSum, 0, 138));
    }

    [Fact]
    public void ChapterSumsMatchTheOriginalsHelpFile()
    {
        // Help\114.txt: over the whole book, C+V gives d/u = 7906/4885 = 1.6184.
        CvSums sums = _classic.ChapterSums(Book(_classic));
        Assert.Equal(114, sums.Count);
        Assert.Equal(6555, sums.C.Sum);
        Assert.Equal(6236, sums.V.Sum);
        Assert.Equal(12791, sums.Plus.Sum);
        Assert.Equal(7906.0 / 4885, sums.Plus.Ratio!.Value, 6);
        Assert.Equal(sums.C.Sum, sums.C.Odd + sums.C.Even);
        Assert.Equal(sums.C.Sum - 1, sums.C.Prime + sums.C.Composite); // chapter 1 is neither
    }

    [Fact]
    public void VerseSumsPairChaptersWithVerseNumbers()
    {
        CvSums sums = _classic.VerseSums(Chapter(_classic, 1));
        Assert.Equal(7, sums.Count);
        Assert.Equal(7, sums.C.Sum);
        Assert.Equal(28, sums.V.Sum);
        Assert.Equal(-21, sums.Minus.Sum);
        Assert.Equal(21, _classic.VerseSums(Chapter(_classic, 1), absoluteDifference: true).Minus.Sum);
    }

    [Fact]
    public void SymmetryFindsWhereBothEndsAgree()
    {
        SymmetryResult result = SelectionLists.Symmetry([1, 2, 3, 3, 2, 1], withBoundaries: false);
        Assert.Equal(5, result.Points.Count);
        Assert.Equal(new SymmetryPoint(3, 6, 6, 10), result.Points[2]);
        Assert.Equal(5 * 100.0 / 6, result.Percent, 6);

        SymmetryResult bounded = SelectionLists.Symmetry([1, 2, 3, 3, 2, 1], withBoundaries: true);
        Assert.Equal(7, bounded.Points.Count);
        Assert.Equal(5 * 100.0 / 6, bounded.Percent, 6);

        Assert.NotEmpty(_submission.Symmetry(Chapter(_submission, 1), SymmetryKind.VerseLetters, true).Points);
    }

    [Fact]
    public void ResearchWordLists()
    {
        ResearchTable allah = _classic.WordList(WordListKind.Allah);
        Assert.Equal(10, allah.Columns.Count);
        Assert.All(allah.Rows, r => Assert.Contains("له", r[6]));
        Assert.True(allah.Rows.Count > 2600, $"{allah.Rows.Count} Allah words");

        // دَكًّا دَكًّا (89:21) is a word repeated at once.
        ResearchTable repeated = _submission.WordList(WordListKind.Repeated, gap: 0);
        Assert.Contains(repeated.Rows, r => r[4] == "89" && r[5] == "21");

        // Every neighboring repeat inside a verse is also a double word.
        ResearchTable doubles = _submission.WordList(WordListKind.Double);
        Assert.Equal(0, doubles.Rows.Count % 2);
        Assert.True(doubles.Rows.Count / 2 >= repeated.Rows.Count);

        // Features.txt gives 2816 for the full statistics of Allah and its derivatives.
        AllahSummary summary = _classic.AllahSummary(Book(_classic));
        Assert.Equal(2816, summary.Total);
        Assert.Equal(2816, allah.Rows.Count);

        // The Submission edition leaves out 9:128-129, and 9:129 has الله once.
        Assert.Equal(2815, _submission.AllahSummary(Book(_submission)).Total);
    }
}
