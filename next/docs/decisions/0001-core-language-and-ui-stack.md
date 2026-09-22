# ADR 0001: Keep the computational core in C#; reject a Rust rewrite

Status: accepted
Date: 2026-09-22

## Context

The modernization brief names Tauri + Svelte + Rust + SQLite as the primary
candidate architecture, with "modern .NET + Avalonia" acceptable "if retaining
the existing C# algorithms substantially reduces migration risk."

The brief also instructs (§51) not to copy an architecture because it is new,
and to justify decisions with evidence from the actual software. This ADR
records that evidence.

## Evidence from the existing codebase

### 1. The computational core has no Windows dependency

Scanned every core library for `System.Windows` / `System.Drawing`:

| Project | Windows coupling |
| --- | --- |
| Model | none |
| Server | none |
| Client | none |
| DataAccess | none |
| Maths | none |
| Research | none |
| Utilities | `System.Drawing` — 72 `Color` constants only |
| Globals | `System.Drawing` — 12 `Color` constants only |

The only coupling is presentation constants (`DIVISOR_COLOR`,
`CARMICHAEL_NUMBER_COLOR`) that leaked into a math library. Removing two
`using` directives makes the entire engine portable. Nothing about the core
requires Windows, and nothing about it requires leaving .NET.

### 2. The slow part is the object graph, not the arithmetic

Measured on this machine with `OracleDump --bench`, engine only, no UI:

| Step | Time | Managed heap after |
| --- | --- | --- |
| Client construct (loads 274 numerical systems) | 134 ms | 5.9 MB |
| **Build book** | **3,804 ms** | **136.4 MB** |
| Rebuild book (same parameters) | 3,267 ms | 203.5 MB |
| Value the entire book | **34 ms** | 203.6 MB |
| Value all 6,236 verses individually | 57 ms | 203.6 MB |
| Switch numerical system (load + rebuild) | 1,811 ms | 272.4 MB |

Building the object graph costs **112x more than computing over it**. The
engine allocates 327,792 `Letter` objects, 77,878 `Word` objects (29 fields
each) and full back-references, then values the whole book in 34 ms.

This is the single most important finding in the audit. **The algorithms are
not the performance problem.** Rewriting them in Rust would target the 34 ms
and leave the 3,804 ms untouched.

Working set climbs 17 MB → 188 MB → 261 MB → 306 MB across successive
rebuilds, so the legacy engine also fails to release the previous graph
promptly. That is a lifetime problem, not a language problem.

### 3. The parts the brief wants made data-driven already are

- **§23 "Value systems must become data-driven."** `NumericalSystem` is named
  `TextMode_LetterOrder_LetterValue` and holds a `Dictionary<char, long>`
  loaded from `Values/*.txt` (410 TSV files, 274 currently loadable). It is
  already data.
- **§24 "Text modes must become explicit."** `SimplificationSystem.Simplify`
  is an ordered find/replace over rules read from `Rules/<method>/<mode>.txt`.
  Eight modes, shipped as data. It is already explicit.

The brief's two biggest "modernize this" items describe work that the legacy
authors already did. The valuable asset is the **rule and value data**, not the
code that applies it. That data must survive byte-exact; the ~10 lines of code
that consume it are trivial in any language.

### 4. The genuinely risky logic is large, subtle, and entirely portable

`Server.cs` is 21,380 lines of valuation and search semantics with no UI
dependency. The value engine alone is governed by:

- 21 modifier flags on `NumericalSystem` (`AddToLetterLNumber`,
  `AddToWordVDistance`, `AbsolutePositions`, …)
- 4 sign-alternation flags (`AlternateLetterValues` … `AlternateChapterValues`)
- a `CalculationMode` enum (sum of letter values, digit sums, digital roots,
  and word-level variants)
- positional metadata and distance-to-previous/next per letter, word, verse
  and chapter

These interact combinatorially. This is exactly the code where a
language-to-language rewrite silently changes results, which §51 forbids.

## Decision

**Keep the computational core in C# and target modern .NET. Do not rewrite the
engine in Rust.**

Specifically:

1. **Core engine**: .NET 9 class libraries, lifted from today's `Model`,
   `Server`, `Research` and `Utilities`, then restructured behind clean
   boundaries. Portable, no Windows dependency.
2. **Storage**: SQLite, as the brief specifies. Adopted in full — the evidence
   supports it directly, since the object graph is the bottleneck.
3. **Desktop UI**: Avalonia. Cross-platform, keeps one language end to end,
   and the brief lists it as acceptable.
4. **CLI**: the same core, per brief §42.

## Why not Tauri + Rust

| Criterion | Assessment |
| --- | --- |
| Performance | Targets the 34 ms, not the 3,804 ms. The win comes from replacing the object graph, which is a data-structure change available in any language. |
| Correctness | Requires reimplementing ~21k lines of combinatorial valuation logic with no compiler assistance for equivalence. Highest-risk option against the brief's "do not silently alter a numerical algorithm." |
| Migration risk | Highest of the three. The brief's own escape clause ("if retaining the existing C# algorithms substantially reduces migration risk") is satisfied. |
| Bundle size | Genuine advantage: ~5 MB vs ~30 MB. Small against a 615 MB install whose bloat is 260 MB of data. |
| Cross-platform | Equivalent to Avalonia. |

The distribution win is real but is dwarfed by the data-side reduction, which
is language-independent.

## Why not Electron

Rejected per brief §37. It would add ~130 MB of Chromium to solve a UI problem
while forcing either a TypeScript rewrite of the engine or a .NET sidecar — in
which case Electron is only an expensive browser.

## What this does not decide

This ADR keeps the *language*. It explicitly does **not** keep the
architecture. Still to be replaced, per the brief:

- the eager object graph (→ columnar data + SQLite, ADR 0002)
- static god-objects `Server` / `Client` (→ explicit services)
- the 87k-line `MainForm` (→ Avalonia feature modules)
- 63 MB of precomputed number text (→ computed at runtime)
- 156 MB of always-shipped translations (→ optional packs)

## Consequences

Positive: the algorithms stay in the language they were written and validated
in; golden tests compare like with like; no combinatorial reimplementation
risk; one language across engine, CLI and UI.

Negative: ~30 MB runtime vs Tauri's ~5 MB; Avalonia has a smaller ecosystem
than the web UI stack; .NET is a heavier dependency than a Rust binary.

Accepted, because correctness of the numerical semantics is the project's
binding constraint and distribution size is dominated by data, not runtime.
