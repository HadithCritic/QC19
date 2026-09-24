using QuranCode.Core;
using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Text;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

/// <summary>
/// Exact selections over the protocol: the analysis, and the scope every
/// range method takes when it is given a selection instead of verses.
/// </summary>
internal sealed partial class Handlers
{
    public SelectionAnalysisDto AnalyzeSelection(AnalyzeParams p)
    {
        ValueSystemSummary system = RequireSystem(p.ValueSystem);
        CountingOptions counting = Counting(p.Counting);
        SelectionAnalysis analysis = _engine.Analyze(ToSelection(p.Selection), system.Name, counting);
        SelectionResolution resolution = analysis.Resolution;
        if (!resolution.IsSuccess) throw RpcException.InvalidParams(resolution.Error!);

        var methodology = new MethodologyDto(
            _engine.Corpus.Edition, system.TextMode, system.Name, ToDto(_engine.Effective(system.TextMode, counting)));
        SelectionStatistics? s = analysis.Statistics;
        var zero = Number(0);

        return new SelectionAnalysisDto(
            SelectionAddress.Format(resolution.Selection),
            ToDto(resolution.Selection),
            s?.Range.First,
            s?.Range.Last,
            resolution.Span?.IsVerseAligned ?? false,
            s is null ? null : new PositionDto(s.Position.BeforeInChapter, s.Position.AfterInChapter, s.Position.BeforeInBook, s.Position.AfterInBook),
            s is null ? zero : Number(s.ChapterCount),
            s is null ? zero : Number(s.VerseCount),
            s is null ? zero : Number(s.WordCount),
            s is null ? zero : Number(s.LetterCount),
            Number(analysis.DistinctWords),
            s is null ? zero : Number(s.DistinctLetterCount),
            s is null ? zero : Number(s.Value),
            s?.LetterFrequencies.Select(f => new LetterCountDto(f.Letter.ToString(), f.Count)).ToArray() ?? [],
            ToDto(analysis.Start),
            ToDto(analysis.End),
            analysis is { Start: CountedPosition a, End: CountedPosition b }
                ? new DeltaDto(b.Chapter - a.Chapter, b.AbsoluteVerse - a.AbsoluteVerse, b.AbsoluteWord - a.AbsoluteWord, b.AbsoluteLetter - a.AbsoluteLetter)
                : null,
            methodology,
            resolution.Notes);
    }

    /// <summary>
    /// What a range method runs over. A selection of whole verses becomes those
    /// verses, so it gets exactly the verse-range result; only a selection
    /// that cuts a word or verse becomes a span. Neither means nothing in it is
    /// counted under the options.
    /// </summary>
    private readonly record struct RangeScope(VerseRange? Range, CountedSpan? Span)
    {
        public bool IsEmpty => Range is null && Span is null;
    }

    private RangeScope ScopeOf(int first, int last, SelectionDto? selection, string valueSystem, CountingOptions counting)
    {
        if (selection is null) return new RangeScope(RequireRange(first, last), null);
        if (first != 0 || last != 0) throw RpcException.InvalidParams("Give first and last, or a selection, not both.");

        SelectionResolution resolution = _engine.ResolveFor(ToSelection(selection), valueSystem, counting);
        if (!resolution.IsSuccess) throw RpcException.InvalidParams(resolution.Error!);
        if (resolution.Span is not CountedSpan span) return default;

        return span.IsVerseAligned
            ? new RangeScope(_engine.VersesOf(span, valueSystem, counting), null)
            : new RangeScope(null, span);
    }

    /// <summary>The verses a scope touches, for the methods that work on whole verses only.</summary>
    private VerseRange? VersesOf(RangeScope scope, string valueSystem, CountingOptions counting) =>
        scope.Span is CountedSpan span ? _engine.VersesOf(span, valueSystem, counting) : scope.Range;

    /// <summary>Absolute verses a selection names, counted or not; only for a selection already resolved.</summary>
    private VerseRange Envelope(QuranSelection selection)
    {
        QuranSelection ordered = selection.Ordered();
        Chapter first = _engine.Chapters[ordered.Start.Chapter - 1];
        Chapter last = _engine.Chapters[ordered.End.Chapter - 1];
        return new VerseRange(
            ordered.Start.Verse is int v ? first.AbsoluteOf(v) : first.FirstVerse,
            ordered.End.Verse is int w ? last.AbsoluteOf(w) : last.LastVerse);
    }

    private static QuranSelection ToSelection(SelectionDto dto) =>
        new(ToLocation(dto.Start), ToLocation(dto.End));

    private static QuranLocation ToLocation(LocationDto dto) => new(dto.Chapter, dto.Verse, dto.Word, dto.Letter);

    private static SelectionDto ToDto(QuranSelection selection) => new(ToDto(selection.Start), ToDto(selection.End));

    private static LocationDto ToDto(QuranLocation location) =>
        new(location.Chapter, location.Verse, location.Word, location.Letter);

    private static CountedPositionDto? ToDto(CountedPosition? p) => p is null ? null : new CountedPositionDto(
        p.Chapter, p.Verse, p.AbsoluteVerse, p.WordInVerse, p.WordInChapter, p.AbsoluteWord,
        p.LetterInWord, p.LetterInVerse, p.LetterInChapter, p.AbsoluteLetter);

    private static CountingDto ToDto(CountingOptions o) => new(
        o.IncludeBasmalas, o.WawAsWord, o.ShaddaAsLetter, o.HamzaAboveLine, o.ElfAboveLine, o.YaaAboveLine, o.NoonAboveLine);
}
