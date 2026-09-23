<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { SweepTotal, VerseRange } from "../../engine/types";
  import { toRadix } from "../../numberDisplay";
  import { app } from "../../state/app.svelte";
  import Notice from "../Notice.svelte";

  // Every total of the selection a Code 19 argument is built from, with the
  // ones that divide by the reader's divisor (19 unless changed) set apart:
  // the counts, the value, the sums of chapter and verse numbers, and each
  // letter's frequency.

  interface Props {
    range: VerseRange;
  }

  let { range }: Props = $props();

  let totals = $state<SweepTotal[]>([]);
  let error = $state<string | null>(null);
  let onlyMultiples = $state(true);
  const load = latest(engine.sweep);

  $effect(() => {
    const r = { ...range };
    load(r, app.valueSystem, { ...app.counting })
      .then(({ current, value }) => {
        if (!current) return;
        totals = value;
        error = null;
      })
      .catch((e: unknown) => (error = describeError(e)));
  });

  const divides = (t: SweepTotal) => {
    const n = BigInt(t.value);
    return n !== 0n && n % BigInt(app.divisor) === 0n;
  };

  const GROUPS: { key: SweepTotal["group"]; title: string }[] = [
    { key: "counts", title: "Counts" },
    { key: "value", title: "Value" },
    { key: "numbers", title: "Chapter and verse numbers" },
    { key: "letters", title: "Letters" },
  ];

  const hits = $derived(totals.filter(divides).length);
  const shown = $derived(onlyMultiples ? totals.filter(divides) : totals);
</script>

{#if error}
  <Notice tone="error" title="Could not sweep the selection" detail={error} />
{:else}
  <div class="bar">
    <p>
      <b class="num">{hits}</b> of <span class="num">{totals.length}</span> totals divide by
      <b class="num">{app.divisor}</b>
    </p>
    <label><input type="checkbox" bind:checked={onlyMultiples} /> only those</label>
  </div>

  {#if shown.length === 0}
    <Notice title="None of the totals divides by {app.divisor}" detail="Clear “only those” to see them all." />
  {:else}
    {#each GROUPS as group (group.key)}
      {@const rows = shown.filter((t) => t.group === group.key)}
      {#if rows.length > 0}
        <section>
          <h2 class="eyebrow">{group.title}</h2>
          <ul class:letters={group.key === "letters"}>
            {#each rows as t (t.label)}
              {@const multiple = divides(t)}
              <li class:multiple>
                <span class="label" lang={group.key === "letters" ? "ar" : undefined}>{t.label}</span>
                <span class="num value">{toRadix(t.value, app.radix)}</span>
                {#if multiple}
                  <span class="num times">{app.divisor} × {(BigInt(t.value) / BigInt(app.divisor)).toLocaleString("en-US")}</span>
                {/if}
              </li>
            {/each}
          </ul>
        </section>
      {/if}
    {/each}
  {/if}
{/if}

<style>
  .bar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-3);
    margin-bottom: var(--space-4);
  }

  .bar p {
    margin: 0;
  }

  .bar label {
    display: flex;
    align-items: center;
    gap: var(--space-1);
    font-size: var(--text-sm);
    cursor: pointer;
  }

  section + section {
    margin-top: var(--space-4);
  }

  ul {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(17rem, 1fr));
    gap: var(--space-2);
    margin: var(--space-2) 0 0;
    padding: 0;
    list-style: none;
  }

  ul.letters {
    grid-template-columns: repeat(auto-fill, minmax(9rem, 1fr));
  }

  li {
    display: flex;
    align-items: baseline;
    gap: var(--space-2);
    padding: var(--space-2) var(--space-3);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md, 0.375rem);
    background: var(--surface);
  }

  li.multiple {
    border-color: transparent;
    background: var(--divisible);
  }

  .label {
    flex: 1;
    min-width: 0;
    color: var(--ink-muted);
    font-size: var(--text-sm);
  }

  .label:lang(ar) {
    color: var(--ink);
    font-family: var(--font-arabic);
    font-size: var(--text-lg);
  }

  .value {
    font-weight: 600;
  }

  .times {
    color: var(--ink-muted);
    font-size: var(--text-xs);
  }
</style>
