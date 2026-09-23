<script lang="ts">
  import { BASE_LETTERS, backspaceAt, extrasFor, insertAt } from "../palette";

  // Features.txt #46: letters to type a search with, for a reader without an
  // Arabic keyboard. Only characters the active text mode keeps are offered,
  // so a search is never built from one it has already folded away.

  interface Props {
    value: string;
    input: HTMLInputElement | undefined;
    textMode: string;
    roots?: boolean;
  }

  let { value = $bindable(), input, textMode, roots = false }: Props = $props();

  const extras = $derived(extrasFor(textMode, roots));

  function apply(edit: (text: string, start: number, end: number) => { text: string; caret: number }): void {
    const start = input?.selectionStart ?? value.length;
    const end = input?.selectionEnd ?? value.length;
    const next = edit(value, start, end);
    value = next.text;
    // Put the caret back after Svelte writes the new value.
    queueMicrotask(() => {
      input?.focus();
      input?.setSelectionRange(next.caret, next.caret);
    });
  }

  const type = (char: string) => apply((t, s, e) => insertAt(t, s, e, char));
</script>

<div class="palette" role="group" aria-label="Letters">
  <div class="keys" dir="rtl" lang="ar">
    {#each BASE_LETTERS as letter (letter)}
      <button type="button" onclick={() => type(letter)}>{letter}</button>
    {/each}
  </div>
  {#if extras.length > 0}
    <div class="keys extras" dir="rtl" lang="ar" aria-label="Further letters this text mode keeps">
      {#each extras as letter (letter)}
        <button type="button" onclick={() => type(letter)}>{letter}</button>
      {/each}
    </div>
  {/if}
  <div class="keys edit">
    <button type="button" class="wide" onclick={() => type(" ")}>Space</button>
    <button type="button" class="wide" aria-label="Delete the letter before the cursor" onclick={() => apply(backspaceAt)}>⌫</button>
  </div>
</div>

<style>
  .palette {
    display: grid;
    gap: var(--space-2);
    padding: var(--space-3);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md, 0.375rem);
    background: var(--surface);
  }

  .keys {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
  }

  .extras {
    padding-top: var(--space-2);
    border-top: 1px dashed var(--rule);
  }

  .edit {
    justify-content: flex-end;
  }

  button {
    min-width: 2.1rem;
    height: 2.1rem;
    border: 1px solid var(--rule);
    border-radius: 0.3rem;
    background: var(--surface-sunk);
    color: var(--ink);
    font-family: var(--font-arabic);
    font-size: var(--text-lg);
    cursor: pointer;
  }

  button:hover {
    border-color: var(--lapis);
  }

  button:active {
    background: var(--lapis-soft);
  }

  .extras button {
    border-color: var(--gilt);
  }

  .wide {
    padding: 0 var(--space-3);
    font-family: var(--font-ui);
    font-size: var(--text-sm);
  }
</style>
