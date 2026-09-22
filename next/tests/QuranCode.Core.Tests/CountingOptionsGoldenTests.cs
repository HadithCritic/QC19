using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
using Xunit;
using Xunit.Abstractions;

namespace QuranCode.Core.Tests;

/// <summary>
/// The Statistics-panel text options against the original: every chapter and
/// the whole book, for each option set and system in
/// <c>golden/counting-options.tsv</c>.
/// </summary>
[Collection(SharedEngine.Collection)]
public sealed class CountingOptionsGoldenTests(ITestOutputHelper output)
{
    private static readonly Dictionary<string, CountingOptions> Sets = new()
    {
        ["default"] = CountingOptions.Default,
        ["no_bismillah"] = new() { IncludeBasmalas = false },
        ["waw_as_word"] = new() { WawAsWord = true },
        ["shadda_as_letter"] = new() { ShaddaAsLetter = true },
        ["hamza_above_line"] = new() { HamzaAboveLine = true },
        ["elf_above_line"] = new() { ElfAboveLine = true },
        ["yaa_above_line"] = new() { YaaAboveLine = true },
        ["noon_above_line"] = new() { NoonAboveLine = true },
        ["waw_and_shadda"] = new() { WawAsWord = true, ShaddaAsLetter = true },
        ["all"] = new()
        {
            IncludeBasmalas = false, WawAsWord = true, ShaddaAsLetter = true,
            HamzaAboveLine = true, ElfAboveLine = true, YaaAboveLine = true, NoonAboveLine = true,
        },
    };

    public static TheoryData<string, string> Cases()
    {
        var cases = new TheoryData<string, string>();
        foreach (var pair in TestPaths.ReadRows("counting-options.tsv").Select(r => (r[0], r[1])).Distinct())
        {
            cases.Add(pair.Item1, pair.Item2);
        }
        return cases;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void MatchesTheOriginal(string options, string system)
    {
        QuranCodeEngine engine = SharedEngine.Instance;
        CountingOptions counting = Sets[options];
        string textMode = engine.ValueSystem(system).TextModeName;

        var mismatches = new List<string>();
        foreach (string[] row in TestPaths.ReadRows("counting-options.tsv").Where(r => r[0] == options && r[1] == system))
        {
            int chapter = int.Parse(row[2]);
            (long words, long letters, long value) expected = (long.Parse(row[4]), long.Parse(row[5]), long.Parse(row[6]));

            VerseRange range = chapter == 0
                ? new VerseRange(1, engine.Verses.Count)
                : new VerseRange(engine.Chapters[chapter - 1].FirstVerse, engine.Chapters[chapter - 1].LastVerse);
            SelectionStatistics stats = engine.Statistics(range, system, counting: counting);
            long actualValue = chapter == 0
                ? engine.ValueOfBook(system, textMode, counting: counting)
                : stats.Value;

            (long, long, long) actual = (stats.WordCount, stats.LetterCount, actualValue);
            if (actual != expected)
            {
                mismatches.Add($"chapter {chapter}: expected words/letters/value {expected}, got {actual}");
            }
        }

        foreach (string mismatch in mismatches.Take(10)) output.WriteLine(mismatch);
        Assert.True(mismatches.Count == 0, $"{mismatches.Count} rows differ from the original for {options} / {system}");
    }
}
