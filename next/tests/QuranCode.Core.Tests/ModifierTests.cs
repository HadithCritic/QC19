using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Checks every value modifier against the legacy engine.
/// </summary>
/// <remarks>
/// The golden file <c>modifiers.tsv</c> was produced by setting each flag on a
/// live legacy <c>NumericalSystem</c> and recording the result. The flags are
/// never persisted, so observation was the only way to learn what they compute.
///
/// <para>
/// Cases change one thing at a time so a failure identifies one modifier, with
/// a few <c>combo_*</c> cases at the end because the modifiers are additive and
/// the interactions need covering.
/// </para>
/// </remarks>
public sealed class ModifierTests : IDisposable
{
    private readonly ContentRepository _content;
    private readonly Segmentation _segmentation;
    private readonly ValueSystem _system;

    public ModifierTests()
    {
        _content = new ContentRepository(TestPaths.ContentDatabase);
        var pipeline = new TextPipeline(_content.GetTextMode("Original"));
        _segmentation = Segmentation.Build(_content.Verses, pipeline);
        _system = _content.GetValueSystem("Original_Alphabet_Primes1");
    }

    public void Dispose() => _content.Dispose();

    /// <summary>
    /// Translates a golden case name into the profile and modifiers it stands
    /// for. Kept as an explicit table so the mapping is reviewable next to the
    /// data it reproduces.
    /// </summary>
    private static (CalculationProfile Profile, ModifierSet Modifiers) Configure(string caseName) =>
        caseName switch
        {
            "default" => (CalculationProfile.Default, ModifierSet.None),

            "mode_letter_digit_sums" => (CalculationProfile.Default with
            { Mode = CalculationMode.SumOfLetterValueDigitSums }, ModifierSet.None),
            "mode_letter_digital_roots" => (CalculationProfile.Default with
            { Mode = CalculationMode.SumOfLetterValueDigitalRoots }, ModifierSet.None),
            "mode_word_digit_sums" => (CalculationProfile.Default with
            { Mode = CalculationMode.SumOfWordValueDigitSums }, ModifierSet.None),
            "mode_word_digital_roots" => (CalculationProfile.Default with
            { Mode = CalculationMode.SumOfWordValueDigitalRoots }, ModifierSet.None),

            "alternate_letters" => (CalculationProfile.Default with
            { AlternateLetterValues = true }, ModifierSet.None),
            "alternate_words" => (CalculationProfile.Default with
            { AlternateWordValues = true }, ModifierSet.None),
            "alternate_verses" => (CalculationProfile.Default with
            { AlternateVerseValues = true }, ModifierSet.None),

            "pos_letter_L" => (Positions(false), new ModifierSet { LetterLNumber = true }),
            "pos_letter_W" => (Positions(false), new ModifierSet { LetterWNumber = true }),
            "pos_letter_V" => (Positions(false), new ModifierSet { LetterVNumber = true }),
            "pos_letter_C" => (Positions(false), new ModifierSet { LetterCNumber = true }),
            "pos_word_W" => (Positions(false), new ModifierSet { WordWNumber = true }),
            "pos_word_V" => (Positions(false), new ModifierSet { WordVNumber = true }),
            "pos_word_C" => (Positions(false), new ModifierSet { WordCNumber = true }),
            "pos_verse_V" => (Positions(false), new ModifierSet { VerseVNumber = true }),
            "pos_verse_C" => (Positions(false), new ModifierSet { VerseCNumber = true }),
            "pos_chapter_C" => (Positions(false), new ModifierSet { ChapterCNumber = true }),

            "abs_letter_L" => (Positions(true), new ModifierSet { LetterLNumber = true }),
            "abs_letter_W" => (Positions(true), new ModifierSet { LetterWNumber = true }),
            "abs_letter_V" => (Positions(true), new ModifierSet { LetterVNumber = true }),
            "abs_letter_C" => (Positions(true), new ModifierSet { LetterCNumber = true }),

            "dist_prev_letter_L" => (Distances(), new ModifierSet { LetterLDistance = true }),
            "dist_prev_letter_W" => (Distances(), new ModifierSet { LetterWDistance = true }),
            "dist_prev_word_W" => (Distances(), new ModifierSet { WordWDistance = true }),
            "dist_prev_verse_V" => (Distances(), new ModifierSet { VerseVDistance = true }),

            "combo_pos_letter_LW" => (Positions(false),
                new ModifierSet { LetterLNumber = true, LetterWNumber = true }),
            "combo_pos_all_letter" => (Positions(false),
                new ModifierSet
                {
                    LetterLNumber = true, LetterWNumber = true,
                    LetterVNumber = true, LetterCNumber = true,
                }),
            "combo_alt_letters_pos_L" => (Positions(false) with
            { AlternateLetterValues = true }, new ModifierSet { LetterLNumber = true }),

            _ => throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "unmapped golden case"),
        };

    private static CalculationProfile Positions(bool absolute) =>
        CalculationProfile.Default with { AddPositions = true, AbsolutePositions = absolute };

    private static CalculationProfile Distances() =>
        CalculationProfile.Default with { AddDistancesToPrevious = true };

    [Fact]
    public void EveryModifierCaseMatchesLegacy()
    {
        var mismatches = new List<string>();
        var caseNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (string[] row in TestPaths.ReadRows("modifiers.tsv"))
        {
            string caseName = row[0];
            string target = row[1];
            long expected = long.Parse(row[2]);
            caseNames.Add(caseName);

            (CalculationProfile profile, ModifierSet modifiers) = Configure(caseName);

            long actual = target == "chapter1"
                ? SegmentedCalculator.ValueOfChapter(_segmentation, 1, _system, profile, modifiers)
                : SegmentedCalculator.ValueOfVerse(
                    _segmentation, int.Parse(target) - 1, _system, profile, modifiers);

            if (actual != expected)
            {
                mismatches.Add($"{caseName} [{target}]: expected {expected}, got {actual}");
            }
        }

        Assert.True(caseNames.Count >= 29,
            $"expected at least 29 golden cases, found {caseNames.Count}");
        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} modifier results differ:\n  " +
            string.Join("\n  ", mismatches.Take(20)));
    }

    /// <summary>
    /// The segmentation itself must match the legacy counts, or every modifier
    /// built on it is measuring the wrong corpus.
    /// </summary>
    [Fact]
    public void SegmentationCountsMatchLegacy()
    {
        Assert.Equal(6236, _segmentation.VerseCount);
        Assert.Equal(77878, _segmentation.WordCount);
        Assert.Equal(327792, _segmentation.LetterCount);
    }

    /// <summary>
    /// Word and letter positional metadata, checked against the per-word golden
    /// dump rather than only through the values it feeds.
    /// </summary>
    [Fact]
    public void WordPositionsMatchLegacy()
    {
        var mismatches = new List<string>();

        foreach (string[] row in TestPaths.ReadRows("word-positions.tsv"))
        {
            int verseNumber = int.Parse(row[0]);
            int wordInVerse = int.Parse(row[1]);
            int expectedLetters = int.Parse(row[3]);
            int expectedNumberInChapter = int.Parse(row[6]);

            int verseIndex = verseNumber - 1;
            int wordIndex = _segmentation.VerseFirstWord[verseIndex] + wordInVerse - 1;

            if (_segmentation.WordLetterCount[wordIndex] != expectedLetters)
            {
                mismatches.Add(
                    $"verse {verseNumber} word {wordInVerse}: letters expected {expectedLetters}, " +
                    $"got {_segmentation.WordLetterCount[wordIndex]}");
            }
            if (_segmentation.WordNumberInChapter[wordIndex] != expectedNumberInChapter)
            {
                mismatches.Add(
                    $"verse {verseNumber} word {wordInVerse}: number_in_chapter expected " +
                    $"{expectedNumberInChapter}, got {_segmentation.WordNumberInChapter[wordIndex]}");
            }
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} word position mismatches:\n  " +
            string.Join("\n  ", mismatches.Take(10)));
    }
}
