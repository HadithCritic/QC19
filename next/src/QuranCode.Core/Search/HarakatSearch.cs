using QuranCode.Core.Content;
using QuranCode.Core.Text;

namespace QuranCode.Core.Search;

/// <summary>
/// Same text with the same marks (F8, Features.txt #43): display words compared
/// exactly as written, harakat included.
/// </summary>
/// <remarks>
/// The legacy F8 runs its text search with diacritics on. Here the display
/// words are compared directly; the stop marks the display attaches to a word
/// are not part of it. Several words must follow one another.
/// Marks compare in canonical order, so typed text matches the source.
/// </remarks>
public static class HarakatSearch
{
    public static IReadOnlyList<VerseHit> Find(IReadOnlyList<Verse> verses, string text, IReadOnlySet<int>? scope = null)
    {
        ArgumentNullException.ThrowIfNull(verses);
        ArgumentNullException.ThrowIfNull(text);

        string[] wanted = DisplayWords.Split(text.Trim()).Select(Letters).Where(w => w.Length > 0).ToArray();
        var found = new List<VerseHit>();
        if (wanted.Length == 0) return found;

        foreach (Verse verse in verses)
        {
            if (scope is not null && !scope.Contains(verse.Number)) continue;
            string[] words = DisplayWords.Split(verse.Text).Select(Letters).ToArray();

            var hits = new List<int>();
            for (int start = 0; start + wanted.Length <= words.Length; start++)
            {
                bool match = true;
                for (int k = 0; k < wanted.Length && match; k++) match = words[start + k] == wanted[k];
                if (match) hits.AddRange(Enumerable.Range(start, wanted.Length));
            }
            if (hits.Count > 0) found.Add(new VerseHit(verse.Number, hits.Distinct().ToArray(), HitWords.Display));
        }
        return found;
    }

    /// <summary>
    /// The word itself, without the stop marks that follow it in the display,
    /// with its marks in canonical order.
    /// </summary>
    private static string Letters(string displayWord) =>
        MarkOrder.Canonical(string.Join(' ', displayWord.Split(' ').Where(token => token.Any(char.IsLetter))));
}
