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
  <div class="system-bar" role="group" aria-label="Calculation system settings">
    {#each ORDER as part, index (part)}
      {#if index > 0}
        <div class="divider" aria-hidden="true"></div>
      {/if}
      <div class="system-segment">
        <span class="segment-label">{LABELS[part]}</span>
        <select
          class="segment-select"
          aria-label="{LABELS[part]} mode"
          value={parts[part]}
          onchange={(e) => change(part, e.currentTarget.value)}
        >
          {#each options[part] as option (option)}
            <option value={option}>{humanize(option)}</option>
          {/each}
        </select>
      </div>
    {/each}
  </div>
{/if}

<style>
  .system-bar {
    display: inline-flex;
    align-items: center;
    background: var(--surface-sunk);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md);
    padding: 2px;
    height: 2.125rem;
    box-shadow: 0 1px 2px rgb(0 0 0 / 0.03);
    min-width: 0;
  }

  .divider {
    width: 1px;
    height: 1.25rem;
    background: var(--rule);
    flex-shrink: 0;
  }

  .system-segment {
    display: flex;
    align-items: center;
    gap: var(--space-1);
    padding: 0 var(--space-2);
    height: 100%;
    border-radius: var(--radius-sm);
    transition: background-color var(--duration) var(--ease);
  }

  .system-segment:hover {
    background: color-mix(in srgb, var(--surface) 60%, transparent);
  }

  .system-segment:focus-within {
    background: var(--surface);
    box-shadow: 0 0 0 1px var(--lapis);
  }

  .segment-label {
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--ink-muted);
    user-select: none;
    flex-shrink: 0;
  }

  .segment-select {
    border: 0;
    background-color: transparent;
    padding: 0 1.25rem 0 var(--space-1);
    height: 100%;
    font-size: var(--text-xs);
    font-weight: 550;
    color: var(--ink);
    cursor: pointer;
    text-overflow: ellipsis;
    white-space: nowrap;
    outline: none;
    background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="%238a8075" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
    background-repeat: no-repeat;
    background-position: right 0.2rem center;
    background-size: 0.75rem;
    max-width: 8.5rem;
  }

  .segment-select:focus {
    color: var(--lapis);
  }

  @media (max-width: 1200px) {
    .segment-select {
      max-width: 6.5rem;
    }
  }

  @media (max-width: 1050px) {
    .segment-label {
      display: none;
    }
  }
</style>
