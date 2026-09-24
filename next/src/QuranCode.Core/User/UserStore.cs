using System.Globalization;
using Microsoft.Data.Sqlite;
using QuranCode.Core.Text;

namespace QuranCode.Core.User;

/// <summary>A verse by chapter and number in chapter, which every edition understands.</summary>
public readonly record struct VerseRef(int Chapter, int Verse);

public sealed record Bookmark(long Id, VerseRef First, VerseRef Last, string Note, DateTime CreatedUtc, DateTime UpdatedUtc);

public enum HistoryKind
{
    /// <summary>A selection the reader opened.</summary>
    Browse,

    /// <summary>A search the reader ran.</summary>
    Find,
}

/// <summary>
/// An exact selection kept for research, with the settings it was studied
/// under, so a result can be reproduced as it was found.
/// </summary>
/// <param name="Address">The canonical selection address, as in 2:255:w4:l2-2:257:w8.</param>
/// <param name="ValueSystem">The value system it was studied in, or null.</param>
/// <param name="Counting">The counting options it was studied with, or null.</param>
public sealed record ResearchSelection(
    long Id, string Title, string Note, string Address, string? ValueSystem, CountingOptions? Counting,
    DateTime CreatedUtc, DateTime UpdatedUtc);

public sealed record HistoryEntry(long Id, HistoryKind Kind, VerseRef? First, VerseRef? Last, string? Term, string? Wordness, DateTime AtUtc);

/// <summary>
/// The reader's own data: bookmarks with notes, browse and find history, the
/// text modes the reader defined, and saved research selections, kept in
/// <c>user.db</c>, apart from the read-only content.
/// </summary>
/// <remarks>
/// <para>
/// Features.txt #68 to #70, and #72. Positions are stored as chapter:verse rather than
/// absolute verse numbers, so a bookmark survives switching between editions
/// whose absolute numbering differs.
/// </para>
/// <para>
/// The schema is versioned and only ever extended by additive migrations; a
/// newer file is refused rather than altered, so no user data is destroyed.
/// </para>
/// </remarks>
public sealed class UserStore : IDisposable
{
    public const int SchemaVersion = 3;
    public const int MaxTitleLength = 200;
    public const int MaxNoteLength = 10_000;
    public const int MaxTermLength = 500;

    /// <summary>History kept per kind; the oldest entries beyond this are dropped.</summary>
    public const int HistoryLimit = 500;

    private static readonly string[] Migrations =
    [
        """
        CREATE TABLE bookmarks (
            id            INTEGER PRIMARY KEY,
            first_chapter INTEGER NOT NULL,
            first_verse   INTEGER NOT NULL,
            last_chapter  INTEGER NOT NULL,
            last_verse    INTEGER NOT NULL,
            note          TEXT    NOT NULL DEFAULT '',
            created_utc   TEXT    NOT NULL,
            updated_utc   TEXT    NOT NULL,
            UNIQUE (first_chapter, first_verse, last_chapter, last_verse)
        );
        CREATE TABLE history (
            id            INTEGER PRIMARY KEY,
            kind          TEXT    NOT NULL CHECK (kind IN ('browse', 'find')),
            first_chapter INTEGER,
            first_verse   INTEGER,
            last_chapter  INTEGER,
            last_verse    INTEGER,
            term          TEXT,
            wordness      TEXT,
            at_utc        TEXT    NOT NULL
        );
        CREATE INDEX history_kind_id ON history (kind, id);
        """,
        """
        CREATE TABLE text_modes (
            name        TEXT PRIMARY KEY,
            base        TEXT NOT NULL,
            description TEXT NOT NULL DEFAULT '',
            updated_utc TEXT NOT NULL
        );
        CREATE TABLE text_mode_rules (
            mode     TEXT    NOT NULL,
            position INTEGER NOT NULL,
            find     TEXT    NOT NULL,
            replace  TEXT    NOT NULL,
            PRIMARY KEY (mode, position)
        );
        """,
        """
        CREATE TABLE research_selections (
            id           INTEGER PRIMARY KEY,
            title        TEXT    NOT NULL,
            note         TEXT    NOT NULL DEFAULT '',
            address      TEXT    NOT NULL,
            value_system TEXT,
            counting     TEXT,
            created_utc  TEXT    NOT NULL,
            updated_utc  TEXT    NOT NULL
        );
        """,
    ];

