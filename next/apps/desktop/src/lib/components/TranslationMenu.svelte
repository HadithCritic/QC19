<script lang="ts">
  import type { Translation } from "../engine/types";
  import { app } from "../state/app.svelte";

  // Multi-select translations dropdown: permits selecting 0 (pure Arabic),
  // 1, or multiple translations simultaneously.

  let open = $state(false);
  let root: HTMLElement | undefined = $state();
  let filter = $state("");

  const offered = $derived(app.allTranslations.filter((t) => t.kind !== "emlaaei"));
  const matching = $derived.by(() => {
    const q = filter.trim().toLowerCase();
    return q ? offered.filter((t) => `${t.name} ${t.translator} ${t.language}`.toLowerCase().includes(q)) : offered;
  });
  const groups = $derived([{ title: "Available translations", list: matching }].filter((g) => g.list.length > 0));

  function onWindowClick(event: MouseEvent): void {
    if (open && root && !root.contains(event.target as Node)) {
      open = false;
    }
  }

  function onKeydown(event: KeyboardEvent): void {
    if (event.key === "Escape" && open) {
      open = false;
      root?.querySelector("button")?.focus();
    }
  }

  function selectNone(): void {
    app.setShownTranslations([]);
  }

  function selectDefault(): void {
    const def = offered.find((t) => t.key === "khalifa") ?? offered[0];
    if (def) app.setShownTranslations([def.key]);
  }

  function selectAll(): void {
    app.setShownTranslations(offered.map((t) => t.key));
  }
</script>

<svelte:window onclick={onWindowClick} onkeydown={onKeydown} />

