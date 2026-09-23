namespace QuranCode.Engine.Protocol;

// Wire types. Kept separate from the engine's own records so the protocol is
// an explicit contract: renaming an engine field cannot silently change what
// the UI receives. The TypeScript mirror is apps/desktop/src/lib/engine/types.ts.

/// <param name="VerseCount">Numbered verses, not counting verse-0 Bismillahs.</param>
/// <param name="RowCount">Every verse row, verse-0 Bismillahs included.</param>
/// <param name="Basmala">"prefix" (classic) or "verse-zero" (Submission).</param>
internal sealed record EngineInfo(
    string Version,
    string Edition,
    string Basmala,
    int ChapterCount,
    int VerseCount,
    int RowCount,
    int ValueSystemCount,
    string DefaultValueSystem);

internal sealed record ChapterDto(
    int Number,
    string Name,
    string TransliteratedName,
    string EnglishName,
    int RevelationOrder,
    string RevelationPlace,
    int VerseCount,
    int FirstVerse,
    bool HasVerseZero,
    string Initialization);

internal sealed record ValueSystemDto(
    string Name,
    string TextMode,
    string LetterOrder,
    string LetterValue,
    bool ResearchOnly);

/// <summary>A verse split for display. <c>Bismillah</c> is the chapter header when present.</summary>
/// <param name="IsBasmala">A verse-0 Bismillah, which the user may choose not to count.</param>
internal sealed record VerseDto(
    int Number,
    int Chapter,
    int NumberInChapter,
    bool IsBasmala,
    string? Bismillah,
    IReadOnlyList<string> Words);

/// <summary>
/// Analysis of one number. <c>Value</c> is a decimal string because a 64-bit
/// total can exceed what a JavaScript number holds exactly (2^53).
/// </summary>
internal sealed record NumberDto(
    string Value,
    string Class,
    string Code,
    long DigitSum,
    long DigitalRoot,
    long? FamilyOrdinal,
    long? ClassOrdinal,
    IReadOnlyList<long>? Factors);

internal sealed record LetterCountDto(string Letter, int Count);

/// <summary>
/// Parameters shared by the selection lists. Each method reads what it needs:
/// <c>WithMarks</c> for words, <c>Scope</c> (book, chapter, verse, word) for
/// letters, <c>AbsoluteDifference</c> and <c>VOverC</c> for the maths sums,
/// <c>Kind</c> (wordLetters, verseWords, verseLetters) and <c>Boundaries</c> for symmetry.
/// </summary>
internal sealed record SelectionParams(
    int First,
    int Last,
    string? ValueSystem = null,
    CountingDto? Counting = null,
    bool WithMarks = false,
    string? Scope = null,
    bool AbsoluteDifference = false,
    bool VOverC = false,
    string? Kind = null,
    bool Boundaries = false);

internal sealed record WordCountDto(string Word, int Count);

internal sealed record WordFrequenciesDto(int Total, int Unique, IReadOnlyList<WordCountDto> Words);

internal sealed record LetterStatisticDto(string Letter, int Order, int Count, long PositionSum, long DistanceSum);

/// <param name="Ratio">d/u, null when no value occurs once.</param>
internal sealed record QuantitySumsDto(double Sum, double Odd, double Even, double Prime, double Composite, double? Ratio);

internal sealed record CvSumsDto(
    int Count,
    QuantitySumsDto C,
    QuantitySumsDto V,
    QuantitySumsDto Plus,
    QuantitySumsDto Minus,
    QuantitySumsDto Times,
    QuantitySumsDto Divided);

/// <summary>The Maths tab: chapter sums (C = chapter, V = its verses) and verse sums (V = verse number).</summary>
internal sealed record MathsDto(CvSumsDto Chapters, CvSumsDto Verses);

internal sealed record SymmetryPointDto(int Position, long Total, long PositionSum, long TotalSum);

internal sealed record SymmetryDto(int Units, IReadOnlyList<SymmetryPointDto> Points, double Percent);

/// <param name="Method">allah, nonAllah, all, double or repeated.</param>
/// <param name="Gap">For repeated: words between the two.</param>
/// <param name="Tsv">Return the whole table as tab-separated text instead of a page of rows.</param>
internal sealed record ResearchParams(
    string Method,
    int? First = null,
    int? Last = null,
    int Gap = 0,
    string? ValueSystem = null,
    CountingDto? Counting = null,
    int? Offset = null,
    int? Limit = null,
    bool Tsv = false);

