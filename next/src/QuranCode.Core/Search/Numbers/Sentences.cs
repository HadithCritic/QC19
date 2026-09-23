using QuranCode.Core.Text;

namespace QuranCode.Core.Search.Numbers;

/// <summary>A pause mark after a word (legacy <c>Stopmark</c>).</summary>
public enum Stopmark
{
    None,

    /// <summary>ۙ (laa): do not stop.</summary>
    MustContinue,

    /// <summary>ۖ (sala): better to continue.</summary>
    ShouldContinue,

    /// <summary>ۚ (jeem): may stop.</summary>
    CanStop,

    /// <summary>ۛ (three dots, in pairs): stop at one of the two.</summary>
    CanStopAtEither,

    /// <summary>ۗ (qala): better to stop.</summary>
    ShouldStop,

    /// <summary>ۜ (seen): a short pause without breath.</summary>
    MustPause,

    /// <summary>ۘ (meem), and every verse end: stop.</summary>
    MustStop,
}

/// <summary>A run of counted words, first to last inclusive (absolute word indexes).</summary>
public readonly record struct WordSpan(int First, int Last)
{
    public int Count => Last - First + 1;
}

/// <summary>
/// Sentences by stop marks (Features.txt #25, #26), as the legacy
/// <c>Server.DoFindSentences</c> splits them.
/// </summary>
/// <remarks>
/// <para>
/// Each counted word takes the stop mark written after it, or after the
/// second of two marks (36:52); the last word of a verse stops. A mark that
/// ends a sentence closes it; ۙ also starts the clause after it as its own
/// sentence; the ۛ pair yields the sentence to each mark and the tails after
/// them; ۜ continues after من and بل and stops after عوجا, مرقدنا and ماليه.
/// Sentences may cross verses.
/// </para>
/// <para>
/// The legacy loop can emit the same sentence twice when marks of ۙ follow one
/// another; each sentence is listed once here.
/// </para>
/// </remarks>
public static class Sentences
{
    private static readonly string[] PauseAndContinue = ["من", "بل"];

    public static Stopmark MarkOf(string token) => token switch
    {
        "ۙ" => Stopmark.MustContinue,
        "ۖ" => Stopmark.ShouldContinue,
        "ۚ" => Stopmark.CanStop,
        "ۛ" => Stopmark.CanStopAtEither,
        "ۗ" => Stopmark.ShouldStop,
        "ۜ" => Stopmark.MustPause,
        "ۘ" => Stopmark.MustStop,
        _ => Stopmark.None,
    };

    /// <summary>
    /// The stop mark after each display word of a verse. Marks the display
    /// attaches to a word follow it as separate tokens.
    /// </summary>
    public static Stopmark[] DisplayMarks(string[] displayWords)
    {
        var marks = new Stopmark[displayWords.Length];
        for (int w = 0; w < displayWords.Length; w++)
        {
            string[] tokens = displayWords[w].Split(' ');
            Stopmark mark = Stopmark.None;
            foreach (string token in tokens.Skip(1))
            {
                Stopmark next = MarkOf(token);
                if (next != Stopmark.None) mark = next; // the second of two marks wins (36:52)
                else if (token is "۩" && mark == Stopmark.None) mark = Stopmark.MustStop;
            }

            // The fourth word of a prefixed Bismillah closes it (legacy ApplyWordStopmarks).
            if (w == 3 && w < displayWords.Length - 1 && mark == Stopmark.None &&
                ArabicNormalizer.Simplify29(tokens[0]) is "الرحيم" or "الررحيم")
            {
                mark = Stopmark.CanStop;
            }
            marks[w] = mark;
        }
        if (marks.Length > 0 && marks[^1] == Stopmark.None) marks[^1] = Stopmark.MustStop;
        return marks;
    }

    /// <summary>Splits a run of words into sentences.</summary>
    /// <param name="marks">The stop mark after each word, in order.</param>
    /// <param name="plainText">Each word's text without marks (Simplify29), for the ۜ cases.</param>
    public static IReadOnlyList<WordSpan> Split(IReadOnlyList<Stopmark> marks, Func<int, string> plainText)
    {
        ArgumentNullException.ThrowIfNull(marks);
        ArgumentNullException.ThrowIfNull(plainText);

        var found = new List<WordSpan>();
        var seen = new HashSet<WordSpan>();
        void Emit(int first, int last)
        {
            var span = new WordSpan(first, last);
            if (first <= last && seen.Add(span)) found.Add(span);
        }

        int n = marks.Count;
        bool doneMustContinue = false;
        int restarted = -1;
        for (int i = 0; i < n - 1; i++)
        {
            if (marks[i] is not (Stopmark.None or Stopmark.CanStopAtEither or Stopmark.MustPause))
            {
                Emit(i, i);
                continue;
            }

            int start = i;
            bool doneEither = false;
            for (int j = i + 1; j < n; j++)
            {
                Stopmark mark = marks[j];
                if (mark == Stopmark.None) continue;

                if (mark == Stopmark.MustContinue)
                {
                    Emit(i, j);
                    if (doneMustContinue)
                    {
                        doneMustContinue = false;
                        continue; // keep building the overlapping longer sentence
                    }

                    for (int k = j + 1; k < n; k++)
                    {
                        if (marks[k] == Stopmark.None) continue;
                        Emit(j + 1, k);
                        doneMustContinue = marks[k] is Stopmark.ShouldContinue or Stopmark.CanStop or Stopmark.ShouldStop;
                        j = k;
                        break;
                    }

                    // Start again from the same word, once: with two ۙ in one
                    // sentence the legacy loop would restart forever.
                    if (doneMustContinue && restarted != start)
                    {
                        restarted = start;
                        i = start - 1;
                        break;
                    }
                    doneMustContinue = false;
                    continue;
                }

                if (mark is Stopmark.ShouldContinue or Stopmark.CanStop or Stopmark.ShouldStop or Stopmark.MustStop)
                {
                    Emit(i, j);
                    i = j;
                    break;
                }

                if (mark == Stopmark.MustPause)
                {
                    string word = plainText(j);
                    if (PauseAndContinue.Contains(word)) continue;

                    // The legacy stops after عوجا, مرقدنا and ماليه and throws on
                    // any other paused word; stopping after any other is the safe reading.
                    Emit(i, j);
                    i = j;
                    break;
                }

                if (mark == Stopmark.CanStopAtEither && !doneEither)
                {
                    Emit(i, j);
                    int afterSecond = -1;
                    for (int k = j + 1; k < n; k++)
                    {
                        if (marks[k] == Stopmark.None) continue;
                        if (marks[k] == Stopmark.CanStopAtEither)
                        {
                            Emit(i, k);
                            afterSecond = k + 1;
                            continue;
                        }
                        Emit(j + 1, k);
                        if (afterSecond >= 0) Emit(afterSecond, k);
                        break;
                    }

                    // Then the whole sentence across both marks, from the same start.
                    j = i - 1;
                    doneEither = true;
                }

                // A ۛ after both have been handled: keep going.
            }
        }
        return found;
    }
}
