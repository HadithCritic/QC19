using System;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

/// <summary>
/// Captures what the Statistics panel's text options do: Bismillah, waw as a
/// word, shadda as a letter, and hamza, alif, yaa and noon above a horizontal
/// line as letters.
/// </summary>
/// <remarks>
/// For each option set and system, one row per chapter (verses, words, letters,
/// value) and one row for the whole book (chapter 0). Each option set rebuilds
/// the book through Client.BuildSimplifiedBook, exactly as the UI does.
/// </remarks>
internal static class CountingOptions
{
    private const char Lf = '\n';

    private sealed class OptionSet
    {
        public string Name;
        public bool WithBismAllah = true;
        public bool WawAsWord;
        public bool ShaddaAsLetter;
        public bool HamzaAboveHorizontalLine;
        public bool ElfAboveHorizontalLine;
        public bool YaaAboveHorizontalLine;
        public bool NoonAboveHorizontalLine;
    }

    private static readonly OptionSet[] Sets =
    {
        new OptionSet { Name = "default" },
        new OptionSet { Name = "no_bismillah", WithBismAllah = false },
        new OptionSet { Name = "waw_as_word", WawAsWord = true },
        new OptionSet { Name = "shadda_as_letter", ShaddaAsLetter = true },
        new OptionSet { Name = "hamza_above_line", HamzaAboveHorizontalLine = true },
        new OptionSet { Name = "elf_above_line", ElfAboveHorizontalLine = true },
        new OptionSet { Name = "yaa_above_line", YaaAboveHorizontalLine = true },
        new OptionSet { Name = "noon_above_line", NoonAboveHorizontalLine = true },
        new OptionSet { Name = "waw_and_shadda", WawAsWord = true, ShaddaAsLetter = true },
        new OptionSet
        {
            Name = "all", WithBismAllah = false, WawAsWord = true, ShaddaAsLetter = true,
            HamzaAboveHorizontalLine = true, ElfAboveHorizontalLine = true,
            YaaAboveHorizontalLine = true, NoonAboveHorizontalLine = true,
        },
    };

    // The original enables these options in every text mode except Original;
    // hamza is also off in Simplified28 and Simplified30. These four cover both.
    private static readonly string[] Systems =
    {
        "Simplified29_Alphabet_Primes1",
        "Simplified31_Alphabet_Primes1",
        "Simplified36_Alphabet_Primes1",
        "Simplified28_Alphabet_Primes1",
    };

    public static void Dump(Client client, string outputDir)
    {
        var rows = new StringBuilder();
        rows.Append("# Counts and values under each Statistics-panel text option, per chapter;").Append(Lf);
        rows.Append("# chapter 0 is the whole book. Built with Client.BuildSimplifiedBook.").Append(Lf);
        rows.Append("options\tsystem\tchapter\tverses\twords\tletters\tvalue").Append(Lf);

        foreach (string system in Systems)
        {
            if (client.LoadedNumericalSystems != null && !client.LoadedNumericalSystems.ContainsKey(system))
            {
                Console.Error.WriteLine("  skipped (not installed): " + system);
                continue;
            }
            client.LoadNumericalSystem(system);
            string textMode = client.NumericalSystem.TextMode;

            foreach (OptionSet set in Sets)
            {
                client.BuildSimplifiedBook(
                    textMode, false, set.WithBismAllah, set.WawAsWord, set.ShaddaAsLetter,
                    set.HamzaAboveHorizontalLine, set.ElfAboveHorizontalLine,
                    set.YaaAboveHorizontalLine, set.NoonAboveHorizontalLine, false);

                Book book = client.Book;
                long bookWords = 0, bookLetters = 0;
                foreach (Chapter chapter in book.Chapters)
                {
                    long words = 0, letters = 0;
                    foreach (Verse verse in chapter.Verses)
                    {
                        words += verse.Words.Count;
                        foreach (Word word in verse.Words) letters += word.Letters.Count;
                    }
                    bookWords += words;
                    bookLetters += letters;
                    Row(rows, set.Name, system, chapter.Number, chapter.Verses.Count, words, letters,
                        client.CalculateValue(chapter));
                }
                Row(rows, set.Name, system, 0, book.Verses.Count, bookWords, bookLetters, client.CalculateValue(book));
                Console.WriteLine("  counting options " + system + " " + set.Name);
            }
        }

        client.LoadNumericalSystem(NumericalSystem.DEFAULT_NAME);
        File.WriteAllText(Path.Combine(outputDir, "counting-options.tsv"), rows.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote counting-options.tsv");
    }

    private static void Row(StringBuilder rows, string options, string system, int chapter,
        long verses, long words, long letters, long value)
    {
        rows.Append(options).Append('\t').Append(system).Append('\t')
            .Append(chapter.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append(verses.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append(words.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append(letters.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append(value.ToString(CultureInfo.InvariantCulture)).Append(Lf);
    }
}
