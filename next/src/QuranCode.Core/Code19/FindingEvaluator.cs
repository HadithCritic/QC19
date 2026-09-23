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
    /// <summary>Computes one finding.</summary>
    /// <exception cref="ArgumentException">The rule names a set, letter or scope that does not exist.</exception>
    public static FindingResult Evaluate(QuranCodeEngine engine, Finding finding)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(finding);

        var counting = new CountingOptions { IncludeBasmalas = finding.IncludeBasmalas };
        Segmentation segmentation = engine.Segmentation(finding.TextMode, counting);
        int[] verses = VersesOf(finding.Scope, segmentation);

        long computed = finding.Measure switch
        {
            FindingMeasure.Verses => verses.Length,
            FindingMeasure.Words => verses.Sum(v => (long)segmentation.VerseWordCount[v]),
            FindingMeasure.Letters => verses.Sum(v => LettersIn(segmentation, v, _ => true)),
            FindingMeasure.LetterOccurrences => CountLetters(segmentation, verses, LettersOf(finding)),
            FindingMeasure.WordFormOccurrences => verses.Sum(v => FormsIn(segmentation, v, SetOf(finding))),
            FindingMeasure.VerseNumberSum => SumVerseNumbers(segmentation, verses, SetOf(finding)),
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

    /// <summary>The scope as view indexes of the verses it covers, in order.</summary>
    private static int[] VersesOf(FindingScope scope, Segmentation segmentation)
    {
        if (scope.Chapters is null) return [.. Enumerable.Range(0, segmentation.VerseCount)];

        var chapters = new HashSet<int>(scope.Chapters);
        var verses = new List<int>();
        for (int v = 0; v < segmentation.VerseCount; v++)
        {
            if (!chapters.Contains(segmentation.VerseChapter[v])) continue;
            if (!scope.CoversVerse(segmentation.VerseNumberInChapter[v])) continue;
            verses.Add(v);
        }
        if (verses.Count == 0)
            throw new ArgumentException($"scope {scope} selects no verse in this edition", nameof(scope));
        return [.. verses];
    }

    private static long LettersIn(Segmentation s, int verse, Func<char, bool> counts)
    {
        int firstWord = s.VerseFirstWord[verse];
        int lastWord = firstWord + s.VerseWordCount[verse] - 1;
        if (lastWord < firstWord) return 0;
        int to = s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord];
        long total = 0;
        for (int i = s.WordFirstLetter[firstWord]; i < to; i++)
            if (counts(s.LetterChars[i])) total++;
        return total;
    }

    private static long CountLetters(Segmentation s, int[] verses, HashSet<char> letters) =>
        verses.Sum(v => LettersIn(s, v, letters.Contains));

    private static long FormsIn(Segmentation s, int verse, IReadOnlySet<string> forms)
    {
        long total = 0;
        int start = s.VerseFirstWord[verse];
        for (int w = start; w < start + s.VerseWordCount[verse]; w++)
            if (forms.Contains(s.WordText(w))) total++;
        return total;
    }

    private static long SumVerseNumbers(Segmentation s, int[] verses, IReadOnlySet<string> forms) =>
        verses.Where(v => FormsIn(s, v, forms) > 0).Sum(v => (long)s.VerseNumberInChapter[v]);
}
