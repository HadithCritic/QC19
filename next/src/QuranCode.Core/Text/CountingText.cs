using System.Text;
using System.Text.RegularExpressions;
using QuranCode.Core.Content;

namespace QuranCode.Core.Text;

/// <summary>
/// Turns a verse into the text that is counted, under a text mode and a set of
/// <see cref="CountingOptions"/>.
/// </summary>
/// <remarks>
/// <para>
/// The order is the original's (<c>Server.BuildSimplifiedBook</c>):
/// </para>
/// <list type="number">
///   <item><description>edition word rules for the verse (such as ما لم in 96:5);</description></item>
///   <item><description>combining marks put in one order (see <see cref="CanonicalizeMarks"/>);</description></item>
///   <item><description>the Bismillah stripped from verse 1, in the classic edition, if not counted;</description></item>
///   <item><description>shadda replaced by the letter it doubles;</description></item>
///   <item><description>hamza, alif, yaa and noon above a line turned into letters;</description></item>
///   <item><description>the text mode's rules;</description></item>
///   <item><description>a leading و split into its own word;</description></item>
///   <item><description>the letter stage.</description></item>
/// </list>
/// <para>
/// With every option off this is exactly <see cref="TextPipeline.Normalize"/>
/// of the verse text, so default counts are unchanged.
/// </para>
/// </remarks>
public sealed partial class CountingText
{
    // The original matches these exact strings, trailing space included, so
    // chapter 1's verse 1 (the Bismillah alone) is never stripped.
    private const string Bismillah = "بِسْمِ ٱللَّهِ ٱلرَّحْمَـٰنِ ٱلرَّحِيمِ ";
    private const string BismillahWithShadda = "بِّسْمِ ٱللَّهِ ٱلرَّحْمَـٰنِ ٱلرَّحِيمِ ";

    private readonly TextPipeline _pipeline;
    private readonly VerseRules _verseRules;
    private readonly CountingOptions _options;
    private readonly bool _stripBismillah;
    private readonly WawWords _waw;

    /// <param name="options">Already reduced to what the text mode allows (<see cref="CountingOptions.For"/>).</param>
    public CountingText(
        TextPipeline pipeline, VerseRules verseRules, CountingOptions options, BasmalaMode basmala, WawWords waw)
    {
        _pipeline = pipeline;
        _verseRules = verseRules;
        _options = options;
        _stripBismillah = basmala == BasmalaMode.Prefix && !options.IncludeBasmalas;
        _waw = waw;
    }

    /// <summary>The counted text of a verse: normalized words separated by spaces.</summary>
    public string Normalize(Verse verse)
    {
        string text = BeforeRules(verse);
        string ruled = _pipeline.Segment(text);
        if (_options.WawAsWord) ruled = SplitWaw(ruled, verse.ChapterNumber, verse.NumberInChapter);
        return _pipeline.LetterStage(ruled);
    }

    /// <summary>A display word normalized the same way, for aligning highlights.</summary>
    public string NormalizeWord(string word) => _pipeline.Normalize(ReplaceMarks(CanonicalizeMarks(word)));

    /// <summary>The verse text after every step that precedes the text mode's rules.</summary>
    public string BeforeRules(Verse verse)
    {
        string text = CanonicalizeMarks(_verseRules.Apply(verse.Number, verse.Text));
        if (_stripBismillah && (text.StartsWith(Bismillah, StringComparison.Ordinal) ||
                                text.StartsWith(BismillahWithShadda, StringComparison.Ordinal)))
        {
            string prefix = text.StartsWith(Bismillah, StringComparison.Ordinal) ? Bismillah : BismillahWithShadda;
            text = text.Replace(prefix, "", StringComparison.Ordinal);
        }
        return ReplaceMarks(text);
    }

    private string ReplaceMarks(string text)
    {
        if (_options.ShaddaAsLetter) text = DoubleShadda(text);
        if (_options.HamzaAboveLine) text = text.Replace("ـٔ", "ـء", StringComparison.Ordinal);
        if (_options.ElfAboveLine)
        {
            text = text.Replace("ـٰ", "ا", StringComparison.Ordinal).Replace("ٰ", "ا", StringComparison.Ordinal);
        }
        if (_options.YaaAboveLine) text = text.Replace("ـۧ", "ي", StringComparison.Ordinal);
        if (_options.NoonAboveLine) text = text.Replace("ـۨ", "ن", StringComparison.Ordinal);
        return text;
    }

    /// <summary>Each shadda becomes a copy of the character before it, left to right.</summary>
    internal static string DoubleShadda(string text)
    {
        if (!text.Contains('ّ')) return text;
        var builder = new StringBuilder(text);
        for (int j = 1; j < builder.Length; j++)
        {
            if (builder[j] == 'ّ') builder[j] = builder[j - 1];
        }
        return builder.ToString();
    }

    private string SplitWaw(string ruled, int chapter, int verse)
    {
        string[] words = ruled.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (_waw.ShouldSplit(words[i], chapter, verse)) words[i] = words[i].Insert(1, " ");
        }
        return string.Join(' ', words);
    }

    /// <summary>
    /// Puts vowel marks after a hamza that sits on a tatweel (ـٔ).
    /// </summary>
    /// <remarks>
    /// Almost every such hamza is stored as tatweel, hamza, vowel, the order the
    /// text-mode rules match. The Submission text stores three (7:58, 10:101,
    /// 15:92) as tatweel, vowel, hamza: the same glyph, but the rules then miss
    /// it and the hamza is dropped while the same word elsewhere keeps it. The
    /// classic text has no such case, so its counts do not change.
    /// </remarks>
    internal static string CanonicalizeMarks(string text) =>
        text.Contains('ـ') ? HamzaAfterVowels().Replace(text, "ـٔ$1") : text;

    [GeneratedRegex("ـ([ً-ْ]+)ٔ")]
    private static partial Regex HamzaAfterVowels();

    /// <summary>
    /// Waw words extended with the shadda-doubled spellings the original adds
    /// when waw-as-word and shadda-as-letter are both on.
    /// </summary>
    public static WawWords WawWordsFor(
        WawWords waw, CountingOptions options, IEnumerable<Verse> verses, CountingText text, TextPipeline pipeline)
    {
        if (!(options.WawAsWord && options.ShaddaAsLetter)) return waw;

        var extra = new List<string>();
        var current = waw;
        foreach (Verse verse in verses)
        {
            // The original scans the lines after the Bismillah is stripped and
            // before the shadda is replaced.
            string line = text.StrippedOnly(verse);
            foreach (string word in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!word.Contains('ّ') || !current.Contains(pipeline.Segment(word))) continue;
                for (int j = 1; j < word.Length; j++)
                {
                    if (word[j] != 'ّ') continue;
                    string variant = pipeline.Segment(word.Insert(j, word[j - 1].ToString()));
                    extra.Add(variant);
                    current = current.With([variant]);
                }
            }
        }
        return extra.Count == 0 ? waw : waw.With(extra);
    }

    private string StrippedOnly(Verse verse)
    {
        string text = CanonicalizeMarks(_verseRules.Apply(verse.Number, verse.Text));
        if (!_stripBismillah) return text;
        if (text.StartsWith(Bismillah, StringComparison.Ordinal)) return text.Replace(Bismillah, "", StringComparison.Ordinal);
        if (text.StartsWith(BismillahWithShadda, StringComparison.Ordinal)) return text.Replace(BismillahWithShadda, "", StringComparison.Ordinal);
        return text;
    }
}
