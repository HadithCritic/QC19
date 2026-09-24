using System.Globalization;
using QuranCode.Core;
using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numbers;
using QuranCode.Core.Numerology;
using QuranCode.Core.Search;
using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Maps protocol calls onto the engine. Validation happens here, at the
/// boundary; the engine below assumes well-formed arguments.
/// </summary>
internal sealed partial class Handlers
{
    /// <summary>Longest text accepted for valuation: several pages of Arabic.</summary>
    public const int MaxTextLength = 20_000;

    public const int DefaultSearchLimit = 50;
    public const int MaxSearchLimit = 500;

    private readonly QuranCodeEngine _engine;
    private Dictionary<string, ValueSystemSummary> _systemsByName = [];
    private int _systemsVersion = -1;

    public Handlers(QuranCodeEngine engine)
    {
        _engine = engine;
    }

    /// <summary>Every value system, rebuilt when the reader's text modes change.</summary>
    private Dictionary<string, ValueSystemSummary> Systems
    {
        get
        {
            if (_systemsVersion == _engine.TextModesVersion) return _systemsByName;
            _systemsByName = _engine.ValueSystemSummaries().ToDictionary(s => s.Name, StringComparer.Ordinal);
            _systemsVersion = _engine.TextModesVersion;
            return _systemsByName;
        }
    }

    public EngineInfo Info() => new(
        typeof(Handlers).Assembly.GetName().Version?.ToString(3) ?? "0.0.0",
        _engine.Corpus.Edition,
        _engine.Corpus.Basmala == BasmalaMode.VerseZero ? "verse-zero" : "prefix",
        _engine.Chapters.Count,
        _engine.Chapters.Sum(c => c.VerseCount),
        _engine.Verses.Count,
        Systems.Count,
        QuranCodeEngine.DefaultValueSystem);

    public IReadOnlyList<ChapterDto> Chapters() => _engine.Chapters
        .Select(c => new ChapterDto(
            c.Number, c.Name, c.TransliteratedName, c.EnglishName,
            c.RevelationOrder, c.RevelationPlace, c.VerseCount, c.FirstVerse, c.HasVerseZero, c.Initialization))
        .ToArray();

    public IReadOnlyList<ValueSystemDto> ValueSystems() => Systems.Values
        .Select(s => new ValueSystemDto(s.Name, s.TextMode, s.LetterOrder, s.LetterValue, s.ResearchOnly))
        .ToArray();

    public IReadOnlyList<VerseDto> ChapterVerses(ChapterParams p)
    {
        Chapter chapter = RequireChapter(p.Chapter);
        var verses = new VerseDto[chapter.RowCount];
        for (int i = 0; i < verses.Length; i++)
        {
            Verse verse = _engine.Verse(chapter.FirstVerse + i);
            VerseDisplay display = _engine.Display(verse);
            verses[i] = new VerseDto(
                verse.Number, verse.ChapterNumber, verse.NumberInChapter, verse.IsBasmala, display.Bismillah, display.Words,
                _engine.ProstrationOf(verse.Number));
        }
        return verses;
    }

    /// <summary>
    /// Every verse's value in a chapter. Classification only, no ordinals:
    /// this feeds 286 verse markers at once and must stay cheap.
    /// </summary>
    public IReadOnlyList<VerseValueDto> ChapterValues(ChapterValuesParams p)
    {
        Chapter chapter = RequireChapter(p.Chapter);
        ValueSystemSummary system = RequireSystem(p.ValueSystem);

        var values = new VerseValueDto[chapter.RowCount];
        for (int i = 0; i < values.Length; i++)
        {
            int number = chapter.FirstVerse + i;
            long? value = _engine.ValueOfVerse(number, system.Name, system.TextMode, counting: Counting(p.Counting));
            values[i] = value is long v
                ? new VerseValueDto(number, v.ToString(CultureInfo.InvariantCulture), NumberTheory.Classify(v).Code())
                : new VerseValueDto(number, null, null);
        }
        return values;
    }

    public StatsDto Stats(RangeParams p)
    {
        string system = RequireSystem(p.ValueSystem).Name;
        CountingOptions counting = Counting(p.Counting);
        SelectionStatistics s;
        if (p.Selection is null)
        {
            s = _engine.Statistics(RequireRange(p.First, p.Last), system, counting: counting);
        }
        else
        {
            // Validated through the same scope as every range method, then analyzed exactly.
            _ = ScopeOf(p.First, p.Last, p.Selection, system, counting);
            QuranSelection selection = ToSelection(p.Selection);
            s = _engine.Analyze(selection, system, counting).Statistics
                ?? new SelectionStatistics(Envelope(selection), system, 0, 0, 0, 0, 0, 0, []);
        }
        VerseRange range = s.Range;

        SelectionPosition position = s.Position;
        return new StatsDto(
            range.First, range.Last, system,
            new PositionDto(position.BeforeInChapter, position.AfterInChapter, position.BeforeInBook, position.AfterInBook),
            Number(s.ChapterCount), Number(s.VerseCount), Number(s.WordCount),
            Number(s.LetterCount), Number(s.DistinctLetterCount), Number(s.Value),
            s.LetterFrequencies.Select(f => new LetterCountDto(f.Letter.ToString(), f.Count)).ToArray());
    }

    public RangeDto ParseReference(ReferenceParams p)
    {
        RequireText(p.Text, "text");
        ReferenceParseResult result = _engine.ParseReference(p.Text, RequireSystem(p.ValueSystem).TextMode, Counting(p.Counting));
        if (!result.IsSuccess) throw RpcException.InvalidParams(result.Error!);
        return new RangeDto(result.Range.First, result.Range.Last);
    }

