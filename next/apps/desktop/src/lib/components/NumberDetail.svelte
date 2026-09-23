<script lang="ts">
  import type { NumberInfo } from "../engine/types";
  import { digitalRootIn, digitSumIn } from "../numberDisplay";
  import { CLASS_NAMES, CLASS_RULES, classLabel, familyLabel, formatFactors } from "../numbers";
  import { app } from "../state/app.svelte";

  interface Props {
    info: NumberInfo;
  }

  let { info }: Props = $props();

  const family = $derived(familyLabel(info));
  const ordinal = $derived(classLabel(info));
  const isPrime = $derived(info.factors?.length === 1);
  // In another base, digit sums are of the digits written in that base.
  const digitSum = $derived(app.radix === 10 ? info.digitSum : digitSumIn(info.value, app.radix));
  const digitalRoot = $derived(app.radix === 10 ? info.digitalRoot : digitalRootIn(info.value, app.radix));
</script>

<dl class="detail" data-class={info.code}>
  <div class="row class-row">
    <dt>Class</dt>
    <dd>
      <span class="swatch" aria-hidden="true"></span>
      <strong>{CLASS_NAMES[info.code]}</strong>
      <span class="rule">{CLASS_RULES[info.code]}</span>
    </dd>
  </div>
  <div class="row">
    <dt>Digit sum</dt>
    <dd class="num">{digitSum}{#if app.radix !== 10}<span class="muted"> in base {app.radix}</span>{/if}</dd>
  </div>
  <div class="row">
    <dt>Digital root</dt>
    <dd class="num">{digitalRoot}</dd>
  </div>
  {#if family}
    <div class="row">
      <dt>Position</dt>
      <dd class="num">
        {family}{#if ordinal}<span class="sep">·</span>{ordinal}{/if}
      </dd>
    </div>
  {/if}
  <div class="row">
    <dt>Factors</dt>
    <dd class="num">
      {#if info.factors === null}
        <span class="muted">Too large to factor here</span>
      {:else if info.factors.length === 0}
        <span class="muted">None</span>
      {:else if isPrime}
        <span class="muted">Prime</span>
      {:else}
        {formatFactors(info.factors)}
      {/if}
    </dd>
  </div>
</dl>

<style>
  .detail {
    display: grid;
    margin: 0;
    font-size: var(--text-sm);
  }

  .row {
    display: grid;
    grid-template-columns: 6.5rem 1fr;
    gap: var(--space-3);
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
  }

  dt {
    color: var(--ink-muted);
  }

  dd {
    margin: 0;
    min-width: 0;
    overflow-wrap: anywhere;
  }

  .class-row dd {
    display: grid;
    grid-template-columns: auto 1fr;
    column-gap: var(--space-2);
    align-items: center;
  }

  .swatch {
    width: 0.65rem;
    height: 0.65rem;
    border-radius: 2px;
    background: var(--class);
  }

  .rule {
    grid-column: 2;
    color: var(--ink-muted);
  }

  .sep {
    margin-inline: 0.4em;
    color: var(--ink-faint);
  }

  .muted {
    color: var(--ink-muted);
    font-family: var(--font-ui);
  }
</style>
