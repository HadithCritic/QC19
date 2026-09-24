using System.Globalization;
using QuranCode.Core;
using QuranCode.Core.Content;
using QuranCode.Core.Search;
using QuranCode.Core.Text;

// QuranCode command line.
//
// Brief §42: the same engine the desktop uses, driven from a shell, so research
// can be scripted and repeated. It holds no logic of its own; every command is a
// thin call into QuranCodeEngine.
//
// Output is tab-separated and culture-invariant so it pipes into other tools
// without surprises.

const string Usage = """
qurancode - Quran text and numerology research

USAGE
  qurancode <command> [options]

COMMANDS
  verse <ref>              Show a verse. <ref> is 2:255 or an absolute number.
  chapter <n>              Show a chapter's verses.
  search <term>            Find words. --whole or --part to filter.
  value <text>             Value arbitrary text.
  value-verse <ref>        Value a verse.
  value-chapter <n>        Value a chapter.
  value-book               Value the whole book.
  systems                  List installed value systems.
  chapters                 List chapters.
  stats                    Corpus totals.
  letters <chars>          How often each of the letters occurs in every verse,
                           counting the Basmalahs, in --mode (default Original).

OPTIONS
  --db <path>              content.db (default: ./content.db or ../data/content.db)
  --system <name>          Value system (default: Original_Alphabet_Primes1)
  --mode <name>            Text mode (default: Original)
  --whole                  Search: match whole words only
  --part                   Search: match only inside longer words
  --hamza                  Letters: keep the hamza above a line (Khalifa's alif
                           is then letters اء --mode Simplified29 --hamza)
  --limit <n>              Cap rows printed (default: 50, 0 for all)

EXAMPLES
  qurancode value-chapter 1
  qurancode search الله --whole --limit 10
  qurancode verse 2:255
""";

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    Console.WriteLine(Usage);
    return 0;
}

string command = args[0];
var positional = new List<string>();
var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

for (int i = 1; i < args.Length; i++)
{
    if (!args[i].StartsWith("--", StringComparison.Ordinal))
    {
        positional.Add(args[i]);
        continue;
    }

    string key = args[i][2..];
    // Flags take no value; everything else consumes the next argument.
    if (key is "whole" or "part" or "hamza")
    {
        options[key] = "true";
    }
    else if (i + 1 < args.Length)
    {
        options[key] = args[++i];
    }
}

string Option(string key, string fallback) =>
    options.TryGetValue(key, out string? value) ? value : fallback;

int limit = int.TryParse(Option("limit", "50"), out int parsed) ? parsed : 50;
string valueSystem = Option("system", QuranCodeEngine.DefaultValueSystem);
string textMode = Option("mode", QuranCodeEngine.DefaultTextMode);

string database = Option("db", "");
if (database.Length == 0)
{
    foreach (string candidate in new[]
    {
        Path.Combine(Environment.CurrentDirectory, "content.db"),
        Path.Combine(AppContext.BaseDirectory, "content.db"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "content.db"),
    })
    {
        if (File.Exists(candidate)) { database = Path.GetFullPath(candidate); break; }
    }
}

if (database.Length == 0 || !File.Exists(database))
{
    Console.Error.WriteLine("content.db not found. Pass --db <path>, or build it with:");
    Console.Error.WriteLine("  python next/data/import/build_content.py -o next/data/content.db");
    return 2;
}

string N(long value) => value.ToString(CultureInfo.InvariantCulture);

