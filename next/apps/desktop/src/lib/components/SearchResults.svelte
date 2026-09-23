<script lang="ts">
  import { tick } from "svelte";
  import type { SearchVerse } from "../engine/types";
  import { describe } from "../searchRequest";
  import { app } from "../state/app.svelte";
  import { SEARCH_PAGE, search } from "../state/search.svelte";
  import Notice from "./Notice.svelte";
  import TranslationMatch from "./TranslationMatch.svelte";
  import UnitResults from "./UnitResults.svelte";

  // One page of results with the matched words marked. F3 and Shift+F3 step
  // through the marks shown (Features.txt #38), wrapping at either end.

  let list: HTMLElement | undefined = $state();

  function reference(verse: SearchVerse): string {
    return `${verse.chapter}:${verse.numberInChapter}`;
  }

  async function onKey(event: KeyboardEvent): Promise<void> {
    if (event.key !== "F3" || app.view !== "search" || !list) return;
    event.preventDefault();
    const marks = [...list.querySelectorAll<HTMLElement>("mark")];
    marks.forEach((mark) => mark.classList.remove("current"));
    const mark = marks[search.stepMark(marks.length, event.shiftKey)];
    if (!mark) return;
    await tick();
    mark.classList.add("current");
    mark.scrollIntoView({ block: "center" });
  }

  const result = $derived(search.result);
  const roots = $derived(result?.roots?.filter((r) => r.kind !== "excluded") ?? []);
</script>

<svelte:window onkeydown={onKey} />

