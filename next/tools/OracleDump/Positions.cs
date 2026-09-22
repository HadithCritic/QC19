using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

// Positional and occurrence metadata.
//
// Server.AdjustValue feeds these fields into the value modifiers that
// NumericalSystem exposes (AddToLetterLNumber, AddToWordVDistance,
// AbsolutePositions, and so on). They are therefore part of the numerical
// contract, not merely navigational data, and a replacement engine has to
// reproduce them exactly before any modifier-enabled system can be trusted.
//
// They are also the hardest part of the model to infer from source alone,
// because they are assigned during Book construction across several passes.
internal static class Positions
{
    private const string Lf = "\n";

    private static string N(long v) { return v.ToString(CultureInfo.InvariantCulture); }

    public static void Dump(Client client, int[] sampleVerses, string outputDir)
    {
        Book book = client.Book;

        DumpWords(book, sampleVerses, outputDir);
        DumpLetters(book, sampleVerses, outputDir);
        DumpChapterValues(client, outputDir);
    }

    private static void DumpWords(Book book, int[] sampleVerses, string outputDir)
    {
        var sb = new StringBuilder();
        sb.Append("# Word-level positional and occurrence metadata for sample verses.").Append(Lf);
        sb.Append("# text_mode=").Append(book.TextMode).Append(Lf);
        sb.Append("# Consumed by Server.AdjustValue(Word) via the AddToWord* modifiers.").Append(Lf);
        sb.Append(string.Join("\t", new[]
        {
            "verse", "word_in_verse", "text", "letters",
            "number", "number_in_verse", "number_in_chapter",
            "frequency", "frequency_in_verse", "frequency_in_chapter",
            "occurrence", "occurrence_in_verse", "occurrence_in_chapter",
            "occurrences_before", "occurrences_after",
            "distance_prev_dL", "distance_prev_dW", "distance_prev_dV", "distance_prev_dC"
        })).Append(Lf);

        foreach (int verseNumber in sampleVerses)
        {
            if (verseNumber < 1 || verseNumber > book.Verses.Count) continue;
            Verse verse = book.Verses[verseNumber - 1];

            foreach (Word word in verse.Words)
            {
                Distance d = word.DistanceToPrevious;
                sb.Append(N(verse.Number)).Append('\t')
                  .Append(N(word.NumberInVerse)).Append('\t')
                  .Append(word.Text.Replace('\t', ' ')).Append('\t')
                  .Append(N(word.Letters.Count)).Append('\t')
                  .Append(N(word.Number)).Append('\t')
                  .Append(N(word.NumberInVerse)).Append('\t')
                  .Append(N(word.NumberInChapter)).Append('\t')
                  .Append(N(word.Frequency)).Append('\t')
                  .Append(N(word.FrequencyInVerse)).Append('\t')
                  .Append(N(word.FrequencyInChapter)).Append('\t')
                  .Append(N(word.Occurrence)).Append('\t')
                  .Append(N(word.OccurrenceInVerse)).Append('\t')
                  .Append(N(word.OccurrenceInChapter)).Append('\t')
                  .Append(N(word.OccurrencesBefore)).Append('\t')
                  .Append(N(word.OccurrencesAfter)).Append('\t')
                  .Append(N(d.dL)).Append('\t')
                  .Append(N(d.dW)).Append('\t')
                  .Append(N(d.dV)).Append('\t')
                  .Append(N(d.dC)).Append(Lf);
            }
        }

        Write(Path.Combine(outputDir, "word-positions.tsv"), sb);
    }

