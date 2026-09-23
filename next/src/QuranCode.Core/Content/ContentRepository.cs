using Microsoft.Data.Sqlite;
using QuranCode.Core.Numerology;
using QuranCode.Core.Text;

namespace QuranCode.Core.Content;

/// <summary>A value system's identity, without loading its letter map.</summary>
/// <param name="ResearchOnly">
/// Hidden unless research mode is on: the system matches the legacy
/// <c>Values/_ExtraSystems.txt</c>, or its text mode is research-only.
/// </param>
public readonly record struct ValueSystemSummary(
    string Name,
    string TextMode,
    string LetterOrder,
    string LetterValue,
    bool ResearchOnly);

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

    /// <summary>Oldest schema this code reads: v3 added editions and verse 0, v4 the waw words.</summary>
    public const int MinimumSchemaVersion = 6;

    private Chapter[]? _chapters;
    private Verse[]? _verses;
    private CorpusInfo? _corpus;
    private VerseRules? _verseRules;
    private Dictionary<string, Partition[]>? _partitions;
    private WawWords? _wawWords;
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
        RequireSchema(databasePath);
    }

    private void RequireSchema(string databasePath)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT MAX(version) FROM schema_version";
        long version = command.ExecuteScalar() is long v ? v : 0;
        if (version < MinimumSchemaVersion)
        {
            _connection.Dispose();
            throw new InvalidOperationException(
                $"{Path.GetFileName(databasePath)} uses content schema v{version}; this build needs v{MinimumSchemaVersion}. " +
                "Rebuild it with next/data/import/build_content.py.");
        }
    }

    /// <summary>Which edition the database holds.</summary>
    public CorpusInfo Corpus => _corpus ??= LoadCorpus();

    /// <summary>Verse-scoped word rules for this edition.</summary>
    public VerseRules VerseRules => _verseRules ??= LoadVerseRules();

    /// <summary>Pages, parts, stations, groups, halves, quarters and bowings, by kind, ordered by number.</summary>
    public IReadOnlyDictionary<string, Partition[]> Partitions => _partitions ??= LoadPartitions();

    private Dictionary<string, Partition[]> LoadPartitions()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT kind, number, first_verse, last_verse FROM partitions ORDER BY kind, number";
        var result = new Dictionary<string, List<Partition>>(StringComparer.Ordinal);
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string kind = reader.GetString(0);
            if (!result.TryGetValue(kind, out List<Partition>? list)) result[kind] = list = [];
            list.Add(new Partition(kind, reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
        }
        return result.ToDictionary(p => p.Key, p => p.Value.ToArray(), StringComparer.Ordinal);
    }

    /// <summary>Words whose leading waw belongs to the word, for waw-as-word.</summary>
    public WawWords WawWords => _wawWords ??= LoadWawWords();

    private WawWords LoadWawWords()
    {
        var words = new HashSet<string>(StringComparer.Ordinal);
        var splits = new Dictionary<string, HashSet<(int, int)>>(StringComparer.Ordinal);

        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT word FROM waw_words";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read()) words.Add(reader.GetString(0));
        }
        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT word, chapter, verse FROM waw_word_splits";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string word = reader.GetString(0);
                if (!splits.TryGetValue(word, out HashSet<(int, int)>? verses)) splits[word] = verses = [];
                verses.Add((reader.GetInt32(1), reader.GetInt32(2)));
            }
        }
        return new WawWords(words, splits);
    }

    private Dictionary<int, Dictionary<int, string>>? _pauseMarks;

    /// <summary>
    /// Pause marks stored apart from the text, by verse number and display
    /// word index; empty for an edition whose text carries its own.
    /// </summary>
    public IReadOnlyDictionary<int, Dictionary<int, string>> PauseMarks => _pauseMarks ??= LoadPauseMarks();

    private Dictionary<int, Dictionary<int, string>> LoadPauseMarks()
    {
        var marks = new Dictionary<int, Dictionary<int, string>>();
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT verse_number, word_index, mark FROM pause_marks";
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            int verse = reader.GetInt32(0);
            if (!marks.TryGetValue(verse, out Dictionary<int, string>? words)) marks[verse] = words = [];
            words[reader.GetInt32(1)] = reader.GetString(2);
        }
        return marks;
    }

    private Dictionary<(int, int), WordData>? _wordData;
    private Dictionary<(string, string), string>? _grammarLabels;

    /// <summary>A display word's gloss, transliteration and grammar parts, or null when there are none.</summary>
    public WordData? WordDataOf(int verseNumber, int wordIndex) =>
        (_wordData ??= LoadWordData()).GetValueOrDefault((verseNumber, wordIndex));

    /// <summary>The name of a corpus tag or feature in a language (en or ar), or null.</summary>
    public string? GrammarLabel(string tag, string language) =>
        (_grammarLabels ??= LoadGrammarLabels()).GetValueOrDefault((tag, language));

    private Dictionary<(int, int), WordData> LoadWordData()
    {
        var parts = new Dictionary<(int, int), List<WordPart>>();
        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT verse_number, word_index, part, form, tag, features FROM word_parts ORDER BY verse_number, word_index, part";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = (reader.GetInt32(0), reader.GetInt32(1));
                if (!parts.TryGetValue(key, out List<WordPart>? list)) parts[key] = list = [];
                list.Add(new WordPart(reader.GetInt32(2), reader.GetString(3), reader.GetString(4), reader.GetString(5).Split('|')));
            }
        }

        var data = new Dictionary<(int, int), WordData>();
        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT verse_number, word_index, meaning, transliteration FROM word_glosses";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = (reader.GetInt32(0), reader.GetInt32(1));
                data[key] = new WordData(reader.GetString(2), reader.GetString(3), parts.GetValueOrDefault(key) ?? []);
            }
        }
        return data;
    }

    private Dictionary<(string, string), string> LoadGrammarLabels()
    {
        var labels = new Dictionary<(string, string), string>();
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT tag, language, label FROM grammar_labels";
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read()) labels[(reader.GetString(0), reader.GetString(1))] = reader.GetString(2);
        return labels;
    }

    private Dictionary<int, string>? _prostrations;

    /// <summary>Prostration verses by absolute number: recommended or obligatory.</summary>
    public IReadOnlyDictionary<int, string> Prostrations => _prostrations ??= LoadProstrations();

    private Dictionary<int, string> LoadProstrations()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT verse_number, type FROM prostrations";
        var map = new Dictionary<int, string>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read()) map[reader.GetInt32(0)] = reader.GetString(1);
        return map;
    }

    private TranslationInfo[]? _translations;

    /// <summary>The translations and other verse texts the database holds.</summary>
    public IReadOnlyList<TranslationInfo> Translations => _translations ??= LoadTranslations();

    private TranslationInfo[] LoadTranslations()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT id, key, language, name, translator, kind, direction FROM translations WHERE installed = 1 ORDER BY id";
        var result = new List<TranslationInfo>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new TranslationInfo(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetString(4), reader.GetString(5), reader.GetString(6) == "rtl"));
        }
        return [.. result];
    }

    /// <summary>One translation's text for a run of verses, by absolute verse number.</summary>
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

    /// <summary>Every verse of one translation, for searching it.</summary>
    public IReadOnlyDictionary<int, string> AllTranslationText(int translationId) =>
        TranslationText(translationId, 1, Verses.Count);

    private WordRoots? _wordRoots;

    /// <summary>Roots of every display word.</summary>
    public WordRoots WordRoots => _wordRoots ??= LoadWordRoots();

    private WordRoots LoadWordRoots()
    {
        var texts = new List<string>();
        var idOf = new Dictionary<long, int>();
        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT id, text FROM roots ORDER BY id";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                idOf[reader.GetInt64(0)] = texts.Count;
                texts.Add(reader.GetString(1));
            }
        }

        IReadOnlyList<Verse> verses = Verses;
        var lists = new List<int>[verses.Count][];
        for (int v = 0; v < verses.Count; v++)
        {
            int words = Text.DisplayWords.Split(verses[v].Text).Length;
            lists[v] = new List<int>[words];
        }

        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT verse_number, word_index, root_id FROM verse_word_roots ORDER BY verse_number, word_index, root_id";
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                List<int>[] words = lists[reader.GetInt32(0) - 1];
                int index = reader.GetInt32(1);
                if (index >= words.Length)
                {
                    throw new InvalidDataException($"verse_word_roots names word {index} of verse {reader.GetInt32(0)}, which has {words.Length} words.");
                }
                (words[index] ??= []).Add(idOf[reader.GetInt64(2)]);
            }
        }

        int[][][] byVerse = lists.Select(words => words.Select(roots => roots?.ToArray() ?? []).ToArray()).ToArray();
        return new WordRoots([.. texts], byVerse);
    }

    private CorpusInfo LoadCorpus()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT key, value FROM corpus";
        var values = new Dictionary<string, string>();
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read()) values[reader.GetString(0)] = reader.GetString(1);
        }

        string edition = values.GetValueOrDefault("edition", "classic");
        BasmalaMode basmala = values.GetValueOrDefault("basmala", "prefix") switch
        {
            "prefix" => BasmalaMode.Prefix,
            "verse-zero" => BasmalaMode.VerseZero,
            string other => throw new InvalidOperationException($"unknown basmala mode in corpus table: {other}"),
        };
        return new CorpusInfo(edition, basmala);
    }

    private VerseRules LoadVerseRules()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT verse_number, find, replace_with FROM verse_rules ORDER BY verse_number, ordinal";
        var rules = new Dictionary<int, List<(string, string)>>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            int verse = reader.GetInt32(0);
            if (!rules.TryGetValue(verse, out List<(string, string)>? list)) rules[verse] = list = [];
            list.Add((reader.GetString(1), reader.GetString(2)));
        }
        return new VerseRules(rules.ToDictionary(p => p.Key, p => p.Value.ToArray()));
    }

    /// <summary>All 114 chapters, loaded on first use.</summary>
    public IReadOnlyList<Chapter> Chapters => _chapters ??= LoadChapters();

    /// <summary>
    /// Every verse row in canonical order, loaded on first use: 6,236 in the
    /// classic edition, 6,346 in the Submission edition (its 112 verse-0
    /// Bismillahs included).
    /// </summary>
    public IReadOnlyList<Verse> Verses => _verses ??= LoadVerses();

    private Chapter[] LoadChapters()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT number, name, transliterated_name, english_name,
                   revelation_order, revelation_place, verse_count, first_verse, has_verse_zero, initialization
            FROM chapters ORDER BY number
            """;

        var result = new List<Chapter>(114);
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Chapter(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetInt32(4), reader.GetString(5),
                reader.GetInt32(6), reader.GetInt32(7), reader.GetBoolean(8), reader.GetString(9)));
        }
        return [.. result];
    }

    private Verse[] LoadVerses()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT number, chapter_number, number_in_chapter, text, is_basmala FROM verses ORDER BY number";

        var result = new List<Verse>(6346);
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Verse(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3), reader.GetBoolean(4)));
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

    /// <summary>Every installed value system, ordered by name.</summary>
    public IReadOnlyList<ValueSystemSummary> ValueSystemSummaries()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT s.name, s.text_mode_name, s.letter_order, s.letter_value,
                   s.research_only OR EXISTS (
                       SELECT 1 FROM text_modes m
                       WHERE m.name = s.text_mode_name AND m.research_only = 1)
            FROM value_systems s
            ORDER BY s.name
            """;

        var result = new List<ValueSystemSummary>();
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new ValueSystemSummary(
                reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetBoolean(4)));
        }
        return result;
    }

    public void Dispose() => _connection.Dispose();
}
