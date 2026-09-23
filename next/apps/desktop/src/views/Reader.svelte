<script lang="ts">
  import { tick } from "svelte";
  import Notice from "../lib/components/Notice.svelte";
  import Rosette from "../lib/components/Rosette.svelte";
  import { describeError, engine, latest } from "../lib/engine/client";
  import type { ClassCode, Verse } from "../lib/engine/types";
  import { CLASS_NAMES } from "../lib/numbers";
  import { keySearch, nextBookmark } from "../lib/readerKeys";
  import { app } from "../lib/state/app.svelte";
  import { search } from "../lib/state/search.svelte";

  // One verse per line, right-aligned, each closed by its rosette. The
  // rosette's ring is the verse's value class under the current system, so the
  // chapter's numeric texture reads down the margin at a glance.

  let verses = $state<Verse[]>([]);
  let codes = $state<Map<number, { value: string | null; code: ClassCode | null }>>(new Map());
  let error = $state<string | null>(null);
  let loading = $state(true);
  let scroller: HTMLElement | undefined = $state();

  const loadVerses = latest(engine.chapterVerses);
  const loadValues = latest(engine.chapterValues);

  const chapter = $derived(app.chapters[app.chapter - 1]);

  async function reveal(verse: number | null): Promise<void> {
    await tick();
    if (verse === null) {
      scroller?.scrollTo({ top: 0 });
      return;
    }
    scroller?.querySelector(`[data-verse="${verse}"]`)?.scrollIntoView({ block: "center" });
  }

  $effect(() => {
    const number = app.chapter;
    loading = true;
    error = null;
    loadVerses(number)
      .then(({ current, value }) => {
        if (!current) return;
        verses = value;
        void reveal(app.focusVerse);
      })
      .catch((e: unknown) => (error = describeError(e)))
      .finally(() => (loading = false));
  });

  $effect(() => {
    const number = app.chapter;
    const system = app.valueSystem;
    const counting = { ...app.counting };
    if (!system) return;
    codes = new Map();
    loadValues(number, system, counting)
      .then(({ current, value }) => {
        if (current) codes = new Map(value.map((v) => [v.number, { value: v.value, code: v.code }]));
      })
      // Rosettes stay neutral if values fail; the text is still readable,
      // and the inspector reports the underlying error when used.
      .catch(() => (codes = new Map()));
  });

  $effect(() => {
    const target = app.focusVerse;
    if (target !== null && verses.some((v) => v.number === target)) void reveal(target);
  });

  function isSelected(verse: number): boolean {
    const s = app.selection;
    return s !== null && verse >= s.first && verse <= s.last;
  }

  function onKey(event: KeyboardEvent, verse: number): void {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      app.selectVerse(verse, event.shiftKey);
    }
  }

  /** Display index of a reader word: the Bismillah header an edition prefixes to verse 1 comes first. */
  function displayIndex(verse: Verse, index: number): number {
    return (verse.bismillah ? verse.bismillah.split(" ").length : 0) + index;
  }

  function reference(verse: Verse): string {
    return `${verse.chapter}:${verse.numberInChapter}`;
  }

  // Alt+click measures from the previous Alt+clicked word (Features.txt #63);
  // Ctrl+click finds the words of the same root (#1); a plain click selects
  // the verse and makes the word the one F4, F7 and F8 act on.
  function onVerseClick(event: MouseEvent, verse: Verse): void {
    const target = (event.target as HTMLElement).closest<HTMLElement>("[data-word]");
    const index = target ? Number(target.dataset.word) : null;
    if (event.altKey && index !== null) {
      event.preventDefault();
      void app.measureTo({ verse: verse.number, word: index });
      return;
    }
    if ((event.ctrlKey || event.metaKey) && index !== null) {
      event.preventDefault();
      const text = verse.words[index] ?? "";
      void search.start({ kind: "related", verse: verse.number, word: displayIndex(verse, index), label: `“${text}” (${reference(verse)})` });
      return;
    }
    app.currentWord = index === null ? null : { verse: verse.number, word: displayIndex(verse, index), text: verse.words[index] ?? "" };
    app.selectVerse(verse.number, event.shiftKey);
  }

  function isCurrentWord(verse: Verse, index: number): boolean {
    return app.currentWord?.verse === verse.number && app.currentWord.word === displayIndex(verse, index);
  }

  // F3 steps through bookmarks here; F4 to F8 start searches from the clicked
  // word or the selected verse (Features.txt #38 to #43).
  function onWindowKey(event: KeyboardEvent): void {
    if (app.view !== "read" || event.ctrlKey || event.altKey || event.metaKey) return;
    if (event.target instanceof Element && event.target.closest("input, textarea, select")) return;

    if (event.key === "F3") {
      event.preventDefault();
      const bookmark = nextBookmark(app.bookmarks ?? [], app.selection?.first ?? null, event.shiftKey);
      if (bookmark?.first != null && bookmark.last != null) app.goTo({ first: bookmark.first, last: bookmark.last });
      return;
    }

    const selected = verses.find((v) => v.number === app.selection?.first) ?? null;
    const word = app.currentWord;
    const wordVerse = word ? verses.find((v) => v.number === word.verse) : undefined;
    const request = keySearch(
      event.key,
      word && wordVerse ? { ...word, label: reference(wordVerse) } : null,
      selected ? { number: selected.number, label: reference(selected), text: selected.words.join(" ") } : null,
    );
    if (!request) return;
    event.preventDefault();
    void search.start(request);
  }

  function isMeasured(verse: number, word: number, end: "from" | "to"): boolean {
    const location = end === "from" ? (app.measurement?.from ?? app.measureFrom) : app.measurement?.to;
    return location !== null && location !== undefined && location.verse === verse && location.word === word;
  }

  function uncounted(verse: Verse): boolean {
    return verse.isBasmala && app.verseZeroExcluded;
  }

  function label(verse: Verse): string {
    if (uncounted(verse)) return `Verse 0, the Bismillah, not counted`;
    const value = codes.get(verse.number);
    const suffix = value?.value && value.code !== null ? `, value ${value.value}, ${CLASS_NAMES[value.code].toLowerCase()}` : "";
    return `Verse ${verse.numberInChapter}${suffix}`;
  }
