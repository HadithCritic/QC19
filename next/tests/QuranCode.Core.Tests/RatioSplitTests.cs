using QuranCode.Core.Analysis;
using QuranCode.Core.Search.Numbers;
using Xunit;

namespace QuranCode.Core.Tests;

public sealed class RatioSplitTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);

    public void Dispose() => _engine.Dispose();

    private RatioSplitResult FirstVerse(RatioMeasure measure, RatioBoundary boundary, double ratio) =>
        _engine.RatioSplits(1, UnitKind.Verses, ratio, measure, boundary)[0];

    [Fact]
    public void TheGoldenRatioSplitsTheBismillahInsideAWord()
    {
        // بسم الله الرحمن الرحيم has 19 letters; 19 × 0.618 rounds to 12, inside الرحمن.
        RatioSplitResult letter = FirstVerse(RatioMeasure.Letters, RatioBoundary.Letter, RatioSplit.GoldenRatio);
        Assert.True(letter.Colored);
        Assert.Equal(12, letter.FirstLetters);
        Assert.Equal(7, letter.SecondLetters);
        Assert.Equal(2, letter.Word);
        Assert.Equal(5, letter.LetterInWord);

        Assert.False(FirstVerse(RatioMeasure.Letters, RatioBoundary.Word, RatioSplit.GoldenRatio).Colored);
    }

    [Fact]
    public void AWordBoundaryColorsWhenTheSplitEndsAWord()
    {
        // 13/19 ends الرحمن.
        RatioSplitResult split = FirstVerse(RatioMeasure.Letters, RatioBoundary.Word, 13.0 / 19);
        Assert.True(split.Colored);
        Assert.Equal(6, split.LetterInWord);
    }

    [Fact]
    public void ByValueThePartsAddUp()
    {
        RatioSplitResult split = FirstVerse(RatioMeasure.Value, RatioBoundary.Letter, RatioSplit.GoldenRatio);
        long total = _engine.ValueOfVerse(1)!.Value;
        Assert.Equal(total, split.FirstValue + split.SecondValue);
        Assert.True(split.FirstValue <= Math.Round(total * RatioSplit.GoldenRatio));
    }

    [Fact]
    public void ScopesGiveTheirUnits()
    {
        Assert.Equal(8, _engine.RatioSplits(2, UnitKind.Verses, 0.5, RatioMeasure.Letters, RatioBoundary.Letter).Count(s => s.FirstVerse < 15));
        Assert.Single(_engine.RatioSplits(2, UnitKind.Chapters, 0.5, RatioMeasure.Letters, RatioBoundary.Verse));
        Assert.Single(_engine.RatioSplits(2, null, 0.5, RatioMeasure.Letters, RatioBoundary.Chapter));
        Assert.True(_engine.RatioSplits(2, UnitKind.Parts, 0.5, RatioMeasure.Letters, RatioBoundary.Verse).Count >= 3);
    }
}
