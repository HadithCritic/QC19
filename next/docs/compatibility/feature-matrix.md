# Feature matrix

Every feature listed in `C#/QuranCode/Features.txt` (79 items), classified per brief §3.3 and
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
| 11 | Verses before/after, in chapter and book | port | **done** | `selection.stats` position; Inspector shows it; follows the Bismillah option |
| 28 | Show Original text, use Simplified29 counts | port | done | This is what `Original` mode already does; see text-normalization spec |
| 29 | Chapter info on hover | port | **done** | Chapter list card on hover or focus: verses, words, letters, value |
| 59 | Sort chapters by number/name/revelation/counts/value | port | **done** | Chapter list sort menu, both directions; counts from `chapters.stats` |
| 60 | Direct chapter/verse entry (`5:55`, `6:19-23`, …) | port | **done** | Chapters, verses, ranges and verse 0 (`2`, `2:255`, `2:255-257`, `1:7-2:2`, `2:0`), Arabic-Indic digits |
| 61 | Direct page/station/part/… entry | port | **done** | `page 1`, `part 30`, `verse 262`, `word 40`, `letter 139`, chapter ranges `3-4` |
| 63 | Distances on text clicks | port | **done** | Alt+click a second word: chapters, verses, words, letters between (`words.distance`) |
| 68 | Back/forward through browse and find history | port | **done** | Back/forward buttons and Alt+Left/Right; searches rerun from history |
| 69 | Find and browse history | port | **done** | `user.db`, 500 per kind, repeats skipped; Saved view and recent searches |
| 70 | Bookmarks with notes, auto-save | port | **done** | Inspector bookmark with an auto-saved note; Saved view lists, opens, deletes |
| 17 | IndoPak font family support | port | todo | UI font selection |
| 46 | Dynamic keyboard per text mode | rewrite | todo | UI |

## Numerology and values

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 2 | Values in all numerical systems at once | port | **done** | Values view: any text in every visible system |
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
| n/a | Arabic text search, 3 wordness modes | port | **done** | 10 queries, 9,582 verse entries verified |
| 1 | Ctrl+Click for same-root verses | port | data | 2,053 roots imported, but `word_roots` is empty: roots are not yet linked to words |
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
| 31 | Emlaaei hamza+elf search improvement | port | data | Needs an emlaaei (standard spelling) text of the Submission edition; none exists (worklog, Phase 1) |
| 15 | Match density shading per chapter | port | todo | UI |
| 38 | F3 navigation through results | port | todo | UI |
| 39–44 | F4–F9 related/similar/same lookups | port | todo | Similarity engine not started |

## Statistics and research

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 3 | Full statistics of "Allah" and derivatives | port | engine | Search + counts |
| 10 | Allah / non-Allah / repeated / all word info | port | engine | Same |
| 21 | Word frequency list, multi-select | port | engine | Frequencies derivable |
| 22 | Letter frequency list with prime factorization | port | engine | Frequencies shown for any selection; factorization of the counts todo |
| 12–13 | C/V classification odd/even, prime/composite | port | engine | Number classes (U, AP, XP, AC, XC) and ordinals done and tested; chapter and verse classification lists todo |
| 6–7 | 4n±1 prime and composite decompositions | port | todo | Same |
| 8 | Front-back symmetry | port | todo | |
| 9 | Waleed's CPIndexChain | port | todo | |
| 5 | Initialized vs non-initialized chapter selection | port | todo | Initial-letter metadata is in `quran-metadata.txt` |
| 57 | Draw locations of "Allah" | port | todo | UI |
| 58 | Draw values as squares, golden ratios, spirals | port | todo | UI |

## Text-mode options

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 18 | Waw as a word | port | **done** | Counting menu; matches the original in every tested text mode (`golden/counting-options.tsv`) |
| 19 | Shadda as a double letter | port | **done** | Counting menu, as above; so are hamza, alif, yaa and noon above a line, which `Features.txt` does not number |
| 20 | BismAllah chapter prefixes | port | **done** | Submission edition: verse 0, counted or not (ADR 0003) |
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
| 77 | Prime Calculator with Yafu | **rewrite** | todo | Keep factoring; the bundled YAFU binaries were removed (no license terms) |
| 79 | Composites analysis | port | todo | Merge as a module |

## Other standalone tools

These legacy programs are not in `Features.txt`, so they are listed here to
keep the inventory complete. Their source is in `C#/<name>/`; what each one
computes has not been reviewed yet, so the notes give only the window title.

| Tool | Window title | Class | Status |
| --- | --- | --- | --- |
| AhlulBayt | Ahlul-Bayt | port | todo |
| DayOfWeek | Day of Week | port | todo |
| Deficients | (none set) | port | todo |
| Dimensions | Dimensions | port | todo |
| Divisibility | Divisibility Rules | port | todo |
| Indices | Indices | port | todo |
| Numbers | Numbers | port | todo |
| Primes | (none set) | port | todo |
| WordDecoder | Word Decoder | port | todo |
| WordFinder | Word Finder | port | todo |
| WordGenerator | WordGenerator | port | todo |

## Deliberate removals

| # | Feature | Reason |
| ---: | --- | --- |
| 36 | F1 = Help opening bundled PDFs | The 40 MB `Help/` tree does not belong in a base install (brief §27). Help becomes fetched or hosted. The capability stays; the bundling does not. |
| 37 | F2 = Bookmarks | Not removed: the keybinding is a UI detail, the feature is row 70. |

Only one genuine removal, and it removes a packaging decision rather than a
capability.

---

## Summary

| Status | Count |
| --- | ---: |
| done | 11 |
| engine (no UI yet) | 12 |
| todo | 47 |
| data (needs a pack) | 7 |
| drop | 1 (#36) |
| part of another row | 1 (#37, see #70) |

Counted per numbered feature, 79 in all; a row such as 32–35 counts as four.
Arabic text search, which `Features.txt` does not number, is also done. The
11 standalone tools above are all todo.

The completed items are concentrated where correctness risk is highest: text
normalization, valuation, the 21 modifiers, and text search. The outstanding
work is mostly UI and number-theory modules, where the legacy behavior is
easier to re-derive and less dangerous to get subtly wrong.

Two items are classed **rewrite** rather than port, both per brief §26:

- **#30, the expression calculator.** The legacy compiles C# at runtime to
  evaluate ordinary arithmetic. That is a security and startup cost for
  something a parser does better.
- **#77, Prime Calculator.** The factoring capability is worth keeping. The
  third-party YAFU, GGNFS and GMP-ECM binaries it launched were removed from
  the repository because they came without license terms. The engine factors
  up to 10^14 on its own; larger numbers need a replacement.
