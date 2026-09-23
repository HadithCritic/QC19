<script lang="ts">
  import CountingMenu from "./lib/components/CountingMenu.svelte";
  import Notice from "./lib/components/Notice.svelte";
  import ReferenceBox from "./lib/components/ReferenceBox.svelte";
  import SystemPicker from "./lib/components/SystemPicker.svelte";
  import { app, type Theme, type View } from "./lib/state/app.svelte";
  import NumbersView from "./views/NumbersView.svelte";
  import StatisticsView from "./views/StatisticsView.svelte";
  import ReadView from "./views/ReadView.svelte";
  import SearchView from "./views/SearchView.svelte";
  import SavedView from "./views/SavedView.svelte";
  import ValuesView from "./views/ValuesView.svelte";

  const VIEWS: { id: View; label: string; icon: string }[] = [
    { id: "read", label: "Read", icon: "M4 5.5C4 4.7 4.7 4 5.5 4H11v15H5.5c-.8 0-1.5-.7-1.5-1.5v-12ZM13 4h5.5c.8 0 1.5.7 1.5 1.5v12c0 .8-.7 1.5-1.5 1.5H13V4Z" },
    { id: "search", label: "Search", icon: "M10.5 4a6.5 6.5 0 1 0 3.9 11.7l4.4 4.4 1.4-1.4-4.4-4.4A6.5 6.5 0 0 0 10.5 4Zm0 2a4.5 4.5 0 1 1 0 9 4.5 4.5 0 0 1 0-9Z" },
    { id: "values", label: "Values", icon: "M6 4h12v2.5H9.2l4.6 5.5-4.6 5.5H18V20H6v-2.2l5.2-5.8L6 6.2V4Z" },
    { id: "statistics", label: "Stats", icon: "M4 20V10h3v10H4Zm6.5 0V4h3v16h-3ZM17 20v-7h3v7h-3Z" },
    { id: "saved", label: "Saved", icon: "M6 3h12v18l-6-4-6 4V3Z" },
    { id: "numbers", label: "Numbers", icon: "M9 3 8.3 7H5v2h3l-.7 4H4v2h3l-.7 4h2l.7-4h4l-.7 4h2l.7-4H19v-2h-3.3l.7-4H20V7h-3.3L17.4 3h-2l-.7 4h-4l.7-4H9Zm1 6h4l-.7 4h-4L10 9Z" },
  ];

  const THEMES: { id: Theme; label: string; icon: string }[] = [
    { id: "system", label: "Auto", icon: "M12 3a9 9 0 1 0 0 18V3Zm0 2v14a7 7 0 0 0 0-14Z" },
    { id: "light", label: "Light", icon: "M12 7a5 5 0 1 0 0 10 5 5 0 0 0 0-10ZM11 1h2v3h-2V1Zm0 19h2v3h-2v-3ZM1 11h3v2H1v-2Zm19 0h3v2h-3v-2Z" },
    { id: "dark", label: "Dark", icon: "M14.5 3A9 9 0 1 0 21 14.5 7 7 0 0 1 14.5 3Z" },
  ];

  const theme = $derived(THEMES.find((t) => t.id === app.theme) ?? THEMES[0]!);

  function cycleTheme(): void {
    const index = THEMES.findIndex((t) => t.id === app.theme);
    app.setTheme(THEMES[(index + 1) % THEMES.length]!.id);
  }

  $effect(() => {
    void app.start();
  });

  $effect(() => {
    const root = document.documentElement;
    if (app.theme === "system") delete root.dataset.theme;
    else root.dataset.theme = app.theme;
  });
</script>

