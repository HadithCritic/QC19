import type { QuranLocation, QuranSelection, VerseRange } from "./engine/types";
import { chapterOfVerse, lastVerse, type ChapterRows } from "./numbers";

// The reader's exact selection, from a chapter down to a letter, in display
// coordinates: verse in chapter (0 for a verse 0), 1-based display word and
// 1-based display letter. The engine resolves it to counted text; nothing
// here depends on the text mode or value system. See
// docs/specs/research-selection.md.

export type SelectMode = "verse" | "word" | "letter";

export type Level = "chapter" | "verse" | "word" | "letter";

export function at(chapter: number, verse: number | null = null, word: number | null = null, letter: number | null = null): QuranLocation {
  return { chapter, verse, word, letter };
}

export function formatLocation(location: QuranLocation): string {
  let text = `${location.chapter}`;
  if (location.verse !== null) text += `:${location.verse}`;
  if (location.word !== null) text += `:w${location.word}`;
  if (location.letter !== null) text += `:l${location.letter}`;
  return text;
}

/** The canonical address, as the engine writes it. */
export function formatSelection(selection: QuranSelection): string {
  const a = formatLocation(selection.start);
  const b = formatLocation(selection.end);
  return a === b ? a : `${a}-${b}`;
}

// Where a location begins and ends, for comparison: a missing part is the
// whole of the part above it.
const BEFORE = -1;
const AFTER = Number.MAX_SAFE_INTEGER;

function begin(location: QuranLocation): number[] {
  return [location.chapter, location.verse ?? BEFORE, location.word ?? BEFORE, location.letter ?? BEFORE];
}

function end(location: QuranLocation): number[] {
  return [location.chapter, location.verse ?? AFTER, location.word ?? AFTER, location.letter ?? AFTER];
}

function compare(a: readonly number[], b: readonly number[]): number {
  for (let i = 0; i < a.length; i++) {
    const d = (a[i] ?? 0) - (b[i] ?? 0);
    if (d !== 0) return d;
  }
  return 0;
}

/** The same selection with its endpoints in Quran order. */
export function ordered(selection: QuranSelection): QuranSelection {
  return compare(begin(selection.end), begin(selection.start)) < 0 ? { start: selection.end, end: selection.start } : selection;
}

/** Grows a selection to a location, from whichever end is nearer in order. */
export function extend(selection: QuranSelection, location: QuranLocation): QuranSelection {
  const s = ordered(selection);
  return compare(begin(location), begin(s.start)) < 0 ? { start: location, end: s.end } : { start: s.start, end: location };
}

/**
 * One click in the reader. The first click starts a selection of that one
 * unit and waits; the next finishes it. Shift extends a selection instead.
 */
export function pick(
  selection: QuranSelection | null,
  pending: QuranLocation | null,
  location: QuranLocation,
  shift: boolean,
): { selection: QuranSelection; pending: QuranLocation | null } {
  if (shift && selection) return { selection: extend(selection, location), pending: null };
  if (pending) return { selection: ordered({ start: pending, end: location }), pending: null };
  return { selection: { start: location, end: location }, pending: location };
}

/**
 * How much of a display word a selection covers: none, all of it, or the
 * 1-based letters from and to.
 */
export function coverage(
  selection: QuranSelection,
  chapter: number,
  verse: number,
  word: number,
  text: string,
): "none" | "full" | { from: number; to: number } {
  const s = ordered(selection);
  const wordStart = [chapter, verse, word, BEFORE];
  const wordEnd = [chapter, verse, word, AFTER];
  if (compare(wordEnd, begin(s.start)) < 0 || compare(wordStart, end(s.end)) > 0) return "none";

  const cutAtStart = s.start.letter !== null && sameWord(s.start, chapter, verse, word);
  const cutAtEnd = s.end.letter !== null && sameWord(s.end, chapter, verse, word);
  if (!cutAtStart && !cutAtEnd) return "full";

  const count = letterClusters(text).length;
  const from = cutAtStart ? (s.start.letter ?? 1) : 1;
  const to = cutAtEnd ? Math.min(s.end.letter ?? count, count) : count;
  return from === 1 && to === count ? "full" : { from, to };
}

function sameWord(location: QuranLocation, chapter: number, verse: number, word: number): boolean {
  return location.chapter === chapter && location.verse === verse && location.word === word;
}

/** Whether a display word is where a word- or letter-level selection starts or ends. */
export function endpointOf(selection: QuranSelection, chapter: number, verse: number, word: number): "start" | "end" | "both" | null {
  const s = ordered(selection);
  const isStart = sameWord(s.start, chapter, verse, word);
  const isEnd = sameWord(s.end, chapter, verse, word);
  return isStart && isEnd ? "both" : isStart ? "start" : isEnd ? "end" : null;
}

// A display letter is one Arabic letter (Lo), tatweel excluded, with the
// marks after it; marks before the first letter are the first letter's. This
// mirrors QuranCode.Core.Text.DisplayLetters, which numbers letters for the engine.
const LETTER = /\p{Lo}/u;
const TATWEEL = "ـ";

function isLetter(c: string): boolean {
  return c !== TATWEEL && LETTER.test(c);
}

/** A display word's letters, each with its marks, in order; they join back into the word. */
export function letterClusters(word: string): string[] {
  const chars = [...word];
  const starts: number[] = [];
  chars.forEach((c, i) => {
    if (isLetter(c)) starts.push(i);
  });
  return starts.map((s, k) => chars.slice(k === 0 ? 0 : s, starts[k + 1] ?? chars.length).join(""));
}

function absolute(chapter: ChapterRows, verse: number): number {
  return chapter.firstVerse + verse - (chapter.hasVerseZero ? 0 : 1);
}

/** The absolute verses a selection names, counted or not. */
export function envelope(selection: QuranSelection, chapters: readonly ChapterRows[]): VerseRange | null {
  const s = ordered(selection);
  const a = chapters[s.start.chapter - 1];
  const b = chapters[s.end.chapter - 1];
  if (!a || !b) return null;
  return {
    first: s.start.verse === null ? a.firstVerse : absolute(a, s.start.verse),
    last: s.end.verse === null ? lastVerse(b) : absolute(b, s.end.verse),
  };
}

function truncate(location: QuranLocation, level: Level): QuranLocation {
  switch (level) {
    case "chapter":
      return at(location.chapter);
    case "verse":
      return at(location.chapter, location.verse);
    case "word":
      return at(location.chapter, location.verse, location.word);
    default:
      return location;
  }
}

/** The selection widened so both ends are whole units of a level. */
export function expand(selection: QuranSelection, level: Exclude<Level, "letter">): QuranSelection {
  const s = ordered(selection);
  return { start: truncate(s.start, level), end: truncate(s.end, level) };
}

/** A verse range as a selection of verses. */
export function verseSelection(range: VerseRange, chapters: readonly ChapterRows[]): QuranSelection | null {
  const locate = (verse: number): QuranLocation | null => {
    const chapter = chapterOfVerse(verse, chapters);
    return chapter ? at(chapter.number, verse - chapter.firstVerse + (chapter.hasVerseZero ? 0 : 1)) : null;
  };
  const start = locate(range.first);
  const finish = locate(range.last);
  return start && finish ? { start, end: finish } : null;
}

/** Whether a selection names a word or letter, so it is finer than whole verses. */
export function isExact(selection: QuranSelection): boolean {
  return selection.start.word !== null || selection.end.word !== null;
}
