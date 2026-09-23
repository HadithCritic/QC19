import type {
  Bookmark,
  Chapter,
  ChapterStats,
  CountingOptions,
  Distance,
  HistoryEntry,
  HistoryKind,
  WordLocation,
  EngineInfo,
  NumberInfo,
  SearchMethod,
  SearchResult,
  Stats,
  SystemValue,
  ValueSystem,
  Verse,
  VerseRange,
  VerseValue,
  Wordness,
} from "./types";

/** A failure the UI can explain. `code` is one of the engine's or the bridge's codes. */
export class EngineError extends Error {
  constructor(
    readonly code: string,
    message: string,
  ) {
    super(message);
    this.name = "EngineError";
  }
}

type Transport = (method: string, params: unknown) => Promise<unknown>;

function isErrorShape(value: unknown): value is { code: string; message: string } {
  return (
    typeof value === "object" &&
    value !== null &&
    typeof (value as { code?: unknown }).code === "string" &&
    typeof (value as { message?: unknown }).message === "string"
  );
}

const tauriTransport: Transport = async (method, params) => {
  const { invoke } = await import("@tauri-apps/api/core");
  try {
    return await invoke("engine", { method, params });
  } catch (error) {
    if (isErrorShape(error)) throw new EngineError(error.code, error.message);
    throw new EngineError("internal", String(error));
  }
};

/** Development in a plain browser: see scripts/engine-dev-bridge.ts. */
const devTransport: Transport = async (method, params) => {
  let response: Response;
  try {
    response = await fetch("/__engine", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ method, params }),
    });
  } catch {
    throw new EngineError("engine_unavailable", "The development engine bridge is not reachable.");
  }
  const body: unknown = await response.json();
  if (!response.ok) {
    throw isErrorShape(body) ? new EngineError(body.code, body.message) : new EngineError("internal", "Unexpected reply.");
  }
  return body;
};

const unavailableTransport: Transport = () =>
  Promise.reject(new EngineError("engine_unavailable", "QuranCode's engine runs inside the desktop app."));

function pickTransport(): Transport {
  if (typeof window !== "undefined" && "__TAURI_INTERNALS__" in window) return tauriTransport;
  return import.meta.env.DEV ? devTransport : unavailableTransport;
}

const transport = pickTransport();

function call<T>(method: string, params?: object): Promise<T> {
  return transport(method, params ?? null) as Promise<T>;
}

/** Typed access to every engine method. */
export const engine = {
  info: () => call<EngineInfo>("engine.info"),
  chapters: () => call<Chapter[]>("chapters.list"),
  systems: () => call<ValueSystem[]>("systems.list"),
  chapterVerses: (chapter: number) => call<Verse[]>("chapter.verses", { chapter }),
  chapterValues: (chapter: number, valueSystem: string, counting: CountingOptions) =>
    call<VerseValue[]>("chapter.values", { chapter, valueSystem, counting }),
  stats: (range: VerseRange, valueSystem: string, counting: CountingOptions) =>
    call<Stats>("selection.stats", { ...range, valueSystem, counting }),
  parseReference: (text: string, valueSystem: string, counting: CountingOptions) =>
    call<VerseRange>("reference.parse", { text, valueSystem, counting }),
  chapterStats: (valueSystem: string, counting: CountingOptions) =>
    call<ChapterStats[]>("chapters.stats", { valueSystem, counting }),
  distance: (from: WordLocation, to: WordLocation, valueSystem: string, counting: CountingOptions) =>
    call<Distance>("words.distance", { from, to, valueSystem, counting }),
  bookmarks: () => call<Bookmark[]>("bookmarks.list"),
  saveBookmark: (range: VerseRange, note: string) => call<Bookmark>("bookmarks.save", { ...range, note }),
  deleteBookmark: (id: number) => call<boolean>("bookmarks.delete", { id }),
  history: (kind: HistoryKind, limit = 50) => call<HistoryEntry[]>("history.list", { kind, limit }),
  addBrowse: (range: VerseRange) => call<boolean>("history.add", { kind: "browse", ...range }),
  addFind: (term: string, wordness: Wordness) => call<boolean>("history.add", { kind: "find", term, wordness }),
  clearHistory: (kind: HistoryKind) => call<boolean>("history.clear", { kind }),
  analyzeNumber: (value: string) => call<NumberInfo>("number.analyze", { value }),
  textValues: (text: string, valueSystems?: string[]) =>
    call<SystemValue[]>("text.values", valueSystems ? { text, valueSystems } : { text }),
  /** Every search method answers with one page of the same result shape. */
  search: (method: SearchMethod, params: Record<string, unknown>) => call<SearchResult>(method, params),
};

/** The message to show for any thrown value. */
export function describeError(error: unknown): string {
  if (error instanceof EngineError) return error.message;
  if (error instanceof Error) return error.message;
  return "Something went wrong.";
}

/**
 * Wraps an async loader so only the most recent call's result is kept. The
 * engine answers in order, but the UI can fire faster than it renders; a stale
 * answer must never overwrite a newer one.
 */
export function latest<A extends unknown[], R>(load: (...args: A) => Promise<R>) {
  let generation = 0;
  return async (...args: A): Promise<{ current: boolean; value: R }> => {
    const mine = ++generation;
    const value = await load(...args);
    return { current: mine === generation, value };
  };
}
