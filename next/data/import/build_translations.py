#!/usr/bin/env python3
"""Build an optional translation pack from the legacy Tanzil translations.

    python build_translations.py --edition-db ../submission.db -o ../submission-translations.db

The legacy tree ships 110 Tanzil translations (about 160 MB) under
DataAccess/Translations/Offline, one line per classic verse. They are too
large for the base install, so they go into a pack beside the edition's
content database, keyed by that edition's verse numbers. The engine opens
the pack when it is given one (--translations).

As in the original:
- The catalog is Offline/metadata.txt; a file with no catalog row (en.asad)
  is not offered.
- A chapter's Bismillah takes the translation of 1:1. The original prefixes
  it to verse 1 of chapters 2 to 114 (not 9); an edition that holds the
  Bismillah as verse 0 gets it there instead.
"""

from __future__ import annotations

import argparse
import hashlib
import io
import os
import sqlite3
import sys
from datetime import datetime, timezone

DEFAULT_LEGACY = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", "C#"))
PACK_SCHEMA_VERSION = 1

#: Languages the Tanzil catalog has that are written right to left.
RIGHT_TO_LEFT = {"ar", "fa", "ur", "ug", "sd", "dv", "ku"}

SCHEMA = """
CREATE TABLE pack (key TEXT PRIMARY KEY, value TEXT NOT NULL);
CREATE TABLE sources (
    id INTEGER PRIMARY KEY, key TEXT NOT NULL UNIQUE, name TEXT NOT NULL, kind TEXT NOT NULL,
    version TEXT, origin TEXT, license TEXT, content_hash TEXT NOT NULL, byte_length INTEGER NOT NULL,
    imported_utc TEXT NOT NULL
);
CREATE TABLE translations (
    id INTEGER PRIMARY KEY, key TEXT NOT NULL UNIQUE, language TEXT NOT NULL, name TEXT NOT NULL,
    translator TEXT NOT NULL, kind TEXT NOT NULL DEFAULT 'translation', direction TEXT NOT NULL DEFAULT 'ltr',
    source_id INTEGER NOT NULL REFERENCES sources(id), installed INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE translation_text (
    translation_id INTEGER NOT NULL REFERENCES translations(id),
    verse_number INTEGER NOT NULL,
    text TEXT NOT NULL,
    PRIMARY KEY (translation_id, verse_number)
);
"""


def read_lines(path: str) -> list[str]:
    raw = open(path, "rb").read()
    if raw.startswith(b"\xff\xfe") or raw.startswith(b"\xfe\xff"):
        text = raw.decode("utf-16")
    else:
        try:
            text = raw.decode("utf-8-sig")
        except UnicodeDecodeError:
            text = raw.decode("cp1252")
    # One line per verse, a verse with no translation included as an empty
    # line; the '#' footer (name, translator, source) follows the last verse.
    # Lines end at LF or CR LF only, as .NET's ReadAllLines reads them: some
    # files hold U+0085, which str.splitlines would also break at.
    lines = [" ".join(line.rstrip("\r").split()) for line in text.split("\n")]
    while lines and (not lines[-1] or lines[-1].startswith("#")):
        lines.pop()
    return lines


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--legacy-root", default=DEFAULT_LEGACY)
    parser.add_argument("--edition-db", required=True, help="the content database whose verse numbers the pack uses")
    parser.add_argument("-o", "--output", required=True)
    args = parser.parse_args()

    offline = os.path.join(args.legacy_root, "DataAccess", "Translations", "Offline")
    edition = sqlite3.connect(args.edition_db)
    edition_name = edition.execute("SELECT value FROM corpus WHERE key = 'edition'").fetchone()[0]
    numbers = {(c, v): n for n, c, v in edition.execute("SELECT number, chapter_number, number_in_chapter FROM verses")}
    zero = {c for c, v in numbers if v == 0}
    # The Tanzil files follow the classic verse numbering, which has 9:128-129.
    classic_counts = [n for (n,) in edition.execute("SELECT verse_count FROM chapters ORDER BY number")]
    classic_counts[8] = 129

    if os.path.exists(args.output):
        os.remove(args.output)
    pack = sqlite3.connect(args.output)
    pack.executescript(SCHEMA)
    pack.executemany("INSERT INTO pack (key, value) VALUES (?, ?)",
                     [("schema_version", str(PACK_SCHEMA_VERSION)), ("edition", edition_name),
                      ("built_utc", datetime.now(timezone.utc).isoformat())])

    with io.open(os.path.join(offline, "metadata.txt"), encoding="utf-8-sig") as handle:
        catalog = [row.rstrip("\r\n").split("\t") for row in handle][1:]

    added, skipped = 0, []
    for row in catalog:
        if len(row) < 4 or not row[0].strip():
            continue
        key, _, language_name, translator = (row[0].strip(), row[1], row[2].strip(), row[3].strip())
        path = os.path.join(offline, key + ".txt")
        if not os.path.exists(path):
            skipped.append(key)
            continue
        lines = read_lines(path)
        if len(lines) != sum(classic_counts):
            skipped.append(key)
            continue
        data = open(path, "rb").read()
        source = pack.execute(
            "INSERT INTO sources (key, name, kind, origin, license, content_hash, byte_length, imported_utc)"
            " VALUES (?,?,?,?,?,?,?,?)",
            (f"tanzil/{key}", f"{language_name}: {translator}", "translation", "tanzil.net",
             "Tanzil translation: attribution to tanzil.net; terms not stated in the file",
             hashlib.sha256(data).hexdigest(), len(data), datetime.now(timezone.utc).isoformat())).lastrowid
        code = key.split(".")[0]
        tid = pack.execute(
            "INSERT INTO translations (key, language, name, translator, kind, direction, source_id, installed)"
            " VALUES (?,?,?,?,?,?,?,1)",
            (f"tanzil.{key}", code, language_name, translator, "translation",
             "rtl" if code in RIGHT_TO_LEFT else "ltr", source)).lastrowid

        rows = []
        index = 0
        first_line = lines[0]
        for chapter, count in enumerate(classic_counts, start=1):
            for verse in range(1, count + 1):
                text = lines[index]
                index += 1
                if chapter in zero and verse == 1:
                    rows.append((tid, numbers[(chapter, 0)], first_line))
                elif chapter not in (1, 9) and verse == 1 and not text.startswith(first_line):
                    text = f"{first_line} {text}"
                number = numbers.get((chapter, verse))
                if number is not None:
                    rows.append((tid, number, text))
        pack.executemany("INSERT INTO translation_text (translation_id, verse_number, text) VALUES (?,?,?)", rows)
        added += 1

    pack.commit()
    pack.execute("VACUUM")
    pack.close()
    size = os.path.getsize(args.output)
    print(f"pack {args.output}: {added} translations for the {edition_name} edition, {size / 1e6:.1f} MB;"
          f" skipped {', '.join(skipped) or 'none'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
