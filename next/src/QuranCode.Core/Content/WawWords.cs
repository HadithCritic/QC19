namespace QuranCode.Core.Content;

/// <summary>
/// Words that begin with و where the waw belongs to the word, so waw-as-word
/// leaves them whole: the legacy <c>Data/waw-words.txt</c>.
/// </summary>
/// <remarks>
/// Some of these spellings are also, in particular verses, و plus another
/// word; the file lists those verses and the word is split there. Words are in
/// the text mode's rule-stage spelling, as the original compares them.
/// </remarks>
public sealed class WawWords
{
    public static readonly WawWords None = new(new HashSet<string>(StringComparer.Ordinal), new Dictionary<string, HashSet<(int, int)>>());

    private readonly HashSet<string> _words;
    private readonly IReadOnlyDictionary<string, HashSet<(int Chapter, int Verse)>> _splitIn;

    public WawWords(HashSet<string> words, IReadOnlyDictionary<string, HashSet<(int Chapter, int Verse)>> splitIn)
    {
        _words = words;
        _splitIn = splitIn;
    }

    public int Count => _words.Count;

    public bool Contains(string word) => _words.Contains(word);

    /// <summary>
    /// Whether a rule-stage word starting with و is split into و and the rest,
    /// following <c>Server.SplitWawPrefixsAsWords</c>.
    /// </summary>
    public bool ShouldSplit(string word, int chapter, int verse)
    {
        if (word.Length < 2 || word[0] != 'و') return false;
        if (!_words.Contains(word)) return true;
        return _splitIn.TryGetValue(word, out HashSet<(int, int)>? verses) && verses.Contains((chapter, verse));
    }

    /// <summary>A copy that also treats <paramref name="extra"/> as whole words.</summary>
    /// <remarks>
    /// With shadda as a letter the original adds each listed word's
    /// shadda-doubled spelling, so the doubling does not make it split. The
    /// added spellings get no verse exceptions, as in the original.
    /// </remarks>
    public WawWords With(IEnumerable<string> extra)
    {
        var words = new HashSet<string>(_words, StringComparer.Ordinal);
        words.UnionWith(extra);
        return new WawWords(words, _splitIn);
    }
}
