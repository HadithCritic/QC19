using QuranCode.Core.Content;
using QuranCode.Core.Search.Numbers;
using Xunit;

namespace QuranCode.Core.Tests;

/// <summary>Phase 4: find by numbers, sentences and letter frequency, checked against direct counts.</summary>
public sealed class NumberSearchTests : IDisposable
{
    private readonly QuranCodeEngine _engine = new(TestPaths.SubmissionDatabase);
    private readonly NumberSearch _search;
    private readonly Segmentation _s;

    public NumberSearchTests()
    {
        _search = _engine.NumberSearch();
        _s = _search.Index.Segmentation;
    }

    public void Dispose() => _engine.Dispose();

    private IReadOnlyList<FoundUnit> Find(NumberQuery query) => _search.Find(query).Units;

    private int VerseLetters(int v) =>
        Enumerable.Range(_s.VerseFirstWord[v], _s.VerseWordCount[v]).Sum(w => _s.WordLetterCount[w]);

    [Fact]
    public void VersesWithAsManyWordsAsTheirNumber()
    {
        var found = Find(new NumberQuery(UnitKind.Verses) { Words = new Criterion(Type: NumberType.Natural) });
        int[] expected = Enumerable.Range(0, _s.VerseCount).Where(v => _s.VerseWordCount[v] == _s.VerseNumberInChapter[v]).ToArray();
        Assert.Equal(expected, found.Select(u => u.Items[0]));
        Assert.NotEmpty(expected);
    }

    [Fact]
    public void ANegativeNumberCountsFromTheEnd()
    {
        var last = Find(new NumberQuery(UnitKind.Verses) { Number = new Criterion(-1) });
        Assert.Equal(114, last.Count);
        Assert.All(last, u => Assert.True(u.Items[0] == _s.VerseCount - 1 || _s.VerseChapter[u.Items[0] + 1] != _s.VerseChapter[u.Items[0]]));
    }

    [Fact]
    public void ChaptersByVerseCountAndKind()
    {
        var seven = Find(new NumberQuery(UnitKind.Chapters) { Verses = new Criterion(7) });
        Assert.Contains(seven, u => u.Items[0] == 1);

        var prime = Find(new NumberQuery(UnitKind.Chapters) { Verses = new Criterion(Type: NumberType.Prime) });
        int[] expected = _engine.Chapters
            .Where(c => Numbers.NumberTheory.IsPrime(Enumerable.Range(0, _s.VerseCount).Count(v => _s.VerseChapter[v] == c.Number)))
            .Select(c => c.Number).ToArray();
        Assert.Equal(expected, prime.Select(u => u.Items[0]));
    }

    [Fact]
    public void WordRangesOfTwoWithNineteenLetters()
    {
        var found = Find(new NumberQuery(UnitKind.Words, UnitShape.Range, Size: 2) { Letters = new Criterion(19) });
        int expected = Enumerable.Range(0, _s.WordCount - 1).Count(w => _s.WordLetterCount[w] + _s.WordLetterCount[w + 1] == 19);
        Assert.Equal(expected, found.Count);
        Assert.All(found, u => Assert.Equal(2, u.Items.Count));
    }

    [Fact]
    public void RangesWithoutASizeTryEverySizeInOrder()
    {
        var found = Find(new NumberQuery(UnitKind.Verses, UnitShape.Range) { Letters = new Criterion(100) });
        int[] sizes = found.Select(u => u.Items.Count).ToArray();
        Assert.Equal(sizes.Order(), sizes);
        Assert.True(sizes.Max() > 1);
        Assert.All(found, u => Assert.Equal(100, u.Items.Sum(VerseLetters)));
    }

    [Fact]
    public void SigmaSumsWordPositions()
    {
        // Σ words = 1 + 2 + ... + n = 10 means a verse of four words.
        var sigma = Find(new NumberQuery(UnitKind.Verses) { Words = new Criterion(10, Comparison.EqualSum) });
        var four = Find(new NumberQuery(UnitKind.Verses) { Words = new Criterion(4) });
        Assert.Equal(four.Select(u => u.Items[0]), sigma.Select(u => u.Items[0]));
    }

    [Fact]
    public void DivisibleByWithAnyRemainder()
    {
        var odd = Find(new NumberQuery(UnitKind.Chapters) { Verses = new Criterion(2, Comparison.DivisibleBy, Remainder: -1) });
        var alsoOdd = Find(new NumberQuery(UnitKind.Chapters) { Verses = new Criterion(Type: NumberType.Odd) });
        Assert.Equal(alsoOdd.Select(u => u.Items[0]), odd.Select(u => u.Items[0]));
    }

    [Fact]
    public void SetsOfChaptersAddUp()
    {
        var pairs = Find(new NumberQuery(UnitKind.Chapters, UnitShape.Set, Size: 2) { Verses = new Criterion(10) });
        Assert.All(pairs, u => Assert.Equal(10, u.Tally.Verses));

        int[] verses = _engine.Chapters.Select(c => Enumerable.Range(0, _s.VerseCount).Count(v => _s.VerseChapter[v] == c.Number)).ToArray();
        int expected = 0;
        for (int a = 0; a < 114; a++)
        {
            for (int b = a + 1; b < 114; b++)
            {
                if (verses[a] + verses[b] == 10) expected++;
            }
        }
        Assert.Equal(expected, pairs.Count);
    }

