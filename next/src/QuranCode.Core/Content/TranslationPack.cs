using Microsoft.Data.Sqlite;

namespace QuranCode.Core.Content;

/// <summary>
/// An optional file of more translations (built by data/import/build_translations.py),
/// keyed by one edition's verse numbers.
/// </summary>
public sealed class TranslationPack : IDisposable
{
    public const int SchemaVersion = 1;

    private readonly SqliteConnection _connection;

    /// <param name="source">This pack's number among the engine's translation sources, carried in each <see cref="TranslationInfo"/>.</param>
    /// <exception cref="InvalidDataException">The file is not a pack for this edition.</exception>
    public TranslationPack(string path, string edition, int source)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!File.Exists(path)) throw new FileNotFoundException("The translation pack was not found.", path);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly }.ToString());
        _connection.Open();

        string? version = Value("schema_version"), packEdition = Value("edition");
        if (version != SchemaVersion.ToString(System.Globalization.CultureInfo.InvariantCulture))
        {
            Dispose();
            throw new InvalidDataException($"The translation pack has schema {version ?? "none"}; this engine reads {SchemaVersion}.");
        }
        if (packEdition != edition)
        {
            Dispose();
            throw new InvalidDataException($"The translation pack is for the {packEdition} edition, not {edition}.");
        }

        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT id, key, language, name, translator, kind, direction FROM translations WHERE installed = 1 ORDER BY language, key";
        var list = new List<TranslationInfo>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new TranslationInfo(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetString(4), reader.GetString(5), reader.GetString(6) == "rtl", source));
        }
        Translations = list;
    }

    public IReadOnlyList<TranslationInfo> Translations { get; }

    public IReadOnlyDictionary<int, string> TranslationText(int translationId, int firstVerse, int lastVerse)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT verse_number, text FROM translation_text WHERE translation_id = $id AND verse_number BETWEEN $first AND $last";
        command.Parameters.AddWithValue("$id", translationId);
        command.Parameters.AddWithValue("$first", firstVerse);
        command.Parameters.AddWithValue("$last", lastVerse);
        var result = new Dictionary<int, string>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read()) result[reader.GetInt32(0)] = reader.GetString(1);
        return result;
    }

    private string? Value(string key)
    {
        try
        {
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT value FROM pack WHERE key = $key";
            command.Parameters.AddWithValue("$key", key);
            return command.ExecuteScalar() as string;
        }
        catch (SqliteException)
        {
            return null; // not a pack at all: the caller reports it as the wrong schema
        }
    }

    public void Dispose() => _connection.Dispose();
}
