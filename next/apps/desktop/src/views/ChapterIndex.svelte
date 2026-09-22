<script lang="ts">
  import { app } from "../lib/state/app.svelte";

  let filter = $state("");

  // Matches chapter number, Arabic name, transliteration or English name.
  const chapters = $derived.by(() => {
    const query = filter.trim().toLowerCase();
    if (!query) return app.chapters;
    return app.chapters.filter(
      (c) =>
        String(c.number) === query ||
        c.name.includes(query) ||
        c.transliteratedName.toLowerCase().includes(query) ||
        c.englishName.toLowerCase().includes(query),
    );
  });
</script>

<nav class="index" aria-label="Chapters">
  <div class="filter">
    <label class="visually-hidden" for="chapter-filter">Filter chapters</label>
    <input id="chapter-filter" class="field" bind:value={filter} placeholder="Filter" autocomplete="off" />
  </div>
  <ol>
    {#each chapters as chapter (chapter.number)}
      <li>
        <button
          type="button"
          class:current={chapter.number === app.chapter}
          aria-current={chapter.number === app.chapter ? "page" : undefined}
          title="{chapter.number} {chapter.transliteratedName}, {chapter.englishName}"
          onclick={() => app.openChapter(chapter.number)}
        >
          <span class="number num">{chapter.number}</span>
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
</nav>

<style>
  .index {
    display: grid;
    grid-template-rows: auto 1fr;
    min-height: 0;
    background: var(--surface);
    border-inline-end: 1px solid var(--rule);
  }

  .filter {
    padding: var(--space-3);
    border-bottom: 1px solid var(--rule);
  }

  .filter input {
    width: 100%;
    height: 2rem;
    font-size: var(--text-sm);
  }

  ol {
    margin: 0;
    padding: var(--space-1) 0;
    list-style: none;
    overflow-y: auto;
  }

  button {
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

  button:hover {
    background: var(--paper);
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

  @media (max-width: 1200px) {
    button {
      grid-template-columns: 1.5rem 1fr;
    }

    .names {
      display: none;
    }

    .name {
      text-align: end;
    }

    .filter {
      padding: var(--space-2);
    }
  }

  .empty {
    padding: var(--space-4) var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }
</style>
