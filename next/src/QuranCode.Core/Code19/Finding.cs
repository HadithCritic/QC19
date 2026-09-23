namespace QuranCode.Core.Code19;

/// <summary>What a finding counts.</summary>
public enum FindingMeasure
{
    /// <summary>Counted words in the scope.</summary>
    Words,

    /// <summary>Counted letters in the scope.</summary>
    Letters,

    /// <summary>
    /// Occurrences in the scope of the letters in <see cref="Finding.Match"/>,
    /// added together: one letter, or several such as حم.
    /// </summary>
    LetterOccurrences,

    /// <summary>Words in the scope whose normalized form is in a named set.</summary>
    WordFormOccurrences,

    /// <summary>
    /// Sum of the in-chapter verse numbers of the verses that hold at least
    /// one word of a named set. A verse counts once however many it holds.
    /// </summary>
    VerseNumberSum,
}

/// <summary>
/// Whether the source gave the counting rule or it was derived. ADR 0004 §7
/// requires an inferred rule to say so wherever the finding is shown, because
/// the figure then rests on an assumption.
/// </summary>
public enum RuleBasis
{
    Stated,
    Inferred,
}

/// <summary>
/// What a finding counts over: the whole book, one or more chapters, or a
/// verse. Several chapters need not be contiguous: the three chapters
/// initialed with ص are 7, 19 and 38.
/// </summary>
public sealed record FindingScope(IReadOnlyList<int>? Chapters = null, int? Verse = null)
{
    public static readonly FindingScope Book = new();

    public FindingScope(int chapter, int? verse = null)
        : this([chapter], verse)
    {
    }

    public override string ToString() => Chapters switch
    {
        null => "book",
        [int one] when Verse is not null => $"{one}:{Verse}",
        [int one] => $"chapter {one}",
        _ => $"chapters {string.Join(", ", Chapters)}",
    };
}

/// <summary>Whether a finding must reproduce for the build to pass.</summary>
public enum FindingCheck
{
    /// <summary>It reproduces, and a change that breaks it fails the build.</summary>
    Gate,

    /// <summary>
    /// A known discrepancy: the published number and the computed one differ
    /// and the reason is not settled. It is shown with the gap, never hidden
    /// and never edited to agree, and it does not fail the build.
    /// </summary>
    Open,
}

/// <summary>
/// A published claim with everything needed to check it: the number, the rule
/// as something the engine can run, where the rule came from, and the counting
/// convention it holds under.
/// </summary>
/// <param name="Id">Stable key, used by the tests and by the interface.</param>
/// <param name="Claim">The claim in one line, as published.</param>
/// <param name="Expected">The published number.</param>
/// <param name="Match">A letter for <see cref="FindingMeasure.LetterOccurrences"/>, or a form-set name.</param>
/// <param name="IncludeBasmalas">
/// The convention this finding was computed under. ADR 0004 §7: it belongs to
/// the finding, never to a global setting, because the results disagree — the
/// count of the word God excludes the 112 unnumbered Basmalahs and others
/// require them.
/// </param>
/// <param name="Rule">The counting rule in words, for a reader.</param>
/// <param name="Source">Appendix or book, with a table or page where there is one.</param>
public sealed record Finding(
    string Id,
    string Claim,
    long Expected,
    FindingMeasure Measure,
    FindingScope Scope,
    string? Match,
    bool IncludeBasmalas,
    string TextMode,
    RuleBasis Basis,
    string Rule,
    string Source,
    FindingCheck Check = FindingCheck.Gate);

/// <summary>A finding and what the engine computes for it.</summary>
public sealed record FindingResult(Finding Finding, long Computed)
{
    /// <summary>The computed number is the published one.</summary>
    public bool Holds => Computed == Finding.Expected;

    /// <summary>The computed number is a multiple of 19.</summary>
    public bool MultipleOf19 => Computed != 0 && Computed % 19 == 0;

    /// <summary>The multiplier when it is a multiple of 19, else null.</summary>
    public long? Multiple => MultipleOf19 ? Computed / 19 : null;
}
