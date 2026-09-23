namespace QuranCode.Core.Search.Numbers;

/// <summary>
/// What a unit (or a run or set of units) measures. Counts add up across
/// units; the <c>…Sum</c> members are the Σ forms (sums of positions), and the
/// letter mask unites.
/// </summary>
public readonly record struct Tally(
    long Number,
    int Verses,
    long VerseSum,
    int Words,
    long WordSum,
    int Letters,
    long LetterSum,
    ulong Mask,
    long Value,
    long LetterFrequencySum)
{
    public int UniqueLetters => UnitIndex.UniqueLetters(Mask);

    public Tally Add(Tally other) => new(
        Number + other.Number,
        Verses + other.Verses,
        VerseSum + other.VerseSum,
        Words + other.Words,
        WordSum + other.WordSum,
        Letters + other.Letters,
        LetterSum + other.LetterSum,
        Mask | other.Mask,
        Value + other.Value,
        LetterFrequencySum + other.LetterFrequencySum);

    /// <summary>The same measure with a different number (a verse or block counts as its own number, not its words').</summary>
    public Tally WithNumber(long number) => this with { Number = number };
}
