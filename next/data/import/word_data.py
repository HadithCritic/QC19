"""Word meanings, transliteration and grammar for each display word.

The legacy data is keyed by the classic word list (Data/77878/word-roots.txt),
which counts the Bismillah as the first four words of verse 1:

- Translations/Offline/77878/en.wordbyword.txt: one line per verse, a
  gloss per word separated by tabs (Windows-1252).
- Translations/Offline/77878/en.transliteration.txt: one line per verse,
  a transliterated word per space.
- Data/77878/word-parts.txt: the Quranic Arabic Corpus morphology, one
  line per word part, keyed (chapter:verse:word:part). It does not repeat the
  Bismillah for verse 1; the legacy copies 1:1's parts there, and so does this.
- QuranCode/Languages/{English,Arabic}.txt: the names of the grammar tags
  (Dictionary Grammar_* rows, UTF-16).

Each edition's display words are lined up with the legacy words as the roots
are (alignment.align_roots): a legacy word split into two display words
gives both its data, and two legacy words joined into one display word give
it both of theirs.
"""

from __future__ import annotations

import io
import os
import re
import sqlite3

from alignment import align_roots, display_words

PART = re.compile(r"^\((\d+):(\d+):(\d+):(\d+)\)\t([^\t]*)\t([^\t]*)\t(.*)$")


def verse_lines(path: str, encoding: str) -> list[str]:
    with io.open(path, encoding=encoding) as handle:
        return [line.rstrip("\r\n") for line in handle if line.strip() and not line.startswith("#")]


def legacy_words(roots_path: str) -> dict[tuple[int, int], list[str]]:
    words: dict[tuple[int, int], list[str]] = {}
    with io.open(roots_path, encoding="utf-8-sig") as handle:
        for line in handle:
            fields = line.rstrip("\r\n").split("\t")
            if len(fields) < 2:
                continue
            chapter, verse, _ = (int(x) for x in fields[0].split(":"))
            words.setdefault((chapter, verse), []).append(fields[1])
    return words


def legacy_parts(path: str) -> dict[tuple[int, int, int], list[tuple[str, str, str]]]:
    parts: dict[tuple[int, int, int], list[tuple[str, str, str]]] = {}
    with io.open(path, encoding="utf-8-sig") as handle:
        for line in handle:
            match = PART.match(line.rstrip("\r\n"))
            if match:
                c, v, w, _ = (int(match.group(i)) for i in range(1, 5))
                parts.setdefault((c, v, w), []).append((match.group(5), match.group(6), match.group(7)))
    return parts


def grammar_labels(languages_dir: str) -> list[tuple[str, str, str]]:
    labels = []
    for language, code in (("English", "en"), ("Arabic", "ar")):
        path = os.path.join(languages_dir, f"{language}.txt")
        with io.open(path, encoding="utf-16") as handle:
            for line in handle:
                fields = line.rstrip("\r\n").split("\t")
                if len(fields) >= 3 and fields[0] == "Dictionary" and fields[1].startswith("Grammar_"):
                    labels.append((fields[1][len("Grammar_"):], code, fields[2]))
    return labels


def import_word_data(db: sqlite3.Connection, legacy_root: str, verse_counts: list[int], verse_zero: bool) -> None:
    """Fills word_glosses, word_parts and grammar_labels for the edition's verses."""
    offline = os.path.join(legacy_root, "DataAccess", "Translations", "Offline", "77878")
    data = os.path.join(legacy_root, "DataAccess", "Data", "77878")

    words = legacy_words(os.path.join(data, "word-roots.txt"))
    glosses = verse_lines(os.path.join(offline, "en.wordbyword.txt"), "cp1252")
    translit = verse_lines(os.path.join(offline, "en.transliteration.txt"), "utf-8-sig")
    parts = legacy_parts(os.path.join(data, "word-parts.txt"))

    # Per classic verse: each legacy word's gloss, transliteration and parts.
    classic: dict[tuple[int, int], list[tuple[str, str, str, list[tuple[str, str, str]]]]] = {}
    index = 0
    for chapter, count in enumerate(verse_counts, start=1):
        for verse in range(1, count + 1):
            texts = words[(chapter, verse)]
            g = glosses[index].split("\t")
            t = translit[index].split()
            prefixed = verse == 1 and chapter not in (1, 9)
            entries = []
            for i, text in enumerate(texts):
                if prefixed and i < 4:
                    word_parts = parts.get((1, 1, i + 1), [])
                else:
                    word_parts = parts.get((chapter, verse, i + 1 - (4 if prefixed else 0)), [])
                entries.append((text, g[i].strip() if i < len(g) else "", t[i] if i < len(t) else "", word_parts))
            classic[(chapter, verse)] = entries
            index += 1

    gloss_rows, part_rows, unaligned = [], [], []
    for number, chapter, verse, text in db.execute(
            "SELECT number, chapter_number, number_in_chapter, text FROM verses ORDER BY number").fetchall():
        source = classic.get((chapter, max(verse, 1)), [])
        if verse_zero and chapter not in (1, 9) and verse in (0, 1):
            source = source[:4] if verse == 0 else source[4:]
        links = align_roots(display_words(text), [(e[0], [i]) for i, e in enumerate(source)])
        if links is None:
            unaligned.append(f"{chapter}:{verse}")
            continue
        for word_index, ids in links:
            picked = [source[i] for i in ids]
            gloss_rows.append((number, word_index, " ".join(p[1] for p in picked if p[1]),
                               " ".join(p[2] for p in picked if p[2])))
            part_number = 0
            for entry in picked:
                for form, tag, features in entry[3]:
                    part_number += 1
                    part_rows.append((number, word_index, part_number, form, tag, features))

    db.executemany("INSERT OR REPLACE INTO word_glosses (verse_number, word_index, meaning, transliteration)"
                   " VALUES (?,?,?,?)", gloss_rows)
    db.executemany("INSERT OR REPLACE INTO word_parts (verse_number, word_index, part, form, tag, features)"
                   " VALUES (?,?,?,?,?,?)", part_rows)
    db.executemany("INSERT OR REPLACE INTO grammar_labels (tag, language, label) VALUES (?,?,?)",
                   grammar_labels(os.path.join(legacy_root, "QuranCode", "Languages")))
    print(f"  word data          {len(gloss_rows)} words, {len(part_rows)} grammar parts, {len(unaligned)} verses unaligned")
    if unaligned:
        raise SystemExit(f"word data could not be aligned in {', '.join(unaligned[:10])}")
