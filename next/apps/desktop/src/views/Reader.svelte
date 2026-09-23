<script lang="ts">
  import { tick } from "svelte";
  import Notice from "../lib/components/Notice.svelte";
  import RatioControl from "../lib/components/RatioControl.svelte";
  import TranslationMenu from "../lib/components/TranslationMenu.svelte";
  import Rosette from "../lib/components/Rosette.svelte";
  import { describeError, engine, latest } from "../lib/engine/client";
  import type { ClassCode, RatioUnit, Verse } from "../lib/engine/types";
  import { cutWord, partOf } from "../lib/ratioColors";
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

  // The chosen translations of the chapter, by key and then verse.
  let translated = $state<Map<string, Map<number, string>>>(new Map());
  const loadTranslations = latest(engine.translationText);

  $effect(() => {
    const keys = [...app.shownTranslations];
    const chapter = app.chapters[app.chapter - 1];
    if (keys.length === 0 || !chapter) {
      translated = new Map();
      return;
    }
    const last = chapter.firstVerse + chapter.verseCount - (chapter.hasVerseZero ? 0 : 1);
    loadTranslations(keys, { first: chapter.firstVerse, last })
      .then(({ current, value }) => {
        if (current) translated = new Map(value.map((t) => [t.key, new Map(t.verses.map((v) => [v.verse, v.text]))]));
      })
      // The Arabic reads on its own; a missing translation is just not shown.
      .catch(() => (translated = new Map()));
  });

  const shown = $derived(app.shownTranslations.map((key) => app.allTranslations.find((t) => t.key === key)).filter((t) => t !== undefined));

  // The export marks footnotes with ±; the footnotes themselves are not in it.
  function readable(text: string): string {
    return text.replaceAll("±", "*");
  }

  let ratioUnits = $state<RatioUnit[]>([]);
  const loadRatio = latest(engine.ratioSplit);

  $effect(() => {
    if (!app.ratioOn || !app.valueSystem) {
      ratioUnits = [];
      return;
    }
    loadRatio(app.chapter, { ...app.ratioOptions }, app.valueSystem, { ...app.counting })
      .then(({ current, value }) => {
        if (current) ratioUnits = value;
      })
      // The text reads the same without colors; the control shows no totals.
      .catch(() => (ratioUnits = []));
  });

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

  function ratioPart(verse: number, index: number) {
    return ratioUnits.length ? partOf(ratioUnits, verse, index) : null;
  }

  function splitLetters(verse: number, index: number): number {
    const part = ratioPart(verse, index);
    return part?.part === "split" ? part.letters : 0;
  }

  function isCurrentWord(verse: Verse, index: number): boolean {
    return app.currentWord?.verse === verse.number && app.currentWord.word === displayIndex(verse, index);
  }

  async function sameValue(range: { first: number; last: number }): Promise<void> {
    try {
      const stats = await engine.stats(range, app.valueSystem, { ...app.counting });
      const value = stats.value.value;
      await search.start({
        kind: "numbers",
        query: { unit: "sentences", shape: "single", criteria: { value: { value } } },
        label: `sentences and verses with the value ${value}`,
      });
    } catch (e) {
      // The reader keeps its text; the search panel says what went wrong.
      search.error = describeError(e);
      app.view = "search";
    }
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

    // F9: sentences and verses with the selection's value (Features.txt #44).
    if (event.key === "F9" && app.selection) {
      event.preventDefault();
      void sameValue(app.selection);
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
      <div class="chapter-badge">
        <span class="badge-num num">SURAH {chapter.number}</span>
        <span class="badge-dot" aria-hidden="true">·</span>
        <span class="badge-place">{chapter.revelationPlace.toUpperCase()}</span>
        <span class="badge-dot" aria-hidden="true">·</span>
        <span><span class="num">{chapter.verseCount}</span> VERSES{chapter.hasVerseZero ? " (+ V0)" : ""}</span>
        <span class="badge-dot" aria-hidden="true">·</span>
        <span>ORDER <span class="num">{chapter.revelationOrder}</span></span>
      </div>

      <p class="arabic title" lang="ar" dir="rtl">{chapter.name}</p>

      <div class="chapter-titles">
        <h1 class="transliteration">{chapter.transliteratedName}</h1>
        {#if chapter.englishName}
          <p class="english-sub">“{chapter.englishName}”</p>
        {/if}
      </div>

      <div class="tools">
        <TranslationMenu />
        <RatioControl units={ratioUnits} />
      </div>
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
              data-ratio={ratioPart(verse.number, index)?.part}
              data-word={index}>{#if ratioPart(verse.number, index)?.part === "split"}{@const [a, b] = cutWord(word, splitLetters(verse.number, index))}<span class="ratio-first">{a}</span><span class="ratio-second">{b}</span>{:else}{word}{/if}</span>{" "}{/each}<Rosette number={verse.numberInChapter} code={codes.get(verse.number)?.code ?? null} />{#if uncounted(verse)}<span class="note" lang="en" dir="ltr">not counted</span>{/if}</div>
          {#each shown as t (t.key)}
            {@const line = translated.get(t.key)?.get(verse.number)}
            {#if line}<p class="translation" class:transliteration={t.kind === "transliteration"} lang={t.language} dir={t.rightToLeft ? "rtl" : "ltr"}>{readable(line)}</p>{/if}
          {/each}</li>
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
    max-width: 48rem;
    margin: 0 auto var(--space-6);
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-2);
  }

  .chapter-badge {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    padding: 0.25rem 0.85rem;
    border: 1px solid var(--rule);
    border-radius: 999px;
    background: var(--surface);
    color: var(--ink-muted);
    font-size: var(--text-xs);
    font-weight: 550;
    letter-spacing: 0.08em;
    box-shadow: 0 1px 2px rgb(0 0 0 / 0.03);
  }

  .badge-num {
    color: var(--gilt);
    font-weight: 700;
  }

  .badge-dot {
    color: var(--rule-strong);
  }

  .title {
    margin: var(--space-2) 0 0;
    font-size: 3.25rem;
    line-height: 1.35;
    font-weight: 700;
    color: var(--ink);
  }

  .chapter-titles {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2px;
    margin-bottom: var(--space-2);
  }

  .transliteration {
    margin: 0;
    font-family: var(--font-display);
    font-size: 2.15rem;
    font-weight: 600;
    letter-spacing: -0.015em;
    color: var(--ink);
  }

  .english-sub {
    margin: 0;
    font-family: var(--font-body);
    font-size: 1.15rem;
    font-style: italic;
    color: var(--ink-muted);
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

  .tools {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: flex-start;
    gap: var(--space-4);
    margin-top: var(--space-2);
  }

  .translation {
    margin: var(--space-1) 0 var(--space-3);
    font-family: var(--font-body);
    font-size: 1.0625rem;
    line-height: 1.65;
    color: var(--ink-muted);
  }

  .translation[dir="ltr"] {
    text-align: left;
  }

  .translation.transliteration {
    font-style: italic;
  }

  .word[data-ratio="first"],
  .ratio-first {
    color: var(--ratio-first);
  }

  .word[data-ratio="second"],
  .ratio-second {
    color: var(--ratio-second);
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