try
{
    using var engine = new QuranCodeEngine(database);

    switch (command)
    {
        case "verse":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("verse: need a reference"); return 2; }
            Verse verse = Resolve(engine, positional[0]);
            Console.WriteLine($"{verse.ChapterNumber}:{verse.NumberInChapter}\t{verse.Text}");
            Console.WriteLine($"value\t{N((engine.ValueOfVerse(verse.Number, valueSystem, textMode) ?? throw new InvalidOperationException("that verse is not counted")))}");
            break;
        }

        case "chapter":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("chapter: need a number"); return 2; }
            int number = int.Parse(positional[0]);
            Chapter chapter = engine.Chapters[number - 1];
            Console.WriteLine($"# {chapter.Number} {chapter.Name} ({chapter.TransliteratedName}) - {chapter.EnglishName}");
            Console.WriteLine($"# {chapter.VerseCount} verses, revealed in {chapter.RevelationPlace}, order {chapter.RevelationOrder}");

            int shown = 0;
            for (int i = 0; i < chapter.VerseCount; i++)
            {
                if (limit > 0 && shown++ >= limit) { Console.WriteLine($"... {chapter.VerseCount - limit} more"); break; }
                Verse verse = engine.Verses[chapter.FirstVerse - 1 + i];
                Console.WriteLine($"{chapter.Number}:{verse.NumberInChapter}\t{verse.Text}");
            }
            break;
        }

        case "search":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("search: need a term"); return 2; }
            string term = string.Join(' ', positional);

            Wordness wordness =
                options.ContainsKey("whole") ? Wordness.WholeWord :
                options.ContainsKey("part") ? Wordness.PartOfWord : Wordness.Any;

            SearchResult result = engine.Search(textMode).Find(term, wordness);
            Console.WriteLine($"# {result.WordCount} words in {result.VerseCount} verses");

            int shown = 0;
            foreach (int verseNumber in result.Verses)
            {
                if (limit > 0 && shown++ >= limit)
                {
                    Console.WriteLine($"... {result.VerseCount - limit} more verses");
                    break;
                }
                Verse verse = engine.Verse(verseNumber);
                Console.WriteLine($"{verse.ChapterNumber}:{verse.NumberInChapter}\t{verse.Text}");
            }
            break;
        }

        case "value":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("value: need text"); return 2; }
            string text = string.Join(' ', positional);
            Console.WriteLine(N(engine.Value(text, valueSystem, textMode)));
            break;
        }

        case "value-verse":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("value-verse: need a reference"); return 2; }
            Verse verse = Resolve(engine, positional[0]);
            Console.WriteLine(N((engine.ValueOfVerse(verse.Number, valueSystem, textMode) ?? throw new InvalidOperationException("that verse is not counted"))));
            break;
        }

        case "value-chapter":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("value-chapter: need a number"); return 2; }
            Console.WriteLine(N(engine.ValueOfChapter(int.Parse(positional[0]), valueSystem, textMode)));
            break;
        }

        case "value-book":
            Console.WriteLine(N(engine.ValueOfBook(valueSystem, textMode)));
            break;

        case "systems":
        {
            IReadOnlyList<string> systems = engine.ValueSystems();
            Console.WriteLine($"# {systems.Count} value systems");
            int shown = 0;
            foreach (string name in systems)
            {
                if (limit > 0 && shown++ >= limit) { Console.WriteLine($"... {systems.Count - limit} more"); break; }
                Console.WriteLine(name);
            }
            break;
        }

        case "chapters":
            Console.WriteLine("chapter\tname\ttransliterated\tenglish\tverses\tplace\torder");
            foreach (Chapter chapter in engine.Chapters)
            {
                Console.WriteLine(
                    $"{chapter.Number}\t{chapter.Name}\t{chapter.TransliteratedName}\t" +
                    $"{chapter.EnglishName}\t{chapter.VerseCount}\t{chapter.RevelationPlace}\t{chapter.RevelationOrder}");
            }
            break;

        case "stats":
        {
            var segmentation = engine.Segmentation(textMode);
            Console.WriteLine($"text_mode\t{textMode}");
            Console.WriteLine($"value_system\t{valueSystem}");
            Console.WriteLine($"chapters\t{N(engine.Chapters.Count)}");
            Console.WriteLine($"verses\t{N(segmentation.VerseCount)}");
            Console.WriteLine($"words\t{N(segmentation.WordCount)}");
            Console.WriteLine($"letters\t{N(segmentation.LetterCount)}");
            Console.WriteLine($"book_value\t{N(engine.ValueOfBook(valueSystem, textMode))}");
            break;
        }

        case "letters":
        {
            if (positional.Count == 0) { Console.Error.WriteLine("letters needs the letters to count, e.g. letters اء"); return 2; }
            char[] letters = [.. positional[0].Distinct()];
            var segmentation = engine.Segmentation(textMode, new CountingOptions
            {
                IncludeBasmalas = true,
                HamzaAboveLine = options.ContainsKey("hamza"),
            });
            Console.WriteLine("chapter\tverse\t" + string.Join('\t', letters));
            for (int v = 0; v < segmentation.VerseCount; v++)
            {
                string text = string.Concat(segmentation.VerseWords(v));
                Console.WriteLine(
                    $"{segmentation.VerseChapter[v]}\t{segmentation.VerseNumberInChapter[v]}\t" +
                    string.Join('\t', letters.Select(l => N(text.Count(c => c == l)))));
            }
            break;
        }

        default:
            Console.Error.WriteLine($"unknown command: {command}");
            Console.Error.WriteLine(Usage);
            return 2;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

return 0;

static Verse Resolve(QuranCodeEngine engine, string reference)
{
    // "2:255" or "2:0" is a reference; a bare number is an absolute verse number.
    if (!reference.Contains(':')) return engine.Verse(int.Parse(reference));

    ReferenceParseResult parsed = engine.ParseReference(reference);
    if (!parsed.IsSuccess) throw new ArgumentException(parsed.Error);
    return engine.Verse(parsed.Range.First);
}
