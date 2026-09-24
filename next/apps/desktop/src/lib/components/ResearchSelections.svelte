<script lang="ts">
  import { COUNTING_CHOICES, DEFAULT_COUNTING } from "../counting";
  import { describeError } from "../engine/client";
  import type { CountingOptions, ResearchSelection } from "../engine/types";
  import { app } from "../state/app.svelte";

  // Exact selections kept for research. Unlike a bookmark, each keeps the
  // value system and counting options it was studied under, and opening it
  // restores them, so a result can be reproduced as it was found.

  let error = $state<string | null>(null);

  function counting(options: CountingOptions | null): string {
    if (!options) return "";
    const changed = COUNTING_CHOICES.filter((c) => options[c.key] !== DEFAULT_COUNTING[c.key]);
    return changed.length === 0 ? "default counting" : changed.map((c) => `${c.label.toLowerCase()} ${options[c.key] ? "on" : "off"}`).join(", ");
  }

  async function edit(saved: ResearchSelection, title: string, note: string): Promise<void> {
    if (title === saved.title && note === saved.note) return;
    try {
      await app.editResearchSelection(saved, title.trim() || saved.address, note);
      error = null;
    } catch (e) {
      error = describeError(e);
    }
  }

  async function remove(saved: ResearchSelection): Promise<void> {
    try {
      await app.deleteResearchSelection(saved.id);
      error = null;
    } catch (e) {
      error = describeError(e);
    }
  }
</script>

<section aria-labelledby="research-title">
  <h2 id="research-title" class="eyebrow">Research selections</h2>
  {#if error}<p class="error" role="alert">{error}</p>{/if}
  {#if !app.researchSelections || app.researchSelections.length === 0}
    <p class="empty">None yet. Select words or letters in the reader and choose Save for research in the inspector.</p>
  {:else}
    <ul class="list">
      {#each app.researchSelections as saved (saved.id)}
        <li>
          <div class="head">
            <input
              class="title"
              value={saved.title}
              aria-label="Title of {saved.address}"
              maxlength="200"
              onchange={(e) => edit(saved, e.currentTarget.value, saved.note)}
            />
            {#if saved.selection}
              <button type="button" class="ref num" onclick={() => app.openResearchSelection(saved)}>{saved.address}</button>
            {:else}
              <span class="ref num missing">{saved.address}</span>
            {/if}
            <button type="button" class="link danger" onclick={() => remove(saved)} aria-label="Delete {saved.title}">Delete</button>
          </div>
          <p class="meta">
            {saved.valueSystem ?? "no system recorded"}{saved.counting ? ` · ${counting(saved.counting)}` : ""}
          </p>
          <textarea
            class="note"
            rows="2"
            placeholder="Notes"
            value={saved.note}
            aria-label="Notes on {saved.title}"
            onchange={(e) => edit(saved, saved.title, e.currentTarget.value)}
          ></textarea>
        </li>
      {/each}
    </ul>
  {/if}
</section>

<style>
  h2 {
    margin: 0 0 var(--space-2);
  }

  .list {
    display: grid;
    gap: var(--space-3);
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .list li {
    padding: var(--space-3) 0 0;
    border-top: 1px solid var(--rule);
  }

  .head {
    display: flex;
    align-items: baseline;
    gap: var(--space-3);
  }

  .title {
    flex: 1;
    min-width: 0;
    padding: 0.1rem 0.3rem;
    border: 1px solid transparent;
    border-radius: var(--radius-sm);
    background: none;
    color: var(--ink);
    font-size: var(--text-md);
    font-weight: 600;
  }

  .title:hover,
  .title:focus {
    border-color: var(--rule);
  }

  .ref {
    padding: 0.1rem 0.4rem;
    border: 0;
    border-radius: var(--radius-sm);
    background: none;
    color: var(--lapis);
    font-size: var(--text-sm);
  }

  .ref:hover {
    background: var(--lapis-soft);
  }

  .ref.missing {
    color: var(--ink-faint);
  }

  .meta {
    margin: var(--space-1) 0 var(--space-2) 0.3rem;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .note {
    width: 100%;
    padding: var(--space-2);
    border: 1px solid var(--rule);
    border-radius: var(--radius-sm);
    background: var(--paper);
    color: var(--ink);
    font: inherit;
    font-size: var(--text-sm);
    resize: vertical;
  }

  .empty {
    margin: 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .error {
    margin: 0 0 var(--space-2);
    color: var(--danger);
    font-size: var(--text-sm);
  }

  .link {
    padding: 0;
    border: 0;
    background: none;
    font-size: var(--text-xs);
    font-weight: 550;
  }

  .link.danger {
    color: var(--danger);
  }
</style>
