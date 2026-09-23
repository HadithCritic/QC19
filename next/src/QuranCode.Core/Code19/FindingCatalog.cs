using System.Globalization;
using System.Reflection;

namespace QuranCode.Core.Code19;

/// <summary>
/// The findings, read from <c>findings.tsv</c> embedded beside this code.
/// Adding a finding is editing that file; it needs no engine change.
/// </summary>
public static class FindingCatalog
{
    private const string Resource = "QuranCode.Core.Code19.findings.tsv";

    private static IReadOnlyList<Finding>? _all;

    /// <summary>Every finding, in file order.</summary>
    public static IReadOnlyList<Finding> All => _all ??= Read(Open());

    private static Stream Open() =>
        typeof(FindingCatalog).GetTypeInfo().Assembly.GetManifestResourceStream(Resource)
        ?? throw new InvalidOperationException($"{Resource} is not embedded in the assembly");

    /// <summary>Parses the catalog format. Public so a test can read a fixture.</summary>
    /// <exception cref="InvalidDataException">A row is malformed.</exception>
    public static IReadOnlyList<Finding> Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var findings = new List<Finding>();
        using var reader = new StreamReader(stream);
        int lineNumber = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (line.Length == 0 || line[0] == '#') continue;

            string[] f = line.Split('\t');
            if (f.Length != 11)
                throw new InvalidDataException($"findings line {lineNumber}: {f.Length} fields, expected 11");
            findings.Add(new Finding(
                Id: f[0],
                Claim: f[1],
                Expected: long.Parse(f[2], CultureInfo.InvariantCulture),
                Measure: ParseMeasure(f[3], lineNumber),
                Scope: ParseScope(f[4], lineNumber),
                Match: f[5].Trim() is { Length: > 0 } match ? match : null,
                IncludeBasmalas: ParseYesNo(f[6], lineNumber),
                TextMode: f[7],
                Basis: ParseBasis(f[8], lineNumber),
                Rule: f[9],
                Source: f[10]));
        }

        var duplicate = findings.GroupBy(x => x.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null) throw new InvalidDataException($"two findings share the id \"{duplicate.Key}\"");
        return findings;
    }

    private static FindingMeasure ParseMeasure(string text, int line) => text switch
    {
        "words" => FindingMeasure.Words,
        "letters" => FindingMeasure.Letters,
        "letterOccurrences" => FindingMeasure.LetterOccurrences,
        "wordFormOccurrences" => FindingMeasure.WordFormOccurrences,
        "verseNumberSum" => FindingMeasure.VerseNumberSum,
        _ => throw new InvalidDataException($"findings line {line}: unknown measure \"{text}\""),
    };

    private static RuleBasis ParseBasis(string text, int line) => text switch
    {
        "stated" => RuleBasis.Stated,
        "inferred" => RuleBasis.Inferred,
        _ => throw new InvalidDataException($"findings line {line}: basis is \"stated\" or \"inferred\", not \"{text}\""),
    };

    private static bool ParseYesNo(string text, int line) => text switch
    {
        "yes" => true,
        "no" => false,
        _ => throw new InvalidDataException($"findings line {line}: basmalas is \"yes\" or \"no\", not \"{text}\""),
    };

    /// <summary>"book", "chapter:50" or "50:1".</summary>
    private static FindingScope ParseScope(string text, int line)
    {
        if (text == "book") return FindingScope.Book;
        if (text.StartsWith("chapter:", StringComparison.Ordinal)
            && int.TryParse(text.AsSpan("chapter:".Length), CultureInfo.InvariantCulture, out int chapter))
            return new FindingScope(chapter);

        string[] parts = text.Split(':');
        if (parts.Length == 2
            && int.TryParse(parts[0], CultureInfo.InvariantCulture, out int c)
            && int.TryParse(parts[1], CultureInfo.InvariantCulture, out int v))
            return new FindingScope(c, v);

        throw new InvalidDataException($"findings line {line}: scope is \"book\", \"chapter:N\" or \"C:V\", not \"{text}\"");
    }
}
