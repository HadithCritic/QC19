namespace QuranCode.Core.Search;

/// <summary>How the terms of a search combine within a verse.</summary>
public enum Grouping
{
    /// <summary>A verse qualifies when any term matches one of its words (the legacy "Any word").</summary>
    Any,

    /// <summary>Every term must match some word of the verse, in any order (the legacy "All words").</summary>
    All,

    /// <summary>The terms must follow one another as written (the legacy Exact search).</summary>
    Phrase,
}

/// <summary>A search term with its <c>+</c> or <c>-</c> prefix.</summary>
public readonly record struct Term(string Text, TermKind Kind);

public enum TermKind
{
    /// <summary>Counts toward the grouping.</summary>
    Plain,

    /// <summary><c>+term</c>: the verse must contain it, whatever the grouping.</summary>
    Required,

    /// <summary><c>-term</c>: the verse must not contain it.</summary>
    Excluded,
}

/// <summary>
/// Everything a text or root search needs besides the corpus.
/// </summary>
/// <param name="Scope">Absolute verse numbers to search, or null for the whole book.</param>
public sealed record TextQuery(
    string Text,
    Wordness Wordness = Wordness.Any,
    Grouping Grouping = Grouping.Any,
    IReadOnlySet<int>? Scope = null)
{
    /// <summary>
    /// Splits the query on spaces and reads the <c>+</c> and <c>-</c> prefixes.
    /// </summary>
    /// <remarks>
    /// The legacy form has hidden <c>+</c> and <c>-</c> labels and never parses
    /// them (Features.txt #55 lists them). They are implemented here: a phrase
    /// search takes its text literally, since a sign inside a phrase is not a
    /// term prefix.
    /// </remarks>
    public IReadOnlyList<Term> Terms(Func<string, string> normalize)
    {
        ArgumentNullException.ThrowIfNull(normalize);
        var terms = new List<Term>();
        foreach (string token in Text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            TermKind kind = Grouping == Grouping.Phrase ? TermKind.Plain : token[0] switch
            {
                '+' => TermKind.Required,
                '-' => TermKind.Excluded,
                _ => TermKind.Plain,
            };
            string text = normalize(kind == TermKind.Plain ? token : token[1..]).Replace(" ", "", StringComparison.Ordinal);
            if (text.Length > 0) terms.Add(new Term(text, kind));
        }
        return terms;
    }
}
