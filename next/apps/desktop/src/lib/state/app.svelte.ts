import { describeError, engine } from "../engine/client";
import type { Chapter, CountingOptions, EngineInfo, ValueSystem, VerseRange } from "../engine/types";
import { DEFAULT_COUNTING, parseCounting, type CountingKey } from "../counting";
import { chapterOfVerse, lastVerse } from "../numbers";
import { visibleSystems } from "../systems";

export type View = "read" | "search" | "values" | "numbers";
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
    const chapter = this.chapterOf(range.first);
    if (!chapter) return;
    this.chapter = chapter.number;
    this.selection = range;
    this.focusVerse = range.first;
    this.view = "read";
  }

  openChapter(number: number): void {
    this.chapter = number;
    this.focusVerse = null;
    const chapter = this.chapters[number - 1];
    if (chapter) this.selection = { first: chapter.firstVerse, last: lastVerse(chapter) };
  }

  /** Click selects one verse; shift-click extends from the current anchor. */
  selectVerse(verse: number, extend: boolean): void {
    const anchor = this.selection;
    if (extend && anchor) {
      const start = anchor.first;
      this.selection = verse >= start ? { first: start, last: verse } : { first: verse, last: anchor.last };
    } else {
      this.selection = { first: verse, last: verse };
    }
  }
}

export const app = new AppState();
