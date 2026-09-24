<script lang="ts">
  import { COUNTING_CHOICES } from "../counting";
  import { describeError, engine, latest } from "../engine/client";
  import type { CountedPosition, NumberInfo, QuranLocation, SelectionAnalysis } from "../engine/types";
  import { isDivisible } from "../numberDisplay";
  import { formatLocation, formatSelection, ordered } from "../selection";
  import { app } from "../state/app.svelte";
  import { humanize } from "../systems";
  import Notice from "./Notice.svelte";
  import NumberChip from "./NumberChip.svelte";
  import NumberDetail from "./NumberDetail.svelte";

  // The inspector for an exact selection: where it starts and ends, how much
  // it holds, its value, and everything that decided that value.

  /** Selection changes come in bursts while clicking; analyze once they settle. */
  const ANALYZE_DELAY_MS = 150;

  let analysis = $state<SelectionAnalysis | null>(null);
  let error = $state<string | null>(null);
  let focused = $state<{ label: string; info: NumberInfo } | null>(null);
  let copied = $state(false);
  let saving = $state(false);
  let title = $state("");
  let saveError = $state<string | null>(null);

  const load = latest(engine.analyze);

  $effect(() => {
    const selection = app.exact;
    const system = app.valueSystem;
    const counting = { ...app.counting };
    if (!selection || !system) return;
    const timer = setTimeout(() => {
      load(selection, system, counting)
        .then(({ current, value }) => {
          if (!current) return;
          analysis = value;
          error = null;
          focused = { label: "Value", info: value.value };
        })
        .catch((e: unknown) => (error = describeError(e)));
    }, ANALYZE_DELAY_MS);
    return () => clearTimeout(timer);
  });

  const selection = $derived(app.exact ? ordered(app.exact) : null);
  const counts = $derived(
    analysis
      ? [
          { label: "Verses", info: analysis.verses },
          { label: "Words", info: analysis.words },
          { label: "Letters", info: analysis.letters },
          { label: "Distinct words", info: analysis.distinctWords },
          { label: "Distinct letters", info: analysis.distinctLetters },
        ]
      : [],
  );
  const divisible = $derived(
    analysis
      ? [
          { label: "Value", info: analysis.value },
          { label: "Words", info: analysis.words },
          { label: "Letters", info: analysis.letters },
        ].map((c) => ({ ...c, divides: isDivisible(c.info.value, app.divisor), quotient: quotient(c.info.value) }))
      : [],
  );
  const counted = $derived(
    analysis ? COUNTING_CHOICES.map((c) => `${c.label}: ${analysis!.methodology.counting[c.key] ? "on" : "off"}`) : [],
  );

  function quotient(value: string): string {
    try {
      return (BigInt(value) / BigInt(app.divisor)).toString();
    } catch {
      return "";
    }
  }

  function describe(location: QuranLocation): string {
    const parts = [`${location.chapter}${location.verse !== null ? `:${location.verse}` : ""}`];
    if (location.word !== null) parts.push(`word ${location.word}`);
    if (location.letter !== null) parts.push(`letter ${location.letter}`);
    return parts.join(" · ");
  }

  function positionRows(p: CountedPosition): [string, number][] {
    return [
      ["Word in verse", p.wordInVerse],
      ["Word in chapter", p.wordInChapter],
      ["Absolute word", p.absoluteWord],
      ["Letter in word", p.letterInWord],
      ["Letter in verse", p.letterInVerse],
      ["Letter in chapter", p.letterInChapter],
      ["Absolute letter", p.absoluteLetter],
    ];
  }

  async function copyAddress(): Promise<void> {
    if (!app.exact) return;
    try {
      await navigator.clipboard.writeText(formatSelection(ordered(app.exact)));
      copied = true;
      setTimeout(() => (copied = false), 1500);
    } catch {
      copied = false; // the address is also shown above, to copy by hand
    }
  }

  async function save(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    saving = true;
    saveError = null;
    try {
      await app.saveResearchSelection(title.trim(), "");
      title = "";
    } catch (e) {
      saveError = describeError(e);
    } finally {
      saving = false;
    }
  }
