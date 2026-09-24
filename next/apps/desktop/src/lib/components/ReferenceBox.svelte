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
      const parsed = await engine.parseReference(text, app.valueSystem, { ...app.counting });
      // An exact address (2:255:w4:l2) selects its words or letters; anything else selects verses.
      if (parsed.selection) app.setExact(parsed.selection);
      else app.goTo({ first: parsed.first, last: parsed.last });
      text = "";
      input?.blur();
    } catch (e) {
      error = describeError(e);
    } finally {
      busy = false;
    }
  }

  function onGlobalKey(event: KeyboardEvent): void {
    // Alt+Left and Alt+Right step through browse history, as in a browser.
    if (event.altKey && (event.key === "ArrowLeft" || event.key === "ArrowRight")) {
      event.preventDefault();
      if (event.key === "ArrowLeft") app.goBack();
      else app.goForward();
      return;
    }
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

<div class="history-nav">
  <button type="button" class="nav" disabled={!app.canGoBack} onclick={() => app.goBack()} aria-label="Back" title="Back (Alt+Left)">
    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M15.4 5.4 14 4l-8 8 8 8 1.4-1.4L8.8 12z" /></svg>
  </button>
  <button type="button" class="nav" disabled={!app.canGoForward} onclick={() => app.goForward()} aria-label="Forward" title="Forward (Alt+Right)">
    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M8.6 18.6 10 20l8-8-8-8-1.4 1.4 6.6 6.6z" /></svg>
  </button>
</div>

<form class="reference" onsubmit={go} role="search">
  <label class="visually-hidden" for="reference-input">Go to chapter or verse</label>
  <input
    id="reference-input"
    bind:this={input}
    bind:value={text}
    class="field num"
    placeholder="Go to 2:255 or page 10"
    title="A chapter (2, 3-4), a verse (2:255, 2:255-257, 24:35-27:62), an exact word or letter (2:255:w4, 2:255:w4:l2-2:257:w8), or a unit: page, station, part, group, half, quarter, bowing, verse, word or letter, such as page 10 or part 3-4"
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
  .history-nav {
    display: flex;
    gap: 2px;
  }

  .nav {
    display: grid;
    place-items: center;
    width: 2rem;
    height: 2rem;
    padding: 0;
    border: 0;
    border-radius: var(--radius-md);
    background: none;
    color: var(--ink-muted);
  }

  .nav:hover:not(:disabled) {
    background: var(--surface-sunk);
    color: var(--ink);
  }

  .nav:disabled {
    opacity: 0.35;
    cursor: default;
  }

  .nav svg {
    width: 1.1rem;
    height: 1.1rem;
    fill: currentColor;
  }

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
