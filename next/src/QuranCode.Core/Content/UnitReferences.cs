using System.Text.RegularExpressions;

namespace QuranCode.Core.Content;

/// <summary>A numbered division of the text: a page, part, station and so on.</summary>
public readonly record struct Partition(string Kind, int Number, int FirstVerse, int LastVerse);

/// <summary>
/// Parses references by unit rather than by chapter: <c>page 10</c>,
/// <c>part 3-4</c>, <c>verse 262</c>, <c>word 100</c>, <c>letter 500</c>.
/// </summary>
/// <remarks>
/// Features.txt #61: direct page, station, part, group, half, quarter, bowing,
/// verse, word and letter entry. The original has one box per unit; here the
/// unit is a keyword, English or the common Arabic-derived name (juz, hizb,
/// manzil, rub, ruku). Word and letter numbers depend on how the text is
/// counted, so the caller supplies the lookup for the current options.
/// </remarks>
public static partial class UnitReferences
{
    private static readonly Dictionary<string, string> Kinds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["page"] = "page", ["p"] = "page",
        ["station"] = "station", ["manzil"] = "station",
        ["part"] = "part", ["juz"] = "part",
        ["group"] = "group", ["hizb"] = "group",
        ["half"] = "half",
        ["quarter"] = "quarter", ["rub"] = "quarter",
        ["bowing"] = "bowing", ["ruku"] = "bowing",
        ["verse"] = "verse", ["v"] = "verse",
        ["word"] = "word", ["w"] = "word",
        ["letter"] = "letter", ["l"] = "letter",
    };

    /// <summary>The unit names accepted, for help text.</summary>
    public static IReadOnlyCollection<string> Units { get; } =
        ["page", "station", "part", "group", "half", "quarter", "bowing", "verse", "word", "letter"];

    [GeneratedRegex(@"^\s*([A-Za-z]+)\s*(\d{1,7})(?:\s*-\s*(\d{1,7}))?\s*$")]
    private static partial Regex Pattern();

    /// <summary>Whether the text starts with a unit keyword, so it is not a chapter reference.</summary>
    public static bool LooksLikeUnit(string text)
    {
        Match match = Pattern().Match(text);
        return match.Success && Kinds.ContainsKey(match.Groups[1].Value);
    }

    /// <param name="partitions">Divisions by kind, each ordered by number.</param>
    /// <param name="verseCount">Verse rows in the edition.</param>
    /// <param name="verseOfWord">Absolute verse of 1-based word N under the current counting, or null.</param>
    /// <param name="verseOfLetter">Absolute verse of 1-based letter N under the current counting, or null.</param>
    public static ReferenceParseResult Parse(
        string text,
        IReadOnlyDictionary<string, Partition[]> partitions,
        int verseCount,
        Func<int, int?> verseOfWord,
        Func<int, int?> verseOfLetter)
    {
        Match match = Pattern().Match(text ?? "");
        if (!match.Success || !Kinds.TryGetValue(match.Groups[1].Value, out string? kind))
        {
            return ReferenceParseResult.Fail($"\"{text?.Trim()}\" is not a unit reference. Try page 10, part 3-4 or word 100.");
        }

        int from = int.Parse(match.Groups[2].Value);
        int to = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : from;
        if (to < from) return ReferenceParseResult.Fail("The end of the range comes before its start.");

        switch (kind)
        {
            case "verse":
                if (from < 1 || to > verseCount)
                {
                    return ReferenceParseResult.Fail($"Verses run from 1 to {verseCount}.");
                }
                return ReferenceParseResult.Ok(new VerseRange(from, to));

            case "word":
            case "letter":
                Func<int, int?> lookup = kind == "word" ? verseOfWord : verseOfLetter;
                if (lookup(from) is not int first || lookup(to) is not int last)
                {
                    return ReferenceParseResult.Fail($"There is no {kind} {(lookup(from) is null ? from : to)} in the text as it is counted.");
                }
                return ReferenceParseResult.Ok(new VerseRange(first, last));

            default:
                Partition[] list = partitions.TryGetValue(kind, out Partition[]? found) ? found : [];
                if (from < 1 || to > list.Length)
                {
                    return ReferenceParseResult.Fail($"There are {list.Length} {kind}s; there is no {kind} {(from < 1 ? from : to)}.");
                }
                return ReferenceParseResult.Ok(new VerseRange(list[from - 1].FirstVerse, list[to - 1].LastVerse));
        }
    }
}
