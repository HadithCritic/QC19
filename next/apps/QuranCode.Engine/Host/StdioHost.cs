namespace QuranCode.Engine.Host;

/// <summary>
/// Reads requests from stdin and writes responses to stdout, one per line,
/// until stdin closes.
/// </summary>
/// <remarks>
/// Requests are handled in order. Whole-book valuation and search take
/// milliseconds, so a queue costs nothing a user can perceive and is simpler to
/// reason about than concurrent access to the engine's caches. The protocol
/// carries ids, so concurrency can be added later without changing the format.
/// </remarks>
internal static class StdioHost
{
    /// <summary>
    /// Longest request line processed. Well above any legitimate request (a
    /// valuation text is capped at 20,000 characters). Longer lines are
    /// rejected without being parsed. The only writer is the app's own Rust
    /// bridge, so this guards against bugs, not an adversary.
    /// </summary>
    public const int MaxLineLength = 1 << 20;

    public static int Run(TextReader input, TextWriter output, Dispatcher dispatcher)
    {
        while (input.ReadLine() is { } line)
        {
            if (line.Length == 0) continue;

            string response = line.Length > MaxLineLength
                ? """{"id":null,"error":{"code":"parse_error","message":"The request is too long."}}"""
                : dispatcher.Handle(line);

            output.WriteLine(response);
            output.Flush();
        }
        return 0;
    }
}
