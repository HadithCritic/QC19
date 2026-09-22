using System.Globalization;
using Microsoft.Data.Sqlite;

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

public sealed record HistoryEntry(long Id, HistoryKind Kind, VerseRef? First, VerseRef? Last, string? Term, string? Wordness, DateTime AtUtc);

/// <summary>
/// The reader's own data: bookmarks with notes and browse and find history,
/// kept in <c>user.db</c>, apart from the read-only content.
/// </summary>
/// <remarks>
/// <para>
/// Features.txt #68 to #70. Positions are stored as chapter:verse rather than
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
    public const int SchemaVersion = 1;
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
