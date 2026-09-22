<script lang="ts">
  import { changedCount, COUNTING_CHOICES, unavailableReason } from "../counting";
  import { app } from "../state/app.svelte";

  // The original's Statistics-panel text options, gathered in one menu. An
  // option the current text mode does not use is shown disabled with the
  // reason, so a saved choice never looks active when it is not.

  let open = $state(false);
  let root: HTMLElement | undefined = $state();

  const textMode = $derived(app.currentSystem?.textMode ?? "");
  const changed = $derived(changedCount(app.counting));

  function bismillahDetail(): string {
    return app.hasVerseZero
      ? "Each chapter's Bismillah is its verse 0. Chapter 1's is its verse 1 and always counts."
      : "The Bismillah at the start of verse 1 of chapters 2 to 114, except 9.";
  }

  function onWindowClick(event: MouseEvent): void {
    if (open && root && !root.contains(event.target as Node)) open = false;
  }

  function onKey(event: KeyboardEvent): void {
    if (event.key === "Escape" && open) {
      open = false;
      root?.querySelector("button")?.focus();
    }
  }
</script>

<svelte:window onclick={onWindowClick} onkeydown={onKey} />

<div class="counting" bind:this={root}>
  <button type="button" class="field trigger" aria-expanded={open} aria-controls="counting-panel" onclick={() => (open = !open)}>
    Counting
    {#if changed > 0}<span class="badge num" aria-label="{changed} changed">{changed}</span>{/if}
  </button>

  {#if open}
    <div id="counting-panel" class="panel" role="group" aria-label="How the text is counted">
      <ul>
        {#each COUNTING_CHOICES as choice (choice.key)}
          {@const reason = unavailableReason(choice.key, textMode, app.hasVerseZero)}
          <li class:disabled={reason !== null}>
            <label>
              <input
                type="checkbox"
                checked={app.counting[choice.key]}
                disabled={reason !== null}
                onchange={(e) => app.setCounting(choice.key, e.currentTarget.checked)}
              />
              <span class="mark arabic" lang="ar" aria-hidden="true">{choice.mark}</span>
              <span class="text">
                <span class="label">{choice.label}</span>
                <span class="detail">{reason ?? (choice.key === "includeBasmalas" ? bismillahDetail() : choice.detail)}</span>
              </span>
            </label>
          </li>
        {/each}
      </ul>
      <footer>
        <button type="button" class="button" disabled={changed === 0} onclick={() => app.resetCounting()}>Reset to defaults</button>
      </footer>
    </div>
  {/if}
</div>

<style>
  .counting {
    position: relative;
  }

  .trigger {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    height: 2rem;
    font-size: var(--text-sm);
    font-weight: 550;
  }

  .badge {
    min-width: 1.2rem;
    padding: 0 0.3rem;
    border-radius: 999px;
    background: var(--lapis);
    color: var(--lapis-ink);
    font-size: var(--text-xs);
    line-height: 1.2rem;
    text-align: center;
  }

  .panel {
    position: absolute;
    top: calc(100% + 6px);
    inset-inline-end: 0;
    z-index: 30;
    width: 22rem;
    padding: var(--space-2);
    background: var(--surface);
    border: 1px solid var(--rule);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-pop);
  }

  ul {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  label {
    display: grid;
    grid-template-columns: auto 1.75rem 1fr;
    align-items: start;
    gap: var(--space-2);
    padding: var(--space-2);
    border-radius: var(--radius-md);
    cursor: pointer;
  }

  label:hover {
    background: var(--paper);
  }

  .disabled label {
    cursor: default;
    opacity: 0.55;
  }

  input {
    margin-top: 0.2rem;
    accent-color: var(--lapis);
  }

  .mark {
    font-size: 1.35rem;
    line-height: 1;
    text-align: center;
  }

  .text {
    display: grid;
  }

  .label {
    font-size: var(--text-sm);
    font-weight: 550;
  }

  .detail {
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  footer {
    display: flex;
    justify-content: flex-end;
    padding: var(--space-2) var(--space-2) var(--space-1);
    border-top: 1px solid var(--rule);
    margin-top: var(--space-1);
  }

  footer .button {
    height: 1.9rem;
    font-size: var(--text-sm);
  }
</style>
