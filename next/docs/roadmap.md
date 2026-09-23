# Roadmap

Replaces phases 0 to 12 in [`worklog.md`](worklog.md), which assumed the goal
was feature parity with QuranCode 1433. The goal is now Code 19, as
[ADR 0004](decisions/0004-code-19-scope.md) records.

Stages are ordered so that each one is useful on its own and none depends on
a later one. Within a stage the work is small enough to finish and verify in
one sitting.

---

## Stage A: Make the code match the scope — audio and translations done

ADR 0004 is a decision; the code still carries what it cut. This stage is
deletion, and it comes first because everything after it is smaller once it
is done.

- Remove audio: `audio.reciters`, `Content/Reciters.cs`, the reciter columns
  in the content database, and `lib/audio/plan.ts`. No player was ever built,
  so this is the plumbing only.
- Narrow translations to Rashad Khalifa's English, the Emlaaei Arabic and the
  transliteration. Drop the 11 other WikiSubmission languages and the
  108-file Tanzil pack from the import and from the translation menu.
- Reduce the character palette to the seven extra characters of the active
  text mode.
- One Arabic font.

**Done when** the app builds and every test passes with those paths gone, and
the Submission database is measurably smaller. Record the new size.

Audio and the translations are done. Removing `audio.rs` also dropped the
app's only use of `reqwest`, so the desktop shell no longer links an HTTP and
TLS stack or reaches the network at all. The Submission database went from
41.2 MB to 21.2 MB.

**Still open:** the character palette (feature 46) is not yet reduced to the
seven extra characters of the active text mode, and the Arabic fonts have not
been narrowed. Both are presentation and neither blocks a later stage.

## Stage B: Findings — **done**

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

Seed it with the four already verified — the word God 2,698, the verse-number
sum 118,123, ق in chapter 50 = 57, and the Basmalah's 19 letters — then work
through the twelve mathematical appendices.

**Done when** the four seed findings reproduce as tests rather than as
one-off checks, and adding a new finding needs no engine change.

Built as `QuranCode.Core/Code19`: a `findings.tsv` catalog embedded in the
engine assembly, five measures (`words`, `letters`, `letterOccurrences`,
`wordFormOccurrences`, `verseNumberSum`), a `findings.list` protocol method
and a Findings view. Five findings seeded and all five reproduce; each runs as
its own test. Measuring the substring rule while building this corrected
ADR 0004 §7, which had said it over-counts by 12 — it is 28, across 16 word
types.

## Stage C: Quranic Initials

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

## Stage D: 19-hunting

General tools for the work, as opposed to checking a published claim.

- Scan a selection across counting dimensions — verses, words, letters,
  values — and report which totals are multiples of 19.
- The expression calculator (feature 30), a parser and not runtime
  compilation.
- Multiples of 19 marked wherever a number is shown. The Numbers view already
  does this with a chosen divisor; extend it to the rest.

**Done when** a chapter can be swept for multiples of 19 without leaving the
app.

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

The other two stand. **Golden data is never edited** to make a test pass —
ADR 0004 §4 allows a golden file to be updated, but only as a deliberate,
documented correction, never to quiet a failure. **Parity first** stays the
default where no defect is demonstrated.
