using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

// Golden-data extractor. Drives the legacy QuranCode engine directly and writes
// deterministic text files describing what it computes.
//
// This exists so the replacement engine can be checked against the behavior of
// the software as shipped, rather than against a reading of its source. The
// legacy engine is the oracle; these files are the contract.
//
// Output is intentionally plain TSV with LF endings and invariant number
// formatting, so a diff is meaningful and the files stay reviewable in git.
internal static class Program
{
    private const string Lf = "\n";

    private static int Main(string[] args)
    {
        // Every path inside Globals is relative, so the legacy engine reads its
        // data from the process working directory.
        string installRoot = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string outputDir = args.Length > 1 ? args[1] : Path.Combine(installRoot, "next", "tests", "golden");

        if (!Directory.Exists(Path.Combine(installRoot, "Data")))
        {
            Console.Error.WriteLine("Not a QuranCode install root (no Data directory): " + installRoot);
            Console.Error.WriteLine("Build the legacy solution (C#/Solution.sln); its output, C#/Build/Release, is one.");
            return 2;
        }

        Directory.SetCurrentDirectory(installRoot);
        Directory.CreateDirectory(outputDir);

        Console.WriteLine("install root : " + installRoot);
        Console.WriteLine("output       : " + outputDir);

        bool benchmark = Array.IndexOf(args, "--bench") >= 0;

        try
        {
            if (benchmark)
            {
                Benchmark.Run(installRoot, outputDir);
                Console.WriteLine("done");
                return 0;
            }

            var client = new Client(NumericalSystem.DEFAULT_NAME);
            BuildBook(client, client.NumericalSystem.TextMode);

            DumpEnvironment(client, outputDir);
            DumpBookStructure(client, outputDir);
            DumpTextModes(client, outputDir);
            DumpValueSystems(client, outputDir);
            DumpLetterFrequencies(client, outputDir);
            Positions.Dump(client, SampleVerses, outputDir);
            Modifiers.Dump(client, outputDir);
            Searches.Dump(client, outputDir);
            CountingOptions.Dump(client, outputDir);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAILED: " + ex.GetType().Name + ": " + ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }

        Console.WriteLine("done");
        return 0;
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    private static void Write(string path, StringBuilder content)
    {
        // UTF-8 without BOM, LF endings, so the files diff cleanly.
        File.WriteAllText(path, content.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote " + Path.GetFileName(path));
    }

    private static string N(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    // The book is not built by the Client constructor; the UI builds it during
    // startup. These are the defaults MainForm uses on a fresh launch, and they
    // are part of the behavioral contract: change one and every count and value
    // below it moves.
    private const bool WithDiacritics = false;
    private const bool WithBismAllah = true;
    private const bool WawAsWord = false;
    private const bool ShaddaAsLetter = false;
    private const bool HamzaAboveHorizontalLineAsLetter = false;
    private const bool ElfAboveHorizontalLineAsLetter = false;
    private const bool YaaAboveHorizontalLineAsLetter = false;
    private const bool NoonAboveHorizontalLineAsLetter = false;
    private const bool EmlaaeiText = false;

    internal static void BuildBookPublic(Client client, string textMode)
    {
        BuildBook(client, textMode);
    }

    private static void BuildBook(Client client, string textMode)
    {
        client.BuildSimplifiedBook(
            textMode,
            WithDiacritics,
            WithBismAllah,
            WawAsWord,
            ShaddaAsLetter,
            HamzaAboveHorizontalLineAsLetter,
            ElfAboveHorizontalLineAsLetter,
            YaaAboveHorizontalLineAsLetter,
            NoonAboveHorizontalLineAsLetter,
            EmlaaeiText);
    }

    /// <summary>
    /// Sample verses spread across the book: openings, famous verses, the
    /// shortest and longest chapters, and the boundaries. Fixed forever so the
    /// golden files stay comparable across runs.
    /// </summary>
    private static readonly int[] SampleVerses =
    {
        1, 2, 7,            // Al-Fatiha
        8, 9, 10,           // start of Al-Baqara
        262,                // Ayat Al-Kursi
        1473,               // mid-book
        3000, 4000, 5000,
        6221, 6222,         // Al-Ikhlaas
        6231, 6236          // final verses
    };

    // ------------------------------------------------------------------
    // dumps
    // ------------------------------------------------------------------

    private static void DumpEnvironment(Client client, string outputDir)
    {
        var sb = new StringBuilder();
        sb.Append("# QuranCode legacy oracle environment").Append(Lf);
        sb.Append("key\tvalue").Append(Lf);
        sb.Append("edition\t").Append(Globals.EDITION).Append(Lf);
        sb.Append("default_numerical_system\t").Append(NumericalSystem.DEFAULT_NAME).Append(Lf);
        sb.Append("default_simplification_system\t").Append(SimplificationSystem.DEFAULT_NAME).Append(Lf);
        sb.Append("loaded_numerical_systems\t")
          .Append(N(client.LoadedNumericalSystems == null ? 0 : client.LoadedNumericalSystems.Count)).Append(Lf);
        sb.Append("word_count_method\t").Append(N(Server.WordCountMethod)).Append(Lf);
        sb.Append("calculation_mode\t").Append(Server.CalculationMode).Append(Lf);
        sb.Append("alternate_letter_values\t").Append(Server.AlternateLetterValues).Append(Lf);
        sb.Append("alternate_word_values\t").Append(Server.AlternateWordValues).Append(Lf);
        sb.Append("alternate_verse_values\t").Append(Server.AlternateVerseValues).Append(Lf);
        sb.Append("alternate_chapter_values\t").Append(Server.AlternateChapterValues).Append(Lf);
        // Book build parameters, recorded because every count below depends on them.
        sb.Append("build.with_diacritics\t").Append(WithDiacritics).Append(Lf);
        sb.Append("build.with_bism_Allah\t").Append(WithBismAllah).Append(Lf);
        sb.Append("build.waw_as_word\t").Append(WawAsWord).Append(Lf);
        sb.Append("build.shadda_as_letter\t").Append(ShaddaAsLetter).Append(Lf);
        sb.Append("build.hamza_above_horizontal_line_as_letter\t").Append(HamzaAboveHorizontalLineAsLetter).Append(Lf);
        sb.Append("build.elf_above_horizontal_line_as_letter\t").Append(ElfAboveHorizontalLineAsLetter).Append(Lf);
        sb.Append("build.yaa_above_horizontal_line_as_letter\t").Append(YaaAboveHorizontalLineAsLetter).Append(Lf);
        sb.Append("build.noon_above_horizontal_line_as_letter\t").Append(NoonAboveHorizontalLineAsLetter).Append(Lf);
        sb.Append("build.emlaaei_text\t").Append(EmlaaeiText).Append(Lf);
        Write(Path.Combine(outputDir, "environment.tsv"), sb);
    }

    private static void DumpBookStructure(Client client, string outputDir)
    {
        Book book = client.Book;
        if (book == null) throw new InvalidOperationException("Client.Book is null after initialization.");

        var sb = new StringBuilder();
        sb.Append("# Book structure under the default numerical system").Append(Lf);
        sb.Append("# text_mode=").Append(book.TextMode).Append(Lf);
        sb.Append("metric\tvalue").Append(Lf);

        // Several partition collections are populated lazily and stay null until
        // something asks for them, so report count or "null" rather than crashing.
        Action<string, object> count = (name, collection) =>
        {
            string value = "null";
            var list = collection as System.Collections.ICollection;
            if (list != null) value = N(list.Count);
            sb.Append(name).Append('\t').Append(value).Append(Lf);
        };

        count("chapters", book.Chapters);
        count("verses", book.Verses);
        count("words", book.Words);
        count("letters", book.Letters);
        count("unique_letters", book.UniqueLetters);
        count("stations", book.Stations);
        count("parts", book.Parts);
        count("groups", book.Groups);
        count("halfs", book.Halfs);
        count("quarters", book.Quarters);
        count("bowings", book.Bowings);
        count("pages", book.Pages);
        Write(Path.Combine(outputDir, "book-structure.tsv"), sb);

        // Per-chapter counts: the single most useful structural regression net.
        var chapters = new StringBuilder();
        chapters.Append("# Per-chapter counts under the default numerical system").Append(Lf);
        chapters.Append("# text_mode=").Append(book.TextMode).Append(Lf);
        chapters.Append("chapter\tname\ttransliterated_name\tverses\twords\tletters\tunique_letters\trevelation_order").Append(Lf);
        foreach (Chapter c in book.Chapters)
        {
            chapters.Append(N(c.Number)).Append('\t')
                    .Append(c.Name).Append('\t')
                    .Append(c.TransliteratedName).Append('\t')
                    .Append(N(c.Verses.Count)).Append('\t')
                    .Append(N(c.Words.Count)).Append('\t')
                    .Append(N(c.Letters.Count)).Append('\t')
                    .Append(N(c.UniqueLetters.Count)).Append('\t')
                    .Append(N(c.RevelationOrder)).Append(Lf);
        }
        Write(Path.Combine(outputDir, "chapters.tsv"), chapters);
    }

    private static void DumpTextModes(Client client, string outputDir)
    {
        // The simplification rule sets shipped in Rules/<word-count-method>/.
        string[] textModes =
        {
            "Original", "Simplified28", "Simplified29", "Simplified30",
            "Simplified31", "Simplified36", "SimplifiedDots", "SimplifiedMarks"
        };

        var sb = new StringBuilder();
        sb.Append("# Text normalization: sample verses under every shipped text mode.").Append(Lf);
        sb.Append("# Produced by Server.LoadSimplificationSystem + string.Simplify(text_mode).").Append(Lf);
        sb.Append("text_mode\tverse\tchapter:verse\tletters\twords\ttext").Append(Lf);

        Book book = client.Book;
        foreach (string textMode in textModes)
        {
            Server.LoadSimplificationSystem(textMode);

            foreach (int verseNumber in SampleVerses)
            {
                if (verseNumber < 1 || verseNumber > book.Verses.Count) continue;
                Verse verse = book.Verses[verseNumber - 1];

                string original = verse.Text;
                string simplified = original.Simplify(textMode);

                // Counts are derived from the simplified form the way the engine
                // does it, so a normalization regression shows up as a count diff.
                int letters = 0;
                foreach (char ch in simplified)
                {
                    if (ch != ' ' && ch != '\n' && ch != '\r') letters++;
                }
                string[] words = simplified.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                sb.Append(textMode).Append('\t')
                  .Append(N(verse.Number)).Append('\t')
                  .Append(N(verse.Chapter.Number)).Append(':').Append(N(verse.NumberInChapter)).Append('\t')
                  .Append(N(letters)).Append('\t')
                  .Append(N(words.Length)).Append('\t')
                  .Append(simplified.Replace('\t', ' ')).Append(Lf);
            }
        }

        // Leave the engine on its default so later dumps are not affected.
        Server.LoadSimplificationSystem(SimplificationSystem.DEFAULT_NAME);
        Write(Path.Combine(outputDir, "text-modes.tsv"), sb);
    }

    private static void DumpValueSystems(Client client, string outputDir)
    {
        // A cross-section of the 410 shipped systems: the default, plus one per
        // distinct letter-order family, so every ordering strategy is covered.
        string[] systems =
        {
            "Original_Alphabet_Primes1",
            "Original_Abjad_Gematria",
            "Original_Appearance_Composites",
            "Simplified29_Alphabet_Primes1",
            "Simplified29_Abjad_Gematria",
            "Simplified29_Frequency_Primes1",
            "Simplified28_Alphabet_Primes1",
            "Simplified31_Alphabet_Primes1",
            "Simplified36_Alphabet_Primes1"
        };

        var letterValues = new StringBuilder();
        letterValues.Append("# Letter value maps per numerical system.").Append(Lf);
        letterValues.Append("system\tletter\tcodepoint\tvalue").Append(Lf);

        var verseValues = new StringBuilder();
        verseValues.Append("# Verse values under each numerical system, default calculation mode.").Append(Lf);
        verseValues.Append("# Server.CalculationMode=SumOfLetterValues, no sign alternation.").Append(Lf);
        verseValues.Append("system\tverse\tchapter:verse\tvalue").Append(Lf);

        var totals = new StringBuilder();
        totals.Append("# Aggregate values per numerical system.").Append(Lf);
        totals.Append("system\ttext_mode\tletter_order\tletter_value\tletter_values_sum\tbook_value\tchapter1_value").Append(Lf);

        foreach (string system in systems)
        {
            if (client.LoadedNumericalSystems != null && !client.LoadedNumericalSystems.ContainsKey(system))
            {
                Console.Error.WriteLine("  skipped (not installed): " + system);
                continue;
            }

            client.LoadNumericalSystem(system);
            NumericalSystem ns = client.NumericalSystem;
            if (ns == null)
            {
                Console.Error.WriteLine("  skipped (failed to load): " + system);
                continue;
            }

            // Each system names its own text mode, and the book's word and letter
            // segmentation follows that mode, so it has to be rebuilt per system.
            BuildBook(client, ns.TextMode);

            // Deterministic ordering: sort by codepoint, not dictionary order.
            var letters = new List<char>(ns.LetterValues.Keys);
            letters.Sort();
            foreach (char letter in letters)
            {
                letterValues.Append(system).Append('\t')
                            .Append(letter).Append('\t')
                            .Append("U+").Append(((int)letter).ToString("X4", CultureInfo.InvariantCulture)).Append('\t')
                            .Append(N(ns.LetterValues[letter])).Append(Lf);
            }

            Book book = client.Book;
            foreach (int verseNumber in SampleVerses)
            {
                if (verseNumber < 1 || verseNumber > book.Verses.Count) continue;
                Verse verse = book.Verses[verseNumber - 1];
                verseValues.Append(system).Append('\t')
                           .Append(N(verse.Number)).Append('\t')
                           .Append(N(verse.Chapter.Number)).Append(':').Append(N(verse.NumberInChapter)).Append('\t')
                           .Append(N(client.CalculateValue(verse))).Append(Lf);
            }

            totals.Append(system).Append('\t')
                  .Append(ns.TextMode).Append('\t')
                  .Append(ns.LetterOrder).Append('\t')
                  .Append(ns.LetterValue).Append('\t')
                  .Append(N(ns.LetterValuesSum)).Append('\t')
                  .Append(N(client.CalculateValue(book))).Append('\t')
                  .Append(N(client.CalculateValue(book.Chapters[0]))).Append(Lf);

            Console.WriteLine("  system " + system);
        }

        client.LoadNumericalSystem(NumericalSystem.DEFAULT_NAME);
        BuildBook(client, client.NumericalSystem.TextMode);

        Write(Path.Combine(outputDir, "letter-values.tsv"), letterValues);
        Write(Path.Combine(outputDir, "verse-values.tsv"), verseValues);
        Write(Path.Combine(outputDir, "system-totals.tsv"), totals);
    }

    private static void DumpLetterFrequencies(Client client, string outputDir)
    {
        Book book = client.Book;
        var sb = new StringBuilder();
        sb.Append("# Letter frequencies across the whole book, default text mode.").Append(Lf);
        sb.Append("# text_mode=").Append(book.TextMode).Append(Lf);
        sb.Append("letter\tcodepoint\tfrequency").Append(Lf);

        var letters = new List<char>(book.UniqueLetters);
        letters.Sort();
        foreach (char letter in letters)
        {
            sb.Append(letter).Append('\t')
              .Append("U+").Append(((int)letter).ToString("X4", CultureInfo.InvariantCulture)).Append('\t')
              .Append(N(book.GetLetterFrequency(letter))).Append(Lf);
        }
        Write(Path.Combine(outputDir, "letter-frequencies.tsv"), sb);
    }
}
