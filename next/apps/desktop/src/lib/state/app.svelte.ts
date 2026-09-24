import { describeError, engine } from "../engine/client";
import type {
  Bookmark,
  Chapter,
  CountingOptions,
  Distance,
  EngineInfo,
  QuranLocation,
  QuranSelection,
  RatioOptions,
  ResearchSelection,
  Scope,
  Translation,
  ValueSystem,
  VerseRange,
  WordLocation,
} from "../engine/types";
import { DEFAULT_COUNTING, parseCounting, type CountingKey } from "../counting";
import { back, canGoBack, canGoForward, current, EMPTY_NAVIGATION, forward, visit, type Navigation } from "../navigation";
import { DEFAULT_DIVISOR, DEFAULT_RADIX, wrapDivisor, wrapRadix } from "../numberDisplay";
import { DEFAULT_RATIO } from "../ratioColors";
import { chapterOfVerse, lastVerse } from "../numbers";
import { visibleSystems } from "../systems";
import { envelope, expand, isExact, pick, verseSelection, type Level, type SelectMode } from "../selection";

export type View = "read" | "search" | "values" | "numbers" | "statistics" | "findings" | "initials" | "saved";

/** How long a selection must stay before it is written to browse history. */
const BROWSE_RECORD_DELAY_MS = 1000;
export type Theme = "system" | "light" | "dark";

interface Settings {
  theme: Theme;
  research: boolean;
  valueSystem: string | null;
  counting: CountingOptions;
  /** Numbers divisible by this are marked (Features.txt #16). */
  divisor: number;
  /** The base numbers are shown in (Features.txt #71). */
  radix: number;
  /** Translations shown under each verse, by key; null until the reader first chooses. */
  translations: string[] | null;
}

const SETTINGS_KEY = "qurancode.settings.v1";
const DEFAULT_SETTINGS: Settings = {
  theme: "system",
  research: false,
  valueSystem: null,
  counting: DEFAULT_COUNTING,
  divisor: DEFAULT_DIVISOR,
  radix: DEFAULT_RADIX,
  translations: null,
};

function wholeNumber(value: unknown, fallback: number, wrap: (n: number) => number): number {
  return typeof value === "number" && Number.isInteger(value) ? wrap(value) : fallback;
}

// Preferences are a per-user convenience. Storage can be unavailable or
// hold junk from an older build; either way the app starts on defaults.
function loadSettings(): Settings {
  try {
    const raw = localStorage.getItem(SETTINGS_KEY);
    if (!raw) return DEFAULT_SETTINGS;
    const parsed = JSON.parse(raw) as Partial<Settings> & { countBasmalas?: unknown };
    const counting = parseCounting(parsed.counting);
    // Settings from before the counting options stored only this flag.
    if (parsed.counting === undefined && parsed.countBasmalas === false) counting.includeBasmalas = false;
    return {
      theme: parsed.theme === "light" || parsed.theme === "dark" ? parsed.theme : "system",
      research: parsed.research === true,
      valueSystem: typeof parsed.valueSystem === "string" ? parsed.valueSystem : null,
      counting,
      divisor: wholeNumber(parsed.divisor, DEFAULT_DIVISOR, wrapDivisor),
      radix: wholeNumber(parsed.radix, DEFAULT_RADIX, wrapRadix),
      translations: Array.isArray(parsed.translations) ? parsed.translations.filter((k): k is string => typeof k === "string") : null,
    };
  } catch {
    return DEFAULT_SETTINGS;
  }
}

function saveSettings(settings: Settings): void {
  try {
    localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
  } catch {
    // Not persisting a preference is not worth interrupting the user for.
  }
}

class AppState {
  status = $state<"starting" | "ready" | "failed">("starting");
  startupError = $state<string | null>(null);

  info = $state<EngineInfo | null>(null);
  chapters = $state<Chapter[]>([]);
  allSystems = $state<ValueSystem[]>([]);
  /** Translations and other verse texts the edition has. */
  allTranslations = $state<Translation[]>([]);
  /** Keys of the translations shown under each verse. */
  shownTranslations = $state<string[]>([]);
  private remembered: string[] | null = null;

