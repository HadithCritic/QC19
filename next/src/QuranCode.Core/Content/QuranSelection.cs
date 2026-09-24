namespace QuranCode.Core.Content;

/// <summary>How precise a location is.</summary>
public enum SelectionLevel
{
    Chapter,
    Verse,
    Word,
    Letter,
}

/// <summary>
/// A place in the Quran as the reader sees it, as precise as a single letter.
/// </summary>
/// <remarks>
/// <para>
/// These are display coordinates, independent of text mode, value system and
/// counting options, so an address stays on the same visible text whatever is
/// being counted. <see cref="Analysis.SelectionResolver"/> finds the counted
/// letters it stands for.
/// </para>
/// <para>
/// <paramref name="Verse"/> is the number in the chapter (0 for a verse-0
/// Bismillah). <paramref name="Word"/> is 1-based among the verse's display
/// words, leaving out the Bismillah header the classic edition prefixes to
/// verse 1. <paramref name="Letter"/> is 1-based among the word's letters, not
/// counting marks (see <see cref="Text.DisplayLetters"/>).
/// </para>
/// </remarks>
public sealed record QuranLocation(int Chapter, int? Verse = null, int? Word = null, int? Letter = null)
{
    public SelectionLevel Level =>
        Letter is not null ? SelectionLevel.Letter
        : Word is not null ? SelectionLevel.Word
        : Verse is not null ? SelectionLevel.Verse
        : SelectionLevel.Chapter;

    /// <summary>Whether every part it has is attached to the part above it.</summary>
    public bool IsWellFormed =>
        (Letter is null || Word is not null) && (Word is null || Verse is not null);

    /// <summary>Where the location begins, for ordering: a missing part sorts before any number.</summary>
    internal (int, int, int, int) Begin => (Chapter, Verse ?? -1, Word ?? -1, Letter ?? -1);
}

/// <summary>An inclusive, continuous selection from one location to another.</summary>
public sealed record QuranSelection(QuranLocation Start, QuranLocation End)
{
    /// <summary>The same selection with its endpoints in Quran order.</summary>
    /// <remarks>A reader may click the end first; that must never give an empty range.</remarks>
    public QuranSelection Ordered() =>
        End.Begin.CompareTo(Start.Begin) < 0 ? new QuranSelection(End, Start) : this;
}
