using QuranCode.Core.Content;

namespace QuranCode.Core.Analysis;

/// <summary>How many counted verses come before and after a selection.</summary>
/// <param name="BeforeInChapter">Counted verses of the first verse's chapter before the selection.</param>
/// <param name="AfterInChapter">Counted verses of the last verse's chapter after the selection.</param>
public readonly record struct SelectionPosition(int BeforeInChapter, int AfterInChapter, int BeforeInBook, int AfterInBook)
{
    public static readonly SelectionPosition None = new(0, 0, 0, 0);

    /// <summary>Positions of a view-index range within its chapters and the book.</summary>
    public static SelectionPosition Of(
        Segmentation segmentation, CorpusView view, IReadOnlyList<Chapter> chapters, int firstIndex, int lastIndex)
    {
        Chapter firstChapter = chapters[segmentation.VerseChapter[firstIndex] - 1];
        Chapter lastChapter = chapters[segmentation.VerseChapter[lastIndex] - 1];
        (int First, int Last) firstRows = view.IndexRange(new VerseRange(firstChapter.FirstVerse, firstChapter.LastVerse))!.Value;
        (int First, int Last) lastRows = view.IndexRange(new VerseRange(lastChapter.FirstVerse, lastChapter.LastVerse))!.Value;

        return new SelectionPosition(
            firstIndex - firstRows.First,
            lastRows.Last - lastIndex,
            firstIndex,
            segmentation.VerseCount - 1 - lastIndex);
    }
}
