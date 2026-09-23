<script lang="ts">
  import CharacterPalette from "../lib/components/CharacterPalette.svelte";
  import FrequencyForm from "../lib/components/FrequencyForm.svelte";
  import NumbersForm from "../lib/components/NumbersForm.svelte";
  import SearchResults from "../lib/components/SearchResults.svelte";
  import { engine } from "../lib/engine/client";
  import type { Grouping, HistoryEntry, SimilarityMethod, Wordness } from "../lib/engine/types";
  import type { ScopeChoice } from "../lib/searchRequest";
  import { app } from "../lib/state/app.svelte";
  import { search } from "../lib/state/search.svelte";
  import { humanize } from "../lib/systems";

  type Kind = "text" | "roots" | "numbers" | "frequency";

  const KINDS: { value: Kind; label: string }[] = [
    { value: "text", label: "Text" },
    { value: "roots", label: "Roots" },
    { value: "numbers", label: "Numbers" },
    { value: "frequency", label: "Letter frequency" },
  ];

  const WORDNESS: { value: Wordness; label: string }[] = [
    { value: "any", label: "Anywhere" },
    { value: "whole", label: "Whole word" },
    { value: "part", label: "Inside a word" },
  ];

  const GROUPINGS: { value: Grouping; label: string; roots: boolean }[] = [
    { value: "any", label: "any of the words", roots: true },
    { value: "all", label: "all of the words", roots: true },
    { value: "phrase", label: "the exact phrase", roots: false },
  ];

  let termInput = $state<HTMLInputElement>();
  let showPalette = $state(false);

  const SCOPES: { value: ScopeChoice; label: string }[] = [
    { value: "book", label: "the whole book" },
    { value: "selection", label: "the selected verses" },
    { value: "results", label: "the current results" },
  ];

  const METHODS: { value: SimilarityMethod; label: string }[] = [
    { value: "text", label: "text" },
    { value: "words", label: "words" },
    { value: "roots", label: "word roots" },
    { value: "values", label: "word values" },
  ];

  let kind = $state<Kind>("text");
  let term = $state("");
  let wordness = $state<Wordness>("any");
  let grouping = $state<Grouping>("any");
  /** For text in other letters: search only the translations shown in the reader. */
  let onlyShown = $state(false);
  let recent = $state<HistoryEntry[]>([]);

  async function loadRecent(): Promise<void> {
    try {
      recent = await engine.history("find", 8);
    } catch {
      recent = []; // history unavailable: no suggestions, nothing else changes
    }
  }

  $effect(() => {
    void loadRecent();
  });

  // A search started elsewhere (history, the reader) fills the form it came from.
  $effect(() => {
    const request = search.request;
    if (request?.kind === "numbers" || request?.kind === "frequency") kind = request.kind;
    if (request?.kind === "text" || request?.kind === "roots") {
      kind = request.kind;
      term = request.term;
      grouping = request.grouping;
      if (request.kind === "text") wordness = request.wordness;
    }
  });

  // Changing how the text is counted changes which words match; rerun an
  // existing search rather than leave stale results on screen.
  let searchedWith: string | null = null;
  $effect(() => {
    const counting = JSON.stringify(app.counting);
    if ((search.result || search.unitResult) && searchedWith !== null && searchedWith !== counting) void search.rerun();
    searchedWith = counting;
  });

  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (!term.trim() || (kind !== "text" && kind !== "roots")) return;
    const kindGrouping = kind === "roots" && grouping === "phrase" ? "any" : grouping;
    await search.start(
      kind === "text"
        ? { kind: "text", term, wordness, grouping: kindGrouping, ...(onlyShown ? { translations: [...app.shownTranslations] } : {}) }
        : { kind: "roots", term, grouping: kindGrouping },
    );
    if (kind === "text") void loadRecent();
  }

  function refineSimilar(change: { method?: SimilarityMethod; threshold?: number }): void {
    const request = search.request;
    if (request?.kind === "similar") void search.refine({ ...request, ...change });
  }

  const similar = $derived(search.request?.kind === "similar" ? search.request : null);
  const scopeDisabled = (value: ScopeChoice): boolean =>
    (value === "selection" && !app.selection) || (value === "results" && !search.verseNumbers?.length);
</script>

