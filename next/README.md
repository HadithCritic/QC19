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
  apps/QuranCode.Desktop/  Avalonia shell with feature pages
  tools/OracleDump/        drives the LEGACY engine to capture golden data
  tools/QuranCode.Bench/   performance measurement
  data/schema/             SQLite schema
  data/import/             content pipeline (Python, build-time only)
  tests/golden/            captured legacy behavior; the contract
  tests/QuranCode.Core.Tests/
  docs/
```

## Build

Needs the .NET 9 SDK and Python 3 (for the content pipeline only).

```bash
# 1. Build the content database from the legacy data tree.
python next/data/import/build_content.py <quran-code-install-root> -o next/data/content.db

# 2. Build and test.
dotnet test next/tests/QuranCode.Core.Tests

# 3. Run.
dotnet run --project next/apps/QuranCode.Cli -- stats
dotnet run --project next/apps/QuranCode.Desktop
```

`content.db` is generated and not in version control. The tests and both apps
report how to build it if it is missing.

## How correctness is established

The legacy assemblies still build, so `OracleDump` runs them headlessly and
records what they compute into `tests/golden/`. The new engine is then compared
against those files.

This is not a formality. Four behaviors that source reading would have gotten
wrong were caught this way:

1. **Normalization happens in two unrelated places.** Rule files handle word
   segmentation during book building; hardcoded `Simplify28..36` methods strip
   diacritics during valuation. Implementing either alone gives wrong numbers —
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
| Tests | 46, all passing |
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
| `docs/decisions/0001-core-language-and-ui-stack.md` | Why C# and Avalonia, and why not Rust |
| `docs/audit/findings.md` | What the legacy audit measured |
| `docs/audit/performance-comparison.md` | Before and after, including the regression |
| `docs/specs/text-normalization.md` | The two-stage pipeline, derived and verified |
| `docs/compatibility/feature-matrix.md` | All 79 features, classified |
