<script lang="ts">
  import type { ClassCode } from "../engine/types";
  import { isDivisible, powerMark, toRadix } from "../numberDisplay";
  import { CLASS_NAMES } from "../numbers";
  import { app } from "../state/app.svelte";

  interface Props {
    value: string;
    /** The number's class; without one the chip shows the number plainly. */
    code?: ClassCode | null;
    size?: "sm" | "md" | "lg";
    /** When given, the chip is a button that opens the number. */
    onselect?: (value: string) => void;
  }

  let { value, code = null, size = "md", onselect }: Props = $props();

  const POWER_NAMES = { 2: "square", 3: "cube", 5: "fifth power", 7: "seventh power" } as const;

  // Shown in the reader's base; the decimal stays in the label and tooltip.
  const shown = $derived(app.radix === 10 ? value : toRadix(value, app.radix));
  const divisible = $derived(isDivisible(value, app.divisor));
  const power = $derived(powerMark(value));
  const extras = $derived(
    [app.radix === 10 ? "" : `${value} in decimal`, divisible ? `divisible by ${app.divisor}` : "", power ? `a ${POWER_NAMES[power]}` : ""]
      .filter(Boolean)
      .join(", "),
  );
  const label = $derived([value, code ? CLASS_NAMES[code].toLowerCase() : "", extras].filter(Boolean).join(", "));
</script>

{#if onselect}
  <button
    type="button"
    class="chip {size}"
    class:divisible
    data-class={code}
    data-power={power}
    title={extras || undefined}
    aria-label="{label}. Open number"
    onclick={() => onselect(value)}
  >
    <span class="value num">{shown}</span>{#if app.radix !== 10}<sub class="base num">{app.radix}</sub>{/if}
    {#if code}<span class="code" aria-hidden="true">{code}</span>{/if}
  </button>
{:else}
  <span class="chip {size}" class:divisible data-class={code} data-power={power} title={extras || undefined} aria-label={label}>
    <span class="value num">{shown}</span>{#if app.radix !== 10}<sub class="base num">{app.radix}</sub>{/if}
    {#if code}<span class="code" aria-hidden="true">{code}</span>{/if}
  </span>
{/if}

<style>
  .chip {
    display: inline-flex;
    align-items: baseline;
    gap: 0.4em;
    padding: 0;
    border: 0;
    background: none;
    color: var(--ink);
    text-align: start;
  }

  button.chip {
    border-radius: var(--radius-sm);
  }

  button.chip:hover .value {
    text-decoration: underline;
    text-decoration-color: var(--class);
    text-underline-offset: 0.2em;
  }

  .value {
    font-weight: 500;
  }

  /* Divisible by the reader's divisor: a tinted band behind the digits. */
  .divisible .value {
    padding: 0 0.2em;
    border-radius: 3px;
    background: var(--divisible);
  }

  /* A square, cube, 5th or 7th power: a dotted underline. */
  .chip[data-power] .value {
    text-decoration: underline dotted var(--ink-muted);
    text-underline-offset: 0.2em;
  }

  .base {
    font-size: 0.6em;
    color: var(--ink-faint);
  }

  .code {
    font-family: var(--font-mono);
    font-size: 0.72em;
    font-weight: 500;
    line-height: 1;
    padding: 0.2em 0.35em 0.15em;
    border-radius: 3px;
    color: var(--class);
    background: color-mix(in srgb, var(--class) 13%, transparent);
    box-shadow: inset 0 0 0 1px color-mix(in srgb, var(--class) 30%, transparent);
  }

  .sm {
    font-size: var(--text-sm);
  }

  .md {
    font-size: var(--text-md);
  }

  .lg {
    font-size: var(--text-2xl);
    letter-spacing: -0.02em;
  }

  .lg .value {
    font-weight: 400;
  }

  .lg .code {
    font-size: 0.36em;
    transform: translateY(-0.9em);
  }
</style>
