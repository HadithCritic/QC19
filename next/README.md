# QuranCode Next

A rebuild of QuranCode as a research engine with a desktop front end, rather
than a Windows application that performs research.

The legacy tree in `../C#/` is kept as the **behavioral oracle**: it is the
authority on what the software computes, and everything here is checked against
it rather than against a reading of its source.

## Layout

```
next/
  src/QuranCode.Core/      the engine: text, numerology, search, content
  apps/QuranCode.Cli/      command line over the same engine
  apps/QuranCode.Engine/   the engine as a JSON-lines sidecar (Native AOT)
  apps/desktop/            Tauri + Svelte desktop app over the sidecar
  tools/OracleDump/        drives the LEGACY engine to capture golden data
  tools/QuranCode.Bench/   performance measurement
  data/schema/             SQLite schema
  data/import/             content pipeline (Python, build-time only)
  data/sources/submission/ the Submission edition export; the authoritative text
  data/editions/           edition-specific word rules
  tests/golden/            captured legacy behavior; the contract
  tests/QuranCode.Core.Tests/
  tests/QuranCode.Engine.Tests/
  docs/
```

## Build

Needs the .NET 9 SDK, Python 3 (content pipeline only), and for the desktop
app Node 22+ with pnpm, Rust, and on Windows the MSVC build tools (Native AOT).

## Texts

The app uses the **Submission edition** as its authoritative text:
`data/sources/submission/ws_quran_*_rows.csv`, exported from
wikisubmission.org and imported without edits.

- Chapter 9 has 127 verses; 9:128 and 9:129 are not part of the text.
- The Bismillah of chapters 2 to 114 (except 9) is verse 0. It is counted by
  default and can be left uncounted with **Count Bismillah** in the top bar.
  Chapter 1's Bismillah is verse 1 and always counts.
- Word boundaries are the edition's own. The one counting rule layered on top
  lives in `data/editions/submission-verse-rules.tsv`: ما لم in 96:5 counts as
  one word, so 96:1-5 is 19 words.

The classic Tanzil text from the legacy install is still built, because the
golden tests compare the engine against the legacy software over that text.
`docs/compatibility/submission-vs-classic.md` lists every verse where the two
differ (30 verses).

To update the text, replace the three CSVs and rebuild `submission.db`.

```bash
# 1. Build both content databases. The install root supplies value systems,
#    text-mode rules and page boundaries; the Submission text comes from
#    data/sources/submission/.
python next/data/import/build_content.py <install-root> -o next/data/content.db
python next/data/import/build_content.py <install-root> --edition submission -o next/data/submission.db

# 2. Test the engine and the sidecar protocol.
dotnet test next/tests/QuranCode.Core.Tests
dotnet test next/tests/QuranCode.Engine.Tests

# 3. Run the desktop app (builds the sidecar, then opens the window).
cd next/apps/desktop && pnpm install && pnpm app

# Or the CLI, against either edition.
dotnet run --project next/apps/QuranCode.Cli -- stats --db next/data/submission.db
```

`pnpm dev` serves the UI in a browser against the same engine, for development.

`content.db` and `submission.db` are generated and not in version control. The
tests, the CLI and the desktop build report how to build them if missing.

## How correctness is established

The legacy assemblies still build, so `OracleDump` runs them headlessly and
records what they compute into `tests/golden/`. The new engine is then compared
against those files.

This is not a formality. Four behaviors that source reading would have gotten
wrong were caught this way:

1. **Normalization happens in two unrelated places.** Rule files handle word
   segmentation during book building; hardcoded `Simplify28..36` methods strip
   diacritics during valuation. Implementing either alone gives wrong numbers:
   Al-Fatiha comes out as 8283 instead of 8317.
2. **`AbsolutePositions` does nothing to the letter L position.** The legacy
   ternary returns the same field on both branches.
3. **Distances reset per chapter** and are baked in at book-build time, so the
   flag that controls them cannot be changed afterwards.
4. **The same calculation mode disagrees with itself.** A verse and the chapter
   containing it are computed by different code paths that do not match.

All four are reproduced rather than corrected, and documented, because
correcting any of them would silently change published results.

## Where things stand

| | |
| --- | --- |
| Tests | 150 engine, 33 protocol, 11 UI, 7 Rust; all passing |
| Startup | 3,938 ms → 266 ms |
| Heap after startup | 136 MB → 2.2 MB |
| Install | 615 MB → 35 MB |
| Per-verse valuation | **+42%, a known regression** |

See `docs/audit/performance-comparison.md` for the full table and
`docs/compatibility/feature-matrix.md` for all 79 legacy features with their
status.

## Documents

| | |
| --- | --- |
| `docs/decisions/0001-core-language-and-ui-stack.md` | Why the engine stays in C#, and why not Rust |
| `docs/decisions/0002-tauri-shell-over-dotnet-engine.md` | The Tauri app over a .NET engine sidecar |
| `docs/decisions/0003-editions-and-verse-zero.md` | Editions as databases; the Bismillah as verse 0 |
| `docs/compatibility/submission-vs-classic.md` | Every verse where the two texts differ |
| `docs/audit/findings.md` | What the legacy audit measured |
| `docs/audit/performance-comparison.md` | Before and after, including the regression |
| `docs/specs/text-normalization.md` | The two-stage pipeline, derived and verified |
| `docs/compatibility/feature-matrix.md` | All 79 features, classified |
