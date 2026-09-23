using System.Text;
using QuranCode.Core;
using QuranCode.Core.Numbers;
using QuranCode.Core.User;
using QuranCode.Engine.Host;

// qurancode-engine --content <path-to-content.db>
//
// Speaks newline-delimited JSON on stdin/stdout; see docs/decisions/0002.
// stderr carries diagnostics only and is never shown to the user verbatim.

var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
// Synchronized: the background warm-up and the request loop may both log.
TextWriter log = TextWriter.Synchronized(new StreamWriter(Console.OpenStandardError(), utf8) { AutoFlush = true });

string? contentPath = null;
string? userPath = null;
for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--content") contentPath = args[i + 1];
    if (args[i] == "--user") userPath = args[i + 1];
}

if (contentPath is null)
{
    log.WriteLine("usage: qurancode-engine --content <path-to-content.db> [--user <path-to-user.db>]");
    return 2;
}

if (!File.Exists(contentPath))
{
    log.WriteLine($"content database not found: {contentPath}");
    return 3;
}

using var engine = new QuranCodeEngine(contentPath);


// The reader's own data. Optional: without it the engine still serves the
// text, and bookmark and history methods say they are unavailable.
UserStore? store = null;
if (userPath is not null)
{
    try
    {
        store = new UserStore(userPath);
    }
    catch (Exception ex)
    {
        log.WriteLine($"user data unavailable: {ex.Message}");
    }
}
using UserStore? ownedStore = store;
var dispatcher = new Dispatcher(new Handlers(engine), store is null ? null : new UserHandlers(engine, store), log);

// Build the number index in the background: classifying a whole-book value
// needs it (about 160 ms the first time), and NumberTheory is thread-safe.
// The engine's own caches are not, so nothing else is warmed off the request
// loop, and the loop starts at once.
if (!args.Contains("--no-warmup"))
{
    _ = Task.Run(() =>
    {
        try
        {
            NumberTheory.CountUpTo(NumberClass.AdditiveComposite, WarmNumberLimit);
        }
        catch (Exception ex)
        {
            // Only means the first request builds it instead.
            log.WriteLine($"warm-up failed: {ex.Message}");
        }
    });
}

using var input = new StreamReader(Console.OpenStandardInput(), utf8);
using var output = new StreamWriter(Console.OpenStandardOutput(), utf8) { AutoFlush = false };

return StdioHost.Run(input, output, dispatcher);

internal static partial class Program
{
    /// <summary>
    /// Covers whole-book values under the shipped systems (about 19.6 to 24
    /// million); larger values extend the index when first asked for.
    /// </summary>
    private const long WarmNumberLimit = 30_000_000;
}
