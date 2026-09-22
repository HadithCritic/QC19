using QuranCode.Core.Analysis;
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
/// the calculators.
/// </para>
/// <para>
/// It is explicitly <b>not</b> the legacy <c>Server</c>. There is no static
/// state, nothing here mutates global configuration, and an instance owns
/// exactly what it opened. Several engines can coexist over different
/// databases, which is how the classic and Submission editions sit side by side.
/// </para>
/// <para>
/// <b>Verse numbers are absolute and stable</b>: 1..6236 in the classic
/// edition, 1..6346 in the Submission edition, whose verse-0 Bismillahs have
/// numbers of their own. Choosing not to count those Bismillahs never
/// renumbers anything; it selects a <see cref="CorpusView"/> without them, and
/// everything counted runs over that view.
/// </para>
/// <para>
/// Everything expensive is lazy and cached per (text mode, Bismillah choice).
/// </para>
/// </remarks>
public sealed class QuranCodeEngine : IDisposable
{
    private readonly ContentRepository _content;
    private readonly Dictionary<bool, CorpusView> _views = [];
    private readonly Dictionary<(string, bool), Segmentation> _segmentations = [];
    private readonly Dictionary<(string, bool), TextSearch> _searches = [];

    /// <summary>The text mode used when none is named.</summary>
    public const string DefaultTextMode = "Original";

    /// <summary>The value system used when none is named.</summary>
    public const string DefaultValueSystem = "Original_Alphabet_Primes1";

    public QuranCodeEngine(string contentDatabasePath)
    {
        _content = new ContentRepository(contentDatabasePath);
    }

    /// <summary>Which edition this engine reads.</summary>
    public CorpusInfo Corpus => _content.Corpus;

    /// <summary>The 114 chapters.</summary>
    public IReadOnlyList<Chapter> Chapters => _content.Chapters;

    /// <summary>Every verse row, verse-0 Bismillahs included, in canonical order.</summary>
    public IReadOnlyList<Verse> Verses => _content.Verses;

    /// <summary>Names of every installed value system.</summary>
    public IReadOnlyList<string> ValueSystems() => _content.ValueSystemNames();

    /// <summary>Every installed value system with its parts and visibility.</summary>
    public IReadOnlyList<ValueSystemSummary> ValueSystemSummaries() => _content.ValueSystemSummaries();

    /// <summary>One verse by absolute number.</summary>
    public Verse Verse(int number) => _content.Verses[number - 1];

    /// <summary>One verse by chapter and number in chapter, as in "2:255" or "2:0".</summary>
    public Verse Verse(int chapter, int numberInChapter) =>
        _content.Verses[_content.Chapters[chapter - 1].AbsoluteOf(numberInChapter) - 1];

    /// <summary>The verses counted with or without verse-0 Bismillahs.</summary>
    public CorpusView View(bool includeBasmalas = true)
    {
        // An edition without verse-0 rows has one view whatever is asked.
        bool key = includeBasmalas || Corpus.Basmala == BasmalaMode.Prefix;
        if (_views.TryGetValue(key, out CorpusView? cached)) return cached;

        var view = new CorpusView(_content.Verses, key);
        _views[key] = view;
        return view;
    }

    /// <summary>The normalization pipeline for a text mode.</summary>
    public TextPipeline Pipeline(string textMode = DefaultTextMode) =>
        new(_content.GetTextMode(textMode));

    /// <summary>A value system by name.</summary>
    public ValueSystem ValueSystem(string name = DefaultValueSystem) =>
        _content.GetValueSystem(name);

    /// <summary>The counted verses segmented under a text mode. Built once per mode and view.</summary>
    public Segmentation Segmentation(string textMode = DefaultTextMode, bool includeBasmalas = true)
    {
        CorpusView view = View(includeBasmalas);
        var key = (textMode, view.IncludesBasmalas);
        if (_segmentations.TryGetValue(key, out Segmentation? cached)) return cached;

        Segmentation built = Content.Segmentation.Build(
            view.Verses, Pipeline(textMode), verseRules: _content.VerseRules);
        _segmentations[key] = built;
        return built;
    }

    /// <summary>Text search over a text mode and view. Built once per pair.</summary>
    public TextSearch Search(string textMode = DefaultTextMode, bool includeBasmalas = true)
    {
        CorpusView view = View(includeBasmalas);
        var key = (textMode, view.IncludesBasmalas);
        if (_searches.TryGetValue(key, out TextSearch? cached)) return cached;

        var search = new TextSearch(Segmentation(textMode, includeBasmalas), view.Verses, Pipeline(textMode));
        _searches[key] = search;
        return search;
    }

    /// <summary>A verse split for display, following the edition's Bismillah convention.</summary>
    public VerseDisplay Display(Verse verse) =>
        DisplayWords.SplitVerse(verse.Text, verse.ChapterNumber, verse.NumberInChapter, Corpus.Basmala);

    /// <summary>Value of arbitrary text.</summary>
    public long Value(
        string text,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null)
    {
        string normalized = Pipeline(textMode).Normalize(text);
        return ValueCalculator.Calculate(normalized, ValueSystem(valueSystem), profile ?? CalculationProfile.Default);
    }

    /// <summary>
    /// Value of one verse, by absolute number; null when it is a Bismillah the
    /// view does not count.
    /// </summary>
    public long? ValueOfVerse(
        int verseNumber,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null,
        bool includeBasmalas = true)
    {
        int index = View(includeBasmalas).IndexOf(verseNumber);
        if (index < 0) return null;

        return SegmentedCalculator.ValueOfVerse(
            Segmentation(textMode, includeBasmalas), index, ValueSystem(valueSystem),
            profile ?? CalculationProfile.Default, modifiers ?? ModifierSet.None);
    }

    /// <summary>Value of one chapter.</summary>
    public long ValueOfChapter(
        int chapterNumber,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null,
        bool includeBasmalas = true) =>
        SegmentedCalculator.ValueOfChapter(
            Segmentation(textMode, includeBasmalas), chapterNumber, ValueSystem(valueSystem),
            profile ?? CalculationProfile.Default, modifiers ?? ModifierSet.None);

    /// <summary>Value of the whole book.</summary>
    public long ValueOfBook(
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        bool includeBasmalas = true)
    {
        Segmentation segmentation = Segmentation(textMode, includeBasmalas);
        return SegmentedCalculator.ValueOfVerses(
            segmentation, 0, segmentation.VerseCount,
            ValueSystem(valueSystem), profile ?? CalculationProfile.Default, ModifierSet.None);
    }

    /// <summary>
    /// Live statistics for a selection, valued under a system in that system's
    /// own text mode.
    /// </summary>
    /// <remarks>
    /// A value system belongs to one text mode (its name starts with it), and
    /// the legacy never values a system against another mode's letters, so the
    /// mode is derived rather than passed and cannot be mismatched.
    /// </remarks>
    public SelectionStatistics Statistics(
        VerseRange range,
        string valueSystem = DefaultValueSystem,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null,
        bool includeBasmalas = true)
    {
        ValueSystem system = ValueSystem(valueSystem);
        return SelectionStatistics.Compute(
            Segmentation(system.TextModeName, includeBasmalas), View(includeBasmalas), Chapters, range, system,
            profile ?? CalculationProfile.Default, modifiers ?? ModifierSet.None);
    }

    /// <summary>Parses a typed reference such as "2:255-257" or "2:0".</summary>
    public ReferenceParseResult ParseReference(string text) => ReferenceParser.Parse(text, Chapters);

    public void Dispose() => _content.Dispose();
}
