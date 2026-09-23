<script lang="ts">
  import type { NumberDetails, Pair } from "../engine/types";
  import { ordinal, toRadix } from "../numberDisplay";
  import { app } from "../state/app.svelte";

  // The original's further value facts: divisors, powers, Carmichael numbers,
  // 4n±1 forms with their square and cube splits (Features.txt #6, #7), and
  // Waleed's CP index chain (#9).

  interface Props {
    details: NumberDetails;
  }

  let { details }: Props = $props();

  const POWERS: Record<number, string> = {
    2: "a square",
    3: "a cube",
    4: "a 4th power",
    5: "a 5th power",
    6: "a 6th power",
    7: "a 7th power",
    8: "an 8th power",
    9: "a 9th power",
    10: "a 10th power",
  };

  function splits(pairs: Pair[], power: 2 | 3, sign: "+" | "−"): string[] {
    const exp = power === 2 ? "²" : "³";
    return pairs.map((p) => (sign === "+" ? `${p.a}${exp} + ${p.b}${exp}` : `${p.b}${exp} − ${p.a}${exp}`));
  }

  const all = $derived(
    details.splits
      ? [
          ...splits(details.splits.squareSums, 2, "+"),
          ...splits(details.splits.squareDifferences, 2, "−"),
          ...splits(details.splits.cubeSums, 3, "+"),
          ...splits(details.splits.cubeDifferences, 3, "−"),
        ]
      : [],
  );

  const kind = $derived(details.number.code === "AP" || details.number.code === "XP" ? "prime" : "composite");
  const shown = (n: number | string): string => toRadix(String(n), app.radix);
</script>

<dl class="facts">
  {#if details.divisorCount !== null}
    <div>
      <dt>Divisors</dt>
      <dd>
        <span class="num">{details.divisorCount}</span>, summing to <span class="num">{shown(details.divisorSum ?? "0")}</span>
        {#if details.divisors}<p class="list num">{details.divisors.map(shown).join(" · ")}</p>{/if}
      </dd>
    </div>
  {/if}

  {#if details.power || details.carmichael}
    <div>
      <dt>Also</dt>
      <dd>
        {[details.power ? POWERS[details.power] : "", details.carmichael ? "a Carmichael number" : ""].filter(Boolean).join(", ")}
      </dd>
    </div>
  {/if}

  {#if details.fourN}
    <div>
      <dt>Form</dt>
      <dd>
        <span class="num">4×{details.fourN.n} {details.fourN.form === "4n+1" ? "+" : "−"} 1</span>{#if details.fourN.ordinal !== null},
          the <span class="num">{ordinal(details.fourN.ordinal)}</span> {kind} of the form {details.fourN.form}{/if}
        {#if all.length > 0}<p class="list num">= {all.join(" = ")}</p>{/if}
      </dd>
    </div>
  {:else if all.length > 0}
    <div>
      <dt>Splits</dt>
      <dd><p class="list num">= {all.join(" = ")}</p></dd>
    </div>
  {/if}

  {#if details.chain}
    <div>
      <dt>CP index chain</dt>
      <dd>
        <span class="num">{details.chain.text}</span>
        <p class="list">
          length <span class="num">{details.chain.length}</span>, sum <span class="num">{shown(details.chain.sum)}</span>
        </p>
        <table class="bits">
          <thead><tr><th></th><th>left to right</th><th>right to left</th></tr></thead>
          <tbody>
            <tr><th>P = 0, C = 1</th><td class="num">{shown(details.chain.primesAsZero)}</td><td class="num">{shown(details.chain.primesAsZeroReversed)}</td></tr>
            <tr><th>P = 1, C = 0</th><td class="num">{shown(details.chain.primesAsOne)}</td><td class="num">{shown(details.chain.primesAsOneReversed)}</td></tr>
          </tbody>
        </table>
      </dd>
    </div>
  {/if}
</dl>

<style>
  .facts {
    display: grid;
    gap: var(--space-3);
    margin: 0;
  }

  .facts > div {
    display: grid;
    grid-template-columns: 8.5rem 1fr;
    gap: var(--space-3);
  }

  dt {
    font-size: var(--text-xs);
    font-weight: 550;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--ink-muted);
  }

  dd {
    margin: 0;
    font-size: var(--text-sm);
  }

  .list {
    margin: var(--space-1) 0 0;
    color: var(--ink-muted);
    overflow-wrap: anywhere;
  }

  .bits {
    margin-top: var(--space-2);
    border-collapse: collapse;
    font-size: var(--text-xs);
  }

  .bits th,
  .bits td {
    padding: 0.15rem 0.6rem 0.15rem 0;
    text-align: start;
    font-weight: 400;
  }

  .bits thead th {
    color: var(--ink-faint);
  }
</style>
