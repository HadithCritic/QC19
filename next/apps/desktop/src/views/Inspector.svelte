<script lang="ts">
  import NumberChip from "../lib/components/NumberChip.svelte";
  import NumberDetail from "../lib/components/NumberDetail.svelte";
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine, latest } from "../lib/engine/client";
  import type { NumberInfo, Stats } from "../lib/engine/types";
  import { rangeReference } from "../lib/numbers";
  import { app } from "../lib/state/app.svelte";
  import { humanize } from "../lib/systems";
  import { COUNTING_CHOICES, DEFAULT_COUNTING, unavailableReason } from "../lib/counting";

  // Live statistics for the selection: the panel the legacy app kept beside
  // the text, recomputed whenever the selection or the system changes.

  let stats = $state<Stats | null>(null);
  let error = $state<string | null>(null);
  let loading = $state(false);
  let focused = $state<{ label: string; info: NumberInfo } | null>(null);

  const load = latest(engine.stats);

  $effect(() => {
    const selection = app.selection;
    const system = app.valueSystem;
    const counting = { ...app.counting };
    if (!selection || !system) {
      stats = null;
      return;
    }
    loading = true;
    error = null;
    load(selection, system, counting)
      .then(({ current, value }) => {
        if (!current) return;
        stats = value;
        focused = { label: "Value", info: value.value };
      })
      .catch((e: unknown) => (error = describeError(e)))
      .finally(() => (loading = false));
  });

  const counts = $derived(
    stats
      ? [
          { label: "Chapters", info: stats.chapters },
          { label: "Verses", info: stats.verses },
          { label: "Words", info: stats.words },
          { label: "Letters", info: stats.letters },
          { label: "Distinct letters", info: stats.distinctLetters },
        ]
      : [],
  );

  // Options in effect that differ from the defaults, named for the reader.
  const counted = $derived(
    COUNTING_CHOICES.filter(
      (c) =>
        app.counting[c.key] !== DEFAULT_COUNTING[c.key] &&
        unavailableReason(c.key, app.currentSystem?.textMode ?? "", app.hasVerseZero) === null,
    ).map((c) => (c.key === "includeBasmalas" ? "without the Bismillah" : c.label.toLowerCase())),
  );

  const maxFrequency = $derived(stats?.letterFrequencies[0]?.count ?? 1);
  const reference = $derived(stats ? rangeReference(stats.first, stats.last, app.chapters) : "");
</script>

<aside class="inspector" aria-label="Selection statistics" aria-busy={loading}>
  {#if error}
    <Notice tone="error" title="These statistics could not be computed" detail={error} />
  {:else if !app.selection}
    <Notice title="Select verses to see their numbers" detail="Click a verse to select it and shift-click to extend the range. You can also type a reference such as 2:255." />
  {:else if stats && stats.verses.value === "0"}
    <Notice
      title="This Bismillah is not counted"
      detail="Turn on Count the Bismillah under Counting in the top bar to include each chapter's verse 0."
    />
  {:else if stats}
    <header>
      <p class="eyebrow">Selection</p>
      <h2 class="num">{reference}</h2>
      <p class="system">{humanize(app.currentSystem?.textMode ?? "")} · {humanize(app.currentSystem?.letterOrder ?? "")} · {humanize(app.currentSystem?.letterValue ?? "")}</p>
      {#if counted.length > 0}
        <p class="system">Counting: {counted.join(", ")}</p>
      {/if}
    </header>

    <section class="value" aria-label="Value">
      <NumberChip value={stats.value.value} code={stats.value.code} size="lg" onselect={() => (focused = { label: "Value", info: stats!.value })} />
    </section>

    <section aria-label="Counts">
      <dl class="counts">
        {#each counts as count (count.label)}
          <div class:active={focused?.info === count.info}>
            <dt>{count.label}</dt>
            <dd><NumberChip value={count.info.value} code={count.info.code} size="sm" onselect={() => (focused = count)} /></dd>
          </div>
        {/each}
      </dl>
    </section>

    {#if focused}
      <section class="focused" aria-label="Number detail">
        <h3 class="eyebrow">{focused.label} <span class="num">{focused.info.value}</span></h3>
        <NumberDetail info={focused.info} />
      </section>
    {/if}

    <section aria-label="Letter frequencies">
      <h3 class="eyebrow">Letter frequencies</h3>
      <ol class="letters">
        {#each stats.letterFrequencies as frequency (frequency.letter)}
          <li>
            <span class="letter arabic" lang="ar">{frequency.letter}</span>
            <span class="bar" style:--share={frequency.count / maxFrequency}></span>
            <span class="num count">{frequency.count}</span>
          </li>
        {/each}
      </ol>
    </section>
  {:else}
    <Notice tone="loading" title="Counting" />
  {/if}
</aside>

<style>
  .inspector {
    display: grid;
    align-content: start;
    gap: var(--space-5);
    padding: var(--space-5);
    overflow-y: auto;
    background: var(--surface);
    border-inline-start: 1px solid var(--rule);
  }

  .inspector[aria-busy="true"] {
    cursor: progress;
  }

  header h2 {
    margin: var(--space-1) 0 0;
    font-size: var(--text-xl);
    font-weight: 500;
  }

  .system {
    margin: var(--space-1) 0 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .value {
    padding-bottom: var(--space-2);
  }

  .counts {
    display: grid;
    margin: 0;
  }

  .counts div {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
  }

  .counts div.active dt {
    color: var(--lapis);
    font-weight: 600;
  }

  .counts dt {
    color: var(--ink-muted);
    font-size: var(--text-sm);
  }

  .counts dd {
    margin: 0;
  }

  h3 {
    margin: 0 0 var(--space-2);
  }

  h3 .num {
    text-transform: none;
    letter-spacing: 0;
    color: var(--ink);
  }

  .focused {
    padding: var(--space-3) var(--space-4) var(--space-2);
    background: var(--paper);
    border-radius: var(--radius-md);
  }

  .letters {
    display: grid;
    gap: 2px;
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .letters li {
    display: grid;
    grid-template-columns: 1.75rem 1fr 3.5rem;
    align-items: center;
    gap: var(--space-2);
    font-size: var(--text-sm);
  }

  .letter {
    font-size: 1.15rem;
    text-align: center;
  }

  .bar {
    height: 6px;
    border-radius: 3px;
    background: linear-gradient(to right, var(--lapis) calc(var(--share) * 100%), var(--surface-sunk) 0);
  }

  .count {
    text-align: end;
    color: var(--ink-muted);
  }
</style>
