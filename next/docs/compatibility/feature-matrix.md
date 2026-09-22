# Feature matrix

Every feature listed in `Features.txt` (79 items), classified per brief §3.3 and
§47. This is the guard against silent feature loss: nothing may be dropped
without a row here saying so and why.

**Status** is the honest state of the new engine today, not a plan.

| Status | Meaning |
| --- | --- |
| **done** | Implemented and covered by a golden test against the legacy engine |
| **engine** | Implemented in the engine; no UI yet |
| **todo** | Not started |
| **data** | Needs a content pack that is not yet imported |
| **drop** | Deliberately not carried over, with a reason |

**Class** is the port/rewrite/remove decision.

---

## Core text and navigation

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 11 | Verses before/after, in chapter and book | port | engine | Derivable from `Segmentation` |
| 28 | Show Original text, use Simplified29 counts | port | done | This is what `Original` mode already does; see text-normalization spec |
| 29 | Chapter info on hover | port | todo | UI |
| 59 | Sort chapters by number/name/revelation/counts/value | port | engine | All fields present in `chapters` |
| 60 | Direct chapter/verse entry (`5:55`, `6:19-23`, …) | port | engine | `TryParseReference` handles `c:v`; ranges todo |
| 61 | Direct page/station/part/… entry | port | engine | `partitions` table holds all 7 kinds |
| 63 | Distances on text clicks | port | engine | `Segmentation` distance arrays |
| 68 | Back/forward through browse and find history | port | todo | Belongs in `user.db` |
| 69 | Find and browse history | port | todo | `user.db` |
| 70 | Bookmarks with notes, auto-save | port | todo | `user.db` |
| 17 | IndoPak font family support | port | todo | UI font selection |
| 46 | Dynamic keyboard per text mode | rewrite | todo | UI |

## Numerology and values

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 2 | Values in all numerical systems at once | port | engine | 407 systems loaded; loop over `ValueSystems()` |
| 47 | Add L, W, V, C and distances to total | port | **done** | All 21 modifiers, 29 golden cases |
| 48 | Add W, V, C and distances | port | **done** | Same |
| 49 | Add V, C and distances | port | **done** | Same |
| 50 | Add C | port | **done** | Same |
| 16 | Highlight values divisible by a user divisor | port | todo | Presentation over existing values |
| 24 | Ratio-based colorization, golden ratio default | port | todo | UI |
| 71 | Base 2–36 number systems | port | todo | Legacy `Radix`; systems named `Base*` |
| 72 | User-defined SimplifiedXX books | port | engine | `text_modes` + `text_mode_rules` are data |
| 23 | DNA symbols (A T C G) in prime proportions | port | todo | Legacy `DNASequenceSystem` |
| 30 | Expression calculator, bases 2–36 | **rewrite** | todo | Brief §26: legacy uses runtime code compilation for ordinary arithmetic; replace with a parser |
| 78 | Circle/sphere/triangle calculators | port | todo | Standalone `Maths` |

## Search

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| — | Arabic text search, 3 wordness modes | port | **done** | 10 queries, 9,582 verse entries verified |
| 1 | Ctrl+Click for same-root verses | port | engine | `roots` and `word_roots` imported (2,053 roots) |
| 53 | Root search, multi-root, +/- include/exclude | port | todo | Data present, query layer todo |
| 52 | Search across all text modes | port | engine | Engine caches a search per mode |
| 51 | Search in one or all translations | port | data | Needs translation packs |
| 73 | Any-language search | port | data | Needs translation packs |
| 67 | Auto-detect search language | port | todo | |
| 55 | `+`/`-` include/exclude in WORDS search | port | todo | |
| 54 | Find by Frequency, all comparison operators | port | todo | |
| 14 | LetterFrequencySum for a phrase per verse | port | todo | |
| 26 | Find sentences by letter frequency sum | port | todo | |
| 25 | Find sentences by numbers | port | todo | |
| 32–35 | FindByNumbers (words/verses/chapters, ranges, sets, operators) | port | todo | The largest remaining search surface |
| 31 | Emlaaei hamza+elf search improvement | port | data | Needs the emlaaei text variant |
| 15 | Match density shading per chapter | port | todo | UI |
| 38 | F3 navigation through results | port | todo | UI |
| 39–44 | F4–F9 related/similar/same lookups | port | todo | Similarity engine not started |

