<script lang="ts">
  import { describeError, engine, latest } from "../engine/client";
  import type { SelectionAnalysis } from "../engine/types";
  import { formatSelection, ordered } from "../selection";
  import { app } from "../state/app.svelte";

  // A one-line reminder of the exact selection outside the reader, so every
  // view shows what it is analyzing. The reader has the full inspector.

  const ANALYZE_DELAY_MS = 150;

  let analysis = $state<SelectionAnalysis | null>(null);
  let error = $state<string | null>(null);
  const load = latest(engine.analyze);

  $effect(() => {
    const selection = app.exact;
    const system = app.valueSystem;
    const counting = { ...app.counting };
    if (!selection || !system) return;
    const timer = setTimeout(() => {
      load(selection, system, counting)
        .then(({ current, value }) => {
          if (current) {
            analysis = value;
            error = null;
          }
        })
        .catch((e: unknown) => (error = describeError(e)));
    }, ANALYZE_DELAY_MS);
    return () => clearTimeout(timer);
  });

  function reread(): void {
    if (app.exact) app.setExact(app.exact);
  }
</script>

{#if app.exact}
  <div class="bar" role="status" aria-label="Selected">
    <span class="eyebrow">Selected</span>
    <span class="num address">{formatSelection(ordered(app.exact))}</span>
    {#if error}
      <span class="error">{error}</span>
    {:else if analysis}
      <span class="figures">
        <span class="num">{analysis.verses.value}</span> verses ·
        <span class="num">{analysis.words.value}</span> words ·
        <span class="num">{analysis.letters.value}</span> letters ·
        value <span class="num">{analysis.value.value}</span>
      </span>
    {/if}
    <span class="actions">
      {#if app.view !== "read"}<button type="button" onclick={reread}>Show in reader</button>{/if}
      {#if app.view !== "statistics"}<button type="button" onclick={() => (app.view = "statistics")}>Analyze</button>{/if}
      <button type="button" onclick={() => app.clearSelection()}>Clear</button>
    </span>
  </div>
{/if}

<style>
  .bar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-2) var(--space-5);
    border-bottom: 1px solid var(--rule);
    background: var(--lapis-soft);
    font-size: var(--text-sm);
  }

  .address {
    font-weight: 600;
  }

  .figures {
    color: var(--ink-muted);
  }

  .error {
    color: var(--danger);
  }

  .actions {
    display: flex;
    gap: var(--space-1);
    margin-inline-start: auto;
  }

  button {
    padding: 0.15rem 0.6rem;
    border: 1px solid var(--rule);
    border-radius: var(--radius-sm);
    background: var(--surface);
    color: var(--ink);
    font-size: var(--text-xs);
    font-weight: 550;
  }

  button:hover {
    border-color: var(--lapis);
    color: var(--lapis);
  }
</style>
