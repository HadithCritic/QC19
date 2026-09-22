using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Compares the new engine against data captured from the legacy engine.
/// </summary>
/// <remarks>
/// The golden files under <c>next/tests/golden/</c> were produced by driving the
/// shipped QuranCode assemblies with <c>OracleDump</c>. They record what the
/// software actually computes, not what its source appears to say, and they are
/// the contract the replacement has to meet.
///
/// <para>
/// Per the brief (§21, §51), a difference here is either a bug in the new engine
/// or a deliberate change that must be documented. It is never something to
/// quietly accept.
/// </para>
/// </remarks>
public sealed class GoldenTests : IDisposable
{
    private readonly ContentRepository _content;

    public GoldenTests()
    {
        _content = new ContentRepository(TestPaths.ContentDatabase);
    }

    public void Dispose() => _content.Dispose();

    private TextPipeline Pipeline(string textMode) =>
        new(_content.GetTextMode(textMode));

    // -- structure --------------------------------------------------------

    [Fact]
    public void BookStructureMatchesLegacy()
    {
        Dictionary<string, string> golden = TestPaths.ReadKeyValue("book-structure.tsv", "metric");

        Assert.Equal(int.Parse(golden["chapters"]), _content.Chapters.Count);
        Assert.Equal(int.Parse(golden["verses"]), _content.Verses.Count);
    }

    [Fact]
    public void ChapterVerseCountsMatchLegacy()
    {
        foreach (string[] row in TestPaths.ReadRows("chapters.tsv"))
        {
            int number = int.Parse(row[0]);
            int expectedVerses = int.Parse(row[3]);

            Chapter chapter = _content.Chapters[number - 1];
            Assert.Equal(number, chapter.Number);
            Assert.Equal(expectedVerses, chapter.VerseCount);
        }
    }

    [Fact]
    public void ChapterNamesMatchLegacy()
    {
        foreach (string[] row in TestPaths.ReadRows("chapters.tsv"))
        {
            Chapter chapter = _content.Chapters[int.Parse(row[0]) - 1];
            Assert.Equal(row[1], chapter.Name);
        }
    }

    // -- value systems ----------------------------------------------------

    [Fact]
    public void LetterValueMapsMatchLegacy()
    {
        int compared = 0;
        foreach (IGrouping<string, string[]> group in
                 TestPaths.ReadRows("letter-values.tsv").GroupBy(r => r[0]))
        {
            ValueSystem system = _content.GetValueSystem(group.Key);
            foreach (string[] row in group)
            {
                char letter = row[1][0];
                long expected = long.Parse(row[3]);
                Assert.Equal(expected, system[letter]);
                compared++;
            }
        }
        Assert.True(compared >= 240, $"expected at least 240 comparisons, made {compared}");
    }

    [Fact]
    public void LetterValueSumsMatchLegacy()
    {
        foreach (string[] row in TestPaths.ReadRows("system-totals.tsv"))
        {
            ValueSystem system = _content.GetValueSystem(row[0]);
            Assert.Equal(long.Parse(row[4]), system.LetterValuesSum);
        }
    }

    // -- valuation --------------------------------------------------------

    /// <summary>
    /// The project's canonical smoke test. Al-Fatiha valuing to 8317 under
    /// Primalogy is documented in the legacy <c>Model/NumericalSystem.cs</c> and
    /// confirmed by the oracle, so it is ground truth from two directions.
    /// </summary>
    [Fact]
    public void AlFatihaValuesTo8317()
    {
        ValueSystem system = _content.GetValueSystem("Original_Alphabet_Primes1");

        string text = ChapterText(1, "Original");
        long value = ValueCalculator.Calculate(text, system);

        Assert.Equal(8317L, value);
    }

    [Fact]
    public void WholeBookValueMatchesLegacy()
    {
        Dictionary<string, string[]> totals = TestPaths.ReadRows("system-totals.tsv")
            .ToDictionary(r => r[0], r => r);

        string[] row = totals["Original_Alphabet_Primes1"];
        ValueSystem system = _content.GetValueSystem("Original_Alphabet_Primes1");

        long value = ValueCalculator.Calculate(BookText("Original"), system);

        Assert.Equal(long.Parse(row[5]), value);
    }

    [Fact]
    public void EveryChapterValueMatchesLegacy()
    {
        ValueSystem system = _content.GetValueSystem("Original_Alphabet_Primes1");

        var mismatches = new List<string>();
        foreach (string[] row in TestPaths.ReadRows("chapter-values.tsv"))
        {
            int number = int.Parse(row[0]);
            long expected = long.Parse(row[5]);
            long actual = ValueCalculator.Calculate(ChapterText(number, "Original"), system);

            if (actual != expected)
            {
                mismatches.Add($"chapter {number}: expected {expected}, got {actual}");
            }
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} of 114 chapters differ:\n  " +
            string.Join("\n  ", mismatches.Take(10)));
    }

    // -- digit transforms -------------------------------------------------

    [Theory]
    [InlineData(0, 0)]
    [InlineData(7, 7)]
    [InlineData(19, 10)]
    [InlineData(8317, 19)]
    [InlineData(19628315, 35)]
    [InlineData(-8317, -19)]
    public void DigitSumIsCorrect(long input, long expected) =>
        Assert.Equal(expected, ValueCalculator.DigitSum(input));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 9)]
    [InlineData(19, 1)]
    [InlineData(8317, 1)]
    [InlineData(-8317, -1)]
    public void DigitalRootIsCorrect(long input, long expected) =>
        Assert.Equal(expected, ValueCalculator.DigitalRoot(input));

    // -- safety -----------------------------------------------------------

    /// <summary>
    /// A profile using modifiers must fail loudly, not return a plausible
    /// number. Silently wrong arithmetic is the failure mode the brief singles
    /// out (§51).
    /// </summary>
    [Fact]
    public void UnimplementedModifiersThrowRatherThanMislead()
    {
        ValueSystem system = _content.GetValueSystem("Original_Alphabet_Primes1");
        var profile = CalculationProfile.Default with { AddPositions = true };

        Assert.Throws<NotSupportedException>(
            () => ValueCalculator.Calculate("test".AsSpan(), system, profile));
    }

    // -- helpers ----------------------------------------------------------

    private string ChapterText(int chapterNumber, string textMode)
    {
        Chapter chapter = _content.Chapters[chapterNumber - 1];
        IReadOnlyList<Verse> verses = _content.Verses;

        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < chapter.VerseCount; i++)
        {
            if (i > 0) builder.Append('\n');
            builder.Append(verses[chapter.FirstVerse - 1 + i].Text);
        }
        return Pipeline(textMode).Normalize(builder.ToString());
    }

    private string BookText(string textMode)
    {
        var builder = new System.Text.StringBuilder(1 << 20);
        foreach (Verse verse in _content.Verses)
        {
            if (builder.Length > 0) builder.Append('\n');
            builder.Append(verse.Text);
        }
        return Pipeline(textMode).Normalize(builder.ToString());
    }
}
