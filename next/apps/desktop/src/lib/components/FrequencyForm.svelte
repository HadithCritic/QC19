<script lang="ts">
  import type { CriterionInput, FrequencyQueryInput, LetterMatchKind } from "../engine/types";
  import { COMPARISONS, FREQUENCY_UNITS, KINDS } from "../numberQuery";
  import { isSet } from "../searchRequest";
  import { search } from "../state/search.svelte";
  import CriterionField from "./CriterionField.svelte";

  // Find by letter frequency (Features.txt #14, #26, #54): for each unit, how
  // many of its letters are letters of the phrase, or how its letters relate
  // to the phrase's.

  const MATCHES: { value: LetterMatchKind; label: string }[] = [
    { value: "all", label: "have all the phrase's letters" },
    { value: "any", label: "have any of the phrase's letters" },
    { value: "only", label: "have only the phrase's letters" },
    { value: "none", label: "have none of the phrase's letters" },
  ];

  let phrase = $state("");
  let unit = $state<FrequencyQueryInput["unit"]>("verses");
  let runs = $state(false);
  let size = $state("");
  let uniqueLetters = $state(false);
  let mode = $state<"sum" | "match">("sum");
  let sum = $state<CriterionInput>({});
  let match = $state<LetterMatchKind>("all");

  const ready = $derived(phrase.trim() !== "" && (mode === "match" || isSet(sum)));

  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (!ready) return;
    const n = Number.parseInt(size, 10);
    const shape = runs && mode === "sum" && unit !== "sentences" ? "range" : "single";
    const query: FrequencyQueryInput = {
      unit,
      phrase: phrase.trim(),
      shape,
      ...(shape === "range" && n > 0 ? { size: n } : {}),
      uniqueLetters,
      ...(mode === "match" ? { match } : { sum }),
    };
    const target = sum.type && sum.type !== "none"
      ? KINDS.find((k) => k.value === sum.type)?.label
      : `${COMPARISONS.find((c) => c.value === (sum.comparison ?? "eq"))?.label} ${(sum.value ?? "").trim()}`;
    const what = mode === "match" ? `that ${MATCHES.find((m) => m.value === match)?.label}` : `with a letter frequency sum ${target}`;
    const runsLabel = shape === "range" ? " in runs" : "";
    await search.start({ kind: "frequency", query, label: `${unit}${runsLabel} ${what}, for ${phrase.trim()}` });
  }
</script>

<form class="frequency" onsubmit={submit}>
  <div class="line">
    <label class="visually-hidden" for="frequency-phrase">Letters or phrase</label>
    <input id="frequency-phrase" class="field term arabic" lang="ar" dir="rtl" bind:value={phrase} placeholder="حروف" autocomplete="off" />
    <label>
      in
      <select class="field" bind:value={unit}>
        {#each FREQUENCY_UNITS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
      </select>
    </label>
    <label class="check"><input type="checkbox" bind:checked={uniqueLetters} /> count each phrase letter once</label>
  </div>

  <div class="line" role="radiogroup" aria-label="Sum or letter match">
    <label class="check"><input type="radio" name="frequency-mode" value="sum" bind:group={mode} /> by the sum</label>
    <label class="check"><input type="radio" name="frequency-mode" value="match" bind:group={mode} /> by which letters</label>
  </div>

  {#if mode === "sum"}
    <CriterionField id="ff-sum" label="Sum" criterion={sum} allowSum={false} onchange={(c) => (sum = c)} />
    {#if unit !== "sentences"}
      <div class="line">
        <label class="check"><input type="checkbox" bind:checked={runs} /> in runs of neighbors</label>
        {#if runs}
          <label>of <input class="field size num" inputmode="numeric" bind:value={size} placeholder="any" /> each</label>
        {/if}
      </div>
    {/if}
  {:else}
    <div class="line">
      <select class="field" bind:value={match} aria-label="How the letters must match">
        {#each MATCHES as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
      </select>
      {#if !uniqueLetters && (match === "all" || match === "any")}
        <span class="hint">A letter counts when the unit has it as many times as the phrase does.</span>
      {/if}
    </div>
  {/if}

  <div class="line">
    <button type="submit" class="button primary" disabled={search.loading || !ready}>Find</button>
  </div>
</form>

<style>
  .frequency {
    display: grid;
    gap: var(--space-3);
  }

  .line {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-3);
    font-size: var(--text-sm);
  }

  .line .field {
    height: 2rem;
    font-size: var(--text-sm);
  }

  .term {
    width: 16rem;
    height: 2.5rem;
    font-size: 1.3rem;
  }

  .size {
    width: 5rem;
  }

  .check {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
  }

  .hint {
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }
</style>
