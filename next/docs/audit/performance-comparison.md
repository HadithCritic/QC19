# Performance: legacy vs new engine

Same machine, same steps, engine only, no UI on either side. Legacy figures from
`next/tests/golden/performance-baseline.tsv` (`OracleDump --bench`); new figures
from `next/tools/QuranCode.Bench`.

Both engines were checked to produce the same numbers before being compared:
whole-book value **19,628,315** and Abjad/Gematria **23,466,310** on both sides.
A speed comparison between engines that disagreed would be meaningless.

## Startup

The legacy engine is not usable until the book is built.

| Step | Legacy | New |
| --- | ---: | ---: |
| Open / construct | 134 ms | 206 ms |
| Load corpus | — | 44 ms |
| Load text mode | — | 6 ms |
| Load value system | — | 10 ms |
| **Build book / materialize** | **3,804 ms** | **0 ms** |
| **Total to usable** | **3,938 ms** | **266 ms** |

**−93%**, from 3.9 s to 0.27 s.

The new engine never builds the graph. It holds 114 chapter and 6,236 verse
records and derives words and letters from text on demand, so the 3,804 ms is
not optimized, it is removed.

`open_database` is slower than the legacy constructor (206 ms vs 134 ms) because
it includes first-use SQLite initialization. It buys the 3,804 ms.

## Memory

| Measure | Legacy | New | Change |
| --- | ---: | ---: | ---: |
| Managed heap after startup | 136.4 MB | 2.2 MB | **−98%** |
| Working set after startup | 188.3 MB | 35.2 MB | **−81%** |
| Working set after full valuation | 261.0 MB | 58.5 MB | −78% |
| Working set after switching system | 306.6 MB | 57.6 MB | −81% |

The legacy figure climbs across rebuilds (188 → 261 → 306 MB) because the
superseded graph is not released promptly. The new engine stays flat.

## Operations

Remeasured 2026-09-22 through the engine calls the app makes
(`next/tools/QuranCode.Bench`, classic text so both sides value the same
words; best of five warm runs, cold run shown where it matters). Legacy figures
are from the committed `performance-baseline.tsv`.

| Operation | Legacy | New |
| --- | ---: | ---: |
| Value all 6,236 verses one at a time | 59 ms | 31 ms |
| Value whole book | 34 ms | 16 ms |
| Statistics for the whole book | n/a | 7 ms (20 ms cold) |
| Switch value system, first use | 1,371 ms | 136 ms |
| Classify the whole-book value, with its position | n/a | 1 ms (172 ms once, now built in the background at start) |
| Value a verse in all 276 visible systems | n/a | 63 ms |
| Search "الله" anywhere | n/a | 4 ms (13 ms cold) |

### The earlier per-verse regression

This document used to report per-verse valuation as 42% slower than the
original. That figure came from a benchmark that normalized every verse's text
again on every call, which is not what the app does: the app normalizes the
text once into a segmentation and values verses from it. Measured the way the
app actually works, the same loop is **31 ms against the original's 59 ms**.

### Fixed in Phase 2

- **Number positions.** Finding a large number's position among additive or
  non-additive primes or composites walked every number up to it on every
  request: about 230 ms for the whole-book value, paid by every whole-book
  statistic. A per-block count index (`NumberTheory.CountSubclass`) now does
  that walk once per block for the life of the process; later lookups take
  about 1 ms. The engine builds the index up to 30 million on a background
  thread at start.
- **Values of a text in every system** normalized the text twice per system.
  The protocol handler now normalizes once per text mode (8 modes, 276
  systems).
- **Startup.** The app starts the engine as soon as it launches, so opening
  the database overlaps the window loading.

Measured through the real sidecar process, the requests the first screen
makes all return within 43 ms, the slowest being the first chapter's values,
which builds the default segmentation.

## Distribution

Measured on the Tauri release build (`pnpm release`), installed silently into
a scratch folder, launched, and uninstalled.

| | Legacy | New |
| --- | ---: | ---: |
| **Installer** | n/a (615 MB loose install) | **5.2 MB** (NSIS) |
| **Installed** | **615 MB** | **about 13 MB** |
| App | 17 executables, 13 DLLs | `qurancode-desktop.exe` 4.1 MB |
| Engine | in the app | `qurancode-engine.exe` 4.0 MB, Native AOT |
| SQLite | none | `e_sqlite3.dll` 2 MB |
| Canonical content | about 21 MB of loose files | `content.db` 3 MB |
| Precomputed number tables | 63 MB | none; computed by a sieve |
| Offline translations and help | 196 MB | none in the base install |
| Runtime needed | .NET Framework 4.0 | none; WebView2 ships with Windows 11 |

## Honest summary

Measured and favorable:

| | |
| --- | ---: |
| Startup to usable | −93% |
| Managed heap after startup | −98% |
| Working set after startup | −81% |
| Switching value system | −90% |
| Value whole book | −53% |
| Value every verse one at a time | −47% |
| Install size | −98% |

Measured and adverse: nothing currently. The earlier per-verse figure was a
benchmark artifact (see Operations).

Not measured yet: rendering very large search results, and cold start of an
installed application on a clean machine.
