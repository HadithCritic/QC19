namespace QuranCode.Core.Tests;

/// <summary>
/// Locates the golden data and the content database, and parses the TSV the
/// oracle emits.
/// </summary>
internal static class TestPaths
{
    private static readonly Lazy<string> NextRoot = new(FindNextRoot);

    /// <summary>Walks up from the test binary to the <c>next/</c> directory.</summary>
    private static string FindNextRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (directory.Name.Equals("next", StringComparison.OrdinalIgnoreCase) &&
                Directory.Exists(Path.Combine(directory.FullName, "tests", "golden")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException(
            $"could not locate the 'next' directory above {AppContext.BaseDirectory}");
    }

    public static string GoldenDirectory => Path.Combine(NextRoot.Value, "tests", "golden");

    /// <summary>
    /// The generated content database. It is not in version control, so the
    /// failure message says how to produce it rather than just reporting a
    /// missing file.
    /// </summary>
    public static string ContentDatabase
    {
        get
        {
            string path = Path.Combine(NextRoot.Value, "data", "content.db");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"content.db not found at {path}. Build it with:\n" +
                    "  python next/data/import/build_content.py <install-root> -o next/data/content.db",
                    path);
            }
            return path;
        }
    }

    /// <summary>
    /// Data rows of a golden TSV: comments and the header are skipped.
    /// </summary>
    public static IEnumerable<string[]> ReadRows(string fileName)
    {
        string path = Path.Combine(GoldenDirectory, fileName);
        bool headerSeen = false;

        foreach (string line in File.ReadLines(path))
        {
            if (line.Length == 0 || line[0] == '#') continue;
            if (!headerSeen) { headerSeen = true; continue; }
            yield return line.Split('\t');
        }
    }

    /// <summary>
    /// A two-column golden file read as a lookup, for the key/value dumps.
    /// </summary>
    public static Dictionary<string, string> ReadKeyValue(string fileName, string keyColumn)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string[] row in ReadRows(fileName))
        {
            if (row.Length >= 2) result[row[0]] = row[1];
        }
        return result;
    }
}
