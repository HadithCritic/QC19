<script lang="ts">
  import { describeError, engine } from "../engine/client";
  import { app } from "../state/app.svelte";

  // Jump to a verse or range by typing it. "/" or Ctrl+K focuses the box from
  // anywhere, because navigation is the most frequent action in a reader.

  let text = $state("");
  let error = $state<string | null>(null);
  let busy = $state(false);
  let input: HTMLInputElement | undefined = $state();

  async function go(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (!text.trim()) return;
    busy = true;
    error = null;
    try {
      app.goTo(await engine.parseReference(text));
      text = "";
      input?.blur();
    } catch (e) {
      error = describeError(e);
    } finally {
      busy = false;
    }
  }

  function onGlobalKey(event: KeyboardEvent): void {
    const target = event.target as HTMLElement | null;
    const typing = target?.closest("input, textarea, select, [contenteditable]") !== null;
    const shortcut = (event.key === "k" && (event.ctrlKey || event.metaKey)) || (event.key === "/" && !typing);
    if (shortcut) {
      event.preventDefault();
      input?.focus();
      input?.select();
    }
  }
</script>

<svelte:window onkeydown={onGlobalKey} />

<form class="reference" onsubmit={go} role="search">
  <label class="visually-hidden" for="reference-input">Go to chapter or verse</label>
  <input
    id="reference-input"
    bind:this={input}
    bind:value={text}
    class="field num"
    placeholder="Go to 2:255"
    autocomplete="off"
    spellcheck="false"
    aria-describedby={error ? "reference-error" : undefined}
    aria-invalid={error !== null}
    oninput={() => (error = null)}
    onkeydown={(e) => e.key === "Escape" && (text = "", (error = null), input?.blur())}
  />
  <kbd aria-hidden="true">/</kbd>
  {#if error}
    <p id="reference-error" class="error" role="alert">{error}</p>
  {/if}
  <button type="submit" class="visually-hidden" disabled={busy}>Go</button>
</form>

<style>
  .reference {
    position: relative;
    width: 13rem;
  }

  input {
    width: 100%;
    height: 2rem;
    padding-inline-end: 2rem;
    font-size: var(--text-sm);
    background: var(--surface-sunk);
    border-color: transparent;
  }

  input:focus-visible {
    background: var(--surface);
  }

  input[aria-invalid="true"] {
    border-color: var(--danger);
  }

  kbd {
    position: absolute;
    inset-inline-end: 0.5rem;
    top: 50%;
    transform: translateY(-50%);
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    color: var(--ink-faint);
    border: 1px solid var(--rule-strong);
    border-radius: 3px;
    padding: 0 0.3rem;
    pointer-events: none;
  }

  .error {
    position: absolute;
    top: calc(100% + 6px);
    inset-inline-start: 0;
    z-index: 20;
    width: max-content;
    max-width: 22rem;
    margin: 0;
    padding: var(--space-2) var(--space-3);
    font-size: var(--text-sm);
    color: var(--danger);
    background: var(--surface);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-pop);
  }
</style>
