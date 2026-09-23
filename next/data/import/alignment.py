"""Lining up the words of two spellings of a verse, and reading its marks.

Shared by build_content.py (roots, pause marks) and word_data.py (glosses,
transliteration, grammar), which all place data keyed by the legacy word
list onto the display words of an edition's text.
"""

from __future__ import annotations


def display_words(text: str) -> list[str]:
    """Split a verse as DisplayWords.Split does: marks join the neighboring word."""
    words: list[str] = []
    leading: str | None = None
    for token in text.split(" "):
        if not token:
            continue
        if not any(ch.isalpha() for ch in token):
            if words:
                words[-1] = f"{words[-1]} {token}"
            else:
                leading = token if leading is None else f"{leading} {token}"
        else:
            words.append(token if leading is None else f"{leading} {token}")
            leading = None
    if leading is not None:
        words.append(leading)
    return words


def letters_of(word: str) -> str:
    # Tatweel is category Lm but only stretches a word; the editions differ in it.
    return "".join(ch for ch in word if ch.isalpha() and ch != "\u0640")


def align_roots(display: list[str], source: list[tuple[str, list[int]]]
                ) -> list[tuple[int, list[int]]] | None:
    """Pair display words with legacy words, allowing two-word joins either way.

    Equal counts pair by position (the Submission spellings of 7:69 and 68:1
    differ in letters, not in words). Otherwise one legacy word may cover two
    display words ("بعدما") or two legacy words one display word ("لوما").
    """
    if len(display) == len(source):
        return [(i, ids) for i, (_, ids) in enumerate(source)]
    links: list[tuple[int, list[int]]] = []
    d = s = 0
    while d < len(display) and s < len(source):
        here, there = letters_of(display[d]), letters_of(source[s][0])
        if here == there:
            links.append((d, source[s][1]))
            d, s = d + 1, s + 1
        elif d + 1 < len(display) and here + letters_of(display[d + 1]) == there:
            links += [(d, source[s][1]), (d + 1, source[s][1])]
            d, s = d + 2, s + 1
        elif s + 1 < len(source) and here == there + letters_of(source[s + 1][0]):
            links.append((d, source[s][1] + source[s + 1][1]))
            d, s = d + 1, s + 2
        else:
            return None
    return links if d == len(display) and s == len(source) else None


PAUSE_MARKS = "ۖۗۘۙۚۛۜ"


def mark_after(display_word: str) -> str | None:
    """The last pause mark attached to a display word (36:52 has two)."""
    found = [t for t in display_word.split(" ")[1:] if t in PAUSE_MARKS and len(t) == 1]
    return found[-1] if found else None
