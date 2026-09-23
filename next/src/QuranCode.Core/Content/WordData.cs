namespace QuranCode.Core.Content;

/// <summary>One part of a word as the Quranic Arabic Corpus analyzes it.</summary>
/// <param name="Form">The part in the corpus's Buckwalter transliteration.</param>
/// <param name="Features">The corpus's features, verbatim, split at their bars: STEM, POS:N, LEM:{som, ROOT:smw, M, GEN.</param>
public sealed record WordPart(int Part, string Form, string Tag, IReadOnlyList<string> Features);

/// <summary>A display word's gloss, transliteration and grammar (Features.txt #62, #64).</summary>
public sealed record WordData(string Meaning, string Transliteration, IReadOnlyList<WordPart> Parts);