<div class="results" aria-live="polite" aria-busy={search.loading} bind:this={list}>
  {#if search.error}
    <Notice tone="error" title="The search did not run" detail={search.error} />
  {:else if search.unitResult && search.request}
    {#if search.unitResult.unitCount === 0}
      <Notice title="Nothing found: {describe(search.request)}" detail="Try other numbers, another kind of unit, or the whole book." />
    {:else}
      <div class="summary">
        <p>
          <span class="num">{search.unitResult.unitCount}</span> found: {describe(search.request)}
          {#if search.scopeFellBack}<span class="fell-back">(nothing to search within, so the whole book)</span>{/if}
        </p>
        {#if search.unitResult.truncated}
          <p class="fell-back">The search stopped at {search.unitResult.unitCount} results; narrow it to see the rest.</p>
        {/if}
        <p class="keys">
          {#if search.mark >= 0}Mark <span class="num">{search.mark + 1}</span> ·{/if}
          <kbd>F3</kbd> next marked word, <kbd>Shift</kbd>+<kbd>F3</kbd> previous
          <button type="button" class="link" onclick={() => search.clear()}>Clear results</button>
        </p>
      </div>
      <UnitResults />
    {/if}
  {:else if result && search.request && result.verseCount === 0}
    <Notice title="Nothing found for {describe(search.request)}" detail="Try fewer letters, another grouping, or the whole book." />
  {:else if result && search.request}
    <div class="summary">
      <p>
        {#if result.wordCount > 0}<span class="num">{result.wordCount}</span>{" words in "}{/if}<span class="num">{result.verseCount}</span>
        verses: {describe(search.request)}
        {#if search.scopeFellBack}<span class="fell-back">(nothing to search within, so the whole book)</span>{/if}
      </p>
      {#if result.foundIn === "translations"}
        <p>Searched in the translations, since the text is not in Arabic letters.</p>
      {:else if result.foundIn === "emlaaei"}
        <p>Not in the Uthmani text as typed; found in the standard spelling, so whole verses are listed.</p>
      {/if}
      {#if roots.length > 0}
        <p class="roots">
          Roots
          {#each roots as root, index (index)}
            <span class="arabic" lang="ar" dir="rtl" title={root.root ? `From “${root.term}”` : `No root found for “${root.term}”`}>{root.root ?? "?"}</span>
          {/each}
        </p>
      {/if}
      <p class="keys">
        {#if search.mark >= 0}Mark <span class="num">{search.mark + 1}</span> ·{/if}
        <kbd>F3</kbd> next mark, <kbd>Shift</kbd>+<kbd>F3</kbd> previous
        <button type="button" class="link" onclick={() => search.clear()}>Clear results</button>
      </p>
    </div>
    <ol>
      {#each search.verses as verse (verse.number)}
        <li>
          <button type="button" class="ref num" onclick={() => app.goTo({ first: verse.number, last: verse.number })} aria-label="Open {reference(verse)} in the reader">
            {reference(verse)}
          </button>
          <div class="verse-text">
            {#if verse.bismillahHighlights.length > 0 && verse.bismillah}
              <p class="quran bismillah" lang="ar" dir="rtl">
                {#each verse.bismillah.split(" ") as word, index (index)}
                  {#if verse.bismillahHighlights.includes(index)}<mark>{word}</mark>{:else}{word}{/if}{" "}
                {/each}
              </p>
            {/if}
            <p class="quran" lang="ar" dir="rtl" class:whole={!verse.aligned}>
              {#each verse.words as word, index (index)}
                {#if verse.highlights.includes(index)}<mark>{word}</mark>{:else}{word}{/if}{" "}
              {/each}
            </p>
            {#each verse.translations ?? [] as line (line.key)}<TranslationMatch {line} />{/each}
          </div>
          {#if verse.score !== null}
            <span class="count num" title="Similarity to the starting verse">{Math.round(verse.score * 100)}%</span>
          {:else if verse.matchCount > 1}
            <span class="count num" title="Matches in this verse">×{verse.matchCount}</span>
          {/if}
        </li>
      {/each}
    </ol>
    {#if search.verses.length < result.verseCount}
      <button type="button" class="button more" disabled={search.loading} onclick={() => search.more()}>
        Show {Math.min(SEARCH_PAGE, result.verseCount - search.verses.length)} more
      </button>
    {/if}
  {:else if search.loading}
    <Notice tone="loading" title="Searching" />
  {:else}
    <Notice
      title="Find a word, a root or a phrase"
      detail="In the reader, Ctrl+click a word for words of the same root. With a word or verse chosen, F4 finds related words, F5 related verses, F6 similar verses, F7 the same text, F8 the same text with its marks, and F9 sentences and verses of the same value."
    />
  {/if}
</div>

<style>
  .results {
    overflow-y: auto;
    padding: var(--space-4) var(--space-6) var(--space-7);
  }

  .summary {
    display: grid;
    gap: var(--space-1);
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .summary p {
    margin: 0;
  }

  .fell-back {
    color: var(--danger);
  }

  .roots .arabic {
    margin-inline-start: var(--space-2);
    font-size: 1.1rem;
    color: var(--ink);
  }

  .keys {
    font-size: var(--text-xs);
  }

  .keys .link {
    margin-inline-start: var(--space-3);
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font-size: var(--text-xs);
    font-weight: 550;
  }

  kbd {
    padding: 0 0.3em;
    border: 1px solid var(--rule-strong);
    border-radius: 4px;
    font: inherit;
    font-size: 0.95em;
  }

  ol {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  li {
    display: grid;
    grid-template-columns: 4.5rem 1fr auto;
    align-items: baseline;
    gap: var(--space-4);
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
  }

  .ref {
    justify-self: start;
    padding: 0.1rem 0.4rem;
    border: 0;
    border-radius: var(--radius-sm);
    background: none;
    font-size: var(--text-sm);
    color: var(--lapis);
  }

  .ref:hover {
    background: var(--lapis-soft);
  }

  .quran {
    margin: 0;
    font-size: 1.45rem;
    line-height: 2.1;
  }

  .quran.whole {
    background: var(--gilt-soft);
    border-radius: var(--radius-sm);
  }

  mark {
    color: inherit;
    background: var(--gilt-soft);
    text-decoration: underline 2px var(--gilt);
    text-underline-offset: 0.35em;
    border-radius: 2px;
  }

  mark:global(.current) {
    background: var(--gilt);
    color: var(--surface);
  }

  .bismillah {
    font-size: 1.1rem;
    color: var(--ink-muted);
  }

  .count {
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  .more {
    margin-top: var(--space-4);
  }
</style>
