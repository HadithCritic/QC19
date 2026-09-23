<script lang="ts">
  import type { Translation } from "../engine/types";
  import { app } from "../state/app.svelte";

  // Which translations show under each verse (Features.txt #27): the
  // edition's own, then any from a translation pack. The standard-spelling
  // Arabic is for search, not for reading, so it is not offered.

  let filter = $state("");

  const offered = $derived(app.allTranslations.filter((t) => t.kind !== "emlaaei"));
  const matching = $derived.by(() => {
    const q = filter.trim().toLowerCase();
    return q ? offered.filter((t) => `${t.name} ${t.translator} ${t.language}`.toLowerCase().includes(q)) : offered;
  });
  const groups = $derived(
    [
      { title: "This edition", list: matching.filter((t) => !t.pack) },
      { title: "Translation pack (Tanzil)", list: matching.filter((t) => t.pack) },
    ].filter((g) => g.list.length > 0),
  );
</script>

{#snippet item(t: Translation)}
  <li>
    <label>
      <input type="checkbox" checked={app.shownTranslations.includes(t.key)} onchange={(e) => app.toggleTranslation(t.key, e.currentTarget.checked)} />
      <span lang={t.language} dir={t.rightToLeft ? "rtl" : "ltr"}>{t.name}</span>
      <span class="by">{t.translator}</span>
    </label>
  </li>
{/snippet}

{#if offered.length > 0}
  <details class="menu">
    <summary>
      Translations{#if app.shownTranslations.length}<span class="count num">{app.shownTranslations.length}</span>{/if}
    </summary>
    <div class="panel">
      {#if offered.length > 12}
        <input class="field" type="search" placeholder="Filter by language or translator" aria-label="Filter translations" bind:value={filter} />
      {/if}
      {#each groups as group (group.title)}
        <p class="group">{group.title}</p>
        <ul>
          {#each group.list as t (t.key)}{@render item(t)}{/each}
        </ul>
      {/each}
    </div>
  </details>
{/if}

<style>
  .menu {
    position: relative;
    font-size: var(--text-xs);
  }

  summary {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    color: var(--ink-muted);
    cursor: pointer;
  }

  .count {
    padding: 0 0.35em;
    border-radius: 999px;
    background: var(--lapis-soft);
    color: var(--lapis);
  }

  .panel {
    position: absolute;
    z-index: 20;
    margin-top: var(--space-1);
    padding: var(--space-2);
    width: 22rem;
    max-height: 24rem;
    overflow-y: auto;
    background: var(--surface);
    border: 1px solid var(--rule);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-pop);
  }

  .panel .field {
    width: 100%;
    height: 1.75rem;
    margin-bottom: var(--space-2);
    font-size: var(--text-xs);
  }

  .group {
    margin: var(--space-2) 0 var(--space-1);
    font-weight: 600;
    color: var(--ink-muted);
  }

  ul {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  label {
    display: grid;
    grid-template-columns: auto 1fr auto;
    align-items: center;
    gap: var(--space-2);
    padding: 0.2rem var(--space-1);
    border-radius: var(--radius-sm);
    cursor: pointer;
  }

  label:hover {
    background: var(--lapis-soft);
  }

  .by {
    color: var(--ink-faint);
    text-align: end;
  }
</style>
