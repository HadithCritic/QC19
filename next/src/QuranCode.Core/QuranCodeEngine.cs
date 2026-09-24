using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Search;
using QuranCode.Core.Search.Numbers;
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
/// <b>How the text is counted</b> is a <see cref="CountingOptions"/>: the
/// original's Statistics-panel options. Everything expensive is lazy and
/// cached per (text mode, effective options).
/// </para>
/// </remarks>
public sealed partial class QuranCodeEngine : IDisposable
{
    private readonly ContentRepository _content;
    private readonly Dictionary<bool, CorpusView> _views = [];
    private readonly Dictionary<(string, CountingOptions), Segmentation> _segmentations = [];
    private readonly Dictionary<(string, CountingOptions), TextSearch> _searches = [];
    private readonly Dictionary<(string, CountingOptions), CountingText> _countingTexts = [];
    private readonly Dictionary<bool, RootSearch> _rootSearches = [];
    private readonly Dictionary<(string, CountingOptions), VerseSimilarity> _similarities = [];
    private readonly Dictionary<(string, CountingOptions), long[]> _wordValues = [];
    private readonly Dictionary<(string, CountingOptions), NumberSearch> _numberSearches = [];

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

    private bool VerseZero => Corpus.Basmala == BasmalaMode.VerseZero;

    /// <summary>The 114 chapters.</summary>
    public IReadOnlyList<Chapter> Chapters => _content.Chapters;

    /// <summary>Every verse row, verse-0 Bismillahs included, in canonical order.</summary>
    public IReadOnlyList<Verse> Verses => _content.Verses;

    /// <summary>Roots of every display word.</summary>
    public WordRoots Roots => _content.WordRoots;

    /// <summary>A display word's gloss, transliteration and grammar (index counts the Bismillah header).</summary>
    public WordData? WordDataOf(int verseNumber, int wordIndex) => _content.WordDataOf(verseNumber, wordIndex);

    /// <summary>The English (en) or Arabic (ar) name of a corpus tag or feature, or null.</summary>
    public string? GrammarLabel(string tag, string language) => _content.GrammarLabel(tag, language);

    /// <summary>A verse's prostration type, recommended or obligatory, or null.</summary>
    public string? ProstrationOf(int verseNumber) => _content.Prostrations.GetValueOrDefault(verseNumber);

    /// <summary>The translations and other verse texts of this edition.</summary>
    public IReadOnlyList<TranslationInfo> Translations => _content.Translations;

    /// <summary>A translation by key, or null.</summary>
    public TranslationInfo? Translation(string key) => Translations.FirstOrDefault(t => t.Key == key);

    private readonly Dictionary<int, IReadOnlyDictionary<int, string>> _translationTexts = [];

    /// <summary>Every verse of one translation, loaded once.</summary>
    public IReadOnlyDictionary<int, string> AllTranslationText(TranslationInfo translation)
    {
        int key = CacheKey(translation);
        if (_translationTexts.TryGetValue(key, out IReadOnlyDictionary<int, string>? cached)) return cached;
        return _translationTexts[key] = TranslationText(translation, 1, Verses.Count);
    }

