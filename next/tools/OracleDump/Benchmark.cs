using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Model;

// Legacy performance baseline.
//
// The modernization brief requires measured before/after numbers rather than
// claims, so this records what the existing engine actually costs. It measures
// the engine only: no WinForms, no splash screen, no control creation. Real
// application startup is therefore at least this, never less.
internal static class Benchmark
{
    private const string Lf = "\n";

    public static void Run(string installRoot, string outputDir)
    {
        var sb = new StringBuilder();
        sb.Append("# Legacy engine performance baseline").Append(Lf);
        sb.Append("# Machine-specific. Re-record before comparing against a new implementation.").Append(Lf);
        sb.Append("# Engine only: excludes all WinForms UI construction.").Append(Lf);
        sb.Append("host\t").Append(Environment.MachineName).Append(Lf);
        sb.Append("os\t").Append(Environment.OSVersion.VersionString).Append(Lf);
        sb.Append("clr\t").Append(Environment.Version).Append(Lf);
        sb.Append("cpu_count\t").Append(Environment.ProcessorCount).Append(Lf);
        sb.Append("captured_utc\t").Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(Lf);
        sb.Append(Lf);
        sb.Append("step\tms\tworking_set_mb\tmanaged_heap_mb\tnote").Append(Lf);

        var process = Process.GetCurrentProcess();
        Action<string, long, string> record = (step, ms, note) =>
        {
            process.Refresh();
            double ws = process.WorkingSet64 / 1024.0 / 1024.0;
            double heap = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            sb.Append(step).Append('\t')
              .Append(ms.ToString(CultureInfo.InvariantCulture)).Append('\t')
              .Append(ws.ToString("F1", CultureInfo.InvariantCulture)).Append('\t')
              .Append(heap.ToString("F1", CultureInfo.InvariantCulture)).Append('\t')
              .Append(note).Append(Lf);
            Console.WriteLine("  {0,-28} {1,7} ms   ws={2,6:F1} MB   heap={3,6:F1} MB   {4}", step, ms, ws, heap, note);
        };

        record("baseline", 0, "before any engine work");

        var sw = Stopwatch.StartNew();
        var client = new Client(NumericalSystem.DEFAULT_NAME);
        sw.Stop();
        record("client_construct", sw.ElapsedMilliseconds,
            "loads " + (client.LoadedNumericalSystems == null ? 0 : client.LoadedNumericalSystems.Count) + " numerical systems");

        sw.Restart();
        Program.BuildBookPublic(client, client.NumericalSystem.TextMode);
        sw.Stop();
        Book book = client.Book;
        record("build_book", sw.ElapsedMilliseconds,
            book.Verses.Count + " verses, " + book.Words.Count + " words, " + book.Letters.Count + " letters");

        // Rebuilding is what happens on every text-mode or option change in the UI.
        sw.Restart();
        Program.BuildBookPublic(client, client.NumericalSystem.TextMode);
        sw.Stop();
        record("rebuild_book", sw.ElapsedMilliseconds, "same parameters; no caching in the legacy engine");

        sw.Restart();
        long bookValue = client.CalculateValue(book);
        sw.Stop();
        record("value_whole_book", sw.ElapsedMilliseconds, "result=" + bookValue.ToString(CultureInfo.InvariantCulture));

        // Per-verse valuation over the whole book: the shape of a bulk research scan.
        sw.Restart();
        long checksum = 0;
        foreach (Verse verse in book.Verses) checksum += client.CalculateValue(verse);
        sw.Stop();
        record("value_all_verses", sw.ElapsedMilliseconds, "6236 calls, checksum=" + checksum.ToString(CultureInfo.InvariantCulture));

        // Switching numerical systems forces a full book rebuild in the legacy engine.
        sw.Restart();
        client.LoadNumericalSystem("Simplified29_Abjad_Gematria");
        Program.BuildBookPublic(client, client.NumericalSystem.TextMode);
        sw.Stop();
        record("switch_numerical_system", sw.ElapsedMilliseconds, "load + full rebuild");

        // Disk footprint of the shipped install, for the distribution comparison.
        sb.Append(Lf);
        sb.Append("directory\tbytes\tfiles").Append(Lf);
        foreach (string name in new[] { "Numbers", "Translations", "Help", "Data", "Values", "Rules", "Audio", "Images", "Fonts" })
        {
            string path = Path.Combine(installRoot, name);
            if (!Directory.Exists(path)) continue;
            long bytes = 0;
            int files = 0;
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                bytes += new FileInfo(file).Length;
                files++;
            }
            sb.Append(name).Append('\t')
              .Append(bytes.ToString(CultureInfo.InvariantCulture)).Append('\t')
              .Append(files.ToString(CultureInfo.InvariantCulture)).Append(Lf);
        }

        string outPath = Path.Combine(outputDir, "performance-baseline.tsv");
        File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine("  wrote " + Path.GetFileName(outPath));
    }
}
