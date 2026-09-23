# Roadmap

Replaces phases 0 to 12 in [`worklog.md`](worklog.md), which assumed the goal
was feature parity with QuranCode 1433. The goal is now Code 19, as
[ADR 0004](decisions/0004-code-19-scope.md) records.

Stages are ordered so that each one is useful on its own and none depends on
a later one. Within a stage the work is small enough to finish and verify in
one sitting.

---

## Stage A: Make the code match the scope (done)

ADR 0004 is a decision; the code still carries what it cut. This stage is
deletion, and it comes first because everything after it is smaller once it
is done.

- Remove audio: `audio.reciters`, `Content/Reciters.cs`, the reciter columns
  in the content database, and `lib/audio/plan.ts`. No player was ever built,
  so this is the plumbing only.
- Narrow translations to Rashad Khalifa's English, the Emlaaei Arabic and the
  transliteration. Drop the 11 other WikiSubmission languages and the
  108-file Tanzil pack from the import and from the translation menu.
- Reduce the character palette to the extra characters of the active text
  mode.
- One Arabic font.

**Done when** the app builds and every test passes with those paths gone, and
the Submission database is measurably smaller. Record the new size.

Audio and the translations are done. Removing `audio.rs` also dropped the
app's only use of `reqwest`, so the desktop shell no longer links an HTTP and
TLS stack or reaches the network at all. The Submission database went from
41.2 MB to 21.2 MB.

The character palette opens beside the search box: the 28 base letters,
then only the extra characters the active text mode keeps, after the
original's `UpdateKeyboard`. There are eight such extras, not seven as first
written (ء ة ى ٱ أ إ ؤ ئ): Simplified28 keeps none, Simplified29 keeps ء,
Simplified30 keeps ة and ى, Simplified31 keeps all three, and Original and the
finer modes keep all eight. A root search offers every one. The base letters
stay because not every reader has an Arabic keyboard, and every mode keeps
them, so none of them can produce an unmatchable search.

One Arabic font: Amiri Quran sets the Quran text and every other Arabic word.
The second family, Amiri, is removed (1.3 MB of font files).

## Stage B: Findings (done)

The core of the project, and the piece with no equivalent in the original.

A **finding** is a published claim with everything needed to check it:

| Field | |
| --- | --- |
| Claim | The sentence as published |
| Expected | The number |
| Rule | The counting rule, as a query the engine can run |
| Stated or inferred | Whether the source gave the rule or it was derived |
| Source | Appendix or book, with page |
| Convention | Bismillahs, text mode, and anything else the count depends on |

Per ADR 0004 §7, the convention belongs to the finding, not to a global
setting, and an inferred rule is flagged as inferred wherever it is shown.

- A findings file, one record per finding, checked into the repository as
  data rather than code.
- An engine method that evaluates a finding and returns computed, expected,
  whether they agree, and whether the total is a multiple of 19.
- A test that runs every finding. A finding that stops reproducing fails the
  build.
- A Findings view: the list, each one's computed and expected value, and its
  provenance.

