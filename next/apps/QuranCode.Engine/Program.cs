using System.Text;
using QuranCode.Core;
using QuranCode.Engine.Host;

// qurancode-engine --content <path-to-content.db>
//
// Speaks newline-delimited JSON on stdin/stdout; see docs/decisions/0002.
// stderr carries diagnostics only and is never shown to the user verbatim.

var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
TextWriter log = new StreamWriter(Console.OpenStandardError(), utf8) { AutoFlush = true };

string? contentPath = null;
for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--content") contentPath = args[i + 1];
}

if (contentPath is null)
{
    log.WriteLine("usage: qurancode-engine --content <path-to-content.db>");
    return 2;
}

if (!File.Exists(contentPath))
{
    log.WriteLine($"content database not found: {contentPath}");
    return 3;
}

using var engine = new QuranCodeEngine(contentPath);
var dispatcher = new Dispatcher(new Handlers(engine), log);

using var input = new StreamReader(Console.OpenStandardInput(), utf8);
using var output = new StreamWriter(Console.OpenStandardOutput(), utf8) { AutoFlush = false };

return StdioHost.Run(input, output, dispatcher);
