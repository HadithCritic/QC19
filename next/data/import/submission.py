"""The Submission edition: text, verse index and chapter metadata from the
wikisubmission.org export (ws_quran_*_rows.csv).

How it differs from the classic Tanzil text, all taken from the data itself:

- Chapter 9 has 127 verses; 9:128 and 9:129 are not part of this edition.
- The Bismillah of chapters 2..114 (except 9) is stored as its own verse 0,
  which the user may choose not to count. Chapter 1's Bismillah is verse 1.
- Orthography follows the edition: 7:69 has بسطة with sin, 68:1 spells نون.
- Word boundaries are the edition's own, so the classic rules that join words
  (such as "بعد ما") are not applied. Verse-scoped rules in
  data/editions/submission-verse-rules.tsv adjust counting where asked.

The export is the authoritative text of this edition. It lives in
next/data/sources/submission/ and is imported as it is.

Only the `arabic` column is used. `arabic_clean` is a different text (it
prefixes the Bismillah to verse 1 and writes 68:1 as ن), so it is ignored.
"""

from __future__ import annotations

import csv
import io
import os
import re

#: The authoritative copy of the export, kept in the repository.
DEFAULT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "sources", "submission")

INDEX_FILE = "ws_quran_index_rows.csv"
TEXT_FILE = "ws_quran_text_rows.csv"
CHAPTERS_FILE = "ws_quran_chapters_rows.csv"
ORIGIN = "wikisubmission.org"

EXPECTED_ROWS = 6346
EXPECTED_BASMALAS = 112

VERSE_RULES = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "editions",
                           "submission-verse-rules.tsv")

_LETTER = re.compile(r"[ء-يٱ-ۓ]")


def read_csv(directory: str, name: str) -> list[dict[str, str]]:
    with io.open(os.path.join(directory, name), encoding="utf-8", newline="") as handle:
        return list(csv.DictReader(handle))


def is_word_join(find: str, replace_with: str) -> bool:
    """A rule that merges two words: letters on both sides of a removed space.

    Rules that drop a space together with a pause mark (" ۚ" -> "") are not
    joins; the mark was never a word.
    """
    if " " not in find or find.count(" ") <= replace_with.count(" "):
        return False
    left, _, right = find.partition(" ")
    return bool(_LETTER.search(left)) and bool(_LETTER.search(right))


def load_rows(directory: str) -> list[dict[str, str]]:
    """Index and text rows joined by verse_index, in canonical order."""
    index = {int(r["verse_index"]): r for r in read_csv(directory, INDEX_FILE)}
    text = {int(r["verse_index"]): r for r in read_csv(directory, TEXT_FILE)}
    if set(index) != set(text):
        raise ValueError("index and text files disagree on verse_index values")

    numbers = sorted(index)
    if numbers != list(range(1, len(numbers) + 1)):
        raise ValueError("verse_index is not contiguous from 1")

    rows = []
    for n in numbers:
        i, t = index[n], text[n]
        if (i["chapter_number"], i["verse_number"]) != (t["chapter_number"], t["verse_number"]):
            raise ValueError(f"verse_index {n}: index and text name different verses")
        arabic = t["arabic"].strip()
        if not arabic:
            raise ValueError(f"verse {i['verse_id']} has no Arabic text")
        rows.append({
            "number": n,
            "chapter": int(i["chapter_number"]),
            "verse": int(i["verse_number"]),
            "text": arabic,
        })

    keys = [(r["chapter"], r["verse"]) for r in rows]
    if keys != sorted(keys):
        raise ValueError("verse_index order is not chapter:verse order")
    return rows


def read_verse_rules() -> list[tuple[str, str, str, str]]:
    """(verse_id, find, replace_with, note) rows; '#' lines are comments."""
    if not os.path.exists(VERSE_RULES):
        return []
    rules = []
    with io.open(VERSE_RULES, encoding="utf-8") as handle:
        for line in handle:
            line = line.rstrip("\r\n")
            if not line or line.startswith("#"):
                continue
            fields = line.split("\t")
            if len(fields) != 4:
                raise ValueError(f"{VERSE_RULES}: expected 4 tab-separated fields: {line!r}")
            rules.append((fields[0], fields[1], fields[2], fields[3]))
    return rules