## Statistics and research

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 3 | Full statistics of "Allah" and derivatives | port | engine | Search + counts |
| 10 | Allah / non-Allah / repeated / all word info | port | engine | Same |
| 21 | Word frequency list, multi-select | port | engine | Frequencies derivable |
| 22 | Letter frequency list with prime factorization | port | engine | Frequencies done; factorization todo |
| 12–13 | C/V classification odd/even, prime/composite | port | todo | Needs the number-theory module |
| 6–7 | 4n±1 prime and composite decompositions | port | todo | Same |
| 8 | Front-back symmetry | port | todo | |
| 9 | Waleed's CPIndexChain | port | todo | |
| 5 | Initialized vs non-initialized chapter selection | port | todo | Initial-letter metadata is in `quran-metadata.txt` |
| 57 | Draw locations of "Allah" | port | todo | UI |
| 58 | Draw values as squares, golden ratios, spirals | port | todo | UI |

## Text-mode options

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 18 | Waw as a word | port | engine | Modeled in `segmentations`; not yet applied |
| 19 | Shadda as a double letter | port | engine | Same |
| 20 | BismAllah chapter prefixes | port | engine | Same |
| 4 | Updated root database | port | done | 2,053 roots imported with provenance |
| 62 | Word grammar, Arabic and English | port | data | `word_grammar` table exists; corpus.quran.com data not imported |
| 64 | Word meaning and transliteration on hover | port | data | Same |

## Audio

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 45 | Previous/next verse in the player | port | todo | |
| 56 | Inter-verse silence, 0.0–2.0× | port | todo | |
| 65 | Auto-download recitations from everyayah.com | port | todo | Optional pack, brief §27 |
| 66 | Auto-download translations from tanzil.net | port | todo | Optional pack |
| 27 | Show all selected translations at once | port | data | |

## Standalone tools

Brief §17: these become modules in one shell rather than separate executables.

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 74 | QuranNet 3D word graph | port | todo | Merge as a module |
| 75 | QuranLab (114 verse-count properties) | port | todo | Merge as a module |
| 76 | InitialLetters sentence builder | port | todo | Merge as a module |
| 77 | Prime Calculator with Yafu | **rewrite** | todo | Keep factoring; drop the bundled Yafu binary unless licensing is cleared |
| 79 | Composites analysis | port | todo | Merge as a module |

## Deliberate removals

| # | Feature | Reason |
| ---: | --- | --- |
| 36 | F1 = Help opening bundled PDFs | The 40 MB `Help/` tree does not belong in a base install (brief §27). Help becomes fetched or hosted. The capability stays; the bundling does not. |
| 37 | F2 = Bookmarks | Not removed — the keybinding is a UI detail, the feature is row 70. |

Only one genuine removal, and it removes a packaging decision rather than a
capability.

---

## Summary

| Status | Count |
| --- | ---: |
| done (golden-tested) | 7 |
| engine (no UI) | 18 |
| todo | 44 |
| data (needs a pack) | 9 |
| drop | 1 |

The completed items are concentrated where correctness risk is highest: text
normalization, valuation, the 21 modifiers, and text search. The outstanding
work is mostly UI and number-theory modules, where the legacy behavior is
easier to re-derive and less dangerous to get subtly wrong.

Two items are classed **rewrite** rather than port, both per brief §26:

- **#30, the expression calculator.** The legacy compiles C# at runtime to
  evaluate ordinary arithmetic. That is a security and startup cost for
  something a parser does better.
- **#77, Prime Calculator.** The factoring capability is worth keeping; the
  bundled third-party Yafu binary needs a licensing check before it is
  redistributed (brief §29).
