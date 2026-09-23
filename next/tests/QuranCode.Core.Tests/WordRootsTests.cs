using Xunit;

namespace QuranCode.Core.Tests;

public sealed class WordRootsTests : IDisposable
{
    private readonly QuranCodeEngine Submission = new(TestPaths.SubmissionDatabase);
    private readonly QuranCodeEngine Classic = new(TestPaths.ContentDatabase);

    public void Dispose()
    {
        Submission.Dispose();
        Classic.Dispose();
    }

    private static string[] RootsOf(QuranCodeEngine engine, int chapter, int verse, int word) =>
        engine.Roots.Of(engine.Verse(chapter, verse).Number, word).Select(engine.Roots.Text).ToArray();

    [Fact]
    public void ClassicCountsTheBismillahInVerseOne()
    {
        Assert.Equal(["ب", "إسم"], RootsOf(Classic, 2, 1, 0));
        Assert.Equal(["رحم", "رحمان"], RootsOf(Classic, 1, 1, 2));
    }

    [Fact]
    public void SubmissionKeepsTheBismillahInVerseZero()
    {
        Assert.Equal(["ب", "إسم"], RootsOf(Submission, 2, 0, 0));
        Assert.Equal(["رحم", "رحيم"], RootsOf(Submission, 2, 0, 3));
        Assert.Empty(RootsOf(Submission, 2, 1, 1)); // 2:1 is the single word الم
    }

    [Fact]
    public void AJoinedLegacyWordGivesItsRootsToBothDisplayWords()
    {
        // 8:6 displays "بَعْدَ مَا" as two words; the legacy file has one, بعدما.
        Assert.Equal(RootsOf(Submission, 8, 6, 3), RootsOf(Submission, 8, 6, 4));
        Assert.NotEmpty(RootsOf(Submission, 8, 6, 3));
    }

    [Fact]
    public void EveryVerseHasRootsForMostWords()
    {
        foreach (QuranCodeEngine engine in new[] { Classic, Submission })
        {
            int words = 0, rooted = 0;
            foreach (Content.Verse verse in engine.Verses)
            {
                foreach (int[] roots in engine.Roots.OfVerse(verse.Number))
                {
                    words++;
                    if (roots.Length > 0) rooted++;
                }
            }
            Assert.True(rooted > words * 0.95, $"{rooted} of {words} words have roots");
        }
    }
}
