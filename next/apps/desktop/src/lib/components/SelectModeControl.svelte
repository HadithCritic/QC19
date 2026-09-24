<script lang="ts">
  import type { SelectMode } from "../selection";
  import { app } from "../state/app.svelte";

  // What a click in the reader selects. Verse is the default; word and letter
  // make the text itself the place a research selection is drawn.

  const MODES: { id: SelectMode; label: string; hint: string }[] = [
    { id: "verse", label: "Verse", hint: "Click a verse to select it; shift-click to extend." },
    { id: "word", label: "Word", hint: "Click the first word, then the last; shift-click to extend. Esc clears." },
    { id: "letter", label: "Letter", hint: "Click the first letter, then the last; shift-click to extend. Esc clears." },
  ];
</script>

<div class="select-mode" role="radiogroup" aria-label="Select by">
  <span class="eyebrow">Select</span>
  {#each MODES as mode (mode.id)}
    <button
      type="button"
      role="radio"
      aria-checked={app.selectMode === mode.id}
      class:active={app.selectMode === mode.id}
      title={mode.hint}
      onclick={() => app.setSelectMode(mode.id)}>{mode.label}</button
    >
  {/each}
</div>

<style>
  .select-mode {
    display: inline-flex;
    align-items: center;
    gap: 2px;
    padding: 2px;
    border: 1px solid var(--rule);
    border-radius: 999px;
    background: var(--surface);
  }

  .eyebrow {
    padding: 0 var(--space-2);
  }

  button {
    padding: 0.2rem 0.7rem;
    border: 0;
    border-radius: 999px;
    background: none;
    color: var(--ink-muted);
    font-size: var(--text-xs);
    font-weight: 550;
  }

  button:hover {
    color: var(--ink);
  }

  button.active {
    background: var(--lapis-soft);
    color: var(--lapis);
  }

  button:focus-visible {
    box-shadow: var(--focus);
  }
</style>
