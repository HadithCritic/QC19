using Microsoft.Data.Sqlite;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;

namespace QuranCode.Core.Content;

/// <summary>A chapter, as a value rather than an object with back-references.</summary>
public readonly record struct Chapter(
    int Number,
    string Name,
    string TransliteratedName,
    string EnglishName,
    int RevelationOrder,
    string RevelationPlace,
    int VerseCount,
    int FirstVerse);

/// <summary>A verse, carrying its canonical text and nothing derived.</summary>
public readonly record struct Verse(
    int Number,
    int ChapterNumber,
    int NumberInChapter,
    string Text);

/// <summary>
/// Read access to <c>content.db</c>.
/// </summary>
/// <remarks>
/// Replaces the legacy <c>DataAccess</c> plus the static <c>Server.Book</c>
/// graph. The measured problem it exists to solve:
///
/// <code>
/// legacy build of the object graph   3,804 ms   136 MB heap
/// legacy valuation over that graph      34 ms
/// </code>
///
/// <para>
/// So this type never builds a graph. Chapters and verses are small enough to
/// hold as flat arrays (114 and 6,236 records); words and letters are derived
/// on demand from verse text and the active text mode, because materializing
/// 327,792 letter objects is precisely the cost being removed.
/// </para>
///
/// <para>
/// Loading is lazy per category, so opening the database does almost no work and
/// startup does not wait for data a given screen will never ask for (§15).
/// </para>
/// </remarks>
public sealed class ContentRepository : IDisposable
{
    private readonly SqliteConnection _connection;

    private Chapter[]? _chapters;
    private Verse[]? _verses;
    private readonly Dictionary<string, TextMode> _textModes = [];
    private readonly Dictionary<string, ValueSystem> _valueSystems = [];

    public ContentRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(databasePath);
        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException($"content database not found: {databasePath}", databasePath);
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared,
        };
        _connection = new SqliteConnection(builder.ToString());
        _connection.Open();
    }

    /// <summary>All 114 chapters, loaded on first use.</summary>
    public IReadOnlyList<Chapter> Chapters => _chapters ??= LoadChapters();

    /// <summary>All 6,236 verses in canonical order, loaded on first use.</summary>
    public IReadOnlyList<Verse> Verses => _verses ??= LoadVerses();

    /// <summary>Verses of one chapter, without loading the rest.</summary>
    public ReadOnlySpan<Verse> VersesOf(int chapterNumber)
    {
        Chapter chapter = Chapters[chapterNumber - 1];
        return Verses.ToArray().AsSpan(chapter.FirstVerse - 1, chapter.VerseCount);
    }

    private Chapter[] LoadChapters()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT number, name, transliterated_name, english_name,
                   revelation_order, revelation_place, verse_count, first_verse
            FROM chapters ORDER BY number
            """;

        var result = new List<Chapter>(114);
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Chapter(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetInt32(4), reader.GetString(5),
                reader.GetInt32(6), reader.GetInt32(7)));
        }
        return [.. result];
    }

    private Verse[] LoadVerses()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT number, chapter_number, number_in_chapter, text FROM verses ORDER BY number";

        var result = new List<Verse>(6236);
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Verse(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3)));
        }
        return [.. result];
    }

    /// <summary>
    /// Loads a text mode and its ordered rules. Cached after first use.
    /// </summary>
    public TextMode GetTextMode(string name, int wordCountMethod = 77878)
    {
        string key = $"{name}/{wordCountMethod}";
        if (_textModes.TryGetValue(key, out TextMode? cached)) return cached;

        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT r.find, r.replace_with
            FROM text_mode_rules r
            JOIN text_modes m ON m.id = r.text_mode_id
            WHERE m.name = $name AND m.word_count_method = $method
            ORDER BY r.ordinal
            """;
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$method", wordCountMethod);

        var rules = new List<(string, string)>();
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read()) rules.Add((reader.GetString(0), reader.GetString(1)));
        }

        if (rules.Count == 0)
        {
            throw new InvalidOperationException($"text mode not found: {name} ({wordCountMethod})");
        }

        var mode = new TextMode(name, wordCountMethod, rules);
        _textModes[key] = mode;
        return mode;
    }

    /// <summary>
    /// Loads a value system and its letter map. Cached after first use.
    /// </summary>
    public ValueSystem GetValueSystem(string name)
    {
        if (_valueSystems.TryGetValue(name, out ValueSystem? cached)) return cached;

        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT s.text_mode_name, s.letter_order, s.letter_value, m.letter, m.value
            FROM value_systems s
            JOIN value_map m ON m.value_system_id = s.id
            WHERE s.name = $name
            """;
        command.Parameters.AddWithValue("$name", name);

        string textMode = "", order = "", value = "";
        var values = new Dictionary<char, long>();
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                textMode = reader.GetString(0);
                order = reader.GetString(1);
                value = reader.GetString(2);
                string letter = reader.GetString(3);
                if (letter.Length == 1) values[letter[0]] = reader.GetInt64(4);
            }
        }

        if (values.Count == 0)
        {
            throw new InvalidOperationException($"value system not found: {name}");
        }

        var system = new ValueSystem(name, textMode, order, value, values);
        _valueSystems[name] = system;
        return system;
    }

    /// <summary>Names of every installed value system.</summary>
    public IReadOnlyList<string> ValueSystemNames()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT name FROM value_systems ORDER BY name";

        var result = new List<string>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read()) result.Add(reader.GetString(0));
        return result;
    }

    public void Dispose() => _connection.Dispose();
}
