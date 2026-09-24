# How Khalifa counted alif

This note records what his own verse-by-verse data shows about counting the
letter alif (ا) in the 13 alif-initialed chapters, and why the alif findings
remain open.

## Sources

- **Khalifa's letter to Edip Yüksel, 27 November 1984.** When he began in
  1969 there were no rules for counting the letters: whether shadda counts as
  two letters, what to do with the sun and moon letters, whether to spell an
  initial out, and whether to count the hamza (ء), the hamza on yaa (ئ, as in
  أولئك) and the hamza on waw (ؤ, as in مؤمنون). "This is why we had
  differences in counting of certain letters." All the systems gave totals
  divisible by 19. The final rules are those of *Quran: Visual Presentation
  of the Miracle* (QVP), and counts not identical with it "should be
  considered NOT VALID". The letter also settles بسطة in 7:69: it is written
  with س, not ص.
- **QVP, pages 193 to 237**, the computer printout of the A, L and M (and R
  or S) counts for every verse of the 13 chapters, with running totals.
- **Quran Initial Count** (qurantalk.gitbook.io/quran-initial-count), which
  reconstructs the same totals with a spelling applied consistently word by
  word.

## The transcription

The alif column of all 13 chapters, 1,435 verses including each verse 0, is
in [`data/sources/qvp/alif-raw.txt`](../../data/sources/qvp/alif-raw.txt),
transcribed page by page from the scan. The printer's slashed zero cannot be
told from 8 at any resolution the scan allows, so
[`data/import/resolve_alif.py`](../../data/import/resolve_alif.py) reads each
0 or 8 as either and takes the reading nearest the engine's estimate for the
verse. Swapping 0 and 8 moves a value by 8 or 80 and the estimate is within a
few, so no choice was close. Every chapter then adds up exactly to its
printed total. The result is
[`data/sources/qvp/alif.tsv`](../../data/sources/qvp/alif.tsv), and
`InitialsTests.TheQvpAlifTableAddsUpToEachPublishedTotal` pins it to the
published figures.

On the first chapter-2 page, where all three columns were transcribed, every
ل and every م matches the engine verse for verse, which confirms the reading
and the alignment. Verse 0 shows A 3, L 4, M 3: Khalifa counted the Basmalah
and counted ٱ as alif.

One figure in the printout is inconsistent with itself: chapter 30's first
page prints a running total of 410 over a column that adds up to 400. The
chapter total, 544, is 400 + 144 and agrees with the column.

## What the data shows

**Khalifa's alif is the plain alif plus the hamza that stands on no seat.**
Counting every alif-seated form (ا أ إ آ ٱ), every standalone ء, and every
hamza written on a tatweel (ـٔ, as in يـَٔادم and شيـًٔا), and not the
seated ئ and ؤ:

| | Verses | Share |
| --- | ---: | ---: |
| Match his count exactly | 1,340 | 93.4% |
| Off by one | 88 | 6.1% |
| Off by two | 7 | 0.5% |

**This is an option the original already had.** Simplified29 drops the hamza
on a tatweel, but the original's Statistics panel offers "hamza above line"
(`CountingOptions.HamzaAboveLine`), which writes ـٔ as ـء before the letter
stage and so keeps it as a hamza. Khalifa's alif is then Simplified29 with
that option on, counting ا and ء together. No new text mode is needed.

Over all 13 chapters this gives exactly his 17,152. Chapters 13 and 15 come
out exact; the others differ by a few, in both directions, and cancel:

| Chapter | 2 | 3 | 7 | 10 | 11 | 12 | 13 | 14 | 15 | 29 | 30 | 31 | 32 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Computed minus his | +2 | -11 | -17 | +2 | +3 | +9 | 0 | +4 | 0 | -3 | -2 | +6 | +7 |

The command line reproduces the per-verse figures:
`qurancode letters اء --mode Simplified29 --hamza`.

**The remaining differences are not consistent by word.** ولئن is counted
with an extra alif in 8 of its 18 verses, أولٓئك in 4 of 49, هٓؤلآء in 3 of
13. No word-level rule can reproduce his printout verse by verse, which is
what his letter says happened, and what Quran Initial Count found: matching
his individual verse counts needs selective spelling, while his totals can be
reached with a consistent one.

## Consequences for the app

- A finding can carry this convention: `yes+hamza` in the catalog's
  basmalas column turns on the hamza above a line and counts ء wherever ا is
  counted (`Finding.HamzaAsAlif`). All the alif findings now use it.
- Four of them reproduce and are gated: the 13-chapter total of 17,152, the
  chapter figures for 13 (A.L.M.R., 1,482) and 15 (A.L.R., 912), and the
  A.L.M. total of 19,874. That last one holds only as a total: its six
  chapters' alifs come to one fewer than his, and chapter 30 has one ل more
  than his printout. The rest stay **open**, each off by the figure above.
- His verse-by-verse figures are now data. They can be shown beside the
  computed counts, but summing them is not a check: it reproduces his totals
  by construction.
- Reaching his chapter totals exactly with a consistent rule needs Quran
  Initial Count's word list, which it publishes only as images.

## The letter-frequency table

A table of whole-Quran letter frequencies (source not recorded) was checked
against the engine. Sixteen of its 28 letters match. The rest differ by
exact multiples of 112, the unnumbered Basmalahs, so the table counts
numbered verses only. Beyond that, س is one higher in the engine and ص one
lower: that is بسطة in 7:69, which the table counts with ص and the Submission
text writes with س, as Khalifa's letter says it should be. It also differs by
one و and uses a different alif convention (56,176 against the engine's
52,960).