internal sealed record ResearchTableDto(
    IReadOnlyList<string> Columns,
    int RowCount,
    int Offset,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    string? Tsv);

internal sealed record AllahSummaryDto(int Allah, int WithAllah, int WithLillah, int Total);

/// <param name="Scope">verse, chapter, page, station, part, group, half, quarter, bowing or book.</param>
/// <param name="Measure">letters or value.</param>
/// <param name="Length">short (the ratio) or long (one minus it).</param>
/// <param name="Boundary">letter, word, sentence, verse or chapter.</param>
internal sealed record RatioParams(
    int Chapter,
    string? Scope = null,
    double? Ratio = null,
    string? Measure = null,
    string? Length = null,
    string? Boundary = null,
    string? ValueSystem = null,
    CountingDto? Counting = null);

/// <summary>
/// One unit's split. <c>SplitVerse</c> and <c>SplitWord</c> (an index into the
/// verse's displayed words, negative inside a Bismillah header) name the word
/// the first part ends in, and <c>SplitLetters</c> how many of its letters it
/// takes.
/// </summary>
internal sealed record RatioUnitDto(
    int FirstVerse,
    int LastVerse,
    bool Colored,
    int SplitVerse,
    int SplitWord,
    int SplitLetters,
    int FirstLetters,
    string FirstValue,
    int SecondLetters,
    string SecondValue);

/// <param name="Word">Index among the verse's display words, Bismillah header included.</param>
internal sealed record WordParams(int Verse, int Word);

/// <summary>A grammar feature: the corpus's text, its English and Arabic names, and Arabic for a lemma or root.</summary>
internal sealed record FeatureDto(string Text, string? English, string? Arabic, string? Script);

internal sealed record WordPartDto(int Part, string Arabic, string Form, string Tag, string? TagEnglish, string? TagArabic, IReadOnlyList<FeatureDto> Features);

internal sealed record WordInfoDto(
    int Verse,
    int Word,
    string Text,
    string? Meaning,
    string? Transliteration,
    IReadOnlyList<string> Roots,
    IReadOnlyList<WordPartDto> Parts);

/// <param name="Pack">Whether it comes from an optional translation pack rather than the edition itself.</param>
internal sealed record TranslationDto(string Key, string Language, string Name, string Translator, string Kind, bool RightToLeft, bool Pack);

/// <param name="Keys">Translation keys, as translations.list gives them.</param>
internal sealed record TranslationTextParams(IReadOnlyList<string> Keys, int First, int Last);

internal sealed record VerseTextDto(int Verse, string Text);

internal sealed record TranslationTextDto(string Key, IReadOnlyList<VerseTextDto> Verses);

/// <summary>A pair (a, b) of a split such as a² + b²; numbers stay under a million, so plain integers.</summary>
internal sealed record PairDto(long A, long B);

internal sealed record SplitsDto(
    IReadOnlyList<PairDto> SquareSums,
    IReadOnlyList<PairDto> SquareDifferences,
    IReadOnlyList<PairDto> CubeSums,
    IReadOnlyList<PairDto> CubeDifferences);

/// <param name="Ordinal">1-based place among primes (or composites) of the same form; null when too far to count.</param>
internal sealed record FourNDto(string Form, long N, long? Ordinal);

internal sealed record IndexChainDto(
    string Text,
    int Length,
    long Sum,
    long PrimesAsZero,
    long PrimesAsZeroReversed,
    long PrimesAsOne,
    long PrimesAsOneReversed);

/// <summary>
/// The value panel's further facts about a number (Features.txt #6, #7, #9 and
/// the divisor and power colors). Parts are null where they do not apply or
/// the number is beyond what they are computed for.
/// </summary>
/// <param name="Divisors">Every divisor, when there are at most 1,000 (decimal strings).</param>
/// <param name="Power">The highest power (2 to 10) the number is.</param>
internal sealed record NumberDetailsDto(
    NumberDto Number,
    int? DivisorCount,
    IReadOnlyList<string>? Divisors,
    string? DivisorSum,
    int? Power,
    bool Carmichael,
    FourNDto? FourN,
    SplitsDto? Splits,
    IndexChainDto? Chain);

/// <summary>
/// A verse's value and class code, for coloring verse markers; both null for a
/// Bismillah that is not being counted.
/// </summary>
internal sealed record VerseValueDto(int Number, string? Value, string? Code);

/// <summary>Counted verses before and after the selection (Features.txt #11).</summary>
internal sealed record PositionDto(int BeforeInChapter, int AfterInChapter, int BeforeInBook, int AfterInBook);

