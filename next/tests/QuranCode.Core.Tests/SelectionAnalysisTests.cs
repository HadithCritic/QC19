using QuranCode.Core.Analysis;
using QuranCode.Core.Code19;
using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Statistics over exact selections. Whole verses must give exactly what the
/// verse-range statistics give; partial spans must count only their letters.
/// </summary>
[Collection(SharedEngine.Collection)]
public sealed class SelectionAnalysisTests
{
    private static QuranCodeEngine Engine => SharedEngine.Instance;

    private const string Gematria = "Simplified29_Abjad_Gematria";

    [Theory]
    [InlineData(QuranCodeEngine.DefaultValueSystem)]
    [InlineData(Gematria)]
    public void EveryChapterSelectedAsAChapterMatchesItsStatistics(string system)
    {
        foreach (Chapter chapter in Engine.Chapters)
        {
            SelectionStatistics expected = Engine.Statistics(new VerseRange(chapter.FirstVerse, chapter.LastVerse), system);
            SelectionStatistics actual = Analyze(chapter.Number.ToString(), system).Statistics!;
            AssertSame(expected, actual);
        }
    }

    [Theory]
    [InlineData("2:255", 262, 262)]
    [InlineData("1:7-2:2", 7, 9)]
    [InlineData("1:1:w1-1:2:w4", 1, 2)] // every word of 1:1 and 1:2 is the two verses
    public void WholeVersesMatchTheirStatistics(string address, int first, int last) =>
        AssertSame(Engine.Statistics(new VerseRange(first, last), Gematria), Analyze(address, Gematria).Statistics!);

