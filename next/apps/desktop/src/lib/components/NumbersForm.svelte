<script lang="ts">
  import type { CriterionInput, NumberField, NumberQueryInput, UnitKind, UnitShape } from "../engine/types";
  import { COMPARISONS, FIELD_LABELS, KINDS, SHAPES, UNITS, fieldsFor, numberScopesFor } from "../numberQuery";
  import { isSet } from "../searchRequest";
  import { search } from "../state/search.svelte";
  import CriterionField from "./CriterionField.svelte";

  // Find by numbers (Features.txt #25, #32 to #35): units, runs or sets of
  // them whose counts and value meet the constraints given.

  let unit = $state<UnitKind>("verses");
  let shape = $state<UnitShape>("single");
  let size = $state("");
  let numberScope = $state<"book" | "chapter" | "verse" | "">("");
  let criteria = $state<Partial<Record<NumberField, CriterionInput>>>({});

  const fields = $derived(fieldsFor(unit, shape));
  const scopes = $derived(numberScopesFor(unit));
  const shapes = $derived(unit === "sentences" ? SHAPES.filter((s) => s.value === "single") : SHAPES);
  const active = $derived(fields.filter((f) => isSet(criteria[f])));
  const sizeNeeded = $derived(shape === "set" && !/^\d+$/.test(size.trim()));

  $effect(() => {
    if (unit === "sentences") shape = "single";
  });

  function describeCriterion(field: NumberField, c: CriterionInput): string {
    const name = FIELD_LABELS[field].toLowerCase();
    if ((c.type ?? "none") !== "none") return `${name} ${KINDS.find((k) => k.value === c.type)?.label ?? c.type}`;
    const op = COMPARISONS.find((o) => o.value === (c.comparison ?? "eq"))?.label ?? "=";
    return `${name} ${op} ${(c.value ?? "").trim()}`;
  }

  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (active.length === 0 || sizeNeeded) return;
    const n = Number.parseInt(size, 10);
    const query: NumberQueryInput = {
      unit,
      shape,
      ...(shape !== "single" && n > 0 ? { size: n } : {}),
      ...(numberScope ? { numberScope } : {}),
      criteria: Object.fromEntries(active.map((f) => [f, criteria[f]])),
    };
    const label = `${UNITS.find((u) => u.value === unit)?.label}${shape === "single" ? "" : shape === "range" ? " in runs" : " in sets"} with ${active
      .map((f) => describeCriterion(f, criteria[f]!))
      .join(", ")}`;
    await search.start({ kind: "numbers", query, label });
  }
</script>

<form class="numbers" onsubmit={submit}>
  <div class="line">
    <label>
      Find
      <select class="field" bind:value={unit}>
        {#each UNITS as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
      </select>
    </label>
    <select class="field" bind:value={shape} aria-label="One at a time, in runs or in sets">
      {#each shapes as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
    </select>
    {#if shape !== "single"}
      <label>
        of
        <input class="field size num" inputmode="numeric" bind:value={size} placeholder={shape === "range" ? "any" : "how many"} />
        {shape === "range" ? "each (blank tries 1 to 29)" : "each"}
      </label>
    {/if}
  </div>

  <div class="criteria">
    {#each fields as field (field)}
      <CriterionField
        id="nf-{field}"
        label={FIELD_LABELS[field]}
        criterion={criteria[field] ?? {}}
        allowSum={field === "verses" || field === "words" || field === "letters"}
        onchange={(c) => (criteria = { ...criteria, [field]: c })}
      />
      {#if field === "number" && scopes.length > 0}
        <label class="scope">
          counted
          <select class="field" bind:value={numberScope}>
            <option value="">{scopes[0]?.label} (usual)</option>
            {#each scopes.slice(1) as option (option.value)}<option value={option.value}>{option.label}</option>{/each}
          </select>
          ; a negative number counts from the end
        </label>
      {/if}
    {/each}
  </div>

  <div class="line">
    <button type="submit" class="button primary" disabled={search.loading || active.length === 0 || sizeNeeded}>Find</button>
    {#if active.length === 0}<span class="hint">Set at least one number.</span>{/if}
    {#if sizeNeeded}<span class="hint">Say how many units each set has.</span>{/if}
    {#if active.length > 0}<button type="button" class="link" onclick={() => (criteria = {})}>Clear numbers</button>{/if}
  </div>
</form>

<style>
  .numbers {
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

  .size {
    width: 5rem;
  }

  .criteria {
    display: grid;
    gap: var(--space-2);
  }

  .scope {
    padding-left: 9.5rem;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .scope .field {
    height: 1.75rem;
    font-size: var(--text-xs);
  }

  .hint {
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .link {
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font-size: var(--text-xs);
    font-weight: 550;
  }
</style>
