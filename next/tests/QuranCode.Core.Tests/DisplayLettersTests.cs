using QuranCode.Core.Content;
using QuranCode.Core.Text;
using Xunit;
using Xunit.Abstractions;

namespace QuranCode.Core.Tests;

[Collection(SharedEngine.Collection)]
public sealed class DisplayLettersTests(ITestOutputHelper output)
{
    [Fact]
    public void CountsLettersNotMarks()
    {
        // ٱللَّهِ: alif wasla, lam, lam with shadda and fatha, ha with kasra.
        Assert.Equal(4, DisplayLetters.Count("ٱللَّهِ"));
    }

    [Fact]
    public void TatweelAndSmallLettersAreMarks()
    {
        // ٱلرَّحْمَـٰنِ: the tatweel carries a superscript alif; neither is a letter of its own.
        Assert.Equal(6, DisplayLetters.Count("ٱلرَّحْمَـٰنِ"));
    }

    [Fact]
    public void PauseAndLeadingMarksBelongToTheirWord()
    {
        Assert.Equal(3, DisplayLetters.Count("رَيْبَ ۛ"));
        Assert.Equal(3, DisplayLetters.Count("۞ فَلَآ"));
        Assert.Equal(0, DisplayLetters.Count("۞"));
    }

    [Fact]
    public void APrefixKeepsEachLettersMarks()
    {
        Assert.Equal("ٱللَّ", DisplayLetters.Prefix("ٱللَّهِ", 3));
        Assert.Equal("", DisplayLetters.Prefix("ٱللَّهِ", 0));
        Assert.Equal("رَيْبَ ۛ", DisplayLetters.Prefix("رَيْبَ ۛ", 3));
        Assert.Equal("۞ فَ", DisplayLetters.Prefix("۞ فَلَآ", 1));
    }

    [Fact]
    public void MapsEachLetterToTheCountedLettersItProduces()
    {
        CountingText text = SharedEngine.Instance.CountingText("Original");

        int[][]? ends = DisplayLetters.Map(["ٱللَّهِ"], "الله", text.NormalizeWord);

        Assert.NotNull(ends);
        Assert.Equal([1, 2, 3, 4], ends![0]);
    }

    [Fact]
    public void AShaddaLetterCanProduceTwoCountedLetters()
    {
        // Original allows no text options, so this uses Simplified29.
        var counting = new CountingOptions { ShaddaAsLetter = true };
        CountingText text = SharedEngine.Instance.CountingText("Simplified29", counting);

        // The word as the corpus spells it, with its marks in the stored order.
        string allah = SharedEngine.Instance.Display(SharedEngine.Instance.Verse(1)).Words[1];
        string counted = Compact(text.NormalizeWord(allah));
        int[][]? ends = DisplayLetters.Map([allah], counted, text.NormalizeWord);

        Assert.Equal(5, counted.Length);
        Assert.NotNull(ends);
        Assert.Equal([1, 2, 4, 5], ends![0]);
    }

    [Fact]
    public void MapsAcrossDisplayWordsJoinedIntoOne()
    {
        CountingText text = SharedEngine.Instance.CountingText("Original");
        string counted = Compact(text.NormalizeWord("بَعْدَ مَا"));

        int[][]? ends = DisplayLetters.Map(["بَعْدَ", "مَا"], counted, text.NormalizeWord);

        Assert.NotNull(ends);
        Assert.Equal([1, 2, 3], ends![0]);
        Assert.Equal([4, 5], ends[1]);
    }

    [Fact]
    public void RefusesAMappingThatIsNotAPrefix()
    {
        CountingText text = SharedEngine.Instance.CountingText("Original");
        Assert.Null(DisplayLetters.Map(["ٱللَّهِ"], "اله", text.NormalizeWord));
    }

    /// <summary>
    /// How many display words can be addressed letter by letter. This is the
    /// gate for letter selection: a word that fails is refused rather than
    /// guessed at, so the rate must stay high in the modes readers use.
    /// </summary>
    [Theory]
    [InlineData("Original", 0.99)]
    [InlineData("Simplified28", 0.99)]
    [InlineData("Simplified29", 0.99)]
    [InlineData("Simplified30", 0.99)]
    [InlineData("Simplified31", 0.99)]
    [InlineData("Simplified36", 0.99)]
    [InlineData("SimplifiedDots", 0.99)]
    [InlineData("SimplifiedMarks", 0.99)]
    public void NearlyEveryWordMapsLetterByLetter(string textMode, double minimum)
    {
        QuranCodeEngine engine = SharedEngine.Instance;
        Segmentation segmentation = engine.Segmentation(textMode);
        CountingText text = engine.CountingText(textMode);
        CorpusView view = engine.View();

        int words = 0, mapped = 0;
        var failures = new List<string>();
        for (int v = 0; v < view.Verses.Count; v++)
        {
            Verse verse = view.Verses[v];
            VerseDisplay display = engine.Display(verse);
            DisplaySpan[]? spans = DisplayWords.Align(display, segmentation.VerseWords(v), text.NormalizeWord);
            if (spans is null) continue;

            foreach ((DisplaySpan span, int first, int count) in Groups(spans))
            {
                int firstWord = segmentation.VerseFirstWord[v] + first;
                int start = segmentation.WordFirstLetter[firstWord];
                int end = segmentation.WordFirstLetter[firstWord + count - 1] + segmentation.WordLetterCount[firstWord + count - 1];
                string counted = new(segmentation.LetterChars, start, end - start);
                string[] group = display.Words.Skip(span.First).Take(span.Count).ToArray();

                words += span.Count;
                if (DisplayLetters.Map(group, counted, text.NormalizeWord) is not null) mapped += span.Count;
                else if (failures.Count < 20) failures.Add($"{verse.ChapterNumber}:{verse.NumberInChapter} {string.Join(' ', group)}");
            }
        }

        double rate = (double)mapped / words;
        output.WriteLine($"{textMode}: {mapped:N0} of {words:N0} display words map ({rate:P3})");
        foreach (string failure in failures) output.WriteLine(failure);
        Assert.True(rate >= minimum, $"{rate:P3} of words map, below {minimum:P0}");
    }

    /// <summary>Runs of counted words that share one display span, with the counted index of the first.</summary>
    private static IEnumerable<(DisplaySpan Span, int First, int Count)> Groups(DisplaySpan[] spans)
    {
        for (int s = 0; s < spans.Length;)
        {
            if (spans[s].Count == 0) { s++; continue; }
            int e = s;
            while (e + 1 < spans.Length && spans[e + 1] == spans[s]) e++;
            yield return (spans[s], s, e - s + 1);
            s = e + 1;
        }
    }

    private static string Compact(string text) => text.Replace(" ", "", StringComparison.Ordinal);
}
