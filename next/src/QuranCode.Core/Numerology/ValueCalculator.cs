namespace QuranCode.Core.Numerology;

/// <summary>
/// Computes numerical values over normalized text.
/// </summary>
/// <remarks>
/// A direct reimplementation of the legacy <c>Server.CalculateValue(string)</c>
/// path, structured as a pure function rather than a method reading static
/// state.
///
/// <para>
/// <b>Why this is a faithful port and not a tidy-up.</b> The legacy walk is
/// verse &#8594; word &#8594; letter, with a sign that can flip at each boundary
/// and digit transforms applied at specific levels. A "simpler" formulation
/// that sums letters and applies transforms afterwards produces different
/// numbers as soon as any non-default option is set. The traversal shape is
/// part of the specification, so it is preserved.
/// </para>
///
/// <para>
/// Operates on <see cref="ReadOnlySpan{T}"/> throughout: no substring
/// allocation, no intermediate arrays, and no object graph. This is what makes
/// valuing the whole book cheap enough to do on demand rather than caching it.
/// </para>
/// </remarks>
public static class ValueCalculator
{
    /// <summary>
    /// Value of already-normalized text.
    /// </summary>
    /// <param name="text">
    /// Text that has already been through <see cref="Text.TextMode.Simplify"/>.
    /// Verses are separated by newlines and words by spaces, which is the shape
    /// the legacy engine's own splitting assumes.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The profile needs positional metadata that is not implemented yet. This
    /// throws rather than returning a plausible but wrong number.
    /// </exception>
    public static long Calculate(ReadOnlySpan<char> text, ValueSystem system, CalculationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.RequiresPositionalMetadata)
        {
            throw new NotSupportedException(
                "Position and distance modifiers are not implemented yet. " +
                "See docs/decisions/0001 and the scope note on CalculationProfile.");
        }

        if (text.IsEmpty) return 0L;

        long total = 0L;
        long verseSign = 1L;

        foreach (Range verseRange in SplitOn(text, '\n'))
        {
            ReadOnlySpan<char> verse = text[verseRange];
            if (verse.IsEmpty) continue;

            long verseValue = 0L;
            long wordSign = 1L;

            foreach (Range wordRange in SplitOn(verse, ' '))
            {
                ReadOnlySpan<char> word = verse[wordRange];
                if (word.IsEmpty) continue;

                long wordValue = 0L;
                long letterSign = 1L;

                foreach (char letter in word)
                {
                    long value = system[letter];
                    if (value == 0L) continue;

                    value = profile.Mode switch
                    {
                        CalculationMode.SumOfLetterValueDigitSums => DigitSum(value),
                        CalculationMode.SumOfLetterValueDigitalRoots => DigitalRoot(value),
                        _ => value,
                    };

                    wordValue += letterSign * value;
                    if (profile.AlternateLetterValues) letterSign = -letterSign;
                }

                wordValue = profile.Mode switch
                {
                    CalculationMode.SumOfWordValueDigitSums => DigitSum(wordValue),
                    CalculationMode.SumOfWordValueDigitalRoots => DigitalRoot(wordValue),
                    _ => wordValue,
                };

                verseValue += wordSign * wordValue;
                if (profile.AlternateWordValues) wordSign = -wordSign;
            }

            total += verseSign * verseValue;
            if (profile.AlternateVerseValues) verseSign = -verseSign;
        }

        return total;
    }

    /// <summary>Value of normalized text under the default profile.</summary>
    public static long Calculate(ReadOnlySpan<char> text, ValueSystem system) =>
        Calculate(text, system, CalculationProfile.Default);

    /// <summary>
    /// Sum of the decimal digits, preserving sign.
    /// </summary>
    internal static long DigitSum(long value)
    {
        long sign = value < 0 ? -1L : 1L;
        value = Math.Abs(value);

        long sum = 0L;
        while (value > 0)
        {
            sum += value % 10;
            value /= 10;
        }
        return sign * sum;
    }

    /// <summary>
    /// Repeated digit sum until a single digit remains, preserving sign.
    /// </summary>
    internal static long DigitalRoot(long value)
    {
        long sign = value < 0 ? -1L : 1L;
        value = Math.Abs(value);

        // The closed form (1 + (n-1) % 9) is equivalent for positive n and
        // avoids the loop, but the loop states the definition and this is not a
        // hot path relative to the per-letter lookup.
        while (value >= 10) value = DigitSum(value);
        return sign * value;
    }

    /// <summary>
    /// Enumerates the non-empty segments between occurrences of a separator.
    /// </summary>
    /// <remarks>
    /// Equivalent to <c>Split(..., RemoveEmptyEntries)</c>, which is what the
    /// legacy engine uses, but yields ranges instead of allocating strings.
    /// </remarks>
    private static SplitEnumerator SplitOn(ReadOnlySpan<char> text, char separator) => new(text, separator);

    private ref struct SplitEnumerator(ReadOnlySpan<char> text, char separator)
    {
        private readonly ReadOnlySpan<char> _text = text;
        private readonly char _separator = separator;
        private int _position;

        public Range Current { get; private set; }

        public readonly SplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            while (_position < _text.Length && _text[_position] == _separator) _position++;
            if (_position >= _text.Length) return false;

            int start = _position;
            while (_position < _text.Length && _text[_position] != _separator) _position++;

            Current = start.._position;
            return true;
        }
    }
}
