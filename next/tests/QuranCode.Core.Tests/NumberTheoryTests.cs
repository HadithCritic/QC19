using QuranCode.Core.Numbers;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Checks the computed sequences against the tables the legacy software ships.
/// </summary>
/// <remarks>
/// The claim being tested is that 63 MB of precomputed number tables under
/// <c>C#/Utilities/Numbers/</c> need not ship with the app because a sieve
/// reproduces them. That claim is only worth making if the output is
/// identical, so these compare element by element against the legacy files
/// rather than spot-checking.
///
/// <para>
/// The tests skip when the legacy source tree is not beside <c>next/</c>.
/// </para>
/// </remarks>
public sealed class NumberTheoryTests
{
    private static string? NumbersDirectory()
    {
        // The tables ship with the legacy Utilities project.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "C#", "Utilities", "Numbers");
            if (File.Exists(Path.Combine(candidate, "primes.txt"))) return candidate;
            directory = directory.Parent;
        }
        return null;
    }

    private static List<long>? ReadTable(string fileName)
    {
        string? directory = NumbersDirectory();
        if (directory is null) return null;

        string path = Path.Combine(directory, fileName);
        if (!File.Exists(path)) return null;

        var result = new List<long>();
        foreach (string line in File.ReadLines(path))
        {
            if (long.TryParse(line.Trim(), out long value)) result.Add(value);
        }
        return result;
    }

    [Fact]
    public void PrimesMatchShippedTable()
    {
        List<long>? expected = ReadTable("primes.txt");
        if (expected is null) return; // legacy source tree not present

        ReadOnlySpan<long> actual = NumberTheory.Primes((int)expected[^1]);

        Assert.Equal(expected.Count, actual.Length);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }

    [Fact]
    public void AdditivePrimesMatchShippedTable()
    {
        List<long>? expected = ReadTable("additive_primes.txt");
        if (expected is null) return;

        List<long> actual = NumberTheory.AdditivePrimes((int)expected[^1]);

        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }

    [Fact]
    public void CompositesMatchShippedTable()
    {
        List<long>? expected = ReadTable("composites.txt");
        if (expected is null) return;

        List<long> actual = NumberTheory.Composites((int)expected[^1]);

        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }

    [Fact]
    public void TriangularNumbersMatchShippedTable()
    {
        List<long>? expected = ReadTable("triangular_numbers.txt");
        if (expected is null) return;

        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], NumberTheory.Triangular(i + 1));
        }
    }

    /// <summary>
    /// The Primalogy figures documented in the legacy
    /// <c>Model/NumericalSystem.cs</c>: Al-Fatiha's structural counts and its
    /// value are all additive primes.
    /// </summary>
    [Theory]
    [InlineData(7)]      // verses
    [InlineData(29)]     // words
    [InlineData(139)]    // letters
    [InlineData(8317)]   // value
    public void AlFatihaFiguresAreAdditivePrimes(long value) =>
        Assert.True(NumberTheory.IsAdditivePrime(value), $"{value} should be an additive prime");

    [Theory]
    [InlineData(2, true)]
    [InlineData(4, false)]
    [InlineData(1_000_003, true)]
    [InlineData(1_000_001, false)]
    public void IsPrimeIsCorrect(long value, bool expected) =>
        Assert.Equal(expected, NumberTheory.IsPrime(value));

    [Fact]
    public void FactorizeIsCorrect()
    {
        Assert.Equal([2, 2, 3], NumberTheory.Factorize(12));
        Assert.Equal([8317], NumberTheory.Factorize(8317));
        Assert.Equal([3, 5, 7, 11, 13, 17, 19], NumberTheory.Factorize(3 * 5 * 7 * 11 * 13 * 17 * 19));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(19, 1)]
    [InlineData(8317, 1)]
    public void DigitalRootIsCorrect(long value, long expected) =>
        Assert.Equal(expected, NumberTheory.DigitalRoot(value));
}
