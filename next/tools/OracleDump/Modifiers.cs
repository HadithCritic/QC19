using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

// Golden data for the value modifiers.
//
// The 21 AddTo* flags on NumericalSystem and the 4 Alternate* flags on Server
// are never written to disk: SaveNumericalSystem persists only letter/value
// pairs. They are runtime state driven by checkboxes in MainForm, so the only
// way to learn what they compute is to set them and observe.
//
// Each case below sets exactly one thing away from the defaults, so a failure in
// the replacement engine points at one flag rather than at a combination. The
// last few cases combine flags, because the modifiers are additive and the
// interaction needs covering too.
internal static class Modifiers
{
    private const string Lf = "\n";

    private static string N(long v) { return v.ToString(CultureInfo.InvariantCulture); }

    private sealed class Case
    {
        public string Name;
        public Action<NumericalSystem> Configure;
        public CalculationMode Mode = CalculationMode.SumOfLetterValues;
        public bool AlternateLetters, AlternateWords, AlternateVerses, AlternateChapters;
    }

    private static readonly int[] Verses = { 1, 2, 7, 8, 262, 6221, 6236 };

    public static void Dump(Client client, string outputDir)
    {
        var cases = new List<Case>
        {
            new Case { Name = "default", Configure = _ => { } },

            // Calculation modes.
            new Case { Name = "mode_letter_digit_sums",
                       Configure = _ => { }, Mode = CalculationMode.SumOfLetterValueDigitSums },
            new Case { Name = "mode_letter_digital_roots",
                       Configure = _ => { }, Mode = CalculationMode.SumOfLetterValueDigitalRoots },
            new Case { Name = "mode_word_digit_sums",
                       Configure = _ => { }, Mode = CalculationMode.SumOfWordValueDigitSums },
            new Case { Name = "mode_word_digital_roots",
                       Configure = _ => { }, Mode = CalculationMode.SumOfWordValueDigitalRoots },

            // Sign alternation.
            new Case { Name = "alternate_letters", Configure = _ => { }, AlternateLetters = true },
            new Case { Name = "alternate_words",   Configure = _ => { }, AlternateWords = true },
            new Case { Name = "alternate_verses",  Configure = _ => { }, AlternateVerses = true },

            // Position modifiers, relative (AbsolutePositions = false).
            new Case { Name = "pos_letter_L", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterLNumber = true; } },
            new Case { Name = "pos_letter_W", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterWNumber = true; } },
            new Case { Name = "pos_letter_V", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterVNumber = true; } },
            new Case { Name = "pos_letter_C", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterCNumber = true; } },
            new Case { Name = "pos_word_W",   Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToWordWNumber = true; } },
            new Case { Name = "pos_word_V",   Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToWordVNumber = true; } },
            new Case { Name = "pos_word_C",   Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToWordCNumber = true; } },
            new Case { Name = "pos_verse_V",  Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToVerseVNumber = true; } },
            new Case { Name = "pos_verse_C",  Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToVerseCNumber = true; } },
            new Case { Name = "pos_chapter_C",Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToChapterCNumber = true; } },

            // Position modifiers, absolute.
            new Case { Name = "abs_letter_L", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AbsolutePositions = true; ns.AddToLetterLNumber = true; } },
            new Case { Name = "abs_letter_W", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AbsolutePositions = true; ns.AddToLetterWNumber = true; } },
            new Case { Name = "abs_letter_V", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AbsolutePositions = true; ns.AddToLetterVNumber = true; } },
            new Case { Name = "abs_letter_C", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AbsolutePositions = true; ns.AddToLetterCNumber = true; } },

            // Distance modifiers.
            new Case { Name = "dist_prev_letter_L", Configure = ns => { Reset(ns); ns.AddDistancesToPrevious = true; ns.AddToLetterLDistance = true; } },
            new Case { Name = "dist_prev_letter_W", Configure = ns => { Reset(ns); ns.AddDistancesToPrevious = true; ns.AddToLetterWDistance = true; } },
            new Case { Name = "dist_prev_word_W",   Configure = ns => { Reset(ns); ns.AddDistancesToPrevious = true; ns.AddToWordWDistance = true; } },
            new Case { Name = "dist_prev_verse_V",  Configure = ns => { Reset(ns); ns.AddDistancesToPrevious = true; ns.AddToVerseVDistance = true; } },

            // Combinations.
            new Case { Name = "combo_pos_letter_LW", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterLNumber = true; ns.AddToLetterWNumber = true; } },
            new Case { Name = "combo_pos_all_letter", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterLNumber = true; ns.AddToLetterWNumber = true; ns.AddToLetterVNumber = true; ns.AddToLetterCNumber = true; } },
            new Case { Name = "combo_alt_letters_pos_L", Configure = ns => { Reset(ns); ns.AddPositions = true; ns.AddToLetterLNumber = true; }, AlternateLetters = true },
        };

        var sb = new StringBuilder();
        sb.Append("# Value modifier behavior, captured from the legacy engine.").Append(Lf);
        sb.Append("# system=Original_Alphabet_Primes1, text_mode=Original").Append(Lf);
        sb.Append("# Each case changes one thing from the defaults unless named combo_*.").Append(Lf);
        sb.Append("# These flags are never persisted; they are runtime UI state.").Append(Lf);
        sb.Append("case\tverse\tvalue").Append(Lf);

        client.LoadNumericalSystem("Original_Alphabet_Primes1");
        Program.BuildBookPublic(client, client.NumericalSystem.TextMode);
        NumericalSystem system = client.NumericalSystem;
        Book book = client.Book;

        foreach (Case c in cases)
        {
            Reset(system);
            c.Configure(system);

            Server.CalculationMode = c.Mode;
            Server.AlternateLetterValues = c.AlternateLetters;
            Server.AlternateWordValues = c.AlternateWords;
            Server.AlternateVerseValues = c.AlternateVerses;
            Server.AlternateChapterValues = c.AlternateChapters;

            foreach (int verseNumber in Verses)
            {
                if (verseNumber < 1 || verseNumber > book.Verses.Count) continue;
                Verse verse = book.Verses[verseNumber - 1];
                sb.Append(c.Name).Append('\t')
                  .Append(N(verse.Number)).Append('\t')
                  .Append(N(client.CalculateValue(verse))).Append(Lf);
            }

            // Chapter 1 as a whole, so chapter-level modifiers show up too.
            sb.Append(c.Name).Append('\t')
              .Append("chapter1").Append('\t')
              .Append(N(client.CalculateValue(book.Chapters[0]))).Append(Lf);

            Console.WriteLine("  case " + c.Name);
        }

        // Leave the engine as we found it.
        Reset(system);
        Server.CalculationMode = CalculationMode.SumOfLetterValues;
        Server.AlternateLetterValues = false;
        Server.AlternateWordValues = false;
        Server.AlternateVerseValues = false;
        Server.AlternateChapterValues = false;

        File.WriteAllText(Path.Combine(outputDir, "modifiers.tsv"), sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote modifiers.tsv");
    }

    /// <summary>Clears every modifier flag so each case starts from a known state.</summary>
    private static void Reset(NumericalSystem ns)
    {
        ns.AddPositions = false;
        ns.AbsolutePositions = false;
        ns.AddDistancesToPrevious = false;
        ns.AddDistancesToNext = false;
        ns.AddDistancesWithinChapters = false;

        ns.AddToLetterLNumber = false; ns.AddToLetterWNumber = false;
        ns.AddToLetterVNumber = false; ns.AddToLetterCNumber = false;
        ns.AddToLetterLDistance = false; ns.AddToLetterWDistance = false;
        ns.AddToLetterVDistance = false; ns.AddToLetterCDistance = false;

        ns.AddToWordWNumber = false; ns.AddToWordVNumber = false; ns.AddToWordCNumber = false;
        ns.AddToWordWDistance = false; ns.AddToWordVDistance = false; ns.AddToWordCDistance = false;

        ns.AddToVerseVNumber = false; ns.AddToVerseCNumber = false;
        ns.AddToVerseVDistance = false; ns.AddToVerseCDistance = false;

        ns.AddToChapterCNumber = false;
    }
}
