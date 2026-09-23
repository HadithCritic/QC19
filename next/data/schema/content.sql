-- QuranCode Next: canonical content schema (content.db)
--
-- Immutable, rebuildable from source data. Nothing here is written at runtime.
-- User state lives in a separate user.db (see user.sql) so that content can be
-- replaced wholesale without touching anything the user created.
--
-- Design is driven by measurements recorded in
-- next/tests/golden/performance-baseline.tsv:
--
--   building the legacy object graph  3,804 ms / 136 MB
--   computing over it                    34 ms
--
-- The arithmetic was never the problem. The goal of this schema is therefore to
-- make the graph unnecessary, not to make the arithmetic faster.

PRAGMA foreign_keys = ON;

-- ---------------------------------------------------------------------------
-- Provenance
-- ---------------------------------------------------------------------------
-- Brief §13/§29: every dataset carries its origin, license and a deterministic
-- hash, so a result can always be traced back to the bytes that produced it.

CREATE TABLE schema_version (
    version     INTEGER NOT NULL,
    applied_utc TEXT    NOT NULL
);

CREATE TABLE sources (
    id            INTEGER PRIMARY KEY,
    key           TEXT    NOT NULL UNIQUE,  -- 'quran-uthmani', 'rules/77878/Simplified29'
    name          TEXT    NOT NULL,
    kind          TEXT    NOT NULL,         -- text | rules | values | translation | metadata
    version       TEXT,
    origin        TEXT,                     -- upstream URL or citation
    license       TEXT,
    content_hash  TEXT    NOT NULL,         -- SHA-256 of the normalized source bytes
    byte_length   INTEGER NOT NULL,
    imported_utc  TEXT    NOT NULL
);

-- ---------------------------------------------------------------------------
-- Canonical text
-- ---------------------------------------------------------------------------
-- The Arabic text is stored once, in its original form. Everything else is
-- derived. Brief §39: canonical content is immutable and never silently edited
-- to make a query work.

CREATE TABLE chapters (
    number              INTEGER PRIMARY KEY,   -- 1..114
    name                TEXT    NOT NULL,
    transliterated_name TEXT    NOT NULL,
    english_name        TEXT    NOT NULL,
    revelation_order    INTEGER NOT NULL,
    revelation_place    TEXT    NOT NULL,      -- Makkah | Madinah
    verse_count         INTEGER NOT NULL,      -- numbered verses, excluding a verse 0
    first_verse         INTEGER NOT NULL,      -- absolute number of the chapter's first row (its verse 0 if any)
    has_verse_zero      INTEGER NOT NULL DEFAULT 0, -- 1: the Bismillah is stored as verse 0
    bowing_count        INTEGER NOT NULL,
    -- Quranic initials: key (chapter 1), full, partial, double (42) or none
    initialization      TEXT    NOT NULL DEFAULT 'none'
);

CREATE TABLE verses (
    number            INTEGER PRIMARY KEY,     -- absolute: 1..6236 classic, 1..6346 submission
    chapter_number    INTEGER NOT NULL REFERENCES chapters(number),
    number_in_chapter INTEGER NOT NULL,
    text              TEXT    NOT NULL,        -- the edition's text, unmodified
    stopmark          TEXT,
    is_basmala        INTEGER NOT NULL DEFAULT 0, -- 1: a verse-0 Bismillah the user may exclude
    UNIQUE (chapter_number, number_in_chapter)
);