</script>

<svelte:window onkeydown={onWindowKey} />

<article class="reader" bind:this={scroller} aria-busy={loading}>
  {#if error}
    <Notice tone="error" title="This chapter could not be opened" detail={error} action={{ label: "Try again", run: () => app.openChapter(app.chapter) }} />
  {:else if chapter}
    <header class="chapter">
      <p class="arabic title" lang="ar" dir="rtl">{chapter.name}</p>
      <h1>
        <span class="num">{chapter.number}</span>
        {chapter.transliteratedName}
        <span class="english">{chapter.englishName}</span>
      </h1>
      <p class="meta">
        {chapter.revelationPlace} · <span class="num">{chapter.verseCount}</span> verses{chapter.hasVerseZero ? " and the Bismillah as verse 0" : ""} · revelation order <span class="num">{chapter.revelationOrder}</span>
        <button type="button" class="link" onclick={() => app.openChapter(chapter.number)}>Select chapter</button>
      </p>
    </header>

    {#if verses[0]?.bismillah}
      <p class="quran bismillah" lang="ar" dir="rtl">{verses[0].bismillah}</p>
    {/if}

    <ol class="quran text" lang="ar" dir="rtl">
      {#each verses as verse (verse.number)}
        <li><div
          class="verse"
          class:basmala={verse.isBasmala}
          class:uncounted={uncounted(verse)}
          class:selected={isSelected(verse.number)}
          data-verse={verse.number}
          role="button"
          tabindex="0"
          aria-pressed={isSelected(verse.number)}
          aria-label={label(verse)}
          onclick={(e) => onVerseClick(e, verse)}
          onkeydown={(e) => onKey(e, verse.number)}
        >{#each verse.words as word, index (index)}<span
              class="word"
              class:measure-from={isMeasured(verse.number, index, "from")}
              class:measure-to={isMeasured(verse.number, index, "to")}
              class:current-word={isCurrentWord(verse, index)}
              data-word={index}>{word}</span>{" "}{/each}<Rosette number={verse.numberInChapter} code={codes.get(verse.number)?.code ?? null} />{#if uncounted(verse)}<span class="note" lang="en" dir="ltr">not counted</span>{/if}</div></li>
      {/each}
    </ol>

    <footer class="legend" aria-label="Verse marker colors">
      {#each ["U", "AP", "XP", "AC", "XC"] as const as code (code)}
        <span data-class={code}><i aria-hidden="true"></i>{code} <span class="legend-name">{CLASS_NAMES[code]}</span></span>
      {/each}
    </footer>
  {/if}
</article>

<style>
  .reader {
    overflow-y: auto;
    padding: var(--space-6) clamp(var(--space-5), 5vw, var(--space-7)) var(--space-7);
    scroll-padding-block: var(--space-7);
  }

  .chapter {
    max-width: 46rem;
    margin: 0 auto var(--space-5);
    text-align: center;
  }

  .title {
    margin: 0;
    font-size: 2.75rem;
    line-height: 1.4;
    font-weight: 700;
    color: var(--ink);
  }

  h1 {
    margin: 0;
    font-size: var(--text-lg);
    font-weight: 600;
    letter-spacing: -0.01em;
  }

  h1 .num {
    color: var(--gilt);
    margin-inline-end: 0.35em;
  }

  .english {
    font-weight: 400;
    color: var(--ink-muted);
    margin-inline-start: 0.35em;
  }

  .meta {
    margin: var(--space-1) 0 0;
    font-size: var(--text-sm);
    color: var(--ink-muted);
  }

  .link {
    margin-inline-start: var(--space-3);
    padding: 0;
    border: 0;
    background: none;
    color: var(--lapis);
    font-weight: 550;
  }

  .link:hover {
    text-decoration: underline;
  }

  .bismillah {
    max-width: 52rem;
    margin: 0 auto var(--space-3);
    text-align: center;
    color: var(--ink-muted);
  }

  .text {
    max-width: 52rem;
    margin: 0 auto;
    padding: 0;
    list-style: none;
    text-align: start; /* right, since the list is dir="rtl" */
  }

  .text li {
    border-bottom: 1px solid var(--rule);
  }

  .text li:last-child {
    border-bottom: 0;
  }

  .verse {
    padding: 0.15em 0.6em;
    border-radius: var(--radius-sm);
    cursor: pointer;
    transition: background var(--duration) var(--ease);
  }

  .verse:hover {
    background: var(--surface-sunk);
  }

  .verse.selected {
    background: var(--lapis-soft);
  }

  .word {
    border-radius: 3px;
  }

  .word.current-word {
    text-decoration: underline 1px var(--ink-faint);
    text-underline-offset: 0.4em;
  }

  .word.measure-from,
  .word.measure-to {
    background: var(--gilt-soft);
    box-shadow: 0 0 0 1px var(--gilt);
  }

  .verse.basmala {
    color: var(--ink-muted);
  }

  .verse.uncounted {
    opacity: 0.55;
  }

  .note {
    margin-inline-start: 0.6em;
    font-family: var(--font-ui);
    font-size: var(--text-xs);
    color: var(--ink-muted);
    vertical-align: middle;
  }

  .legend {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: var(--space-4);
    max-width: 46rem;
    margin: var(--space-6) auto 0;
    padding-top: var(--space-4);
    border-top: 1px solid var(--rule);
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .legend > span {
    display: inline-flex;
    align-items: center;
    gap: 0.4em;
  }

  .legend i {
    width: 0.7rem;
    height: 0.7rem;
    border-radius: 50%;
    border: 2px solid var(--class);
  }

  .legend-name {
    font-family: var(--font-ui);
  }
</style>
