namespace QuranCode.Core.Search.Numbers;

/// <summary>What a number or frequency search finds (legacy <c>NumbersResultType</c>).</summary>
public enum UnitKind
{
    Words,
    Verses,
    Chapters,
    Sentences,
    Pages,
    Stations,
    Parts,
    Groups,
    Halves,
    Quarters,
    Bowings,
}

/// <summary>Single units, runs of neighboring units, or any combination of units.</summary>
public enum UnitShape
{
    Single,
    Range,
    Set,
}

/// <summary>Which number of a unit the Number constraint reads (legacy <c>NumberScope</c>).</summary>
public enum NumberScope
{
    /// <summary>Its number in the book.</summary>
    Book,

    /// <summary>Its number in its chapter.</summary>
    Chapter,

    /// <summary>Its number in its verse (words only).</summary>
    Verse,
}

/// <summary>
/// A search by numbers (Features.txt #25, #32 to #35): the units to look at
/// and the constraints they must meet. Unset constraints are ignored.
/// </summary>
/// <param name="Size">Units per range or set; null tries every size a range may have.</param>
/// <param name="Scope">Which number the Number constraint reads; null for the unit's usual one.</param>
public sealed record NumberQuery(
    UnitKind Unit,
    UnitShape Shape = UnitShape.Single,
    int? Size = null,
    NumberScope? Scope = null)
{
    public Criterion Number { get; init; } = Criterion.Any;
    public Criterion Verses { get; init; } = Criterion.Any;
    public Criterion Words { get; init; } = Criterion.Any;
    public Criterion Letters { get; init; } = Criterion.Any;
    public Criterion UniqueLetters { get; init; } = Criterion.Any;
    public Criterion Value { get; init; } = Criterion.Any;

    /// <summary>How many times the same text occurs (words and verses).</summary>
    public Criterion Frequency { get; init; } = Criterion.Any;

    /// <summary>Which occurrence of its text a unit is (words and verses).</summary>
    public Criterion Occurrence { get; init; } = Criterion.Any;

    public bool HasConstraint =>
        Number.IsSet || Verses.IsSet || Words.IsSet || Letters.IsSet || UniqueLetters.IsSet ||
        Value.IsSet || Frequency.IsSet || Occurrence.IsSet;

    /// <summary>The number the Number constraint reads when none is chosen, as in the original.</summary>
    public NumberScope EffectiveScope => Scope ?? Unit switch
    {
        UnitKind.Words => NumberScope.Verse,
        UnitKind.Verses => NumberScope.Chapter,
        _ => NumberScope.Book,
    };
}

/// <summary>A found unit, run or set, and what it measured.</summary>
/// <param name="Items">
/// What was found, in the unit's own terms: counted word indexes for words,
/// verse indexes of the counted verses for verses, block numbers for chapters
/// and partitions, the first and last word index for a sentence.
/// </param>
public sealed record FoundUnit(UnitKind Unit, UnitShape Shape, IReadOnlyList<int> Items, Tally Tally);

/// <summary>A chapter or partition as a run of counted verses.</summary>
public readonly record struct Block(int Number, int FirstVerse, int LastVerse);
