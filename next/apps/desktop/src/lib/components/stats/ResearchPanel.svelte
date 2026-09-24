<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { AllahSummary, ResearchMethod, ResearchTable, Scope } from "../../engine/types";
  import { app } from "../../state/app.svelte";
  import Notice from "../Notice.svelte";

  // The original's research word lists (Features.txt #3, #10), over the
  // selection or the whole book, and its Allah statistics. The original saves
  // each list to a file; here a list can be copied as tab-separated text.

  interface Props {
    scope: Scope;
  }

  let { scope }: Props = $props();

  const PAGE = 100;
  const METHODS: { value: ResearchMethod; label: string }[] = [
    { value: "allah", label: "Allah words (the 12 forms)" },
    { value: "nonAllah", label: "Words like Allah that are not" },
    { value: "all", label: "All words" },
    { value: "double", label: "Double words (next to each other)" },
    { value: "repeated", label: "Repeated words within a verse" },
  ];

  let method = $state<ResearchMethod>("allah");
  let wholeBook = $state(true);
  let gap = $state(0);
  let table = $state<ResearchTable | null>(null);
  let offset = $state(0);
  let error = $state<string | null>(null);
  let summary = $state<AllahSummary | null>(null);
  let copied = $state<string | null>(null);

  const load = latest(engine.researchWords);
  const loadSummary = latest(engine.selectionAllah);

  $effect(() => {
    const r = wholeBook ? null : $state.snapshot(scope);
    const request = { method, range: r, gap, valueSystem: app.valueSystem, counting: { ...app.counting }, offset, limit: PAGE };
    load(request)
      .then(({ current, value }) => {
        if (!current) return;
        table = value;
        error = null;
      })
      .catch((e: unknown) => (error = describeError(e)));
  });

  $effect(() => {
    loadSummary($state.snapshot(scope), app.valueSystem, { ...app.counting })
      .then(({ current, value }) => {
        if (current) summary = value;
      })
      .catch(() => (summary = null)); // the list below reports errors; the summary line just hides
  });

  // A new list starts from its first page.
  $effect(() => {
    void method;
    void wholeBook;
    void gap;
    offset = 0;
  });

  async function copy(): Promise<void> {
    copied = null;
    try {
      const whole = await engine.researchWords({
        method, range: wholeBook ? null : $state.snapshot(scope), gap, valueSystem: app.valueSystem, counting: { ...app.counting }, tsv: true,
      });
      await navigator.clipboard.writeText(whole.tsv ?? "");
      copied = `Copied ${whole.rowCount} rows.`;
    } catch (e) {
      copied = `Could not copy: ${describeError(e)}`;
    }
  }
</script>

{#if summary}
  <p class="allah">
    In the selection: <span class="arabic" lang="ar">الله</span> <strong class="num">{summary.allah}</strong> ·
    words with <span class="arabic" lang="ar">الله</span> <strong class="num">{summary.withAllah}</strong> ·
    words with <span class="arabic" lang="ar">لله</span> <strong class="num">{summary.withLillah}</strong> ·
    total <strong class="num">{summary.total}</strong>
  </p>
{/if}

<div class="bar">
  <select class="field" bind:value={method} aria-label="Which list">
    {#each METHODS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
  </select>
  {#if method === "repeated"}
    <label>words between <input class="field gap num" type="number" min="0" max="100" bind:value={gap} /></label>
  {/if}
  <label><input type="checkbox" bind:checked={wholeBook} /> whole book</label>
  <button type="button" class="button" disabled={!table?.rowCount} onclick={copy}>Copy as tabbed text</button>
  {#if copied}<span class="hint">{copied}</span>{/if}
</div>

{#if error}
  <Notice tone="error" title="The list could not be made" detail={error} />
{:else if table}
  <p class="hint"><span class="num">{table.rowCount}</span> rows. Order and Total count the same word across the whole book.</p>
  <div class="scroll">
    <table>
      <thead><tr>{#each table.columns as column (column)}<th>{column}</th>{/each}</tr></thead>
      <tbody>
        {#each table.rows as row, i (i)}
          <tr>
            {#each row as cell, j (j)}
              <td class:arabic={/[؀-ۿ]/.test(cell)} lang={/[؀-ۿ]/.test(cell) ? "ar" : undefined}>{cell}</td>
            {/each}
          </tr>
        {/each}
      </tbody>
    </table>
  </div>
  {#if table.rowCount > PAGE}
    <div class="pages">
      <button type="button" class="button" disabled={offset === 0} onclick={() => (offset = Math.max(0, offset - PAGE))}>Previous</button>
      <span class="num">{offset + 1}–{Math.min(offset + PAGE, table.rowCount)}</span>
      <button type="button" class="button" disabled={offset + PAGE >= table.rowCount} onclick={() => (offset += PAGE)}>Next</button>
    </div>
  {/if}
{:else}
  <Notice tone="loading" title="Making the list" />
{/if}

<style>
  .allah {
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
  }

  .allah .arabic {
    font-size: 1.1rem;
  }

  .bar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-4);
    margin-bottom: var(--space-3);
    font-size: var(--text-sm);
  }

  .bar .field {
    height: 2rem;
    font-size: var(--text-sm);
  }

  .gap {
    width: 4rem;
  }

  .hint {
    margin: 0 0 var(--space-2);
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .scroll {
    overflow-x: auto;
  }

  table {
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  th,
  td {
    padding: 0.2rem 0.9rem 0.2rem 0;
    text-align: end;
    border-bottom: 1px solid var(--rule);
    white-space: nowrap;
  }

  th {
    font-size: var(--text-xs);
    font-weight: 550;
    color: var(--ink-muted);
  }

  td.arabic {
    font-size: 1.1rem;
  }

  .pages {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-top: var(--space-3);
    font-size: var(--text-sm);
  }
</style>
