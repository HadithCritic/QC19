using Microsoft.Data.Sqlite;
using QuranCode.Core.Code19;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

public sealed class SweepTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);

    public void Dispose()
    {
        _engine.Dispose();
        SqliteConnection.ClearAllPools();
    }

    private VerseRange Chapter(int number)
    {
        Chapter c = _engine.Chapters[number - 1];
        return new VerseRange(c.FirstVerse, c.LastVerse);
    }

    private IReadOnlyList<SweepTotal> Of(int chapter, bool basmalas = true, string system = "Simplified29_Alphabet_Primes1") =>
        Sweep.Of(_engine, Chapter(chapter), system, new CountingOptions { IncludeBasmalas = basmalas });

    private static long Total(IReadOnlyList<SweepTotal> totals, string label) =>
        totals.Single(t => t.Label == label).Value;

    /// <summary>The sweep carries the letter counts the Qaf findings rest on.</summary>
    [Fact]
    public void Chapter50HoldsItsQafs()
    {
        IReadOnlyList<SweepTotal> totals = Of(50);
        Assert.Equal(57, Total(totals, "ق"));
        Assert.Equal(1, Total(totals, "Chapters"));
        Assert.Equal(50, Total(totals, "Sum of chapter numbers"));
    }

    /// <summary>
    /// Chapter 42 has 53 verses and chapter 50 has 45; each number plus its
    /// verse count is 95, 19x5 (Quran Initial Count, Q). Numbered verses only.
    /// </summary>
    [Fact]
    public void AChaptersNumberPlusItsVersesIs95ForBothQafChapters()
    {
        Assert.Equal(95, Total(Of(42, basmalas: false), "Chapter numbers + verses"));
        Assert.Equal(95, Total(Of(50, basmalas: false), "Chapter numbers + verses"));
    }

    [Fact]
    public void VerseNumbersSumOverTheSelection()
    {
        // Al-Fatiha: 1 + 2 + ... + 7 = 28, and its verses are the first seven of the book.
        IReadOnlyList<SweepTotal> totals = Sweep.Of(_engine, new VerseRange(1, 7), "Simplified29_Alphabet_Primes1");
        Assert.Equal(28, Total(totals, "Sum of verse numbers"));
        Assert.Equal(28, Total(totals, "Sum of verse numbers in the book"));
        Assert.Equal(7, Total(totals, "Verses"));
    }

    /// <summary>The counting convention the reader chose changes the totals, as it should.</summary>
    [Fact]
    public void TheBasmalahToggleIsHonored()
    {
        Assert.Equal(Total(Of(50, basmalas: false), "Verses") + 1, Total(Of(50, basmalas: true), "Verses"));
        Assert.Equal(Total(Of(50, basmalas: false), "Words") + 4, Total(Of(50, basmalas: true), "Words"));
    }

    [Fact]
    public void EveryGroupIsPresent()
    {
        var groups = Of(19).Select(t => t.Group).ToHashSet();
        Assert.Equal(Enum.GetValues<SweepGroup>().ToHashSet(), groups);
    }
}
