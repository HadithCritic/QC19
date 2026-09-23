namespace QuranCode.Core.Code19;

/// <summary>One initialed chapter: the letters it opens with, as one string.</summary>
/// <param name="Chapter">Chapter number.</param>
/// <param name="Letters">Its initials in order, one char each, without marks.</param>
/// <param name="Verses">How many opening verses carry them: 1 everywhere except chapter 42.</param>
public sealed record InitialedChapter(int Chapter, string Letters, int Verses = 1);

/// <summary>Counts of one initial letter in one chapter.</summary>
/// <param name="Published">Khalifa's figure for it, from The Computer Speaks, or null if none is recorded.</param>
public sealed record InitialCount(int Chapter, char Letter, long Count, long? Published = null)
{
    /// <summary>There is a published figure and the text reproduces it.</summary>
    public bool Matches => Published == Count;
}

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

    /// <summary>
    /// Khalifa's count of each initial through its own chapter, from The
    /// Computer Speaks as tabulated by Quran Initial Count. Every figure except
    /// alif, and ل in chapters 11 and 30, is reproduced by the text; those
    /// exceptions are open findings in the catalog, with their reasons.
    /// </summary>
    public static IReadOnlyDictionary<(int Chapter, char Letter), long> Published { get; } = Table(
        (2, "ا4502 ل3202 م2195"), (3, "ا2521 ل1892 م1249"), (7, "ا2529 ل1530 م1164 ص97"),
        (10, "ا1319 ل913 ر257"), (11, "ا1370 ل794 ر325"), (12, "ا1306 ل812 ر257"),
        (13, "ا605 ل480 م260 ر137"), (14, "ا585 ل452 ر160"), (15, "ا493 ل323 ر96"),
        (19, "ك137 ه175 ي343 ع117 ص26"), (20, "ط28 ه251"),
        (26, "ط33 س94 م484"), (27, "ط27 س94"), (28, "ط19 س102 م460"),
        (29, "ا774 ل554 م344"), (30, "ا544 ل393 م317"), (31, "ا347 ل297 م173"), (32, "ا257 ل155 م158"),
        (36, "ي237 س48"), (38, "ص29"),
        (40, "ح64 م380"), (41, "ح48 م276"), (42, "ح53 م300 ع98 س54 ق57"),
        (43, "ح44 م324"), (44, "ح16 م150"), (45, "ح31 م200"), (46, "ح36 م225"),
        (50, "ق57"), (68, "ن133"));

    private static Dictionary<(int, char), long> Table(params (int Chapter, string Counts)[] rows)
    {
        var table = new Dictionary<(int, char), long>();
        foreach ((int chapter, string counts) in rows)
            foreach (string entry in counts.Split(' '))
                table[(chapter, entry[0])] = long.Parse(entry.AsSpan(1), System.Globalization.CultureInfo.InvariantCulture);
        return table;
    }

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
                FindingEvaluator.Evaluate(engine, LetterFinding(chapter.Chapter, letter, textMode, includeBasmalas)).Computed,
                Published.TryGetValue((chapter.Chapter, letter), out long published) ? published : null)),
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
