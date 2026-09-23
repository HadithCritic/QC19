import { describeError, engine } from "../engine/client";
import type { Bookmark, Chapter, CountingOptions, Distance, EngineInfo, ValueSystem, VerseRange, WordLocation } from "../engine/types";
import { DEFAULT_COUNTING, parseCounting, type CountingKey } from "../counting";
import { back, canGoBack, canGoForward, current, EMPTY_NAVIGATION, forward, visit, type Navigation } from "../navigation";
import { chapterOfVerse, lastVerse } from "../numbers";
import { visibleSystems } from "../systems";

export type View = "read" | "search" | "values" | "numbers" | "saved";

/** How long a selection must stay before it is written to browse history. */
const BROWSE_RECORD_DELAY_MS = 1000;
export type Theme = "system" | "light" | "dark";

interface Settings {
  theme: Theme;
  research: boolean;
  valueSystem: string | null;
  counting: CountingOptions;
}

const SETTINGS_KEY = "qurancode.settings.v1";
const DEFAULT_SETTINGS: Settings = { theme: "system", research: false, valueSystem: null, counting: DEFAULT_COUNTING };

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

  view = $state<View>("read");
  chapter = $state(1);
  selection = $state<VerseRange | null>(null);
  /** Verse to bring into view in the reader, set by navigation. */
  focusVerse = $state<number | null>(null);
  /** Ranges visited this session, for Back and Forward. */
  navigation = $state<Navigation>(EMPTY_NAVIGATION);
  canGoBack = $derived(canGoBack(this.navigation));
  canGoForward = $derived(canGoForward(this.navigation));
  private browseTimer: ReturnType<typeof setTimeout> | undefined;

  /** The word last clicked in the reader, for F4, F7 and F8: its verse, display index (Bismillah header included) and text. */
  currentWord = $state<{ verse: number; word: number; text: string } | null>(null);

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
  }

  async start(): Promise<void> {
    this.status = "starting";
    this.startupError = null;
    try {
      const [info, chapters, systems] = await Promise.all([engine.info(), engine.chapters(), engine.systems()]);
      this.info = info;
      this.chapters = chapters;
      this.allSystems = systems;

      // A remembered system may be gone or hidden; fall back to the default.
      const remembered = visibleSystems(systems, this.research).some((s) => s.name === this.valueSystem);
      if (!remembered) this.valueSystem = info.defaultValueSystem;
      this.status = "ready";
      void this.loadBookmarks();
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
    });
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

  /** Click selects one verse; shift-click extends from the current anchor. */
  selectVerse(verse: number, extend: boolean): void {
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
