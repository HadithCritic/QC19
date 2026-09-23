<script lang="ts">
  import DisplaySettings from "../lib/components/DisplaySettings.svelte";
  import NumberChip from "../lib/components/NumberChip.svelte";
  import NumberDetail from "../lib/components/NumberDetail.svelte";
  import NumberFacts from "../lib/components/NumberFacts.svelte";
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine } from "../lib/engine/client";
  import type { NumberDetails, NumberInfo } from "../lib/engine/types";
  import { ExpressionError, evaluate, isExpression } from "../lib/expression";
  import { fromRadix } from "../lib/numberDisplay";
  import { CLASS_NAMES, CLASS_RULES } from "../lib/numbers";
  import { app } from "../lib/state/app.svelte";
  import { humanize } from "../lib/systems";

  // Look up any whole number: its class, position among primes or composites,
  // digit sum and root, and factors. The box also calculates (Features.txt
  // #30): an expression such as 19^2 or 114C2, or Arabic text, which is
  // valued in the current letter-value system.

  const EXAMPLES = ["8317", "19", "114", "6236", "729139", "19*142", "2^19-1", "بسم الله الرحمن الرحيم"];
  const CODES = ["U", "AP", "XP", "AC", "XC"] as const;

  let text = $state("");
  let info = $state<NumberInfo | null>(null);
  let details = $state<NumberDetails | null>(null);
  let error = $state<string | null>(null);
  let loading = $state(false);

  const LONG_MAX = 9223372036854775807n;

  // What was calculated, when the input was more than a plain number.
  let worked = $state<string | null>(null);

  // Arabic letters and marks: text to value, as the original's value box does.
  const ARABIC = /[؀-ۿ]/;

  /** The input as a decimal whole number, and how it was arrived at. */
  async function resolve(value: string): Promise<{ decimal: string; how: string | null }> {
    const input = value.trim();
    if (ARABIC.test(input)) {
      const system = app.valueSystem;
      const [row] = await engine.textValues(input, [system]);
      if (!row) throw new Error(`The text could not be valued in ${system}.`);
      return { decimal: row.value.value, how: `The value of the text in ${humanize(system)}, ${row.letterCount} letters.` };
    }
    if (isExpression(input, app.radix)) {
      const result = evaluate(input, app.radix);
      if (result.kind === "real") {
        throw new ExpressionError(`${input} = ${result.value.toLocaleString("en-US", { maximumFractionDigits: 12 })}, which is not a whole number.`);
      }
      const shown = `${input} = ${result.value.toLocaleString("en-US")}`;
      // The engine classifies numbers that fit in 64 bits; a larger exact
      // result is still worth seeing, with its divisibility by the divisor.
      if (result.value > LONG_MAX || result.value < -LONG_MAX) {
        const divides = result.value % BigInt(app.divisor) === 0n;
        throw new ExpressionError(
          `${shown}. That is too large to classify (the limit is ${LONG_MAX.toLocaleString("en-US")}); it is ${divides ? "" : "not "}divisible by ${app.divisor}.`,
        );
      }
      return { decimal: result.value.toString(), how: shown };
    }
    // A number typed while another base is chosen is read in that base.
    const decimal = app.radix === 10 ? input.replace(/,/g, "") : fromRadix(input.replace(/,/g, ""), app.radix);
    if (decimal === null) throw new Error(`"${value}" is not a number in base ${app.radix}.`);
    return { decimal, how: null };
  }

  async function analyze(value: string): Promise<void> {
    text = value;
    if (!value.trim()) return;
    loading = true;
    error = null;
    try {
      const { decimal, how } = await resolve(value);
      worked = how;
      [info, details] = await Promise.all([engine.analyzeNumber(decimal), engine.numberDetails(decimal)]);
    } catch (e) {
      info = null;
      details = null;
      error = e instanceof ExpressionError ? e.message : describeError(e);
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
    worked = null;
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
      <label class="visually-hidden" for="number-input">Whole number, expression or Arabic text</label>
      <input id="number-input" class="field num" dir="auto" bind:value={text} placeholder="8317 or 19*142" autocomplete="off" aria-describedby="number-help" />
      <button type="submit" class="button primary" disabled={loading || !text.trim()}>Look up</button>
    </form>
    <p id="number-help" class="help">
      A number, an expression (+ − × ÷, \ for whole division, %, ^, !, nPk, nCk, sqrt(), pi, e, phi) or Arabic text.
      Numbers are read in base {app.radix}.
    </p>
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
        {#if worked}<p class="worked">{worked}</p>{/if}
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

  .help {
    margin: var(--space-2) 0 0;
    color: var(--ink-muted);
    font-size: var(--text-sm);
  }

  .worked {
    margin: 0;
    color: var(--ink-muted);
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    overflow-wrap: anywhere;
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
