using QuranCode.Core.Content;

namespace QuranCode.Core.Numerology;

/// <summary>
/// Valuation over a built <see cref="Segmentation"/>, including the position and
/// distance modifiers.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ValueCalculator"/> handles the common case where a value is a
/// function of text alone. This type handles the case where it also depends on
/// where each element sits and how far it is from its previous occurrence,
/// which is what the legacy <c>Server.AdjustValue</c> overloads compute.
/// </para>
/// <para>
/// The traversal is verse &#8594; word &#8594; letter, matching the legacy order,
/// because sign alternation and the digit transforms are applied at specific
/// levels and reordering changes results.
/// </para>
/// </remarks>
public static class SegmentedCalculator
{
    /// <summary>Value of one verse, by index.</summary>
    /// <remarks>
    /// Uses the legacy single-verse semantics, which differ from the aggregate
    /// path in the word-level digit modes. See <see cref="ValueOfVerses"/>.
    /// </remarks>
    public static long ValueOfVerse(
        Segmentation segmentation, int verseIndex,
        ValueSystem system, CalculationProfile profile, ModifierSet modifiers) =>
        Calculate(segmentation, verseIndex, 1, system, profile, modifiers, singleVerse: true);

    /// <summary>Value of one chapter, by 1-based chapter number.</summary>
    public static long ValueOfChapter(
        Segmentation segmentation, int chapterNumber,
        ValueSystem system, CalculationProfile profile, ModifierSet modifiers)
    {
        int first = -1, count = 0;
        for (int v = 0; v < segmentation.VerseCount; v++)
        {
            if (segmentation.VerseChapter[v] != chapterNumber) continue;
            if (first < 0) first = v;
            count++;
        }
        if (first < 0) return 0L;

        long total = ValueOfVerses(segmentation, first, count, system, profile, modifiers);

        // The chapter-level modifier is applied once per chapter, not per verse.
        if (profile.AddPositions && modifiers.ChapterCNumber) total += chapterNumber;

        return total;
    }

    /// <summary>Value of a contiguous run of verses.</summary>
    /// <remarks>
    /// <para>
    /// <b>This does not always equal the sum of the individual verse values.</b>
    /// The legacy engine implements the same calculation three times, and the
    /// aggregate path in <c>Server.CalculateValue(List&lt;Verse&gt;)</c> differs
    /// from the single-verse path in <c>Server.CalculateValue(Verse)</c>.
    /// </para>
    /// <para>
    /// In the word-level digit modes, the single-verse path routes through
    /// <c>CalculateValue(Letter)</c>, which suppresses the letter value unless
    /// the word is one letter long, so most verses come out as zero. The
    /// aggregate path inlines the letter loop with no such guard and produces a
    /// real number. Chapter 1 is the clearest case: its seven verses each value
    /// to 0 in <c>SumOfWordValueDigitSums</c>, while the chapter values to 289.
    /// </para>
    /// <para>
    /// Both behaviors are reproduced because both are observable in the shipped
    /// software, and reconciling them would change published results. This is a
    /// legacy inconsistency, recorded rather than corrected.
    /// </para>
    /// </remarks>
    public static long ValueOfVerses(
        Segmentation segmentation, int firstVerse, int verseCount,
        ValueSystem system, CalculationProfile profile, ModifierSet modifiers) =>
        Calculate(segmentation, firstVerse, verseCount, system, profile, modifiers, singleVerse: false);

