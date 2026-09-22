using System.Collections.Frozen;

namespace QuranCode.Core.Numerology;

/// <summary>
/// A letter-to-number mapping, named <c>TextMode_LetterOrder_LetterValue</c>.
/// </summary>
/// <remarks>
/// The legacy <c>NumericalSystem</c> is already data-driven: 410 of these ship
/// as TSV files under <c>Values/</c>. This type keeps that design and changes
/// only the storage and the lookup.
///
/// <para>
/// The map is frozen at construction. Lookup happens once per letter of the
/// corpus (327,792 per full-book valuation), so it is the hottest path in the
/// engine, and <see cref="FrozenDictionary{TKey,TValue}"/> is measurably faster
/// to read than the legacy <c>Dictionary</c> for a map that is never mutated.
/// </para>
/// </remarks>
public sealed class ValueSystem
{
    /// <summary>Full system name, for example <c>Original_Alphabet_Primes1</c>.</summary>
    public string Name { get; }

    /// <summary>Text mode this system's letter set belongs to.</summary>
    public string TextModeName { get; }

    /// <summary>Ordering strategy: Alphabet, Abjad, Frequency, Appearance, and others.</summary>
    public string LetterOrder { get; }

    /// <summary>Value strategy: Primes1, Gematria, Composites, and others.</summary>
    public string LetterValue { get; }

    private readonly FrozenDictionary<char, long> _values;

    public ValueSystem(
        string name,
        string textModeName,
        string letterOrder,
        string letterValue,
        IReadOnlyDictionary<char, long> values)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(values);

        Name = name;
        TextModeName = textModeName;
        LetterOrder = letterOrder;
        LetterValue = letterValue;
        _values = values.ToFrozenDictionary();
    }

    /// <summary>Distinct letters carrying a value.</summary>
    public int LetterCount => _values.Count;

    /// <summary>
    /// Sum of all letter values. The legacy engine exposes this as
    /// <c>LetterValuesSum</c>; it is a useful integrity check on import.
    /// </summary>
    public long LetterValuesSum
    {
        get
        {
            long sum = 0;
            foreach (long value in _values.Values) sum += value;
            return sum;
        }
    }

    /// <summary>
    /// Value of a single letter, or zero when the letter is not in the system.
    /// </summary>
    /// <remarks>
    /// Unmapped characters return zero rather than throwing, matching the legacy
    /// behavior. Spaces, punctuation and diacritics that survive normalization
    /// therefore contribute nothing, which is what makes a sum over raw text
    /// equal a sum over segmented letters.
    /// </remarks>
    public long this[char letter] => _values.GetValueOrDefault(letter, 0L);

    /// <summary>Whether the system assigns a value to this letter.</summary>
    public bool Contains(char letter) => _values.ContainsKey(letter);

    /// <summary>The mapping, for export and verification.</summary>
    public IReadOnlyDictionary<char, long> Values => _values;
}
