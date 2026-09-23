using System;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

/// <summary>
/// Captures the "Base" letter-value systems, which value a word by reading its
/// letters' 0 and 1 values as the digits of a number in that base rather than
/// by adding them up.
/// </summary>
/// <remarks>
/// One row per chapter and one for the whole book (chapter 0), under every
/// Base system, plus the sample verses. Each system rebuilds the book in its
/// own text mode, as the UI does.
/// </remarks>
internal static class BaseSystems
{
    private const char Lf = '\n';

    private static readonly int[] SampleVerses = { 1, 2, 7, 8, 262, 1473, 6236 };

    public static void Dump(Client client, string outputDir)
    {
        var rows = new StringBuilder();
        rows.Append("# Values under the Base letter-value systems: per chapter (0 is the book),").Append(Lf);
        rows.Append("# then sample verses as verse:N. Default calculation mode.").Append(Lf);
        rows.Append("system\tunit\tvalue").Append(Lf);

        // Loading a system changes the loaded set, so take the names first.
        var names = new System.Collections.Generic.List<string>();
        foreach (string name in client.LoadedNumericalSystems.Keys)
        {
            if (name.Contains("_Base")) names.Add(name);
        }
        names.Sort(StringComparer.Ordinal);

        foreach (string system in names)
        {
            client.LoadNumericalSystem(system);
            client.BuildSimplifiedBook(client.NumericalSystem.TextMode, false, true, false, false, false, false, false, false, false);

            Book book = client.Book;
            foreach (Chapter chapter in book.Chapters)
            {
                Row(rows, system, chapter.Number.ToString(CultureInfo.InvariantCulture), client.CalculateValue(chapter));
            }
            Row(rows, system, "0", client.CalculateValue(book));
            foreach (int number in SampleVerses)
            {
                if (number > book.Verses.Count) continue;
                Row(rows, system, "verse:" + number.ToString(CultureInfo.InvariantCulture), client.CalculateValue(book.Verses[number - 1]));
            }
            Console.WriteLine("  base system " + system);
        }

        client.LoadNumericalSystem(NumericalSystem.DEFAULT_NAME);
        File.WriteAllText(Path.Combine(outputDir, "base-systems.tsv"), rows.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote base-systems.tsv");
    }

    private static void Row(StringBuilder rows, string system, string unit, long value)
    {
        rows.Append(system).Append('\t').Append(unit).Append('\t')
            .Append(value.ToString(CultureInfo.InvariantCulture)).Append(Lf);
    }
}