-- Which edition this database holds and how it treats the Bismillah:
--   edition  classic | submission
--   basmala  prefix     (verse 1 of chapters 2..114 except 9 begins with it)
--            verse-zero (stored as its own verse 0; chapter 1's is verse 1)
CREATE TABLE corpus (
    key   TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

-- Words that begin with a waw belonging to the word (legacy Data/waw-words.txt),
-- which waw-as-word leaves whole, and the verses where the same spelling is
-- و plus another word and is split after all.
CREATE TABLE waw_words (
    word TEXT PRIMARY KEY
);

CREATE TABLE waw_word_splits (
    word    TEXT    NOT NULL REFERENCES waw_words(word),
    chapter INTEGER NOT NULL,
    verse   INTEGER NOT NULL,
    PRIMARY KEY (word, chapter, verse)
);

-- Word rules scoped to one verse, applied to that verse's text before the
-- text mode's rules, in every text mode. The stored text is never edited;
-- these only change how words are counted (for example, 96:5 counts ما لم as
-- one word in the Submission edition).
CREATE TABLE verse_rules (
    verse_number INTEGER NOT NULL REFERENCES verses(number),
    ordinal      INTEGER NOT NULL,
    find         TEXT    NOT NULL,
    replace_with TEXT    NOT NULL,
    note         TEXT    NOT NULL,
    PRIMARY KEY (verse_number, ordinal)
);

-- The eight partition schemes the legacy Book exposes in parallel
-- (stations, parts, groups, halfs, quarters, bowings, pages). The legacy model
-- gave each its own class and its own verse list; they are all the same shape,
-- so one table with a discriminator replaces seven classes.
CREATE TABLE partitions (
    kind         TEXT    NOT NULL,  -- station | part | group | half | quarter | bowing | page
    number       INTEGER NOT NULL,
    first_verse  INTEGER NOT NULL REFERENCES verses(number),
    last_verse   INTEGER NOT NULL REFERENCES verses(number),
    PRIMARY KEY (kind, number)
);
CREATE INDEX idx_partitions_range ON partitions(kind, first_verse, last_verse);

-- ---------------------------------------------------------------------------
-- Text modes
-- ---------------------------------------------------------------------------
-- Brief §24. These already exist as data in the legacy tree
-- (Rules/<word-count-method>/<mode>.txt) as ordered find/replace rules applied
-- in sequence by SimplificationSystem.Simplify. That design is kept; only the
-- storage changes.
--
-- Rule ORDER IS SIGNIFICANT. The rules are not a set.

CREATE TABLE text_modes (
    id                INTEGER PRIMARY KEY,
    name              TEXT    NOT NULL,  -- Original, Simplified28..36, SimplifiedDots, SimplifiedMarks
    word_count_method INTEGER NOT NULL,  -- 77878 | 77880
    research_only     INTEGER NOT NULL DEFAULT 0,  -- 1: hidden unless research mode (legacy: non-Standard editions)
    source_id         INTEGER NOT NULL REFERENCES sources(id),
    UNIQUE (name, word_count_method)
);

CREATE TABLE text_mode_rules (
    text_mode_id INTEGER NOT NULL REFERENCES text_modes(id),
    ordinal      INTEGER NOT NULL,       -- application order; significant
    find         TEXT    NOT NULL,
    replace_with TEXT    NOT NULL,
    PRIMARY KEY (text_mode_id, ordinal)
);

-- Segmentation is per text mode: the same verse yields different word and
-- letter counts under Original vs Simplified36. Materializing all eight modes
-- for every letter would be ~2.6M rows, so segmentation is built on demand and
-- cached here, keyed by the mode and the build options that affect it.
CREATE TABLE segmentations (
    id                INTEGER PRIMARY KEY,
    text_mode_id      INTEGER NOT NULL REFERENCES text_modes(id),
    -- The nine MainForm build flags. They change counts, so they are part of
    -- the identity of a segmentation, not a runtime setting.
    with_diacritics   INTEGER NOT NULL,
    with_bism_allah   INTEGER NOT NULL,
    waw_as_word       INTEGER NOT NULL,
    shadda_as_letter  INTEGER NOT NULL,
    hamza_as_letter   INTEGER NOT NULL,
    elf_as_letter     INTEGER NOT NULL,
    yaa_as_letter     INTEGER NOT NULL,
    noon_as_letter    INTEGER NOT NULL,
    emlaaei_text      INTEGER NOT NULL,
    input_hash        TEXT    NOT NULL,   -- hash of source text + rules + flags
    algorithm_version INTEGER NOT NULL,
    built_utc         TEXT    NOT NULL,
    word_count        INTEGER NOT NULL,
    letter_count      INTEGER NOT NULL,
    UNIQUE (text_mode_id, with_diacritics, with_bism_allah, waw_as_word,
            shadda_as_letter, hamza_as_letter, elf_as_letter, yaa_as_letter,
            noon_as_letter, emlaaei_text)
);

-- Words as a flat, ordered table rather than objects with back-references.
-- The legacy Word carried 29 fields and a pointer to its Verse; here the
-- relationships are integer keys and the derived counters live in
-- word_metrics, computed once per segmentation.
CREATE TABLE words (
    segmentation_id   INTEGER NOT NULL REFERENCES segmentations(id),
    number            INTEGER NOT NULL,      -- absolute within the segmentation
    verse_number      INTEGER NOT NULL REFERENCES verses(number),
    number_in_verse   INTEGER NOT NULL,
    number_in_chapter INTEGER NOT NULL,
    text              TEXT    NOT NULL,
    letter_count      INTEGER NOT NULL,
    PRIMARY KEY (segmentation_id, number)
);
CREATE INDEX idx_words_verse ON words(segmentation_id, verse_number, number_in_verse);
CREATE INDEX idx_words_text  ON words(segmentation_id, text);

-- Letters are NOT stored one row per letter. 327,792 rows per segmentation,
-- times the option space, is exactly the object graph this schema exists to
-- avoid. A word's letters are derivable from its text plus the mode's letter
-- set, and per-letter metrics are materialized only where a modifier needs
-- them (letter_metrics below).

-- ---------------------------------------------------------------------------
-- Value systems
-- ---------------------------------------------------------------------------
-- Brief §23. Already data in the legacy tree: Values/*.txt, named
-- TextMode_LetterOrder_LetterValue, holding letter -> value pairs.
-- 410 files ship; 274 load under the default word count method.

CREATE TABLE value_systems (
    id                INTEGER PRIMARY KEY,
    name              TEXT    NOT NULL UNIQUE,  -- Original_Alphabet_Primes1
    text_mode_name    TEXT    NOT NULL,         -- Original
    letter_order      TEXT    NOT NULL,         -- Alphabet | Abjad | Frequency | Appearance | ...
    letter_value      TEXT    NOT NULL,         -- Primes1 | Gematria | Composites | ...
    letter_values_sum INTEGER NOT NULL,         -- checksum; verified on import
    research_only     INTEGER NOT NULL DEFAULT 0, -- 1: matches Values/_ExtraSystems.txt
    source_id         INTEGER NOT NULL REFERENCES sources(id)
);

CREATE TABLE value_map (
    value_system_id INTEGER NOT NULL REFERENCES value_systems(id),
    letter          TEXT    NOT NULL,           -- single character
    value           INTEGER NOT NULL,
    PRIMARY KEY (value_system_id, letter)
);

-- The 21 modifier flags on the legacy NumericalSystem, plus the calculation
-- mode and the four sign-alternation flags. Stored as a named, reusable profile
-- so a research result can cite one instead of describing hidden global state.
CREATE TABLE calculation_profiles (
    id                       INTEGER PRIMARY KEY,
    name                     TEXT    NOT NULL UNIQUE,
    calculation_mode         TEXT    NOT NULL,  -- SumOfLetterValues | ...DigitSums | ...DigitalRoots | ...
    alternate_letter_values  INTEGER NOT NULL DEFAULT 0,
    alternate_word_values    INTEGER NOT NULL DEFAULT 0,
    alternate_verse_values   INTEGER NOT NULL DEFAULT 0,
    alternate_chapter_values INTEGER NOT NULL DEFAULT 0,
    add_positions            INTEGER NOT NULL DEFAULT 0,
    absolute_positions       INTEGER NOT NULL DEFAULT 0,
    add_distances_to_prev    INTEGER NOT NULL DEFAULT 0,
    add_distances_to_next    INTEGER NOT NULL DEFAULT 0,
    distances_within_chapter INTEGER NOT NULL DEFAULT 0,
    -- The 17 AddTo* switches, stored as a bitmask to avoid 17 columns.
    -- Bit order is fixed by docs/specs/calculation-spec.md and must not change.
    add_to_mask              INTEGER NOT NULL DEFAULT 0
);

-- ---------------------------------------------------------------------------
-- Derived metrics
-- ---------------------------------------------------------------------------
-- Brief §12/§14: precomputed, and carrying enough identity to invalidate
-- precisely when a source or an algorithm changes.

CREATE TABLE derived_artifacts (
    id                INTEGER PRIMARY KEY,
    kind              TEXT    NOT NULL,   -- verse_metrics | word_metrics | letter_metrics | fts
    segmentation_id   INTEGER REFERENCES segmentations(id),
    value_system_id   INTEGER REFERENCES value_systems(id),
    profile_id        INTEGER REFERENCES calculation_profiles(id),
    input_hash        TEXT    NOT NULL,
    algorithm_version INTEGER NOT NULL,
    output_hash       TEXT    NOT NULL,
    built_utc         TEXT    NOT NULL,
    row_count         INTEGER NOT NULL,
    UNIQUE (kind, segmentation_id, value_system_id, profile_id, algorithm_version)
);

CREATE TABLE verse_metrics (
    artifact_id     INTEGER NOT NULL REFERENCES derived_artifacts(id) ON DELETE CASCADE,
    verse_number    INTEGER NOT NULL REFERENCES verses(number),
    word_count      INTEGER NOT NULL,
    letter_count    INTEGER NOT NULL,
    unique_letters  INTEGER NOT NULL,
    value           INTEGER NOT NULL,
    PRIMARY KEY (artifact_id, verse_number)
);
CREATE INDEX idx_verse_metrics_value ON verse_metrics(artifact_id, value);

CREATE TABLE word_metrics (
    artifact_id        INTEGER NOT NULL REFERENCES derived_artifacts(id) ON DELETE CASCADE,
    word_number        INTEGER NOT NULL,
    value              INTEGER NOT NULL,
    frequency          INTEGER NOT NULL,
    occurrence         INTEGER NOT NULL,
    occurrences_before INTEGER NOT NULL,
    occurrences_after  INTEGER NOT NULL,
    -- Distance to the previous identical word, in letters/words/verses/chapters.
    -- Feeds the AddToWord*Distance modifiers.
    dist_prev_l        INTEGER NOT NULL,
    dist_prev_w        INTEGER NOT NULL,
    dist_prev_v        INTEGER NOT NULL,
    dist_prev_c        INTEGER NOT NULL,
    PRIMARY KEY (artifact_id, word_number)
);
CREATE INDEX idx_word_metrics_value ON word_metrics(artifact_id, value);

-- Per-letter metrics, materialized only when a modifier profile actually needs
-- them. Most profiles do not.
CREATE TABLE letter_metrics (
    artifact_id        INTEGER NOT NULL REFERENCES derived_artifacts(id) ON DELETE CASCADE,
    letter_number      INTEGER NOT NULL,
    word_number        INTEGER NOT NULL,
    character          TEXT    NOT NULL,
    value              INTEGER NOT NULL,
    number_in_word     INTEGER NOT NULL,
    number_in_verse    INTEGER NOT NULL,
    number_in_chapter  INTEGER NOT NULL,
    occurrence         INTEGER NOT NULL,
    dist_prev_l        INTEGER NOT NULL,
    dist_prev_w        INTEGER NOT NULL,
    dist_prev_v        INTEGER NOT NULL,
    dist_prev_c        INTEGER NOT NULL,
    PRIMARY KEY (artifact_id, letter_number)
);

CREATE TABLE letter_frequencies (
    artifact_id INTEGER NOT NULL REFERENCES derived_artifacts(id) ON DELETE CASCADE,
    scope       TEXT    NOT NULL,   -- book | chapter | verse
    scope_id    INTEGER NOT NULL,   -- 0 for book
    character   TEXT    NOT NULL,
    frequency   INTEGER NOT NULL,
    PRIMARY KEY (artifact_id, scope, scope_id, character)
);

-- ---------------------------------------------------------------------------
-- Roots and grammar
-- ---------------------------------------------------------------------------

CREATE TABLE roots (
    id   INTEGER PRIMARY KEY,
    text TEXT    NOT NULL UNIQUE
);

CREATE TABLE word_roots (
    segmentation_id INTEGER NOT NULL,
    word_number     INTEGER NOT NULL,
    root_id         INTEGER NOT NULL REFERENCES roots(id),
    PRIMARY KEY (segmentation_id, word_number, root_id),
    FOREIGN KEY (segmentation_id, word_number) REFERENCES words(segmentation_id, number)
);
CREATE INDEX idx_word_roots_root ON word_roots(root_id);

-- Roots of the words a reader sees. word_index counts the verse's display
-- words from 0 (DisplayWords.Split over verses.text), so it holds in every
-- text mode and needs no segmentation. A word may have several roots
-- (particles and pronouns are listed too, as the legacy file lists them).
CREATE TABLE verse_word_roots (
    verse_number INTEGER NOT NULL REFERENCES verses(number),
    word_index   INTEGER NOT NULL,
    root_id      INTEGER NOT NULL REFERENCES roots(id),
    PRIMARY KEY (verse_number, word_index, root_id)
);
CREATE INDEX idx_verse_word_roots_root ON verse_word_roots(root_id);

-- Verses of prostration (sajda), from the metadata's prostration table. The
-- recitation player pauses after them, longer after an obligatory one.
CREATE TABLE prostrations (
    verse_number INTEGER PRIMARY KEY REFERENCES verses(number),
    type         TEXT NOT NULL    -- recommended | obligatory
);

-- Reciters whose verse-by-verse recordings everyayah.com serves (legacy
-- Audio/metadata.txt). folder is the path under https://everyayah.com/data/.
CREATE TABLE reciters (
    folder   TEXT PRIMARY KEY,
    language TEXT NOT NULL,
    name     TEXT NOT NULL,
    quality  TEXT NOT NULL,
    ordinal  INTEGER NOT NULL
);

-- Word by word: an English gloss and a transliteration for each display word
-- (legacy en.wordbyword and en.transliteration), and the Quranic Arabic
-- Corpus morphology of its parts. word_index counts display words from 0.
CREATE TABLE word_glosses (
    verse_number    INTEGER NOT NULL REFERENCES verses(number),
    word_index      INTEGER NOT NULL,
    meaning         TEXT    NOT NULL,
    transliteration TEXT    NOT NULL,
    PRIMARY KEY (verse_number, word_index)
);

CREATE TABLE word_parts (
    verse_number INTEGER NOT NULL REFERENCES verses(number),
    word_index   INTEGER NOT NULL,
    part         INTEGER NOT NULL,     -- 1-based within the word
    form         TEXT    NOT NULL,     -- Buckwalter transliteration, as the corpus gives it
    tag          TEXT    NOT NULL,     -- part of speech: N, V, P, DET ...
    features     TEXT    NOT NULL,     -- the corpus's features, verbatim: STEM|POS:N|LEM:...|ROOT:...|M|GEN
    PRIMARY KEY (verse_number, word_index, part)
);

-- Names of the corpus's tags and features in English and Arabic (legacy
-- Languages/*.txt, Dictionary Grammar_* rows).
CREATE TABLE grammar_labels (
    tag      TEXT NOT NULL,
    language TEXT NOT NULL,
    label    TEXT NOT NULL,
    PRIMARY KEY (tag, language)
);

-- Pause marks after display words, for editions whose text does not carry
-- them (the Submission export keeps them only in its arabic_clean column).
-- word_index counts display words from 0, as in verse_word_roots.
CREATE TABLE pause_marks (
    verse_number INTEGER NOT NULL REFERENCES verses(number),
    word_index   INTEGER NOT NULL,
    mark         TEXT    NOT NULL,
    PRIMARY KEY (verse_number, word_index)
);

CREATE TABLE word_grammar (
    segmentation_id INTEGER NOT NULL,
    word_number     INTEGER NOT NULL,
    part_number     INTEGER NOT NULL,
    part_text       TEXT    NOT NULL,
    pos_tag         TEXT,
    description     TEXT,
    PRIMARY KEY (segmentation_id, word_number, part_number)
);

-- ---------------------------------------------------------------------------
-- Translations
-- ---------------------------------------------------------------------------
-- Brief §27: optional packs, not shipped in the base install. The legacy tree
-- carries 156 MB of plaintext translations that are always present. Each pack
-- attaches as its own database file.

CREATE TABLE translations (
    id           INTEGER PRIMARY KEY,
    key          TEXT    NOT NULL UNIQUE,   -- en.asad, submission.en
    language     TEXT    NOT NULL,          -- BCP 47: en, fa, ar, en-Latn
    name         TEXT    NOT NULL,          -- shown to the reader
    translator   TEXT    NOT NULL,
    -- translation | transliteration | emlaaei (the Arabic in standard spelling)
    kind         TEXT    NOT NULL DEFAULT 'translation',
    direction    TEXT    NOT NULL DEFAULT 'ltr',  -- ltr | rtl
    source_id    INTEGER NOT NULL REFERENCES sources(id),
    installed    INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE translation_text (
    translation_id INTEGER NOT NULL REFERENCES translations(id) ON DELETE CASCADE,
    verse_number   INTEGER NOT NULL REFERENCES verses(number),
    text           TEXT    NOT NULL,
    PRIMARY KEY (translation_id, verse_number)
);

-- ---------------------------------------------------------------------------
-- Search
-- ---------------------------------------------------------------------------
-- Brief §11: FTS5 for text, ordinary indexes for structured queries. FTS is
-- deliberately not used for numeric research; those go through the metric
-- indexes above.

CREATE VIRTUAL TABLE verses_fts USING fts5(
    text,
    content = 'verses',
    content_rowid = 'number',
    tokenize = 'unicode61 remove_diacritics 0'
);

-- Not filled: translation search matches substrings in memory, as the
-- original does, which a token index cannot answer. Kept for a later
-- word search over large packs.
CREATE VIRTUAL TABLE translation_fts USING fts5(
    text,
    content = 'translation_text',
    tokenize = 'unicode61 remove_diacritics 2'
);
