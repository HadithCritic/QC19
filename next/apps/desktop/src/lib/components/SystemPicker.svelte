<script lang="ts">
  import { app } from "../state/app.svelte";
  import { humanize, partOptions, partsOf, withPart, type Part } from "../systems";

  // A value system is named TextMode_LetterOrder_LetterValue. Three linked
  // pickers replace one list of 285 (or 407) names, and each only offers
  // choices that combine into a real system.

  const parts = $derived(app.currentSystem ? partsOf(app.currentSystem) : null);
  const options = $derived(parts ? partOptions(app.systems, parts) : null);

  const LABELS: Record<Part, string> = { textMode: "Text", letterOrder: "Order", letterValue: "Values" };
  const ORDER: Part[] = ["textMode", "letterOrder", "letterValue"];

  function change(part: Part, value: string): void {
    if (!parts) return;
    const next = withPart(app.systems, parts, part, value);
    if (next) app.setSystem(next.name);
  }
</script>

{#if parts && options}
  <fieldset class="picker" aria-label="Letter-value system">
    {#each ORDER as part (part)}
      <label>
        <span class="eyebrow">{LABELS[part]}</span>
        <select class="field" aria-label="{LABELS[part]}" value={parts[part]} onchange={(e) => change(part, e.currentTarget.value)}>
          {#each options[part] as option (option)}
            <option value={option}>{humanize(option)}</option>
          {/each}
        </select>
      </label>
    {/each}
  </fieldset>
{/if}

<style>
  .picker {
    display: flex;
    gap: var(--space-2);
    margin: 0;
    padding: 0;
    border: 0;
    min-width: 0;
  }

  label {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }

  select {
    width: 9.5rem;
    height: 2rem;
    font-size: var(--text-sm);
    text-overflow: ellipsis;
  }

  @media (max-width: 1280px) {
    select {
      width: 7.75rem;
    }
  }

  @media (max-width: 1180px) {
    .eyebrow {
      display: none;
    }
  }
</style>
