using QuranCode.Core.Numbers;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Classification and ordinals, pinned to the figures the legacy Readme
/// publishes and to the edge cases of <c>Utilities/Numbers.cs</c>.
/// </summary>
public sealed class NumberClassTests
{
    [Theory]
    [InlineData(0, NumberClass.None)]
    [InlineData(1, NumberClass.Unit)]
    [InlineData(2, NumberClass.AdditivePrime)]
    [InlineData(3, NumberClass.AdditivePrime)]
    [InlineData(4, NumberClass.AdditiveComposite)]
    [InlineData(10, NumberClass.NonAdditiveComposite)] // digit sum 1 is the unit, not composite
    [InlineData(19, NumberClass.NonAdditivePrime)]     // digit sum 10
    [InlineData(29, NumberClass.AdditivePrime)]        // digit sum 11
    [InlineData(22, NumberClass.AdditiveComposite)]    // digit sum 4
    [InlineData(25, NumberClass.NonAdditiveComposite)] // digit sum 7
    [InlineData(113, NumberClass.AdditivePrime)]
    [InlineData(114, NumberClass.AdditiveComposite)]   // digit sum 6
    [InlineData(6229, NumberClass.AdditivePrime)]
    [InlineData(729139, NumberClass.AdditivePrime)]
    [InlineData(139297, NumberClass.AdditivePrime)]
    [InlineData(8317, NumberClass.AdditivePrime)]
    public void ClassifiesLikeTheLegacy(long value, NumberClass expected) =>
        Assert.Equal(expected, NumberTheory.Classify(value));

    [Theory]
    [InlineData(-1, NumberClass.Unit)]
    [InlineData(-7, NumberClass.AdditivePrime)]
    [InlineData(-19, NumberClass.NonAdditivePrime)]
    [InlineData(-114, NumberClass.AdditiveComposite)]
    public void NegativeValuesClassifyByMagnitude(long value, NumberClass expected) =>
        Assert.Equal(expected, NumberTheory.Classify(value));

    [Fact]
    public void IsPrimeUsesMagnitudeLikeTheLegacy()
    {
        Assert.True(NumberTheory.IsPrime(-7));
        Assert.False(NumberTheory.IsPrime(-1));
        Assert.False(NumberTheory.IsPrime(0));
    }

    [Theory]
    [InlineData(NumberClass.AdditivePrime, 114, 16)]
    [InlineData(NumberClass.AdditiveComposite, 114, 53)]
    [InlineData(NumberClass.NonAdditivePrime, 506, 42)]
    [InlineData(NumberClass.NonAdditiveComposite, 506, 131)]
    [InlineData(NumberClass.Prime, 619, 114)]
    [InlineData(NumberClass.Composite, 621, 506)]
    [InlineData(NumberClass.Prime, 1, 0)]
    [InlineData(NumberClass.Composite, 3, 0)]
    public void CountsMatchTheReadme(NumberClass numberClass, long limit, long expected) =>
        Assert.Equal(expected, NumberTheory.CountUpTo(numberClass, limit));

    [Theory]
    [InlineData(619, NumberClass.Prime, 114)]
    [InlineData(53, NumberClass.Prime, 16)]
    [InlineData(131, NumberClass.Prime, 32)]
    [InlineData(109, NumberClass.Prime, 29)]
    [InlineData(621, NumberClass.Composite, 506)]
    [InlineData(29, NumberClass.AdditivePrime, 7)]
    [InlineData(-619, NumberClass.Prime, 114)]
    public void OrdinalIsOneBased(long value, NumberClass numberClass, long expected) =>
        Assert.Equal(expected, NumberTheory.Ordinal(value, numberClass));

    [Theory]
    [InlineData(620, NumberClass.Prime)]
    [InlineData(619, NumberClass.Composite)]
    [InlineData(19, NumberClass.AdditivePrime)]
    [InlineData(0, NumberClass.Prime)]
    public void OrdinalIsZeroOutsideTheClass(long value, NumberClass numberClass) =>
        Assert.Equal(0, NumberTheory.Ordinal(value, numberClass));

    [Fact]
    public void OrdinalOfLargeValueIsNullBeyondTheIndexLimit() =>
        Assert.Null(NumberTheory.TryOrdinal(NumberTheory.OrdinalLimit + 1, NumberClass.Prime));

    [Fact]
    public void AllBookValueIsIndexable()
    {
        // The Original_Alphabet_Primes1 value of the whole book. Screens show
        // its class and ordinal, so it must sit inside the index limit.
        const long bookValue = 19_628_315;
        Assert.NotNull(NumberTheory.TryOrdinal(bookValue, NumberTheory.Classify(bookValue)));
    }

    [Theory]
    [InlineData(NumberClass.AdditivePrime, NumberClass.Prime)]
    [InlineData(NumberClass.NonAdditivePrime, NumberClass.Prime)]
    [InlineData(NumberClass.AdditiveComposite, NumberClass.Composite)]
    [InlineData(NumberClass.NonAdditiveComposite, NumberClass.Composite)]
    [InlineData(NumberClass.Unit, NumberClass.Unit)]
    public void FamilyGroupsSubclasses(NumberClass numberClass, NumberClass family) =>
        Assert.Equal(family, numberClass.Family());
}
