# Work log: Phases 0 to 12

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

