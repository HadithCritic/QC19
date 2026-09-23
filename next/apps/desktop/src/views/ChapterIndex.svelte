<script lang="ts">
  import NumberChip from "../lib/components/NumberChip.svelte";
  import { PRESETS, TESTS, countedVerses, selectChapters, type ChapterPreset, type NumberTest } from "../lib/chapterClasses";
  import { CHAPTER_SORTS, sortChapters, type ChapterSort } from "../lib/chapterSort";
  import { engine, latest } from "../lib/engine/client";
  import type { Chapter, ChapterStats } from "../lib/engine/types";
  import { shade } from "../lib/searchRequest";
  import { app } from "../lib/state/app.svelte";
  import { search } from "../lib/state/search.svelte";

  let filter = $state("");
  let sort = $state<ChapterSort>("number");
  let descending = $state(false);
  let stats = $state<Map<number, ChapterStats>>(new Map());
  let hovered = $state<{ chapter: Chapter; top: number } | null>(null);
  let preset = $state<ChapterPreset>("all");
  let numberTest = $state<NumberTest>("any");
  let versesTest = $state<NumberTest>("any");
  const classFiltered = $derived(preset !== "all" || numberTest !== "any" || versesTest !== "any");

  const load = latest(engine.chapterStats);

  // Words, letters and values depend on the system and the counting options.
  $effect(() => {
    const system = app.valueSystem;
    const counting = { ...app.counting };
    if (!system) return;
    load(system, counting)
      .then(({ current, value }) => {
        if (current) stats = new Map(value.map((s) => [s.chapter, s]));
      })
      // Sorting then falls back to what the chapter list itself knows.
      .catch(() => (stats = new Map()));
  });

  // Matches chapter number, Arabic name, transliteration or English name.
  const chapters = $derived.by(() => {
    const query = filter.trim().toLowerCase();
    const classes = selectChapters(app.chapters, preset, numberTest, versesTest, !app.verseZeroExcluded);
    const matching = query
      ? classes.filter(
          (c) =>
            String(c.number) === query ||
            c.name.includes(query) ||
            c.transliteratedName.toLowerCase().includes(query) ||
            c.englishName.toLowerCase().includes(query),
        )
      : classes;
    return sortChapters(matching, stats, sort, descending);
  });

  const INITIAL_TITLES: Record<Chapter["initialization"], string | undefined> = {
    key: "The key chapter",
    full: "Opens with initials on their own",
    partial: "Opens with initials within a verse",
    double: "Opens with initials in two verses",
    none: undefined,
  };

  function show(chapter: Chapter, event: Event): void {
    const item = (event.currentTarget as HTMLElement).getBoundingClientRect();
    hovered = { chapter, top: item.top };
  }

  const details = $derived(hovered ? stats.get(hovered.chapter.number) : undefined);

  // While search results are shown, each chapter is shaded by its matches
  // (Features.txt #15), darker with more.
  const matches = $derived(search.chapterCounts);
  const matchingChapters = $derived(matches ? matches.filter((n) => n > 0).length : 0);

  function matchTitle(count: number): string | undefined {
    if (count <= 0) return undefined;
    return count === 1 ? "1 match" : `${count} matches`;
  }
</script>

