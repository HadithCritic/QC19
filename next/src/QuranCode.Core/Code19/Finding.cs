namespace QuranCode.Core.Code19;

/// <summary>What a finding counts.</summary>
public enum FindingMeasure
{
    /// <summary>Counted words in the scope.</summary>
    Words,

    /// <summary>Counted letters in the scope.</summary>
    Letters,

    /// <summary>Occurrences of one letter in the scope.</summary>
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

/// <summary>What a finding counts over: the whole book, a chapter, or a verse.</summary>
public sealed record FindingScope(int? Chapter = null, int? Verse = null)
{
    public static readonly FindingScope Book = new();

    public override string ToString() =>
        Chapter is null ? "book" : Verse is null ? $"chapter {Chapter}" : $"{Chapter}:{Verse}";
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
    string Source);

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
