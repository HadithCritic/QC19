<script lang="ts">
  import Notice from "../lib/components/Notice.svelte";
  import { describeError, engine } from "../lib/engine/client";
  import type { HistoryEntry, HistoryKind } from "../lib/engine/types";
  import { app } from "../lib/state/app.svelte";

  // Bookmarks with their notes, and browse and find history (Features.txt #69,
  // #70). Opening a browse entry selects its range in the reader.

  let browse = $state<HistoryEntry[]>([]);
  let find = $state<HistoryEntry[]>([]);
  let error = $state<string | null>(null);

  async function loadHistory(): Promise<void> {
    try {
      [browse, find] = await Promise.all([engine.history("browse"), engine.history("find")]);
      error = null;
    } catch (e) {
      error = describeError(e);
    }
  }

  $effect(() => {
    void loadHistory();
    void app.loadBookmarks();
  });

  async function clear(kind: HistoryKind): Promise<void> {
    try {
      await engine.clearHistory(kind);
      await loadHistory();
    } catch (e) {
      error = describeError(e);
    }
  }

  function when(utc: string): string {
    return new Date(utc).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" });
  }

  const bookmarks = $derived(
    [...(app.bookmarks ?? [])].sort((a, b) => (a.first ?? Infinity) - (b.first ?? Infinity) || a.id - b.id),
  );
</script>

<section class="saved" aria-labelledby="saved-title">
  <header>
    <h1 id="saved-title">Saved</h1>
    <p class="hint">Bookmarks and notes, and what you have read and searched. Everything here is kept on this computer.</p>
  </header>

  <div class="body">
    {#if app.bookmarks === null}
      <Notice tone="error" title="Bookmarks and history are not available" detail="The app could not open its user data file." />
    {:else}
      <section aria-labelledby="bookmarks-title">
        <h2 id="bookmarks-title" class="eyebrow">Bookmarks</h2>
        {#if bookmarks.length === 0}
          <p class="empty">No bookmarks yet. Select verses in the reader and choose Bookmark.</p>
        {:else}
          <ul class="list">
            {#each bookmarks as bookmark (bookmark.id)}
              <li>
                {#if bookmark.first !== null && bookmark.last !== null}
                  <button type="button" class="ref num" onclick={() => app.goTo({ first: bookmark.first!, last: bookmark.last! })}>{bookmark.reference}</button>
                {:else}
                  <span class="ref num missing" title="This verse is not part of the open edition">{bookmark.reference}</span>
                {/if}
                <p class="note">{bookmark.note || "No note"}</p>
                <span class="time">{when(bookmark.updatedUtc)}</span>
                <button type="button" class="link danger" onclick={() => app.deleteBookmark(bookmark.id)} aria-label="Delete the bookmark on {bookmark.reference}">Delete</button>
              </li>
            {/each}
          </ul>
        {/if}
      </section>

      {#if error}
        <Notice tone="error" title="History could not be loaded" detail={error} />
      {:else}
        <div class="columns">
          <section aria-labelledby="browse-title">
            <div class="head">
              <h2 id="browse-title" class="eyebrow">Browse history</h2>
              <button type="button" class="link" disabled={browse.length === 0} onclick={() => clear("browse")}>Clear</button>
            </div>
            {#if browse.length === 0}
              <p class="empty">Selections you stay on for a second appear here.</p>
            {:else}
              <ul class="list compact">
                {#each browse as entry (entry.id)}
                  <li>
                    {#if entry.first !== null && entry.last !== null}
                      <button type="button" class="ref num" onclick={() => app.goTo({ first: entry.first!, last: entry.last! })}>{entry.reference}</button>
                    {:else}
                      <span class="ref num missing">{entry.reference}</span>
                    {/if}
                    <span class="time">{when(entry.atUtc)}</span>
                  </li>
                {/each}
              </ul>
            {/if}
          </section>

          <section aria-labelledby="find-title">
            <div class="head">
              <h2 id="find-title" class="eyebrow">Search history</h2>
              <button type="button" class="link" disabled={find.length === 0} onclick={() => clear("find")}>Clear</button>
            </div>
            {#if find.length === 0}
              <p class="empty">Searches you run appear here.</p>
            {:else}
              <ul class="list compact">
                {#each find as entry (entry.id)}
                  <li>
                    <button type="button" class="ref arabic" lang="ar" dir="rtl" onclick={() => app.searchFor(entry.term ?? "", entry.wordness ?? "any")}>{entry.term}</button>
                    <span class="time">{when(entry.atUtc)}</span>
                  </li>
                {/each}
              </ul>
            {/if}
          </section>
        </div>
      {/if}
    {/if}
  </div>
</section>

<style>
  .saved {
    display: grid;
    grid-template-rows: auto 1fr;
    height: 100%;
    min-height: 0;
  }

  header {
    padding: var(--space-5) var(--space-6) var(--space-4);
    border-bottom: 1px solid var(--rule);
    background: var(--surface);
  }

  h1 {
    margin: 0;
    font-size: var(--text-xl);
    font-weight: 600;
    letter-spacing: -0.015em;
  }

  .hint {
    margin: var(--space-1) 0 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .body {
    display: grid;
    align-content: start;
    gap: var(--space-6);
    overflow-y: auto;
    padding: var(--space-5) var(--space-6) var(--space-7);
  }

  .columns {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-6);
  }

  .head {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
  }

  h2 {
    margin: 0 0 var(--space-2);
  }

  .list {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .list li {
    display: grid;
    grid-template-columns: 7rem 1fr auto auto;
    align-items: baseline;
    gap: var(--space-3);
    padding: var(--space-2) 0;
    border-top: 1px solid var(--rule);
  }

  .list.compact li {
    grid-template-columns: 1fr auto;
  }

  .ref {
    justify-self: start;
    padding: 0.1rem 0.4rem;
    border: 0;
    border-radius: var(--radius-sm);
    background: none;
    color: var(--lapis);
    font-size: var(--text-sm);
    text-align: start;
  }

  .ref.arabic {
    font-size: 1.1rem;
  }

  .ref:hover {
    background: var(--lapis-soft);
  }

  .ref.missing {
    color: var(--ink-faint);
  }

  .note {
    margin: 0;
    font-size: var(--text-sm);
    white-space: pre-wrap;
    overflow-wrap: anywhere;
  }

  .time {
    font-size: var(--text-xs);
    color: var(--ink-faint);
    white-space: nowrap;
  }

  .empty {
    margin: 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
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

  @media (max-width: 1000px) {
    .columns {
      grid-template-columns: 1fr;
    }
  }
</style>
