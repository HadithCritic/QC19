<script lang="ts">
  import DisplaySettings from "../lib/components/DisplaySettings.svelte";
  import NumberChip from "../lib/components/NumberChip.svelte";
  import NumberDetail from "../lib/components/NumberDetail.svelte";
  import NumberFacts from "../lib/components/NumberFacts.svelte";
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine } from "../lib/engine/client";
  import type { NumberDetails, NumberInfo } from "../lib/engine/types";
  import { fromRadix } from "../lib/numberDisplay";
  import { CLASS_NAMES, CLASS_RULES } from "../lib/numbers";
  import { app } from "../lib/state/app.svelte";

  // Look up any whole number: its class, position among primes or composites,
  // digit sum and root, and factors.

  const EXAMPLES = ["8317", "19", "114", "6236", "729139"];
  const CODES = ["U", "AP", "XP", "AC", "XC"] as const;

  let text = $state("");
  let info = $state<NumberInfo | null>(null);
  let details = $state<NumberDetails | null>(null);
  let error = $state<string | null>(null);
  let loading = $state(false);

  async function analyze(value: string): Promise<void> {
    text = value;
    if (!value.trim()) return;
    loading = true;
    error = null;
    try {
      // A number typed while another base is chosen is read in that base.
      const decimal = app.radix === 10 ? value : fromRadix(value.replace(/,/g, ""), app.radix);
      if (decimal === null) throw new Error(`"${value}" is not a number in base ${app.radix}.`);
      [info, details] = await Promise.all([engine.analyzeNumber(decimal), engine.numberDetails(decimal)]);
    } catch (e) {
      info = null;
      details = null;
      error = describeError(e);
    } finally {
      loading = false;
    }
  }

  // A number sent from another view (a total, a value) is looked up at once, in decimal.
  $effect(() => {
    const sent = app.numberToOpen;
    if (sent === null) return;
    app.numberToOpen = null;
    void analyzeDecimal(sent);
  });

  async function analyzeDecimal(value: string): Promise<void> {
    loading = true;
    error = null;
    text = value;
    try {
      [info, details] = await Promise.all([engine.analyzeNumber(value), engine.numberDetails(value)]);
    } catch (e) {
      info = null;
      details = null;
      error = describeError(e);
    } finally {
      loading = false;
    }
  }

  function submit(event: SubmitEvent): void {
    event.preventDefault();
    void analyze(text);
  }
</script>

<section class="numbers" aria-labelledby="numbers-title">
  <header>
    <h1 id="numbers-title">Look up a number</h1>
    <form onsubmit={submit}>
      <label class="visually-hidden" for="number-input">Whole number</label>
      <input id="number-input" class="field num" inputmode={app.radix > 10 ? "text" : "numeric"} bind:value={text} placeholder="8317" autocomplete="off" />
      <button type="submit" class="button primary" disabled={loading || !text.trim()}>Look up</button>
    </form>
    <DisplaySettings />
    <p class="examples">
      Try
      {#each EXAMPLES as example (example)}
        <button type="button" class="example num" onclick={() => analyze(example)}>{example}</button>
      {/each}
    </p>
  </header>

  <div class="body" aria-live="polite">
    {#if error}
      <Notice tone="error" title="That number could not be analyzed" detail={error} />
    {:else if info}
      <div class="result">
        <NumberChip value={info.value} code={info.code} size="lg" />
        <NumberDetail {info} />
        {#if details}<NumberFacts {details} />{/if}
      </div>
    {:else}
      <Notice title="Enter a whole number" detail="Negative numbers are classified by their magnitude, as the original software does." />
    {/if}

    <section class="key" aria-labelledby="key-title">
      <h2 id="key-title" class="eyebrow">Classes</h2>
      <dl>
        {#each CODES as code (code)}
          <div data-class={code}>
            <dt><span class="swatch" aria-hidden="true"></span><span class="num">{code}</span> {CLASS_NAMES[code]}</dt>
            <dd>{CLASS_RULES[code]}</dd>
          </div>
        {/each}
      </dl>
    </section>
  </div>
</section>

<style>
  .numbers {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
    padding: var(--space-5) var(--space-6) var(--space-4);
    border-bottom: 1px solid var(--rule);
    background: var(--surface);
  }

  h1 {
    margin: 0 0 var(--space-3);
    font-size: var(--text-xl);
    font-weight: 600;
    letter-spacing: -0.015em;
  }

  form {
    display: flex;
    gap: var(--space-3);
  }

  form input {
    width: 16rem;
    height: 2.5rem;
    font-size: var(--text-lg);
  }

  form .button {
    height: 2.5rem;
  }

  .examples {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-2);
    margin: var(--space-3) 0 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .example {
    padding: 0.1rem 0.5rem;
    border: 1px solid var(--rule-strong);
    border-radius: 999px;
    background: var(--surface);
    font-size: var(--text-xs);
  }

  .example:hover {
    border-color: var(--lapis);
    color: var(--lapis);
  }

  .body {
    overflow-y: auto;
    padding: var(--space-5) var(--space-6) var(--space-7);
  }

  .result {
    display: grid;
    gap: var(--space-4);
    max-width: 30rem;
  }

  .key {
    max-width: 30rem;
    margin-top: var(--space-7);
  }

  .key dl {
    margin: var(--space-2) 0 0;
  }

  .key div {
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
    font-size: var(--text-sm);
  }

  .key dt {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    font-weight: 600;
  }

  .key dd {
    margin: 0 0 0 1.4rem;
    color: var(--ink-muted);
  }

  .swatch {
    width: 0.65rem;
    height: 0.65rem;
    border-radius: 2px;
    background: var(--class);
  }
</style>
