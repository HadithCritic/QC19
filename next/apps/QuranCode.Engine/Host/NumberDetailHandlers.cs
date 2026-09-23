using System.Globalization;
using QuranCode.Core.Analysis;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>A number's divisors, powers, 4n±1 form, square and cube splits and CP index chain.</summary>
internal sealed partial class Handlers
{
    /// <summary>Divisors are listed up to this many; a count is always given when the number is factored.</summary>
    public const int MaxListedDivisors = 1000;

    public NumberDetailsDto NumberDetails(NumberParams p)
    {
        long value = ParseWholeNumber(p.Value);
        NumberDto number = Number(value);
        long magnitude = value == long.MinValue ? long.MaxValue : Math.Abs(value);

        IReadOnlyList<long>? divisors = number.Factors is { } factors && magnitude > 0 ? NumberForms.Divisors(factors) : null;
        FourNForm? fourN = NumberForms.FourN(magnitude);
        PowerSplits? splits = NumberForms.Splits(magnitude);
        IndexChain? chain = NumberForms.Chain(magnitude);

        return new NumberDetailsDto(
            number,
            divisors?.Count,
            divisors is { Count: <= MaxListedDivisors } ? divisors.Select(Text).ToArray() : null,
            divisors is null ? null : Text(divisors.Aggregate(0L, (sum, d) => sum > long.MaxValue - d ? long.MaxValue : sum + d)),
            NumberForms.Power(magnitude),
            number.Factors is { } f && NumberForms.IsCarmichael(magnitude, f),
            fourN is null ? null : new FourNDto(fourN.Plus ? "4n+1" : "4n-1", fourN.N, fourN.Ordinal),
            splits is null ? null : new SplitsDto(Pairs(splits.SquareSums), Pairs(splits.SquareDifferences), Pairs(splits.CubeSums), Pairs(splits.CubeDifferences)),
            chain is null ? null : new IndexChainDto(
                chain.Text, chain.Links.Count, chain.Sum,
                chain.PrimesAsZero, chain.PrimesAsZeroReversed, chain.PrimesAsOne, chain.PrimesAsOneReversed));
    }

    private static PairDto[] Pairs(IReadOnlyList<(long A, long B)> pairs) => pairs.Select(p => new PairDto(p.A, p.B)).ToArray();

    private static string Text(long value) => value.ToString(CultureInfo.InvariantCulture);
}
