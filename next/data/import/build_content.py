#!/usr/bin/env python3
"""Build content.db from the legacy QuranCode source tree.

A build-time tool. It never ships with the application; the application only
ever reads the database this produces.

Brief §13: canonical source -> validate -> normalize -> hash -> build. Every
source file is recorded in `sources` with a SHA-256 of its raw bytes, so any
row in the database can be traced back to the bytes it came from.

Brief §39: canonical content is copied verbatim. The importer never edits
Arabic text. Normalization is expressed as text-mode rules stored alongside the
text, applied later by the engine, never baked in here.

Usage:
    python build_content.py -o content.db
    python build_content.py --edition submission -o submission.db

The classic edition is the legacy Tanzil text and is what the golden tests
check against. The submission edition is the app's authoritative text: its
text, verse index and chapter names come from the wikisubmission.org export in
next/data/sources/submission/ (override with --submission-dir), and everything
else (value systems, text-mode rules, page and part boundaries) from the
legacy source tree in C#/ (override with --legacy-root).
"""

from __future__ import annotations

import argparse
import hashlib
import io
import os
import sqlite3
import sys
from datetime import datetime, timezone

import submission
import word_data
from alignment import PAUSE_MARKS, align_roots, display_words, letters_of, mark_after  # noqa: F401

SCHEMA_VERSION = 6

# Text modes the legacy hides in the Standard edition. Hardcoded there too:
# Server.LoadSimplificationSystems skips "SimplifiedMarks" when EDITION is Standard.
RESEARCH_ONLY_TEXT_MODES = frozenset({"SimplifiedMarks"})
ALGORITHM_VERSION = 1

# Word count method: the legacy engine ships two segmentation variants
# (Data/77878 and Data/77880). 77878 is the default and matches the golden data.
DEFAULT_WORD_COUNT_METHOD = 77878

TEXT_MODES = [
    "Original", "Simplified28", "Simplified29", "Simplified30",
    "Simplified31", "Simplified36", "SimplifiedDots", "SimplifiedMarks",
]

# Where each install-layout folder lives in the legacy source tree. Data is
# split between two projects; the first match wins.
LEGACY_LAYOUT = {
    "Data": ["DataAccess/Data", "Model/Data"],
    "Values": ["Server/Values"],
    "Rules": ["Server/Rules"],
}

