<script lang="ts">
  import { untrack } from "svelte";
  import { describeError } from "../engine/client";
  import type { VerseRange } from "../engine/types";
  import { app } from "../state/app.svelte";

  // Bookmark the selection and keep a note on it (Features.txt #70). The note
  // saves itself shortly after typing stops, as the original's auto-save did.

  interface Props {
    range: VerseRange;
  }

  let { range }: Props = $props();

  const SAVE_DELAY_MS = 600;

  const bookmark = $derived(app.bookmarkFor(range));
  let note = $state("");
  let status = $state<"idle" | "saving" | "saved" | "error">("idle");
  let error = $state<string | null>(null);
  let timer: ReturnType<typeof setTimeout> | undefined;

  // Load the note when the selection moves to another bookmark. Only the
  // bookmark's identity is tracked: a save that updates the stored note must
  // not overwrite what is being typed meanwhile.
  $effect(() => {
    const id = bookmark?.id;
    untrack(() => {
      note = id === undefined ? "" : (app.bookmarkFor(range)?.note ?? "");
      status = "idle";
    });
  });

  async function save(text: string): Promise<void> {
    status = "saving";
    try {
      await app.saveBookmark(range, text);
      status = "saved";
      error = null;
    } catch (e) {
      status = "error";
      error = describeError(e);
    }
  }

  function onInput(): void {
    clearTimeout(timer);
    status = "idle";
    timer = setTimeout(() => void save(note), SAVE_DELAY_MS);
  }

  async function toggle(): Promise<void> {
    clearTimeout(timer);
    if (bookmark) {
      try {
        await app.deleteBookmark(bookmark.id);
        note = "";
      } catch (e) {
        error = describeError(e);
      }
    } else {
      await save(note);
    }
  }
</script>

{#if app.bookmarks !== null}
  <section class="bookmark" aria-label="Bookmark">
    <button type="button" class="toggle" class:on={bookmark !== undefined} aria-pressed={bookmark !== undefined} onclick={toggle}>
      <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 3h12v18l-6-4-6 4V3Z" /></svg>
      {bookmark ? "Bookmarked" : "Bookmark"}
    </button>
    {#if bookmark}
      <label class="visually-hidden" for="bookmark-note">Note</label>
      <textarea id="bookmark-note" rows="3" bind:value={note} oninput={onInput} placeholder="Add a note" maxlength="10000"></textarea>
      <p class="status" aria-live="polite">
        {#if status === "saving"}Saving{:else if status === "saved"}Saved{:else if status === "error"}{error}{/if}
      </p>
    {:else if error}
      <p class="status error">{error}</p>
    {/if}
  </section>
{/if}

<style>
  .bookmark {
    display: grid;
    gap: var(--space-2);
  }

  .toggle {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    justify-self: start;
    height: 1.9rem;
    padding: 0 var(--space-3);
    border: 1px solid var(--rule-strong);
    border-radius: var(--radius-md);
    background: var(--surface);
    font-size: var(--text-sm);
    font-weight: 550;
  }

  .toggle svg {
    width: 0.95rem;
    height: 0.95rem;
    fill: none;
    stroke: currentColor;
    stroke-width: 2;
  }

  .toggle.on {
    border-color: var(--gilt);
    color: var(--gilt);
  }

  .toggle.on svg {
    fill: currentColor;
  }

  textarea {
    width: 100%;
    padding: var(--space-2) var(--space-3);
    border: 1px solid var(--rule-strong);
    border-radius: var(--radius-md);
    background: var(--surface);
    font-size: var(--text-sm);
    resize: vertical;
  }

  .status {
    min-height: 1em;
    margin: 0;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .status.error {
    color: var(--danger);
  }
</style>
