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
        CorpusView view = engine.View(counting);
        (int first, int last) = VerseRangeOf(finding.Scope, view, segmentation);

        long computed = finding.Measure switch
        {
            FindingMeasure.Words => CountWords(segmentation, first, last),
            FindingMeasure.Letters => CountLetters(segmentation, first, last),
            FindingMeasure.LetterOccurrences => CountLetter(segmentation, first, last, LetterOf(finding)),
            FindingMeasure.WordFormOccurrences => CountForms(segmentation, first, last, SetOf(finding)),
            FindingMeasure.VerseNumberSum => SumVerseNumbers(segmentation, first, last, SetOf(finding)),
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

    private static char LetterOf(Finding finding) =>
        finding.Match is { Length: 1 } match
            ? match[0]
            : throw new ArgumentException($"\"{finding.Match}\" is not a single letter", nameof(finding));

    /// <summary>The scope as an inclusive range of view indexes.</summary>
    private static (int First, int Last) VerseRangeOf(FindingScope scope, CorpusView view, Segmentation segmentation)
    {
        if (scope.Chapter is null) return (0, segmentation.VerseCount - 1);

        var indexes = new List<int>();
        for (int i = 0; i < segmentation.VerseCount; i++)
        {
            if (segmentation.VerseChapter[i] != scope.Chapter) continue;
            if (scope.Verse is not null && segmentation.VerseNumberInChapter[i] != scope.Verse) continue;
            indexes.Add(i);
        }
        if (indexes.Count == 0)
            throw new ArgumentException($"scope {scope} selects no verse in this edition", nameof(scope));
        _ = view;
        return (indexes[0], indexes[^1]);
    }

    private static long CountWords(Segmentation s, int first, int last)
    {
        long total = 0;
        for (int v = first; v <= last; v++) total += s.VerseWordCount[v];
        return total;
    }

    private static long CountLetters(Segmentation s, int first, int last)
    {
        long total = 0;
        for (int w = s.VerseFirstWord[first]; w < s.VerseFirstWord[last] + s.VerseWordCount[last]; w++)
            total += s.WordLetterCount[w];
        return total;
    }

    private static long CountLetter(Segmentation s, int first, int last, char letter)
    {
        int from = s.WordFirstLetter[s.VerseFirstWord[first]];
        int lastWord = s.VerseFirstWord[last] + s.VerseWordCount[last] - 1;
        int to = s.WordFirstLetter[lastWord] + s.WordLetterCount[lastWord];
        long total = 0;
        for (int i = from; i < to; i++)
            if (s.LetterChars[i] == letter) total++;
        return total;
    }

    private static long CountForms(Segmentation s, int first, int last, IReadOnlySet<string> forms)
    {
        long total = 0;
        for (int v = first; v <= last; v++)
        {
            int start = s.VerseFirstWord[v];
            for (int w = start; w < start + s.VerseWordCount[v]; w++)
                if (forms.Contains(s.WordText(w))) total++;
        }
        return total;
    }

    private static long SumVerseNumbers(Segmentation s, int first, int last, IReadOnlySet<string> forms)
    {
        long total = 0;
        for (int v = first; v <= last; v++)
        {
            int start = s.VerseFirstWord[v];
            for (int w = start; w < start + s.VerseWordCount[v]; w++)
            {
                if (!forms.Contains(s.WordText(w))) continue;
                total += s.VerseNumberInChapter[v];
                break;
            }
        }
        return total;
    }
}
