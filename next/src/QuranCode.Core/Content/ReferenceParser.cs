namespace QuranCode.Core.Content;

/// <summary>An inclusive run of verses by absolute number (1..6236 classic, 1..6346 Submission).</summary>
public readonly record struct VerseRange(int First, int Last)
{
    public int Count => Last - First + 1;

    public bool Contains(int verse) => verse >= First && verse <= Last;
}

/// <summary>The outcome of parsing a reference: a range, or a reason it is not one.</summary>
public readonly record struct ReferenceParseResult(VerseRange Range, string? Error)
{
    public bool IsSuccess => Error is null;

    internal static ReferenceParseResult Ok(VerseRange range) => new(range, null);

    internal static ReferenceParseResult Fail(string error) => new(default, error);
}

/// <summary>
/// Parses the references a reader types: <c>2</c>, <c>3-4</c>, <c>2:255</c>,
/// <c>2:255-257</c>, <c>3-4:19</c> and <c>24:35-27:62</c>.
/// </summary>
/// <remarks>
/// Features.txt #60. Arabic-Indic and Persian digits are accepted because an
/// Arabic keyboard produces them. Every failure carries a sentence the UI can
/// show as it is, so a bad reference is never a silent no-op.
/// </remarks>
public static class ReferenceParser
{
    public static ReferenceParseResult Parse(string? text, IReadOnlyList<Chapter> chapters)
    {
        ArgumentNullException.ThrowIfNull(chapters);

        string input = NormalizeDigits(text ?? "").Replace(" ", "", StringComparison.Ordinal);
        if (input.Length == 0) return ReferenceParseResult.Fail("The reference is empty.");

        string[] ends = input.Split('-');
        if (ends.Length > 2) return FormatError(text!);

        if (!TryParsePoint(ends[0], chapters, isEnd: false, out int first, out string? error))
        {
            return ReferenceParseResult.Fail(error!);
        }

        int last;
        if (ends.Length == 1)
        {
            if (!TryParsePoint(ends[0], chapters, isEnd: true, out last, out error))
            {
                return ReferenceParseResult.Fail(error!);
            }
        }
        else if (ends[1].Contains(':'))
        {
            if (!TryParsePoint(ends[1], chapters, isEnd: true, out last, out error))
            {
                return ReferenceParseResult.Fail(error!);
            }
        }
        else if (!ends[0].Contains(':'))
        {
            // "3-4": whole chapters.
            if (!TryParsePoint(ends[1], chapters, isEnd: true, out last, out error))
            {
                return ReferenceParseResult.Fail(error!);
            }
        }
        else
        {
            // "2:255-257": the end is a verse in the same chapter.
            string chapterPart = ends[0][..ends[0].IndexOf(':')];
            if (!TryParsePoint($"{chapterPart}:{ends[1]}", chapters, isEnd: true, out last, out error))
            {
                return ReferenceParseResult.Fail(error!);
            }
        }

        if (last < first) return ReferenceParseResult.Fail("The end of the range comes before its start.");
        return ReferenceParseResult.Ok(new VerseRange(first, last));
    }

    private static bool TryParsePoint(
        string point, IReadOnlyList<Chapter> chapters, bool isEnd, out int verse, out string? error)
    {
        verse = 0;
        error = null;

        string[] parts = point.Split(':');
        if (parts.Length > 2 || !parts.All(IsNumber))
        {
            error = $"\"{point}\" is not in the format chapter or chapter:verse.";
            return false;
        }

        int chapterNumber = int.Parse(parts[0]);
        if (chapterNumber < 1 || chapterNumber > chapters.Count)
        {
            error = $"There is no chapter {chapterNumber}; chapters run from 1 to {chapters.Count}.";
            return false;
        }

        Chapter chapter = chapters[chapterNumber - 1];
        if (parts.Length == 1)
        {
            verse = isEnd ? chapter.LastVerse : chapter.FirstVerse;
            return true;
        }

        int inChapter = int.Parse(parts[1]);
        if (inChapter < chapter.FirstNumberInChapter || inChapter > chapter.VerseCount)
        {
            error = $"Chapter {chapterNumber} has verses {chapter.FirstNumberInChapter} to {chapter.VerseCount}; there is no verse {inChapter}.";
            return false;
        }

        verse = chapter.AbsoluteOf(inChapter);
        return true;
    }

    // Bounded length keeps int.Parse from overflowing on pasted junk.
    private static bool IsNumber(string s) => s.Length is > 0 and <= 5 && s.All(char.IsAsciiDigit);

    private static ReferenceParseResult FormatError(string text) =>
        ReferenceParseResult.Fail($"\"{text.Trim()}\" is not a reference. Try 2, 2:255, 2:255-257 or 1:7-2:2.");

    private static string NormalizeDigits(string text) => string.Create(text.Length, text, static (span, source) =>
    {
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            span[i] = c switch
            {
                >= '٠' and <= '٩' => (char)('0' + (c - '٠')),
                >= '۰' and <= '۹' => (char)('0' + (c - '۰')),
                _ => c,
            };
        }
    });
}
