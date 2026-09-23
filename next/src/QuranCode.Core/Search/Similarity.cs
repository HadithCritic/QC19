namespace QuranCode.Core.Search;

/// <summary>Edit-distance similarity, as the legacy <c>String.GetSimilarityPercentage</c> computes it.</summary>
public static class Similarity
{
    /// <summary>1 - distance / longer length; two empty strings are identical.</summary>
    public static double Of(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        int longer = Math.Max(a.Length, b.Length);
        return longer == 0 ? 1.0 : 1.0 - (double)Distance(a, b) / longer;
    }

    /// <summary>
    /// The best similarity two strings of these lengths could have, since the
    /// distance is at least the difference in length. Lets a scan skip verses
    /// that cannot reach a threshold without computing the distance.
    /// </summary>
    public static double UpperBound(int a, int b)
    {
        int longer = Math.Max(a, b);
        return longer == 0 ? 1.0 : 1.0 - (double)Math.Abs(a - b) / longer;
    }

    /// <summary>Levenshtein distance, two rows at a time.</summary>
    public static int Distance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        int[] previous = new int[b.Length + 1];
        int[] current = new int[b.Length + 1];
        for (int j = 0; j <= b.Length; j++) previous[j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int substitution = previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1);
                current[j] = Math.Min(Math.Min(previous[j] + 1, current[j - 1] + 1), substitution);
            }
            (previous, current) = (current, previous);
        }
        return previous[b.Length];
    }
}
