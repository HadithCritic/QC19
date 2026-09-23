namespace QuranCode.Core.Content;

/// <summary>How an edition stores the Bismillah that opens chapters 2..114 (except 9).</summary>
public enum BasmalaMode
{
    /// <summary>Classic: it is the start of verse 1's text and always counted.</summary>
    Prefix,

    /// <summary>Submission: it is its own verse 0, which the user may exclude.</summary>
    VerseZero,
}

/// <summary>Which edition a content database holds.</summary>
public sealed record CorpusInfo(string Edition, BasmalaMode Basmala);

/// <summary>A chapter, as a value rather than an object with back-references.</summary>
/// <param name="VerseCount">Numbered verses, not counting a verse 0.</param>
/// <param name="FirstVerse">Absolute number of the chapter's first row: its verse 0 when it has one.</param>
/// <param name="Initialization">Quranic initials: key (chapter 1), full, partial, double (42) or none.</param>
public readonly record struct Chapter(
    int Number,
    string Name,
    string TransliteratedName,
    string EnglishName,
    int RevelationOrder,
    string RevelationPlace,
    int VerseCount,
    int FirstVerse,
    bool HasVerseZero = false,
    string Initialization = "none")
{
    /// <summary>Rows the chapter occupies, its verse 0 included.</summary>
    public int RowCount => VerseCount + (HasVerseZero ? 1 : 0);

    /// <summary>Absolute number of the chapter's last verse.</summary>
    public int LastVerse => FirstVerse + RowCount - 1;

    /// <summary>The lowest verse number the chapter has: 0 or 1.</summary>
    public int FirstNumberInChapter => HasVerseZero ? 0 : 1;

    /// <summary>Absolute number of verse <paramref name="numberInChapter"/>.</summary>
    public int AbsoluteOf(int numberInChapter) => FirstVerse + numberInChapter - FirstNumberInChapter;

    public bool Contains(int absoluteVerse) => absoluteVerse >= FirstVerse && absoluteVerse <= LastVerse;
}

/// <summary>A verse, carrying its canonical text and nothing derived.</summary>
/// <param name="IsBasmala">A verse-0 Bismillah, which may be excluded from counting.</param>
public readonly record struct Verse(
    int Number,
    int ChapterNumber,
    int NumberInChapter,
    string Text,
    bool IsBasmala = false);

/// <summary>
/// Word rules scoped to single verses, applied before the text mode's rules.
/// </summary>
/// <remarks>
/// The edition's text is never edited. These only change how a verse's words
/// are counted, for example joining ما لم in 96:5 in the Submission edition.
/// </remarks>
public sealed class VerseRules
{
    public static readonly VerseRules None = new(new Dictionary<int, (string, string)[]>());

    private readonly IReadOnlyDictionary<int, (string Find, string Replace)[]> _rules;

    public VerseRules(IReadOnlyDictionary<int, (string Find, string Replace)[]> rules) => _rules = rules;

    public int Count => _rules.Values.Sum(r => r.Length);

    public string Apply(int verseNumber, string text)
    {
        if (!_rules.TryGetValue(verseNumber, out (string Find, string Replace)[]? rules)) return text;
        foreach ((string find, string replace) in rules) text = text.Replace(find, replace, StringComparison.Ordinal);
        return text;
    }
}

/// <summary>
/// The verses counted under the current choice, with their absolute numbers.
/// </summary>
/// <remarks>
/// Absolute verse numbers never change: a selection is always expressed in
/// them. Excluding the verse-0 Bismillahs removes those rows from the view,
/// and everything counted (segmentation, values, positions, distances) runs
/// over the view. When nothing is excluded the view is the whole corpus and
/// view index = absolute number - 1.
/// </remarks>
public sealed class CorpusView
{
    private readonly int[] _indexOf;

    public IReadOnlyList<Verse> Verses { get; }

    public bool IncludesBasmalas { get; }

    public CorpusView(IReadOnlyList<Verse> all, bool includeBasmalas)
    {
        ArgumentNullException.ThrowIfNull(all);
        IncludesBasmalas = includeBasmalas;
        Verses = includeBasmalas ? all : all.Where(v => !v.IsBasmala).ToArray();

        _indexOf = new int[all.Count + 1];
        Array.Fill(_indexOf, -1);
        for (int i = 0; i < Verses.Count; i++) _indexOf[Verses[i].Number] = i;
    }

    /// <summary>View index of an absolute verse, or -1 when it is excluded or does not exist.</summary>
    public int IndexOf(int verseNumber) =>
        verseNumber >= 1 && verseNumber < _indexOf.Length ? _indexOf[verseNumber] : -1;

    /// <summary>
    /// The inclusive view-index range covering an absolute range, skipping
    /// excluded rows at either end; null when every verse in it is excluded.
    /// </summary>
    public (int First, int Last)? IndexRange(VerseRange range)
    {
        int first = -1, last = -1;
        for (int n = range.First; n <= range.Last && first < 0; n++) first = IndexOf(n);
        for (int n = range.Last; n >= range.First && last < 0; n--) last = IndexOf(n);
        return first < 0 ? null : (first, last);
    }
}
