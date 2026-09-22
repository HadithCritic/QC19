namespace QuranCode.Core.Text;

/// <summary>
/// How the text is counted: the Statistics-panel options of the original.
/// </summary>
/// <remarks>
/// <para>
/// Each option mirrors a flag of the legacy <c>Server.BuildSimplifiedBook</c>
/// and defaults to the value the original starts with. Values are captured in
/// <c>tests/golden/counting-options.tsv</c> for every combination tested.
/// </para>
/// <para>
/// The original enables the text options in every text mode except Original,
/// and hamza above a line also not in Simplified28 or Simplified30, whose
/// letter sets have no hamza (<c>MainForm</c>, where the check boxes are
/// enabled). <see cref="For"/> drops an option a text mode does not allow, so a
/// saved preference never silently changes a count it should not affect.
/// </para>
/// </remarks>
public sealed record CountingOptions
{
    public static readonly CountingOptions Default = new();

    /// <summary>
    /// Count the Bismillah. In an edition with verse 0 it selects whether those
    /// verses count at all; in the classic edition it strips the Bismillah from
    /// the start of verse 1, as the original's option does.
    /// </summary>
    public bool IncludeBasmalas { get; init; } = true;

    /// <summary>A leading و is its own word, except in words where it belongs to the root.</summary>
    public bool WawAsWord { get; init; }

    /// <summary>A shadda counts as a second copy of the letter it sits on.</summary>
    public bool ShaddaAsLetter { get; init; }

    /// <summary>Hamza above a horizontal line (ـٔ) counts as the letter ء.</summary>
    public bool HamzaAboveLine { get; init; }

    /// <summary>Alif above a horizontal line (ـٰ and ٰ) counts as the letter ا.</summary>
    public bool ElfAboveLine { get; init; }

    /// <summary>Yaa above a horizontal line (ـۧ) counts as the letter ي.</summary>
    public bool YaaAboveLine { get; init; }

    /// <summary>Noon above a horizontal line (ـۨ) counts as the letter ن.</summary>
    public bool NoonAboveLine { get; init; }

    /// <summary>Whether a text mode allows the text options at all.</summary>
    public static bool AllowsTextOptions(string textMode) => textMode != "Original";

    /// <summary>Whether a text mode allows hamza above a line.</summary>
    public static bool AllowsHamzaAboveLine(string textMode) =>
        AllowsTextOptions(textMode) && textMode is not ("Simplified28" or "Simplified30");

    /// <summary>These options with anything the text mode does not allow switched off.</summary>
    /// <remarks>
    /// In the classic edition the original forces the Bismillah on in the
    /// Original text mode. An edition with verse 0 treats it as a choice of
    /// which verses count, which every text mode allows.
    /// </remarks>
    public CountingOptions For(string textMode, bool basmalaIsVerseZero)
    {
        if (AllowsTextOptions(textMode))
        {
            return AllowsHamzaAboveLine(textMode) ? this : this with { HamzaAboveLine = false };
        }

        return Default with { IncludeBasmalas = basmalaIsVerseZero ? IncludeBasmalas : true };
    }

    /// <summary>Whether any option rewrites the text before the text mode's rules.</summary>
    public bool ChangesText(bool basmalaIsVerseZero) =>
        WawAsWord || ShaddaAsLetter || HamzaAboveLine || ElfAboveLine || YaaAboveLine || NoonAboveLine
        || (!IncludeBasmalas && !basmalaIsVerseZero);
}
