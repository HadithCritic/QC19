<script lang="ts">
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine } from "../lib/engine/client";
  import type { Finding } from "../lib/engine/types";

  // Published Code 19 results, each recomputed from the text. A finding shows
  // its counting rule, its convention and its source, so a reader can see what
  // was counted and on whose authority, not only the number.
  //
  // ADR 0004: an inferred rule is marked, because the figure then rests on an
  // assumption rather than on something the source stated.

  let findings = $state<Finding[]>([]);
  let error = $state<string | null>(null);
  let loading = $state(true);
  let openId = $state<string | null>(null);
  let show = $state<"all" | "holds" | "open">("all");

  const shown = $derived(
    show === "all" ? findings : show === "holds" ? findings.filter((f) => f.holds) : findings.filter((f) => !f.holds),
  );

  const holding = $derived(findings.filter((f) => f.holds).length);
  const open = $derived(findings.filter((f) => f.check === "open").length);
  // A gated finding that fails is a regression; an open one is a known gap.
  const broken = $derived(findings.filter((f) => f.check === "gate" && !f.holds).length);

  function load(): void {
    loading = true;
    error = null;
    engine
      .findings()
      .then((list) => (findings = list))
      .catch((e: unknown) => (error = describeError(e)))
      .finally(() => (loading = false));
  }

  $effect(() => load());

  function group(value: number): string {
    return value.toLocaleString("en-US");
  }
</script>

<section class="findings">
  <header>
    <h1>Findings</h1>
    {#if !loading && !error && findings.length > 0}
      <p class="tally">
        {holding} of {findings.length} reproduce from this text{#if open > 0}; {open} open{/if}{#if broken > 0}; <strong class="broken">{broken} no longer reproduce</strong>{/if}
      </p>
      <div class="filter" role="group" aria-label="Show">
        {#each [["all", "All"], ["holds", "Reproduce"], ["open", "Do not reproduce"]] as [id, label] (id)}
          <button type="button" aria-pressed={show === id} onclick={() => (show = id as typeof show)}>{label}</button>
        {/each}
      </div>
    {/if}
  </header>

  <div class="scroll">
    {#if loading}
      <Notice tone="loading" title="Checking the findings" />
    {:else if error}
      <Notice tone="error" title="Could not check the findings" detail={error} action={{ label: "Try again", run: load }} />
    {:else if findings.length === 0}
      <Notice title="No findings yet" detail="The catalog is empty." />
    {:else}
      <ul>
        {#each shown as f (f.id)}
          <li class:disagrees={!f.holds && f.check === "gate"} class:open={f.check === "open"}>
            <button
              type="button"
              class="head"
              aria-expanded={openId === f.id}
              onclick={() => (openId = openId === f.id ? null : f.id)}
            >
              <span class="mark" class:holds={f.holds} class:open={!f.holds && f.check === "open"} aria-hidden="true"
                >{f.holds ? "✓" : f.check === "open" ? "?" : "✕"}</span
              >
              <span class="claim">{f.claim}</span>
              <span class="value" class:divisible={f.multipleOf19}>
                {group(f.computed)}
                {#if f.multiple !== null}<em>19 × {group(f.multiple)}</em>{/if}
              </span>
            </button>

            {#if !f.holds}
              <p class="mismatch" class:open={f.check === "open"}>
                Computed {group(f.computed)}, published {group(f.expected)}, a gap of {group(Math.abs(f.expected - f.computed))}.
                {#if f.check === "open"}Open: the cause is not settled, so this does not fail the build.{/if}
              </p>
            {/if}

            {#if openId === f.id}
              <dl class="detail">
                <dt>Rule</dt>
                <dd>
                  {f.rule}
                  {#if f.basis === "inferred"}
                    <span class="inferred">inferred, not stated by the source</span>
                  {/if}
                </dd>
                <dt>Counted over</dt>
                <dd>{f.scope}, {f.convention}, {f.textMode}</dd>
                <dt>Source</dt>
                <dd>{f.source}</dd>
              </dl>
            {/if}
          </li>
        {/each}
      </ul>
    {/if}
  </div>
</section>

<style>
  .findings {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-4);
    padding: var(--space-4) var(--space-5);
    border-bottom: 1px solid var(--rule);
  }

  h1 {
    margin: 0;
    font-size: var(--text-lg);
  }

  .filter {
    display: flex;
    gap: 2px;
    padding: 2px;
    border-radius: var(--radius-md, 0.375rem);
    background: var(--surface-sunk);
  }

  .filter button {
    padding: 0.2rem 0.6rem;
    border: 0;
    border-radius: inherit;
    background: none;
    color: var(--ink-muted);
    font: inherit;
    font-size: var(--text-sm);
    cursor: pointer;
  }

  .filter button[aria-pressed="true"] {
    background: var(--surface);
    color: var(--ink);
  }

  .tally {
    margin: 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .scroll {
    overflow-y: auto;
    padding: var(--space-4) var(--space-5) var(--space-6);
  }

  ul {
    display: grid;
    gap: var(--space-3);
    margin: 0 auto;
    padding: 0;
    max-width: 62rem;
    list-style: none;
  }

  li {
    border: 1px solid var(--rule);
    border-radius: 0.5rem;
    background: var(--surface);
  }

  li.disagrees {
    border-color: var(--danger);
  }

  .head {
    display: grid;
    grid-template-columns: auto 1fr auto;
    align-items: center;
    gap: var(--space-3);
    width: 100%;
    padding: var(--space-3) var(--space-4);
    border: 0;
    border-radius: inherit;
    background: none;
    color: inherit;
    font: inherit;
    text-align: left;
    cursor: pointer;
  }

  .head:hover {
    background: var(--surface-sunk);
  }

  .mark {
    color: var(--danger);
    font-weight: 700;
  }

  .mark.holds {
    color: var(--class-xp);
  }

  .claim {
    min-width: 0;
  }

  .value {
    font-family: var(--font-mono);
    font-variant-numeric: tabular-nums;
    white-space: nowrap;
  }

  .value.divisible {
    padding: 0.1rem 0.4rem;
    border-radius: 0.25rem;
    background: var(--divisible);
  }

  .value em {
    margin-left: var(--space-2);
    color: var(--ink-muted);
    font-size: var(--text-xs);
    font-style: normal;
  }

  li.open {
    border-color: var(--gilt);
  }

  .mark.open {
    color: var(--gilt);
  }

  .mismatch.open {
    color: var(--gilt);
  }

  .broken {
    color: var(--danger);
  }

  .mismatch {
    margin: 0;
    padding: 0 var(--space-4) var(--space-3);
    color: var(--danger);
    font-size: var(--text-sm);
  }

  .detail {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: var(--space-2) var(--space-4);
    margin: 0;
    padding: var(--space-3) var(--space-4) var(--space-4);
    border-top: 1px solid var(--rule);
    font-size: var(--text-sm);
  }

  dt {
    color: var(--ink-faint);
  }

  dd {
    margin: 0;
  }

  .inferred {
    display: inline-block;
    margin-left: var(--space-2);
    padding: 0.05rem 0.4rem;
    border-radius: 0.25rem;
    background: var(--gilt-soft);
    color: var(--gilt);
    font-size: var(--text-xs);
  }
</style>
