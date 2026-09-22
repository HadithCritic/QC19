# ADR 0003: Editions as content databases; the Bismillah as verse 0

Status: accepted
Date: 2026-09-22

## Context

The owner wants the application to use the Submission edition
(wikisubmission.org export, `ws_quran_*_rows.csv`) instead of the classic
Tanzil text:

- chapter 9 has 127 verses: 9:128 and 9:129 are excluded;
- the Bismillah of chapters 2..114 (except 9) is verse 0, counted by default
  and excludable by the user, as the legacy `with_bism_Allah` option allowed;
  chapter 1's Bismillah is verse 1 and cannot be excluded;
- its orthography differs (7:69 بسطة with sin, 68:1 نون spelled out, and 27
  more verses listed in `docs/compatibility/submission-vs-classic.md`);
- ما لم in 96:5 counts as one word, so 96:1-5 is 19 words.

The classic text must stay: it is what the golden tests compare against the
legacy engine, and those tests are the evidence the engine computes correctly.

## Decision

1. **An edition is a content database.** `build_content.py --edition
   submission` builds `data/submission.db` with the same schema as the classic
   `data/content.db`. The export is kept in the repository at
   `data/sources/submission/` and is the authoritative text of the app.
   Text, verse index and chapter names come from it;
   value systems, text-mode rules and page/part/bowing boundaries come from the
   legacy source tree in `C#/`, remapped onto the new verse numbering. The app ships the
   Submission database; `QURANCODE_EDITION=classic` stages the classic one.
2. **The database describes itself.** A `corpus` table records the edition and
   the Bismillah convention (`prefix` or `verse-zero`). The engine reads it and
   has no edition-specific code paths beyond that switch.
3. **Verse numbers are absolute and stable.** Submission rows are numbered
   1..6346 with the verse-0 Bismillahs included. Not counting the Bismillahs
   never renumbers anything: the engine builds a `CorpusView` without them and
   every count, value, position, distance and search runs over that view. A
   selection therefore survives the setting being switched.
4. **Only the `arabic` column is used.** `arabic_clean` is a different text
   (it prefixes the Bismillah to verse 1 and writes 68:1 as ن).
5. **The edition's spacing is authoritative.** The classic rules that join
   words (such as بعد ما) are not imported for this edition. Word-boundary
   changes the owner asks for are verse-scoped rows in
   `data/editions/submission-verse-rules.tsv`, applied before the text mode's
   rules. The stored text is never edited, so the reader still shows مَا لَمْ.

## Consequences

- Classic behavior is unchanged: all golden tests still pass against
  `content.db`.
- Tests now need both databases. Each fails with the command that builds it.
- Pages, parts and bowings are the classic Madani boundaries mapped onto this
  edition. A chapter's first partition includes its verse 0; a boundary at a
  verse this edition lacks moves to the next verse that exists.
- The word-roots data (`Data/77878/word-roots.txt`) is keyed to the classic
  segmentation and is not yet linked to words in either edition.
- The export carries no license statement. Confirm the terms for the Arabic
  text and the translations before a public release.
