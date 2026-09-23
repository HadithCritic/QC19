using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Code19;

/// <summary>
/// Runs a finding's rule against the text and reports what it computes.
/// </summary>
/// <remarks>
/// Everything counts over the <see cref="Segmentation"/> of the finding's own
/// text mode and counting convention, so a finding that needs the unnumbered
/// Basmalahs and one that must leave them out can both be right at once.
/// </remarks>
public static class FindingEvaluator
{
    /// <summary>
    /// The join rule that counts the negation لا and the verb after it as one
    /// word, as the classical grammarians treat them. Whether the next word is
    /// a verb comes from the Quranic Arabic Corpus.
    /// </summary>
    public const string LaVerb = "la+verb";

    /// <summary>The counted words of one verse that fall in a scope: [First, End).</summary>
    private readonly record struct VerseWords(int Verse, int First, int End);

    /// <summary>Computes one finding.</summary>
    /// <exception cref="ArgumentException">The rule names a set, letter or scope that does not exist.</exception>
    public static FindingResult Evaluate(QuranCodeEngine engine, Finding finding)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(finding);

        var counting = new CountingOptions { IncludeBasmalas = finding.IncludeBasmalas };
        Segmentation segmentation = engine.Segmentation(finding.TextMode, counting);
        VerseWords[] scope = ScopeOf(finding.Scope, segmentation);

        long computed = finding.Measure switch
        {
            FindingMeasure.Verses => scope.Length,
            FindingMeasure.Words => CountWords(engine, counting, segmentation, scope, finding.Match),
            FindingMeasure.Letters => scope.Sum(r => CountLetters(segmentation, r, _ => true)),
            FindingMeasure.LetterOccurrences => scope.Sum(r => CountLetters(segmentation, r, LettersOf(finding).Contains)),
            FindingMeasure.OwnInitials => CountOwnInitials(segmentation, scope, finding.Match),
            FindingMeasure.WordFormOccurrences => scope.Sum(r => CountForms(segmentation, r, SetOf(finding))),
            FindingMeasure.VerseNumberSum => scope
                .Where(r => CountForms(segmentation, r, SetOf(finding)) > 0)
                .Sum(r => (long)segmentation.VerseNumberInChapter[r.Verse]),
            _ => throw new ArgumentException($"unknown measure {finding.Measure}", nameof(finding)),
        };
        return new FindingResult(finding, computed);
    }

    /// <summary>Computes every finding, in the order given.</summary>
    public static IReadOnlyList<FindingResult> EvaluateAll(QuranCodeEngine engine, IEnumerable<Finding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        return [.. findings.Select(f => Evaluate(engine, f))];
    }

    private static IReadOnlySet<string> SetOf(Finding finding) =>
        WordForms.Named(finding.Match ?? "")
        ?? throw new ArgumentException($"no word-form set named \"{finding.Match}\"", nameof(finding));

    private static HashSet<char> LettersOf(Finding finding) =>
        finding.Match is { Length: > 0 } match
            ? [.. match]
            : throw new ArgumentException("a letter count needs at least one letter", nameof(finding));

    /// <summary>The scope as the counted words of each verse it covers, in order.</summary>
    private static VerseWords[] ScopeOf(FindingScope scope, Segmentation s)
    {
        var chapters = scope.Chapters is null ? null : new HashSet<int>(scope.Chapters);
        var result = new List<VerseWords>();
        for (int v = 0; v < s.VerseCount; v++)
        {
            if (chapters is not null && !chapters.Contains(s.VerseChapter[v])) continue;
            if (!scope.CoversVerse(s.VerseNumberInChapter[v])) continue;
            result.Add(new VerseWords(v, s.VerseFirstWord[v], s.VerseFirstWord[v] + s.VerseWordCount[v]));
        }
        if (result.Count == 0)
            throw new ArgumentException($"scope {scope} selects no verse in this edition", nameof(scope));

        if (scope.StopBefore is { } stop)
        {
            VerseWords last = result[^1];
            int end = last.First;
            while (end < last.End && !s.WordText(end).StartsWith(stop, StringComparison.Ordinal)) end++;
            if (end == last.End)
                throw new ArgumentException($"scope {scope}: no word in its last verse starts with {stop}", nameof(scope));
            result[^1] = last with { End = end };
        }
        return [.. result];
    }

    private static long CountWords(
        QuranCodeEngine engine, CountingOptions counting, Segmentation s, VerseWords[] scope, string? join)
    {
        long total = scope.Sum(r => (long)(r.End - r.First));
        if (string.IsNullOrWhiteSpace(join)) return total;
        if (join != LaVerb) throw new ArgumentException($"no join rule named \"{join}\"");

        CorpusView view = engine.View(counting);
        foreach (VerseWords r in scope)
        {
            int number = view.Verses[r.Verse].Number;
            int first = s.VerseFirstWord[r.Verse];
            for (int w = r.First; w + 1 < r.End; w++)
            {
                if (s.WordText(w) != "لا") continue;
                WordData? next = engine.WordDataOf(number, w + 1 - first);
                if (next is not null && next.Parts.Any(p => p.Tag == "V")) total--;
            }
        }
        return total;
    }

    private static long CountLetters(Segmentation s, VerseWords r, Func<char, bool> counts)
    {
        if (r.End <= r.First) return 0;
        int to = s.WordFirstLetter[r.End - 1] + s.WordLetterCount[r.End - 1];
        long total = 0;
        for (int i = s.WordFirstLetter[r.First]; i < to; i++)
            if (counts(s.LetterChars[i])) total++;
        return total;
    }

    private static long CountOwnInitials(Segmentation s, VerseWords[] scope, string? only)
    {
        var initials = QuranicInitials.Chapters.ToDictionary(c => c.Chapter, c => new HashSet<char>(c.Letters));
        if (!string.IsNullOrWhiteSpace(only))
            foreach (HashSet<char> letters in initials.Values) letters.IntersectWith(only);

        long total = 0;
        foreach (VerseWords r in scope)
            if (initials.TryGetValue(s.VerseChapter[r.Verse], out HashSet<char>? letters) && letters.Count > 0)
                total += CountLetters(s, r, letters.Contains);
        return total;
    }

    private static long CountForms(Segmentation s, VerseWords r, IReadOnlySet<string> forms)
    {
        long total = 0;
        for (int w = r.First; w < r.End; w++)
            if (forms.Contains(s.WordText(w))) total++;
        return total;
    }
}
