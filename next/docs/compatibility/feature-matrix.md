# Feature matrix

> **This is an inventory of the original, not a plan.**
> [ADR 0004](../decisions/0004-code-19-scope.md) narrowed the project to Code 19
> and removed audio, drawing, the standalone tools except InitialLetters,
> multiple Arabic fonts, DNA symbols and the geometry calculators. Rows for
> those features are kept so the record of what the original did stays
> complete, but they are no longer work to be done. Read the statuses as
> "what exists", never as "what is left".

Every feature listed in `C#/QuranCode/Features.txt` (79 items), classified per brief §3.3 and
§47. This is the guard against silent feature loss: nothing may be dropped
without a row here saying so and why.

**Status** is the honest state of the new engine today, not a plan.

| Status | Meaning |
| --- | --- |
| **done** | Built and reachable from the app. Where the original defines a number or a result, it is checked against golden data captured from it; where the feature is presentation, it is checked by use |
| **engine** | Implemented in the engine; no screen yet |
| **todo** | Not started |
| **drop** | Deliberately not carried over, with a reason |
| **out** | Removed from scope by [ADR 0004](../decisions/0004-code-19-scope.md) |

There is no longer a **data** status: the items that waited on content packs
were imported in Phase 6.

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
| 17 | IndoPak font family support | port | out | ADR 0004: one Arabic font |
| 46 | Dynamic keyboard per text mode | rewrite | **done** | A palette beside the search box: the 28 base letters and only the extras the active text mode keeps (up to eight), after `UpdateKeyboard`; every extra for a root search |

## Numerology and values

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 2 | Values in all numerical systems at once | port | **done** | Values view: any text in every visible system |
| 47 | Add L, W, V, C and distances to total | port | **done** | All 21 modifiers, 29 golden cases |
| 48 | Add W, V, C and distances | port | **done** | Same |
| 49 | Add V, C and distances | port | **done** | Same |
| 50 | Add C | port | **done** | Same |
| 16 | Highlight values divisible by a user divisor | port | **done** | Divisor 2 to 9999, default 19, wraps as in the original; every number chip and the Maths sums are marked; powers get a dotted underline |
| 24 | Ratio-based colorization, golden ratio default | port | **done** | Reader colors the parts of each verse, chapter, partition or the book at 1/π, 1/e, 1/φ, 1/♥ or any ratio, by letters or value, at a letter, word, pause mark, verse or chapter end, with totals |
| 71 | Base 2–36 number systems | port | **done** | Numbers shown and typed in any base 2 to 36, digit sums in that base; the 20 Base letter-value systems value words by their digits (golden: base-systems.tsv) |
| 72 | User-defined SimplifiedXX books | port | engine | `text_modes` + `text_mode_rules` are data |
| 23 | DNA symbols (A T C G) in prime proportions | port | out | ADR 0004 |
| 30 | Expression calculator, bases 2–36 | **rewrite** | **done** | A parser in the Numbers view in place of the original's run-time C# compilation; exact whole numbers, any base, Arabic text valued in the current system |
| 78 | Circle/sphere/triangle calculators | port | out | ADR 0004 |

