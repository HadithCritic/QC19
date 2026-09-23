<script lang="ts">
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine } from "../lib/engine/client";
  import type { InitialedChapter } from "../lib/engine/types";

  // The 29 chapters that open with Quranic Initials (Features.txt #76), and how
  // often each chapter's own initials occur in it. Counts that divide by 19 are
  // marked, since that is what the initials are studied for.
  //
  // Each count is computed from the text and set beside Khalifa's figure from
  // The Computer Speaks. Where they differ, his figure is shown too: every
  // alif, and ل in chapters 11 and 30. The claims built on these counts, and
  // the reasons for the differences, are in the findings catalog.

  let chapters = $state<InitialedChapter[]>([]);
  let error = $state<string | null>(null);
  let loading = $state(true);
  let only19 = $state(false);

  const shown = $derived(only19 ? chapters.filter((c) => c.counts.some((n) => n.multipleOf19)) : chapters);
  const letters = $derived([...new Set(chapters.flatMap((c) => [...c.letters]))]);
  const counts = $derived(chapters.flatMap((c) => c.counts).filter((n) => n.published !== null));
  const matching = $derived(counts.filter((n) => n.published === n.count).length);

  function load(): void {
    loading = true;
    error = null;
    engine
      .initials()
      .then((list) => (chapters = list))
      .catch((e: unknown) => (error = describeError(e)))
      .finally(() => (loading = false));
  }

  $effect(() => load());
</script>

<section class="initials">
  <header>
    <h1>Quranic Initials</h1>
    {#if !loading && !error && chapters.length > 0}
      <p class="summary">
        {chapters.length} chapters, {letters.length} letters
        <span class="sep" aria-hidden="true">·</span>
        {matching} of {counts.length} counts match Khalifa
        <span class="sep" aria-hidden="true">·</span>
        <label>
          <input type="checkbox" bind:checked={only19} />
          only multiples of 19
        </label>
      </p>
    {/if}
  </header>

  <div class="scroll">
    {#if loading}
      <Notice tone="loading" title="Counting the initials" />
    {:else if error}
      <Notice tone="error" title="Could not count the initials" detail={error} action={{ label: "Try again", run: load }} />
    {:else if shown.length === 0}
      <Notice title="No chapter matches" detail="No initialed chapter has a count that divides by 19." />
    {:else}
      <table>
        <caption class="visually-hidden">
          Each initialed chapter, its initials, and how often each occurs in that chapter.
        </caption>
        <colgroup>
          <col class="chapter" />
          <col class="name" />
          <col class="letters" />
          <col />
        </colgroup>
        <thead>
          <tr>
            <th scope="col" class="num">Chapter</th>
            <th scope="col">Name</th>
            <th scope="col">Initials</th>
            <th scope="col">Counts</th>
          </tr>
        </thead>
        <tbody>
          {#each shown as c (c.chapter)}
            <tr>
              <td class="num">{c.chapter}</td>
              <td lang="ar" dir="rtl" class="name">{c.name}</td>
              <td lang="ar" dir="rtl" class="letters">
                {c.letters}
                {#if c.verses > 1}<span class="note">over {c.verses} verses</span>{/if}
              </td>
              <td>
                <ul class="counts">
                  {#each c.counts as n (n.letter)}
                    {@const differs = n.published !== null && n.published !== n.count}
                    <li
                      class:divisible={n.multipleOf19}
                      class:differs
                      title={n.published === null
                        ? undefined
                        : differs
                          ? `Khalifa: ${n.published.toLocaleString("en-US")}. The text differs by ${Math.abs(n.published - n.count)}.`
                          : "Matches Khalifa's figure"}
                    >
                      <span lang="ar" dir="rtl">{n.letter}</span>
                      <b>{n.count.toLocaleString("en-US")}</b>
                      {#if differs}<s class="published">{n.published?.toLocaleString("en-US")}</s>{/if}
                      {#if n.multipleOf19}<em>19 × {(n.count / 19).toLocaleString("en-US")}</em>{/if}
                    </li>
                  {/each}
                </ul>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    {/if}
  </div>
</section>

<style>
  .initials {
    display: grid;
    grid-template-rows: auto 1fr;
    grid-template-columns: minmax(0, 1fr);
    height: 100%;
    min-height: 0;
  }

  header {
    display: flex;
    flex-wrap: wrap;
    align-items: baseline;
    justify-content: space-between;
    gap: var(--space-4);
    padding: var(--space-4) var(--space-5);
    border-bottom: 1px solid var(--rule);
  }

  h1 {
    margin: 0;
    font-size: var(--text-lg);
  }

  .summary {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    margin: 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .summary label {
    display: flex;
    align-items: center;
    gap: var(--space-1);
    cursor: pointer;
  }

  .sep {
    color: var(--ink-faint);
  }

  .scroll {
    overflow-y: auto;
    padding: var(--space-4) var(--space-5) var(--space-6);
  }

  table {
    width: 100%;
    max-width: 62rem;
    margin: 0 auto;
    border-collapse: collapse;
    table-layout: fixed;
    font-size: var(--text-sm);
  }

  col.chapter {
    width: 4.5rem;
  }

  col.name {
    width: 8rem;
  }

  col.letters {
    width: 7rem;
  }

  th {
    padding: var(--space-2) var(--space-3);
    border-bottom: 1px solid var(--rule-strong);
    color: var(--ink-faint);
    font-weight: 500;
    text-align: left;
  }

  td {
    padding: var(--space-2) var(--space-3);
    border-bottom: 1px solid var(--rule);
    vertical-align: top;
  }

  .num {
    text-align: right;
    font-variant-numeric: tabular-nums;
  }

  .name {
    font-family: var(--font-arabic);
    font-size: var(--text-lg);
  }

  .letters {
    font-family: var(--font-quran);
    font-size: var(--text-lg);
    white-space: nowrap;
  }

  .note {
    display: block;
    color: var(--ink-faint);
    font-family: var(--font-ui);
    font-size: var(--text-xs);
    direction: ltr;
  }

  .counts {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .counts li {
    display: flex;
    align-items: baseline;
    gap: 0.3rem;
    padding: 0.1rem 0.45rem;
    border: 1px solid var(--rule);
    border-radius: 0.25rem;
    font-family: var(--font-mono);
    font-variant-numeric: tabular-nums;
  }

  .counts li.divisible {
    border-color: transparent;
    background: var(--divisible);
  }

  .counts b {
    font-weight: 600;
  }

  /* Khalifa's figure where the text disagrees with it: shown, not hidden. */
  .counts li.differs {
    border-color: var(--gilt);
  }

  .published {
    color: var(--gilt);
    font-size: var(--text-xs);
    text-decoration: none;
  }

  .published::before {
    content: "Khalifa ";
    font-family: var(--font-ui);
  }

  .counts em {
    color: var(--ink-muted);
    font-size: var(--text-xs);
    font-style: normal;
  }

  .visually-hidden {
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    clip-path: inset(50%);
    white-space: nowrap;
  }
</style>
