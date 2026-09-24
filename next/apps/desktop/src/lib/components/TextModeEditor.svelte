<script lang="ts">
  import Notice from "./Notice.svelte";
  import { describeError, engine } from "../engine/client";
  import type { TextMode, TextModes } from "../engine/types";
  import { app } from "../state/app.svelte";
  import { humanize } from "../systems";

  // The reader's own text modes (Features.txt #72). A mode starts from a
  // stock one, adds find-and-replace rules, and gets every value system its
  // base has under its own name.

  let list = $state<TextModes | null>(null);
  let loadError = $state<string | null>(null);
  let draft = $state<TextMode | null>(null);
  /** The name the draft was opened under, so a rename replaces it. */
  let editing = $state<string | null>(null);
  let saveError = $state<string | null>(null);
  let busy = $state(false);

  async function load(): Promise<void> {
    try {
      list = await engine.textModes();
      loadError = null;
    } catch (e) {
      loadError = describeError(e);
    }
  }

  $effect(() => {
    void load();
  });

  function startNew(): void {
    editing = null;
    saveError = null;
    draft = { name: "", base: list?.bases.includes("Simplified29") ? "Simplified29" : (list?.bases[0] ?? ""), rules: [{ find: "", replace: "" }], description: "" };
  }

  function startEdit(mode: TextMode): void {
    editing = mode.name;
    saveError = null;
    draft = { ...mode, rules: mode.rules.map((r) => ({ ...r })) };
  }

  async function save(): Promise<void> {
    if (!draft) return;
    busy = true;
    try {
      const mode = { ...draft, rules: draft.rules.filter((r) => r.find !== "" || r.replace !== "") };
      await engine.saveTextMode(mode);
      if (editing !== null && editing !== mode.name) await engine.deleteTextMode(editing);
      draft = null;
      editing = null;
      saveError = null;
      await Promise.all([load(), app.reloadSystems()]);
    } catch (e) {
      saveError = describeError(e);
    } finally {
      busy = false;
    }
  }

  async function remove(name: string): Promise<void> {
    busy = true;
    try {
      await engine.deleteTextMode(name);
      if (editing === name) draft = null;
      await Promise.all([load(), app.reloadSystems()]);
    } catch (e) {
      loadError = describeError(e);
    } finally {
      busy = false;
    }
  }
</script>

<section aria-labelledby="text-modes-title">
  <div class="head">
    <h2 id="text-modes-title" class="eyebrow">Text modes</h2>
    {#if list && !draft}
      <button type="button" class="link" onclick={startNew}>New text mode</button>
    {/if}
  </div>
  <p class="hint">
    A text mode of your own starts from a stock one and changes letters before they are counted, to count the way a source did.
    Its base's rules run first, then yours, then the base's letter forms. It gets every value system its base has, under its own
    name, in the system picker.
  </p>

  {#if loadError}
    <Notice tone="error" title="Text modes could not be loaded" detail={loadError} />
  {:else if list}
    {#if list.modes.length === 0 && !draft}
      <p class="empty">No text modes of your own yet.</p>
    {:else if list.modes.length > 0}
      <ul class="modes">
        {#each list.modes as mode (mode.name)}
          <li>
            <span class="name">{mode.name}</span>
            <span class="meta">on {humanize(mode.base)}, {mode.rules.length} {mode.rules.length === 1 ? "rule" : "rules"}</span>
            <p class="description">{mode.description}</p>
            <button type="button" class="link" disabled={busy} onclick={() => startEdit(mode)}>Edit</button>
            <button type="button" class="link danger" disabled={busy} onclick={() => remove(mode.name)} aria-label="Delete the text mode {mode.name}">Delete</button>
          </li>
        {/each}
      </ul>
    {/if}

    {#if draft}
      <form
        class="editor"
        onsubmit={(e) => {
          e.preventDefault();
          void save();
        }}
      >
        <div class="row">
          <label>
            <span>Name</span>
            <input class="field" bind:value={draft.name} placeholder="TaaAsHaa" maxlength="64" pattern="[A-Za-z0-9]+" required />
          </label>
          <label>
            <span>Starts from</span>
            <select bind:value={draft.base}>
              {#each list.bases as base (base)}
                <option value={base}>{humanize(base)}</option>
              {/each}
            </select>
          </label>
        </div>
        <label class="wide">
          <span>What it changes, and why</span>
          <textarea class="field" rows="2" bind:value={draft.description}></textarea>
        </label>

        <fieldset>
          <legend>Rules, applied in order to every occurrence</legend>
          {#each draft.rules as rule, i (i)}
            <div class="rule">
              <input class="field arabic" lang="ar" dir="rtl" bind:value={rule.find} aria-label="Rule {i + 1}: find" placeholder="find" />
              <span class="arrow" aria-hidden="true">becomes</span>
              <input class="field arabic" lang="ar" dir="rtl" bind:value={rule.replace} aria-label="Rule {i + 1}: replace with" placeholder="(nothing)" />
              <button type="button" class="link danger" onclick={() => draft!.rules.splice(i, 1)} disabled={draft.rules.length === 1} aria-label="Remove rule {i + 1}">Remove</button>
            </div>
          {/each}
          <button type="button" class="link" onclick={() => draft!.rules.push({ find: "", replace: "" })}>Add a rule</button>
        </fieldset>

        {#if saveError}
          <Notice tone="error" title="The text mode was not saved" detail={saveError} />
        {/if}
        <div class="actions">
          <button type="submit" class="button primary" disabled={busy}>Save</button>
          <button type="button" class="button" onclick={() => (draft = null)}>Cancel</button>
        </div>
      </form>
    {/if}
  {/if}
</section>

<style>
  .head {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
  }

  h2 {
    margin: 0 0 var(--space-2);
  }

  .hint,
  .empty {
    margin: 0 0 var(--space-3);
    font-size: var(--text-sm);
    color: var(--ink-muted);
    max-width: 70ch;
  }

  .modes {
    margin: 0 0 var(--space-3);
    padding: 0;
    list-style: none;
  }

  .modes li {
    display: grid;
    grid-template-columns: auto auto 1fr auto auto;
    align-items: baseline;
    gap: var(--space-3);
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
  }

  .name {
    font-weight: 600;
  }

  .meta {
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  .description {
    margin: 0;
    font-size: var(--text-sm);
    overflow-wrap: anywhere;
  }

  .editor {
    display: grid;
    gap: var(--space-3);
    padding: var(--space-4);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md, 8px);
    background: var(--surface);
  }

  .row {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-4);
  }

  label {
    display: grid;
    gap: var(--space-1);
    font-size: var(--text-sm);
  }

  label > span,
  legend {
    font-size: var(--text-xs);
    font-weight: 550;
    color: var(--ink-muted);
  }

  .wide textarea {
    width: 100%;
    resize: vertical;
  }

  fieldset {
    display: grid;
    gap: var(--space-2);
    justify-items: start;
    margin: 0;
    padding: 0;
    border: 0;
  }

  .rule {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-2);
  }

  .rule input {
    width: 9rem;
    font-size: 1.1rem;
  }

  .arrow {
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  .actions {
    display: flex;
    gap: var(--space-2);
  }

  .link {
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font-size: var(--text-xs);
    font-weight: 550;
  }

  .link:disabled {
    color: var(--ink-faint);
    cursor: default;
  }

  .link.danger {
    color: var(--danger);
  }
</style>