Seed it with the four already verified (the word God 2,698, the verse-number
sum 118,123, ق in chapter 50 = 57, and the Basmalah's 19 letters), then work
through the twelve mathematical appendices.

**Done when** the four seed findings reproduce as tests rather than as
one-off checks, and adding a new finding needs no engine change.

Built as `QuranCode.Core/Code19`: a `findings.tsv` catalog embedded in the
engine assembly, five measures (`words`, `letters`, `letterOccurrences`,
`wordFormOccurrences`, `verseNumberSum`), a `findings.list` protocol method
and a Findings view. Five findings seeded and all five reproduce; each runs as
its own test. Measuring the substring rule while building this corrected
ADR 0004 §7, which had said it over-counts by 12. It is 28, across 16 word
types.

## Stage C: Quranic Initials (done)

The largest missing computation, and the one Code 19 rests on most heavily
after the word counts. Feature 76 in the matrix; nothing computes it today.

- The 14 initial letters and the 29 chapters that open with them.
- Per-chapter and per-group letter counts, in the text mode each published
  table used.
- Checked against Table 9 of *Mysterious Alphabets*, which is how ق = 57 was
  confirmed.

A view rather than the original's standalone tool, per ADR 0004 §2.

**Done when** the table reproduces and its rows are findings in Stage B's
sense, with the same provenance fields.

Built as `Code19/QuranicInitials.cs` and an Initials view: the 29 chapters and
14 letters as stated data, tested against the edition's own marking and
against each chapter's opening words, with each chapter's own initials
counted through it. Two conventions are pinned by tests: chapter 42 carries
its initials over two verses, and chapter 68 writes its single ن out as نون.

The published figures came from Appendix 1 rather than Table 9, whose scan
does not OCR reliably. Ten became findings, and eight reproduce exactly: ل and
م in chapters 2 and 3, ص in 7, 19 and 38 (152), ن in 68 (133), يس in 36 (285)
and حم in 40 to 46 (2,147), along with ق in 42 matching ق in 50 (57 each).
All of them hold only with the Basmalahs counted, the opposite of the Allah
count. The appendix does not state that convention, so it is marked inferred.

**Alif does not reproduce.** Appendix 1 gives 4,502 in chapter 2 and 2,521 in
chapter 3; the engine gives 4,217 and 2,353 in every stock text mode. Which
written forms count as alif is the open question. Both are in the catalog
with the new `open` check: shown with their gap, never edited to agree, not
failing the build, and tested to still disagree so that a change settling
them is noticed.

Appendix 1's list of simple facts then added eight more, all reproducing:
6,346 and 6,234 verses, 96:1-5 as 19 words and 76 letters, chapter 96 as 19
verses and 304 letters, chapter 110 as 19 words and 110:1 as 19 letters. The
catalog gained a `verses` measure and verse-run scopes such as `96:1-5` for
them. The same list mixes conventions: chapter 96's 304 letters include its
Basmalah (285 without), while chapter 110's 19 words leave its Basmalah out
(23 with). Facts 15 to 17, about the initials data itself, are tests in
`InitialsTests`. Fact 7, the 342 words between the two Basmalahs of chapter
27, needed a scope that ends inside a verse; it was encoded later the same
day, below.

Later the same day the catalog took in Khalifa's complete per-chapter tables
from *The Computer Speaks*, as tabulated by Quran Initial Count, and grew to
53 findings. Every non-alif figure for all 29 chapters reproduces except ل in
chapters 11 and 30, where his printout has one ل fewer than the text (11:70
and 30:21); Quran Initial Count takes both to be printout errors in verse
counts whose chapter totals still hold. A new `ownInitials` measure forms the
group totals, including the two ط sets (1,767 and 2,584). Fact 7 is now
encoded: `27:1-30 before بسم` ends a scope inside a verse, and a `la+verb`
join rule counts لا and a following verb as one word, using the corpus's verb
tags. That turns 349 words into 342.

**Alif.** Quran Initial Count explains the gap: Khalifa's totals count some
hamzas as alif, chosen word by word and applied to every occurrence of that
word and its derivatives, with a few alternate spellings from early
manuscripts. The engine counts every alif-seated form (ا أ إ آ ٱ) and no
standalone hamza, which gives 4,217 in chapter 2 against 4,502. The site
publishes its word list only as images.

Khalifa's letter of 27 November 1984 then named *Quran: Visual Presentation of
the Miracle* as his final counting system, and its verse-by-verse printout for
all 13 alif chapters was transcribed: 1,435 verses, every chapter adding up to
his total. It shows his alif is the plain alif plus the hamza on no seat (the
standalone ء and the hamza on a tatweel), which matches 93% of his verses
exactly and his 17,152 alifs to within 2. The rest are choices made verse by
verse, not by word, so no rule reproduces them all. The alif findings stay
open; the rule belongs in stage E as a text mode. The whole account is in
[`research/alif-counting.md`](research/alif-counting.md).

## Stage D: 19-hunting (done)

General tools for the work, as opposed to checking a published claim.

- Scan a selection across counting dimensions (verses, words, letters,
  values) and report which totals are multiples of 19.
- The expression calculator (feature 30), a parser and not runtime
  compilation.
- Multiples of 19 marked wherever a number is shown. The Numbers view already
  does this with a chosen divisor; extend it to the rest.

**Done when** a chapter can be swept for multiples of 19 without leaving the
app.

The Stats view opens on a Multiples tab: every total of the selection (the
counts, the value, the sums of chapter and verse numbers, chapter numbers plus
verses, and each letter's frequency), with the ones that divide by the
reader's divisor set apart. It follows the Basmalah toggle and the value
system. Chapter 50 shows its 57 ق as 19 × 3 at once.

The Numbers view calculates what is typed, with a parser in place of the
original's run-time C# compilation (feature 30). Whole numbers stay exact:
19^20 is 37589973457545958193355601. Numbers are read in the chosen base;
`\` is whole division, `%` remainder, `^` and `!` work anywhere in an
expression (the original handled them only as the whole input), and nPk,
nCk, pi, e, phi and functions such as sqrt are there. Arabic text is valued
in the current system, as in the original. A result that is not whole is
reported as such instead of rounded.

## Stage E: User-defined text modes

Feature 72. A text mode changes letter counts, so it changes results, which
is why ADR 0004 kept it. It comes after findings so that a custom mode can be
checked against the findings that already pass in the stock modes.

## Stage F: Release

Installer, the licensing statement ADR 0004 §8 settles, and a first tag.

---

## Not on this roadmap

Everything ADR 0004 removed: audio, drawing, the standalone tools other than
InitialLetters, multiple Arabic fonts, DNA symbols and the geometry
calculators. The [feature matrix](compatibility/feature-matrix.md) keeps a row
for each as an inventory of the original. It is not a backlog.

## Standing decisions, amended

The work log's **No publishing** decision no longer holds: the owner has
authorized pushing to the GitHub repository, and the three commits that were
held back are pushed. Releases and tags are still the owner's call.

The other two stand. **Golden data is never edited** to make a test pass.
ADR 0004 §4 allows a golden file to be updated, but only as a deliberate,
documented correction, never to quiet a failure. **Parity first** stays the
default where no defect is demonstrated.
