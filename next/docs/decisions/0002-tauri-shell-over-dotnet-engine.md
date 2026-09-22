# ADR 0002: Tauri shell over a .NET engine sidecar

Status: accepted
Date: 2026-09-22
Supersedes: the "Desktop UI: Avalonia" line of ADR 0001. Everything else in
0001 stands, in particular the decision to keep the engine in C#.

## Context

ADR 0001 kept the computational core in C# because it reproduces the legacy
numbers exactly and is covered by golden tests. That reasoning is unchanged.

The UI is the main thing the owner wants modernized, and the owner already
builds desktop apps with Tauri. A web front end gives the most control over
Arabic typography, dense tables and theming, and the owner is fluent in it.

## Decision

```
┌──────────────────────── Tauri app ─────────────────────────┐
│  WebView: Svelte + TypeScript                              │
│      │  invoke("engine", {method, params})                 │
│  Rust: owns one child process, correlates requests by id,  │
│        enforces a timeout, restarts the child if it dies   │
└──────┼─────────────────────────────────────────────────────┘
       │  stdin/stdout, one JSON object per line
┌──────┴─────────────────────────────────────────────────────┐
│  qurancode-engine: .NET, Native AOT, QuranCode.Core        │
│  reads content.db (bundled, read-only)                     │
└────────────────────────────────────────────────────────────┘
```

1. **Engine sidecar.** `apps/QuranCode.Engine` is a console host over
   `QuranCodeEngine`, compiled with Native AOT so it starts in milliseconds and
   needs no .NET runtime on the user's machine. It speaks newline-delimited
   JSON on stdin/stdout and nothing else: no sockets, no ports.
2. **Rust bridge.** The Tauri backend exposes exactly one command, `engine`,
   which forwards a method name and parameters. The webview cannot spawn
   processes, touch the file system or reach the network; the shell plugin is
   not installed. This keeps the capability surface to one audited function.
3. **Front end.** Svelte 5 with TypeScript and Vite. No component library: the
   design system is a small set of CSS tokens and a handful of components,
   because every dependency here is weight in the bundle and in review.
4. **Data.** `content.db` ships as a Tauri resource and is opened read-only.
   User data (bookmarks, history) goes in a separate `user.db` in the OS
   app-data directory, owned by the engine, so the content file is never
   written to.

## Protocol

Request: `{"id": 7, "method": "verse.value", "params": {...}}`

Response: `{"id": 7, "result": ...}` or
`{"id": 7, "error": {"code": "invalid_params", "message": "..."}}`

- One request per line, one response per line, UTF-8.
- The engine handles requests in order. Valuation and search over the whole
  book run in milliseconds, so a queue is simpler than cancellation and loses
  nothing a user can perceive. If that stops being true the protocol already
  carries ids, so concurrent handling can be added without a format change.
- Error codes are a closed set (`invalid_params`, `not_found`,
  `unknown_method`, `internal`). Messages are for the UI; stack traces go to
  stderr, which the Rust side logs and never forwards to the webview.

## Editions

The four legacy editions were launched with modifier keys. They become one
application:

| Legacy edition | Now |
| --- | --- |
| Standard | The default |
| Research | A setting that reveals research methods and extra value systems |
| Ultimate (C# scripting) | A setting, **off by default**, labelled as running arbitrary code |
| BigNumbers | No mode: big-number work is computed when a screen asks for it |

Scripting will only ever run code the user typed or explicitly opened in the
script editor, never code that arrives inside a project or data file.

## Consequences

Positive: modern UI in the owner's own stack; the engine and its golden tests
are untouched; the webview has no ambient authority.

Negative: two toolchains (Rust and .NET) to build a release; a process
boundary to maintain; AOT forbids reflection-based JSON, so every message type
is registered with the source generator.

The Avalonia shell (`apps/QuranCode.Desktop`) is removed once the Tauri app
covers its four pages, so there is one UI to maintain.

## Licensing

The legacy source is GPLv3, and this is a derivative work, so it is GPLv3 too.
That also makes the GPL-licensed Quranic Arabic Corpus (grammar, morphology)
compatible. The Arabic display font is Amiri Quran (SIL OFL). The legacy
`me_quran.ttf` and `Al_Mushaf.ttf` have no license text and are not
redistributed.
