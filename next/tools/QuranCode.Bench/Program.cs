using System.Diagnostics;
using System.Globalization;
using QuranCode.Core;
using QuranCode.Core.Analysis;
using QuranCode.Core.Content;
using QuranCode.Core.Numbers;
using QuranCode.Core.Search;

// Engine performance through the calls the app actually makes, measured the
// same way as the legacy baseline in next/tests/golden/performance-baseline.tsv
// where a legacy equivalent exists. Engine only, no UI.
//
// usage: QuranCode.Bench [content.db] [--repeat N]

string databasePath = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal))
    ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "content.db");
databasePath = Path.GetFullPath(databasePath);
int repeat = args.SkipWhile(a => a != "--repeat").Skip(1).Select(int.Parse).FirstOrDefault(5);

if (!File.Exists(databasePath))
{
    Console.Error.WriteLine($"content database not found: {databasePath}");
    return 2;
}

Process process = Process.GetCurrentProcess();

void Record(string step, double ms, string note)
{
    process.Refresh();
    Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
        "  {0,-30} {1,9:F1} ms   ws={2,6:F1} MB   {3}", step, ms, process.WorkingSet64 / 1048576.0, note));
}

// Runs once cold, then `repeat` times warm, and reports the cold time and the
// best warm time: the first is what a user waits for, the second the steady state.
(double Cold, double Warm, T Result) Time<T>(Func<T> work)
{
    var watch = Stopwatch.StartNew();
    T result = work();
    double cold = watch.Elapsed.TotalMilliseconds;
    double warm = double.MaxValue;
    for (int i = 0; i < repeat; i++)
    {
        watch.Restart();
        work();
        warm = Math.Min(warm, watch.Elapsed.TotalMilliseconds);
    }
    return (cold, warm, result);
}

void Report<T>(string step, Func<T> work, Func<T, string> note)
{
    var (cold, warm, result) = Time(work);
    Record(step + " (cold)", cold, note(result));
    Record(step + " (warm)", warm, "");
}

Console.WriteLine($"database: {databasePath}");
Record("baseline", 0, "");

var watch = Stopwatch.StartNew();
using var engine = new QuranCodeEngine(databasePath);
int verses = engine.Verses.Count;
Record("open_engine_and_corpus", watch.Elapsed.TotalMilliseconds, $"{engine.Chapters.Count} chapters, {verses} verses");

// What the first request pays: the default text mode's segmentation.
watch.Restart();
Segmentation segmentation = engine.Segmentation();
Record("first_segmentation", watch.Elapsed.TotalMilliseconds,
    $"{segmentation.WordCount} words, {segmentation.LetterCount} letters");

Report("value_whole_book", () => engine.ValueOfBook(), v => $"result={v}");
Report("value_all_verses", () =>
{
    long sum = 0;
    for (int v = 1; v <= verses; v++) sum += engine.ValueOfVerse(v) ?? 0;
    return sum;
}, v => $"checksum={v}");

var book = new VerseRange(1, verses);
Report("stats_whole_book", () => engine.Statistics(book), s => $"value={s.Value}");
Report("stats_chapter_2", () => engine.Statistics(new VerseRange(engine.Chapters[1].FirstVerse, engine.Chapters[1].LastVerse)), s => $"value={s.Value}");

// What selection.stats adds on top: analysing six numbers, one of them the
// whole-book value, which needs its position among all composites.
long bookValue = engine.ValueOfBook();
Report("analyze_book_value", () => NumberAnalysis.Of(bookValue), a => $"{a.Code} ordinal={a.ClassOrdinal}");
Report("analyze_small_counts", () =>
{
    long x = 0;
    foreach (long n in new long[] { 114, verses, 77878, 327792, 29 }) x += NumberAnalysis.Of(n).DigitSum;
    return x;
}, _ => "5 counts");

// chapter.values for the longest chapter: 286 verse values and classes.
Report("chapter_values_2", () =>
{
    Chapter c = engine.Chapters[1];
    int count = 0;
    for (int v = c.FirstVerse; v <= c.LastVerse; v++)
    {
        long value = engine.ValueOfVerse(v) ?? 0;
        count += (int)NumberTheory.Classify(value);
    }
    return count;
}, _ => "286 verses classified");

// text.values for the Values view: a verse-length text in every visible system.
string sample = engine.Verse(2, 255).Text;
string[] systems = engine.ValueSystemSummaries().Where(s => !s.ResearchOnly).Select(s => s.Name).ToArray();
var modeOf = engine.ValueSystemSummaries().ToDictionary(s => s.Name, s => s.TextMode);
Report("text_values_all_systems", () =>
{
    long sum = 0;
    foreach (string name in systems) sum += NumberAnalysis.Of(engine.Value(sample, name, modeOf[name])).DigitSum;
    return sum;
}, _ => $"{systems.Length} systems, ayat al-kursi");

Report("search_allah_any", () => engine.Search().Find("الله", Wordness.Any), r => $"{r.WordCount} words");
Report("search_rare_whole", () => engine.Search().Find("بعوضه", Wordness.WholeWord), r => $"{r.WordCount} words");

watch.Restart();
engine.Statistics(book, "Simplified29_Abjad_Gematria");
Record("switch_value_system", watch.Elapsed.TotalMilliseconds, "first use of Simplified29");

process.Refresh();
Console.WriteLine();
Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  peak working set {0:F1} MB", process.PeakWorkingSet64 / 1048576.0));
return 0;
