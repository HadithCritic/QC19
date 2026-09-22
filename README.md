# QuranCode

A desktop research tool for the numerical study of the Quran: letter-value
(gematria-style) systems, number classification, live statistics for any
selection of verses, and Arabic text search.

This is a rebuild of **QuranCode 1433** by Ali Adams (qurancode.com), a
Windows Forms application. The original is kept in [`C#/`](C%23/) as the
reference for what the software computes; the new application is in
[`next/`](next/).

## What the app does today

- **Reader.** Chapters with one right-aligned verse per line. Each verse
  marker is ringed in the color of that verse's value class, so a chapter's
  numeric pattern is visible at a glance.
- **Live statistics.** Select a verse, a range or a chapter (click, shift-click,
  or type `2:255`, `2:255-257`, `1:7-2:2`, `2:0`) to see its value and its
  chapter, verse, word, letter and distinct-letter counts, each classified.
  Letter frequencies are listed beside them.
- **Number classes.** Every number is shown as unit (U), additive prime (AP),
  non-additive prime (XP), additive composite (AC) or non-additive composite
  (XC), with its digit sum, digital root, position among primes or composites
  (for example 619 is P114) and prime factors.
- **407 letter-value systems**, chosen by text mode, letter order and letter
  values. 122 research-only systems appear when **Research** is switched on.
- **Search.** Arabic text search anywhere in a word, as a whole word, or inside
  a word, with the matching words highlighted.
- **Values.** The value of any Arabic text in every system at once.
- **Numbers.** Look up any whole number.
- Light and dark themes.

Of the original's 79 listed features, 9 are done, 14 are in the engine without
a screen yet, 47 are still to do, and 7 wait on data that is not imported yet.
[`next/docs/compatibility/feature-matrix.md`](next/docs/compatibility/feature-matrix.md)
tracks each one, along with the 16 standalone tools the original shipped.

## The text

The app uses the **Submission edition**, exported from wikisubmission.org and
kept in [`next/data/sources/submission/`](next/data/sources/submission/). It is
the authoritative text and is imported without edits.

- Chapter 9 has 127 verses. 9:128 and 9:129 are not part of the text.
- The Bismillah of chapters 2 to 114, except 9, is **verse 0**. It is counted by
  default and can be left out with **Count Bismillah** in the top bar. Chapter
  1's Bismillah is its verse 1 and always counts.
- Word boundaries are the edition's own. One counting rule is layered on top,
  in [`next/data/editions/submission-verse-rules.tsv`](next/data/editions/submission-verse-rules.tsv):
  ما لم in 96:5 counts as one word, so 96:1-5 is 19 words.

| | Bismillahs counted | Not counted |
| --- | ---: | ---: |
| Verses | 6,346 | 6,234 |
| Words | 77,850 | 77,402 |
| Letters | 327,662 | 325,534 |

The classic Tanzil text from the original is still built, because the tests
compare the new engine against the original software over that text.
[`next/docs/compatibility/submission-vs-classic.md`](next/docs/compatibility/submission-vs-classic.md)
lists the 30 verses where the two texts differ.

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
dotnet test next/tests/QuranCode.Core.Tests      # engine: 150 tests
dotnet test next/tests/QuranCode.Engine.Tests    # protocol: 33 tests
cd next/apps/desktop && pnpm test                # interface logic: 11 tests
cd next/apps/desktop/src-tauri && cargo test     # Rust bridge: 7 tests
```

To record new golden data, build the original solution (`C#/Solution.sln`,
which needs the .NET Framework 4.0 targeting pack) and run OracleDump with its
output folder, `C#/Build/Release`, as the install root.

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
| [Feature matrix](next/docs/compatibility/feature-matrix.md) | All 79 original features and 16 tools, with status |
| [Submission vs classic](next/docs/compatibility/submission-vs-classic.md) | Every verse where the two texts differ |
| [Text normalization](next/docs/specs/text-normalization.md) | The two-stage pipeline, verified |
| [Audit findings](next/docs/audit/findings.md) | What was measured in the original |
| [Performance](next/docs/audit/performance-comparison.md) | Before and after |

## License

GPL-3.0, the license of the original QuranCode; see [LICENSE](LICENSE). The
interface fonts (Amiri, Amiri Quran, Instrument Sans, IBM Plex Mono) are under
the SIL Open Font License. The classic text comes from tanzil.net. The
Submission edition export carries no license statement; its terms should be
confirmed before a public release.

The original QuranCode is the work of Ali Adams: http://qurancode.com and
http://heliwave.com.