  view = $state<View>("read");
  chapter = $state(1);
  selection = $state<VerseRange | null>(null);
  /**
   * An exact selection of words or letters, which the reader highlights and
   * every analysis uses; null when the selection is whole verses. While it is
   * set, `selection` holds the verses it touches, so everything that works
   * on verses (search scope, bookmarks, history) keeps working.
   */
  exact = $state<QuranSelection | null>(null);
  /** The first click of an exact selection, waiting for the second. */
  pendingStart = $state<QuranLocation | null>(null);
  /** What a click in the reader selects. */
  selectMode = $state<SelectMode>("verse");
  /** What analyses run over: the exact selection, or the selected verses. */
  scope = $derived<Scope | null>(this.exact ? { selection: this.exact } : this.selection);
  /** Saved research selections; null while unavailable (no user data file). */
  researchSelections = $state<ResearchSelection[] | null>(null);

  /** Verse to bring into view in the reader, set by navigation. */
  focusVerse = $state<number | null>(null);
  /** Ranges visited this session, for Back and Forward. */
  navigation = $state<Navigation>(EMPTY_NAVIGATION);
  canGoBack = $derived(canGoBack(this.navigation));
  canGoForward = $derived(canGoForward(this.navigation));
  private browseTimer: ReturnType<typeof setTimeout> | undefined;

  /** The word last clicked in the reader, for F4, F7 and F8: its verse, display index (Bismillah header included) and text. */
  currentWord = $state<{ verse: number; word: number; text: string } | null>(null);

  /** Ratio coloring in the reader (Features.txt #24), for this session. */
  ratioOn = $state(false);
  ratioOptions = $state<RatioOptions>({ ...DEFAULT_RATIO });

  /** A number another view sent to the Numbers view to look up. */
  numberToOpen = $state<string | null>(null);

  /** Bookmarks, loaded at start; null while unavailable (no user data file). */
  bookmarks = $state<Bookmark[] | null>(null);

  /** Alt+click a word to start measuring; the next Alt+click measures to it. */
  measureFrom = $state<WordLocation | null>(null);
  measurement = $state<{ from: WordLocation; to: WordLocation; distance: Distance } | null>(null);
  measureError = $state<string | null>(null);

  theme = $state<Theme>("system");
  research = $state(false);
  valueSystem = $state("");
  /** How the text is counted; the engine applies only what the text mode allows. */
  counting = $state<CountingOptions>({ ...DEFAULT_COUNTING });
  divisor = $state(DEFAULT_DIVISOR);
  radix = $state(DEFAULT_RADIX);

  systems = $derived(visibleSystems(this.allSystems, this.research));
  hasVerseZero = $derived(this.info?.basmala === "verse-zero");
  /** Whether verse-0 Bismillahs are left out of the counts. */
  verseZeroExcluded = $derived(this.hasVerseZero && !this.counting.includeBasmalas);
  currentSystem = $derived(this.allSystems.find((s) => s.name === this.valueSystem));

  constructor() {
    const settings = loadSettings();
    this.theme = settings.theme;
    this.research = settings.research;
    this.valueSystem = settings.valueSystem ?? "";
    this.counting = settings.counting;
    this.divisor = settings.divisor;
    this.radix = settings.radix;
    this.remembered = settings.translations;
  }

  async start(): Promise<void> {
    this.status = "starting";
    this.startupError = null;
    // A selection belongs to the content it was made in.
    this.exact = null;
    this.pendingStart = null;
    try {
      const [info, chapters, systems, translations] = await Promise.all([
        engine.info(),
        engine.chapters(),
        engine.systems(),
        engine.translations(),
      ]);
      this.allTranslations = translations;
      // Until the reader chooses, the first translation is shown (the edition's own English).
      const known = new Set(translations.map((t) => t.key));
      const firstTranslation = translations.find((t) => t.kind === "translation");
      this.shownTranslations = this.remembered
        ? this.remembered.filter((k) => known.has(k))
        : firstTranslation
          ? [firstTranslation.key]
          : [];
      this.info = info;
      this.chapters = chapters;
      this.allSystems = systems;

      // A remembered system may be gone or hidden; fall back to the default.
      const remembered = visibleSystems(systems, this.research).some((s) => s.name === this.valueSystem);
      if (!remembered) this.valueSystem = info.defaultValueSystem;
      this.status = "ready";
      void this.loadBookmarks();
      void this.loadResearchSelections();
    } catch (error) {
      this.startupError = describeError(error);
      this.status = "failed";
    }
  }

  persist(): void {
    saveSettings({
      theme: this.theme,
      research: this.research,
      valueSystem: this.valueSystem,
      counting: { ...this.counting },
      divisor: this.divisor,
      radix: this.radix,
      // Before the edition's list arrives, keep what was remembered.
      translations: this.status === "ready" ? [...this.shownTranslations] : this.remembered,
    });
  }

