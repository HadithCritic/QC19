# Audit findings

Covers the local QuranCode 1433 B89 tree. Every number here was measured or
counted, not estimated. Reproduce with:

```
next/tools/OracleDump/bin/Release/OracleDump.exe <install-root> --bench
```

## Scale

| | |
| --- | --- |
| Install size | 615 MB |
| Source | 243,170 lines C# across 210 files (after removing 80k lines of dead source) |
| Solution projects | 31 |
| Shipped executables | 17 |
| Shipped DLLs | 13 |
| Tests | 0 |

## Where the bloat is

| Directory | Size | Assessment |
| --- | --- | --- |
| `Translations/` | 156 MB | 112 plaintext translations, always shipped. → optional packs |
| `Numbers/` | 63 MB | Precomputed primes/composites as decimal ASCII, 6.65M lines. → sieve at runtime |
| `Help/` | 40 MB | Bundled PDFs and JPEGs. → fetch on demand |
| `Data/` | 20 MB | Canonical text, dictionaries, metadata. → keep, this is the real content |
| `Values/` | 738 KB | 410 value systems. → keep, this is the crown jewel |
| `Rules/` | 72 KB | 16 text-mode rule sets. → keep, this is the other crown jewel |

Data, not code, is ~85% of the install.

## Where the time goes

Engine only. No WinForms, no splash screen, no control creation. Real startup
is at least this, never less.

| Step | Time | Heap after |
| --- | --- | --- |
| Client construct (loads 274 value systems) | 134 ms | 5.9 MB |
| **Build book** | **3,804 ms** | **136.4 MB** |
| Rebuild book | 3,267 ms | 203.5 MB |
| Value whole book | **34 ms** | — |
| Value all 6,236 verses | 57 ms | — |
| Switch value system | 1,811 ms | 272.4 MB |

**Building the graph costs 112x more than computing over it.** This is the
finding the whole modernization turns on.

Working set climbs 17 → 188 → 261 → 306 MB across successive rebuilds, so the
superseded graph is not released promptly either.

The graph is 327,792 `Letter` objects (each with ~20 counters and a back-pointer
to its `Word`), 77,878 `Word` objects with 29 fields, 6,236 `Verse` objects,
and eight parallel partition collections. `BuildSimplifiedBook` tears it all
down and rebuilds it on any option change; it is called from 40 sites.

## What the legacy authors already got right

The brief assumes two things need modernizing that do not:

- **§23 value systems → data-driven.** Already data: `Values/*.txt`, named
  `TextMode_LetterOrder_LetterValue`, parsed into `Dictionary<char, long>`.
  410 shipped, 274 loadable under word count method 77878.
- **§24 text modes → explicit.** Already data: `Rules/<method>/<mode>.txt`, an
  ordered find/replace list applied by a ten-line `Simplify()`.

The asset worth preserving is this **data**, byte-exact. The code that consumes
it is trivial. This is the main respect in which the brief's plan overestimates
the work.

## What is genuinely hard

`Server.cs`, 21,380 lines, no UI dependency, governing:

- 21 modifier flags on `NumericalSystem` (`AddToLetterLNumber`,
  `AddToWordVDistance`, `AbsolutePositions`, …)
- 4 sign-alternation flags
- a `CalculationMode` enum (letter values, digit sums, digital roots, and
  word-level variants)
- positional and distance-to-previous metadata per letter, word, verse, chapter

These interact combinatorially and are entangled with static mutable state.
This is the code a language rewrite would silently break, and the reason
ADR 0001 keeps C#.

## Architectural debt

| Issue | Evidence |
| --- | --- |
| Giant form | `MainForm.cs` 61,311 lines; `MainForm.Designer.cs` 728 controls |
| Dead source | `MainFormA87.cs` 56,623 lines, never compiled — **removed** |
| Copy-paste forks | 8 frozen `QuranNet` copies differing by ~120 lines — **removed** |
| Static god-objects | `Server` entirely static; `Client` a thin facade over it |
| No async | 0 uses of `async`/`await` in 243k lines; `Application.DoEvents()` instead |
| No tests | 0 |
| Fragile packaging | `Release.bat` moved the source tree then `RD /S /Q` — **replaced** |
| Duplicate GUIDs | `Controls.csproj` shared `Utilities.csproj`'s GUID — **fixed** |
| Escaping output | `Maths.csproj` wrote x86/x64 output outside the tree — **fixed** |
| Runtime litter | Engine writes `Rules/*.txt` beside the executable on load |

## Verified ground truth

Captured in `next/tests/golden/`. Self-validating: the extracted values match
the claims documented in `Model/NumericalSystem.cs`.

| | |
| --- | --- |
| Chapters / verses / words / letters | 114 / 6,236 / 77,878 / 327,792 |
| Unique letters | 29 |
| Al-Fatiha | 7 verses, 29 words, 139 letters, value **8317** |
| Whole book, `Original_Alphabet_Primes1` | 19,628,315 |

`Original` and `Simplified29` produce identical book values (19,628,315),
confirming that the Original text mode already normalizes onto the 29-letter
set.

## Consequences for the plan

1. Keep the algorithms in C#; replace the object graph. ADR 0001.
2. The startup target is governed by the 3,804 ms graph build, not by I/O.
3. Preserve `Values/` and `Rules/` byte-exact; treat them as canonical source
   with provenance records rather than as loose files.
4. Distribution reduction is mostly a data-packaging exercise: 259 MB of the
   615 MB install is translations, precomputed numbers and help assets that do
   not belong in a base install.
