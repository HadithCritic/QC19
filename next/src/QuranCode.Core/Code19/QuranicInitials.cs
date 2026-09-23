namespace QuranCode.Core.Code19;

/// <summary>One initialed chapter: the letters it opens with, as one string.</summary>
/// <param name="Chapter">Chapter number.</param>
/// <param name="Letters">Its initials in order, one char each, without marks.</param>
/// <param name="Verses">How many opening verses carry them: 1 everywhere except chapter 42.</param>
public sealed record InitialedChapter(int Chapter, string Letters, int Verses = 1);

/// <summary>Counts of one initial letter in one chapter.</summary>
public sealed record InitialCount(int Chapter, char Letter, long Count);

/// <summary>
/// The Quranic Initials: the 29 chapters that open with disconnected letters,
/// and the 14 distinct letters among them.
/// </summary>
/// <remarks>
/// <para>
/// The chapters and their letters are stated data, not derived, because two
/// of them cannot be read off the text reliably. Chapter 42 carries its
/// initials over two verses (حم then عسق), and chapter 68 writes its single
/// initial ن out as the word نون. <see cref="QuranicInitials"/> is checked
/// against the text by a test, which is where those two conventions are
/// pinned.
/// </para>
/// <para>
/// This is the data behind Feature 76. Counting a letter through a chapter is
/// <see cref="FindingMeasure.LetterOccurrences"/>, so a published table row
/// becomes an ordinary finding with the same provenance fields.
/// </para>
/// </remarks>
public static class QuranicInitials
{
    /// <summary>The 29 initialed chapters, in chapter order.</summary>
    public static IReadOnlyList<InitialedChapter> Chapters { get; } =
    [
        new(2, "الم"), new(3, "الم"), new(7, "المص"),
        new(10, "الر"), new(11, "الر"), new(12, "الر"),
        new(13, "المر"), new(14, "الر"), new(15, "الر"),
        new(19, "كهيعص"), new(20, "طه"),
        new(26, "طسم"), new(27, "طس"), new(28, "طسم"),
        new(29, "الم"), new(30, "الم"), new(31, "الم"), new(32, "الم"),
        new(36, "يس"), new(38, "ص"),
        new(40, "حم"), new(41, "حم"),
        // The only chapter whose initials run over two verses: حم then عسق.
        new(42, "حمعسق", Verses: 2),
        new(43, "حم"), new(44, "حم"), new(45, "حم"), new(46, "حم"),
        new(50, "ق"),
        // Written out as the word نون, but the initial is the one letter ن.
        new(68, "ن"),
    ];

    /// <summary>The 14 distinct initial letters, in the order they first appear.</summary>
    public static IReadOnlyList<char> Letters { get; } =
        [.. Chapters.SelectMany(c => c.Letters).Distinct()];

    /// <summary>The initialed chapters that open with a given letter.</summary>
    public static IReadOnlyList<InitialedChapter> With(char letter) =>
        [.. Chapters.Where(c => c.Letters.Contains(letter))];

    /// <summary>
    /// How often each of a chapter's own initials occurs through that chapter.
    /// </summary>
    public static IReadOnlyList<InitialCount> CountsIn(
        QuranCodeEngine engine,
        InitialedChapter chapter,
        string textMode = "Simplified29",
        bool includeBasmalas = true)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        return
        [
            .. chapter.Letters.Distinct().Select(letter => new InitialCount(
                chapter.Chapter,
                letter,
                FindingEvaluator.Evaluate(engine, LetterFinding(chapter.Chapter, letter, textMode, includeBasmalas)).Computed)),
        ];
    }

    /// <summary>Every initialed chapter's counts, in chapter order.</summary>
    public static IReadOnlyList<InitialCount> AllCounts(
        QuranCodeEngine engine,
        string textMode = "Simplified29",
        bool includeBasmalas = true) =>
        [.. Chapters.SelectMany(c => CountsIn(engine, c, textMode, includeBasmalas))];

    /// <summary>A throwaway finding that counts one letter in one chapter.</summary>
    private static Finding LetterFinding(int chapter, char letter, string textMode, bool includeBasmalas) =>
        new(
            Id: $"initial-{letter}-{chapter}",
            Claim: $"The letter {letter} in chapter {chapter}",
            Expected: 0,
            Measure: FindingMeasure.LetterOccurrences,
            Scope: new FindingScope(chapter),
            Match: letter.ToString(),
            IncludeBasmalas: includeBasmalas,
            TextMode: textMode,
            Basis: RuleBasis.Stated,
            Rule: $"Count {letter} through chapter {chapter}.",
            Source: "computed");
}
