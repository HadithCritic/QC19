# Work log: Phases 0 to 12

> **Superseded by [ADR 0004](decisions/0004-code-19-scope.md).** These phases
> assumed the goal was feature parity with the original. The project is now
> scoped to Code 19, and the remaining phases in that plan are not being
> carried out. This log stays as the record of how phases 0 to 6 were built
> and what was decided along the way; several of those decisions still bind.

Running record of the autonomous build: what was completed, each judgment
call and why, each gated item and how it was handled, and any unresolved
failures. Newest entries go at the bottom of each phase.

Standing decisions:

- **No publishing.** Work is committed locally on `main` and not pushed. Pushing
  to the public GitHub repository counts as publishing; the owner pushes after
  review. Nothing is released, tagged for release, or uploaded.
- **Golden data is never edited** to make a test pass.
- **Parity first.** Where the original C# app defines behavior, it is matched
  and checked against golden data captured from it.

---

## Phase 0: Confirm the foundation

### 0.5 License of the Submission export (gated item)

Researched rather than assumed. Findings, recorded in
`next/data/sources/submission/SOURCE.md`:

- The same tables are published by WikiSubmission in the public repository
  `WikiSubmission/ws-backend` (`assets/source_data/*.sql`), licensed AGPL-3.0,
  which is compatible with this project's GPL-3.0.
- wikisubmission.org says "Everything here is open to read, cite, and reuse".
- Its Terms of Use are generic and restrict derivatives "unless we expressly
  state otherwise"; nothing addresses the Quran data or the translations.
- Translation copyright (Rashad Khalifa and others) is not addressed. The app
  does not use the translations yet.

