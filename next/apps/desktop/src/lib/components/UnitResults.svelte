<script lang="ts">
  import type { FoundUnit } from "../engine/types";
  import { app } from "../state/app.svelte";
  import { SEARCH_PAGE, search } from "../state/search.svelte";
  import NumberChip from "./NumberChip.svelte";

  // Units, runs or sets a number or frequency search found: what each
  // measures, and its first verses with any found words marked.

  function open(unit: FoundUnit): void {
    // A set of scattered verses opens at its first one.
    const last = unit.verseNumbers ? unit.firstVerse : unit.lastVerse;
    app.goTo({ first: unit.firstVerse, last });
  }

  function plural(n: number, word: string): string {
    return `${n} ${word}${n === 1 ? "" : "s"}`;
  }

  const result = $derived(search.unitResult);
</script>

{#if result}
  <ol class="units">
    {#each search.units as unit, index (index)}
      <li>
        <div class="head">
          <button type="button" class="ref" onclick={() => open(unit)} aria-label="Open {unit.reference} in the reader">{unit.reference}</button>
          <span class="measures">
            {#if unit.verses > 0}{plural(unit.verses, "verse")} ·{/if}
            {plural(unit.words, "word")} · {plural(unit.letters, "letter")} ({unit.uniqueLetters} distinct)
            {#if unit.letterFrequencySum !== null}· sum <span class="num">{unit.letterFrequencySum}</span>{/if}
          </span>
          <NumberChip value={unit.value.value} code={unit.value.code} size="sm" />
        </div>
        {#each unit.preview as verse (verse.number)}
          <p class="quran" lang="ar" dir="rtl">
            <span class="where num" lang="en" dir="ltr">{verse.chapter}:{verse.numberInChapter}</span>
            {#if verse.bismillah && verse.bismillahHighlights.length > 0}
              {#each verse.bismillah.split(" ") as word, i (i)}{#if verse.bismillahHighlights.includes(i)}<mark>{word}</mark>{:else}{word}{/if}{" "}{/each}
            {/if}
            {#each verse.words as word, i (i)}{#if verse.highlights.includes(i)}<mark>{word}</mark>{:else}{word}{/if}{" "}{/each}
          </p>
        {/each}
        {#if unit.morePreview}<p class="more-verses">and more verses</p>{/if}
      </li>
    {/each}
  </ol>
  {#if search.units.length < result.unitCount}
    <button type="button" class="button more" disabled={search.loading} onclick={() => search.more()}>
      Show {Math.min(SEARCH_PAGE, result.unitCount - search.units.length)} more
    </button>
  {/if}
{/if}

<style>
  .units {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  li {
    padding: var(--space-3) 0;
    border-top: 1px solid var(--rule);
  }

  .head {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
  }

  .ref {
    padding: 0.1rem 0.4rem;
    border: 0;
    border-radius: var(--radius-sm);
    background: none;
    color: var(--lapis);
    font-size: var(--text-sm);
    font-weight: 550;
  }

  .ref:hover {
    background: var(--lapis-soft);
  }

  .measures {
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .quran {
    margin: var(--space-1) 0 0;
    font-size: 1.3rem;
    line-height: 2;
  }

  .where {
    margin-inline-end: var(--space-2);
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  mark {
    color: inherit;
    background: var(--gilt-soft);
    text-decoration: underline 2px var(--gilt);
    text-underline-offset: 0.35em;
    border-radius: 2px;
  }

  mark:global(.current) {
    background: var(--gilt);
    color: var(--surface);
  }

  .more-verses {
    margin: 0;
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  .more {
    margin-top: var(--space-4);
  }
</style>
