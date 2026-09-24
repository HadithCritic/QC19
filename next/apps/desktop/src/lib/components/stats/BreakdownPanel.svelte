<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { Breakdown, BreakdownRow, BreakdownUnit, QuranSelection, Scope } from "../../engine/types";
  import { verseSelection } from "../../selection";
  import { app } from "../../state/app.svelte";
  import NumberChip from "../NumberChip.svelte";

  // The selection verse by verse, word by word or letter by letter, each row
  // with its counted letters and value in the current system. Rows add up to
  // the selection, so a claimed total can be checked part by part. Clicking a
  // row selects that verse, word or letter in the reader.

  interface Props {
    scope: Scope;
  }

  let { scope }: Props = $props();

  const PAGE = 100;
  const UNITS: { value: BreakdownUnit; label: string }[] = [
    { value: "verse", label: "Verse" },
    { value: "word", label: "Word" },
    { value: "letter", label: "Letter" },
  ];

  let by = $state<BreakdownUnit>("word");
  let offset = $state(0);
  let breakdown = $state<Breakdown | null>(null);
  let error = $state<string | null>(null);
  const load = latest(engine.breakdown);

  const selection = $derived<QuranSelection | null>("selection" in scope ? scope.selection : verseSelection(scope, app.chapters));

  // A new selection or unit starts from the first page.
  $effect(() => {
    void selection;
    void by;
    offset = 0;
  });

  $effect(() => {
    const s = $state.snapshot(selection);
    if (!s) return;
    load(s, by, app.valueSystem, { ...app.counting }, offset, PAGE)
      .then(({ current, value }) => {
        if (!current) return;
        breakdown = value;
        error = null;
      })
      .catch((e: unknown) => {
        breakdown = null;
        error = describeError(e);
      });
  });

  function open(row: BreakdownRow): void {
    app.setExact({ start: row.location, end: row.location });
  }
</script>

<div class="controls">
  <span class="eyebrow">Break down by</span>
  {#each UNITS as unit (unit.value)}
    <label><input type="radio" bind:group={by} value={unit.value} /> {unit.label}</label>
  {/each}
</div>

{#if error}
  <p class="error" role="alert">{error}</p>
{:else if breakdown}
  <p class="hint">
    <span class="num">{breakdown.rowCount}</span> rows. Values are in the current system; a row is valued as its run of letters, so the rows add up to
    the selection. Click a row to select it in the reader.
  </p>
  <table>
    <thead>
      <tr>
        <th scope="col">Address</th>
        <th scope="col">Text</th>
        <th scope="col" class="n">Letters</th>
        <th scope="col" class="n">Value</th>
      </tr>
    </thead>
    <tbody>
      {#each breakdown.rows as row (row.address + row.text)}
        <tr onclick={() => open(row)}>
          <td class="num"><button type="button" class="link" onclick={(e) => (e.stopPropagation(), open(row))}>{row.address}</button></td>
          <td class="arabic" lang="ar" dir="rtl">{row.text}</td>
          <td class="n num">{row.letters}</td>
          <td class="n"><NumberChip value={row.value} size="sm" /></td>
        </tr>
      {/each}
    </tbody>
  </table>
  {#if breakdown.rowCount > PAGE}
    <div class="pages">
      <button type="button" disabled={offset === 0} onclick={() => (offset = Math.max(0, offset - PAGE))}>Previous</button>
      <span class="num">{offset + 1}–{Math.min(offset + PAGE, breakdown.rowCount)} of {breakdown.rowCount}</span>
      <button type="button" disabled={offset + PAGE >= breakdown.rowCount} onclick={() => (offset += PAGE)}>Next</button>
    </div>
  {/if}
{/if}

<style>
  .controls {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
    margin-bottom: var(--space-3);
    font-size: var(--text-sm);
  }

  .hint {
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .error {
    color: var(--danger);
    font-size: var(--text-sm);
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  th {
    padding: var(--space-2);
    border-bottom: 1px solid var(--rule-strong);
    color: var(--ink-muted);
    font-weight: 550;
    text-align: start;
  }

  td {
    padding: var(--space-1) var(--space-2);
    border-bottom: 1px solid var(--rule);
  }

  tbody tr {
    cursor: pointer;
  }

  tbody tr:hover {
    background: var(--surface-sunk);
  }

  .n {
    text-align: end;
  }

  .arabic {
    font-family: var(--font-quran);
    font-size: 1.2rem;
  }

  .link {
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font: inherit;
  }

  .pages {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-3);
    margin-top: var(--space-3);
    font-size: var(--text-sm);
  }
</style>
