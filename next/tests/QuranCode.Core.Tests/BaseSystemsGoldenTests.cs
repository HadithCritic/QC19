using System.Globalization;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// The Base letter-value systems read a word's letter values as digits; checked
/// against every chapter, the book and sample verses from the legacy engine.
/// </summary>
public sealed class BaseSystemsGoldenTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.ContentDatabase);

    public void Dispose() => _engine.Dispose();

    [Fact]
    public void EveryBaseSystemMatchesTheLegacyValues()
    {
        var mismatches = new List<string>();
        int rows = 0;
        foreach (string[] row in TestPaths.ReadRows("base-systems.tsv"))
        {
            string system = row[0], unit = row[1];
            long expected = long.Parse(row[2], CultureInfo.InvariantCulture);
            string textMode = _engine.ValueSystem(system).TextModeName;

            long actual = unit switch
            {
                "0" => _engine.ValueOfBook(system, textMode),
                _ when unit.StartsWith("verse:", StringComparison.Ordinal) =>
                    _engine.ValueOfVerse(int.Parse(unit[6..], CultureInfo.InvariantCulture), system, textMode) ?? -1,
                _ => _engine.ValueOfChapter(int.Parse(unit, CultureInfo.InvariantCulture), system, textMode),
            };
            rows++;
            if (actual != expected) mismatches.Add($"{system} {unit}: {actual}, expected {expected}");
        }
        Assert.Equal(2440, rows);
        Assert.Empty(mismatches.Take(10));
    }
}
