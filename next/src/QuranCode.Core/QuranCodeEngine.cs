using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Search;
using QuranCode.Core.Text;

namespace QuranCode.Core;

/// <summary>
/// The engine every front end talks to.
/// </summary>
/// <remarks>
/// <para>
/// This is the one composition point: the desktop UI, the CLI and any future
/// API all sit on this and none of them reach past it into the repository or
/// the calculators. That is the separation the brief asks for in §43, and the
/// reason it is worth a type of its own rather than leaving each front end to
/// wire up its own pipeline.
/// </para>
/// <para>
/// It is explicitly <b>not</b> the legacy <c>Server</c>. There is no static
/// state, nothing here mutates global configuration, and an instance owns
/// exactly what it opened. Several engines can coexist over different databases
/// or different text modes, which the legacy design made impossible.
/// </para>
/// <para>
/// Everything expensive is lazy. Constructing an engine opens the database and
/// does nothing else; the segmentation and the search index are built on first
/// use and only for the text mode actually asked for.
/// </para>
/// </remarks>
public sealed class QuranCodeEngine : IDisposable
{
    private readonly ContentRepository _content;
    private readonly Dictionary<string, Segmentation> _segmentations = [];
    private readonly Dictionary<string, TextSearch> _searches = [];

    /// <summary>The text mode used when none is named.</summary>
    public const string DefaultTextMode = "Original";

    /// <summary>The value system used when none is named.</summary>
    public const string DefaultValueSystem = "Original_Alphabet_Primes1";

    public QuranCodeEngine(string contentDatabasePath)
    {
        _content = new ContentRepository(contentDatabasePath);
    }

    /// <summary>The 114 chapters.</summary>
    public IReadOnlyList<Chapter> Chapters => _content.Chapters;

    /// <summary>The 6,236 verses in canonical order.</summary>
    public IReadOnlyList<Verse> Verses => _content.Verses;

    /// <summary>Names of every installed value system.</summary>
    public IReadOnlyList<string> ValueSystems() => _content.ValueSystemNames();

    /// <summary>One verse by absolute number.</summary>
    public Verse Verse(int number) => _content.Verses[number - 1];

    /// <summary>One verse by chapter and position, as in "2:255".</summary>
    public Verse Verse(int chapter, int numberInChapter)
    {
        Chapter c = _content.Chapters[chapter - 1];
        return _content.Verses[c.FirstVerse - 1 + numberInChapter - 1];
    }

    /// <summary>Parses a "chapter:verse" reference.</summary>
    public static bool TryParseReference(string reference, out int chapter, out int verse)
    {
        chapter = verse = 0;
        string[] parts = reference.Split(':');
        return parts.Length == 2
            && int.TryParse(parts[0], out chapter)
            && int.TryParse(parts[1], out verse);
    }

    /// <summary>The normalization pipeline for a text mode.</summary>
    public TextPipeline Pipeline(string textMode = DefaultTextMode) =>
        new(_content.GetTextMode(textMode));

    /// <summary>A value system by name.</summary>
    public ValueSystem ValueSystem(string name = DefaultValueSystem) =>
        _content.GetValueSystem(name);

    /// <summary>
    /// The corpus segmented under a text mode. Built once per mode, on demand.
    /// </summary>
    public Segmentation Segmentation(string textMode = DefaultTextMode)
    {
        if (_segmentations.TryGetValue(textMode, out Segmentation? cached)) return cached;

        Segmentation built = Content.Segmentation.Build(_content.Verses, Pipeline(textMode));
        _segmentations[textMode] = built;
        return built;
    }

    /// <summary>Text search over a text mode. Built once per mode, on demand.</summary>
    public TextSearch Search(string textMode = DefaultTextMode)
    {
        if (_searches.TryGetValue(textMode, out TextSearch? cached)) return cached;

        var search = new TextSearch(Segmentation(textMode), _content.Verses, Pipeline(textMode));
        _searches[textMode] = search;
        return search;
    }

    /// <summary>
    /// Value of arbitrary text.
    /// </summary>
    public long Value(
        string text,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null)
    {
        string normalized = Pipeline(textMode).Normalize(text);
        return ValueCalculator.Calculate(normalized, ValueSystem(valueSystem), profile ?? CalculationProfile.Default);
    }

    /// <summary>Value of one verse, by absolute number.</summary>
    public long ValueOfVerse(
        int verseNumber,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null)
    {
        profile ??= CalculationProfile.Default;
        modifiers ??= ModifierSet.None;

        return SegmentedCalculator.ValueOfVerse(
            Segmentation(textMode), verseNumber - 1, ValueSystem(valueSystem), profile, modifiers);
    }

    /// <summary>Value of one chapter.</summary>
    public long ValueOfChapter(
        int chapterNumber,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null)
    {
        profile ??= CalculationProfile.Default;
        modifiers ??= ModifierSet.None;

        return SegmentedCalculator.ValueOfChapter(
            Segmentation(textMode), chapterNumber, ValueSystem(valueSystem), profile, modifiers);
    }

    /// <summary>Value of the whole book.</summary>
    public long ValueOfBook(
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null)
    {
        Segmentation segmentation = Segmentation(textMode);
        return SegmentedCalculator.ValueOfVerses(
            segmentation, 0, segmentation.VerseCount,
            ValueSystem(valueSystem), profile ?? CalculationProfile.Default, ModifierSet.None);
    }

    public void Dispose() => _content.Dispose();
}