<section class="search" aria-labelledby="search-title">
  <header>
    <h1 id="search-title">Search</h1>
    <div class="kinds" role="group" aria-label="What to search">
      {#each KINDS as option (option.value)}
        <button type="button" aria-pressed={kind === option.value} onclick={() => (kind = option.value)}>{option.label}</button>
      {/each}
    </div>

    <p class="hint">
      {#if kind === "numbers"}
        Find words, verses, sentences, chapters or partitions, singly, in runs or in any combination, by their counts and value.
      {:else if kind === "frequency"}
        For each unit, count how many of its letters are the phrase's letters, or ask which of them it has.
      {:else if kind === "text"}
        Matching ignores diacritics and letter forms, as the {humanize(app.currentSystem?.textMode ?? "Original")} text mode defines them.
        {#if app.counting.shaddaAsLetter || app.counting.wawAsWord}
          With {app.counting.shaddaAsLetter ? "shadda counted as a letter" : "waw counted as a word"} the searched text
          changes too, so type the word as it is then counted{app.counting.shaddaAsLetter ? ", with the doubled letter" : ""}.
        {/if}
      {:else}
        Type roots, or words whose roots you want; each word is matched to its closest root.
      {/if}
      {#if kind === "text" || kind === "roots"}Put + before a word a verse must have, and - before one it must not.{/if}
      {#if kind === "text"}Text in other letters searches the translations.{/if}
    </p>

    {#if kind === "numbers"}
      <NumbersForm />
    {:else if kind === "frequency"}
      <FrequencyForm />
    {:else}
    <form onsubmit={submit}>
      <label class="visually-hidden" for="search-term">{kind === "text" ? "Arabic text to find" : "Roots or words to find"}</label>
      <input
        id="search-term"
        class="field term arabic"
        lang="ar"
        dir="auto"
        bind:value={term}
        bind:this={termInput}
        placeholder={kind === "text" ? "اكتب كلمة" : "جذر"}
        autocomplete="off"
      />
      <button
        type="button"
        class="button palette-toggle"
        aria-expanded={showPalette}
        aria-controls="character-palette"
        title="Letters to type with, limited to those the text mode keeps"
        onclick={() => (showPalette = !showPalette)}>ا ب ت</button
      >

      {#if kind === "text"}
        <div class="wordness" role="radiogroup" aria-label="Where the text must appear">
          {#each WORDNESS as option (option.value)}
            <label class:checked={wordness === option.value}>
              <input type="radio" name="wordness" value={option.value} bind:group={wordness} />
              {option.label}
            </label>
          {/each}
        </div>
      {/if}

      <button type="submit" class="button primary" disabled={search.loading || !term.trim()}>Search</button>
    </form>
    {#if showPalette}
      <div id="character-palette">
        <CharacterPalette
          bind:value={term}
          input={termInput}
          textMode={app.currentSystem?.textMode ?? "Original"}
          roots={kind === "roots"}
        />
      </div>
    {/if}
    {/if}

    <div class="options">
      {#if kind === "text" || kind === "roots"}
        <label>
          Find
          <select class="field" bind:value={grouping}>
            {#each GROUPINGS.filter((g) => kind === "text" || g.roots) as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
          </select>
        </label>
      {/if}
      {#if kind === "text" && app.shownTranslations.length > 0}
        <label><input type="checkbox" bind:checked={onlyShown} /> only the translations shown in the reader</label>
      {/if}
      <label>
        in
        <select class="field" bind:value={search.scope}>
          {#each SCOPES as option (option.value)}<option value={option.value} disabled={scopeDisabled(option.value)}>{option.label}</option>{/each}
        </select>
      </label>
    </div>

    {#if similar}
      <div class="origin">
        <label>
          Compare by
          <select class="field" value={similar.method} onchange={(e) => refineSimilar({ method: e.currentTarget.value as SimilarityMethod })}>
            {#each METHODS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
          </select>
        </label>
        <label>
          at least <span class="num">{Math.round(similar.threshold * 100)}%</span>
          <input
            type="range"
            min="0.3"
            max="1"
            step="0.05"
            value={similar.threshold}
            onchange={(e) => refineSimilar({ threshold: Number(e.currentTarget.value) })}
          />
        </label>
      </div>
    {/if}

    {#if recent.length > 0}
      <p class="recent">
        <span>Recent</span>
        {#each recent as entry (entry.id)}
          <button
            type="button"
            class="chip arabic"
            lang="ar"
            dir="rtl"
            onclick={() => search.start({ kind: "text", term: entry.term ?? "", wordness: entry.wordness ?? "any", grouping: "any" })}
            >{entry.term}</button
          >
        {/each}
      </p>
    {/if}
  </header>

  <SearchResults />
</section>

<style>
  .search {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
    max-height: 60vh;
    overflow-y: auto;
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
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  form {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
  }

  .palette-toggle {
    font-family: var(--font-arabic);
  }

  #character-palette {
    max-width: 36rem;
    margin-top: var(--space-3);
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

  .kinds {
    display: flex;
    gap: var(--space-1);
    margin-bottom: var(--space-3);
  }

  .kinds button {
    padding: 0.3rem 0.8rem;
    border: 1px solid transparent;
    border-radius: var(--radius-md);
    background: none;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .kinds button[aria-pressed="true"] {
    border-color: var(--rule-strong);
    background: var(--surface-sunk);
    color: var(--ink);
    font-weight: 550;
  }

  .options,
  .origin {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
    margin: var(--space-3) 0 0;
    font-size: var(--text-sm);
  }

  .options select,
  .origin select {
    height: 2rem;
    font-size: var(--text-sm);
  }

  .origin input[type="range"] {
    width: 8rem;
    vertical-align: middle;
  }

  .recent {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-2);
    margin: var(--space-3) 0 0;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .chip {
    padding: 0 var(--space-2);
    border: 1px solid var(--rule-strong);
    border-radius: 999px;
    background: var(--surface);
    font-size: 1rem;
    line-height: 1.7;
  }

  .chip:hover {
    border-color: var(--lapis);
  }
</style>
