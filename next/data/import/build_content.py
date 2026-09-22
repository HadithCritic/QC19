#!/usr/bin/env python3
"""Build content.db from the legacy QuranCode data tree.

A build-time tool. It never ships with the application; the application only
ever reads the database this produces.

Brief §13: canonical source -> validate -> normalize -> hash -> build. Every
source file is recorded in `sources` with a SHA-256 of its raw bytes, so any
row in the database can be traced back to the bytes it came from.

Brief §39: canonical content is copied verbatim. The importer never edits
Arabic text. Normalization is expressed as text-mode rules stored alongside the
text, applied later by the engine, never baked in here.

Usage:
    python build_content.py <install-root> [-o content.db]
"""

from __future__ import annotations

import argparse
import hashlib
import io
import os
import sqlite3
import sys
from datetime import datetime, timezone

SCHEMA_VERSION = 1
ALGORITHM_VERSION = 1

# Word count method: the legacy engine ships two segmentation variants
# (Data/77878 and Data/77880). 77878 is the default and matches the golden data.
DEFAULT_WORD_COUNT_METHOD = 77878

TEXT_MODES = [
    "Original", "Simplified28", "Simplified29", "Simplified30",
    "Simplified31", "Simplified36", "SimplifiedDots", "SimplifiedMarks",
]

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
    def __init__(self, root: str, db_path: str) -> None:
        self.root = os.path.abspath(root)
        self.db_path = db_path
        self.db: sqlite3.Connection | None = None
        self.errors: list[str] = []

    # -- infrastructure ---------------------------------------------------

    def path(self, *parts: str) -> str:
        return os.path.join(self.root, *parts)

    def register_source(self, key: str, name: str, kind: str, rel_path: str,
                        origin: str = "", license_: str = "") -> int:
        full = self.path(rel_path)
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
                key = (int(r[1]), int(r[2]))
                if key not in verse_index:
                    self.errors.append(f"{kind} {r[0]} references missing verse {key}")
                    continue
                starts.append((int(r[0]), verse_index[key]))

            for i, (number, first) in enumerate(starts):
                last = starts[i + 1][1] - 1 if i + 1 < len(starts) else total_verses
                self.db.execute(
                    "INSERT INTO partitions (kind, number, first_verse, last_verse)"
                    " VALUES (?,?,?,?)", (kind, number, first, last))
            print(f"  partitions/{kind:<8}{len(starts)}")

    # -- text modes -------------------------------------------------------

    def import_text_modes(self) -> None:
        total_rules = 0
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
                    "INSERT INTO text_modes (name, word_count_method, source_id)"
                    " VALUES (?,?,?)", (mode, method, source_id))
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
                    self.db.execute(
                        "INSERT INTO text_mode_rules (text_mode_id, ordinal, find,"
                        " replace_with) VALUES (?,?,?,?)",
                        (mode_id, ordinal, find, replace_with))
                    ordinal += 1
                total_rules += ordinal
        modes = self.db.execute("SELECT COUNT(*) FROM text_modes").fetchone()[0]
        print(f"  text modes         {modes} ({total_rules} rules)")
        mixed = {e.split(":")[1] for e in encodings_seen}
        if len(mixed) > 1:
            print(f"    note: rule files use mixed encodings {sorted(mixed)}")

    # -- value systems ----------------------------------------------------

    def import_value_systems(self) -> None:
        values_dir = self.path("Values")
        if not os.path.isdir(values_dir):
            self.errors.append("no Values directory")
            return

        imported = 0
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
            cur = self.db.execute(
                "INSERT INTO value_systems (name, text_mode_name, letter_order,"
                " letter_value, letter_values_sum, source_id) VALUES (?,?,?,?,?,?)",
                (name, text_mode_name, letter_order, letter_value,
                 sum(v for _, v in pairs), source_id))
            system_id = cur.lastrowid
            self.db.executemany(
                "INSERT OR IGNORE INTO value_map (value_system_id, letter, value)"
                " VALUES (?,?,?)",
                [(system_id, letter, value) for letter, value in pairs])
            imported += 1
        print(f"  value systems      {imported}")
        if len(encodings_seen) > 1:
            print(f"    note: value files use mixed encodings {sorted(encodings_seen)}")

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
        checks = [
            ("chapters", "SELECT COUNT(*) FROM chapters", 114),
            ("verses", "SELECT COUNT(*) FROM verses", 6236),
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
            "SELECT COUNT(*) FROM verses WHERE chapter_number=1").fetchone()[0]
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
            self.import_verses(text_source)
            self.import_partitions(sections)
            self.import_text_modes()
            self.import_value_systems()
            self.import_roots()
            self.build_fts()

            self.db.commit()
            self.db.execute("VACUUM")

            ok = self.validate()

            if self.errors:
                print("\nerrors:")
                for error in self.errors:
                    print("  " + error)

            size_mb = os.path.getsize(self.db_path) / 1024 / 1024
            print(f"\ncontent.db  {size_mb:.1f} MB")
            return 0 if ok and not self.errors else 1
        finally:
            self.db.close()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("root", help="QuranCode install root")
    parser.add_argument("-o", "--output", default="content.db")
    args = parser.parse_args()

    if not os.path.isdir(os.path.join(args.root, "Data")):
        print(f"not a QuranCode install root: {args.root}", file=sys.stderr)
        return 2
    return Importer(args.root, args.output).run()


if __name__ == "__main__":
    sys.exit(main())