    private readonly SqliteConnection _connection;

    public UserStore(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var builder = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate };
        _connection = new SqliteConnection(builder.ToString());
        _connection.Open();
        Execute("PRAGMA journal_mode = WAL");
        Migrate();
    }

    private void Migrate()
    {
        Execute("CREATE TABLE IF NOT EXISTS user_schema_version (version INTEGER NOT NULL)");
        long current = Scalar("SELECT COALESCE(MAX(version), 0) FROM user_schema_version");
        if (current > SchemaVersion)
        {
            _connection.Dispose();
            throw new InvalidOperationException(
                $"user.db was written by a newer version of QuranCode (schema {current}); it is left untouched.");
        }

        for (long version = current + 1; version <= SchemaVersion; version++)
        {
            using SqliteTransaction transaction = _connection.BeginTransaction();
            Execute(Migrations[version - 1], transaction);
            Execute($"INSERT INTO user_schema_version (version) VALUES ({version})", transaction);
            transaction.Commit();
        }
    }

    // -- bookmarks ------------------------------------------------------

    public IReadOnlyList<Bookmark> Bookmarks()
    {
        using SqliteCommand command = Command(
            "SELECT id, first_chapter, first_verse, last_chapter, last_verse, note, created_utc, updated_utc " +
            "FROM bookmarks ORDER BY first_chapter, first_verse, last_chapter, last_verse");
        var result = new List<Bookmark>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Bookmark(
                reader.GetInt64(0), new VerseRef(reader.GetInt32(1), reader.GetInt32(2)),
                new VerseRef(reader.GetInt32(3), reader.GetInt32(4)), reader.GetString(5),
                ParseTime(reader.GetString(6)), ParseTime(reader.GetString(7))));
        }
        return result;
    }

    /// <summary>Adds a bookmark, or replaces the note of the one on the same range.</summary>
    public Bookmark SaveBookmark(VerseRef first, VerseRef last, string note)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (note.Length > MaxNoteLength) throw new ArgumentException($"A note is at most {MaxNoteLength:N0} characters.", nameof(note));

        string now = Now();
        using SqliteCommand command = Command(
            """
            INSERT INTO bookmarks (first_chapter, first_verse, last_chapter, last_verse, note, created_utc, updated_utc)
            VALUES ($fc, $fv, $lc, $lv, $note, $now, $now)
            ON CONFLICT (first_chapter, first_verse, last_chapter, last_verse)
            DO UPDATE SET note = excluded.note, updated_utc = excluded.updated_utc
            RETURNING id, created_utc
            """);
        AddRange(command, first, last);
        command.Parameters.AddWithValue("$note", note);
        command.Parameters.AddWithValue("$now", now);

        using SqliteDataReader reader = command.ExecuteReader();
        reader.Read();
        return new Bookmark(reader.GetInt64(0), first, last, note, ParseTime(reader.GetString(1)), ParseTime(now));
    }

    /// <summary>Removes a bookmark; false when there was none with that id.</summary>
    public bool DeleteBookmark(long id)
    {
        using SqliteCommand command = Command("DELETE FROM bookmarks WHERE id = $id");
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery() > 0;
    }

    // -- history --------------------------------------------------------

    /// <summary>Records a selection, unless it repeats the latest one.</summary>
    public void AddBrowse(VerseRef first, VerseRef last)
    {
        HistoryEntry? latest = History(HistoryKind.Browse, 1).FirstOrDefault();
        if (latest is not null && latest.First == first && latest.Last == last) return;

        using SqliteCommand command = Command(
            "INSERT INTO history (kind, first_chapter, first_verse, last_chapter, last_verse, at_utc) " +
            "VALUES ('browse', $fc, $fv, $lc, $lv, $now)");
        AddRange(command, first, last);
        command.Parameters.AddWithValue("$now", Now());
        command.ExecuteNonQuery();
        Trim(HistoryKind.Browse);
    }

    /// <summary>Records a search, unless it repeats the latest one.</summary>
    public void AddFind(string term, string wordness)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(term);
        if (term.Length > MaxTermLength) throw new ArgumentException($"A search is at most {MaxTermLength} characters.", nameof(term));

        HistoryEntry? latest = History(HistoryKind.Find, 1).FirstOrDefault();
        if (latest is not null && latest.Term == term && latest.Wordness == wordness) return;

        using SqliteCommand command = Command("INSERT INTO history (kind, term, wordness, at_utc) VALUES ('find', $term, $wordness, $now)");
        command.Parameters.AddWithValue("$term", term);
        command.Parameters.AddWithValue("$wordness", wordness);
        command.Parameters.AddWithValue("$now", Now());
        command.ExecuteNonQuery();
        Trim(HistoryKind.Find);
    }

    /// <summary>Newest first.</summary>
    public IReadOnlyList<HistoryEntry> History(HistoryKind kind, int limit)
    {
        using SqliteCommand command = Command(
            "SELECT id, first_chapter, first_verse, last_chapter, last_verse, term, wordness, at_utc " +
            "FROM history WHERE kind = $kind ORDER BY id DESC LIMIT $limit");
        command.Parameters.AddWithValue("$kind", Name(kind));
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, HistoryLimit));

        var result = new List<HistoryEntry>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            VerseRef? first = reader.IsDBNull(1) ? null : new VerseRef(reader.GetInt32(1), reader.GetInt32(2));
            VerseRef? last = reader.IsDBNull(3) ? null : new VerseRef(reader.GetInt32(3), reader.GetInt32(4));
            result.Add(new HistoryEntry(
                reader.GetInt64(0), kind, first, last,
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                ParseTime(reader.GetString(7))));
        }
        return result;
    }

    public void ClearHistory(HistoryKind kind)
    {
        using SqliteCommand command = Command("DELETE FROM history WHERE kind = $kind");
        command.Parameters.AddWithValue("$kind", Name(kind));
        command.ExecuteNonQuery();
    }

    private void Trim(HistoryKind kind)
    {
        using SqliteCommand command = Command(
            "DELETE FROM history WHERE kind = $kind AND id NOT IN " +
            "(SELECT id FROM history WHERE kind = $kind ORDER BY id DESC LIMIT $limit)");
        command.Parameters.AddWithValue("$kind", Name(kind));
        command.Parameters.AddWithValue("$limit", HistoryLimit);
        command.ExecuteNonQuery();
    }

    // -- text modes -----------------------------------------------------

    /// <summary>The reader's text modes, by name, with their rules in order.</summary>
    public IReadOnlyList<DerivedTextMode> TextModes()
    {
        var rules = new Dictionary<string, List<TextRule>>(StringComparer.Ordinal);
        using (SqliteCommand command = Command("SELECT mode, find, replace FROM text_mode_rules ORDER BY mode, position"))
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string mode = reader.GetString(0);
                if (!rules.TryGetValue(mode, out List<TextRule>? list)) rules[mode] = list = [];
                list.Add(new TextRule(reader.GetString(1), reader.GetString(2)));
            }
        }

        using SqliteCommand modes = Command("SELECT name, base, description FROM text_modes ORDER BY name");
        var result = new List<DerivedTextMode>();
        using SqliteDataReader row = modes.ExecuteReader();
        while (row.Read())
        {
            string name = row.GetString(0);
            result.Add(new DerivedTextMode(name, row.GetString(1), rules.GetValueOrDefault(name) ?? [], row.GetString(2)));
        }
        return result;
    }

    /// <summary>Adds a text mode, or replaces the one of the same name.</summary>
    /// <exception cref="ArgumentException">The definition is not sound.</exception>
    public void SaveTextMode(DerivedTextMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        if (mode.Problem() is { } problem) throw new ArgumentException(problem, nameof(mode));

        using SqliteTransaction transaction = _connection.BeginTransaction();
        DeleteTextMode(mode.Name, transaction);

        using (SqliteCommand command = Command(
            "INSERT INTO text_modes (name, base, description, updated_utc) VALUES ($name, $base, $description, $now)"))
        {
            command.Transaction = transaction;
            command.Parameters.AddWithValue("$name", mode.Name);
            command.Parameters.AddWithValue("$base", mode.Base);
            command.Parameters.AddWithValue("$description", mode.Description);
            command.Parameters.AddWithValue("$now", Now());
            command.ExecuteNonQuery();
        }

        using (SqliteCommand command = Command(
            "INSERT INTO text_mode_rules (mode, position, find, replace) VALUES ($mode, $position, $find, $replace)"))
        {
            command.Transaction = transaction;
            SqliteParameter position = command.Parameters.Add("$position", SqliteType.Integer);
            SqliteParameter find = command.Parameters.Add("$find", SqliteType.Text);
            SqliteParameter replace = command.Parameters.Add("$replace", SqliteType.Text);
            command.Parameters.AddWithValue("$mode", mode.Name);
            for (int i = 0; i < mode.Rules.Count; i++)
            {
                position.Value = i;
                find.Value = mode.Rules[i].Find;
                replace.Value = mode.Rules[i].Replace;
                command.ExecuteNonQuery();
            }
        }
        transaction.Commit();
    }

    /// <summary>Removes a text mode; false when there was none of that name.</summary>
    public bool DeleteTextMode(string name)
    {
        using SqliteTransaction transaction = _connection.BeginTransaction();
        bool removed = DeleteTextMode(name, transaction);
        transaction.Commit();
        return removed;
    }

    private bool DeleteTextMode(string name, SqliteTransaction transaction)
    {
        using SqliteCommand rules = Command("DELETE FROM text_mode_rules WHERE mode = $name");
        rules.Transaction = transaction;
        rules.Parameters.AddWithValue("$name", name);
        rules.ExecuteNonQuery();

        using SqliteCommand mode = Command("DELETE FROM text_modes WHERE name = $name");
        mode.Transaction = transaction;
        mode.Parameters.AddWithValue("$name", name);
        return mode.ExecuteNonQuery() > 0;
    }

    // -- research selections -------------------------------------------

    /// <summary>Every saved research selection, most recently changed first.</summary>
    public IReadOnlyList<ResearchSelection> ResearchSelections()
    {
        using SqliteCommand command = Command(
            "SELECT id, title, note, address, value_system, counting, created_utc, updated_utc " +
            "FROM research_selections ORDER BY updated_utc DESC, id DESC");
        var result = new List<ResearchSelection>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ResearchSelection(
                reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : ParseCounting(reader.GetString(5)),
                ParseTime(reader.GetString(6)), ParseTime(reader.GetString(7))));
        }
        return result;
    }

    /// <summary>Adds a research selection, or replaces the one with <paramref name="id"/>.</summary>
    /// <returns>The saved selection; null when <paramref name="id"/> names none.</returns>
    /// <exception cref="ArgumentException">The title, note or address is not acceptable.</exception>
    public ResearchSelection? SaveResearchSelection(
        long? id, string title, string note, string address, string? valueSystem, CountingOptions? counting)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(address);
        if (title.Length > MaxTitleLength) throw new ArgumentException($"A title is at most {MaxTitleLength} characters.", nameof(title));
        if (note.Length > MaxNoteLength) throw new ArgumentException($"A note is at most {MaxNoteLength:N0} characters.", nameof(note));
        if (Content.SelectionAddress.Parse(address) is { IsSuccess: false, Error: string error }) throw new ArgumentException(error, nameof(address));

        string now = Now();
        using SqliteCommand command = Command(id is null
            ? """
              INSERT INTO research_selections (title, note, address, value_system, counting, created_utc, updated_utc)
              VALUES ($title, $note, $address, $system, $counting, $now, $now)
              RETURNING id, created_utc
              """
            : """
              UPDATE research_selections
              SET title = $title, note = $note, address = $address, value_system = $system, counting = $counting, updated_utc = $now
              WHERE id = $id
              RETURNING id, created_utc
              """);
        if (id is long existing) command.Parameters.AddWithValue("$id", existing);
        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$note", note);
        command.Parameters.AddWithValue("$address", address);
        command.Parameters.AddWithValue("$system", (object?)valueSystem ?? DBNull.Value);
        command.Parameters.AddWithValue("$counting", counting is null ? DBNull.Value : FormatCounting(counting));
        command.Parameters.AddWithValue("$now", now);

        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new ResearchSelection(
            reader.GetInt64(0), title, note, address, valueSystem, counting, ParseTime(reader.GetString(1)), ParseTime(now));
    }

    /// <summary>Removes a research selection; false when there was none with that id.</summary>
    public bool DeleteResearchSelection(long id)
    {
        using SqliteCommand command = Command("DELETE FROM research_selections WHERE id = $id");
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery() > 0;
    }

    // Counting options are stored as the names of those switched on, which
    // reads plainly in the file and needs no serializer.
    private static readonly (string Name, Func<CountingOptions, bool> Get, Func<CountingOptions, CountingOptions> Set)[] CountingFlags =
    [
        ("IncludeBasmalas", o => o.IncludeBasmalas, o => o with { IncludeBasmalas = true }),
        ("WawAsWord", o => o.WawAsWord, o => o with { WawAsWord = true }),
        ("ShaddaAsLetter", o => o.ShaddaAsLetter, o => o with { ShaddaAsLetter = true }),
        ("HamzaAboveLine", o => o.HamzaAboveLine, o => o with { HamzaAboveLine = true }),
        ("ElfAboveLine", o => o.ElfAboveLine, o => o with { ElfAboveLine = true }),
        ("YaaAboveLine", o => o.YaaAboveLine, o => o with { YaaAboveLine = true }),
        ("NoonAboveLine", o => o.NoonAboveLine, o => o with { NoonAboveLine = true }),
    ];

    private static string FormatCounting(CountingOptions options) =>
        string.Join(',', CountingFlags.Where(f => f.Get(options)).Select(f => f.Name));

    private static CountingOptions ParseCounting(string text)
    {
        var names = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.Ordinal);
        CountingOptions options = CountingOptions.Default with { IncludeBasmalas = false };
        foreach (var flag in CountingFlags)
        {
            if (names.Contains(flag.Name)) options = flag.Set(options);
        }
        return options;
    }

    // -- plumbing -------------------------------------------------------

    private static string Name(HistoryKind kind) => kind == HistoryKind.Browse ? "browse" : "find";

    private static string Now() => DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

    private static DateTime ParseTime(string text) =>
        DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static void AddRange(SqliteCommand command, VerseRef first, VerseRef last)
    {
        command.Parameters.AddWithValue("$fc", first.Chapter);
        command.Parameters.AddWithValue("$fv", first.Verse);
        command.Parameters.AddWithValue("$lc", last.Chapter);
        command.Parameters.AddWithValue("$lv", last.Verse);
    }

    private SqliteCommand Command(string sql)
    {
        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    private void Execute(string sql, SqliteTransaction? transaction = null)
    {
        using SqliteCommand command = Command(sql);
        command.Transaction = transaction;
        command.ExecuteNonQuery();
    }

    private long Scalar(string sql)
    {
        using SqliteCommand command = Command(sql);
        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    public void Dispose() => _connection.Dispose();
}
