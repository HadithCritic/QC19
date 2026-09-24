using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Analysis;

/// <summary>
/// The counted text a selection covers: inclusive indexes into a
/// <see cref="Segmentation"/>, of view verses, words and letters.
/// </summary>
/// <param name="IsVerseAligned">
/// It starts at a verse's first counted letter and ends at a verse's last, so
/// verse-range analysis, with every legacy path, applies to it unchanged.
/// </param>
public sealed record CountedSpan(
    int FirstChapter, int LastChapter,
    int FirstVerse, int LastVerse,
    int FirstWord, int LastWord,
    int FirstLetter, int LastLetter,
    bool IsVerseAligned)
{
    public int ChapterCount => LastChapter - FirstChapter + 1;
    public int VerseCount => LastVerse - FirstVerse + 1;
    public int WordCount => LastWord - FirstWord + 1;
    public int LetterCount => LastLetter - FirstLetter + 1;
}

/// <summary>
/// A selection resolved against the counted text, or the reason it cannot be.
/// </summary>
/// <param name="Selection">The selection with its endpoints in Quran order, as the reader gave them.</param>
/// <param name="Span">What is counted; null when nothing in the selection is counted under the options.</param>
/// <param name="Notes">Sentences for the reader about what the options leave out.</param>
public sealed record SelectionResolution(
    QuranSelection Selection, CountedSpan? Span, IReadOnlyList<string> Notes, string? Error)
{
    public bool IsSuccess => Error is null;
}

/// <summary>
/// Turns a selection in display coordinates into the counted letters it covers.
/// </summary>
/// <remarks>
/// <para>
/// A start resolves to the first counted letter at or after it, an end to the
/// last counted letter at or before it. That one rule covers every level:
/// whole chapters and verses, words through <see cref="DisplayWords.Align"/>,
/// letters through <see cref="DisplayLetters.Map"/>, and verses the options
/// leave out, which are skipped inward without changing the address.
/// </para>
/// <para>
/// Only the endpoint verses are aligned, so resolving costs the same for a
/// word as for the whole book. See <c>docs/specs/research-selection.md</c>.
/// </para>
/// </remarks>
public sealed class SelectionResolver
{
    private readonly IReadOnlyList<Chapter> _chapters;
    private readonly IReadOnlyList<Verse> _verses;
    private readonly CorpusView _view;
    private readonly Segmentation _segmentation;
    private readonly Func<Verse, VerseDisplay> _display;
    private readonly Func<string, string> _normalizeWord;
    private readonly string _textMode;

    /// <param name="verses">Every verse row, in canonical order, excluded ones included.</param>
    /// <param name="segmentation">The counted text of <paramref name="view"/>.</param>
    /// <param name="normalizeWord">The normalization <paramref name="segmentation"/> used, for one display word.</param>
    public SelectionResolver(
        IReadOnlyList<Chapter> chapters, IReadOnlyList<Verse> verses, CorpusView view, Segmentation segmentation,
        Func<Verse, VerseDisplay> display, Func<string, string> normalizeWord, string textMode)
    {
        _chapters = chapters ?? throw new ArgumentNullException(nameof(chapters));
        _verses = verses ?? throw new ArgumentNullException(nameof(verses));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _segmentation = segmentation ?? throw new ArgumentNullException(nameof(segmentation));
        _display = display ?? throw new ArgumentNullException(nameof(display));
        _normalizeWord = normalizeWord ?? throw new ArgumentNullException(nameof(normalizeWord));
        _textMode = textMode ?? throw new ArgumentNullException(nameof(textMode));
    }

    public SelectionResolution Resolve(QuranSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        QuranSelection ordered = selection.Ordered();
        var notes = new List<string>();

        try
        {
            int first = StartLetter(ordered.Start, notes);
            int last = EndLetter(ordered.End, notes);
            if (first > last)
            {
                notes.Add("Nothing in this selection is counted under the current options.");
                return new SelectionResolution(ordered, null, notes.Distinct().ToArray(), null);
            }
            return new SelectionResolution(ordered, Span(first, last), notes.Distinct().ToArray(), null);
        }
        catch (UnresolvableException ex)
        {
            return new SelectionResolution(ordered, null, [], ex.Message);
        }
    }

