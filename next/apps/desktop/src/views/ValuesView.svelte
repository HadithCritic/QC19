<script lang="ts">
  import NumberChip from "../lib/components/NumberChip.svelte";
  import NumberDetail from "../lib/components/NumberDetail.svelte";
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine, latest } from "../lib/engine/client";
  import type { SystemValue } from "../lib/engine/types";
  import { app } from "../lib/state/app.svelte";
  import { humanize } from "../lib/systems";

  // Features.txt #2: the value of any text in every letter-value system at
  // once. Recomputed as the user types, after a short pause.

  const DEBOUNCE_MS = 250;

  let text = $state("");
  let filter = $state("");
  let rows = $state<SystemValue[]>([]);
  let error = $state<string | null>(null);
  let loading = $state(false);

  const load = latest(engine.textValues);

  $effect(() => {
    const input = text.trim();
    const names = app.systems.map((s) => s.name);
    if (!input) {
      rows = [];
      error = null;
      return;
    }
    const timer = setTimeout(() => {
      loading = true;
      load(input, names)
        .then(({ current, value }) => {
          if (current) {
            rows = value;
            error = null;
          }
        })
        .catch((e: unknown) => (error = describeError(e)))
        .finally(() => (loading = false));
    }, DEBOUNCE_MS);
    return () => clearTimeout(timer);
  });

  const byName = $derived(new Map(app.systems.map((s) => [s.name, s])));
  const current = $derived(rows.find((r) => r.valueSystem === app.valueSystem));
  const shown = $derived.by(() => {
    const query = filter.trim().toLowerCase();
    return query ? rows.filter((r) => r.valueSystem.toLowerCase().includes(query)) : rows;
  });
</script>

<section class="values" aria-labelledby="values-title">
  <header>
    <h1 id="values-title">Value any text</h1>
    <p class="hint">Type or paste Arabic. Each system reads it in its own text mode, so letter counts can differ between rows.</p>
    <label class="visually-hidden" for="values-text">Arabic text</label>
    <textarea id="values-text" class="quran input" lang="ar" dir="rtl" rows="3" bind:value={text} placeholder="بسم الله الرحمن الرحيم"></textarea>
  </header>

  <div class="body" aria-busy={loading}>
    {#if error}
      <Notice tone="error" title="These values could not be computed" detail={error} />
    {:else if rows.length === 0}
      <Notice title="Nothing to value yet" detail="The value under your chosen system appears here, followed by every other system." />
    {:else}
      {#if current}
        <div class="current">
          <div>
            <p class="eyebrow">{humanize(app.currentSystem?.letterOrder ?? "")} · {humanize(app.currentSystem?.letterValue ?? "")}</p>
            <NumberChip value={current.value.value} code={current.value.code} size="lg" />
            <p class="letters"><span class="num">{current.letterCount}</span> letters</p>
          </div>
          <NumberDetail info={current.value} />
        </div>
      {/if}

      <div class="table-head">
        <h2 class="eyebrow">All {rows.length} systems</h2>
        <label class="visually-hidden" for="values-filter">Filter systems</label>
        <input id="values-filter" class="field" bind:value={filter} placeholder="Filter systems" autocomplete="off" />
      </div>
      <table>
        <thead>
          <tr>
            <th scope="col">Text</th>
            <th scope="col">Order</th>
            <th scope="col">Values</th>
            <th scope="col" class="n">Letters</th>
            <th scope="col" class="n">Value</th>
            <th scope="col" class="n">Digit sum</th>
          </tr>
        </thead>
        <tbody>
          {#each shown as row (row.valueSystem)}
            {@const system = byName.get(row.valueSystem)}
            <tr class:selected={row.valueSystem === app.valueSystem}>
              <td>{humanize(system?.textMode ?? "")}</td>
              <td>{humanize(system?.letterOrder ?? "")}</td>
              <td>{humanize(system?.letterValue ?? "")}</td>
              <td class="n num">{row.letterCount}</td>
              <td class="n"><NumberChip value={row.value.value} code={row.value.code} size="sm" /></td>
              <td class="n num">{row.value.digitSum}</td>
            </tr>
          {/each}
        </tbody>
      </table>
    {/if}
  </div>
</section>

<style>
  .values {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
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
    margin: var(--space-1) 0 var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .input {
    width: 100%;
    max-width: 52rem;
    padding: var(--space-2) var(--space-4);
    border: 1px solid var(--rule-strong);
    border-radius: var(--radius-md);
    background: var(--surface);
    font-size: 1.5rem;
    line-height: 1.9;
    resize: vertical;
  }

  .body {
    overflow-y: auto;
    padding: var(--space-5) var(--space-6) var(--space-7);
  }

  .current {
    display: grid;
    grid-template-columns: minmax(14rem, auto) minmax(18rem, 26rem);
    gap: var(--space-6);
    align-items: start;
    margin-bottom: var(--space-6);
  }

  .letters {
    margin: var(--space-1) 0 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .table-head {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-2);
  }

  .table-head h2 {
    margin: 0;
  }

  .table-head input {
    width: 14rem;
    height: 2rem;
    font-size: var(--text-sm);
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  th {
    position: sticky;
    top: calc(-1 * var(--space-5));
    padding: var(--space-2) var(--space-3);
    text-align: start;
    font-weight: 600;
    color: var(--ink-muted);
    background: var(--paper);
    border-bottom: 1px solid var(--rule-strong);
  }

  td {
    padding: 0.35rem var(--space-3);
    border-bottom: 1px solid var(--rule);
  }

  .n {
    text-align: end;
  }

  tr.selected td {
    background: var(--lapis-soft);
  }
</style>
