using System.Globalization;
using System.Text.RegularExpressions;

namespace QuranCode.Core.Content;

/// <summary>The outcome of parsing an address: a selection, or a reason it is not one.</summary>
public readonly record struct SelectionParseResult(QuranSelection? Selection, string? Error)
{
    public bool IsSuccess => Error is null;

    internal static SelectionParseResult Ok(QuranSelection selection) => new(selection, null);

    internal static SelectionParseResult Fail(string error) => new(null, error);
}

/// <summary>
/// Exact addresses a researcher can type, copy and save: <c>2</c>,
/// <c>2:255</c>, <c>2:255:w4</c>, <c>2:255:w4:l2</c>, and a range of two
/// of them joined by <c>-</c>.
/// </summary>
/// <remarks>
/// <para>
/// Syntax only: whether chapter 2 has a verse 300 is for the resolver, which
/// knows the edition. Arabic-Indic and Persian digits are accepted.
/// </para>
/// <para>
/// Each end of a range is written in full. The one shorthand allowed is a
/// chapter range such as <c>2-5</c>. Anything shorter after a finer start, as in
/// <c>2:255:w4-5</c>, could mean word 5 or chapter 5, so it is refused rather
/// than guessed at; the verse-range shorthand <c>2:255-257</c> stays with
/// <see cref="ReferenceParser"/>.
/// </para>
/// </remarks>
public static partial class SelectionAddress
{
    private const string Example = "write it as 2, 2:255, 2:255:w4 or 2:255:w4:l2";

    public static SelectionParseResult Parse(string? text)
    {
        string input = ReferenceParser.NormalizeDigits(text ?? "").Replace(" ", "", StringComparison.Ordinal);
        if (input.Length == 0) return SelectionParseResult.Fail("The address is empty.");

        string[] ends = input.Split('-');
        if (ends.Length > 2) return FormatError(text!);

        if (ParseLocation(ends[0]) is not QuranLocation start) return FormatError(text!);
        if (ends.Length == 1) return SelectionParseResult.Ok(new QuranSelection(start, start));

        if (ParseLocation(ends[1]) is not QuranLocation end) return FormatError(text!);
        if (end.Level == SelectionLevel.Chapter && start.Level != SelectionLevel.Chapter)
        {
            return SelectionParseResult.Fail(
                $"Write the end of \"{Clip(text!)}\" in full, as in 2:255:w4-2:255:w8, so it cannot be misread.");
        }
        return SelectionParseResult.Ok(new QuranSelection(start, end));
    }

    /// <summary>The canonical address of a location.</summary>
    public static string Format(QuranLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        string text = location.Chapter.ToString(CultureInfo.InvariantCulture);
        if (location.Verse is int verse) text += $":{verse.ToString(CultureInfo.InvariantCulture)}";
        if (location.Word is int word) text += $":w{word.ToString(CultureInfo.InvariantCulture)}";
        if (location.Letter is int letter) text += $":l{letter.ToString(CultureInfo.InvariantCulture)}";
        return text;
    }

    /// <summary>The canonical address of a selection: one location, or two joined by <c>-</c>.</summary>
    public static string Format(QuranSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return selection.Start == selection.End
            ? Format(selection.Start)
            : $"{Format(selection.Start)}-{Format(selection.End)}";
    }

    private static QuranLocation? ParseLocation(string text)
    {
        Match match = Location().Match(text);
        if (!match.Success) return null;

        int? Number(string group) =>
            match.Groups[group].Success ? int.Parse(match.Groups[group].Value, CultureInfo.InvariantCulture) : null;

        var location = new QuranLocation(Number("c")!.Value, Number("v"), Number("w"), Number("l"));
        bool positive = location.Chapter >= 1 && (location.Word is null or >= 1) && (location.Letter is null or >= 1);
        return positive ? location : null;
    }

    // Verse 0 is a real verse in the Submission edition; words and letters start at 1.
    // Numbers are capped at six digits so parsing can never overflow.
    [GeneratedRegex(@"^(?<c>[0-9]{1,6})(?::(?<v>[0-9]{1,6})(?::[wW](?<w>[0-9]{1,6})(?::[lL](?<l>[0-9]{1,6}))?)?)?$", RegexOptions.CultureInvariant)]
    private static partial Regex Location();

    private static SelectionParseResult FormatError(string text) =>
        SelectionParseResult.Fail($"\"{Clip(text)}\" is not in a recognized format: {Example}.");

    private static string Clip(string text) => text.Length <= 40 ? text : text[..40] + "…";
}
