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
    bool HasVerseZero);

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
    double? Score = null);

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
    IReadOnlyList<RootTermDto>? Roots = null);

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
    ScopeDto? Scope = null);

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
