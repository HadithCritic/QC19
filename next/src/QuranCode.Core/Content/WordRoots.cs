namespace QuranCode.Core.Content;

/// <summary>
/// The roots of every display word, as the legacy word-roots file lists them
/// (particles and pronouns included).
/// </summary>
/// <remarks>
/// A word is addressed by its absolute verse number and its index among the
/// verse's display words (<see cref="Text.DisplayWords.Split"/> over the stored
/// verse text), so the links hold in every text mode and counting option.
/// </remarks>
public sealed class WordRoots
{
    private static readonly int[] None = [];

    private readonly string[] _roots;
    private readonly Dictionary<string, int> _ids;
    private readonly int[][][] _byVerse;

    /// <param name="roots">Root texts; a root's id is its index.</param>
    /// <param name="byVerse">Per verse (index = number - 1), per display word, the root ids.</param>
    public WordRoots(string[] roots, int[][][] byVerse)
    {
        ArgumentNullException.ThrowIfNull(roots);
        ArgumentNullException.ThrowIfNull(byVerse);
        _roots = roots;
        _byVerse = byVerse;
        _ids = new Dictionary<string, int>(roots.Length, StringComparer.Ordinal);
        for (int i = 0; i < roots.Length; i++) _ids[roots[i]] = i;
    }

    /// <summary>Every root, in import order.</summary>
    public IReadOnlyList<string> All => _roots;

    public string Text(int id) => _roots[id];

    public int? IdOf(string root) => _ids.TryGetValue(root.Trim(), out int id) ? id : null;

    /// <summary>Root ids of one display word, or none when the word is unknown.</summary>
    public IReadOnlyList<int> Of(int verseNumber, int wordIndex)
    {
        if (verseNumber < 1 || verseNumber > _byVerse.Length) return None;
        int[][] words = _byVerse[verseNumber - 1];
        return wordIndex >= 0 && wordIndex < words.Length ? words[wordIndex] : None;
    }

    /// <summary>Per display word of a verse, its root ids.</summary>
    public IReadOnlyList<int[]> OfVerse(int verseNumber) =>
        verseNumber >= 1 && verseNumber <= _byVerse.Length ? _byVerse[verseNumber - 1] : [];

    public int VerseCount => _byVerse.Length;
}
