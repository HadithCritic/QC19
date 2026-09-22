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