## Search

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| n/a | Arabic text search, 3 wordness modes | port | **done** | 10 queries, 9,582 verse entries verified |
| 1 | Ctrl+Click for same-root verses | port | **done** | Every display word linked to its roots (`verse_word_roots`); Ctrl+click and F4 search the longest root |
| 53 | Root search, multi-root, +/- include/exclude | port | **done** | Terms resolve to their best root as in the original; any/all; `+`/`-` added (the original never parsed them) |
| 52 | Search across all text modes | port | **done** | As in the original, an Arabic search that finds nothing tries the standard-spelling (Emlaaei) text: Tanzil's for the classic edition, the export's arabic_clean for Submission |
| 51 | Search in one or all translations | port | **done** | The edition's translations and transliteration, or only those shown; matches marked in the translation line |
| 73 | Any-language search | port | **done** | Follows from the language rule, as in the original |
| 67 | Auto-detect search language | port | **done** | The original's rule: all Quran letters, marks, Indian digits and symbols is Arabic, anything else searches translations (آ added) |
| 55 | `+`/`-` include/exclude in WORDS search | port | **done** | Not implemented in the original (labels hidden); implemented here for text and root search |
| 54 | Find by Frequency, all comparison operators | port | **done** | Letter frequency sums with every operator and number kind; all/any/only/no letter matches |
| 14 | LetterFrequencySum for a phrase per verse | port | **done** | Each found unit shows its sum; a sum of 1 or more lists every verse with its sum |
| 26 | Find sentences by letter frequency sum | port | **done** | Sentences split by pause marks as the original does |
| 25 | Find sentences by numbers | port | **done** | Whole verses first, then pause-mark sentences; Submission marks placed from the classic text |
| 32–35 | FindByNumbers (words/verses/chapters, ranges, sets, operators) | port | **done** | Words, verses, sentences, chapters and the 7 partitions; singly, in runs, in sets; every operator, Σ, #, and 18 number kinds |
| 31 | Emlaaei hamza+elf search improvement | port | **done** | The standard-spelling fallback (#52) covers it; the export's arabic_clean column is that text for the Submission edition |
| 15 | Match density shading per chapter | port | **done** | Chapter list shaded by matches on the original's 44-step ramp |
| 38 | F3 navigation through results | port | **done** | F3 and Shift+F3 through marks; through bookmarks in the reader |
| 39–44 | F4–F9 related/similar/same lookups | port | **done** | F4 related words, F5 related verses, F6 similar verses (4 methods, threshold), F7 same text, F8 same with marks, F9 same value. The all-pairs similar mode is not reproduced (worklog, Phase 4) |

## Statistics and research

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 3 | Full statistics of "Allah" and derivatives | port | **done** | Allah, words with الله, words with لله: 2,816 in the classic text, the figure Features.txt gives; 2,815 in Submission |
| 10 | Allah / non-Allah / repeated / all word info | port | **done** | The research lists (Allah, non-Allah, all, double, repeated words) paged, or copied as tabbed text |
| 21 | Word frequency list, multi-select | port | **done** | Stats view: by frequency or word, with marks in Original mode, chosen words totaled and searched together |
| 22 | Letter frequency list with prime factorization | port | **done** | Order, frequency, Σ position, Σ distance, sortable both ways; chosen letters totaled; any total opens in Numbers for factors |
| 12–13 | C/V classification odd/even, prime/composite | port | **done** | Maths sums for chapters and verses with d/u (checked against the original's help file: 7906/4885); chapter selection by the kind of number and verse count |
| 6–7 | 4n±1 prime and composite decompositions | port | **done** | Form, place among its kind, and every sum and difference of two squares and two cubes up to a million |
| 8 | Front-back symmetry | port | **done** | Letters per word, words or letters per verse, with or without the boundaries |
| 9 | Waleed's CPIndexChain | port | **done** | Chain, both bit readings each way, sum and length |
| 5 | Initialized vs non-initialized chapter selection | port | **done** | Imported per chapter (42 doubly, as the original forces); marked in the chapter list and selectable |
| 57 | Draw locations of "Allah" | port | out | ADR 0004: no drawing |
| 58 | Draw values as squares, golden ratios, spirals | port | out | ADR 0004: no drawing |

## Text-mode options

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 18 | Waw as a word | port | **done** | Counting menu; matches the original in every tested text mode (`golden/counting-options.tsv`) |
| 19 | Shadda as a double letter | port | **done** | Counting menu, as above; so are hamza, alif, yaa and noon above a line, which `Features.txt` does not number |
| 20 | BismAllah chapter prefixes | port | **done** | Submission edition: verse 0, counted or not (ADR 0003) |
| 4 | Updated root database | port | done | 2,053 roots imported with provenance |
| 62 | Word grammar, Arabic and English | port | **done** | Quranic Arabic Corpus parts per display word, tags named in English and Arabic from the original's language files, shown in the inspector |
| 64 | Word meaning and transliteration on hover | port | **done** | Word-by-word English and transliteration on click (the original shows them in its title bar on hover) |

## Audio

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 45 | Previous/next verse in the player | port | out | ADR 0004: no audio |
| 56 | Inter-verse silence, 0.0–2.0× | port | out | ADR 0004: no audio |
| 65 | Auto-download recitations from everyayah.com | port | out | ADR 0004: no audio. The reciter catalog and the Rust downloader are removed |
| 66 | Auto-download translations from tanzil.net | port | out | ADR 0004: the translation pack is removed |
| 27 | Show all selected translations at once | port | **done** | Under each verse. ADR 0004 narrows the choice to Khalifa's English, the transliteration and the Emlaaei text; the other 12 and the 108-translation Tanzil pack are removed |

## Standalone tools

Brief §17: these become modules in one shell rather than separate executables.

| # | Feature | Class | Status | Notes |
| ---: | --- | --- | --- | --- |
| 74 | QuranNet 3D word graph | port | out | ADR 0004 |
| 75 | QuranLab (114 verse-count properties) | port | out | ADR 0004 |
| 76 | InitialLetters sentence builder | port | **done** | Kept by ADR 0004 as the Initials view, not a tool: the 29 chapters, 14 letters and their counts. The original's sentence builder is not carried over; the counts are what Code 19 needs |
| 77 | Prime Calculator with Yafu | **rewrite** | out | ADR 0004. Number factoring is already in the Numbers view |
| 79 | Composites analysis | port | out | ADR 0004 |

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
| done | 63 |
| engine (no UI yet) | 1 (#72) |
| todo | 0 |
| out (ADR 0004) | 13 |
| drop | 1 (#36) |
| part of another row | 1 (#37, see #70) |
| **total** | **79** |

Counted per numbered feature, 79 in all; a row such as 32–35 counts as four.
Arabic text search, which `Features.txt` does not number, is also done. The
11 other standalone tools listed above are all todo and are not part of the 79.

Every numbered feature that ADR 0004 kept in scope is now built.

The 13 marked **out** are audio (45, 56, 65, 66), drawing (57, 58), the other
standalone tools (74, 75, 77, 79), the IndoPak fonts (17), DNA symbols (23)
and the geometry calculators (78), together with the 11 unnumbered legacy
programs.

Nothing is blocked on missing data any more. The three items that were
(#27 translations, #62 grammar, #64 word meanings) were imported in Phase 6,
and #31 is covered by the standard-spelling fallback built for #52.

The completed work is concentrated where correctness risk is highest: text
normalization, valuation, the 21 modifiers, search, and the number classes,
all checked against golden data captured from the original. What remains is
mostly presentation and self-contained calculators, where the legacy behavior
is easier to re-derive and less dangerous to get subtly wrong.

Two items are classed **rewrite** rather than port, both per brief §26:

- **#30, the expression calculator.** The legacy compiles C# at runtime to
  evaluate ordinary arithmetic. That is a security and startup cost for
  something a parser does better.
- **#77, Prime Calculator.** The factoring capability is worth keeping. The
  third-party YAFU, GGNFS and GMP-ECM binaries it launched were removed from
  the repository because they came without license terms. The engine factors
  up to 10^14 on its own; larger numbers need a replacement.
