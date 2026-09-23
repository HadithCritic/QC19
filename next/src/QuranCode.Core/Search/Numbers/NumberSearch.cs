using QuranCode.Core.Content;

namespace QuranCode.Core.Search.Numbers;

/// <summary>Units found by a number or frequency search.</summary>
/// <param name="Truncated">True when the search stopped at <see cref="NumberSearch.MaxResults"/>.</param>
public sealed record NumberSearchResult(IReadOnlyList<FoundUnit> Units, bool Truncated);

/// <summary>
/// Find by numbers (Features.txt #25, #32 to #35), following the legacy
/// <c>Server.DoFindWords/Verses/Chapters/Sentences</c> and their range and
/// set forms.
/// </summary>
/// <remarks>
/// <para>
/// A unit's number is read in the book, its chapter or its verse; a negative
/// number counts from the end. Counts are its verses, words, letters and
/// distinct letters, and its value is the sum of its words' letter values.
/// With Σ a count becomes the sum of positions (letters in their words, words
/// in their verses, verses in their chapters). The # kind compares a count
/// with the unit's own number. A run or set adds its units up.
/// </para>
/// <para>
/// Ranges try every size from 1 to 29 when none is given (up to one less than
/// the number of blocks for chapters and partitions), as the original does.
/// Sets need a size, and a search that would test more than
/// <see cref="MaxCombinations"/> sets is refused instead of running for hours.
/// </para>
/// </remarks>
public sealed class NumberSearch
{
    public const int MaxRangeSize = 29;
    public const long MaxCombinations = 5_000_000;
    public const int MaxResults = 100_000;

    private readonly UnitIndex _index;
    private readonly IReadOnlyDictionary<UnitKind, IReadOnlyList<Block>> _blocks;