    public NumberDto AnalyzeNumber(NumberParams p) => Number(ParseWholeNumber(p.Value));

    private static long ParseWholeNumber(string value)
    {
        string text = value.Trim().Replace(",", "", StringComparison.Ordinal);
        if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long parsed))
        {
            throw RpcException.InvalidParams($"\"{Truncate(value)}\" is not a whole number between -9223372036854775808 and 9223372036854775807.");
        }
        return parsed;
    }

    public IReadOnlyList<SystemValueDto> TextValues(TextValuesParams p)
    {
        RequireText(p.Text, "text");

        IEnumerable<ValueSystemSummary> systems = p.ValueSystems is { Count: > 0 } names
            ? names.Distinct(StringComparer.Ordinal).Select(RequireSystem)
            : Systems.Values;

        // Normalize once per text mode, not once per system: 276 systems share
        // eight text modes.
        var normalizedByMode = new Dictionary<string, (string Text, int Letters)>(StringComparer.Ordinal);
        return systems
            .Select(s =>
            {
                if (!normalizedByMode.TryGetValue(s.TextMode, out (string Text, int Letters) normalized))
                {
                    string text = _engine.Pipeline(s.TextMode).Normalize(p.Text);
                    normalized = (text, text.Count(char.IsLetter));
                    normalizedByMode[s.TextMode] = normalized;
                }
                long value = ValueCalculator.Calculate(normalized.Text, _engine.ValueSystem(s.Name));
                return new SystemValueDto(s.Name, normalized.Letters, Number(value));
            })
            .ToArray();
    }

    public IReadOnlyList<ChapterStatsDto> ChapterStats(ChaptersStatsParams p)
    {
        string system = RequireSystem(p.ValueSystem).Name;
        return _engine.ChapterStatistics(system, Counting(p.Counting))
            .Select((s, i) => new ChapterStatsDto(
                i + 1, s.VerseCount, s.WordCount, s.LetterCount,
                s.Value.ToString(CultureInfo.InvariantCulture), NumberTheory.Classify(s.Value).Code()))
            .ToArray();
    }

    public DistanceDto Distance(DistanceParams p)
    {
        ArgumentNullException.ThrowIfNull(p.From);
        ArgumentNullException.ThrowIfNull(p.To);
        string textMode = RequireSystem(p.ValueSystem).TextMode;
        WordDistance distance = _engine.Distance(
                new WordLocation(p.From.Verse, p.From.Word), new WordLocation(p.To.Verse, p.To.Word),
                textMode, Counting(p.Counting))
            ?? throw RpcException.NotFound("One of those words is not counted, so there is no distance to measure.");
        return new DistanceDto(distance.Chapters, distance.Verses, distance.Words, distance.Letters);
    }

    private static CountingOptions Counting(CountingDto? dto) => dto is null
        ? CountingOptions.Default
        : new CountingOptions
        {
            IncludeBasmalas = dto.IncludeBasmalas,
            WawAsWord = dto.WawAsWord,
            ShaddaAsLetter = dto.ShaddaAsLetter,
            HamzaAboveLine = dto.HamzaAboveLine,
            ElfAboveLine = dto.ElfAboveLine,
            YaaAboveLine = dto.YaaAboveLine,
            NoonAboveLine = dto.NoonAboveLine,
        };

    internal static NumberDto Number(long value)
    {
        NumberAnalysis a = NumberAnalysis.Of(value);
        return new NumberDto(
            value.ToString(CultureInfo.InvariantCulture), a.Class.ToString(), a.Code,
            a.DigitSum, a.DigitalRoot, a.FamilyOrdinal, a.ClassOrdinal, a.Factors);
    }

    private Chapter RequireChapter(int number)
    {
        if (number < 1 || number > _engine.Chapters.Count)
        {
            throw RpcException.InvalidParams($"Chapters run from 1 to {_engine.Chapters.Count}.");
        }
        return _engine.Chapters[number - 1];
    }

    private VerseRange RequireRange(int first, int last)
    {
        int count = _engine.Verses.Count;
        if (first < 1 || last > count || last < first)
        {
            throw RpcException.InvalidParams($"A range must satisfy 1 <= first <= last <= {count}.");
        }
        return new VerseRange(first, last);
    }

    private ValueSystemSummary RequireSystem(string? name)
    {
        name ??= QuranCodeEngine.DefaultValueSystem;
        return Systems.TryGetValue(name, out ValueSystemSummary system)
            ? system
            : throw RpcException.NotFound($"There is no value system named \"{Truncate(name)}\".");
    }

    private static void RequireText(string text, string name)
    {
        if (string.IsNullOrWhiteSpace(text)) throw RpcException.InvalidParams($"{name} is empty.");
        if (text.Length > MaxTextLength)
        {
            throw RpcException.InvalidParams($"{name} is longer than {MaxTextLength:N0} characters.");
        }
    }

    private static Wordness ParseWordness(string? value) => value switch
    {
        null or "any" => Wordness.Any,
        "whole" => Wordness.WholeWord,
        "part" => Wordness.PartOfWord,
        _ => throw RpcException.InvalidParams("wordness must be any, whole or part."),
    };

    // Echoed input is clipped so an error message never carries a huge payload.
    private static string Truncate(string text) => text.Length <= 40 ? text : text[..40] + "…";
}
