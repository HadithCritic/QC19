<script lang="ts">
  import { describeError, engine, latest } from "../../engine/client";
  import type { Scope, WordFrequencies } from "../../engine/types";
  import { app } from "../../state/app.svelte";
  import { search } from "../../state/search.svelte";
  import Notice from "../Notice.svelte";
  import NumberChip from "../NumberChip.svelte";

  // Word frequencies of the selection (Features.txt #21). Choose words to see
  // their total, and search for them together.

  interface Props {
    scope: Scope;
  }

  let { scope }: Props = $props();

  let withMarks = $state(false);
  let byWord = $state(false);
  let data = $state<WordFrequencies | null>(null);
  let error = $state<string | null>(null);
  let chosen = $state<Set<string>>(new Set());

  const load = latest(engine.selectionWords);
  // Words with their marks only differ from the counted words in the modes that keep marks,
  // and are listed for whole verses, so not for a selection of words or letters.
  const marksAvailable = $derived(!("selection" in scope) && ["Original", "SimplifiedMarks"].includes(app.currentSystem?.textMode ?? ""));

  $effect(() => {
    const r = $state.snapshot(scope);
    const marks = withMarks && marksAvailable;
    load(r, app.valueSystem, { ...app.counting }, marks)
      .then(({ current, value }) => {
        if (!current) return;
        data = value;
        error = null;
        chosen = new Set();
      })
      .catch((e: unknown) => (error = describeError(e)));
  });

  const words = $derived(
    data
      ? byWord
        ? [...data.words].sort((a, b) => a.word.localeCompare(b.word, "ar"))
        : data.words
      : [],
  );
  const chosenTotal = $derived(words.filter((w) => chosen.has(w.word)).reduce((sum, w) => sum + w.count, 0));

  function toggle(word: string): void {
    const next = new Set(chosen);
    if (next.has(word)) next.delete(word);
    else next.add(word);
    chosen = next;
  }

  function find(): void {
    void search.start({ kind: "text", term: [...chosen].join(" "), wordness: "whole", grouping: "any" });
  }
</script>

{#if error}
  <Notice tone="error" title="The word list could not be made" detail={error} />
{:else if data}
  <div class="bar">
    <span>
      <NumberChip value={String(chosen.size ? chosenTotal : data.total)} size="sm" onselect={(v) => app.openNumber(v)} />
      words, <NumberChip value={String(chosen.size || data.unique)} size="sm" onselect={(v) => app.openNumber(v)} />
      {chosen.size ? "chosen" : "different"}
    </span>
    <label><input type="checkbox" bind:checked={byWord} /> sort by word</label>
    {#if marksAvailable}<label><input type="checkbox" bind:checked={withMarks} /> with marks</label>{/if}
    <button type="button" class="button" disabled={chosen.size === 0} onclick={find}>Find the chosen words</button>
    {#if chosen.size}<button type="button" class="link" onclick={() => (chosen = new Set())}>Clear choice</button>{/if}
  </div>
  <ol class="words">
    {#each words as w (w.word)}
      <li>
        <label class:chosen={chosen.has(w.word)}>
          <input type="checkbox" checked={chosen.has(w.word)} onchange={() => toggle(w.word)} />
          <span class="count num">{w.count}</span>
          <span class="word arabic" lang="ar" dir="rtl">{w.word}</span>
        </label>
      </li>
    {/each}
  </ol>
{:else}
  <Notice tone="loading" title="Counting words" />
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

  .words {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(11rem, 1fr));
    gap: 0 var(--space-3);
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .words label {
    display: grid;
    grid-template-columns: auto 2.5rem 1fr;
    align-items: center;
    gap: var(--space-2);
    padding: 0.15rem var(--space-2);
    border-radius: var(--radius-sm);
    cursor: pointer;
  }

  .words label:hover,
  .words label.chosen {
    background: var(--lapis-soft);
  }

  .count {
    font-size: var(--text-xs);
    color: var(--ink-muted);
    text-align: end;
  }

  .word {
    font-size: 1.15rem;
    text-align: end;
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
