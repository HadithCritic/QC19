import { describeError, engine } from "../engine/client";
import type { SearchResult, SearchVerse } from "../engine/types";
import { scopeFor, step, toCall, type ScopeChoice, type SearchRequest } from "../searchRequest";
import { app } from "./app.svelte";

const PAGE = 50;

/**
 * The current search, shared by the search view, the reader (Ctrl+click and
 * F4 to F9 start searches) and the chapter list (which shades by matches).
 */
class SearchState {
  request = $state<SearchRequest | null>(null);
  scope = $state<ScopeChoice>("book");
  result = $state<SearchResult | null>(null);
  verses = $state<SearchVerse[]>([]);
  loading = $state(false);
  error = $state<string | null>(null);
  /** Set when the chosen scope had nothing to search within, so the book was searched. */
  scopeFellBack = $state(false);
  /** The mark F3 is on, among the loaded results' marks; -1 for none. */
  mark = $state(-1);

  private scopeParam: Record<string, unknown> = {};
  private generation = 0;

  /** Starts a search from its first page. */
  async start(request: SearchRequest): Promise<void> {
    const previous = this.result?.verseNumbers ?? null;
    const scope = scopeFor(this.scope, app.selection, previous);
    this.scopeFellBack = this.scope !== "book" && scope === undefined;
    this.scopeParam = scope ? { scope } : {};
    this.request = request;
    app.view = "search";
    await this.load(0);
    if (request.kind === "text" && !this.error) {
      engine.addFind(request.term.trim(), request.wordness).catch(() => {
        // History is a convenience; failing to record it is not an error to show.
      });
    }
  }

  /** Loads more results after the ones shown. */
  more(): Promise<void> {
    return this.load(this.verses.length);
  }

  /** Runs a changed request over the same verses (a new threshold or method for similar verses). */
  refine(request: SearchRequest): Promise<void> {
    this.request = request;
    return this.load(0);
  }

  /** Reruns the current search with the same scope, after the counting options change. */
  rerun(): Promise<void> {
    return this.request ? this.load(0) : Promise.resolve();
  }

  clear(): void {
    this.generation++;
    this.request = null;
    this.result = null;
    this.verses = [];
    this.error = null;
    this.mark = -1;
    this.loading = false;
  }

  /** Moves the F3 mark; returns its index, or -1 when there are no marks. */
  stepMark(count: number, backward: boolean): number {
    this.mark = step(this.mark, count, backward);
    return this.mark;
  }

  private async load(offset: number): Promise<void> {
    const request = this.request;
    if (!request) return;
    const generation = ++this.generation;
    const { method, params } = toCall(request);
    this.loading = true;
    this.error = null;
    try {
      const page = await engine.search(method, {
        ...params,
        ...this.scopeParam,
        valueSystem: app.valueSystem,
        counting: { ...app.counting },
        offset,
        limit: PAGE,
      });
      if (generation !== this.generation) return;
      this.result = page;
      this.verses = offset === 0 ? page.verses : [...this.verses, ...page.verses];
      if (offset === 0) this.mark = -1;
    } catch (e) {
      if (generation !== this.generation) return;
      this.error = describeError(e);
      if (offset === 0) {
        this.result = null;
        this.verses = [];
      }
    } finally {
      if (generation === this.generation) this.loading = false;
    }
  }
}

export const search = new SearchState();
export const SEARCH_PAGE = PAGE;