<nav class="index" aria-label="Chapters">
  <div class="tools">
    <label class="visually-hidden" for="chapter-filter">Filter chapters</label>
    <input id="chapter-filter" class="field" bind:value={filter} placeholder="Filter" autocomplete="off" />
    <div class="sort">
      <label class="visually-hidden" for="chapter-sort">Sort chapters by</label>
      <select id="chapter-sort" class="field" bind:value={sort}>
        {#each CHAPTER_SORTS as option (option.id)}<option value={option.id}>{option.label}</option>{/each}
      </select>
      <button
        type="button"
        class="field direction"
        aria-label={descending ? "Descending; sort ascending" : "Ascending; sort descending"}
        title={descending ? "Descending" : "Ascending"}
        onclick={() => (descending = !descending)}>{descending ? "↓" : "↑"}</button
      >
    </div>
    <details class="classes" open={classFiltered}>
      <summary>Chapter classes</summary>
      <label class="visually-hidden" for="chapter-preset">Which chapters</label>
      <select id="chapter-preset" class="field" bind:value={preset}>
        {#each PRESETS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
      </select>
      <label>
        number
        <select class="field" bind:value={numberTest}>
          {#each TESTS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
        </select>
      </label>
      <label>
        verses
        <select class="field" bind:value={versesTest}>
          {#each TESTS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
        </select>
      </label>
      {#if classFiltered}
        <p class="matching">
          <span class="num">{chapters.length}</span> chapters,
          <span class="num">{chapters.reduce((sum, c) => sum + countedVerses(c, !app.verseZeroExcluded), 0)}</span> verses,
          numbers adding to <span class="num">{chapters.reduce((sum, c) => sum + c.number, 0)}</span>
          <button type="button" class="reset" onclick={() => ((preset = "all"), (numberTest = "any"), (versesTest = "any"))}>Show all</button>
        </p>
      {/if}
    </details>
    {#if matches}
      <p class="matching">Matches in <span class="num">{matchingChapters}</span> chapters</p>
    {/if}
  </div>
  <ol onmouseleave={() => (hovered = null)}>
    {#each chapters as chapter (chapter.number)}
      <li>
        <button
          type="button"
          class:current={chapter.number === app.chapter}
          aria-current={chapter.number === app.chapter ? "page" : undefined}
          style:--match={matches ? `${Math.round(10 + shade(matches[chapter.number - 1] ?? 0) * 50)}%` : null}
          class:matched={(matches?.[chapter.number - 1] ?? 0) > 0}
          title={matchTitle(matches?.[chapter.number - 1] ?? 0)}
          onclick={() => app.openChapter(chapter.number)}
          onmouseenter={(e) => show(chapter, e)}
          onfocus={(e) => show(chapter, e)}
          onblur={() => (hovered = null)}
        >
          <span class="number num" data-initials={chapter.initialization} title={INITIAL_TITLES[chapter.initialization]}>{chapter.number}</span>
          <span class="names">
            <span class="latin">{chapter.transliteratedName}</span>
            <span class="english">{chapter.englishName}</span>
          </span>
          <span class="arabic name" lang="ar" dir="rtl">{chapter.name}</span>
        </button>
      </li>
    {:else}
      <li class="empty">No chapter matches “{filter}”.</li>
    {/each}
  </ol>

  {#if hovered}
    <div class="card" role="tooltip" style:top="{hovered.top}px">
      <p class="card-title"><span class="num">{hovered.chapter.number}</span> {hovered.chapter.transliteratedName}
        <span class="arabic" lang="ar" dir="rtl">{hovered.chapter.name}</span></p>
      <p class="card-sub">{hovered.chapter.englishName} · {hovered.chapter.revelationPlace} · revelation order <span class="num">{hovered.chapter.revelationOrder}</span></p>
      {#if details}
        <dl>
          <div><dt>Verses</dt><dd class="num">{details.verses}</dd></div>
          <div><dt>Words</dt><dd class="num">{details.words}</dd></div>
          <div><dt>Letters</dt><dd class="num">{details.letters}</dd></div>
          <div><dt>Value</dt><dd><NumberChip value={details.value} code={details.code} size="sm" /></dd></div>
        </dl>
      {/if}
    </div>
  {/if}
</nav>

<style>
  .index {
    position: relative;
    display: grid;
    grid-template-rows: auto 1fr;
    min-height: 0;
    background: var(--surface);
    border-inline-end: 1px solid var(--rule);
  }

  .tools {
    display: grid;
    gap: var(--space-2);
    padding: var(--space-3);
    border-bottom: 1px solid var(--rule);
  }

  .tools input,
  .tools select,
  .direction {
    height: 2rem;
    font-size: var(--text-sm);
  }

  .tools input {
    width: 100%;
  }

  .sort {
    display: grid;
    grid-template-columns: 1fr auto;
    gap: var(--space-1);
  }

  .direction {
    width: 2rem;
    padding: 0;
  }

  ol {
    margin: 0;
    padding: var(--space-1) 0;
    list-style: none;
    overflow-y: auto;
  }

  button:not(.direction) {
    display: grid;
    grid-template-columns: 2rem 1fr auto;
    align-items: center;
    gap: var(--space-2);
    width: 100%;
    padding: var(--space-2) var(--space-3);
    border: 0;
    background: none;
    text-align: start;
    border-radius: 0;
  }

  button:not(.direction):hover {
    background: var(--paper);
  }

  button.matched {
    background: color-mix(in srgb, var(--gilt) var(--match), transparent);
  }

  .classes {
    display: grid;
    gap: var(--space-1);
    font-size: var(--text-xs);
  }

  .classes summary {
    color: var(--ink-muted);
    cursor: pointer;
  }

  .classes .field {
    width: 100%;
    height: 1.75rem;
    font-size: var(--text-xs);
  }

  .classes label {
    display: grid;
    grid-template-columns: 3.2rem 1fr;
    align-items: center;
    color: var(--ink-muted);
  }

  .reset {
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font-size: var(--text-xs);
  }

  /* Chapters that open with initials carry a gilt mark under their number,
     stronger from partial to full to double (the original shades the list). */
  .number[data-initials="partial"],
  .number[data-initials="full"],
  .number[data-initials="double"],
  .number[data-initials="key"] {
    text-decoration: underline 2px;
    text-underline-offset: 0.3em;
  }

  .number[data-initials="partial"] {
    text-decoration-color: color-mix(in srgb, var(--gilt) 45%, transparent);
  }

  .number[data-initials="full"] {
    text-decoration-color: var(--gilt);
  }

  .number[data-initials="double"] {
    text-decoration-style: double;
    text-decoration-color: var(--gilt);
  }

  .number[data-initials="key"] {
    text-decoration-color: var(--ink);
  }

  .matching {
    margin: 0;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  button.current {
    background: var(--lapis-soft);
    box-shadow: inset 3px 0 0 var(--lapis);
  }

  .number {
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  .current .number {
    color: var(--lapis);
  }

  .names {
    display: grid;
    min-width: 0;
  }

  .latin {
    font-size: var(--text-sm);
    font-weight: 550;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .english {
    font-size: var(--text-xs);
    color: var(--ink-muted);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .name {
    font-size: 1.1rem;
    color: var(--ink-muted);
  }

  .card {
    position: fixed;
    left: calc(var(--rail-width) + var(--index-width) + 6px);
    z-index: 25;
    width: 16rem;
    padding: var(--space-3) var(--space-4);
    background: var(--surface);
    border: 1px solid var(--rule);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-pop);
    pointer-events: none;
  }

  .card-title {
    display: flex;
    justify-content: space-between;
    gap: var(--space-2);
    margin: 0;
    font-weight: 600;
  }

  .card-title .arabic {
    font-size: 1.2rem;
    font-weight: 400;
  }

  .card-sub {
    margin: 0 0 var(--space-2);
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .card dl {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-2);
    margin: 0;
  }

  .card dt {
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .card dd {
    margin: 0;
    font-size: var(--text-sm);
  }

  @media (max-width: 1200px) {
    button:not(.direction) {
      grid-template-columns: 1.5rem 1fr;
    }

    .names {
      display: none;
    }

    .name {
      text-align: end;
    }

    .tools {
      padding: var(--space-2);
    }

    .card {
      left: calc(var(--rail-width) + 6.5rem + 6px);
    }
  }

  .empty {
    padding: var(--space-4) var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }
</style>
