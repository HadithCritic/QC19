<script lang="ts">
  import type { RatioBoundary, RatioOptions, RatioScope, RatioUnit } from "../engine/types";
  import { RATIO_PRESETS, ratioTotals } from "../ratioColors";
  import { app } from "../state/app.svelte";
  import NumberChip from "./NumberChip.svelte";

  // Ratio coloring (Features.txt #24): split each verse, chapter or larger
  // unit at a ratio, 1/φ by default, and color its two parts.

  interface Props {
    units: RatioUnit[];
  }

  let { units }: Props = $props();

  const SCOPES: { value: RatioScope; label: string }[] = [
    { value: "verse", label: "each verse" },
    { value: "chapter", label: "the chapter" },
    { value: "page", label: "each page" },
    { value: "station", label: "each station" },
    { value: "part", label: "each part" },
    { value: "group", label: "each group" },
    { value: "half", label: "each half" },
    { value: "quarter", label: "each quarter" },
    { value: "bowing", label: "each bowing" },
    { value: "book", label: "the whole book" },
  ];
  const BOUNDARIES: { value: RatioBoundary; label: string; wide: boolean }[] = [
    { value: "letter", label: "any letter", wide: false },
    { value: "word", label: "a word's end", wide: false },
    { value: "sentence", label: "a pause mark", wide: false },
    { value: "verse", label: "a verse's end", wide: true },
    { value: "chapter", label: "a chapter's end", wide: true },
  ];

  function set(change: Partial<RatioOptions>): void {
    app.ratioOptions = { ...app.ratioOptions, ...change };
  }

  const boundaries = $derived(
    BOUNDARIES.filter((b) => !b.wide || (b.value === "verse" ? app.ratioOptions.scope !== "verse" : app.ratioOptions.scope === "book")),
  );
  const totals = $derived(ratioTotals(units));
  const colored = $derived(units.filter((u) => u.colored).length);
</script>

<div class="ratio">
  <label class="toggle"><input type="checkbox" bind:checked={app.ratioOn} /> Ratio colors</label>
  {#if app.ratioOn}
    <div class="options">
      <span class="presets" role="group" aria-label="Ratio">
        {#each RATIO_PRESETS as preset (preset.label)}
          <button type="button" aria-pressed={Math.abs(app.ratioOptions.ratio - preset.value) < 1e-9} onclick={() => set({ ratio: preset.value })}>{preset.label}</button>
        {/each}
        <input
          class="field num"
          type="number"
          min="0"
          max="1"
          step="0.001"
          aria-label="Ratio"
          value={app.ratioOptions.ratio.toFixed(3)}
          onchange={(e) => {
            const r = Number.parseFloat(e.currentTarget.value);
            if (r >= 0 && r <= 1) set({ ratio: r });
          }}
        />
      </span>
      <select class="field" aria-label="What to split" value={app.ratioOptions.scope} onchange={(e) => set({ scope: e.currentTarget.value as RatioScope, boundary: "letter" })}>
        {#each SCOPES as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
      </select>
      <select class="field" aria-label="Split by" value={app.ratioOptions.measure} onchange={(e) => set({ measure: e.currentTarget.value as RatioOptions["measure"] })}>
        <option value="letters">by letters</option>
        <option value="value">by value</option>
      </select>
      <select class="field" aria-label="Which part is the ratio" value={app.ratioOptions.length} onchange={(e) => set({ length: e.currentTarget.value as RatioOptions["length"] })}>
        <option value="short">ratio first</option>
        <option value="long">ratio last</option>
      </select>
      <label>
        at
        <select class="field" value={app.ratioOptions.boundary} onchange={(e) => set({ boundary: e.currentTarget.value as RatioBoundary })}>
          {#each boundaries as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
        </select>
      </label>
    </div>
    <p class="totals">
      <span class="num">{colored}</span> of <span class="num">{units.length}</span> split there.
      <span class="first">First parts</span> <span class="num">{totals.firstLetters}</span> letters,
      <NumberChip value={totals.firstValue.toString()} size="sm" onselect={(v) => app.openNumber(v)} />;
      <span class="second">second parts</span> <span class="num">{totals.secondLetters}</span> letters,
      <NumberChip value={totals.secondValue.toString()} size="sm" onselect={(v) => app.openNumber(v)} />
    </p>
  {/if}
</div>

<style>
  .ratio {
    display: grid;
    gap: var(--space-2);
    font-size: var(--text-xs);
  }

  .toggle {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    color: var(--ink-muted);
  }

  .options {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-2);
  }

  .field {
    height: 1.75rem;
    font-size: var(--text-xs);
  }

  .presets {
    display: inline-flex;
    align-items: center;
    gap: 2px;
  }

  .presets button {
    padding: 0.1rem 0.45rem;
    border: 1px solid var(--rule-strong);
    border-radius: var(--radius-sm);
    background: var(--surface);
    font-size: var(--text-xs);
  }

  .presets button[aria-pressed="true"] {
    border-color: var(--ratio-first);
    color: var(--ratio-first);
    font-weight: 600;
  }

  .presets input {
    width: 4.5rem;
  }

  .totals {
    margin: 0;
    color: var(--ink-muted);
  }

  .first {
    color: var(--ratio-first);
    font-weight: 600;
  }

  .second {
    color: var(--ratio-second);
    font-weight: 600;
  }
</style>