    /// <summary>
    /// The clamped calculator over whole verses must be the aggregate path,
    /// in every calculation mode and with every modifier family on.
    /// </summary>
    [Theory]
    [InlineData(CalculationMode.SumOfLetterValues, false, false)]
    [InlineData(CalculationMode.SumOfLetterValueDigitSums, true, false)]
    [InlineData(CalculationMode.SumOfWordValueDigitSums, false, true)]
    [InlineData(CalculationMode.SumOfWordValueDigitalRoots, true, true)]
    public void LetterRunsOverWholeVersesEqualTheAggregatePath(CalculationMode mode, bool positions, bool distances)
    {
        Segmentation s = Engine.Segmentation("Simplified29");
        ValueSystem system = Engine.ValueSystem(Gematria);
        var profile = new CalculationProfile
        {
            Mode = mode,
            AlternateLetterValues = positions,
            AlternateWordValues = distances,
            AddPositions = positions,
            AbsolutePositions = positions,
            AddDistancesToPrevious = distances,
        };
        var modifiers = new ModifierSet
        {
            LetterLNumber = true, LetterWNumber = true, LetterVNumber = true, LetterCNumber = true,
            LetterLDistance = true, LetterWDistance = true, LetterVDistance = true, LetterCDistance = true,
            WordWNumber = true, WordVNumber = true, WordCNumber = true,
            WordWDistance = true, WordVDistance = true, WordCDistance = true,
            VerseVNumber = true, VerseCNumber = true,
        };

        foreach ((int first, int count) in new[] { (0, 7), (7, 286), (262, 3), (6000, 100) })
        {
            int lastWord = s.VerseFirstWord[first + count - 1] + s.VerseWordCount[first + count - 1] - 1;
            long expected = SegmentedCalculator.ValueOfVerses(s, first, count, system, profile, modifiers);
            long actual = SegmentedCalculator.ValueOfLetters(
                s, s.WordFirstLetter[s.VerseFirstWord[first]], s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord] - 1,
                system, profile, modifiers);
            Assert.Equal(expected, actual);
        }
    }

    [Theory]
    [InlineData("1:1:w2:l2")]
    [InlineData("1:1:w2:l2-1:1:w2:l3")]
    [InlineData("1:1:w2:l3-1:1:w3:l2")]
    [InlineData("2:255:w4:l2-2:257:w8:l3")]
    [InlineData("1:7:w9-2:1:w1")]
    public void APartialValueIsTheSumOfItsLetters(string address)
    {
        SelectionAnalysis analysis = Analyze(address, Gematria);
        CountedSpan span = analysis.Resolution.Span!;
        Segmentation s = Engine.Segmentation("Simplified29");
        ValueSystem system = Engine.ValueSystem(Gematria);

        long sum = 0;
        for (int l = span.FirstLetter; l <= span.LastLetter; l++) sum += system[s.LetterChars[l]];

        Assert.False(span.IsVerseAligned);
        Assert.Equal(sum, analysis.Statistics!.Value);
        Assert.Equal(span.LetterCount, analysis.Statistics.LetterFrequencies.Sum(f => f.Count));
    }

    [Fact]
    public void OneLetterCountsAsOneOfEverything()
    {
        SelectionStatistics stats = Analyze("1:1:w2:l2", Gematria).Statistics!;

        Assert.Equal(1, stats.ChapterCount);
        Assert.Equal(1, stats.VerseCount);
        Assert.Equal(1, stats.WordCount);
        Assert.Equal(1, stats.LetterCount);
        Assert.Equal(30, stats.Value); // ل
        Assert.Equal(new VerseRange(1, 1), stats.Range);
    }

    [Fact]
    public void EndpointsAreCountedPositions()
    {
        SelectionAnalysis analysis = Analyze("1:1:w2:l2-1:2:w1", Gematria);

        Assert.Equal(new CountedPosition(1, 1, 1, 2, 2, 2, 2, 5, 5, 5), analysis.Start);
        Assert.Equal(1, analysis.End!.Chapter);
        Assert.Equal(2, analysis.End.Verse);
        Assert.Equal(1, analysis.End.WordInVerse);
        Assert.Equal(5, analysis.End.AbsoluteWord);
    }

    [Fact]
    public void DistinctWordsCountTheWordsTouched()
    {
        Assert.Equal(1, Analyze("1:1:w2:l1-1:1:w2:l2", Gematria).DistinctWords);
        Assert.Equal(4, Analyze("1:1", Gematria).DistinctWords);
    }

    [Fact]
    public void SpanListsCountOnlyTheSpan()
    {
        CountedSpan span = Span("1:1:w2:l2-1:1:w3:l3");

        Assert.Equal(2, Engine.WordFrequencies(span, Gematria).Sum(w => w.Count));
        Assert.Equal(span.LetterCount, Engine.LetterStatistics(span, Gematria).Sum(l => l.Count));
        SymmetryResult wordLetters = Engine.Symmetry(span, SymmetryKind.WordLetters, true, Gematria);
        Assert.Equal(2, wordLetters.Units);
        Assert.Equal(3 + 3, wordLetters.Points[^1].Total); // with boundaries, the last point is the whole
        Assert.Equal(1, Engine.AllahSummary(span, Gematria).Allah);
        Assert.Equal(new VerseRange(1, 1), Engine.VersesOf(span, Gematria));
    }

    [Fact]
    public void SymmetryOfAPartialVerseCountsItsSelectedLetters()
    {
        CountedSpan span = Span("1:1:w3:l2-1:2:w1:l2");
        SymmetryResult verseLetters = Engine.Symmetry(span, SymmetryKind.VerseLetters, true, Gematria);
        SymmetryResult verseWords = Engine.Symmetry(span, SymmetryKind.VerseWords, true, Gematria);

        Assert.Equal(2, verseLetters.Units);
        Assert.Equal(span.LetterCount, verseLetters.Points[^1].Total);
        Assert.Equal(2, verseWords.Units);
        Assert.Equal(2 + 1, verseWords.Points[^1].Total); // two words of 1:1 and one of 1:2
    }

    [Fact]
    public void TheSweepOfASelectionUsesItsCounts()
    {
        CountedSpan span = Span("1:1:w2-1:1:w3");
        IReadOnlyList<SweepTotal> totals = Sweep.Of(Engine, span, Gematria);

        Assert.Equal(2, totals.Single(t => t.Label == "Words").Value);
        Assert.Equal(1, totals.Single(t => t.Label == "Sum of verse numbers").Value);
    }

    [Fact]
    public void ChangingTheSystemKeepsTheSelection()
    {
        SelectionAnalysis gematria = Analyze("2:255:w4-2:257:w8", Gematria);
        SelectionAnalysis primes = Analyze("2:255:w4-2:257:w8", "Simplified29_Alphabet_Primes1");

        Assert.Equal(gematria.Resolution.Span, primes.Resolution.Span);
        Assert.NotEqual(gematria.Statistics!.Value, primes.Statistics!.Value);
    }

    private static SelectionAnalysis Analyze(string address, string system) =>
        Engine.Analyze(SelectionResolverTests.Parse(address), system);

    private static CountedSpan Span(string address) =>
        Engine.ResolveFor(SelectionResolverTests.Parse(address), Gematria).Span!;

    private static void AssertSame(SelectionStatistics expected, SelectionStatistics actual)
    {
        Assert.Equal(expected.Range, actual.Range);
        Assert.Equal(expected.ChapterCount, actual.ChapterCount);
        Assert.Equal(expected.VerseCount, actual.VerseCount);
        Assert.Equal(expected.WordCount, actual.WordCount);
        Assert.Equal(expected.LetterCount, actual.LetterCount);
        Assert.Equal(expected.DistinctLetterCount, actual.DistinctLetterCount);
        Assert.Equal(expected.Value, actual.Value);
        Assert.Equal(expected.LetterFrequencies, actual.LetterFrequencies);
        Assert.Equal(expected.Position, actual.Position);
    }
}

[Collection(SubmissionEngine.Collection)]
public sealed class SubmissionSelectionAnalysisTests
{
    [Fact]
    public void AnUncountedSelectionHasNoStatistics()
    {
        SelectionAnalysis analysis = SubmissionEngine.Instance.Analyze(
            SelectionResolverTests.Parse("2:0"), counting: new CountingOptions { IncludeBasmalas = false });

        Assert.True(analysis.Resolution.IsSuccess);
        Assert.Null(analysis.Statistics);
        Assert.NotEmpty(analysis.Resolution.Notes);
    }

    [Fact]
    public void NinetySixOneToFiveIsNineteenWords()
    {
        // The edition's reason for counting ما لم as one word.
        SelectionAnalysis analysis = SubmissionEngine.Instance.Analyze(SelectionResolverTests.Parse("96:1-96:5"));
        Assert.Equal(19, analysis.Statistics!.WordCount);
    }
}
