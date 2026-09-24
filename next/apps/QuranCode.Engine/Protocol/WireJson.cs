using System.Text.Json.Serialization;

namespace QuranCode.Engine.Protocol;

/// <summary>
/// Source-generated serializers for every wire type. Native AOT has no
/// reflection-based JSON, so a type missing here fails to compile.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(RpcError))]
[JsonSerializable(typeof(EngineInfo))]
[JsonSerializable(typeof(IReadOnlyList<ChapterDto>))]
[JsonSerializable(typeof(IReadOnlyList<ValueSystemDto>))]
[JsonSerializable(typeof(IReadOnlyList<VerseDto>))]
[JsonSerializable(typeof(NumberDto))]
[JsonSerializable(typeof(IReadOnlyList<VerseValueDto>))]
[JsonSerializable(typeof(ChapterValuesParams))]
[JsonSerializable(typeof(StatsDto))]
[JsonSerializable(typeof(RangeDto))]
[JsonSerializable(typeof(IReadOnlyList<SystemValueDto>))]
[JsonSerializable(typeof(SearchResultDto))]
[JsonSerializable(typeof(ChapterParams))]
[JsonSerializable(typeof(RangeParams))]
[JsonSerializable(typeof(ReferenceParams))]
[JsonSerializable(typeof(NumberParams))]
[JsonSerializable(typeof(TextValuesParams))]
[JsonSerializable(typeof(SearchParams))]
[JsonSerializable(typeof(VerseSearchParams))]
[JsonSerializable(typeof(NumberSearchParams))]
[JsonSerializable(typeof(FrequencySearchParams))]
[JsonSerializable(typeof(UnitSearchResultDto))]
[JsonSerializable(typeof(NumberDetailsDto))]
[JsonSerializable(typeof(IReadOnlyList<TranslationDto>))]
[JsonSerializable(typeof(IReadOnlyList<FindingDto>))]
[JsonSerializable(typeof(IReadOnlyList<InitialedChapterDto>))]
[JsonSerializable(typeof(IReadOnlyList<SweepTotalDto>))]
[JsonSerializable(typeof(WordParams))]
[JsonSerializable(typeof(WordInfoDto))]
[JsonSerializable(typeof(TranslationTextParams))]
[JsonSerializable(typeof(IReadOnlyList<TranslationTextDto>))]
[JsonSerializable(typeof(SelectionParams))]
[JsonSerializable(typeof(WordFrequenciesDto))]
[JsonSerializable(typeof(IReadOnlyList<LetterStatisticDto>))]
[JsonSerializable(typeof(MathsDto))]
[JsonSerializable(typeof(SymmetryDto))]
[JsonSerializable(typeof(ResearchParams))]
[JsonSerializable(typeof(ResearchTableDto))]
[JsonSerializable(typeof(AllahSummaryDto))]
[JsonSerializable(typeof(RatioParams))]
[JsonSerializable(typeof(IReadOnlyList<RatioUnitDto>))]
[JsonSerializable(typeof(CountingDto))]
[JsonSerializable(typeof(IReadOnlyList<ChapterStatsDto>))]
[JsonSerializable(typeof(DistanceDto))]
[JsonSerializable(typeof(IReadOnlyList<BookmarkDto>))]
[JsonSerializable(typeof(BookmarkDto))]
[JsonSerializable(typeof(IReadOnlyList<HistoryDto>))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(ChaptersStatsParams))]
[JsonSerializable(typeof(DistanceParams))]
[JsonSerializable(typeof(BookmarkSaveParams))]
[JsonSerializable(typeof(IdParams))]
[JsonSerializable(typeof(HistoryListParams))]
[JsonSerializable(typeof(HistoryAddParams))]
[JsonSerializable(typeof(HistoryClearParams))]
[JsonSerializable(typeof(TextModesDto))]
[JsonSerializable(typeof(TextModeDto))]
[JsonSerializable(typeof(NameParams))]
internal sealed partial class WireJson : JsonSerializerContext;
