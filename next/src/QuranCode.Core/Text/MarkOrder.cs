namespace QuranCode.Core.Text;

/// <summary>
/// Puts runs of Arabic combining marks in Unicode canonical order.
/// </summary>
/// <remarks>
/// Typed text orders its marks canonically (fatha before shadda) while the
/// source text often does not, so exact comparisons need one order. The engine
/// runs with invariant globalization, where <see cref="string.Normalize()"/>
/// does not reorder, so the reordering is done here from the combining classes
/// of the Arabic block. Letters are never changed.
/// </remarks>
public static class MarkOrder
{
    public static string Canonical(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        char[] chars = text.ToCharArray();
        bool changed = false;

        // Insertion sort within each run of marks. A starter (class 0) ends the
        // run, and the sort is stable, so marks of one class keep their order.
        for (int i = 1; i < chars.Length; i++)
        {
            char mark = chars[i];
            int cls = CombiningClass(mark);
            if (cls == 0) continue;

            int j = i;
            while (j > 0 && CombiningClass(chars[j - 1]) > cls)
            {
                chars[j] = chars[j - 1];
                j--;
            }
            if (j == i) continue;
            chars[j] = mark;
            changed = true;
        }
        return changed ? new string(chars) : text;
    }

    /// <summary>Canonical combining class of an Arabic-block character; 0 for everything else.</summary>
    public static int CombiningClass(char c) => c switch
    {
        >= 'ؐ' and <= 'ؗ' => 230,
        'ؘ' => 30,
        'ؙ' => 31,
        'ؚ' => 32,
        >= 'ً' and <= 'ْ' => 27 + (c - 'ً'), // fathatan 27 ... sukun 34
        'ٓ' or 'ٔ' => 230,
        'ٕ' or 'ٖ' => 220,
        >= 'ٗ' and <= 'ٛ' => 230,
        'ٜ' => 220,
        >= 'ٝ' and <= 'ٟ' => c == 'ٟ' ? 220 : 230,
        'ٰ' => 35,
        >= 'ۖ' and <= 'ۜ' => 230,
        >= '۟' and <= 'ۢ' => 230,
        'ۣ' => 220,
        'ۤ' => 230,
        'ۧ' or 'ۨ' => 230,
        '۪' => 220,
        '۫' or '۬' => 230,
        'ۭ' => 220,
        _ => 0,
    };
}