DEFAULT_LEGACY_ROOT = os.path.normpath(
    os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", "C#"))

PARTITION_SECTIONS = {
    "station": "station", "part": "part", "group": "group", "half": "half",
    "quarter": "quarter", "bowing": "bowing", "page": "page",
}


def utcnow() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")


def read_text_detect(path: str) -> tuple[str, str]:
    """Read a legacy text file, detecting its encoding from the BOM.

    The Rules/ tree is not encoding-consistent: Simplified36.txt ships as UTF-8
    while its seven siblings are UTF-16LE. Sniffing the BOM rather than assuming
    keeps the importer honest about what is actually on disk, and the detected
    encoding is recorded so the inconsistency stays visible.
    """
    with open(path, "rb") as handle:
        raw = handle.read()

    if raw.startswith(b"\xff\xfe"):
        return raw.decode("utf-16-le")[1:], "utf-16-le"
    if raw.startswith(b"\xfe\xff"):
        return raw.decode("utf-16-be")[1:], "utf-16-be"
    if raw.startswith(b"\xef\xbb\xbf"):
        return raw.decode("utf-8-sig"), "utf-8-sig"
    return raw.decode("utf-8"), "utf-8"


def sha256_file(path: str) -> tuple[str, int]:
    """Hash the raw bytes, not the decoded text, so encoding is part of identity."""
    digest = hashlib.sha256()
    size = 0
    with open(path, "rb") as handle:
        while True:
            chunk = handle.read(1 << 20)
            if not chunk:
                break
            digest.update(chunk)
            size += len(chunk)
    return digest.hexdigest(), size


class Importer:
    def __init__(self, root: str, db_path: str, edition: str = "classic",
                 submission_dir: str | None = None) -> None:
        self.root = os.path.abspath(root)
        self.db_path = db_path
        self.edition = edition
        self.submission_dir = os.path.abspath(submission_dir) if submission_dir else None
        self.db: sqlite3.Connection | None = None
        self.errors: list[str] = []

    # -- infrastructure ---------------------------------------------------

    def path(self, *parts: str) -> str:
        """Resolves an install-layout path ("Data/quran-metadata.txt") in the legacy source tree.

        The legacy projects keep their data beside the code that loads it
        (LEGACY_LAYOUT); a release copies it into Data/, Values/ and Rules/.
        Reading the source tree means the repository holds one copy.
        """
        relative = os.path.normpath(os.path.join(*parts))
        head, _, rest = relative.partition(os.sep)
        bases = LEGACY_LAYOUT.get(head, [head])
        for base in bases:
            candidate = os.path.join(self.root, base, rest)
            if os.path.exists(candidate):
                return candidate
        return os.path.join(self.root, bases[0], rest)

    def register_source(self, key: str, name: str, kind: str, rel_path: str,
                        origin: str = "", license_: str = "") -> int:
        full = rel_path if os.path.isabs(rel_path) else self.path(rel_path)
        content_hash, size = sha256_file(full)
        cur = self.db.execute(
            "INSERT INTO sources (key, name, kind, version, origin, license,"
            " content_hash, byte_length, imported_utc)"
            " VALUES (?,?,?,?,?,?,?,?,?)",
            (key, name, kind, None, origin, license_, content_hash, size, utcnow()),
        )
        return cur.lastrowid

    def create_schema(self) -> None:
        schema_path = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                   "..", "schema", "content.sql")
        with io.open(schema_path, encoding="utf-8") as handle:
            self.db.executescript(handle.read())
        self.db.execute("INSERT INTO schema_version (version, applied_utc) VALUES (?,?)",
                        (SCHEMA_VERSION, utcnow()))

    # -- metadata ---------------------------------------------------------

    def read_metadata_sections(self) -> dict[str, list[list[str]]]:
        """quran-metadata.txt is several TSV tables stacked, each with a header.

        Sections are delimited by a header row whose first field is not numeric.
        """
        path = self.path("Data", "quran-metadata.txt")
        with io.open(path, encoding="utf-8-sig") as handle:
            lines = handle.read().splitlines()

        sections: dict[str, list[list[str]]] = {}
        current: str | None = None
        for line in lines:
            if not line.strip() or line.startswith("//"):
                continue
            fields = line.split("\t")
            if not fields[0].strip().isdigit():
                current = fields[0].strip()
                sections[current] = []
            elif current is not None:
                sections[current].append([f.strip() for f in fields])
        return sections

    def import_chapters(self, sections) -> None:
        rows = sections.get("chapter", [])
        if len(rows) != 114:
            self.errors.append(f"expected 114 chapters, found {len(rows)}")
        for r in rows:
            # chapter verses first_verse name transliterated english place order bowings
            self.db.execute(
                "INSERT INTO chapters (number, verse_count, first_verse, name,"
                " transliterated_name, english_name, revelation_place,"
                " revelation_order, bowing_count) VALUES (?,?,?,?,?,?,?,?,?)",
                (int(r[0]), int(r[1]), int(r[2]), r[3], r[4], r[5], r[6],
                 int(r[7]), int(r[8])),
            )
        print(f"  chapters           {len(rows)}")

    INITIALIZATION = {"Key": "key", "FullyInitialized": "full", "PartiallyInitialized": "partial"}

    def import_initialization(self, sections) -> None:
        """Which chapters open with Quranic initials (the metadata's initialization table).

        The legacy Book forces chapter 42, whose initials span two verses
        (حم, then عسق), to DoublyInitialized; the metadata lists it as fully
        initialized.
        """
        types: dict[int, str] = {}
        for row in sections.get("initialization", []):
            kind = self.INITIALIZATION.get(row[3])
            if kind is None:
                self.errors.append(f"unknown initialization type {row[3]!r}")
                continue
            types[int(row[1])] = kind
        types[42] = "double"
        self.db.executemany("UPDATE chapters SET initialization = ? WHERE number = ?",
                            [(kind, chapter) for chapter, kind in types.items()])
        counts = {k: list(types.values()).count(k) for k in sorted(set(types.values()))}
        print(f"  initialization     {counts}")

    def import_prostrations(self, sections) -> None:
        """Prostration verses by chapter and verse; one the edition lacks is reported."""
        rows = []
        for row in sections.get("prostration", []):
            chapter, verse, kind = int(row[1]), int(row[2]), row[3].strip().lower()
            if kind not in ("recommended", "obligatory"):
                self.errors.append(f"unknown prostration type {row[3]!r}")
                continue
            found = self.db.execute(
                "SELECT number FROM verses WHERE chapter_number = ? AND number_in_chapter = ?", (chapter, verse)).fetchone()
            if found is None:
                self.errors.append(f"prostration verse {chapter}:{verse} is not in this edition")
                continue
            rows.append((found[0], kind))
        self.db.executemany("INSERT INTO prostrations (verse_number, type) VALUES (?, ?)", rows)
        print(f"  prostrations       {len(rows)}")

    def import_submission(self) -> None:
        """Replaces the classic text with the Submission edition's.

        Chapter names, verse counts and revelation order come from the export;
        revelation place and bowing counts, which it does not carry, stay from
        the classic metadata.
        """
        directory = self.submission_dir
        rows = submission.load_rows(directory)
        chapters = submission.read_csv(directory, submission.CHAPTERS_FILE)
        for name in (submission.INDEX_FILE, submission.TEXT_FILE, submission.CHAPTERS_FILE):
            self.register_source(f"submission/{name}", name, "text",
                                 os.path.join(directory, name), origin=submission.ORIGIN)

        first_row: dict[int, int] = {}
        has_zero: set[int] = set()
        for row in rows:
            first_row.setdefault(row["chapter"], row["number"])
            if row["verse"] == 0:
                has_zero.add(row["chapter"])

        for c in chapters:
            number = int(c["chapter_number"])
            self.db.execute(
                "UPDATE chapters SET verse_count=?, first_verse=?, has_verse_zero=?,"
                " name=?, transliterated_name=?, english_name=?, revelation_order=?"
                " WHERE number=?",
                (int(c["chapter_verses"]), first_row[number], int(number in has_zero),
                 c["title_arabic"], c["title_transliterated"], c["title_english"],
                 int(c["revelation_order"]), number))

        for row in rows:
            self.db.execute(
                "INSERT INTO verses (number, chapter_number, number_in_chapter, text,"
                " stopmark, is_basmala) VALUES (?,?,?,?,?,?)",
                (row["number"], row["chapter"], row["verse"], row["text"], None,
                 int(row["verse"] == 0)))
        print(f"  verses             {len(rows)} ({len(has_zero)} verse-0 Bismillahs)")

        texts = {r["number"]: r["text"] for r in rows}
        verse_ids = {(r["chapter"], r["verse"]): r["number"] for r in rows}
        self.import_pause_marks(rows)
        self.import_submission_translations(rows, directory)

        rules = submission.read_verse_rules()
        for ordinal, (verse_id, find, replace_with, note) in enumerate(rules):
            chapter, verse = (int(x) for x in verse_id.split(":"))
            number = verse_ids.get((chapter, verse))
            if number is None or find not in texts[number]:
                self.errors.append(f"verse rule for {verse_id}: {find!r} not found in the verse")
                continue
            self.db.execute(
                "INSERT INTO verse_rules (verse_number, ordinal, find, replace_with, note)"
                " VALUES (?,?,?,?,?)", (number, ordinal, find, replace_with, note))
        print(f"  verse rules        {len(rules)}")

    def import_word_data(self) -> None:
        """Glosses, transliteration and grammar per display word (word_data.py)."""
        data = os.path.join("DataAccess", "Data", "77878")
        offline = os.path.join("DataAccess", "Translations", "Offline", "77878")
        self.register_source("words/glosses", "word by word English", "translation",
                             os.path.join(offline, "en.wordbyword.txt"), origin="qurandev.appspot.com, edited by Ali Adams")
        self.register_source("words/transliteration", "word transliteration", "translation",
                             os.path.join(offline, "en.transliteration.txt"), origin="tanzil.net")
        self.register_source("words/grammar", "Quranic Arabic Corpus morphology 0.4", "metadata",
                             os.path.join(data, "word-parts.txt"), origin="corpus.quran.com",
                             license_="GNU GPL; verbatim copies only; cite corpus.quran.com")
        counts = [int(r[1]) for r in self.read_metadata_sections().get("chapter", [])]
        word_data.import_word_data(self.db, self.root, counts, verse_zero=self.edition == "submission")

    def import_classic_texts(self) -> None:
        """The classic edition's standard-spelling text and verse transliteration, from Tanzil."""
        offline = os.path.join("DataAccess", "Translations", "Offline", "77878")
        counts = [int(r[1]) for r in self.read_metadata_sections().get("chapter", [])]
        texts = [
            ("ar.emlaaei.txt", "tanzil.emlaaei", "ar", "Standard spelling", "Tanzil", "emlaaei", "rtl", "CC BY 3.0 (Tanzil)"),
            ("en.transliteration.txt", "tanzil.translit", "en-Latn", "Transliteration", "Tanzil", "transliteration", "ltr", ""),
        ]
        for file, key, language, name, translator, kind, direction, license_ in texts:
            source = self.register_source(f"texts/{key}", name, "translation", os.path.join(offline, file),
                                          origin="tanzil.net", license_=license_)
            lines = word_data.verse_lines(self.path(offline, file), "utf-8-sig")
            if len(lines) != sum(counts):
                self.errors.append(f"{file}: {len(lines)} lines, expected {sum(counts)}")
                continue
            tid = self.db.execute(
                "INSERT INTO translations (key, language, name, translator, kind, direction, source_id, installed)"
                " VALUES (?,?,?,?,?,?,?,1)", (key, language, name, translator, kind, direction, source)).lastrowid
            self.db.executemany("INSERT INTO translation_text (translation_id, verse_number, text) VALUES (?,?,?)",
                                [(tid, i + 1, " ".join(line.split())) for i, line in enumerate(lines)])
        print(f"  verse texts        {len(texts)} from Tanzil")

    def import_submission_translations(self, rows: list[dict], directory: str) -> None:
        """The export's translations, its transliteration, and arabic_clean as the Emlaaei text.

        arabic_clean prefixes the Bismillah to verse 1 of chapters 2 to 114
        (not 9), which this edition holds as verse 0, so it is taken off there.
        """
        source = self.register_source("submission/translations", "WikiSubmission translations", "translation",
                                      os.path.join(directory, submission.TEXT_FILE), origin=submission.ORIGIN)
        specs = [spec for spec in submission.TRANSLATIONS]
        specs.append(("arabic_clean", "submission.emlaaei", "ar", "Standard spelling", "WikiSubmission", "emlaaei", "rtl"))
        for column, key, language, name, translator, kind, direction in specs:
            cur = self.db.execute(
                "INSERT INTO translations (key, language, name, translator, kind, direction, source_id, installed)"
                " VALUES (?,?,?,?,?,?,?,1)", (key, language, name, translator, kind, direction, source))
            tid = cur.lastrowid
            texts = []
            for row in rows:
                text = row["clean"] if column == "arabic_clean" else row["translations"][column]
                if column == "arabic_clean" and row["verse"] == 1 and row["chapter"] not in (1, 9):
                    words = text.split(" ")
                    if " ".join(words[:4]) == "بسم الله الرحمن الرحيم":
                        text = " ".join(words[4:])
                if text:
                    texts.append((tid, row["number"], text))
            self.db.executemany(
                "INSERT INTO translation_text (translation_id, verse_number, text) VALUES (?,?,?)", texts)
        count = self.db.execute("SELECT COUNT(*) FROM translation_text").fetchone()[0]
        print(f"  translations       {len(specs)} texts, {count} verse rows")

    def import_pause_marks(self, rows: list[dict]) -> None:
        """Pause marks (ۚ ۖ ۗ ...) for the Submission text, from the classic text.

        The export's arabic column has no pause marks; its arabic_clean column
        has them but spells words differently, so its words cannot be lined
        up reliably. The classic Tanzil text is the same script as the arabic
        column, so its words line up as the roots do (align_roots), and each
        word takes the mark written after its classic counterpart. Verse 0
        takes the marks of the Bismillah that opens classic verse 1.
        """
        classic = self.classic_marks()
        placed, skipped = 0, []
        for row in rows:
            chapter, verse = row["chapter"], row["verse"]
            source = classic.get((chapter, max(verse, 1)), [])
            if chapter not in (1, 9) and verse in (0, 1):
                source = source[:4] if verse == 0 else source[4:]
            links = align_roots(display_words(row["text"]), [(w, [i]) for i, (w, _) in enumerate(source)])
            if links is None:
                skipped.append(f"{chapter}:{verse}")
                continue
            # Where one classic word covers two display words, its mark goes on the second.
            last_of: dict[int, int] = {}
            for index, ids in links:
                for i in ids:
                    last_of[i] = index
            for i, index in last_of.items():
                mark = source[i][1]
                if mark:
                    self.db.execute(
                        "INSERT OR REPLACE INTO pause_marks (verse_number, word_index, mark) VALUES (?, ?, ?)",
                        (row["number"], index, mark))
                    placed += 1
        print(f"  pause marks        {placed} placed from the classic text, {len(skipped)} verses unaligned")
        if skipped:
            raise SystemExit(f"pause marks could not be aligned in {', '.join(skipped[:10])}")

    def classic_marks(self) -> dict[tuple[int, int], list[tuple[str, str | None]]]:
        """Each classic verse's display words with the pause mark after each."""
        with io.open(self.path("Data", "quran-uthmani.txt"), encoding="utf-8") as handle:
            texts = [l.strip() for l in handle if l.strip() and not l.startswith("#")]
        counts = [int(r[1]) for r in self.read_metadata_sections().get("chapter", [])]
        verses: dict[tuple[int, int], list[tuple[str, str | None]]] = {}
        index = 0
        for chapter, count in enumerate(counts, start=1):
            for verse in range(1, count + 1):
                words = display_words(texts[index])
                verses[(chapter, verse)] = [(w.split(" ")[0], mark_after(w)) for w in words]
                index += 1
        return verses

    def import_verses(self, source_id: int) -> None:
        path = self.path("Data", "quran-uthmani.txt")
        with io.open(path, encoding="utf-8") as handle:
            texts = [l.strip() for l in handle if l.strip() and not l.startswith("#")]

        if len(texts) != 6236:
            self.errors.append(f"expected 6236 verses, found {len(texts)}")

        # Map absolute verse number -> (chapter, number in chapter).
        chapters = self.db.execute(
            "SELECT number, verse_count FROM chapters ORDER BY number").fetchall()
        mapping: list[tuple[int, int]] = []
        for number, verse_count in chapters:
            for i in range(verse_count):
                mapping.append((number, i + 1))

        if len(mapping) != len(texts):
            self.errors.append(
                f"chapter verse counts sum to {len(mapping)} but text has {len(texts)}")

        stopmarks = self.read_stopmarks()
        for index, text in enumerate(texts):
            chapter_number, number_in_chapter = mapping[index]
            self.db.execute(
                "INSERT INTO verses (number, chapter_number, number_in_chapter,"
                " text, stopmark) VALUES (?,?,?,?,?)",
                (index + 1, chapter_number, number_in_chapter, text,
                 stopmarks.get(index + 1)),
            )
        print(f"  verses             {len(texts)}")

    def read_stopmarks(self) -> dict[int, str]:
        path = self.path("Data", "verse-stopmarks.txt")
        if not os.path.exists(path):
            return {}
        result: dict[int, str] = {}
        with io.open(path, encoding="utf-8-sig") as handle:
            for number, line in enumerate(handle, start=1):
                line = line.rstrip("\r\n")
                if line:
                    result[number] = line
        return result

    def import_partitions(self, sections) -> None:
        """Partition sections give a start point; the end is the next start - 1."""
        verse_index = {
            (c, v): n for n, c, v in self.db.execute(
                "SELECT number, chapter_number, number_in_chapter FROM verses")
        }
        total_verses = self.db.execute("SELECT COUNT(*) FROM verses").fetchone()[0]

        for section, kind in PARTITION_SECTIONS.items():
            rows = sections.get(section, [])
            if not rows:
                continue
            starts: list[tuple[int, int]] = []
            for r in rows:
                number = self.partition_start(verse_index, int(r[1]), int(r[2]))
                if number is None:
                    self.errors.append(f"{kind} {r[0]} references missing verse {r[1]}:{r[2]}")
                    continue
                starts.append((int(r[0]), number))

            for i, (number, first) in enumerate(starts):
                last = starts[i + 1][1] - 1 if i + 1 < len(starts) else total_verses
                self.db.execute(
                    "INSERT INTO partitions (kind, number, first_verse, last_verse)"
                    " VALUES (?,?,?,?)", (kind, number, first, last))
            print(f"  partitions/{kind:<8}{len(starts)}")

    @staticmethod
    def partition_start(verse_index: dict[tuple[int, int], int], chapter: int, verse: int) -> int | None:
        """Row a partition starting at chapter:verse begins on.

        A partition starting at a chapter's verse 1 includes its verse-0
        Bismillah. A start at a verse this edition does not have (9:128, 9:129)
        moves to the next verse that exists.
        """
        if verse == 1 and (chapter, 0) in verse_index:
            return verse_index[(chapter, 0)]
        if (chapter, verse) in verse_index:
            return verse_index[(chapter, verse)]
        later = [n for (c, v), n in verse_index.items() if (c, v) > (chapter, verse)]
        return min(later) if later else None

    # -- text modes -------------------------------------------------------

    def import_text_modes(self) -> None:
        total_rules = 0
        skipped_joins = 0
        encodings_seen: set[str] = set()
        for method in (77878, 77880):
            rules_dir = self.path("Rules", str(method))
            if not os.path.isdir(rules_dir):
                continue
            for mode in TEXT_MODES:
                rel = os.path.join("Rules", str(method), mode + ".txt")
                if not os.path.exists(self.path(rel)):
                    continue
                source_id = self.register_source(
                    f"rules/{method}/{mode}", f"{mode} rules ({method})", "rules", rel)
                cur = self.db.execute(
                    "INSERT INTO text_modes (name, word_count_method, research_only, source_id)"
                    " VALUES (?,?,?,?)",
                    (mode, method, int(mode in RESEARCH_ONLY_TEXT_MODES), source_id))
                mode_id = cur.lastrowid

                # Ordinal is significant: these are applied in sequence and
                # later rules depend on earlier ones having run.
                text, encoding = read_text_detect(self.path(rel))
                encodings_seen.add(f"{mode}:{encoding}")
                lines = text.splitlines()

                ordinal = 0
                for line in lines:
                    if not line.strip() or line.lstrip().startswith("#"):
                        continue
                    fields = line.split("\t")
                    if len(fields) < 2:
                        continue
                    find, replace_with = fields[0], fields[1]
                    if not find:
                        continue
                    if self.edition == "submission" and submission.is_word_join(find, replace_with):
                        skipped_joins += 1
                        continue
                    self.db.execute(
                        "INSERT INTO text_mode_rules (text_mode_id, ordinal, find,"
                        " replace_with) VALUES (?,?,?,?)",
                        (mode_id, ordinal, find, replace_with))
                    ordinal += 1
                total_rules += ordinal
        modes = self.db.execute("SELECT COUNT(*) FROM text_modes").fetchone()[0]
        print(f"  text modes         {modes} ({total_rules} rules)")
        if skipped_joins:
            print(f"    skipped {skipped_joins} word-joining rules: this edition's spacing is authoritative")
        mixed = {e.split(":")[1] for e in encodings_seen}
        if len(mixed) > 1:
            print(f"    note: rule files use mixed encodings {sorted(mixed)}")

    # -- value systems ----------------------------------------------------

    def import_value_systems(self) -> None:
        values_dir = self.path("Values")
        if not os.path.isdir(values_dir):
            self.errors.append("no Values directory")
            return

        extra_patterns = self.read_extra_system_patterns(values_dir)
        imported = 0
        research_only = 0
        encodings_seen: set[str] = set()
        for filename in sorted(os.listdir(values_dir)):
            if not filename.endswith(".txt"):
                continue
            name = filename[:-4]
            # Name is TextMode_LetterOrder_LetterValue.
            parts = name.split("_")
            if len(parts) < 3:
                continue
            text_mode_name, letter_order, letter_value = parts[0], parts[1], "_".join(parts[2:])

            rel = os.path.join("Values", filename)
            pairs: list[tuple[str, int]] = []
            # Values/ is encoding-inconsistent in the same way Rules/ is.
            value_text, encoding = read_text_detect(self.path(rel))
            encodings_seen.add(encoding)
            for line in value_text.splitlines():
                    line = line.rstrip("\r\n")
                    if not line or line.startswith("#"):
                        continue
                    fields = line.split("\t")
                    if len(fields) < 2:
                        continue
                    letter = fields[0]
                    try:
                        pairs.append((letter, int(fields[1])))
                    except ValueError:
                        continue
            if not pairs:
                continue

            source_id = self.register_source(f"values/{name}", name, "values", rel)
            is_extra = any(pattern in name for pattern in extra_patterns)
            research_only += is_extra
            cur = self.db.execute(
                "INSERT INTO value_systems (name, text_mode_name, letter_order,"
                " letter_value, letter_values_sum, research_only, source_id)"
                " VALUES (?,?,?,?,?,?,?)",
                (name, text_mode_name, letter_order, letter_value,
                 sum(v for _, v in pairs), int(is_extra), source_id))
            system_id = cur.lastrowid
            self.db.executemany(
                "INSERT OR IGNORE INTO value_map (value_system_id, letter, value)"
                " VALUES (?,?,?)",
                [(system_id, letter, value) for letter, value in pairs])
            imported += 1
        print(f"  value systems      {imported} ({research_only} research-only)")
        if len(encodings_seen) > 1:
            print(f"    note: value files use mixed encodings {sorted(encodings_seen)}")

    def read_extra_system_patterns(self, values_dir: str) -> list[str]:
        """Substrings that mark a value system as research-only.

        The legacy loads these from Values/_ExtraSystems.txt and skips any
        matching system in the Standard and Research editions
        (Server.LoadNumericalSystems). Substring match, as the legacy does.
        """
        path = os.path.join(values_dir, "_ExtraSystems.txt")
        if not os.path.isfile(path):
            return []
        text, _ = read_text_detect(path)
        patterns = []
        for line in text.splitlines():
            line = line.strip()
            if line and not line.startswith("#"):
                patterns.append(line)
        return patterns

    # -- waw words -------------------------------------------------------

    def import_waw_words(self) -> None:
        """Data/waw-words.txt: a word per line, optionally a tab and c:v,c:v verses."""
        rel = os.path.join("Data", "waw-words.txt")
        if not os.path.exists(self.path(rel)):
            self.errors.append("Data/waw-words.txt is missing")
            return
        self.register_source("data/waw-words", "waw words", "metadata", rel)
        text, _ = read_text_detect(self.path(rel))
        words = splits = 0
        for line in text.splitlines():
            fields = line.strip("\r\n").split("\t")
            word = fields[0].strip()
            if not word:
                continue
            self.db.execute("INSERT OR IGNORE INTO waw_words (word) VALUES (?)", (word,))
            words += 1
            if len(fields) > 1:
                for address in fields[1].split(","):
                    parts = address.strip().split(":")
                    if len(parts) == 2 and all(p.isdigit() for p in parts):
                        self.db.execute(
                            "INSERT OR IGNORE INTO waw_word_splits (word, chapter, verse) VALUES (?,?,?)",
                            (word, int(parts[0]), int(parts[1])))
                        splits += 1
        print(f"  waw words          {words} ({splits} verse exceptions)")

    # -- roots ------------------------------------------------------------

    def import_roots(self) -> None:
        rel = os.path.join("Data", str(DEFAULT_WORD_COUNT_METHOD), "word-roots.txt")
        if not os.path.exists(self.path(rel)):
            print("  roots              skipped (not present)")
            return
        self.register_source(f"roots/{DEFAULT_WORD_COUNT_METHOD}", "word roots",
                             "metadata", rel)

        roots: dict[str, int] = {}
        with io.open(self.path(rel), encoding="utf-8-sig") as handle:
            for line in handle:
                fields = line.rstrip("\r\n").split("\t")
                if len(fields) < 3:
                    continue
                for root_text in fields[2].split("|"):
                    root_text = root_text.strip()
                    if root_text and root_text not in roots:
                        cur = self.db.execute(
                            "INSERT INTO roots (text) VALUES (?)", (root_text,))
                        roots[root_text] = cur.lastrowid
        print(f"  roots              {len(roots)} distinct")
        self.link_word_roots(rel, roots)

    def link_word_roots(self, rel: str, roots: dict[str, int]) -> None:
        """Attach roots to each verse's display words.

        The legacy file keys words by classic chapter:verse:word and counts the
        Bismillah as the first four words of verse 1. A verse-zero edition holds
        those four words in verse 0 instead.
        """
        legacy: dict[tuple[int, int], list[tuple[str, list[int]]]] = {}
        with io.open(self.path(rel), encoding="utf-8-sig") as handle:
            for line in handle:
                fields = line.rstrip("\r\n").split("\t")
                if len(fields) < 3:
                    continue
                chapter, verse, _ = (int(x) for x in fields[0].split(":"))
                ids = [roots[r.strip()] for r in fields[2].split("|") if r.strip()]
                legacy.setdefault((chapter, verse), []).append((fields[1], ids))

        verse_zero = self.edition == "submission"
        rows: list[tuple[int, int, int]] = []
        unaligned: list[str] = []
        verses = self.db.execute(
            "SELECT number, chapter_number, number_in_chapter, text FROM verses ORDER BY number")
        for number, chapter, verse, text in verses.fetchall():
            source = legacy.get((chapter, max(verse, 1)), [])
            if verse_zero and chapter not in (1, 9) and verse in (0, 1):
                source = source[:4] if verse == 0 else source[4:]
            links = align_roots(display_words(text), source)
            if links is None:
                unaligned.append(f"{chapter}:{verse}")
                continue
            rows.extend((number, index, root) for index, ids in links for root in ids)
        self.db.executemany(
            "INSERT OR IGNORE INTO verse_word_roots (verse_number, word_index, root_id) VALUES (?, ?, ?)",
            rows)
        print(f"  word roots         {len(rows)} links, {len(unaligned)} verses unaligned")
        if unaligned:
            raise SystemExit(f"roots could not be aligned in {', '.join(unaligned[:10])}")

    # -- search -----------------------------------------------------------

    def build_fts(self) -> None:
        self.db.execute(
            "INSERT INTO verses_fts (rowid, text) SELECT number, text FROM verses")
        count = self.db.execute("SELECT COUNT(*) FROM verses_fts").fetchone()[0]
        print(f"  fts rows           {count}")

    # -- validation -------------------------------------------------------

    def validate(self) -> bool:
        """Check against the figures the legacy engine actually produced.

        These come from next/tests/golden/, captured by driving the legacy
        engine. They are not aspirational values.
        """
        verse_checks = (
            [("verses", "SELECT COUNT(*) FROM verses", 6236)]
            if self.edition == "classic" else
            [("rows", "SELECT COUNT(*) FROM verses", submission.EXPECTED_ROWS),
             ("bismillahs", "SELECT COUNT(*) FROM verses WHERE is_basmala=1", submission.EXPECTED_BASMALAS),
             ("numbered", "SELECT COUNT(*) FROM verses WHERE is_basmala=0", 6234),
             ("chapter 9", "SELECT verse_count FROM chapters WHERE number=9", 127),
             ("counts", "SELECT SUM(verse_count) FROM chapters", 6234)])
        checks = [
            ("chapters", "SELECT COUNT(*) FROM chapters", 114),
            *verse_checks,
            ("stations", "SELECT COUNT(*) FROM partitions WHERE kind='station'", 7),
            ("parts", "SELECT COUNT(*) FROM partitions WHERE kind='part'", 30),
            ("groups", "SELECT COUNT(*) FROM partitions WHERE kind='group'", 60),
            ("halfs", "SELECT COUNT(*) FROM partitions WHERE kind='half'", 120),
            ("quarters", "SELECT COUNT(*) FROM partitions WHERE kind='quarter'", 240),
            ("bowings", "SELECT COUNT(*) FROM partitions WHERE kind='bowing'", 556),
            ("pages", "SELECT COUNT(*) FROM partitions WHERE kind='page'", 604),
        ]
        print("\nvalidation against golden data:")
        ok = True
        for label, query, expected in checks:
            actual = self.db.execute(query).fetchone()[0]
            status = "ok " if actual == expected else "FAIL"
            if actual != expected:
                ok = False
            print(f"  [{status}] {label:<12} expected {expected:>6}  actual {actual:>6}")

        # Al-Fatiha structure, the project's canonical smoke test.
        verses = self.db.execute(
            "SELECT COUNT(*) FROM verses WHERE chapter_number=1 AND is_basmala=0").fetchone()[0]
        status = "ok " if verses == 7 else "FAIL"
        if verses != 7:
            ok = False
        print(f"  [{status}] al-fatiha    expected      7  actual {verses:>6}")
        return ok

    # -- driver -----------------------------------------------------------

    def run(self) -> int:
        if os.path.exists(self.db_path):
            os.remove(self.db_path)

        self.db = sqlite3.connect(self.db_path)
        self.db.execute("PRAGMA journal_mode = WAL")
        try:
            print(f"source : {self.root}")
            print(f"target : {self.db_path}\n")
            self.create_schema()

            sections = self.read_metadata_sections()
            self.register_source("quran-metadata", "Quran metadata", "metadata",
                                 os.path.join("Data", "quran-metadata.txt"),
                                 origin="tanzil.net", license_="cc-by")
            text_source = self.register_source(
                "quran-uthmani", "Quran Uthmani text", "text",
                os.path.join("Data", "quran-uthmani.txt"),
                origin="tanzil.net", license_="cc-by")

            self.import_chapters(sections)
            if self.edition == "submission":
                self.import_submission()
            else:
                self.import_verses(text_source)
            basmala = "verse-zero" if self.edition == "submission" else "prefix"
            self.db.executemany("INSERT INTO corpus (key, value) VALUES (?,?)",
                                [("edition", self.edition), ("basmala", basmala)])
            self.import_initialization(sections)
            self.import_prostrations(sections)
            self.import_partitions(sections)
            self.import_text_modes()
            self.import_value_systems()
            self.import_waw_words()
            self.import_roots()
            self.import_word_data()
            if self.edition == "classic":
                self.import_classic_texts()
            self.build_fts()

            self.db.commit()
            self.db.execute("VACUUM")

            ok = self.validate()

            if self.errors:
                print("\nerrors:")
                for error in self.errors:
                    print("  " + error)

            size_mb = os.path.getsize(self.db_path) / 1024 / 1024
            print(f"\n{os.path.basename(self.db_path)}  {size_mb:.1f} MB ({self.edition} edition)")
            return 0 if ok and not self.errors else 1
        finally:
            self.db.close()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--legacy-root", default=DEFAULT_LEGACY_ROOT,
                        help="the legacy QuranCode source tree (default: the repository's C# folder)")
    parser.add_argument("-o", "--output", default="content.db")
    parser.add_argument("--edition", choices=["classic", "submission"], default="classic")
    parser.add_argument("--submission-dir", default=submission.DEFAULT_DIR,
                        help="folder holding the ws_quran_*_rows.csv export "
                             "(default: next/data/sources/submission)")
    args = parser.parse_args()

    if not os.path.isdir(os.path.join(args.legacy_root, "Server", "Values")):
        print(f"not the legacy QuranCode source tree: {args.legacy_root}", file=sys.stderr)
        return 2
    if args.edition == "submission":
        names = (submission.INDEX_FILE, submission.TEXT_FILE, submission.CHAPTERS_FILE)
        missing = [n for n in names
                   if not args.submission_dir or not os.path.exists(os.path.join(args.submission_dir, n))]
        if missing:
            print(f"--submission-dir must contain {', '.join(missing)}", file=sys.stderr)
            return 2
    return Importer(args.legacy_root, args.output, args.edition, args.submission_dir).run()


if __name__ == "__main__":
    sys.exit(main())
