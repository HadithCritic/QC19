# QuranCode

A desktop research tool for **Code 19**, Rashad Khalifa's numerical analysis
of the Quran: finding multiples of 19 in counts and values, and checking
published results against the text. It also carries the general machinery that
serves that work: letter-value (gematria-style) systems, number
classification, live statistics for any selection of verses, and Arabic text
search.

This is a rebuild of **QuranCode 1433** by Ali Adams (qurancode.com), a
Windows Forms application. The original is kept in [`C#/`](C%23/) as the
reference for what the software computes; the new application is in
[`next/`](next/).

## What the app does today

Eight screens: **Read**, **Search**, **Values**, **Stats**, **Findings**,
**Initials**, **Saved** and **Numbers**.

### Read

- Chapters with one right-aligned verse per line. Each verse marker is ringed
  in the color of that verse's value class, so a chapter's numeric pattern is
  visible at a glance.
- Select a verse, a range or a chapter by clicking, shift-clicking, or typing a
  reference: `2:255`, `2:255-257`, `1:7-2:2`, `2:0`, a chapter range `3-4`, or
  a unit such as `page 1`, `part 30`, `word 40`, `letter 139`.
- Click a word for its root, its English meaning, its transliteration and its
  grammar from the Quranic Arabic Corpus. Alt+click a second word to measure
  the distance between them in chapters, verses, words and letters.
- Rashad Khalifa's English translation under each verse, with the
  transliteration. Per ADR 0004 these are the only translations, together with
  the Emlaaei standard spelling, which serves search rather than reading.
- Ratio coloring: split a verse, chapter, partition or the whole book at 1/π,
  1/e, 1/φ or any ratio, by letters or by value.
- Back and forward through selections; F3 steps through bookmarks.

### Search

- Arabic text: several terms, any of them, all of them, or as an exact phrase,
  with `+must` and `-must-not`; anywhere in a word, as a whole word, or only
  inside a longer word. Scope is the book, the current selection, or the
  previous results.
- A letter palette for typing without an Arabic keyboard. It offers only
  the characters the active text mode keeps, so it never builds a search
  that cannot match.
- Roots, with multiple roots and any/all grouping. Ctrl+click or F4 searches
  the clicked word's longest root.
- Related verses (F5), similar verses by text, words, roots or values with an
  adjustable threshold (F6), the same word (F7), the same text with its marks
  (F8), and the same value (F9).
- By numbers: words, verses, sentences, chapters and the seven partitions,
  singly, in runs of neighbors or in sets, over counts, values and 18 number
  kinds, with every comparison operator.
- By letter frequency sum, with all/any/only/none letter matching.
- Translations, chosen automatically by what you type: all-Arabic text searches
  the Arabic, anything else searches the translations.
- The chapter list shades by how many matches each chapter holds.

### Findings

Khalifa's published results, each recomputed from the text and marked as
holding or not. A finding shows the counting rule it was computed under, the
convention (whether the 112 unnumbered Basmalahs count), the text mode and the
appendix it comes from. It also says explicitly when the rule was **inferred**
rather than stated by the source, so a reader can see what rests on an
assumption. Multiples of 19 are shown with their multiplier.

