using System.Diagnostics;
using System.Globalization;
using QuranCode.Core.Content;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;

// New-engine performance, measured the same way as the legacy baseline in
// next/tests/golden/performance-baseline.tsv so the two are comparable.
//
// Same machine, same steps, engine only, no UI on either side.

string databasePath = args.Length > 0
    ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "content.db");
databasePath = Path.GetFullPath(databasePath);

if (!File.Exists(databasePath))
{
    Console.Error.WriteLine($"content.db not found: {databasePath}");
    return 2;
}

Process process = Process.GetCurrentProcess();

void Record(string step, long ms, string note)
{
    process.Refresh();
    double workingSet = process.WorkingSet64 / 1024.0 / 1024.0;
    double heap = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
    Console.WriteLine(
        string.Format(CultureInfo.InvariantCulture,
            "  {0,-28} {1,7} ms   ws={2,6:F1} MB   heap={3,6:F1} MB   {4}",
            step, ms, workingSet, heap, note));
}

Record("baseline", 0, "before any engine work");

var stopwatch = Stopwatch.StartNew();
using var content = new ContentRepository(databasePath);
stopwatch.Stop();
Record("open_database", stopwatch.ElapsedMilliseconds, "lazy; nothing loaded yet");

stopwatch.Restart();
int chapterCount = content.Chapters.Count;
int verseCount = content.Verses.Count;
stopwatch.Stop();
Record("load_corpus", stopwatch.ElapsedMilliseconds,
    $"{chapterCount} chapters, {verseCount} verses");

// The legacy equivalent is "build book": 3,804 ms and 136 MB, because it
// materializes 327,792 Letter and 77,878 Word objects. Here the comparable work
// is normalizing the text, which is all valuation actually needs.
stopwatch.Restart();
TextMode mode = content.GetTextMode("Original");
var pipeline = new TextPipeline(mode);
stopwatch.Stop();
Record("load_text_mode", stopwatch.ElapsedMilliseconds, $"{mode.RuleCount} rules");

stopwatch.Restart();
ValueSystem system = content.GetValueSystem("Original_Alphabet_Primes1");
stopwatch.Stop();
Record("load_value_system", stopwatch.ElapsedMilliseconds, $"{system.LetterCount} letters");

var bookBuilder = new System.Text.StringBuilder(1 << 20);
foreach (Verse verse in content.Verses)
{
    if (bookBuilder.Length > 0) bookBuilder.Append('\n');
    bookBuilder.Append(verse.Text);
}
string rawBook = bookBuilder.ToString();

stopwatch.Restart();
string normalized = pipeline.Normalize(rawBook);
stopwatch.Stop();
Record("normalize_book", stopwatch.ElapsedMilliseconds, $"{normalized.Length} chars");

stopwatch.Restart();
long bookValue = ValueCalculator.Calculate(normalized, system);
stopwatch.Stop();
Record("value_whole_book", stopwatch.ElapsedMilliseconds, $"result={bookValue}");

stopwatch.Restart();
long checksum = 0;
foreach (Verse verse in content.Verses)
{
    checksum += ValueCalculator.Calculate(pipeline.Normalize(verse.Text), system);
}
stopwatch.Stop();
Record("value_all_verses", stopwatch.ElapsedMilliseconds,
    $"{verseCount} calls, checksum={checksum}");

// The legacy engine rebuilds the whole object graph to change system: 1,811 ms.
stopwatch.Restart();
ValueSystem other = content.GetValueSystem("Simplified29_Abjad_Gematria");
TextPipeline otherPipeline = new(content.GetTextMode("Simplified29"));
long otherValue = ValueCalculator.Calculate(otherPipeline.Normalize(rawBook), other);
stopwatch.Stop();
Record("switch_value_system", stopwatch.ElapsedMilliseconds, $"result={otherValue}");

Console.WriteLine();
Console.WriteLine($"  content.db  {new FileInfo(databasePath).Length / 1024.0 / 1024.0:F1} MB");
return 0;
