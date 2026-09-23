<script lang="ts">
  import type { CriterionInput } from "../engine/types";
  import { COMPARISONS, KINDS } from "../numberQuery";

  // One constraint of a number search: a comparison with a value, or a kind
  // of number. Empty means "any".

  interface Props {
    id: string;
    label: string;
    criterion: CriterionInput;
    /** Hide the Σ comparison where a sum of positions means nothing. */
    allowSum?: boolean;
    onchange: (criterion: CriterionInput) => void;
  }

  let { id, label, criterion, allowSum = true, onchange }: Props = $props();

  const comparisons = $derived(allowSum ? COMPARISONS : COMPARISONS.filter((c) => c.value !== "sum"));
  const byKind = $derived((criterion.type ?? "none") !== "none");

  function update(change: Partial<CriterionInput>): void {
    onchange({ ...criterion, ...change });
  }
</script>

<div class="row">
  <label for="{id}-value">{label}</label>
  <select
    class="field op"
    aria-label="{label}: comparison"
    disabled={byKind}
    value={criterion.comparison ?? "eq"}
    onchange={(e) => update({ comparison: e.currentTarget.value as CriterionInput["comparison"] })}
  >
    {#each comparisons as option (option.value)}<option value={option.value} title={option.title}>{option.label}</option>{/each}
  </select>
  <input
    id="{id}-value"
    class="field value num"
    inputmode="numeric"
    placeholder="any"
    disabled={byKind}
    value={criterion.value ?? ""}
    oninput={(e) => update({ value: e.currentTarget.value })}
  />
  {#if criterion.comparison === "div" && !byKind}
    <label class="remainder">
      remainder
      <input
        class="field num"
        inputmode="numeric"
        placeholder="0"
        title="-1 for any remainder but 0"
        value={criterion.remainder ?? ""}
        oninput={(e) => {
          const n = Number.parseInt(e.currentTarget.value, 10);
          update({ remainder: Number.isNaN(n) ? undefined : n });
        }}
      />
    </label>
  {/if}
  <select
    class="field kind"
    aria-label="{label}: kind of number"
    value={criterion.type ?? "none"}
    onchange={(e) => update({ type: e.currentTarget.value as CriterionInput["type"] })}
  >
    {#each KINDS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
  </select>
</div>

<style>
  .row {
    display: grid;
    grid-template-columns: 9.5rem 3.5rem 6rem auto 1fr;
    align-items: center;
    gap: var(--space-2);
    font-size: var(--text-sm);
  }

  .row > label {
    color: var(--ink-muted);
  }

  .field {
    height: 2rem;
    font-size: var(--text-sm);
  }

  .kind {
    justify-self: start;
    max-width: 14rem;
  }

  .remainder {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    color: var(--ink-muted);
    font-size: var(--text-xs);
  }

  .remainder input {
    width: 3.5rem;
  }

  .row:not(:has(.remainder)) .kind {
    grid-column: 4 / 6;
  }
</style>