    /// <param name="blocks">Chapters and each kind of partition as runs of counted verses.</param>
    public NumberSearch(UnitIndex index, IReadOnlyDictionary<UnitKind, IReadOnlyList<Block>> blocks)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(blocks);
        _index = index;
        _blocks = blocks;
    }

    public UnitIndex Index => _index;

    /// <summary>The chapters or partitions of a kind, as runs of counted verses.</summary>
    public IReadOnlyList<Block> BlocksOf(UnitKind unit) => _blocks[unit];

    /// <param name="verses">Indexes of the counted verses to search, in order; null for the whole book.</param>
    public NumberSearchResult Find(NumberQuery query, IReadOnlyList<int>? verses = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        Validate(query);

        IReadOnlyList<Item> items = Items(query.Unit, query.EffectiveScope, verses, null);
        (int[] frequency, int[] occurrence) = query.Shape == UnitShape.Single && (query.Frequency.IsSet || query.Occurrence.IsSet)
            ? Occurrences(query.Unit, query.EffectiveScope, items)
            : ([], []);

        bool sentences = query.Unit == UnitKind.Sentences;
        NumberSearchResult found = Evaluate(query.Unit, query.Shape, query.Size, items, (tally, item, index) =>
            Accepts(query, tally, item, sentences,
                frequency.Length > 0 ? frequency[index] : 0,
                occurrence.Length > 0 ? occurrence[index] : 0));

        return sentences ? WithWholeVerses(found, query, verses) : found;
    }

    private static void Validate(NumberQuery query)
    {
        if (!query.HasConstraint) throw new ArgumentException("Set at least one number to search for.");
        if (query.Unit == UnitKind.Sentences)
        {
            if (query.Shape != UnitShape.Single) throw new ArgumentException("Sentences are searched one at a time.");
            if (query.Number.IsSet) throw new ArgumentException("Sentences have no number of their own.");
        }
        if ((query.Frequency.IsSet || query.Occurrence.IsSet) &&
            (query.Shape != UnitShape.Single || query.Unit is not (UnitKind.Words or UnitKind.Verses)))
        {
            throw new ArgumentException("Frequency and occurrence apply to single words and verses.");
        }
        if (query.Size is < 1) throw new ArgumentException("A range or set has at least one unit.");
        if (query.Shape == UnitShape.Set && query.Size is null) throw new ArgumentException("Choose how many units a set has.");
    }

    /// <summary>One unit measured, with what its constraints need to know about it.</summary>
    /// <param name="LastNumber">The highest number in the unit's book, chapter or verse, for counting from the end.</param>
    /// <param name="Words">The counted words the unit covers.</param>
    internal readonly record struct Item(int Id, Tally Tally, long OwnNumber, long LastNumber, int Container, WordSpan Words);

    /// <summary>The units of a kind within the searched verses, in book order.</summary>
    /// <param name="letterFrequency">Per counted word, a letter frequency sum to carry in the tallies.</param>
    internal IReadOnlyList<Item> Items(UnitKind unit, NumberScope scope, IReadOnlyList<int>? verses, long[]? letterFrequency)
    {
        Segmentation s = _index.Segmentation;
        IReadOnlyList<int> source = verses ?? Enumerable.Range(0, s.VerseCount).ToArray();
        var items = new List<Item>();

        switch (unit)
        {
            case UnitKind.Words:
                int[] chapterWords = ChapterWordCounts();
                foreach (int v in source)
                {
                    int first = s.VerseFirstWord[v];
                    for (int w = first; w < first + s.VerseWordCount[v]; w++)
                    {
                        int chapter = s.VerseChapter[v];
                        (long number, long size, int container) = scope switch
                        {
                            NumberScope.Book => ((long)w + 1, (long)s.WordCount, 0),
                            NumberScope.Chapter => (s.WordNumberInChapter[w], chapterWords[chapter], chapter),
                            _ => (s.WordNumberInVerse[w], s.VerseWordCount[v], v),
                        };
                        items.Add(new Item(w, WordTally(w, letterFrequency).WithNumber(number), number, size, container, new WordSpan(w, w)));
                    }
                }
                break;

            case UnitKind.Verses:
                int[] lastInChapter = ChapterLastVerseNumbers();
                foreach (int v in source)
                {
                    int chapter = s.VerseChapter[v];
                    (long number, long size, int container) = scope == NumberScope.Book
                        ? ((long)v + 1, (long)s.VerseCount, 0)
                        : (s.VerseNumberInChapter[v], lastInChapter[chapter], chapter);
                    items.Add(new Item(v, VerseTally(v, letterFrequency).WithNumber(number), number, size, container, VerseWords(v, v)));
                }
                break;

            case UnitKind.Sentences:
                foreach (WordSpan span in _index.Sentences(verses))
                {
                    Tally tally = default;
                    for (int w = span.First; w <= span.Last; w++) tally = tally.Add(WordTally(w, letterFrequency));
                    items.Add(new Item(span.First, tally with { Number = 0 }, 0, 0, 0, span));
                }
                break;

            default:
                IReadOnlyList<Block> blocks = _blocks[unit];
                var inSource = new HashSet<int>(source);
                foreach (Block block in blocks)
                {
                    if (!Enumerable.Range(block.FirstVerse, block.LastVerse - block.FirstVerse + 1).Any(inSource.Contains)) continue;
                    Tally tally = default;
                    for (int v = block.FirstVerse; v <= block.LastVerse; v++) tally = tally.Add(VerseTally(v, letterFrequency));
                    items.Add(new Item(block.Number, tally.WithNumber(block.Number), block.Number, blocks.Count, 0, VerseWords(block.FirstVerse, block.LastVerse)));
                }
                break;
        }
        return items;
    }

    private WordSpan VerseWords(int firstVerse, int lastVerse)
    {
        Segmentation s = _index.Segmentation;
        return new WordSpan(s.VerseFirstWord[firstVerse], s.VerseFirstWord[lastVerse] + s.VerseWordCount[lastVerse] - 1);
    }

    internal Tally WordTally(int w, long[]? letterFrequency)
    {
        Segmentation s = _index.Segmentation;
        int letters = s.WordLetterCount[w];
        return new Tally(
            Number: w + 1,
            Verses: 0,
            VerseSum: 0,
            Words: 1,
            WordSum: s.WordNumberInVerse[w],
            Letters: letters,
            LetterSum: (long)letters * (letters + 1) / 2,
            Mask: _index.WordMasks[w],
            Value: _index.WordValues[w],
            LetterFrequencySum: letterFrequency?[w] ?? 0);
    }

    internal Tally VerseTally(int v, long[]? letterFrequency)
    {
        Segmentation s = _index.Segmentation;
        Tally tally = default;
        int first = s.VerseFirstWord[v];
        for (int w = first; w < first + s.VerseWordCount[v]; w++) tally = tally.Add(WordTally(w, letterFrequency));
        return tally with { Number = v + 1, Verses = 1, VerseSum = s.VerseNumberInChapter[v] };
    }

    /// <summary>Tests every single unit, run or set of the items.</summary>
    internal static NumberSearchResult Evaluate(
        UnitKind unit, UnitShape shape, int? size, IReadOnlyList<Item> items, Func<Tally, Item, int, bool> accept)
    {
        var found = new List<FoundUnit>();
        bool truncated = false;

        bool Add(FoundUnit unitFound)
        {
            if (found.Count >= MaxResults)
            {
                truncated = true;
                return false;
            }
            found.Add(unitFound);
            return true;
        }

        switch (shape)
        {
            case UnitShape.Single:
                for (int i = 0; i < items.Count; i++)
                {
                    if (accept(items[i].Tally, items[i], i) && !Add(new FoundUnit(unit, shape, SingleItems(unit, items[i]), items[i].Tally))) break;
                }
                break;

            case UnitShape.Range:
                int largest = size ?? (IsBlock(unit) ? Math.Max(1, items.Count - 1) : MaxRangeSize);
                int smallest = size ?? 1;
                // Grouped by size, then by start, as the original lists them.
                var bySize = new List<FoundUnit>[largest + 1];
                int kept = 0;
                for (int start = 0; start < items.Count && !truncated; start++)
                {
                    Tally tally = default;
                    for (int length = 1; length <= largest && start + length <= items.Count; length++)
                    {
                        Item last = items[start + length - 1];
                        tally = tally.Add(last.Tally);
                        if (length < smallest || !accept(tally, last with { OwnNumber = tally.Number }, start)) continue;
                        (bySize[length] ??= []).Add(new FoundUnit(unit, shape, Ids(items, start, length), tally));
                        if (++kept >= MaxResults)
                        {
                            truncated = true;
                            break;
                        }
                    }
                }
                foreach (List<FoundUnit>? list in bySize)
                {
                    if (list is not null) found.AddRange(list);
                }
                break;

            case UnitShape.Set:
                int k = size!.Value;
                if (Combinations(items.Count, k) > MaxCombinations)
                {
                    throw new InvalidOperationException(
                        $"Sets of {k} from {items.Count:N0} units are too many to test; search within fewer verses or use a smaller set.");
                }
                foreach (int[] combination in Combine(items.Count, k))
                {
                    Tally tally = default;
                    foreach (int i in combination) tally = tally.Add(items[i].Tally);
                    if (!accept(tally, items[combination[0]] with { OwnNumber = tally.Number }, combination[0])) continue;
                    if (!Add(new FoundUnit(unit, shape, combination.Select(i => items[i].Id).ToArray(), tally))) break;
                }
                break;
        }
        return new NumberSearchResult(found, truncated);
    }

    private static bool IsBlock(UnitKind unit) => unit is not (UnitKind.Words or UnitKind.Verses or UnitKind.Sentences);

    private static IReadOnlyList<int> SingleItems(UnitKind unit, Item item) =>
        unit == UnitKind.Sentences ? [item.Words.First, item.Words.Last] : [item.Id];

    private static int[] Ids(IReadOnlyList<Item> items, int start, int length)
    {
        var ids = new int[length];
        for (int i = 0; i < length; i++) ids[i] = items[start + i].Id;
        return ids;
    }

    private static bool Accepts(NumberQuery q, Tally t, Item item, bool sentence, int frequency, int occurrence)
    {
        // Sentences take neither # nor Σ (legacy Compare(Sentence)).
        bool Check(Criterion c, long count, long sum)
        {
            if (!c.IsSet) return true;
            if (sentence && (c.Type == NumberType.Natural || c.Comparison == Comparison.EqualSum)) return false;
            return c.Accepts(c.WantsSum ? sum : count, item.OwnNumber);
        }

        Criterion number = q.Number;
        if (number.Type == NumberType.None && number.Value < 0 && item.LastNumber > 0)
        {
            number = number with { Value = item.LastNumber + number.Value + 1 };
        }

        return Check(number, t.Number, t.Number)
            && Check(q.Verses, t.Verses, t.VerseSum)
            && Check(q.Words, t.Words, t.WordSum)
            && Check(q.Letters, t.Letters, t.LetterSum)
            && Check(q.UniqueLetters, t.UniqueLetters, t.UniqueLetters)
            && Check(q.Value, t.Value, t.Value)
            && Check(q.Frequency, frequency, frequency)
            && Check(q.Occurrence, occurrence, occurrence);
    }

    /// <summary>How often each item's text occurs in its container across the book, and which occurrence it is.</summary>
    private (int[] Frequency, int[] Occurrence) Occurrences(UnitKind unit, NumberScope scope, IReadOnlyList<Item> items)
    {
        Segmentation s = _index.Segmentation;
        var counts = new Dictionary<(int, string), int>();
        var ordinal = new Dictionary<int, int>();

        int Container(int v, int w) => scope switch
        {
            NumberScope.Book => 0,
            NumberScope.Chapter => s.VerseChapter[v],
            _ => unit == UnitKind.Words ? v : s.VerseChapter[v],
        };

        for (int v = 0; v < s.VerseCount; v++)
        {
            if (unit == UnitKind.Verses)
            {
                var key = (Container(v, -1), _index.VerseTexts[v]);
                counts[key] = counts.GetValueOrDefault(key) + 1;
                ordinal[v] = counts[key];
                continue;
            }
            int first = s.VerseFirstWord[v];
            for (int w = first; w < first + s.VerseWordCount[v]; w++)
            {
                var key = (Container(v, w), _index.WordTexts[w]);
                counts[key] = counts.GetValueOrDefault(key) + 1;
                ordinal[w] = counts[key];
            }
        }

        var frequency = new int[items.Count];
        var occurrence = new int[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            int id = items[i].Id;
            int verse = unit == UnitKind.Verses ? id : s.WordVerse[id];
            string text = unit == UnitKind.Verses ? _index.VerseTexts[id] : _index.WordTexts[id];
            frequency[i] = counts[(Container(verse, id), text)];
            occurrence[i] = ordinal[id];
        }
        return (frequency, occurrence);
    }

    /// <summary>
    /// The sentences search lists matching whole verses first, unless a found
    /// sentence is already the whole verse (legacy <c>Server.FindSentences</c>).
    /// </summary>
    private NumberSearchResult WithWholeVerses(NumberSearchResult sentences, NumberQuery query, IReadOnlyList<int>? verses)
    {
        Segmentation s = _index.Segmentation;
        var known = new HashSet<(int, int)>(sentences.Units.Select(u => (u.Items[0], u.Items[1])));
        var whole = new List<FoundUnit>();
        foreach (int v in verses ?? Enumerable.Range(0, s.VerseCount).ToArray())
        {
            int first = s.VerseFirstWord[v], last = first + s.VerseWordCount[v] - 1;
            if (last < first || known.Contains((first, last))) continue;
            Tally tally = VerseTally(v, null) with { Number = 0 };
            var item = new Item(first, tally, 0, 0, 0, new WordSpan(first, last));
            if (Accepts(query, tally, item, sentence: true, 0, 0)) whole.Add(new FoundUnit(UnitKind.Sentences, UnitShape.Single, [first, last], tally));
        }
        return new NumberSearchResult([.. whole, .. sentences.Units], sentences.Truncated);
    }

    private int[] ChapterWordCounts()
    {
        Segmentation s = _index.Segmentation;
        var counts = new int[115];
        for (int v = 0; v < s.VerseCount; v++) counts[s.VerseChapter[v]] += s.VerseWordCount[v];
        return counts;
    }

    /// <summary>A chapter's last verse number; with a verse 0 it is one less than its verse count.</summary>
    private int[] ChapterLastVerseNumbers()
    {
        Segmentation s = _index.Segmentation;
        var last = new int[115];
        for (int v = 0; v < s.VerseCount; v++) last[s.VerseChapter[v]] = Math.Max(last[s.VerseChapter[v]], s.VerseNumberInChapter[v]);
        return last;
    }

    internal static long Combinations(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        long result = 1;
        for (int i = 1; i <= k; i++)
        {
            result = result * (n - k + i) / i;
            if (result > MaxCombinations) return result;
        }
        return result;
    }

    /// <summary>Every k-combination of 0..n-1 in lexicographic order.</summary>
    internal static IEnumerable<int[]> Combine(int n, int k)
    {
        if (k <= 0 || k > n) yield break;
        int[] c = Enumerable.Range(0, k).ToArray();
        while (true)
        {
            yield return (int[])c.Clone();
            int i = k - 1;
            while (i >= 0 && c[i] == n - k + i) i--;
            if (i < 0) yield break;
            c[i]++;
            for (int j = i + 1; j < k; j++) c[j] = c[j - 1] + 1;
        }
    }
}