{#if offered.length > 0}
  <div class="menu-container" bind:this={root}>
    <button
      type="button"
      class="field trigger"
      class:active={open}
      aria-expanded={open}
      aria-haspopup="dialog"
      onclick={() => (open = !open)}
    >
      <svg viewBox="0 0 24 24" class="icon" aria-hidden="true">
        <path fill="currentColor" d="M12.87 15.07l-2.54-2.51.03-.03c1.74-1.94 2.98-4.17 3.71-6.53H17V4h-7V2H8v2H1v2h11.17C11.5 7.92 10.44 9.75 9 11.35 8.07 10.32 7.3 9.19 6.69 8h-2c.73 1.63 1.73 3.17 2.98 4.56l-5.09 5.02L4 19l5-5 3.11 3.11.76-2.04zM18.5 10h-2L12 22h2l1.12-3h4.75L21 22h2l-4.5-12zm-2.62 7l1.62-4.33L19.12 17h-3.24z"/>
      </svg>
      <span class="label">Translations</span>
      <span class="count-badge num" class:zero={app.shownTranslations.length === 0}>
        {app.shownTranslations.length}
      </span>
      <svg viewBox="0 0 24 24" class="chevron" class:open aria-hidden="true">
        <path fill="currentColor" d="M7 10l5 5 5-5z" />
      </svg>
    </button>

    {#if open}
      <div class="panel" role="dialog" aria-label="Translation selection">
        <div class="panel-header">
          <div class="quick-actions">
            <button
              type="button"
              class="quick-btn"
              class:selected={app.shownTranslations.length === 0}
              onclick={selectNone}
              title="Hide translations and display pure Arabic"
            >
              None (0)
            </button>
            <button
              type="button"
              class="quick-btn"
              onclick={selectDefault}
              title="Reset to Rashad Khalifa's translation"
            >
              Default (1)
            </button>
            <button
              type="button"
              class="quick-btn"
              onclick={selectAll}
              title="Select all translations"
            >
              All ({offered.length})
            </button>
          </div>
        </div>

        {#if offered.length > 6}
          <div class="search-box">
            <input
              class="field search-field"
              type="search"
              placeholder="Search translations..."
              aria-label="Filter translations"
              bind:value={filter}
            />
          </div>
        {/if}

        <div class="list-scroll">
          {#each groups as group (group.title)}
            <ul role="group" aria-label={group.title}>
              {#each group.list as t (t.key)}
                {@const isSelected = app.shownTranslations.includes(t.key)}
                <li class:checked={isSelected}>
                  <label class="item-label">
                    <input
                      type="checkbox"
                      checked={isSelected}
                      onchange={(e) => app.toggleTranslation(t.key, e.currentTarget.checked)}
                    />
                    <div class="item-info">
                      <span class="item-name" lang={t.language} dir={t.rightToLeft ? "rtl" : "ltr"}>{t.name}</span>
                      <span class="item-by">{t.translator}</span>
                    </div>
                  </label>
                </li>
              {/each}
            </ul>
          {/each}
        </div>

        <div class="panel-footer">
          {#if app.shownTranslations.length === 0}
            <span class="footer-msg">Arabic only (no translation selected)</span>
          {:else}
            <span class="footer-msg">Showing <strong class="num">{app.shownTranslations.length}</strong> translation{app.shownTranslations.length > 1 ? "s" : ""}</span>
          {/if}
        </div>
      </div>
    {/if}
  </div>
{/if}

<style>
  .menu-container {
    position: relative;
    display: inline-block;
    font-size: var(--text-xs);
    text-align: start;
  }

  .trigger {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    height: 1.85rem;
    padding: 0 var(--space-3);
    border: 1px solid var(--rule);
    border-radius: var(--radius-sm);
    background: var(--surface);
    color: var(--ink);
    font-size: var(--text-xs);
    font-weight: 550;
    cursor: pointer;
    transition: all var(--duration) var(--ease);
  }

  .trigger:hover,
  .trigger.active {
    border-color: var(--rule-strong);
    background: var(--surface-sunk);
  }

  .icon {
    width: 0.95rem;
    height: 0.95rem;
    color: var(--ink-muted);
  }

  .label {
    letter-spacing: 0.01em;
  }

  .count-badge {
    padding: 0.05rem 0.45rem;
    border-radius: 999px;
    background: var(--lapis-soft);
    color: var(--lapis);
    font-size: 0.7rem;
    font-weight: 600;
  }

  .count-badge.zero {
    background: var(--rule);
    color: var(--ink-muted);
  }

  .chevron {
    width: 1rem;
    height: 1rem;
    color: var(--ink-muted);
    transition: transform var(--duration) var(--ease);
  }

  .chevron.open {
    transform: rotate(180deg);
  }

  .panel {
    position: absolute;
    top: calc(100% + var(--space-1));
    left: 0;
    z-index: 35;
    width: 22rem;
    background: var(--surface);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-pop);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    animation: fadeIn 120ms var(--ease);
  }

  @keyframes fadeIn {
    from {
      opacity: 0;
      transform: translateY(-4px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .panel-header {
    padding: var(--space-2);
    border-bottom: 1px solid var(--rule);
    background: var(--paper);
  }

  .quick-actions {
    display: flex;
    gap: var(--space-1);
  }

  .quick-btn {
    flex: 1;
    padding: 0.25rem var(--space-2);
    border: 1px solid var(--rule);
    border-radius: var(--radius-sm);
    background: var(--surface);
    color: var(--ink);
    font-size: var(--text-xs);
    font-weight: 500;
    cursor: pointer;
    transition: all var(--duration) var(--ease);
  }

  .quick-btn:hover {
    border-color: var(--rule-strong);
    background: var(--surface-sunk);
  }

  .quick-btn.selected {
    border-color: var(--lapis);
    background: var(--lapis-soft);
    color: var(--lapis);
    font-weight: 600;
  }

  .search-box {
    padding: var(--space-2) var(--space-2) 0;
  }

  .search-field {
    width: 100%;
    height: 1.85rem;
    font-size: var(--text-xs);
  }

  .list-scroll {
    max-height: 18rem;
    overflow-y: auto;
    padding: var(--space-1) var(--space-2);
  }

  ul {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  li {
    margin: 1px 0;
    border-radius: var(--radius-sm);
    transition: background-color var(--duration) var(--ease);
  }

  li:hover {
    background: var(--surface-sunk);
  }

  li.checked {
    background: color-mix(in srgb, var(--lapis-soft) 40%, transparent);
  }

  .item-label {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-2);
    cursor: pointer;
    user-select: none;
  }

  .item-info {
    display: flex;
    flex-direction: column;
    gap: 1px;
    min-width: 0;
    flex: 1;
  }

  .item-name {
    font-weight: 550;
    color: var(--ink);
    font-size: var(--text-xs);
  }

  .item-by {
    color: var(--ink-muted);
    font-size: 0.72rem;
  }

  .panel-footer {
    padding: var(--space-2);
    border-top: 1px solid var(--rule);
    background: var(--paper);
    font-size: var(--text-xs);
    color: var(--ink-muted);
    text-align: center;
  }
</style>