    private static void DumpLetters(Book book, int[] sampleVerses, string outputDir)
    {
        var sb = new StringBuilder();
        sb.Append("# Letter-level positional and occurrence metadata.").Append(Lf);
        sb.Append("# text_mode=").Append(book.TextMode).Append(Lf);
        sb.Append("# Consumed by Server.AdjustValue(Letter) via the AddToLetter* modifiers.").Append(Lf);
        sb.Append("# Restricted to the first two sample verses: this is per-letter data and").Append(Lf);
        sb.Append("# the whole book would be 327,792 rows.").Append(Lf);
        sb.Append(string.Join("\t", new[]
        {
            "verse", "letter", "codepoint",
            "number", "number_in_word", "number_in_verse", "number_in_chapter",
            "frequency", "frequency_in_word", "frequency_in_verse", "frequency_in_chapter",
            "occurrence", "occurrence_in_word", "occurrence_in_verse", "occurrence_in_chapter",
            "occurrences_before", "occurrences_after",
            "distance_prev_dL", "distance_prev_dW", "distance_prev_dV", "distance_prev_dC"
        })).Append(Lf);

        int emitted = 0;
        foreach (int verseNumber in sampleVerses)
        {
            if (emitted >= 2) break;
            if (verseNumber < 1 || verseNumber > book.Verses.Count) continue;
            Verse verse = book.Verses[verseNumber - 1];
            emitted++;

            foreach (Word word in verse.Words)
            {
                foreach (Letter letter in word.Letters)
                {
                    Distance d = letter.DistanceToPrevious;
                    sb.Append(N(verse.Number)).Append('\t')
                      .Append(letter.Character).Append('\t')
                      .Append("U+").Append(((int)letter.Character).ToString("X4", CultureInfo.InvariantCulture)).Append('\t')
                      .Append(N(letter.Number)).Append('\t')
                      .Append(N(letter.NumberInWord)).Append('\t')
                      .Append(N(letter.NumberInVerse)).Append('\t')
                      .Append(N(letter.NumberInChapter)).Append('\t')
                      .Append(N(letter.Frequency)).Append('\t')
                      .Append(N(letter.FrequencyInWord)).Append('\t')
                      .Append(N(letter.FrequencyInVerse)).Append('\t')
                      .Append(N(letter.FrequencyInChapter)).Append('\t')
                      .Append(N(letter.Occurrence)).Append('\t')
                      .Append(N(letter.OccurrenceInWord)).Append('\t')
                      .Append(N(letter.OccurrenceInVerse)).Append('\t')
                      .Append(N(letter.OccurrenceInChapter)).Append('\t')
                      .Append(N(letter.OccurrencesBefore)).Append('\t')
                      .Append(N(letter.OccurrencesAfter)).Append('\t')
                      .Append(N(d.dL)).Append('\t')
                      .Append(N(d.dW)).Append('\t')
                      .Append(N(d.dV)).Append('\t')
                      .Append(N(d.dC)).Append(Lf);
                }
            }
        }

        Write(Path.Combine(outputDir, "letter-positions.tsv"), sb);
    }

    private static void DumpChapterValues(Client client, string outputDir)
    {
        Book book = client.Book;
        var sb = new StringBuilder();
        sb.Append("# Value of every chapter under the default numerical system.").Append(Lf);
        sb.Append("# system=").Append(client.NumericalSystem.Name).Append(Lf);
        sb.Append("# A full-book regression net that stays small enough to review.").Append(Lf);
        sb.Append("chapter\tname\tverses\twords\tletters\tvalue").Append(Lf);

        foreach (Chapter chapter in book.Chapters)
        {
            sb.Append(N(chapter.Number)).Append('\t')
              .Append(chapter.Name).Append('\t')
              .Append(N(chapter.Verses.Count)).Append('\t')
              .Append(N(chapter.Words.Count)).Append('\t')
              .Append(N(chapter.Letters.Count)).Append('\t')
              .Append(N(client.CalculateValue(chapter))).Append(Lf);
        }

        Write(Path.Combine(outputDir, "chapter-values.tsv"), sb);
    }

    private static void Write(string path, StringBuilder content)
    {
        File.WriteAllText(path, content.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote " + Path.GetFileName(path));
    }
}