internal sealed record StatsDto(
    int First,
    int Last,
    string ValueSystem,
    PositionDto Position,
    NumberDto Chapters,
    NumberDto Verses,
    NumberDto Words,
    NumberDto Letters,
    NumberDto DistinctLetters,
    NumberDto Value,
    IReadOnlyList<LetterCountDto> LetterFrequencies);

internal sealed record RangeDto(int First, int Last);

/// <summary>One chapter's figures under the current system and counting, for sorting.</summary>
internal sealed record ChapterStatsDto(int Chapter, int Verses, int Words, int Letters, string Value, string Code);

internal sealed record WordLocationDto(int Verse, int Word);

internal sealed record DistanceDto(int Chapters, int Verses, int Words, int Letters);

/// <summary>
/// A bookmark. <c>First</c> and <c>Last</c> are absolute verse numbers in the
/// open edition, null when the stored chapter:verse does not exist in it.
/// </summary>
internal sealed record BookmarkDto(long Id, string Reference, int? First, int? Last, string Note, string CreatedUtc, string UpdatedUtc);

internal sealed record HistoryDto(long Id, string Kind, string? Reference, int? First, int? Last, string? Term, string? Wordness, string AtUtc);

/// <summary>One system's value for a text, with how many letters that system's text mode counts.</summary>
internal sealed record SystemValueDto(string ValueSystem, int LetterCount, NumberDto Value);

/// <summary>A verse that matched, with the display word indices to highlight.</summary>
/// <param name="Aligned">
/// False when display words could not be mapped to searchable words; the UI
/// then marks the whole verse instead of individual words.
/// </param>
/// <param name="BismillahHighlights">
/// Words of the Bismillah header to highlight. The engine counts the header
/// as the first words of verse 1, one display word per engine word.
/// </param>
internal sealed record SearchVerseDto(
    int Number,
    int Chapter,
    int NumberInChapter,
    bool IsBasmala,
    string? Bismillah,
    IReadOnlyList<string> Words,
    IReadOnlyList<int> Highlights,
    bool Aligned,
    IReadOnlyList<int> BismillahHighlights,
    int MatchCount,
    double? Score = null,
    IReadOnlyList<TranslationMatchDto>? Translations = null);

/// <summary>A translation line a search matched: its key, text, and each match as [start, length].</summary>
internal sealed record TranslationMatchDto(string Key, string Text, IReadOnlyList<IReadOnlyList<int>> Ranges);

/// <param name="ChapterCounts">Matches per chapter (words, or verses when no words are marked), for shading the chapter list.</param>
/// <param name="Roots">For a root search, the root each term resolved to (null when none).</param>
/// <param name="VerseNumbers">Every found verse, not only this page, so a later search can look within them.</param>
internal sealed record SearchResultDto(
    string Term,
    int WordCount,
    int VerseCount,
    int Offset,
    IReadOnlyList<SearchVerseDto> Verses,
    IReadOnlyList<int> ChapterCounts,
    IReadOnlyList<int> VerseNumbers,
    IReadOnlyList<RootTermDto>? Roots = null,
    string? FoundIn = null);

internal sealed record RootTermDto(string Term, string Kind, string? Root);

/// <summary>Where to search: the whole book when null, else a verse range or a list of verses.</summary>
internal sealed record ScopeDto(int? First = null, int? Last = null, IReadOnlyList<int>? Verses = null);

/// <summary>How the text is counted; every member optional, defaults as in the original.</summary>
internal sealed record CountingDto(
    bool IncludeBasmalas = true,
    bool WawAsWord = false,
    bool ShaddaAsLetter = false,
    bool HamzaAboveLine = false,
    bool ElfAboveLine = false,
    bool YaaAboveLine = false,
    bool NoonAboveLine = false);

// Parameters. Optional members carry a default; without one, strict
// constructor binding treats even a nullable parameter as required.

internal sealed record ChapterParams(int Chapter);

internal sealed record ChapterValuesParams(int Chapter, string? ValueSystem = null, CountingDto? Counting = null);

internal sealed record RangeParams(int First, int Last, string? ValueSystem = null, CountingDto? Counting = null);

internal sealed record ReferenceParams(string Text, string? ValueSystem = null, CountingDto? Counting = null);

internal sealed record ChaptersStatsParams(string? ValueSystem = null, CountingDto? Counting = null);

internal sealed record DistanceParams(WordLocationDto From, WordLocationDto To, string? ValueSystem = null, CountingDto? Counting = null);

