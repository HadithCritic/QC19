#!/usr/bin/env python3
"""Resolve Khalifa's transcribed verse-by-verse alif counts.

Input is data/sources/qvp/alif-raw.txt, transcribed from the computer printout
in Quran: Visual Presentation of the Miracle. The scan does not separate the
printer's slashed zero from 8, so each transcribed 0 or 8 may be either. A
cell is settled by the candidate nearest the engine's estimate for that verse
(plain alif plus standalone hamza, which is within a few of Khalifa's count;
swapping 0 and 8 moves a value by 8 or 80, so the choice is never close).
Every chapter must then add up to its printed total, and each page is checked
against its printed running total.

The estimate comes from the command line, in Simplified29, counting the
Basmalahs:

    dotnet run --project next/apps/QuranCode.Cli -- letters اء --mode Simplified29 \\
        --db next/data/submission.db > letters.tsv
    python next/data/import/resolve_alif.py next/data/sources/qvp/alif-raw.txt letters.tsv \\
        next/data/sources/qvp/alif.tsv
"""
import itertools
import sys


def variants(text):
    """Every reading of a transcribed number with each 0 or 8 as either."""
    options = [("0", "8") if c in "08" else (c,) for c in text]
    return sorted({int("".join(p)) for p in itertools.product(*options)})


def read_estimates(path):
    with open(path, encoding="utf-8-sig") as f:
        header = f.readline().rstrip("\r\n").split("\t")
        alif, hamza = header.index("ا"), header.index("ء")
        estimates = {}
        for line in f:
            p = line.rstrip("\r\n").split("\t")
            estimates[(int(p[0]), int(p[1]))] = int(p[alif]) + int(p[hamza])
    return estimates


def read_pages(path):
    pages = []
    with open(path, encoding="utf-8") as f:
        for line in f:
            if not line.strip() or line.startswith("#"):
                continue
            head, counts = line.split(":")
            chapter, page, first, total = head.split()
            pages.append((int(chapter), int(page), int(first), total, counts.split()))
    return pages


def main():
    raw_path, estimates_path = sys.argv[1], sys.argv[2]
    out_path = sys.argv[3] if len(sys.argv) > 3 else None
    estimates = read_estimates(estimates_path)
    pages = read_pages(raw_path)

    ok = True
    rows = []
    running = {}
    last_page = {chapter: page for chapter, page, *_ in pages}
    for chapter, page, first, total, counts in pages:
        page_sum = 0
        for i, text in enumerate(counts):
            verse = first + i
            estimate = estimates[(chapter, verse)]
            candidates = variants(text)
            best = min(candidates, key=lambda c: (abs(c - estimate), c))
            tied = [c for c in candidates if c != best and abs(c - estimate) == abs(best - estimate)]
            if tied or abs(best - estimate) > 6:
                ok = False
                print(f"  check {chapter}:{verse}: read {text}, took {best}, estimate {estimate}, tied {tied}")
            rows.append((chapter, verse, best))
            page_sum += best
        running[chapter] = running.get(chapter, 0) + page_sum
        holds = running[chapter] in variants(total)
        # The chapter total must hold. A running total on an earlier page is
        # only a check: the printout itself gets one wrong (chapter 30, page 1
        # prints 410 over a column that adds up to 400, and 400 + 144 is the
        # chapter's printed 544).
        if not holds and page == last_page[chapter]:
            ok = False
            status = "MISMATCH"
        elif not holds:
            status = f"printed running total {total}, the column gives {running[chapter]}"
        else:
            status = "ok"
        print(f"chapter {chapter}, page {page}, verses {first}-{first + len(counts) - 1}: "
              f"{page_sum}, running {running[chapter]} ({status})")

    if out_path:
        with open(out_path, "w", encoding="utf-8", newline="\n") as f:
            f.write("# Khalifa's alif count for each verse of the 13 alif-initialed chapters, from\n")
            f.write("# Quran: Visual Presentation of the Miracle, resolved by resolve_alif.py.\n")
            f.write("chapter\tverse\talif\n")
            for chapter, verse, alif in rows:
                f.write(f"{chapter}\t{verse}\t{alif}\n")
    print("all chapters hold" if ok else "SOME CHECKS FAILED")
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