    [Fact]
    public void TooManySetsAreRefused() =>
        Assert.Throws<InvalidOperationException>(() => Find(new NumberQuery(UnitKind.Words, UnitShape.Set, Size: 3) { Letters = new Criterion(3) }));

    [Fact]
    public void PagesArePartitions()
    {
        var pages = Find(new NumberQuery(UnitKind.Pages) { Number = new Criterion(1) });
        Assert.Single(pages);
        Assert.Equal(1, pages[0].Items[0]);
    }

    [Fact]
    public void WordFrequencyAndOccurrence()
    {
        // Words whose text occurs exactly once in the book.
        var once = Find(new NumberQuery(UnitKind.Words, Scope: NumberScope.Book) { Frequency = new Criterion(1) });
        var texts = _search.Index.WordTexts.GroupBy(t => t).Where(g => g.Count() == 1).Count();
        Assert.Equal(texts, once.Count);
    }

    [Fact]
    public void InvalidQueriesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Find(new NumberQuery(UnitKind.Verses)));
        Assert.Throws<ArgumentException>(() => Find(new NumberQuery(UnitKind.Sentences) { Number = new Criterion(3) }));
        Assert.Throws<ArgumentException>(() => Find(new NumberQuery(UnitKind.Verses, UnitShape.Set) { Letters = new Criterion(3) }));
    }

    [Fact]
    public void Sentences2v2SplitAtBothDotMarks()
    {
        // ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ ۛ فِيهِ ۛ هُدًى لِّلْمُتَّقِينَ
        int verse = _engine.View().IndexOf(_engine.Verse(2, 2).Number);
        int first = _s.VerseFirstWord[verse];
        var spans = _search.Index.Sentences().Where(s => s.First >= first && s.Last < first + _s.VerseWordCount[verse]).ToArray();
        Assert.Contains(new WordSpan(first, first + 3), spans);     // up to the first mark
        Assert.Contains(new WordSpan(first, first + 4), spans);     // up to the second
        Assert.Contains(new WordSpan(first + 4, first + 6), spans); // from فيه to the end
        Assert.Contains(new WordSpan(first + 5, first + 6), spans); // after the second mark
        Assert.Contains(new WordSpan(first, first + 6), spans);     // the whole verse
    }

    [Fact]
    public void SentencesByWordCountListWholeVersesFirst()
    {
        var found = Find(new NumberQuery(UnitKind.Sentences) { Words = new Criterion(4) });
        Assert.All(found, u => Assert.Equal(4, u.Tally.Words));
        Assert.All(found, u => Assert.Equal(2, u.Items.Count));
        Assert.Contains(found, u => u.Items[0] == 0); // the Bismillah, 1:1, as a whole verse
    }

    [Fact]
    public void LetterFrequencySumMatchesADirectCount()
    {
        var frequency = new FrequencySearch(_search);
        string phrase = "الله";
        var weights = phrase.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
        long Direct(int v)
        {
            long sum = 0;
            for (int w = _s.VerseFirstWord[v]; w < _s.VerseFirstWord[v] + _s.VerseWordCount[v]; w++)
            {
                for (int l = _s.WordFirstLetter[w]; l < _s.WordFirstLetter[w] + _s.WordLetterCount[w]; l++)
                {
                    sum += weights.GetValueOrDefault(_s.LetterChars[l]);
                }
            }
            return sum;
        }

        var found = frequency.Find(new FrequencyQuery(UnitKind.Verses, phrase, Sum: new Criterion(20))).Units;
        int[] expected = Enumerable.Range(0, _s.VerseCount).Where(v => Direct(v) == 20).ToArray();
        Assert.Equal(expected, found.Select(u => u.Items[0]));
        Assert.NotEmpty(expected);

        var unique = frequency.Find(new FrequencyQuery(UnitKind.Verses, phrase, UniqueLetters: true, Sum: new Criterion(1, Comparison.GreaterOrEqual))).Units;
        Assert.True(unique.Count > found.Count);
    }

    [Fact]
    public void LetterMatching()
    {
        var frequency = new FrequencySearch(_search);
        var none = frequency.Find(new FrequencyQuery(UnitKind.Words, "ا", Match: LetterMatch.NoLetterOf)).Units;
        var any = frequency.Find(new FrequencyQuery(UnitKind.Words, "ا", UniqueLetters: true, Match: LetterMatch.AnyLetterOf)).Units;
        Assert.Equal(_s.WordCount, none.Count + any.Count);
    }

    [Theory]
    [InlineData(64, NumberType.Square, true)]
    [InlineData(64, NumberType.Cubic, true)]
    [InlineData(64, NumberType.Sextic, true)]
    [InlineData(63, NumberType.Square, false)]
    [InlineData(144, NumberType.Fibonacci, true)]
    [InlineData(145, NumberType.Fibonacci, false)]
    [InlineData(19, NumberType.AdditivePrime, false)]
    [InlineData(23, NumberType.AdditivePrime, true)]
    [InlineData(0, NumberType.Even, false)]
    public void NumberKinds(long n, NumberType type, bool expected) => Assert.Equal(expected, Criteria.IsOfType(n, type));
}
