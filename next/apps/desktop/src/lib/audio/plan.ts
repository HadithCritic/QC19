// What the recitation player plays, and the pauses between, following the
// original's MainForm player (Features.txt #45, #56, #65). Pure, so it can be
// tested apart from audio playback.

/** A verse the player can play: its absolute number, chapter and number in chapter. */
export interface PlayVerse {
  number: number;
  chapter: number;
  verse: number;
  /** Last verse of its chapter, for the pause at a chapter's end. */
  lastInChapter: boolean;
  prostration: "recommended" | "obligatory" | null;
}

/** One recording: the bundled Audhubillah, or a reciter's verse file named CCCVVV. */
export type Recording = { kind: "audhubillah" } | { kind: "verse"; name: string };

/** The reciter's file for a verse: a verse 0 Bismillah is 1:1's recording. */
export function fileName(chapter: number, verse: number): string {
  if (verse === 0) return "001001";
  return `${String(chapter).padStart(3, "0")}${String(verse).padStart(3, "0")}`;
}

const BISMILLAH: Recording = { kind: "verse", name: "001001" };
const AUDHUBILLAH: Recording = { kind: "audhubillah" };

/**
 * Recordings played before a verse. The first play of the session opens with
 * the Audhubillah and the Bismillah (none for chapter 9, and none when
 * starting at 1:1 or at a verse 0, which is the Bismillah itself). After
 * that, verse 1 of every chapter but 1 and 9 gets the Bismillah, unless its
 * verse 0 was just played, and 9:1 gets the Audhubillah.
 */
export function preamble(verse: PlayVerse, previous: PlayVerse | null, firstPlay: boolean): Recording[] {
  const bismillahNeeded = verse.chapter !== 9 && verse.verse !== 0 && !(verse.chapter === 1 && verse.verse === 1);
  if (firstPlay) return bismillahNeeded ? [AUDHUBILLAH, BISMILLAH] : [AUDHUBILLAH];
  if (verse.chapter === 9 && verse.verse === 1) return [AUDHUBILLAH];
  const afterVerseZero = previous !== null && previous.chapter === verse.chapter && previous.verse === 0;
  if (verse.verse === 1 && verse.chapter !== 1 && verse.chapter !== 9 && !afterVerseZero) return [BISMILLAH];
  return [];
}

/** Pause after a verse's prostration: 10 s when obligatory, 5 s when recommended. */
const PROSTRATION_MS = { obligatory: 10_000, recommended: 5_000 } as const;

/** The pause at a chapter's end when there is no silence between verses. */
export const CHAPTER_END_MS = 3000;

/**
 * Milliseconds to wait after one play of a verse: silence of `factor` times
 * its length (0 to 2), the chapter-end pause when there is no silence and
 * this was the verse's last repeat, and the prostration pause.
 */
export function pauseAfter(verse: PlayVerse, lengthMs: number, factor: number, lastRepeat: boolean): number {
  let pause = Math.max(0, lengthMs * factor);
  if (factor === 0 && lastRepeat && verse.lastInChapter) pause += CHAPTER_END_MS;
  if (verse.prostration) pause += PROSTRATION_MS[verse.prostration];
  return pause;
}

/** The original's silence-between-selections steps: none, 10 s, 1 min ... 1 day, random. */
export const SELECTION_SILENCES: { label: string; ms: number | "random" }[] = [
  { label: "none", ms: 0 },
  { label: "10 seconds", ms: 10_000 },
  { label: "1 minute", ms: 60_000 },
  { label: "5 minutes", ms: 300_000 },
  { label: "15 minutes", ms: 900_000 },
  { label: "1 hour", ms: 3_600_000 },
  { label: "2 hours", ms: 7_200_000 },
  { label: "6 hours", ms: 21_600_000 },
  { label: "12 hours", ms: 43_200_000 },
  { label: "1 day", ms: 86_400_000 },
  { label: "random, 10 seconds to a day", ms: "random" },
];

/** Milliseconds of a selection silence step; the random step draws from 10 s to 24 h. */
export function selectionSilence(step: number, random: () => number = Math.random): number {
  const entry = SELECTION_SILENCES[Math.max(0, Math.min(SELECTION_SILENCES.length - 1, Math.round(step)))]!;
  if (entry.ms !== "random") return entry.ms;
  return Math.round(10_000 + random() * (86_400_000 - 10_000));
}

/** Repeat counts the original cycles through; Infinity is unlimited. */
export const REPEAT_COUNTS = [Infinity, 2, 3, 5, 7] as const;

/** The next count in the cycle (the original's click), or the previous one with Shift. */
export function nextRepeat(count: number, backward = false): number {
  const index = REPEAT_COUNTS.indexOf(count as (typeof REPEAT_COUNTS)[number]);
  const at = index < 0 ? 0 : index;
  const step = backward ? REPEAT_COUNTS.length - 1 : 1;
  return REPEAT_COUNTS[(at + step) % REPEAT_COUNTS.length]!;
}

/**
 * What Previous does: within the first 3 seconds of a verse it goes to the
 * previous verse; later it starts the verse again (the original's rule).
 */
export function previousIndex(index: number, positionMs: number): number {
  return positionMs < 3000 ? Math.max(0, index - 1) : index;
}
