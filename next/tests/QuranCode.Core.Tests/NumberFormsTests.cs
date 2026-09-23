using QuranCode.Core.Analysis;
using Xunit;

namespace QuranCode.Core.Tests;

public sealed class NumberFormsTests
{
    [Fact]
    public void DivisorsComeFromTheFactors() =>
        Assert.Equal([1L, 2, 3, 4, 6, 12], NumberForms.Divisors([2, 2, 3]));

    [Fact]
    public void TheIndexChainOf619()
    {
        // Worked by hand from the original's GenerateIndexChain: 619 is the 114th prime, 114 the 83rd composite, ...
        IndexChain chain = NumberForms.Chain(619)!;
        Assert.Equal("P114-C83-P23-P9-C4-C1", chain.Text);
        Assert.Equal(0b010011, chain.PrimesAsZero);
        Assert.Equal(0b110010, chain.PrimesAsZeroReversed);
        Assert.Equal(0b101100, chain.PrimesAsOne);
        Assert.Equal(0b001101, chain.PrimesAsOneReversed);
        Assert.Equal(234, chain.Sum);
        Assert.Null(NumberForms.Chain(1));
    }

    [Fact]
    public void FourNFormsAndTheirPlaces()
    {
        Assert.Equal(new FourNForm(true, 3, 2), NumberForms.FourN(13));   // 5, 13
        Assert.Equal(new FourNForm(false, 1, 1), NumberForms.FourN(3));
        Assert.Equal(new FourNForm(true, 2, 1), NumberForms.FourN(9));    // first composite of the form
        Assert.Null(NumberForms.FourN(12));
    }

    [Fact]
    public void SquareAndCubeSplitsAsTheOriginalListsThem()
    {
        PowerSplits thirteen = NumberForms.Splits(13)!;
        Assert.Equal([(2L, 3L)], thirteen.SquareSums);
        Assert.Equal([(6L, 7L)], thirteen.SquareDifferences);

        PowerSplits fortyFive = NumberForms.Splits(45)!;
        Assert.Equal([(3L, 6L)], fortyFive.SquareSums);
        Assert.Equal([(2L, 7L), (6L, 9L), (22L, 23L)], fortyFive.SquareDifferences);

        Assert.Contains((1L, 12L), NumberForms.Splits(1729)!.CubeSums);
        Assert.Contains((9L, 10L), NumberForms.Splits(1729)!.CubeSums);
        Assert.Null(NumberForms.Splits(2_000_000));
    }

    [Fact]
    public void PowersAndCarmichaelNumbers()
    {
        Assert.Equal(6, NumberForms.Power(64));
        Assert.Equal(2, NumberForms.Power(49));
        Assert.Null(NumberForms.Power(50));
        Assert.True(NumberForms.IsCarmichael(561, [3, 11, 17]));
        Assert.False(NumberForms.IsCarmichael(15, [3, 5]));
    }
}
