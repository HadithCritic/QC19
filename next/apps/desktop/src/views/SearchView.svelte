<script lang="ts">
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine } from "../lib/engine/client";
  import type { SearchResult, SearchVerse, Wordness } from "../lib/engine/types";
  import { app } from "../lib/state/app.svelte";
  import { humanize } from "../lib/systems";

  const PAGE = 50;

  const WORDNESS: { value: Wordness; label: string }[] = [
    { value: "any", label: "Anywhere" },
    { value: "whole", label: "Whole word" },
    { value: "part", label: "Inside a word" },
  ];

  let term = $state("");
  let wordness = $state<Wordness>("any");
  let result = $state<SearchResult | null>(null);
  let verses = $state<SearchVerse[]>([]);
  let error = $state<string | null>(null);
  let loading = $state(false);

  async function run(offset: number): Promise<void> {
    if (!term.trim()) return;
    loading = true;
    error = null;
    try {
      const page = await engine.search(term, wordness, app.valueSystem, app.includeBasmalas, offset, PAGE);
      result = page;
      verses = offset === 0 ? page.verses : [...verses, ...page.verses];
    } catch (e) {
      error = describeError(e);
    } finally {
      loading = false;
    }
  }

  // Changing whether Bismillahs count changes which verses match; rerun an
  // existing search rather than leave stale results on screen.
  let searchedWith: boolean | null = null;
  $effect(() => {
    const include = app.includeBasmalas;
    if (result && searchedWith !== null && searchedWith !== include) void run(0);
    searchedWith = include;
  });

  function submit(event: SubmitEvent): void {
    event.preventDefault();
    void run(0);
  }

  function reference(verse: SearchVerse): string {
    return `${verse.chapter}:${verse.numberInChapter}`;
  }
</script>

<section class="search" aria-labelledby="search-title">
  <header>
    <h1 id="search-title">Search the text</h1>
    <p class="hint">
      Matching ignores diacritics and letter forms, as the {humanize(app.currentSystem?.textMode ?? "Original")} text mode defines them.
    </p>

    <form onsubmit={submit}>
      <label class="visually-hidden" for="search-term">Arabic text to find</label>
      <input id="search-term" class="field term arabic" lang="ar" dir="rtl" bind:value={term} placeholder="اكتب كلمة" autocomplete="off" />

      <div class="wordness" role="radiogroup" aria-label="Where the text must appear">
        {#each WORDNESS as option (option.value)}
          <label class:checked={wordness === option.value}>
            <input type="radio" name="wordness" value={option.value} bind:group={wordness} />
            {option.label}
          </label>
        {/each}
      </div>

      <button type="submit" class="button primary" disabled={loading || !term.trim()}>Search</button>
    </form>
  </header>

  <div class="results" aria-live="polite" aria-busy={loading}>
    {#if error}
      <Notice tone="error" title="The search did not run" detail={error} />
    {:else if result && result.verseCount === 0}
      <Notice title="No verse contains “{result.term}”" detail="Try fewer letters, or search anywhere in a word." />
    {:else if result}
      <p class="summary">
        <span class="num">{result.wordCount}</span> words in <span class="num">{result.verseCount}</span> verses
      </p>
      <ol>
        {#each verses as verse (verse.number)}
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
            </div>
            {#if verse.matchCount > 1}<span class="count num" title="Matches in this verse">×{verse.matchCount}</span>{/if}
          </li>
        {/each}
      </ol>
      {#if verses.length < result.verseCount}
        <button type="button" class="button more" disabled={loading} onclick={() => run(verses.length)}>
          Show {Math.min(PAGE, result.verseCount - verses.length)} more
        </button>
      {/if}
    {:else if loading}
      <Notice tone="loading" title="Searching" />
    {:else}
      <Notice title="Find a word or part of one" detail="Results show every matching verse with the matching words marked. Open one to see its numbers." />
    {/if}
  </div>
</section>

<style>
  .search {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
    padding: var(--space-5) var(--space-6) var(--space-4);
    border-bottom: 1px solid var(--rule);
    background: var(--surface);
  }

  h1 {
    margin: 0;
    font-size: var(--text-xl);
    font-weight: 600;
    letter-spacing: -0.015em;
  }

  .hint {
    margin: var(--space-1) 0 var(--space-4);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  form {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
  }

  .term {
    width: 22rem;
    height: 2.75rem;
    font-size: 1.4rem;
  }

  .wordness {
    display: inline-flex;
    padding: 3px;
    border-radius: var(--radius-md);
    background: var(--surface-sunk);
  }

  .wordness label {
    padding: 0.3rem 0.75rem;
    border-radius: 6px;
    font-size: var(--text-sm);
    color: var(--ink-muted);
    cursor: pointer;
  }

  .wordness label.checked {
    background: var(--surface);
    color: var(--ink);
    font-weight: 550;
    box-shadow: 0 1px 2px rgb(0 0 0 / 0.08);
  }

  .wordness label:has(input:focus-visible) {
    box-shadow: var(--focus);
  }

  .wordness input {
    position: absolute;
    opacity: 0;
    pointer-events: none;
  }

  .results {
    overflow-y: auto;
    padding: var(--space-4) var(--space-6) var(--space-7);
  }

  .summary {
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
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
