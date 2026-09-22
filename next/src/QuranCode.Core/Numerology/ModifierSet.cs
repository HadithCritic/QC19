namespace QuranCode.Core.Numerology;

/// <summary>
/// The 21 <c>AddTo*</c> switches that add positions and distances to a value.
/// </summary>
/// <remarks>
/// <para>
/// In the legacy engine these are public fields on <c>NumericalSystem</c>,
/// mutated directly by checkboxes in <c>MainForm</c> and never written to disk:
/// <c>SaveNumericalSystem</c> persists only letter/value pairs. A saved value
/// system therefore does not record the modifiers that were active when a result
/// was produced, which is the reproducibility gap the brief describes in §20
/// and §38.
/// </para>
/// <para>
/// Each switch is gated by a master flag. <c>AddToLetterLNumber</c> does nothing
/// unless <c>AddPositions</c> is also set, and the distance switches do nothing
/// unless <c>AddDistancesToPrevious</c> is set. That gating is preserved here.
/// </para>
/// <para>
/// <b>A legacy quirk that is reproduced deliberately.</b> In
/// <c>Server.AdjustValue(Letter)</c>, the L-position line reads:
/// </para>
/// <code>
/// value = (AbsolutePositions) ? letter.NumberInWord : letter.NumberInWord;
/// </code>
/// <para>
/// Both branches are the same field, so <c>AbsolutePositions</c> has no effect
/// on the letter L position. The golden data confirms it: cases
/// <c>pos_letter_L</c> and <c>abs_letter_L</c> produce identical values. This is
/// almost certainly a typo in the original, but correcting it would change
/// published numbers, so it is preserved and recorded instead.
/// </para>
/// </remarks>
public sealed record ModifierSet
{
    /// <summary>No modifiers. The legacy default.</summary>
    public static readonly ModifierSet None = new();

    // Letter-level positions.
    public bool LetterLNumber { get; init; }
    public bool LetterWNumber { get; init; }
    public bool LetterVNumber { get; init; }
    public bool LetterCNumber { get; init; }

    // Letter-level distances to the previous identical letter.
    public bool LetterLDistance { get; init; }
    public bool LetterWDistance { get; init; }
    public bool LetterVDistance { get; init; }
    public bool LetterCDistance { get; init; }

    // Word-level positions.
    public bool WordWNumber { get; init; }
    public bool WordVNumber { get; init; }
    public bool WordCNumber { get; init; }

    // Word-level distances.
    public bool WordWDistance { get; init; }
    public bool WordVDistance { get; init; }
    public bool WordCDistance { get; init; }

    // Verse-level.
    public bool VerseVNumber { get; init; }
    public bool VerseCNumber { get; init; }
    public bool VerseVDistance { get; init; }
    public bool VerseCDistance { get; init; }

    // Chapter-level.
    public bool ChapterCNumber { get; init; }

    /// <summary>Whether any position switch is set.</summary>
    public bool AnyPosition =>
        LetterLNumber || LetterWNumber || LetterVNumber || LetterCNumber ||
        WordWNumber || WordVNumber || WordCNumber ||
        VerseVNumber || VerseCNumber || ChapterCNumber;

    /// <summary>Whether any distance switch is set.</summary>
    public bool AnyDistance =>
        LetterLDistance || LetterWDistance || LetterVDistance || LetterCDistance ||
        WordWDistance || WordVDistance || WordCDistance ||
        VerseVDistance || VerseCDistance;
}
