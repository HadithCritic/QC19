namespace QuranCode.Core.Search.Numbers;

/// <summary>How the letters of a unit must relate to the phrase's (legacy <c>FrequencyMatchingType</c>).</summary>
public enum LetterMatch
{
    AllLettersOf,
    AnyLetterOf,
    OnlyLettersOf,
    NoLetterOf,
}

/// <summary>
/// A search by letter frequency (Features.txt #14, #26, #54): for each unit,
/// how many of its letters are letters of the phrase.
/// </summary>
/// <param name="Phrase">Already in the text mode's letters; spaces are ignored.</param>
/// <param name="UniqueLetters">Count each phrase letter once, instead of once per time the phrase has it.</param>
/// <param name="Sum">The constraint on the sum; the Natural kind means no kind, as in the original.</param>
/// <param name="Match">Instead of a sum, how the unit's letters must relate to the phrase's.</param>
public sealed record FrequencyQuery(
    UnitKind Unit,
    string Phrase,
    UnitShape Shape = UnitShape.Single,
    int? Size = null,
    bool UniqueLetters = false,
    Criterion? Sum = null,
    LetterMatch? Match = null);

/// <summary>
/// Letter frequency sums, following <c>Server.CalculateLetterFrequencySum</c>:
/// for every letter of the phrase, the number of times the unit has it. With
/// duplicate letters (the default) a letter the phrase repeats counts once per
/// repetition. Words, verses, chapters and sentences, singly or as runs.
/// </summary>
public sealed class FrequencySearch
{
    private readonly NumberSearch _numbers;

    public FrequencySearch(NumberSearch numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        _numbers = numbers;
    }

    /// <param name="verses">Indexes of the counted verses to search; null for the whole book.</param>
    public NumberSearchResult Find(FrequencyQuery query, IReadOnlyList<int>? verses = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Unit is not (UnitKind.Words or UnitKind.Verses or UnitKind.Chapters or UnitKind.Sentences))
        {
            throw new ArgumentException("A frequency search finds words, verses, chapters or sentences.");
        }
        if (query.Shape == UnitShape.Set || (query.Unit == UnitKind.Sentences && query.Shape != UnitShape.Single))
        {
            throw new ArgumentException("A frequency search finds single units or runs of them.");
        }

        UnitIndex index = _numbers.Index;
        string letters = new(query.Phrase.Where(c => !char.IsWhiteSpace(c)).ToArray());
        if (query.UniqueLetters) letters = new string(letters.Distinct().ToArray());
        if (letters.Length == 0) throw new ArgumentException("The phrase has no letters.");

        // How many times the phrase uses each letter of the text.
        var weight = new Dictionary<char, int>();
        foreach (char c in letters) weight[c] = weight.GetValueOrDefault(c) + 1;

        long[] perWord = LetterFrequencySums(index, weight);
        IReadOnlyList<NumberSearch.Item> items = _numbers.Items(query.Unit, NumberScope.Book, verses, perWord);

        if (query.Match is LetterMatch match)
        {
            if (query.Shape != UnitShape.Single) throw new ArgumentException("Letter matching finds single units.");
            ulong phraseMask = Mask(index, weight.Keys);
            return NumberSearch.Evaluate(query.Unit, query.Shape, query.Size, items, (tally, item, _) =>
                Matches(match, tally, item, weight, phraseMask, query.UniqueLetters, index));
        }

        Criterion sum = query.Sum ?? throw new ArgumentException("Give a sum to compare with, or a way to match letters.");
        if (sum.Type == NumberType.Natural) sum = sum with { Type = NumberType.None };
        return NumberSearch.Evaluate(query.Unit, query.Shape, query.Size, items, (tally, _, _) =>
            sum.Accepts(tally.LetterFrequencySum, 0));
    }

    /// <summary>Per counted word, the sum of the phrase's letter weights over its letters.</summary>
    private static long[] LetterFrequencySums(UnitIndex index, Dictionary<char, int> weight)
    {
        var s = index.Segmentation;
        var sums = new long[s.WordCount];
        for (int w = 0; w < sums.Length; w++)
        {
            int first = s.WordFirstLetter[w];
            for (int l = first; l < first + s.WordLetterCount[w]; l++) sums[w] += weight.GetValueOrDefault(s.LetterChars[l]);
        }
        return sums;
    }

    private static ulong Mask(UnitIndex index, IEnumerable<char> letters)
    {
        ulong mask = 0;
        foreach (char c in letters)
        {
            if (index.LetterIndex.TryGetValue(c, out int bit)) mask |= 1UL << bit;
        }
        return mask;
    }

    /// <summary>The legacy <c>IsMatchingLetters</c>.</summary>
    private static bool Matches(
        LetterMatch match, Tally tally, NumberSearch.Item item, Dictionary<char, int> weight,
        ulong phraseMask, bool unique, UnitIndex index)
    {
        switch (match)
        {
            case LetterMatch.OnlyLettersOf:
                return (tally.Mask & ~phraseMask) == 0;
            case LetterMatch.NoLetterOf:
                return (tally.Mask & phraseMask) == 0;
        }

        // With duplicate letters, a letter counts only when the unit has it as
        // many times as the phrase does.
        Dictionary<char, int>? counts = unique ? null : LetterCounts(index, item);
        int hits = 0;
        foreach ((char c, int times) in weight)
        {
            bool present = index.LetterIndex.TryGetValue(c, out int bit) && (tally.Mask & (1UL << bit)) != 0;
            if (present && (unique || counts!.GetValueOrDefault(c) == times)) hits++;
        }
        return match == LetterMatch.AllLettersOf ? hits == weight.Count : hits > 0;
    }

    private static Dictionary<char, int> LetterCounts(UnitIndex index, NumberSearch.Item item)
    {
        var s = index.Segmentation;
        var counts = new Dictionary<char, int>();
        for (int w = item.Words.First; w <= item.Words.Last; w++)
        {
            int first = s.WordFirstLetter[w];
            for (int l = first; l < first + s.WordLetterCount[w]; l++)
            {
                counts[s.LetterChars[l]] = counts.GetValueOrDefault(s.LetterChars[l]) + 1;
            }
        }
        return counts;
    }
}