  toggleTranslation(key: string, on: boolean): void {
    const rest = this.shownTranslations.filter((k) => k !== key);
    this.shownTranslations = on ? [...rest, key] : rest;
    this.persist();
  }

  setShownTranslations(keys: string[]): void {
    this.shownTranslations = [...keys];
    this.persist();
  }

  /** Looks a number up in the Numbers view. */
  openNumber(value: string): void {
    this.numberToOpen = value;
    this.view = "numbers";
  }

  setDivisor(divisor: number): void {
    this.divisor = wrapDivisor(divisor);
    this.persist();
  }

  setRadix(radix: number): void {
    this.radix = wrapRadix(radix);
    this.persist();
  }

  setResearch(on: boolean): void {
    this.research = on;
    // Leaving research mode must not strand the user on a hidden system.
    if (!on && this.currentSystem?.researchOnly && this.info) this.valueSystem = this.info.defaultValueSystem;
    this.persist();
  }

  setSystem(name: string): void {
    this.valueSystem = name;
    this.persist();
  }

  setCounting(key: CountingKey, on: boolean): void {
    this.counting = { ...this.counting, [key]: on };
    this.persist();
  }

  resetCounting(): void {
    this.counting = { ...DEFAULT_COUNTING };
    this.persist();
  }

  setTheme(theme: Theme): void {
    this.theme = theme;
    this.persist();
  }

  chapterOf(verse: number): Chapter | undefined {
    return chapterOfVerse(verse, this.chapters) as Chapter | undefined;
  }

  /** Opens the reader on a range and selects it. */
  goTo(range: VerseRange): void {
    if (!this.show(range, range.first)) return;
    this.view = "read";
  }

  openChapter(number: number): void {
    const chapter = this.chapters[number - 1];
    if (chapter) this.show({ first: chapter.firstVerse, last: lastVerse(chapter) }, null);
  }

  setSelectMode(mode: SelectMode): void {
    this.selectMode = mode;
    this.pendingStart = null;
  }

  /** A click on a word or letter: starts, finishes or extends the exact selection. */
  pickLocation(location: QuranLocation, shift: boolean): void {
    const next = pick(this.exact, this.pendingStart, location, shift);
    this.pendingStart = next.pending;
    this.setExact(next.selection, false);
  }

  /**
   * Makes a selection the active one: exact when it names words or letters,
   * whole verses otherwise. With `reveal`, the reader opens on its start.
   */
  setExact(selection: QuranSelection, reveal = true): void {
    const range = envelope(selection, this.chapters);
    if (!range) return;
    if (!isExact(selection)) {
      this.exact = null;
      this.pendingStart = null;
      if (reveal) this.goTo(range);
      else this.select(range);
      return;
    }
    this.exact = selection;
    if (reveal) {
      const chapter = this.chapterOf(range.first);
      if (chapter) this.chapter = chapter.number;
      this.focusVerse = range.first;
      this.view = "read";
    }
    this.select(range);
  }

  /** Widens the selection so both ends are whole words, verses or chapters. */
  expandSelection(level: Exclude<Level, "letter">): void {
    const current = this.exact ?? (this.selection ? verseSelection(this.selection, this.chapters) : null);
    if (current) this.setExact(expand(current, level), false);
  }

  /** Clears the selection, exact or not. */
  clearSelection(): void {
    this.exact = null;
    this.pendingStart = null;
    this.selection = null;
  }

  async loadResearchSelections(): Promise<void> {
    try {
      this.researchSelections = await engine.researchSelections();
    } catch {
      this.researchSelections = null; // unavailable: the Saved view says so
    }
  }

  /** Saves the active selection for research, with the current system and counting. */
  async saveResearchSelection(title: string, note: string, id?: number): Promise<ResearchSelection | null> {
    const selection = this.exact ?? (this.selection ? verseSelection(this.selection, this.chapters) : null);
    if (!selection) return null;
    const saved = await engine.saveResearchSelection({
      selection,
      ...(id !== undefined ? { id } : {}),
      title,
      note,
      valueSystem: this.valueSystem,
      counting: { ...this.counting },
    });
    this.researchSelections = [saved, ...(this.researchSelections ?? []).filter((r) => r.id !== saved.id)];
    return saved;
  }

  /** Renames a saved selection or changes its note, keeping what it was studied under. */
  async editResearchSelection(saved: ResearchSelection, title: string, note: string): Promise<void> {
    if (!saved.selection) return;
    const edited = await engine.saveResearchSelection({
      selection: saved.selection,
      id: saved.id,
      title,
      note,
      ...(saved.valueSystem ? { valueSystem: saved.valueSystem } : {}),
      ...(saved.counting ? { counting: saved.counting } : {}),
    });
    this.researchSelections = (this.researchSelections ?? []).map((r) => (r.id === edited.id ? edited : r));
  }