**Action taken:** none that publishes. Work continues on the basis that the
data is AGPL-3.0 via ws-backend. **Owner to confirm** with WikiSubmission
before any public release (the owner's address is at wikisubmission.org).

### 0.1 Native window

Built the release app (`pnpm release`) and ran `qurancode-desktop.exe`: the
window opens titled QuranCode, starts one `qurancode-engine.exe` child, and
shows Al-Fatiha with value-colored verse markers (screenshot taken). No
difference from the browser build was seen.

### 0.2 Windows installer

`QuranCode_0.1.0_x64-setup.exe` (5.2 MB, NSIS). Tested with a silent install
into a scratch folder: it installs the app, engine, `content.db` and
`e_sqlite3.dll` side by side; the installed app starts its engine; the silent
uninstaller removes it. **Decision:** test on this machine in a scratch folder
and uninstall, since no clean machine is available. Not published.

### 0.3 Continuous integration

Added `.github/workflows/ci.yml`: an engine job (build both databases, engine
and protocol tests) and a desktop job (stage the engine, type check, interface
tests, rustfmt, clippy with warnings as errors, Rust tests). Validated as
YAML locally. **Not run on GitHub:** that needs a push, which is held for the
owner's review.

### 0.4 Golden data from the original

**Decision:** instead of installing the .NET Framework 4.0 developer pack
system-wide, build with Microsoft's reference-assembly package from NuGet
(`Microsoft.NETFramework.ReferenceAssemblies.net40`) and the installed Visual
Studio Build Tools MSBuild. Reversible, nothing installed.

The original's whole solution builds with 0 errors into `C#/Build/Release`
(ignored by git). OracleDump built against it regenerates all 15 golden files
**identically** to the committed baseline. Scripted as `next/tools/oracle.py`
(`--check` to compare, `--out` to capture new cases).

### Phase 0 checks

All passing: engine 150, protocol 33, interface 11, Rust 7, rustfmt, clippy
(0 warnings), oracle reproduces the golden data. **Phase 0 complete.**

---

## Phase 1: Settle how text is counted

### 1.1 Mark-order fix (7:58, 10:101, 15:92)

The Submission file stores three hamzas on a tatweel as tatweel, vowel, hamza
instead of tatweel, hamza, vowel. The text-mode rules match only the second
order, so these hamzas were dropped while the same words elsewhere kept theirs.
`CountingText.CanonicalizeMarks` reorders them before counting; the stored text
is not edited. **Behavior change, logged:** Submission letter totals rise by 2
(327,662 to 327,664 with Bismillahs counted), 7:58 and 10:101 now count as in
the classic text and leave the edition comparison (30 to 28 verses). The
classic text has no such case, so no golden figure moves.

### 1.2 Counting options

Captured new golden data from the original, `tests/golden/counting-options.tsv`
(a new file; no baseline was changed): 10 option sets (default, no Bismillah,
each option alone, waw with shadda, all) across Simplified29, 31, 36 and 28,
per chapter and for the whole book. Implemented `CountingOptions` and
`CountingText` in the engine, following `Server.BuildSimplifiedBook`'s order.
**All 40 combinations match the original exactly.**

Decisions:

- **Availability per text mode follows the original's enabled check boxes**
  (priority 1): no text options in Original; no hamza in Simplified28 or 30.
  The engine drops a disallowed option rather than applying it.
- **Leaving out the Bismillah in an edition with verse 0 works in every text
  mode,** including Original, where the classic original forces it on. This is
  the owner's explicit requirement for verse 0 and takes precedence.
- **One `counting` object on the protocol** replaces the single
  `includeBasmalas` flag.
- **Interface:** a Counting menu in the top bar replaces the Count Bismillah
  switch, listing all seven options with the original's marks; disallowed ones
  are disabled with the reason. The simplest conventional control for seven
  related check boxes.

Bug found and fixed on the way: Arabic literals typed into source files were
being put into Unicode canonical order (fatha before shadda), which the text
does not use, so the Bismillah prefix never matched. The two constants are now
generated from the text itself; no other literal in the source has stacked
marks.

### 1.3 Emlaaei text (#31)

**Not implemented, by decision.** The original loads a separate emlaaei
(standard spelling) text of the classic edition. No such text exists for the
Submission edition, and mixing the classic one in would contradict its verse
structure (9:128-129, verse 0, 68:1). Left as a data item in the feature matrix.

### Phase 1 checks

All passing: engine 198, protocol 35, interface 15, Rust 7, rustfmt, clippy,
and the oracle reproduces every golden file including the new
`counting-options.tsv`. **Phase 1 complete.**

---

## Phase 2: Performance

### 2.1 Measurements

Rewrote `tools/QuranCode.Bench` to time the calls the app makes, cold and
warm. Findings:

- **The recorded "per-verse valuation is 42% slower" was a benchmark artifact.**
  The old benchmark re-normalized every verse on every call; the app values
  from a segmentation built once. Measured correctly: 31 ms against the
  original's 59 ms.
- **Real hotspot:** a large number's position in its class walked every number
  up to it, about 230 ms per whole-book statistic.
- The Values view normalized the text twice per system (276 systems).
- First-screen requests wait on engine start and the first segmentation.

### 2.2 Fixes

- Per-block count index for additive and non-additive primes and composites,
  with a running digit sum: position lookups 230 ms to about 1 ms after a
  one-time 158 ms build. Checked against brute-force counts at block
  boundaries.
- Values handler normalizes once per text mode: 258 ms to 34 ms.
- The Rust bridge starts the engine at app launch; the engine builds the
  number index to 30 million on a background thread (NumberTheory is
  thread-safe; the engine's caches are not, so nothing else is warmed off the
  request loop). **Decision:** an earlier version warmed synchronously and
  delayed the first answer from about 410 ms to about 980 ms after process
  start, so it was replaced.
- Measured through the sidecar: every first-screen request returns within
  43 ms; whole-book statistics 159 ms to 9 ms.

`docs/audit/performance-comparison.md` updated, including the distribution
figures of the Tauri build (5.2 MB installer, 13.2 MB installed).

### Phase 2 checks

All passing: engine 203, protocol 35, interface 15, Rust 7, rustfmt, clippy,
oracle reproduces. **Phase 2 complete.**

---

## Phase 3: Navigation and your own data

### 3.1 What was built

- Units in the reference box (#61): page, station, part, group, half,
  quarter, bowing, verse, word and letter, plus chapter ranges such as `3-4`.
  Word and letter numbers follow the counting options.
- Verses before and after the selection, in the chapter and the book (#11).
- Chapter list sort by every field, both directions (#59), and a hover or
  focus card with the chapter's counts and value (#29).
- Alt+click a second word to measure the distance to it (#63).
- A separate `user.db` (never the content database) holds bookmarks with
  notes (#70) and browse and find history (#69). Positions are stored as
  chapter:verse, so they survive a change of edition; a stored verse missing
  from the open edition shows but cannot be opened.
- Back and forward through the selections (#68); the Saved view lists
  bookmarks and both histories; the search view shows recent searches.

### 3.2 Decisions

- **Decision:** history entries are written only after a selection stays for
  about a second, so arrow-key stepping does not flood the list. The original
  added every change; this is a usability choice and does not change counts.
- **Decision:** searching with "shadda as a letter" (or "waw as a word")
  searches the changed text, as the original does (`Server.BuildSimplifiedBook`
  rebuilds the book with the options before any search). So `الرحمن` finds
  one verse with shadda counted, and `الررحمن` finds the rest. Kept for
  parity; the search view now says so when either option is on.
- Engine methods for bookmarks and history answer `unavailable` when the app
  starts without a user file, so the reader still works.

### Phase 3 checks

All passing: engine 210, protocol 43, interface 22, Rust 7, rustfmt, clippy,
svelte-check 0 errors, oracle reproduces. Checked in the browser: bookmark
with a note, recent searches, Saved view lists and reopens entries.
**Phase 3 complete.**

---

## Phase 4: Search

A read of the original's search code (Server.cs, Client.cs, MainForm.cs)
came first, since several Features.txt entries describe things the code does
differently. Findings that shaped this phase:

- `+` and `-` (#55) are not implemented in the original: the labels exist but
  are hidden and the server never parses them.
- "Search across all text modes" (#52) is really a fallback to the Emlaaei
  (standard spelling) text when a search finds nothing.
- F4 to F9 act on the word at the caret, and F3 steps through the marks of the
  current result (or through bookmarks when there is none).
- Similar verses (F6) compare one verse with the rest by text, words, word
  roots or word values, at 70% unless changed.

### 4.1 Text, roots, related and similar verses

- Text search takes several terms: any of them, all of them, or the exact
  phrase (the original's WORDS and Exact searches), within the whole book, the
  selected verses or the current results.
- **Decision:** `+word` (must contain) and `-word` (must not contain) are
  implemented, since Features.txt lists them; in a phrase they are literal.
- **Decision:** a word that matches two terms is marked and counted once. The
  original adds it twice; the verse list is the same either way.
- **Fix (changes behavior):** search terms now go through the same word
  normalization as the text, so a term typed with its marks follows the
  counting options (a typed shadda doubles its letter when shadda counts as a
  letter). Before, terms were always Simplify29, and typed marks never
  matched under those options. Golden search results are unchanged.
- Roots: the legacy word-roots file was imported but never linked to words.
  The new `verse_word_roots` table (schema 5) links every display word to its
  roots in both editions. The legacy file counts the Bismillah as the first
  four words of verse 1 and joins "بعد ما" in 2:181, 8:6 and 13:37; the
  Submission edition also writes 15:7's "لو ما" as one word. The importer
  aligns these and fails the build if any verse cannot be aligned. All 77,851
  display words have roots.
- Root search resolves each term as the original's `GetBestRoot` does (exact,
  then each simplification, then the root of a word spelled that way, then the
  closest root containing the term), with any/all grouping and `+`/`-`.
- Ctrl+click and F4: words sharing the clicked word's longest root. F5:
  verses where every word pairs with its own word sharing a root. F6: similar
  verses by text, words, roots or values, threshold adjustable. F7: the same
  word (or the verse as a phrase). F8: the same text with its marks.
- **Decision:** the original's all-pairs similar-verses mode (every verse
  against every other, grouped) is not reproduced. It is quadratic, and in the
  original its "same roots" method compares word texts and its "similar
  words" method compares against a character count, so it cannot be matched
  faithfully without copying bugs. The one-verse form, which F6 uses, is
  complete.
- **Fix:** exact comparison with marks needed one mark order. Typed text puts
  fatha before shadda; the source often does not. The engine runs with
  invariant globalization, where `string.Normalize` does not reorder, so
  `MarkOrder` reorders Arabic marks by their combining classes. A test checks
  it against .NET's own normalization on every word.
- Chapter list shading (#15): while results exist, each chapter is shaded by
  its matches. **Decision:** the original fades a color in 16-step jumps and
  reaches black at 44 matches; here the same ramp sets the strength of the
  theme's gold, so it works in light and dark themes.
- F3 and Shift+F3 step through the marks of the loaded results with a
  counter; in the reader they step through bookmarks, as in the original.
- **Decision:** #52 needs an Emlaaei text. The classic edition's Emlaaei text
  is a translation file, which arrives with translations in Phase 6; the
  Submission edition has none (see Phase 1).
- Timing through the sidecar: similar verses for 2:282 (the longest) at 30%
  takes 16 to 83 ms by method; related verses 23 ms; root search 7 ms.

### 4.2 Numbers, sentences and letter frequency

- Find by numbers: words, verses, sentences, chapters and the seven
  partitions (pages, stations, parts, groups, halves, quarters, bowings),
  singly, in runs of neighbors, or in sets. Each constraint (number, verses,
  words, letters, distinct letters, value, and for single words and verses
  how often their text occurs and which occurrence they are) takes =, not =,
  <, at most, >, at least, divisible with a remainder (-1 for any), not
  divisible, Σ (sum of positions), # (the unit's own number) or one of 18
  number kinds, as the original's `NumberQuery` does. A negative number counts
  from the end. Tested against direct counts over the segmentation.
- Runs without a size try every size from 1 to 29 (one less than the number
  of blocks for chapters and partitions), sorted by size then start, as in
  the original.
- **Decision:** sets need a size, and a set search that would test more than
  5,000,000 combinations is refused with a message to narrow the scope. The
  original has no limit and would run for hours. Results stop at 100,000
  units, and the result says so.
- **Decision:** a run of size 1 lists each unit on its own. The original
  returns one "run" holding every match at size 1, which reads as a bug.
- **Decision:** a chapter's number is its number, not its place in the
  current chapter sort (the original reads the sorted position).
- Sentences follow `DoFindSentences` step for step: each word takes the
  pause mark after it, a closing mark ends a sentence, ۙ also yields the clause
  after it, the ۛ pair yields the sentence to each mark and the tails after
  them, and ۜ continues after من and بل. **Fixes:** the original can restart
  the same sentence forever when two ۙ fall in one sentence (guarded to one
  restart), it lists some sentences twice (listed once here), and it throws on
  a ۜ after an unknown word (it stops there instead).
- **Decision (data):** the Submission export's text has no pause marks. Its
  `arabic_clean` column has them but spells words differently, so they cannot
  be placed reliably. The marks are taken from the classic Tanzil text,
  aligned word by word like the roots, into a `pause_marks` table; the
  displayed text is unchanged. Where the export's own marks can be placed by
  position (5,982 verses), all 3,926 agree with the classic ones.
- **Decision:** in the Original and SimplifiedMarks modes the original's
  sentence counts include diacritics and pause-mark glyphs as letters and
  words; here a sentence counts its words and letters like everything else.
  The simplified modes, the default, give the same numbers either way.
- Letter frequency: for each unit, the sum over the phrase's letters of how
  often the unit has each (repeated phrase letters count once each unless
  "count each phrase letter once" is chosen), with every comparison and number
  kind, or the original's all/any/only/none letter matches. Words, verses,
  sentences and chapters, singly or in runs.
- F9 finds sentences and whole verses with the value of the selection.
- Timing through the sidecar: word runs of any size over the whole book
  183 ms; verse runs 57 ms; letter frequency word runs 116 ms; chapter sets of
  three 50 ms.

### Phase 4 checks

All passing: engine 252, protocol 64, interface 37, Rust 7, rustfmt, clippy,
svelte-check 0 errors, oracle reproduces. Checked in the browser: Ctrl+click,
root search with `-`, F3, chapter shading, F6 on 55:13, number search for
chapters of 7 verses, F9 on 1:1. **Phase 4 complete.**

---

## Phase 5: Statistics and number theory

As before, an agent read the original's code for these features first.

- **Base value systems (fix, changes behavior).** The 20 "Base" letter-value
  systems were valued as plain sums. The original reads a word's letter
  values as the digits of a number in that base, first letter least
  significant. It does this only for a word and a single verse; chapters,
  the book and runs of verses go through its ordinary sum. Both are
  reproduced. OracleDump now captures every chapter, the book and sample
  verses under all 20 systems in a new golden file, base-systems.tsv (2,440
  rows, all matching). No existing baseline was changed.
- Number details: divisors (listed up to 1,000), the highest power, Carmichael
  numbers, the 4n±1 form and its 1-based place among primes or composites of
  that form (as the original shows it), every sum and difference of two
  squares and two cubes up to a million, and Waleed's CP index chain with its
  four bit readings. The chain of 619 is P114-C83-P23-P9-C4-C1.
- Divisor (2 to 9999, default 19) and base (2 to 36) settings, kept with the
  other preferences. Every number chip shows the base and marks divisible
  numbers; squares from 49, cubes from 125, 5th powers from 243 and 7th powers
  from 128 get a dotted underline, the thresholds the original colors at.
  Digit sums follow the base, and numbers typed in the Numbers view are read
  in it. **Decision:** the original's "interesting number" colors have no
  definition in the code that could be reproduced, so they are not shown.
- A Stats view for the selection (or the open chapter): word frequencies,
  letter statistics (order, frequency, Σ position, Σ distance, with position
  scope), the Maths sums, symmetry, and the research lists.
- Maths sums: C, V, C+V, C−V (or |C−V|), C×V and C÷V (or V÷C), each split into
  odd, even, prime and composite, with d/u. **Check:** the original's help file
  gives d/u = 7906/4885 for C+V over the whole book; the test matches it.
- Allah statistics: the classic text gives 2,816 (الله 2,265, other words with
  الله 402, words with لله 149), the figure Features.txt states. The
  Submission edition gives 2,815: it leaves out 9:128-129, and 9:129 has
  الله once.
- Research lists: Allah words, look-alikes, all words, double words and
  repeated words with a gap, over the selection or the book, paged and
  copyable as tabbed text. **Decision:** the original writes these to files
  in a Research folder; copying keeps the app from writing files unasked.
- Chapter initials imported per chapter (key 1, full 18, partial 10, and 42
  as doubly initialized, which the original forces in code). The chapter
  list marks them and selects chapters by initials, place, heavy or light, a
  Mersenne exponent, and the kind of number and verse count. **Decision:**
  the original's selection makes a chapter selection; here it filters the
  chapter list and totals what it shows, since a selection is one range.
- Ratio coloring in the reader, following `Colorize*UserRatiosText`: every
  scope from a verse to the book, by letters or value, the ratio first or
  last, and a boundary at a letter, a word's end, a pause mark, a verse's end
  or a chapter's end. A split inside a word cuts it after its letters.
  **Decision:** the ratio options last for the session; the original saves
  its ratio in its settings file.
- **Decision:** the original computes symmetry only when translations are
  showing, which reads as an accident; here it works on the Arabic text.

### Phase 5 checks

All passing: engine 268, protocol 75, interface 51, Rust 7, rustfmt, clippy,
svelte-check 0 errors, oracle reproduces (with base-systems.tsv). Checked in
the browser: ratio colors on chapter 1, each Stats tab, the number facts of
619, and the chapter classes (29 chapters open with initials).
**Phase 5 complete.**

---

## Phase 6: Grammar, meanings and translations

An agent read the original's code and data for these features first.

- **Translations (#27).** The Submission export's 13 translations and its
  transliteration are imported into the Submission content database, with
  the `arabic_clean` column as the standard-spelling text. The export marks
  footnotes with ± (the footnotes themselves are not in it); the text is
  stored as given and ± is shown as *. The reader shows any number of chosen
  translations under each verse, the English by default, remembered with the
  other preferences.
- **Translation pack.** The original's 110 Tanzil translations (about 160 MB)
  would multiply the installer, so `build_translations.py` makes an optional
  pack per edition, keyed by its verse numbers (108 translations, 187 MB; the
  other three are the per-word-list files imported below). The engine opens
  packs given with `--translations`; the app passes `translations.db` from
  its data folder when present, and the development bridge the pack in
  `next/data`. A pack for another edition is refused. As in the original, a
  chapter's Bismillah takes 1:1's translation (prefixed to verse 1 in the
  classic edition, verse 0 in Submission), and a catalog-less file (en.asad)
  is not offered. **Fix:** lines are split at LF only, as .NET reads them;
  Python's `splitlines` also broke at U+0085, which two files contain.
- **Word meanings, transliteration and grammar (#62, #64).** Imported per
  display word from `en.wordbyword`, `en.transliteration` and the Quranic
  Arabic Corpus `word-parts.txt`, aligned to both editions as the roots are
  (all verses aligned). The grammar file has no Bismillah on verse 1; like
  the original, 1:1's parts are used. Tag names in English and Arabic come
  from the original's language files. Clicking a word shows it all in the
  inspector. **Decision:** the original shows this in its title bar on hover
  and the grammar only on its Grammar tab; one card on click serves both.
- **Translation search and the language rule (#51, #67, #73).** Typed text
  that is all Quran letters, marks, Indian digits and symbols is searched in
  the Arabic; anything else searches the translations, as the original's
  `IsArabic` decides. **Decision:** آ is added to the Arabic letters; the
  original's list leaves it out, so a word typed with it went to the
  translations. Matching ignores case and folds no accents, as the original
  does; **decision:** the typed text is matched literally rather than as a
  regular expression. By default the edition's own translations are
  searched, or only those shown; the original searches all loaded ones.
  **Decision:** a pack's translations are searched when shown, so a search
  does not load all 108 into memory. Matches are marked in the translation
  line (the original lists whole verses).
- **Standard-spelling fallback (#52, #31).** An Arabic search that finds
  nothing tries the standard spelling, both simplified in the text mode, and
  lists whole verses, saying so. الكتاب finds nothing in the Uthmani text and
  151 verses this way.
- The unused translations full-text index is left empty (8.7 MB): translation
  search matches substrings in memory, which a token index cannot answer. The
  Submission database is now 41 MB with translations and word data.
- Licenses are recorded in `data/sources/submission/SOURCE.md` for the owner's
  review; nothing is published.
- **Not done:** the original's interface translations (`Languages/*.txt`,
  five languages) belong with the help and accessibility work in Phase 11.

### Phase 6 checks

All passing: engine 280, protocol 80, interface 51, Rust 7, rustfmt, clippy,
svelte-check 0 errors, oracle reproduces. Checked in the browser with the
pack: 122 translations in two groups, Pickthall under the verses, the word
card for لِلَّهِ. **Phase 6 complete.**

---
