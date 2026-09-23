<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { CvSums, Maths, QuantitySums, VerseRange } from "../../engine/types";
  import { toRadix } from "../../numberDisplay";
  import { app } from "../../state/app.svelte";
  import Notice from "../Notice.svelte";

  // The original's Maths tab (Features.txt #12, #13): for the chapters the
  // selection touches (C = chapter number, V = its verses) and for its verses
  // (C = chapter, V = verse number), the sums of C, V and their combinations,
  // split into odd, even, prime and composite, with the ratio d/u of repeated
  // to single values.

  interface Props {
    range: VerseRange;
  }

  let { range }: Props = $props();

  let absoluteDifference = $state(false);
  let vOverC = $state(false);
  let maths = $state<Maths | null>(null);
  let error = $state<string | null>(null);
  const load = latest(engine.selectionMaths);

  $effect(() => {
    const r = { ...range };
    load(r, { ...app.counting }, absoluteDifference, vOverC)
      .then(({ current, value }) => {
        if (!current) return;
        maths = value;
        error = null;
      })
      .catch((e: unknown) => (error = describeError(e)));
  });

  const ROWS: { key: keyof Omit<CvSums, "count">; label: () => string }[] = [
    { key: "c", label: () => "C" },
    { key: "v", label: () => "V" },
    { key: "plus", label: () => "C + V" },
    { key: "minus", label: () => (absoluteDifference ? "|C − V|" : "C − V") },
    { key: "times", label: () => "C × V" },
    { key: "divided", label: () => (vOverC ? "V ÷ C" : "C ÷ V") },
  ];

  function shown(value: number, divided: boolean): string {
    if (divided) return value.toFixed(1);
    return toRadix(String(Math.round(value)), app.radix);
  }

  function marked(value: number, divided: boolean): boolean {
    const whole = Math.round(value);
    return !divided && whole !== 0 && whole % app.divisor === 0;
  }

  const COLUMNS: { key: keyof Omit<QuantitySums, "ratio">; label: string }[] = [
    { key: "sum", label: "Σ" },
    { key: "odd", label: "odd" },
    { key: "even", label: "even" },
    { key: "prime", label: "prime" },
    { key: "composite", label: "composite" },
  ];
</script>

{#snippet table(title: string, sums: CvSums)}
  <section>
    <h3 class="eyebrow">{title} <span class="count num">({sums.count})</span></h3>
    <table>
      <thead>
        <tr>
          <th></th>
          {#each COLUMNS as column (column.key)}<th>{column.label}</th>{/each}
          <th title="Repeated values times how often, over values that occur once">d/u</th>
        </tr>
      </thead>
      <tbody>
        {#each ROWS as row (row.key)}
          <tr>
            <th scope="row">{row.label()}</th>
            {#each COLUMNS as column (column.key)}
              <td class="num" class:divisible={marked(sums[row.key][column.key], row.key === "divided")}>
                {shown(sums[row.key][column.key], row.key === "divided")}
              </td>
            {/each}
            <td class="num">{sums[row.key].ratio === null ? "" : sums[row.key].ratio!.toFixed(5)}</td>
          </tr>
        {/each}
      </tbody>
    </table>
  </section>
{/snippet}

{#if error}
  <Notice tone="error" title="The sums could not be made" detail={error} />
{:else if maths}
  <div class="bar">
    <label><input type="checkbox" bind:checked={absoluteDifference} /> |C − V|</label>
    <label><input type="checkbox" bind:checked={vOverC} /> V ÷ C</label>
    <span class="hint">1 is neither prime nor composite; ÷ values are tested by their whole part.</span>
  </div>
  {@render table("Chapters: C = number, V = verses", maths.chapters)}
  {@render table("Verses: C = chapter, V = verse number", maths.verses)}
{:else}
  <Notice tone="loading" title="Adding up" />
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

  .hint {
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  section {
    margin-bottom: var(--space-5);
  }

  h3 {
    margin: 0 0 var(--space-2);
  }

  .count {
    font-weight: 400;
    color: var(--ink-faint);
  }

  table {
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  th,
  td {
    padding: 0.25rem 1rem 0.25rem 0;
    text-align: end;
    border-bottom: 1px solid var(--rule);
  }

  thead th {
    font-size: var(--text-xs);
    font-weight: 550;
    color: var(--ink-muted);
  }

  tbody th {
    text-align: start;
    font-weight: 550;
    white-space: nowrap;
  }

  td.divisible {
    background: var(--divisible);
  }
</style>
