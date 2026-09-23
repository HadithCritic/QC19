# ADR 0004: Code 19 as the purpose; scope cut to match

Status: accepted
Date: 2026-09-23
Supersedes: the phased roadmap in `docs/worklog.md` (phases 0–12), which
assumed the goal was feature parity with the original. Everything in ADR 0001
(engine in C#), ADR 0002 (Tauri shell) and ADR 0003 (editions, verse 0)
stands.

## Context

Until now the project's stated goal was to reproduce QuranCode 1433's feature
set on a modern stack. 60 of its 79 features are done on that basis.

The owner's actual purpose is narrower and more specific: **Code 19** —
Rashad Khalifa's numerical analysis of the Quran. Finding multiples of 19 in
counts and values, and checking published results. Large parts of the
original serve a different kind of research and are not wanted.

This ADR records the change of purpose, what it removes, and what it adds.

## The goal

Code 19 work, on the Submission edition, checkable against Khalifa's
published results.

Before any Code 19 feature existed, four of his figures already reproduced
exactly from the engine and the Submission text:

| Result | Computed | Published | Source |
| --- | ---: | ---: | --- |
| Occurrences of the word God | 2,698 = 19×142 | 2,698 | Appendix 1 |
| Sum of those verse numbers | 118,123 = 19×6,217 | 118,123 | Appendix 1 |
| ق in chapter 50 | 57 = 19×3 | 57 | *Mysterious Alphabets*, Table 9 |
| Basmalah | 4 words, 19 letters | 4 / 19 | Appendix 1 |

So the arithmetic and the text are already sound. What is missing is
presentation and the infrastructure to state a finding, compute it, and say
whether it holds.

## Decisions

### 1. Removed

| Removed | Features |
| --- | --- |
| Audio | 45, 56, 65, 66 |
| Drawing | 57, 58 |
| Standalone tools, except InitialLetters | 74, 75, 77, 79, and the 11 unnumbered legacy programs |
| Multiple Arabic fonts | 17 |
| DNA symbols | 23 |
| Geometry calculators | 78 |

One application, one Arabic font. Removal is from scope, not from history:
the legacy source stays in `C#/` and the feature matrix keeps a row for each.

### 2. Kept

- **InitialLetters (76)** folds in as a first-class view, not a tool. The
  Quranic Initials are central to Code 19 and nothing computes them yet.
- **Expression calculator (30)**, because checking a number against 19 by
  hand is the core activity. Still a parser, not runtime code compilation.
- **User-defined text modes (72)**, because a text mode changes letter counts
  and therefore changes Code 19 results.
- **The character palette (46)**, reduced to the seven extra characters per
  text mode. Its job is preventing searches that cannot match: in
  Simplified29 the letter ة has already been folded to ه, so offering it
  would guarantee zero results.

### 3. Translations

Rashad Khalifa's English, the Emlaaei Arabic text and the transliteration.

The 11 other WikiSubmission languages and the 108-file Tanzil pack are
dropped. Emlaaei is kept because the search fallback needs it, and the
transliteration because the word inspector shows it; neither is a
translation in the ordinary sense.

### 4. Parity is no longer absolute

Until now the rule was to reproduce the original exactly, including its bugs,
with four documented defects deliberately preserved.

That rule is relaxed: **where the original is demonstrably wrong, fix it,
document the change, and update the affected golden file deliberately.**
Parity remains the default, and the golden data remains the regression net
against accidental drift. What changes is that a defect may now be corrected
on purpose, with a record, instead of being reproduced forever.

The four preserved legacy behaviors are not revisited by this ADR. Each needs
its own decision, because each is load-bearing for numbers already published.

### 5. Editions

The **Submission edition is the sole Code 19 text**. Chapter 9 ends at verse
127: Appendix 24 is the argument for removing 9:128–129, and the classic text
computes on the assumption that they belong, which is exactly what this work
rejects.

The classic Tanzil edition stays as a **test-only fixture**, never shipped.
It is what the 17 golden files are captured against, and it matters more now
that defects may be corrected rather than preserved.

### 6. Sources of truth

| Source | Role |
| --- | --- |
| Appendices, 1992 edition (WikiSubmission archive) | **Authoritative** for claims and expected values |
| *Mysterious Alphabets*, *The Computer Speaks* | Initial-letter tables, counting conventions |
| Submitters Perspective, 1985–1990 | Supporting context |

The appendix PDFs carry clean text layers, so they are read directly; no
scraping of a live site is needed. Twelve of the 38 appendices argue
numerically, by a count of explicit 19-arithmetic in their own text
(`19x`, `x 19`, `multiple of 19`, `divisible by 19`):

1, 2, 15, 19, 24, 25, 26, 27, 29, 37, 38 and the introduction — about 98
pages. Appendix 23 is added for its revelation-order table, which is data
rather than argument.

*The Computer Speaks* and *Mysterious Alphabets* are scans with no text
layer. Their OCR is unreliable for digits — Table 9 comes out with `2018
i511` and `1s9` — so their tables are read by rendering the page and reading
it, which is legible and is how ق = 57 was confirmed.

### 7. Counting conventions are per finding, not global

No single global setting can serve. The count of the word God excludes the
112 unnumbered Basmalahs; other results require them. So **every finding
records the convention it was computed under** and displays it.

Where a source states its rule, the rule is used. Where it does not, the rule
is **inferred and the inference is flagged in the finding**, so a reader can
see which figures rest on an assumption.

The first such rule, derived from a published index of all 2,698 occurrences
and verified to reproduce both of Appendix 1's totals exactly:

> Count a word when its normalized form is one of eleven whole words — الله،
> لله، بالله، والله، ولله، فالله، فلله، تالله، وتالله، ءالله، ابالله —
> matched as a **whole word, never a substring**, in **numbered verses
> only**. اللهم is excluded.

A substring rule gives 2,726 instead, which is not a multiple of 19: 28
extra occurrences across 16 word types, none of them the name — ظِلَٰلُهَا
(their shadows), خِلَٰلِهِۦ (through it), ٱللَّهَبِ (the flame), ٱللَّهْوِ
(the amusement), يُضْلِلْهُ (misleads him) and others. اللهم is among them,
which is why it has to be excluded by name rather than by spelling.

> Corrected 2026-09-23. This paragraph first said the substring rule
> over-counts by 12. Measuring it while building the findings tests gave 28
> across 16 types. The whole-word rule above, and both Appendix 1 totals, are
> unaffected; only this aside was wrong. The measurement is now a test
> (`FindingsTests.ASubstringRuleWouldOverCount`).

### 8. Licensing

The owner confirms this is part of a WikiSubmission project, so the Arabic
text and the translations raise no third-party issue. ADR 0003's open
question is closed. The work remains GPL-3.0 as a derivative of the original.

## Consequences

Positive: a much smaller surface; the removed features were the bulk of the
outstanding work. The engine needs no change to serve Code 19 — four
published results already reproduce.

Negative: the feature matrix no longer measures progress, because most of
what it tracks is now out of scope. It stays as an inventory of the original,
not a plan. A new roadmap replaces phases 0–12.

Open: which appendices beyond the twelve deserve inclusion as the findings
list grows, and whether any of the four preserved legacy defects should now
be corrected.
