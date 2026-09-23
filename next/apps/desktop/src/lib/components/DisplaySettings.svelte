<script lang="ts">
  import { DEFAULT_DIVISOR, MAX_DIVISOR, MAX_RADIX, MIN_DIVISOR, MIN_RADIX } from "../numberDisplay";
  import { app } from "../state/app.svelte";

  // The divisor numbers are marked by (Features.txt #16) and the base they
  // are shown in (#71). As in the original, the arrows wrap past either end;
  // clicking the divisor's name resets it to 19, and the base's switches
  // between 10 and 19.
</script>

<div class="settings" role="group" aria-label="How numbers are shown">
  <div class="stepper">
    <button type="button" class="name" title="Reset to {DEFAULT_DIVISOR}" onclick={() => app.setDivisor(DEFAULT_DIVISOR)}>Divisor</button>
    <button type="button" class="step" aria-label="Divisor down" onclick={(e) => app.setDivisor(app.divisor - (e.shiftKey ? 10 : 1))}>−</button>
    <input
      class="field num"
      type="number"
      min={MIN_DIVISOR}
      max={MAX_DIVISOR}
      aria-label="Divisor"
      value={app.divisor}
      onchange={(e) => {
        const n = Number.parseInt(e.currentTarget.value, 10);
        if (!Number.isNaN(n)) app.setDivisor(Math.min(MAX_DIVISOR, Math.max(MIN_DIVISOR, n)));
      }}
    />
    <button type="button" class="step" aria-label="Divisor up" onclick={(e) => app.setDivisor(app.divisor + (e.shiftKey ? 10 : 1))}>+</button>
  </div>
  <div class="stepper">
    <button type="button" class="name" title="Switch between base 10 and 19" onclick={() => app.setRadix(app.radix === 10 ? 19 : 10)}>Base</button>
    <button type="button" class="step" aria-label="Base down" onclick={() => app.setRadix(app.radix - 1)}>−</button>
    <input
      class="field num"
      type="number"
      min={MIN_RADIX}
      max={MAX_RADIX}
      aria-label="Base"
      value={app.radix}
      onchange={(e) => {
        const n = Number.parseInt(e.currentTarget.value, 10);
        if (!Number.isNaN(n)) app.setRadix(Math.min(MAX_RADIX, Math.max(MIN_RADIX, n)));
      }}
    />
    <button type="button" class="step" aria-label="Base up" onclick={() => app.setRadix(app.radix + 1)}>+</button>
  </div>
  <p class="hint">Marked: <span class="sample">divisible by {app.divisor}</span>. Shift with the arrows steps the divisor by 10.</p>
</div>

<style>
  .settings {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-4);
    font-size: var(--text-sm);
  }

  .stepper {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
  }

  .name {
    padding: 0 var(--space-1);
    border: 0;
    background: none;
    color: var(--ink-muted);
    font-size: var(--text-sm);
  }

  .name:hover {
    color: var(--lapis);
  }

  .step {
    width: 1.75rem;
    height: 1.75rem;
    padding: 0;
    border: 1px solid var(--rule-strong);
    border-radius: var(--radius-sm);
    background: var(--surface);
  }

  input {
    width: 4.5rem;
    height: 1.75rem;
    font-size: var(--text-sm);
    text-align: center;
  }

  .hint {
    margin: 0;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .sample {
    padding: 0 0.3em;
    border-radius: 3px;
    background: var(--divisible);
    color: var(--ink);
  }
</style>