internal sealed record BookmarkSaveParams(int First, int Last, string Note = "");

internal sealed record IdParams(long Id);

internal sealed record HistoryListParams(string Kind, int Limit = 50);

internal sealed record HistoryAddParams(string Kind, int? First = null, int? Last = null, string? Term = null, string? Wordness = null);

internal sealed record HistoryClearParams(string Kind);

internal sealed record NumberParams(string Value);

internal sealed record TextValuesParams(string Text, IReadOnlyList<string>? ValueSystems = null);

internal sealed record SearchParams(
    string Term,
    string? Wordness = null,
    string? ValueSystem = null,
    int? Offset = null,
    int? Limit = null,
    CountingDto? Counting = null,
    string? Grouping = null,
    ScopeDto? Scope = null,
    IReadOnlyList<string>? Translations = null);

/// <summary>
/// One constraint of a number search. <c>Value</c> is a decimal string (it may
/// be negative, counting from the end); <c>Comparison</c> is eq, ne, lt, le,
/// gt, ge, div, ndiv or sum; <c>Type</c> is a number kind (none, natural,
/// prime, additivePrime, nonAdditivePrime, composite, additiveComposite,
/// nonAdditiveComposite, odd, even, fibonacci, square, cubic ... decic).
/// </summary>
internal sealed record CriterionDto(string? Value = null, string? Comparison = null, string? Type = null, int? Remainder = null);

/// <param name="Unit">words, verses, chapters, sentences, pages, stations, parts, groups, halves, quarters or bowings.</param>
/// <param name="Shape">single, range or set.</param>
/// <param name="NumberScope">book, chapter or verse: which number the number constraint reads.</param>
internal sealed record NumberSearchParams(
    string Unit,
    string? Shape = null,
    int? Size = null,
    string? NumberScope = null,
    CriterionDto? Number = null,
    CriterionDto? Verses = null,
    CriterionDto? Words = null,
    CriterionDto? Letters = null,
    CriterionDto? UniqueLetters = null,
    CriterionDto? Value = null,
    CriterionDto? Frequency = null,
    CriterionDto? Occurrence = null,
    string? ValueSystem = null,
    int? Offset = null,
    int? Limit = null,
    CountingDto? Counting = null,
    ScopeDto? Scope = null);

/// <param name="Match">Instead of a sum: all, any, only or none (of the phrase's letters).</param>
internal sealed record FrequencySearchParams(
    string Unit,
    string Phrase,
    string? Shape = null,
    int? Size = null,
    bool UniqueLetters = false,
    CriterionDto? Sum = null,
    string? Match = null,
    string? ValueSystem = null,
    int? Offset = null,
    int? Limit = null,
    CountingDto? Counting = null,
    ScopeDto? Scope = null);

/// <summary>A found unit, run or set, what it measures, and a few of its verses to show.</summary>
/// <param name="FirstVerse">Absolute number of its first verse; with <c>LastVerse</c>, what opening it selects.</param>
/// <param name="VerseNumbers">For a set of verses, blocks or words: the verses it covers.</param>
/// <param name="LetterFrequencySum">For a frequency search, the sum it measured.</param>
/// <param name="MorePreview">True when the unit has more verses than the preview shows.</param>
internal sealed record FoundUnitDto(
    string Reference,
    int FirstVerse,
    int LastVerse,
    IReadOnlyList<int>? VerseNumbers,
    int Verses,
    int Words,
    int Letters,
    int UniqueLetters,
    NumberDto Value,
    string? LetterFrequencySum,
    IReadOnlyList<SearchVerseDto> Preview,
    bool MorePreview);

internal sealed record UnitSearchResultDto(
    int UnitCount,
    bool Truncated,
    int Offset,
    IReadOnlyList<FoundUnitDto> Units,
    IReadOnlyList<int> ChapterCounts,
    IReadOnlyList<int> VerseNumbers);

/// <summary>A search that starts from one verse, or one word of it (F4 to F6).</summary>
/// <param name="Word">Index among the verse's display words, Bismillah header included.</param>
/// <param name="Method">For search.similar: text, words, roots or values.</param>
/// <param name="Threshold">For search.similar: 0 to 1, default 0.7 as in the original.</param>
internal sealed record VerseSearchParams(
    int Verse,
    int? Word = null,
    string? Method = null,
    double? Threshold = null,
    string? ValueSystem = null,
    int? Offset = null,
    int? Limit = null,
    CountingDto? Counting = null,
    ScopeDto? Scope = null);