    private static long Calculate(
        Segmentation segmentation, int firstVerse, int verseCount,
        ValueSystem system, CalculationProfile profile, ModifierSet modifiers,
        bool singleVerse)
    {
        ArgumentNullException.ThrowIfNull(segmentation);
        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(modifiers);

        // A Base system reads each word's letters as digits and adds the words,
        // with no calculation mode or additions, but only for a single verse:
        // the legacy CalculateValue(Verse) decodes, while CalculateValue(List<Verse>),
        // which chapters and the book go through, adds letter values like any
        // other system. Both are reproduced, as with the word-level digit modes.
        if (system.Radix is not null && singleVerse)
        {
            long sum = 0L;
            for (int v = firstVerse; v < firstVerse + verseCount; v++)
            {
                int firstWord = segmentation.VerseFirstWord[v];
                for (int w = firstWord; w < firstWord + segmentation.VerseWordCount[v]; w++)
                {
                    sum += system.BaseWordValue(segmentation.LetterChars.AsSpan(segmentation.WordFirstLetter[w], segmentation.WordLetterCount[w]));
                }
            }
            return sum;
        }

        bool positions = profile.AddPositions;
        bool distances = profile.AddDistancesToPrevious;

        long total = 0L;
        long verseSign = 1L;

        for (int v = firstVerse; v < firstVerse + verseCount; v++)
        {
            int chapterNumber = segmentation.VerseChapter[v];
            int verseInChapter = segmentation.VerseNumberInChapter[v];

            long verseValue = 0L;
            long wordSign = 1L;

            int firstWord = segmentation.VerseFirstWord[v];
            int words = segmentation.VerseWordCount[v];

            for (int w = firstWord; w < firstWord + words; w++)
            {
                long wordValue = 0L;
                long letterSign = 1L;

                int firstLetter = segmentation.WordFirstLetter[w];
                int letters = segmentation.WordLetterCount[w];

                for (int l = firstLetter; l < firstLetter + letters; l++)
                {
                    long value = system[segmentation.LetterChars[l]];

                    // The word-level digit modes suppress the letter value
                    // unless the word is a single letter. This is
                    // Server.CalculateValue(Letter), which guards those cases
                    // with `letter.Word.Letters.Count == 1`, presumably to avoid
                    // double counting against the word-level transform applied
                    // below. The practical effect is that most verses value to
                    // zero in these modes, which the golden data confirms.
                    bool wordMode = profile.Mode is CalculationMode.SumOfWordValueDigitSums
                                                 or CalculationMode.SumOfWordValueDigitalRoots;
                    if (singleVerse && wordMode && letters != 1) value = 0L;

                    value = profile.Mode switch
                    {
                        CalculationMode.SumOfLetterValueDigitSums =>
                            ValueCalculator.DigitSum(value),
                        CalculationMode.SumOfLetterValueDigitalRoots =>
                            ValueCalculator.DigitalRoot(value),
                        // The word modes transform per letter only on the
                        // single-verse path; the aggregate path transforms the
                        // summed word value instead.
                        CalculationMode.SumOfWordValueDigitSums when singleVerse =>
                            ValueCalculator.DigitSum(value),
                        CalculationMode.SumOfWordValueDigitalRoots when singleVerse =>
                            ValueCalculator.DigitalRoot(value),
                        _ => value,
                    };

                    // The adjustment carries the SAME sign as the letter value,
                    // and the sign flips only after both have been added. The
                    // legacy shape is:
                    //
                    //   letter_value  = l_sign * CalculateValue(letter);
                    //   letter_value += l_sign * AdjustValue(letter);
                    //   l_sign       *= AlternateLetterValues ? -1 : +1;
                    //   word_value   += letter_value;
                    //
                    // Applying the modifier unsigned, or flipping between the
                    // two additions, changes every alternating result.
                    long adjustment = 0L;

                    if (positions)
                    {
                        // AbsolutePositions is deliberately ignored for the L
                        // position: the legacy ternary returns NumberInWord on
                        // both branches. See ModifierSet.
                        if (modifiers.LetterLNumber) adjustment += segmentation.LetterNumberInWord[l];
                        if (modifiers.LetterWNumber)
                        {
                            adjustment += profile.AbsolutePositions
                                ? segmentation.LetterNumberInVerse[l]
                                : segmentation.WordNumberInVerse[w];
                        }
                        if (modifiers.LetterVNumber)
                        {
                            adjustment += profile.AbsolutePositions
                                ? segmentation.LetterNumberInChapter[l]
                                : verseInChapter;
                        }
                        if (modifiers.LetterCNumber)
                        {
                            adjustment += profile.AbsolutePositions ? l + 1 : chapterNumber;
                        }
                    }

                    if (distances)
                    {
                        if (modifiers.LetterLDistance) adjustment += segmentation.LetterDistanceL[l];
                        if (modifiers.LetterWDistance) adjustment += segmentation.LetterDistanceW[l];
                        if (modifiers.LetterVDistance) adjustment += segmentation.LetterDistanceV[l];
                        if (modifiers.LetterCDistance) adjustment += segmentation.LetterDistanceC[l];
                    }

                    wordValue += letterSign * (value + adjustment);

                    // The sign advances on every letter, including one whose
                    // value is zero, because the legacy loop has no such guard.
                    if (profile.AlternateLetterValues) letterSign = -letterSign;
                }

                wordValue = profile.Mode switch
                {
                    CalculationMode.SumOfWordValueDigitSums => ValueCalculator.DigitSum(wordValue),
                    CalculationMode.SumOfWordValueDigitalRoots => ValueCalculator.DigitalRoot(wordValue),
                    _ => wordValue,
                };

                if (positions)
                {
                    if (modifiers.WordWNumber) wordValue += segmentation.WordNumberInVerse[w];
                    if (modifiers.WordVNumber) wordValue += verseInChapter;
                    if (modifiers.WordCNumber) wordValue += chapterNumber;
                }

                if (distances)
                {
                    if (modifiers.WordWDistance) wordValue += segmentation.WordDistanceW[w];
                    if (modifiers.WordVDistance) wordValue += segmentation.WordDistanceV[w];
                    if (modifiers.WordCDistance) wordValue += segmentation.WordDistanceC[w];
                }

                verseValue += wordSign * wordValue;
                if (profile.AlternateWordValues) wordSign = -wordSign;
            }

            if (positions)
            {
                if (modifiers.VerseVNumber) verseValue += verseInChapter;
                if (modifiers.VerseCNumber) verseValue += chapterNumber;
            }

            total += verseSign * verseValue;
            if (profile.AlternateVerseValues) verseSign = -verseSign;
        }

        return total;
    }
}
