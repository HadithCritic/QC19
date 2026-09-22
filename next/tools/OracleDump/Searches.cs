using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

// Golden data for text search.
//
// Server.FindWords has six overloads carrying thirteen or more parameters each,
// so the practical way to pin its behavior down is to call it across a spread of
// option combinations and record which verses come back.
//
// Results are recorded as verse numbers rather than match objects: the verse set
// is what a user sees and what a replacement has to reproduce, and it stays
// stable even if the internal match representation changes.
//
// Location filters (TextLocationInVerse / TextLocationInWord) are not covered:
// the Client overload MainForm actually calls does not accept them, so running
// them here produced duplicates of the unfiltered case rather than real data.
internal static class Searches
{
    private const string Lf = "\n";

    private sealed class Query
    {
        public string Name;
        public string Text;
        public TextWordness Wordness = TextWordness.Any;
    }

    public static void Dump(Client client, string outputDir)
    {
        // Terms chosen to exercise different shapes: a very common word, a
        // divine name, a short particle prone to false substring matches, and a
        // rare term.
        var queries = new List<Query>
        {
            new Query { Name = "allah_any",        Text = "الله" },
            new Query { Name = "allah_whole",      Text = "الله", Wordness = TextWordness.WholeWord },
            new Query { Name = "allah_part",       Text = "الله", Wordness = TextWordness.PartOfWord },
            new Query { Name = "rahman_any",       Text = "الرحمن" },
            new Query { Name = "rahman_whole",     Text = "الرحمن", Wordness = TextWordness.WholeWord },
            new Query { Name = "min_any",          Text = "من" },
            new Query { Name = "min_whole",        Text = "من", Wordness = TextWordness.WholeWord },
            new Query { Name = "kitab_any",        Text = "كتاب" },
            new Query { Name = "salat_any",        Text = "الصلوه" },
            new Query { Name = "yawm_whole",       Text = "يوم", Wordness = TextWordness.WholeWord },
        };

        // Probe: take a word straight out of the book and search for it. If
        // even this returns nothing, the failure is structural rather than in
        // the query terms.
        Book probeBook = client.Book;
        string probeWord = probeBook.Verses[0].Words[1].Text;
        client.SearchScope = SearchScope.Book;
        int probeCount = client.FindWords(
            TextSearchBlockSize.Verse, probeWord, LanguageType.RightToLeft, null,
            TextWordGrouping.Or, TextWordness.Any, false, false,
            -1, NumberType.None, ComparisonOperator.Equal, 0);
        Console.WriteLine("  PROBE word=[" + probeWord + "] len=" + probeWord.Length
            + " -> " + probeCount + " words");
        Console.WriteLine("  PROBE verse1 text=[" + probeBook.Verses[0].Text + "]");
        Console.WriteLine("  PROBE book verses=" + probeBook.Verses.Count);

        var summary = new StringBuilder();
        summary.Append("# Text search results from the legacy engine.").Append(Lf);
        summary.Append("# system=Original_Alphabet_Primes1, text_mode=Original, scope=Book").Append(Lf);
        summary.Append("# case_sensitive=false, with_diacritics=false, multiplicity=-1/None (MainForm defaults)").Append(Lf);
        summary.Append("query\tterm\twordness\tmatched_words\tmatched_verses\tfirst_verses").Append(Lf);

        var detail = new StringBuilder();
        detail.Append("# Verse numbers matched by each query, in order.").Append(Lf);
        detail.Append("query\tverse").Append(Lf);

        foreach (Query query in queries)
        {
            client.SearchScope = SearchScope.Book;

            int matched;
            try
            {
                // The Client overload is what MainForm calls. It manages scope
                // and stores the result, where the raw Server entry point needs
                // state the UI would normally have set up.
                matched = client.FindWords(
                    TextSearchBlockSize.Verse,
                    query.Text,
                    LanguageType.RightToLeft,
                    null,
                    TextWordGrouping.Or,
                    query.Wordness,
                    false,   // case_sensitive
                    false,   // with_diacritics
                    -1,      // multiplicity: MainForm default
                    NumberType.None,
                    ComparisonOperator.Equal,
                    0);      // multiplicity_remainder
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("  query " + query.Name + " failed: " + ex.Message);
                continue;
            }

            List<Word> words = client.FoundWords;

            var verses = new List<int>();
            var seen = new HashSet<int>();
            if (words != null)
            {
                foreach (Word word in words)
                {
                    if (word != null && word.Verse != null && seen.Add(word.Verse.Number))
                    {
                        verses.Add(word.Verse.Number);
                    }
                }
            }
            verses.Sort();

            var firstFew = new StringBuilder();
            for (int i = 0; i < verses.Count && i < 5; i++)
            {
                if (i > 0) firstFew.Append(',');
                firstFew.Append(verses[i].ToString(CultureInfo.InvariantCulture));
            }

            summary.Append(query.Name).Append('\t')
                   .Append(query.Text).Append('\t')
                   .Append(query.Wordness).Append('\t')
                   .Append(matched.ToString(CultureInfo.InvariantCulture)).Append('\t')
                   .Append(verses.Count.ToString(CultureInfo.InvariantCulture)).Append('\t')
                   .Append(firstFew).Append(Lf);

            foreach (int verse in verses)
            {
                detail.Append(query.Name).Append('\t')
                      .Append(verse.ToString(CultureInfo.InvariantCulture)).Append(Lf);
            }

            Console.WriteLine("  query " + query.Name + " -> " + verses.Count + " verses");
        }

        File.WriteAllText(Path.Combine(outputDir, "search-summary.tsv"), summary.ToString(), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(outputDir, "search-verses.tsv"), detail.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote search-summary.tsv, search-verses.tsv");
    }
}