{#if app.status === "starting"}
  <main class="splash"><Notice tone="loading" title="Opening QuranCode" /></main>
{:else if app.status === "failed"}
  <main class="splash">
    <Notice tone="error" title="The analysis engine did not start" detail={app.startupError ?? undefined} action={{ label: "Try again", run: () => app.start() }} />
  </main>
{:else}
  <div class="shell">
    <nav class="rail" aria-label="Views">
      <span class="mark" aria-hidden="true">
        <svg viewBox="0 0 1024 1024"><g fill="none" stroke="currentColor" stroke-width="70" stroke-linejoin="round"><rect x="272" y="272" width="480" height="480" /><rect x="272" y="272" width="480" height="480" transform="rotate(45 512 512)" /></g><circle cx="512" cy="512" r="100" fill="currentColor" /></svg>
      </span>
      {#each VIEWS as view (view.id)}
        <button type="button" class:active={app.view === view.id} aria-current={app.view === view.id ? "page" : undefined} onclick={() => (app.view = view.id)}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d={view.icon} /></svg>
          <span>{view.label}</span>
        </button>
      {/each}
      <button type="button" class="theme" onclick={cycleTheme} aria-label="Theme: {theme.label}. Change theme">
        <svg viewBox="0 0 24 24" aria-hidden="true"><path d={theme.icon} /></svg>
        <span>{theme.label}</span>
      </button>
    </nav>

    <header class="topbar">
      <ReferenceBox />
      <SystemPicker />
      <div class="settings">
        <CountingMenu />
        <label class="switch" title="Show the research-only letter-value systems and the SimplifiedMarks text mode">
          <input type="checkbox" role="switch" checked={app.research} onchange={(e) => app.setResearch(e.currentTarget.checked)} />
          <span class="track" aria-hidden="true"></span>
          Research
        </label>
      </div>
    </header>

    <main class="content">
      {#if app.view === "read"}
        <ReadView />
      {:else if app.view === "search"}
        <SearchView />
      {:else if app.view === "values"}
        <ValuesView />
      {:else if app.view === "saved"}
        <SavedView />
      {:else if app.view === "statistics"}
        <StatisticsView />
      {:else}
        <NumbersView />
      {/if}
    </main>
  </div>
{/if}

<style>
  .splash {
    display: grid;
    place-items: center;
    height: 100%;
  }

  .shell {
    display: grid;
    grid-template-columns: var(--rail-width) minmax(0, 1fr);
    grid-template-rows: var(--topbar-height) minmax(0, 1fr);
    height: 100%;
  }

  .rail {
    grid-row: 1 / -1;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-1);
    padding: var(--space-2) 0;
    background: var(--rail);
    color: var(--rail-ink);
  }

  .mark {
    display: grid;
    place-items: center;
    width: 2.25rem;
    height: 2.25rem;
    margin-bottom: var(--space-3);
    color: var(--gilt);
  }

  .mark svg {
    width: 1.6rem;
  }

  .rail button {
    display: grid;
    justify-items: center;
    gap: 2px;
    width: 3.75rem;
    padding: var(--space-2) 0;
    border: 0;
    border-radius: var(--radius-md);
    background: none;
    color: color-mix(in srgb, var(--rail-ink) 62%, transparent);
    font-size: 0.6875rem;
    font-weight: 550;
  }

  .rail button:hover {
    color: var(--rail-ink);
    background: color-mix(in srgb, var(--rail-ink) 8%, transparent);
  }

  .rail button.active {
    color: var(--rail-ink);
    background: color-mix(in srgb, var(--rail-ink) 14%, transparent);
  }

  .rail button.active svg {
    fill: var(--gilt);
  }

  .rail svg {
    width: 1.25rem;
    height: 1.25rem;
    fill: currentColor;
  }

  .topbar {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    padding: 0 var(--space-4);
    background: var(--surface);
    border-bottom: 1px solid var(--rule);
    min-width: 0;
  }

  .settings {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-inline-start: auto;
  }

  .switch {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    font-size: var(--text-sm);
    font-weight: 550;
    cursor: pointer;
  }

  .switch input {
    position: absolute;
    opacity: 0;
  }

  .track {
    position: relative;
    width: 2rem;
    height: 1.125rem;
    border-radius: 999px;
    background: var(--rule-strong);
    transition: background var(--duration) var(--ease);
  }

  .track::after {
    content: "";
    position: absolute;
    top: 2px;
    left: 2px;
    width: calc(1.125rem - 4px);
    height: calc(1.125rem - 4px);
    border-radius: 50%;
    background: var(--surface);
    transition: transform var(--duration) var(--ease);
  }

  .switch input:checked + .track {
    background: var(--lapis);
  }

  .switch input:checked + .track::after {
    transform: translateX(0.875rem);
  }

  .switch input:focus-visible + .track {
    box-shadow: var(--focus);
  }

  .rail .theme {
    margin-top: auto;
  }

  .content {
    min-height: 0;
    overflow: hidden;
  }
</style>
