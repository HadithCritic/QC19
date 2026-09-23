<script lang="ts">
  import DisplaySettings from "../lib/components/DisplaySettings.svelte";
  import LettersPanel from "../lib/components/stats/LettersPanel.svelte";
  import MathsPanel from "../lib/components/stats/MathsPanel.svelte";
  import MultiplesPanel from "../lib/components/stats/MultiplesPanel.svelte";
  import ResearchPanel from "../lib/components/stats/ResearchPanel.svelte";
  import SymmetryPanel from "../lib/components/stats/SymmetryPanel.svelte";
  import WordsPanel from "../lib/components/stats/WordsPanel.svelte";
  import type { VerseRange } from "../lib/engine/types";
  import { app } from "../lib/state/app.svelte";

  // Lists and sums over the selection, or the open chapter when nothing is
  // selected: the panels the original keeps beside its text.

  type Tab = "multiples" | "words" | "letters" | "maths" | "symmetry" | "research";
  const TABS: { value: Tab; label: string }[] = [
    { value: "multiples", label: "Multiples" },
    { value: "words", label: "Words" },
    { value: "letters", label: "Letters" },
    { value: "maths", label: "Maths" },
    { value: "symmetry", label: "Symmetry" },
    { value: "research", label: "Research lists" },
  ];

  let tab = $state<Tab>("multiples");

  const chapter = $derived(app.chapters[app.chapter - 1]);
  const range = $derived<VerseRange | null>(
    app.selection ?? (chapter ? { first: chapter.firstVerse, last: chapter.firstVerse + chapter.verseCount - (chapter.hasVerseZero ? 0 : 1) } : null),
  );

  function describeRange(r: VerseRange): string {
    const a = app.chapterOf(r.first);
    const b = app.chapterOf(r.last);
    if (!a || !b) return "";
    const va = r.first - a.firstVerse + (a.hasVerseZero ? 0 : 1);
    const vb = r.last - b.firstVerse + (b.hasVerseZero ? 0 : 1);
    if (r.first === r.last) return `${a.number}:${va}`;
    return a.number === b.number ? `${a.number}:${va}-${vb}` : `${a.number}:${va}-${b.number}:${vb}`;
  }
</script>

<section class="statistics" aria-labelledby="stats-title">
  <header>
    <h1 id="stats-title">Statistics</h1>
    <p class="hint">
      {#if range}
        {app.selection ? "The selection" : `Chapter ${chapter?.number}, since nothing is selected`}:
        <span class="num">{describeRange(range)}</span>
      {/if}
    </p>
    <div class="tabs" role="tablist" aria-label="Statistics">
      {#each TABS as option (option.value)}
        <button type="button" role="tab" aria-selected={tab === option.value} onclick={() => (tab = option.value)}>{option.label}</button>
      {/each}
    </div>
    <DisplaySettings />
  </header>

  <div class="body" role="tabpanel">
    {#if range}
      {#if tab === "multiples"}<MultiplesPanel {range} />
      {:else if tab === "words"}<WordsPanel {range} />
      {:else if tab === "letters"}<LettersPanel {range} />
      {:else if tab === "maths"}<MathsPanel {range} />
      {:else if tab === "symmetry"}<SymmetryPanel {range} />
      {:else}<ResearchPanel {range} />{/if}
    {/if}
  </div>
</section>

<style>
  .statistics {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
    display: grid;
    gap: var(--space-3);
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
    margin: 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .tabs {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-1);
  }

  .tabs button {
    padding: 0.3rem 0.8rem;
    border: 1px solid transparent;
    border-radius: var(--radius-md);
    background: none;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .tabs button[aria-selected="true"] {
    border-color: var(--rule-strong);
    background: var(--surface-sunk);
    color: var(--ink);
    font-weight: 550;
  }

  .body {
    overflow-y: auto;
    padding: var(--space-4) var(--space-6) var(--space-7);
  }
</style>