    private CountedSpan Span(int firstLetter, int lastLetter)
    {
        Segmentation s = _segmentation;
        int firstWord = s.LetterWord[firstLetter], lastWord = s.LetterWord[lastLetter];
        int firstVerse = s.WordVerse[firstWord], lastVerse = s.WordVerse[lastWord];
        bool aligned = firstLetter == VerseStart(firstVerse) && lastLetter == VerseEnd(lastVerse);
        return new CountedSpan(
            s.VerseChapter[firstVerse], s.VerseChapter[lastVerse], firstVerse, lastVerse,
            firstWord, lastWord, firstLetter, lastLetter, aligned);
    }

    /// <summary>The first counted letter at or after a location.</summary>
    private int StartLetter(QuranLocation location, List<string> notes)
    {
        (Chapter chapter, int? absolute) = Validate(location);
        if (absolute is not int verse) return FirstLetterFrom(chapter.FirstVerse);
        if (location.Word is not int word) return FirstLetterFrom(verse, notes);

        int index = _view.IndexOf(verse);
        if (index < 0)
        {
            notes.Add(NotCounted(verse));
            return FirstLetterFrom(verse + 1);
        }
        return WordLetters(location, index, word - 1, notes).First;
    }

    /// <summary>The last counted letter at or before a location.</summary>
    private int EndLetter(QuranLocation location, List<string> notes)
    {
        (Chapter chapter, int? absolute) = Validate(location);
        if (absolute is not int verse) return LastLetterUpTo(chapter.LastVerse);
        if (location.Word is not int word) return LastLetterUpTo(verse, notes);

        int index = _view.IndexOf(verse);
        if (index < 0)
        {
            notes.Add(NotCounted(verse));
            return LastLetterUpTo(verse - 1);
        }
        return WordLetters(location, index, word - 1, notes).Last;
    }

    /// <summary>
    /// The counted letters a display word, or one of its letters, stands for.
    /// Last is First - 1 when a letter produces no counted letter.
    /// </summary>
    private (int First, int Last) WordLetters(QuranLocation location, int index, int displayWord, List<string> notes)
    {
        Segmentation s = _segmentation;
        Verse verse = _view.Verses[index];
        VerseDisplay display = _display(verse);

        DisplaySpan[] spans = DisplayWords.Align(display, s.VerseWords(index), _normalizeWord)
            ?? throw new UnresolvableException(
                $"The words of {Reference(verse)} cannot be matched to the counted text in {_textMode}; select the whole verse instead.");

        // Every counted word drawn from this display word: two with waw as a word,
        // and one shared with its neighbor when the two are counted as one word.
        int firstCounted = -1, lastCounted = -1;
        for (int c = 0; c < spans.Length; c++)
        {
            DisplaySpan span = spans[c];
            if (span.Count == 0 || displayWord < span.First || displayWord >= span.First + span.Count) continue;
            if (firstCounted < 0) firstCounted = c;
            lastCounted = c;
        }
        if (firstCounted < 0)
        {
            throw new UnresolvableException(
                $"Word {displayWord + 1} of {Reference(verse)} has no counted word in {_textMode}.");
        }

        int firstWord = s.VerseFirstWord[index] + firstCounted;
        int lastWord = s.VerseFirstWord[index] + lastCounted;
        int start = s.WordFirstLetter[firstWord];
        int end = s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord];
        // A whole display word that is a whole counted unit needs no letter map.
        DisplaySpan group = spans[firstCounted];
        if (location.Letter is null && group.Count == 1) return (start, end - 1);

        string[] words = display.Words.Skip(group.First).Take(group.Count).ToArray();
        string counted = new(s.LetterChars, start, end - start);
        int[][] ends = DisplayLetters.Map(words, counted, _normalizeWord)
            ?? throw new UnresolvableException(
                $"The letters of word {displayWord + 1} of {Reference(verse)} cannot be matched one by one to the counted text in {_textMode}; " +
                (location.Letter is null ? "select it with the words it is counted with." : "select the whole word instead."));

