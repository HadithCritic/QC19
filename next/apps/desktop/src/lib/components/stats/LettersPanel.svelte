<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { LetterScope, LetterStatistic, VerseRange } from "../../engine/types";
  import { app } from "../../state/app.svelte";
  import Notice from "../Notice.svelte";
  import NumberChip from "../NumberChip.svelte";

  // Letter statistics of the selection (Features.txt #22): each letter's
  // frequency, the sum of its positions and of the distances between its
  // occurrences. Choose letters for their totals; open any total to factor it.

  interface Props {
    range: VerseRange;
  }

  let { range }: Props = $props();

  type Column = "order" | "letter" | "count" | "positionSum" | "distanceSum";
  const COLUMNS: { key: Column; label: string; title: string }[] = [
    { key: "order", label: "#", title: "Order of first appearance" },
    { key: "letter", label: "Letter", title: "Letter" },
    { key: "count", label: "Frequency", title: "How many times" },
    { key: "positionSum", label: "Σ position", title: "Sum of its positions" },
    { key: "distanceSum", label: "Σ distance", title: "Sum of the distances to the previous occurrence" },
  ];
  const SCOPES: { value: LetterScope; label: string }[] = [
    { value: "book", label: "in the book" },
    { value: "chapter", label: "in the chapter" },
    { value: "verse", label: "in the verse" },
    { value: "word", label: "in the word" },
  ];

  let scope = $state<LetterScope>("book");
  let letters = $state<LetterStatistic[] | null>(null);
  let error = $state<string | null>(null);
  let sortBy = $state<Column>("order");
  let descending = $state(false);
  let chosen = $state<Set<string>>(new Set());

  const load = latest(engine.selectionLetters);

  $effect(() => {
    const r = { ...range };
    const s = scope;
    load(r, app.valueSystem, { ...app.counting }, s)
      .then(({ current, value }) => {
        if (!current) return;
        letters = value;
        error = null;
      })
      .catch((e: unknown) => (error = describeError(e)));
  });

  // Ties are broken by first appearance, in the same direction, as the original does.
  const rows = $derived.by(() => {
    if (!letters) return [];
    const sign = descending ? -1 : 1;
    return [...letters].sort((a, b) => {
      const primary = sortBy === "letter" ? a.letter.localeCompare(b.letter, "ar") : (a[sortBy] as number) - (b[sortBy] as number);
      return sign * (primary !== 0 ? primary : a.order - b.order);
    });
  });

  const picked = $derived(letters?.filter((l) => chosen.size === 0 || chosen.has(l.letter)) ?? []);
  const totals = $derived({
    letters: picked.length,
    count: picked.reduce((s, l) => s + l.count, 0),
    positionSum: picked.reduce((s, l) => s + l.positionSum, 0),
    distanceSum: picked.reduce((s, l) => s + l.distanceSum, 0),
  });

  function sortOn(column: Column): void {
    if (sortBy === column) descending = !descending;
    else {
      sortBy = column;
      descending = column !== "order" && column !== "letter";
    }
  }

  function toggle(letter: string): void {
    const next = new Set(chosen);
    if (next.has(letter)) next.delete(letter);
    else next.add(letter);
    chosen = next;
  }
</script>

{#if error}
  <Notice tone="error" title="The letter statistics could not be made" detail={error} />
{:else if letters}
  <div class="bar">
    <label>
      Positions counted
      <select class="field" bind:value={scope}>
        {#each SCOPES as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
      </select>
    </label>
    <span class="totals">
      {chosen.size ? "Chosen" : "All"}:
      <NumberChip value={String(totals.letters)} size="sm" onselect={(v) => app.openNumber(v)} /> letters,
      <NumberChip value={String(totals.count)} size="sm" onselect={(v) => app.openNumber(v)} /> in all,
      Σ position <NumberChip value={String(totals.positionSum)} size="sm" onselect={(v) => app.openNumber(v)} />,
      Σ distance <NumberChip value={String(totals.distanceSum)} size="sm" onselect={(v) => app.openNumber(v)} />
    </span>
    {#if chosen.size}<button type="button" class="link" onclick={() => (chosen = new Set())}>Clear choice</button>{/if}
  </div>
  <table>
    <thead>
      <tr>
        <th class="pick"><span class="visually-hidden">Choose</span></th>
        {#each COLUMNS as column (column.key)}
          <th aria-sort={sortBy === column.key ? (descending ? "descending" : "ascending") : "none"}>
            <button type="button" title={column.title} onclick={() => sortOn(column.key)}>
              {column.label}{#if sortBy === column.key}{descending ? " ▼" : " ▲"}{/if}
            </button>
          </th>
        {/each}
      </tr>
    </thead>
    <tbody>
      {#each rows as row (row.letter)}
        <tr class:chosen={chosen.has(row.letter)}>
          <td class="pick"><input type="checkbox" aria-label="Choose {row.letter}" checked={chosen.has(row.letter)} onchange={() => toggle(row.letter)} /></td>
          <td class="num">{row.order}</td>
          <td class="letter arabic" lang="ar">{row.letter}</td>
          <td class="num">{row.count}</td>
          <td class="num">{row.positionSum}</td>
          <td class="num">{row.distanceSum}</td>
        </tr>
      {/each}
    </tbody>
  </table>
{:else}
  <Notice tone="loading" title="Counting letters" />
{/if}

<style>
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

  table {
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  th,
  td {
    padding: 0.2rem 0.9rem 0.2rem 0;
    text-align: end;
    border-bottom: 1px solid var(--rule);
  }

  th button {
    padding: 0;
    border: 0;
    background: none;
    color: var(--ink-muted);
    font-size: var(--text-xs);
    font-weight: 550;
  }

  .pick {
    text-align: start;
  }

  .letter {
    font-size: 1.25rem;
    text-align: center;
  }

  tr.chosen td {
    background: var(--lapis-soft);
  }

  .link {
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font-size: var(--text-xs);
    font-weight: 550;
  }
</style>
