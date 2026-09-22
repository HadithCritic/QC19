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

| Operation | Legacy | New | Change |
| --- | ---: | ---: | ---: |
| Value whole book | 34 ms | 22 ms | −35% |
| **Value all 6,236 verses individually** | **57 ms** | **81 ms** | **+42%** |
| Switch value system | 1,811 ms | 48 ms | **−97%** |

### The regression, stated plainly

Valuing verses one at a time is **42% slower** than the legacy engine.

The cause is known and is a consequence of the design, not a bug: the legacy
engine pre-normalized every verse while building the book, so per-verse
valuation was pure arithmetic over a ready object. The new engine normalizes on
demand, so this loop pays for 6,236 separate normalizations.

It is a fair trade at this stage — 24 ms lost on a bulk operation against
3,672 ms and 134 MB saved at startup — but it is a real regression and it is not
being reported as a win. The fix is to normalize once and index verse offsets
into the result, which is what `segmentations` in the schema exists for. Not yet
implemented.

Switching value systems is 48 ms against 1,811 ms because it no longer implies
rebuilding anything.

## Distribution

Measured on a published build (`win-x64`, framework-dependent, content
included), confirmed to launch.

| | Legacy | New |
| --- | ---: | ---: |
| **Total install** | **615 MB** | **35 MB** |
| Canonical content | ~21 MB loose files (`Data/`, `Values/`, `Rules/`) | 3.1 MB `content.db` |
| Precomputed number tables | 63 MB | 0 — computed by a sieve |
| Offline translations | 156 MB | 0 — optional packs |
| Help PDFs and images | 40 MB | 0 — fetched on demand |
| Executables | 17 | 1 |
| Full-text index | none | included |
| Files in install root | 4,388 | 43 |

**-94%.**

Two caveats, both real:

- The 35 MB excludes the .NET 9 runtime, which the legacy install also
  excluded (it needed .NET Framework 4.0). A self-contained build would add
  roughly 70 MB.
- 105 MB of that was third-party native `.pdb` files that SkiaSharp and
  HarfBuzzSharp ship inside their NuGet packages and the SDK copies on
  publish. They are debug symbols for native code this project does not
  debug, and a `RemoveNativeSymbols` target now drops them. Without that
  target the figure is 140 MB, which is still -77%, but shipping another
  project's debug symbols is not a reduction anyone should have to accept.

## Honest summary

Measured and real: startup −93%, heap −98%, system switching −97%, content
3.1 MB with an FTS index included.

Measured and adverse: per-verse valuation +42%.

Not yet measured, because not yet built: search latency, large-result rendering,
cold start of a packaged application, and anything involving the UI.