        // Letters of only this display word, even when it is counted as one word with its neighbor.
        int j = displayWord - group.First;
        int before, after;
        if (location.Letter is int l)
        {
            before = l > 1 ? ends[j][l - 2] : EndOfWordsBefore(ends, j);
            after = ends[j][l - 1];
            if (after == before)
            {
                notes.Add($"Letter {l} of word {displayWord + 1} of {Reference(verse)} is not a counted letter in {_textMode}.");
            }
        }
        else
        {
            before = EndOfWordsBefore(ends, j);
            after = ends[j].Length > 0 ? ends[j][^1] : before;
        }
        return (start + before, start + after - 1);
    }

    /// <summary>Counted letters of the group's words before word <paramref name="j"/>.</summary>
    private static int EndOfWordsBefore(int[][] ends, int j)
    {
        for (int w = j - 1; w >= 0; w--)
        {
            if (ends[w].Length > 0) return ends[w][^1];
        }
        return 0;
    }

    private int FirstLetterFrom(int verse, List<string> notes)
    {
        if (_view.IndexOf(verse) < 0) notes.Add(NotCounted(verse));
        return FirstLetterFrom(verse);
    }

    private int LastLetterUpTo(int verse, List<string> notes)
    {
        if (_view.IndexOf(verse) < 0) notes.Add(NotCounted(verse));
        return LastLetterUpTo(verse);
    }

    /// <summary>First counted letter of the first counted verse at or after an absolute verse.</summary>
    private int FirstLetterFrom(int verse)
    {
        for (int n = verse; n <= _verses.Count; n++)
        {
            int index = _view.IndexOf(n);
            if (index >= 0 && _segmentation.VerseWordCount[index] > 0) return VerseStart(index);
        }
        return _segmentation.LetterCount;
    }

    /// <summary>Last counted letter of the last counted verse at or before an absolute verse.</summary>
    private int LastLetterUpTo(int verse)
    {
        for (int n = verse; n >= 1; n--)
        {
            int index = _view.IndexOf(n);
            if (index >= 0 && _segmentation.VerseWordCount[index] > 0) return VerseEnd(index);
        }
        return -1;
    }

    private int VerseStart(int index) => _segmentation.WordFirstLetter[_segmentation.VerseFirstWord[index]];

    private int VerseEnd(int index)
    {
        int last = _segmentation.VerseFirstWord[index] + _segmentation.VerseWordCount[index] - 1;
        return _segmentation.WordFirstLetter[last] + _segmentation.WordLetterCount[last] - 1;
    }

    /// <summary>The location's chapter and absolute verse (null for a whole chapter), or why it does not exist.</summary>
    private (Chapter Chapter, int? Verse) Validate(QuranLocation location)
    {
        if (!location.IsWellFormed)
        {
            throw new UnresolvableException("A letter needs a word, and a word needs a verse.");
        }
        if (location.Chapter < 1 || location.Chapter > _chapters.Count)
        {
            throw new UnresolvableException($"Chapters run from 1 to {_chapters.Count}.");
        }

        Chapter chapter = _chapters[location.Chapter - 1];
        if (location.Verse is not int number) return (chapter, null);
        if (number < chapter.FirstNumberInChapter || number > chapter.VerseCount)
        {
            throw new UnresolvableException(
                $"Chapter {chapter.Number} has verses {chapter.FirstNumberInChapter} to {chapter.VerseCount}.");
        }

        int absolute = chapter.AbsoluteOf(number);
        if (location.Word is not int word) return (chapter, absolute);

        Verse verse = _verses[absolute - 1];
        IReadOnlyList<string> words = _display(verse).Words;
        if (word < 1 || word > words.Count)
        {
            throw new UnresolvableException($"{Reference(verse)} has words 1 to {words.Count}.");
        }

        if (location.Letter is int letter)
        {
            int letters = DisplayLetters.Count(words[word - 1]);
            if (letter < 1 || letter > letters)
            {
                throw new UnresolvableException($"Word {word} of {Reference(verse)} has letters 1 to {letters}.");
            }
        }
        return (chapter, absolute);
    }

    private string NotCounted(int absolute) =>
        $"{Reference(_verses[absolute - 1])} is not counted under the current options.";

    private static string Reference(Verse verse) => $"{verse.ChapterNumber}:{verse.NumberInChapter}";

    /// <summary>A selection that names something absent or unmappable; its message is for the reader.</summary>
    private sealed class UnresolvableException(string message) : Exception(message);
}