</script>

{#if selection}
  <header>
    <p class="eyebrow">{app.pendingStart ? "Selection started" : "Selection"}</p>
    <h2 class="num address">{formatSelection(selection)}</h2>
    <dl class="ends">
      <div><dt>Start</dt><dd>{describe(selection.start)}</dd></div>
      {#if formatLocation(selection.start) !== formatLocation(selection.end)}
        <div><dt>End</dt><dd>{describe(selection.end)}</dd></div>
      {/if}
    </dl>
    {#if app.pendingStart}
      <p class="hint">Click where the selection ends. Shift-click extends; Esc clears.</p>
    {/if}
  </header>

  <div class="actions">
    {#if selection.start.letter !== null || selection.end.letter !== null}
      <button type="button" onclick={() => app.expandSelection("word")}>Whole words</button>
    {/if}
    <button type="button" onclick={() => app.expandSelection("verse")}>Whole verses</button>
    <button type="button" onclick={() => app.expandSelection("chapter")}>Whole chapters</button>
    <button type="button" onclick={copyAddress}>{copied ? "Copied" : "Copy address"}</button>
    <button type="button" onclick={() => (app.view = "statistics")}>Analyze</button>
    <button type="button" onclick={() => app.clearSelection()}>Clear</button>
  </div>

  {#if error}
    <Notice tone="error" title="This selection could not be analyzed" detail={error} />
  {:else if analysis}
    {#each analysis.notes as note (note)}
      <p class="note">{note}</p>
    {/each}

    <section class="value" aria-label="Value">
      <p class="eyebrow">{humanize(analysis.methodology.textMode)} · {humanize(app.currentSystem?.letterOrder ?? "")} · {humanize(app.currentSystem?.letterValue ?? "")}</p>
      <NumberChip value={analysis.value.value} code={analysis.value.code} size="lg" onselect={() => (focused = { label: "Value", info: analysis!.value })} />
    </section>

    <section aria-label="Span">
      <h3 class="eyebrow">Selection span</h3>
      <dl class="counts">
        {#each counts as count (count.label)}
          <div class:active={focused?.info === count.info}>
            <dt>{count.label}</dt>
            <dd><NumberChip value={count.info.value} code={count.info.code} size="sm" onselect={() => (focused = count)} /></dd>
          </div>
        {/each}
      </dl>
      {#if !analysis.verseAligned && analysis.start}
        <p class="hint">Counts are inclusive. A word or verse the selection cuts counts once; letters and value are only the selected letters.</p>
      {/if}
    </section>

    <section aria-label="Divisibility">
      <h3 class="eyebrow">Divisibility by <span class="num">{app.divisor}</span></h3>
      <dl class="divisibility">
        {#each divisible as d (d.label)}
          <div class:divides={d.divides}>
            <dt>{d.label}</dt>
            <dd class="num">{d.divides ? `${d.quotient} ×${app.divisor}` : "no"}</dd>
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

    {#if analysis.start && analysis.end}
      <details>
        <summary class="eyebrow">Endpoint positions</summary>
        <p class="hint">Counted positions under the current text mode and options; absolute numbers run through the whole counted text.</p>
        <table class="positions">
          <thead><tr><th></th><th>Start</th><th>End</th></tr></thead>
          <tbody>
            {#each positionRows(analysis.start) as [label, value], i (label)}
              <tr><th>{label}</th><td class="num">{value}</td><td class="num">{positionRows(analysis.end)[i]?.[1]}</td></tr>
            {/each}
          </tbody>
        </table>
        {#if analysis.delta}
          <p class="hint">
            End minus start: <span class="num">{analysis.delta.chapters}</span> chapters, <span class="num">{analysis.delta.verses}</span> verses,
            <span class="num">{analysis.delta.words}</span> words, <span class="num">{analysis.delta.letters}</span> letters. This is a difference
            of positions, not the selected count.
          </p>
        {/if}
      </details>
    {/if}

    <details>
      <summary class="eyebrow">Methodology</summary>
      <dl class="method">
        <div><dt>Selection</dt><dd class="num">{analysis.address}</dd></div>
        <div><dt>Edition</dt><dd>{humanize(analysis.methodology.edition)}</dd></div>
        <div><dt>Text mode</dt><dd>{analysis.methodology.textMode}</dd></div>
        <div><dt>Value system</dt><dd>{analysis.methodology.valueSystem}</dd></div>
        {#each counted as line (line)}
          <div><dt></dt><dd>{line}</dd></div>
        {/each}
      </dl>
    </details>

    {#if app.researchSelections}
      <form class="save" onsubmit={save}>
        <label class="eyebrow" for="selection-title">Save for research</label>
        <div class="save-row">
          <input id="selection-title" class="field" placeholder={analysis.address} bind:value={title} maxlength="200" />
          <button type="submit" disabled={saving}>Save</button>
        </div>
        {#if saveError}<p class="hint error" role="alert">{saveError}</p>{/if}
      </form>
    {/if}
  {:else}
    <Notice tone="loading" title="Counting" />
  {/if}
{/if}

<style>
  header h2 {
    margin: var(--space-1) 0 0;
    font-size: var(--text-lg);
    font-weight: 500;
    word-break: break-all;
  }

  .ends {
    display: grid;
    gap: 2px;
    margin: var(--space-2) 0 0;
    font-size: var(--text-sm);
  }

  .ends div,
  .method div {
    display: grid;
    grid-template-columns: 5.5rem 1fr;
    gap: var(--space-2);
  }

  .ends dt,
  .method dt {
    color: var(--ink-muted);
  }

  .ends dd,
  .method dd {
    margin: 0;
  }

  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-1);
  }

  .actions button,
  .save button {
    padding: 0.2rem 0.6rem;
    border: 1px solid var(--rule);
    border-radius: var(--radius-sm);
    background: var(--surface);
    color: var(--ink);
    font-size: var(--text-xs);
    font-weight: 550;
  }

  .actions button:hover,
  .save button:hover {
    border-color: var(--lapis);
    color: var(--lapis);
  }

  .note {
    margin: 0;
    padding: var(--space-2) var(--space-3);
    border-radius: var(--radius-sm);
    background: var(--gilt-soft);
    font-size: var(--text-sm);
  }

  .hint {
    margin: var(--space-2) 0 0;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .hint.error {
    color: var(--danger);
  }

  h3 {
    margin: 0 0 var(--space-2);
  }

  h3 .num {
    text-transform: none;
    letter-spacing: 0;
    color: var(--ink);
  }

  .counts,
  .divisibility {
    display: grid;
    margin: 0;
  }

  .counts div,
  .divisibility div {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
  }

  .counts dt,
  .divisibility dt {
    color: var(--ink-muted);
    font-size: var(--text-sm);
  }

  .counts dd,
  .divisibility dd {
    margin: 0;
  }

  .counts div.active dt {
    color: var(--lapis);
    font-weight: 600;
  }

  .divisibility .divides dd {
    color: var(--divisible);
    font-weight: 600;
  }

  .divisibility div:not(.divides) dd {
    color: var(--ink-faint);
  }

  .focused {
    padding: var(--space-3) var(--space-4) var(--space-2);
    background: var(--paper);
    border-radius: var(--radius-md);
  }

  details summary {
    cursor: pointer;
  }

  .positions {
    width: 100%;
    margin-top: var(--space-2);
    border-collapse: collapse;
    font-size: var(--text-sm);
  }

  .positions th,
  .positions td {
    padding: 2px 0;
    text-align: end;
  }

  .positions th:first-child {
    text-align: start;
    font-weight: 400;
    color: var(--ink-muted);
  }

  .method {
    display: grid;
    gap: 2px;
    margin: var(--space-2) 0 0;
    font-size: var(--text-sm);
  }

  .save {
    display: grid;
    gap: var(--space-2);
  }

  .save-row {
    display: flex;
    gap: var(--space-2);
  }

  .save-row input {
    flex: 1;
    min-width: 0;
  }
</style>
