using QuranCode.Core.Code19;
using QuranCode.Engine.Protocol;

namespace QuranCode.Engine.Host;

internal sealed partial class Handlers
{
    /// <summary>
    /// Every published Code 19 result with what the engine computes for it.
    /// Each carries its own counting convention and says whether its rule was
    /// stated by the source or inferred, as ADR 0004 requires.
    /// </summary>
    public IReadOnlyList<FindingDto> Findings() =>
    [
        .. FindingEvaluator.EvaluateAll(_engine, FindingCatalog.All).Select(r => new FindingDto(
            r.Finding.Id,
            r.Finding.Claim,
            r.Finding.Expected,
            r.Computed,
            r.Holds,
            r.MultipleOf19,
            r.Multiple,
            Name(r.Finding.Measure),
            r.Finding.Scope.ToString(),
            r.Finding.TextMode,
            r.Finding.Basis == RuleBasis.Inferred ? "inferred" : "stated",
            r.Finding.Rule,
            Convention(r.Finding),
            r.Finding.Source,
            r.Finding.Check == FindingCheck.Open ? "open" : "gate")),
    ];

    /// <summary>
    /// The 29 initialed chapters, each with how often its own initials occur
    /// in it (Features.txt #76). The counts are computed, not stated; a
    /// published figure for one of them belongs in the findings catalog.
    /// </summary>
    public IReadOnlyList<InitialedChapterDto> Initials() =>
    [
        .. QuranicInitials.Chapters.Select(chapter => new InitialedChapterDto(
            chapter.Chapter,
            _engine.Chapters[chapter.Chapter - 1].Name,
            chapter.Letters,
            chapter.Verses,
            [
                .. QuranicInitials.CountsIn(_engine, chapter).Select(c =>
                    new InitialCountDto(c.Letter.ToString(), c.Count, c.Count % 19 == 0)),
            ])),
    ];

    private static string Name(FindingMeasure measure) => measure switch
    {
        FindingMeasure.Words => "words",
        FindingMeasure.Letters => "letters",
        FindingMeasure.LetterOccurrences => "letterOccurrences",
        FindingMeasure.WordFormOccurrences => "wordFormOccurrences",
        FindingMeasure.VerseNumberSum => "verseNumberSum",
        _ => measure.ToString(),
    };

    private static string Convention(Finding finding) =>
        finding.IncludeBasmalas
            ? "counting the 112 unnumbered Basmalahs"
            : "numbered verses only";
}