    /// <summary>
    /// The Emlaaei fallback (Features.txt #52): verses whose standard-spelling
    /// text holds the term, both simplified in the text mode, as the legacy
    /// searches when the Uthmani text finds nothing. Empty when the edition
    /// has no such text.
    /// </summary>
    public IReadOnlyList<int> EmlaaeiSearch(
        string term, Wordness wordness, string textMode = DefaultTextMode,
        CountingOptions? counting = null, IReadOnlySet<int>? scope = null)
    {
        TranslationInfo? emlaaei = Translations.FirstOrDefault(t => t.Kind == "emlaaei");
        if (emlaaei is null) return [];
        TextPipeline pipeline = Pipeline(textMode);
        string needle = string.Join(' ', pipeline.Normalize(term).Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (needle.Length == 0) return [];

        CorpusView view = View(counting);
        var found = new List<int>();
        foreach ((int verse, string text) in AllTranslationText(emlaaei).OrderBy(t => t.Key))
        {
            if (view.IndexOf(verse) < 0 || (scope is not null && !scope.Contains(verse))) continue;
            string line = string.Join(' ', pipeline.Normalize(text).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (TranslationSearch.Matches(line, needle, wordness).Count > 0) found.Add(verse);
        }
        return found;
    }

    /// <summary>One translation's text for a run of verses.</summary>
    public IReadOnlyDictionary<int, string> TranslationText(TranslationInfo translation, int firstVerse, int lastVerse) =>
        _content.TranslationText(translation.Id, firstVerse, lastVerse);

    private static int CacheKey(TranslationInfo translation) => translation.Id;

    /// <summary>Names of every installed value system.</summary>
    public IReadOnlyList<string> ValueSystems() => _content.ValueSystemNames();

    /// <summary>Every installed value system with its parts and visibility.</summary>
    public IReadOnlyList<ValueSystemSummary> ValueSystemSummaries() => _content.ValueSystemSummaries();

    /// <summary>One verse by absolute number.</summary>
    public Verse Verse(int number) => _content.Verses[number - 1];

    /// <summary>One verse by chapter and number in chapter, as in "2:255" or "2:0".</summary>
    public Verse Verse(int chapter, int numberInChapter) =>
        _content.Verses[_content.Chapters[chapter - 1].AbsoluteOf(numberInChapter) - 1];

    /// <summary>The options that actually apply in a text mode.</summary>
    public CountingOptions Effective(string textMode, CountingOptions? counting) =>
        (counting ?? CountingOptions.Default).For(BaseOf(textMode), VerseZero);

    /// <summary>The verses counted with or without verse-0 Bismillahs.</summary>
    public CorpusView View(CountingOptions? counting = null)
    {
        // Only an edition with verse-0 rows has anything to leave out; the
        // classic edition strips its Bismillah from the text instead.
        bool include = !VerseZero || (counting ?? CountingOptions.Default).IncludeBasmalas;
        if (_views.TryGetValue(include, out CorpusView? cached)) return cached;

        var view = new CorpusView(_content.Verses, include);
        _views[include] = view;
        return view;
    }

    /// <summary>The normalization pipeline for a text mode.</summary>
    public TextPipeline Pipeline(string textMode = DefaultTextMode) => PipelineOf(textMode);

    /// <summary>A value system by name.</summary>
    public ValueSystem ValueSystem(string name = DefaultValueSystem) =>
        _content.GetValueSystem(name);

    /// <summary>How verses are turned into counted text under a text mode and options.</summary>
    public CountingText CountingText(string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        CountingOptions effective = Effective(textMode, counting);
        var key = (textMode, effective);
        if (_countingTexts.TryGetValue(key, out CountingText? cached)) return cached;

        TextPipeline pipeline = Pipeline(textMode);
        var plain = new CountingText(pipeline, _content.VerseRules, effective, Corpus.Basmala, _content.WawWords);
        WawWords waw = Text.CountingText.WawWordsFor(_content.WawWords, effective, View(effective).Verses, plain, pipeline);
        CountingText text = ReferenceEquals(waw, _content.WawWords)
            ? plain
            : new CountingText(pipeline, _content.VerseRules, effective, Corpus.Basmala, waw);

        _countingTexts[key] = text;
        return text;
    }

    /// <summary>The counted verses segmented under a text mode. Built once per mode and options.</summary>
    public Segmentation Segmentation(string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        CountingOptions effective = Effective(textMode, counting);
        var key = (textMode, effective);
        if (_segmentations.TryGetValue(key, out Segmentation? cached)) return cached;

        CountingText text = CountingText(textMode, effective);
        Segmentation built = Content.Segmentation.Build(View(effective).Verses, text.Normalize);
        _segmentations[key] = built;
        return built;
    }

    /// <summary>Text search over a text mode and options. Built once per pair.</summary>
    public TextSearch Search(string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        CountingOptions effective = Effective(textMode, counting);
        var key = (textMode, effective);
        if (_searches.TryGetValue(key, out TextSearch? cached)) return cached;

        var search = new TextSearch(
            Segmentation(textMode, effective), View(effective).Verses, Pipeline(textMode),
            CountingText(textMode, effective).NormalizeWord);
        _searches[key] = search;
        return search;
    }

    /// <summary>Root search over the counted verses.</summary>
    public RootSearch RootSearch(CountingOptions? counting = null)
    {
        CorpusView view = View(counting);
        bool key = view.Verses.Count == Verses.Count;
        if (_rootSearches.TryGetValue(key, out RootSearch? cached)) return cached;
        return _rootSearches[key] = new RootSearch(Roots, view.Verses);
    }

    /// <summary>Related and similar verses under a text mode and options.</summary>
    public VerseSimilarity Similarity(string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        CountingOptions effective = Effective(textMode, counting);
        var key = (textMode, effective);
        if (_similarities.TryGetValue(key, out VerseSimilarity? cached)) return cached;
        return _similarities[key] = new VerseSimilarity(
            Segmentation(textMode, effective), View(effective).Verses, Search(textMode, effective).WordTexts,
            Roots, RootSearch(effective));
    }

    /// <summary>Plain value (sum of letter values) of every counted word, in corpus order.</summary>
    public IReadOnlyList<long> WordValues(string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ValueSystem system = ValueSystem(valueSystem);
        CountingOptions effective = Effective(system.TextModeName, counting);
        var key = (valueSystem, effective);
        if (_wordValues.TryGetValue(key, out long[]? cached)) return cached;

        Segmentation segmentation = Segmentation(system.TextModeName, effective);
        long[] values = new long[segmentation.WordCount];
        for (int w = 0; w < values.Length; w++)
        {
            int first = segmentation.WordFirstLetter[w];
            if (system.Radix is not null)
            {
                values[w] = system.BaseWordValue(segmentation.LetterChars.AsSpan(first, segmentation.WordLetterCount[w]));
                continue;
            }
            for (int l = first; l < first + segmentation.WordLetterCount[w]; l++) values[w] += system[segmentation.LetterChars[l]];
        }
        return _wordValues[key] = values;
    }

    /// <summary>Number and frequency searches under a value system (and its text mode) and options.</summary>
    public NumberSearch NumberSearch(string valueSystem = DefaultValueSystem, CountingOptions? counting = null)
    {
        ValueSystem system = ValueSystem(valueSystem);
        string textMode = system.TextModeName;
        CountingOptions effective = Effective(textMode, counting);
        var key = (valueSystem, effective);
        if (_numberSearches.TryGetValue(key, out NumberSearch? cached)) return cached;

        CorpusView view = View(effective);
        var index = new UnitIndex(
            Segmentation(textMode, effective), view.Verses, WordValues(valueSystem, effective),
            CountingText(textMode, effective), Display, _content.PauseMarks);
        return _numberSearches[key] = new NumberSearch(index, Blocks(view));
    }

    private static readonly (UnitKind Unit, string Kind)[] PartitionUnits =
    [
        (UnitKind.Pages, "page"), (UnitKind.Stations, "station"), (UnitKind.Parts, "part"), (UnitKind.Groups, "group"),
        (UnitKind.Halves, "half"), (UnitKind.Quarters, "quarter"), (UnitKind.Bowings, "bowing"),
    ];

    /// <summary>Chapters and partitions as runs of the counted verses.</summary>
    private Dictionary<UnitKind, IReadOnlyList<Block>> Blocks(CorpusView view)
    {
        IReadOnlyList<Verse> verses = view.Verses;
        Block? Over(int number, int firstVerse, int lastVerse)
        {
            int first = -1, last = -1;
            for (int v = 0; v < verses.Count; v++)
            {
                if (verses[v].Number < firstVerse || verses[v].Number > lastVerse) continue;
                if (first < 0) first = v;
                last = v;
            }
            return first < 0 ? null : new Block(number, first, last);
        }

        var blocks = new Dictionary<UnitKind, IReadOnlyList<Block>>
        {
            [UnitKind.Chapters] = Chapters.Select(c => Over(c.Number, c.FirstVerse, c.LastVerse)).OfType<Block>().ToArray(),
        };
        foreach ((UnitKind unit, string kind) in PartitionUnits)
        {
            blocks[unit] = Partitions.TryGetValue(kind, out Partition[]? parts)
                ? parts.Select(p => Over(p.Number, p.FirstVerse, p.LastVerse)).OfType<Block>().ToArray()
                : [];
        }
        return blocks;
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
    /// options do not count.
    /// </summary>
    public long? ValueOfVerse(
        int verseNumber,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null,
        CountingOptions? counting = null)
    {
        int index = View(counting).IndexOf(verseNumber);
        if (index < 0) return null;

        return SegmentedCalculator.ValueOfVerse(
            Segmentation(textMode, counting), index, ValueSystem(valueSystem),
            profile ?? CalculationProfile.Default, modifiers ?? ModifierSet.None);
    }

    /// <summary>Value of one chapter.</summary>
    public long ValueOfChapter(
        int chapterNumber,
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        ModifierSet? modifiers = null,
        CountingOptions? counting = null) =>
        SegmentedCalculator.ValueOfChapter(
            Segmentation(textMode, counting), chapterNumber, ValueSystem(valueSystem),
            profile ?? CalculationProfile.Default, modifiers ?? ModifierSet.None);

    /// <summary>Value of the whole book.</summary>
    public long ValueOfBook(
        string valueSystem = DefaultValueSystem,
        string textMode = DefaultTextMode,
        CalculationProfile? profile = null,
        CountingOptions? counting = null)
    {
        Segmentation segmentation = Segmentation(textMode, counting);
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
        CountingOptions? counting = null)
    {
        ValueSystem system = ValueSystem(valueSystem);
        return SelectionStatistics.Compute(
            Segmentation(system.TextModeName, counting), View(counting), Chapters, range, system,
            profile ?? CalculationProfile.Default, modifiers ?? ModifierSet.None);
    }

    /// <summary>Statistics for every chapter, for sorting and chapter details (Features.txt #29, #59).</summary>
    public IReadOnlyList<SelectionStatistics> ChapterStatistics(
        string valueSystem = DefaultValueSystem, CountingOptions? counting = null) =>
        Chapters.Select(c => Statistics(new VerseRange(c.FirstVerse, c.LastVerse), valueSystem, counting: counting)).ToArray();

    /// <summary>
    /// Distance between two words the reader clicked, in chapters, verses,
    /// words and letters of the counted text; null when either is not counted.
    /// </summary>
    public WordDistance? Distance(
        WordLocation from, WordLocation to, string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        Segmentation segmentation = Segmentation(textMode, counting);
        CorpusView view = View(counting);
        CountingText text = CountingText(textMode, counting);

        CountedWord? Find(WordLocation location)
        {
            if (location.Verse < 1 || location.Verse > Verses.Count) return null;
            Verse verse = Verse(location.Verse);
            return WordDistance.Locate(location, verse, Display(verse), segmentation, view, text.NormalizeWord);
        }

        return Find(from) is CountedWord a && Find(to) is CountedWord b ? WordDistance.Between(a, b) : null;
    }

    /// <summary>Pages, parts, stations and the other divisions, by kind.</summary>
    public IReadOnlyDictionary<string, Partition[]> Partitions => _content.Partitions;

    /// <summary>
    /// Parses a typed reference: by chapter ("2:255-257", "3-4", "2:0") or by
    /// unit ("page 10", "part 3-4", "word 100"). Word and letter numbers follow
    /// the text as counted under the text mode and options.
    /// </summary>
    public ReferenceParseResult ParseReference(
        string text, string textMode = DefaultTextMode, CountingOptions? counting = null)
    {
        if (!UnitReferences.LooksLikeUnit(text ?? "")) return ReferenceParser.Parse(text, Chapters);

        return UnitReferences.Parse(
            text!, Partitions, Verses.Count,
            word => VerseOfUnit(word, textMode, counting, letters: false),
            letter => VerseOfUnit(letter, textMode, counting, letters: true));
    }

    private int? VerseOfUnit(int number, string textMode, CountingOptions? counting, bool letters)
    {
        Segmentation segmentation = Segmentation(textMode, counting);
        int count = letters ? segmentation.LetterCount : segmentation.WordCount;
        if (number < 1 || number > count) return null;

        int word = letters ? segmentation.LetterWord[number - 1] : number - 1;
        return View(counting).Verses[segmentation.WordVerse[word]].Number;
    }

    public void Dispose()
    {
        _content.Dispose();
    }
}