The catalog is [one data file](next/src/QuranCode.Core/Code19/findings.tsv)
of 53 findings, from Appendix 1 and from Khalifa's tables in *The Computer
Speaks* as tabulated by [Quran Initial Count](https://qurantalk.gitbook.io/quran-initial-count).
Every row runs as a test: a gated finding that stops reproducing fails the
build. Nineteen are marked **open** and shown with their gap rather than hidden
or edited to agree. Seventeen of them involve alif. Khalifa's verse-by-verse
printout, transcribed from *Quran: Visual Presentation of the Miracle*, shows
he also counted the hamza that stands on no seat as alif, which reproduces 93%
of his verses and his alif total to within 2, with the rest decided verse by
verse ([how he counted alif](next/docs/research/alif-counting.md)). The other
two are ل in chapters 11 and 30, where his printout has one ل fewer than the
text.

### Initials

The 29 chapters that open with Quranic Initials, their 14 letters, and how
often each chapter's own initials occur in it, beside Khalifa's figure for
each. 63 of the 78 reproduce; the 15 that do not are the 13 alifs and the two
ل counts above.

### Values, Stats and Numbers

- **Values.** Any Arabic text valued across the letter-value systems at once.
  A core set shows by default; the rest of the 407 appear when **Research** is
  switched on. The 20 Base systems read a word's letters as digits in that
  base, as the original does.
- **Stats.** A Multiples tab that lists every total of the selection and
  sets apart the ones divisible by 19 (or any divisor): counts, value, the
  sums of chapter and verse numbers, and each letter. Then word frequencies, letter statistics, the Maths sums (C, V, C±V,
  C×V, C÷V split by odd, even, prime and composite), front-back symmetry, and
  the research lists (Allah words, look-alikes, doubles, repeats).
- **Numbers.** A calculator: type 19*142, 2^19-1, 114C2 or Arabic text, and
  the result is looked up. Whole numbers stay exact. Any whole number: its
  class (U, AP, XP, AC, XC), digit sum, digital root, position among primes or
  composites (619 is P114), factors,
  divisors, sums and differences of two squares and two cubes, and Waleed's
  CP index chain. Numbers are shown and read in any base from 2 to 36, and
  numbers divisible by a chosen divisor (default 19) are marked.

### Throughout

- **Saved.** Bookmarks with notes, and browse and find history, in a separate
  `user.db` that is never the content database.
- **Counting options** from the original's Statistics panel: the Bismillah, waw
  as a word, shadda as a letter, and hamza, alif, yaa and noon above a line as
  letters. Each is checked against the original across four text modes
  (`next/tests/golden/counting-options.tsv`).
- Light, dark and automatic themes.

## Scope

[ADR 0004](next/docs/decisions/0004-code-19-scope.md) narrowed the project to
Code 19. Audio, drawing, the standalone tools except InitialLetters, multiple
Arabic fonts, DNA symbols and the geometry calculators are **out of scope**.

Of the original's 79 features, 63 are built and 13 are out of scope. The
[feature matrix](next/docs/compatibility/feature-matrix.md) keeps a row for
every one of them, including those now out of scope, as an inventory of what
the original did, not as a list of work remaining. What is planned is in the
[roadmap](next/docs/roadmap.md).

Thirty-four published findings reproduce exactly from the Submission text,
among them: the word God occurs **2,698** times (19×142) and the verse numbers
of those verses sum to **118,123** (19×6,217); ق occurs **57** times in both
chapter 50 and chapter 42; ن occurs **133** times in chapter 68 (19×7); and
the seven حم chapters hold **2,147** of those two letters (19×113); and the
first revelation, 96:1–5, is **19** words and **76** letters. Each is checked
on every build.

## The text

The app uses the **Submission edition**, exported from wikisubmission.org and
kept in [`next/data/sources/submission/`](next/data/sources/submission/). It is
the authoritative text and is imported without edits.

- Chapter 9 has 127 verses. 9:128 and 9:129 are not part of the text.
- The Bismillah of chapters 2 to 114, except 9, is **verse 0**. It is counted by
  default and can be left out from the **Counting** menu in the top bar, in
  every text mode. Chapter 1's Bismillah is its verse 1 and always counts.
- Word boundaries are the edition's own. One counting rule is layered on top,
  in [`next/data/editions/submission-verse-rules.tsv`](next/data/editions/submission-verse-rules.tsv):
  ما لم in 96:5 counts as one word, so 96:1-5 is 19 words.

| | Bismillahs counted | Not counted |
| --- | ---: | ---: |
| Verses | 6,346 | 6,234 |
| Words | 77,850 | 77,402 |
| Letters | 327,664 | 325,536 |

The classic Tanzil text from the original is still built, because the tests
compare the new engine against the original software over that text.
[`next/docs/compatibility/submission-vs-classic.md`](next/docs/compatibility/submission-vs-classic.md)
lists the 28 verses where the two texts differ.

To update the text, replace the three CSV files and rebuild the database.

## Getting started

You need:

- the [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Python 3.10 or later (build time only)
- Node.js 22 or later with pnpm
- Rust (stable) and, on Windows, the Visual Studio C++ build tools, which the
  engine's native (AOT) build links with

Build the two content databases. They are generated, not stored in git:

```bash
python next/data/import/build_content.py -o next/data/content.db
python next/data/import/build_content.py --edition submission -o next/data/submission.db
```

Run the desktop app. The first run compiles the engine and the Rust shell,
which takes several minutes:

```bash
cd next/apps/desktop
pnpm install
pnpm app
```

`pnpm dev` serves the same interface in a browser, against the same engine,
for development. The command line reads either database:

```bash
dotnet run --project next/apps/QuranCode.Cli -- stats --db next/data/submission.db
```

## How it is built

```
Tauri app (next/apps/desktop)
  Svelte + TypeScript interface
  Rust bridge: one "engine" command; owns the engine process, times out
  requests and restarts the engine if it stops
        │  one JSON request and response per line over stdin/stdout
qurancode-engine (next/apps/QuranCode.Engine)
  .NET, compiled with Native AOT, so no .NET install is needed to run it
  QuranCode.Core: text normalization, valuation, number theory, search
        │
content database (SQLite, read-only)
```

The engine stayed in C# because the original's valuation rules are large and
subtle, and a line-for-line port in the same language can be checked against
the original directly. The reasoning is in
[ADR 0001](next/docs/decisions/0001-core-language-and-ui-stack.md) and
[ADR 0002](next/docs/decisions/0002-tauri-shell-over-dotnet-engine.md).

## How correctness is established

`next/tools/OracleDump` runs the original engine headlessly and records what it
computes in `next/tests/golden/`. The new engine is tested against those
files. That caught four behaviors a reading of the source would have missed:

1. **Normalization happens in two unrelated places.** Rule files segment words;
   hardcoded methods strip diacritics. Implementing only one gives Al-Fatiha
   8283 instead of 8317.
2. **`AbsolutePositions` does nothing to the letter position.** Both branches
   of the original's condition return the same field.
3. **Distances reset per chapter** and are fixed when the text is built.
4. **One calculation mode disagrees with itself.** A verse and its chapter go
   through different code paths that do not match.

All four are reproduced, not corrected, because correcting them would change
published results. The figures in the original's Readme are tests too: for
example, there are 16 additive primes up to 114 and 131 non-additive
composites up to 506.

Run the tests:

```bash
dotnet test next/tests/QuranCode.Core.Tests      # engine: 383 tests
dotnet test next/tests/QuranCode.Engine.Tests    # protocol: 83 tests
cd next/apps/desktop && pnpm test                # interface logic: 69 tests
cd next/apps/desktop && pnpm check               # types: 339 files, 0 errors
cd next/apps/desktop/src-tauri && cargo test     # Rust bridge: 7 tests
```

The engine and protocol tests need both content databases built first.

`next/tools/oracle.py` builds the original (with Microsoft's .NET Framework 4.0
reference assemblies from NuGet and the Visual Studio Build Tools, so no
developer pack is needed) and runs OracleDump against its output,
`C#/Build/Release`:

```bash
python next/tools/oracle.py --check          # regenerate and compare with next/tests/golden
python next/tools/oracle.py --out <folder>   # regenerate into a folder
```

## Repository layout

```
next/
  apps/desktop/            Tauri app: Svelte interface and Rust bridge
  apps/QuranCode.Engine/   the engine as a sidecar process
  apps/QuranCode.Cli/      command line
  src/QuranCode.Core/      the engine
  data/sources/submission/ the Submission edition text
  data/editions/           edition word rules
  data/import/             builds the content databases (Python)
  data/schema/             database schema
  tests/                   engine and protocol tests, and golden data
  tools/                   OracleDump and a benchmark
  docs/                    decisions, audits, specifications
C#/                        the original QuranCode 1433 source and data
```

The build reads the letter-value systems, text-mode rules, word roots and page
boundaries from the original's data in `C#/`.

## Documents

| | |
| --- | --- |
| [ADR 0001](next/docs/decisions/0001-core-language-and-ui-stack.md) | Why the engine stays in C# |
| [ADR 0002](next/docs/decisions/0002-tauri-shell-over-dotnet-engine.md) | The Tauri app over a .NET engine |
| [ADR 0003](next/docs/decisions/0003-editions-and-verse-zero.md) | Editions as databases; the Bismillah as verse 0 |
| [ADR 0004](next/docs/decisions/0004-code-19-scope.md) | **Code 19 as the purpose; scope cut to match** |
| [Feature matrix](next/docs/compatibility/feature-matrix.md) | All 79 original features and 16 tools, with status |
| [Submission vs classic](next/docs/compatibility/submission-vs-classic.md) | Every verse where the two texts differ |
| [Text normalization](next/docs/specs/text-normalization.md) | The two-stage pipeline, verified |
| [Audit findings](next/docs/audit/findings.md) | What was measured in the original |
| [Performance](next/docs/audit/performance-comparison.md) | Before and after |
| [Roadmap](next/docs/roadmap.md) | What is planned, in stages |
| [How Khalifa counted alif](next/docs/research/alif-counting.md) | His verse-by-verse printout, transcribed and analyzed |
| [Work log](next/docs/worklog.md) | What was built in each phase, and every judgment call |

## License

GPL-3.0, the license of the original QuranCode; see [LICENSE](LICENSE). The
interface fonts (Amiri Quran, Instrument Sans, IBM Plex Mono) are under
the SIL Open Font License. The classic text comes from tanzil.net. The
Submission edition export carries no license statement; its terms should be
confirmed before a public release.

The original QuranCode is the work of Ali Adams: http://qurancode.com and
http://heliwave.com.
