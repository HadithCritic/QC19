using QuranCode.Core.Content;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>
/// Letter frequencies over the whole corpus, checked against the legacy engine.
/// </summary>
/// <remarks>
/// Added after a screenshot of the desktop Statistics page appeared to show the
/// wrong letter at the top of a descending list. A screenshot cannot settle
/// that; this can.
/// </remarks>
public sealed class FrequencyTests : IDisposable
{
    private readonly ContentRepository _content;
    private readonly Segmentation _segmentation;

    public FrequencyTests()
    {
        _content = new ContentRepository(TestPaths.ContentDatabase);
        _segmentation = Segmentation.Build(_content.Verses, new TextPipeline(_content.GetTextMode("Original")));
    }

    public void Dispose() => _content.Dispose();

    private Dictionary<char, int> Counts()
    {
        var counts = new Dictionary<char, int>();
        foreach (char letter in _segmentation.LetterChars)
        {
            counts[letter] = counts.GetValueOrDefault(letter) + 1;
        }
        return counts;
    }

    [Fact]
    public void LetterFrequenciesMatchLegacy()
    {
        Dictionary<char, int> actual = Counts();
        var mismatches = new List<string>();
        int compared = 0;

        foreach (string[] row in TestPaths.ReadRows("letter-frequencies.tsv"))
        {
            char letter = row[0][0];
            int expected = int.Parse(row[2]);
            int got = actual.GetValueOrDefault(letter);
            compared++;

            if (got != expected)
            {
                mismatches.Add($"U+{(int)letter:X4}: expected {expected}, got {got}");
            }
        }

        Assert.Equal(29, compared);
        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} letter frequency mismatches:\n  " + string.Join("\n  ", mismatches));
    }

    [Fact]
    public void FrequenciesSumToLetterCount()
    {
        Assert.Equal(327792, Counts().Values.Sum());
        Assert.Equal(29, Counts().Count);
    }

    /// <summary>
    /// Alef is the most frequent letter, and by a wide margin. This is the
    /// assertion the desktop screenshot appeared to contradict.
    /// </summary>
    [Fact]
    public void AlefIsTheMostFrequentLetter()
    {
        List<KeyValuePair<char, int>> ordered = [.. Counts().OrderByDescending(p => p.Value)];

        Assert.Equal('ا', ordered[0].Key);   // ا
        Assert.Equal(52991, ordered[0].Value);
        Assert.Equal('ل', ordered[1].Key);   // ل
        Assert.Equal(38550, ordered[1].Value);
    }
}