  async deleteResearchSelection(id: number): Promise<void> {
    await engine.deleteResearchSelection(id);
    this.researchSelections = (this.researchSelections ?? []).filter((r) => r.id !== id);
  }

  /** Reopens a saved selection with the system and counting it was saved under. */
  openResearchSelection(saved: ResearchSelection): void {
    if (!saved.selection) return;
    if (saved.valueSystem && this.allSystems.some((s) => s.name === saved.valueSystem)) this.setSystem(saved.valueSystem);
    if (saved.counting) {
      this.counting = { ...saved.counting };
      this.persist();
    }
    this.setExact(saved.selection);
  }

  /** Click selects one verse; shift-click extends from the current anchor. */
  selectVerse(verse: number, extend: boolean): void {
    this.exact = null;
    this.pendingStart = null;
    const anchor = this.selection;
    const range =
      extend && anchor
        ? verse >= anchor.first
          ? { first: anchor.first, last: verse }
          : { first: verse, last: anchor.last }
        : { first: verse, last: verse };
    this.select(range);
  }

  /** Measures from the previous Alt+clicked word to this one, then starts again from it. */
  async measureTo(to: WordLocation): Promise<void> {
    const from = this.measureFrom;
    this.measureFrom = to;
    this.measureError = null;
    if (!from) {
      this.measurement = null;
      return;
    }
    try {
      const distance = await engine.distance(from, to, this.valueSystem, { ...this.counting });
      this.measurement = { from, to, distance };
    } catch (error) {
      this.measurement = null;
      this.measureError = describeError(error);
    }
  }

  /**
   * Fetches the value systems again, after the reader defines or removes a
   * text mode. A chosen system that went with its mode falls back to the
   * default.
   */
  async reloadSystems(): Promise<void> {
    const systems = await engine.systems();
    this.allSystems = systems;
    if (!visibleSystems(systems, this.research).some((s) => s.name === this.valueSystem)) {
      this.valueSystem = this.info?.defaultValueSystem ?? this.valueSystem;
    }
  }

  async loadBookmarks(): Promise<void> {
    try {
      this.bookmarks = await engine.bookmarks();
    } catch {
      this.bookmarks = null; // bookmarks unavailable: the controls hide themselves
    }
  }

  bookmarkFor(range: VerseRange): Bookmark | undefined {
    return this.bookmarks?.find((b) => b.first === range.first && b.last === range.last);
  }

  /** Adds or updates the bookmark on a range. */
  async saveBookmark(range: VerseRange, note: string): Promise<Bookmark> {
    const saved = await engine.saveBookmark(range, note);
    const others = (this.bookmarks ?? []).filter((b) => b.id !== saved.id);
    this.bookmarks = [...others, saved];
    return saved;
  }

  async deleteBookmark(id: number): Promise<void> {
    await engine.deleteBookmark(id);
    this.bookmarks = (this.bookmarks ?? []).filter((b) => b.id !== id);
  }

  clearMeasurement(): void {
    this.measureFrom = null;
    this.measurement = null;
    this.measureError = null;
  }

  goBack(): void {
    this.navigation = back(this.navigation);
    this.showCurrent();
  }

  goForward(): void {
    this.navigation = forward(this.navigation);
    this.showCurrent();
  }

  private showCurrent(): void {
    const range = current(this.navigation);
    if (range) this.show(range, range.first, false);
    this.view = "read";
  }

  /** Selects a range in its chapter; false when no chapter holds it. */
  private show(range: VerseRange, focus: number | null, record = true): boolean {
    const chapter = this.chapterOf(range.first);
    if (!chapter) return false;
    this.exact = null;
    this.pendingStart = null;
    this.chapter = chapter.number;
    this.focusVerse = focus;
    this.select(range, record);
    return true;
  }

  private select(range: VerseRange, record = true): void {
    this.selection = range;
    if (!record) return;
    this.navigation = visit(this.navigation, range);

    // Only a selection the reader stays on is worth keeping in history.
    clearTimeout(this.browseTimer);
    this.browseTimer = setTimeout(() => {
      engine.addBrowse(range).catch(() => {
        // History is a convenience; failing to record it is not an error to show.
      });
    }, BROWSE_RECORD_DELAY_MS);
  }
}

export const app = new AppState();
