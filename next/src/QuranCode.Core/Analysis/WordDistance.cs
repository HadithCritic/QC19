using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Analysis;

/// <summary>A word as the reader sees it: a verse and a 0-based display word within it.</summary>
public readonly record struct WordLocation(int Verse, int DisplayWord);

/// <summary>A word's place in the counted text: its chapter, view verse, word and first letter indexes.</summary>
public readonly record struct CountedWord(int Chapter, int VerseIndex, int WordIndex, int LetterIndex);

/// <summary>
/// How far apart two words are, in chapters, verses, words and letters:
/// Features.txt #63, "distances on text clicks".
/// </summary>
public readonly record struct WordDistance(int Chapters, int Verses, int Words, int Letters)
{
    public static WordDistance Between(CountedWord a, CountedWord b) => new(
        Math.Abs(b.Chapter - a.Chapter),
        Math.Abs(b.VerseIndex - a.VerseIndex),
        Math.Abs(b.WordIndex - a.WordIndex),
        Math.Abs(b.LetterIndex - a.LetterIndex));

    /// <summary>
    /// Finds a display word in the counted text, or null when its verse is not
    /// counted or its words cannot be aligned.
    /// </summary>
    public static CountedWord? Locate(
        WordLocation location, Verse verse, VerseDisplay display, Segmentation segmentation,
        CorpusView view, Func<string, string> normalizeWord)
    {
        int index = view.IndexOf(location.Verse);
        if (index < 0) return null;

        DisplaySpan[]? spans = DisplayWords.Align(display, segmentation.VerseWords(index), normalizeWord);
        if (spans is null) return null;

        for (int s = 0; s < spans.Length; s++)
        {
            DisplaySpan span = spans[s];
            if (span.Count > 0 && location.DisplayWord >= span.First && location.DisplayWord < span.First + span.Count)
            {
                int word = segmentation.VerseFirstWord[index] + s;
                return new CountedWord(verse.ChapterNumber, index, word, segmentation.WordFirstLetter[word]);
            }
        }
        return null;
    }
}
