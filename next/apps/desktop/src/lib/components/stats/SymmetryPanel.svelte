<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { Symmetry, SymmetryKind, Scope } from "../../engine/types";
  import { app } from "../../state/app.svelte";
  import Notice from "../Notice.svelte";

  // Front-back symmetry (Features.txt #8): add up the units from the front and
  // from the back at once, and list every place the two totals agree.

  interface Props {
    scope: Scope;
  }

  let { scope }: Props = $props();

  const KINDS: { value: SymmetryKind; label: string; unit: string; total: string }[] = [
    { value: "wordLetters", label: "letters of each word", unit: "Words", total: "Letters" },
    { value: "verseWords", label: "words of each verse", unit: "Verses", total: "Words" },
    { value: "verseLetters", label: "letters of each verse", unit: "Verses", total: "Letters" },
  ];

  let kind = $state<SymmetryKind>("wordLetters");
  let boundaries = $state(false);
  let result = $state<Symmetry | null>(null);
  let error = $state<string | null>(null);
  const load = latest(engine.selectionSymmetry);

  $effect(() => {
    const r = $state.snapshot(scope);
    load(r, app.valueSystem, { ...app.counting }, kind, boundaries)
      .then(({ current, value }) => {
        if (!current) return;
        result = value;
        error = null;
      })
      .catch((e: unknown) => (error = describeError(e)));
  });

  const labels = $derived(KINDS.find((k) => k.value === kind)!);
</script>

<div class="bar">
  <label>
    Compare the
    <select class="field" bind:value={kind}>
      {#each KINDS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
    </select>
  </label>
  <label><input type="checkbox" bind:checked={boundaries} /> count the empty start and the full end</label>
</div>

{#if error}
  <Notice tone="error" title="The symmetry could not be worked out" detail={error} />
{:else if result}
  <p class="summary">
    <span class="num">{result.points.length - (boundaries ? 2 : 0)}</span> places of agreement in
    <span class="num">{result.units}</span> {labels.unit.toLowerCase()}: symmetry <strong class="num">{result.percent.toFixed(3)}%</strong>
  </p>
  {#if result.points.length > 0}
    <table>
      <thead>
        <tr><th>{labels.unit}</th><th>{labels.total}</th><th>Σ {labels.unit.toLowerCase()}</th><th>Σ {labels.total.toLowerCase()}</th></tr>
      </thead>
      <tbody>
        {#each result.points as point, i (i)}
          <tr><td class="num">{point.position}</td><td class="num">{point.total}</td><td class="num">{point.positionSum}</td><td class="num">{point.totalSum}</td></tr>
        {/each}
      </tbody>
    </table>
  {/if}
{:else}
  <Notice tone="loading" title="Comparing both ends" />
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

  .summary {
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
  }

  table {
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  th,
  td {
    padding: 0.2rem 1.2rem 0.2rem 0;
    text-align: end;
    border-bottom: 1px solid var(--rule);
  }

  th {
    font-size: var(--text-xs);
    font-weight: 550;
    color: var(--ink-muted);
  }
</style>
