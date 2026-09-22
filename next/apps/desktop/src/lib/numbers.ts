import type { ClassCode, NumberInfo } from "./engine/types";

export const CLASS_NAMES: Record<ClassCode, string> = {
  "": "Zero",
  U: "Unit",
  AP: "Additive prime",
  XP: "Non-additive prime",
  AC: "Additive composite",
  XC: "Non-additive composite",
};

/** What makes the class what it is, in one line. */
export const CLASS_RULES: Record<ClassCode, string> = {
  "": "Zero has no class.",
  U: "One: divisible by nothing smaller.",
  AP: "Prime, and its digit sum is prime.",
  XP: "Prime, but its digit sum is not.",
  AC: "Composite, and its digit sum is composite.",
  XC: "Composite, but its digit sum is not.",
};

export function isPrimeClass(code: ClassCode): boolean {
  return code === "AP" || code === "XP";
}

/** "P114" or "C506": the ordinal within primes or composites, as the Readme writes it. */
export function familyLabel(info: NumberInfo): string | null {
  if (info.familyOrdinal === null) return null;
  return `${isPrimeClass(info.code) ? "P" : "C"}${info.familyOrdinal}`;
}

/** "AP16": the ordinal within the leaf class. */
export function classLabel(info: NumberInfo): string | null {
  if (info.classOrdinal === null || info.code === "") return null;
  return `${info.code}${info.classOrdinal}`;
}

/** 2 × 3 × 19, with repeated factors folded into powers: 2³ × 3. */
export function formatFactors(factors: readonly number[]): string {
  const counts = new Map<number, number>();
  for (const factor of factors) counts.set(factor, (counts.get(factor) ?? 0) + 1);
  return [...counts]
    .map(([factor, power]) => (power === 1 ? `${factor}` : `${factor}${superscript(power)}`))
    .join(" × ");
}

const SUPERSCRIPTS = "⁰¹²³⁴⁵⁶⁷⁸⁹";

function superscript(n: number): string {
  return [...String(n)].map((d) => SUPERSCRIPTS[Number(d)]).join("");
}

/** The parts of a chapter that decide how its verses are numbered. */
export interface ChapterRows {
  number: number;
  firstVerse: number;
  verseCount: number;
  hasVerseZero: boolean;
}

/** Rows a chapter occupies: its numbered verses plus a verse 0 if it has one. */
export function rowCount(chapter: ChapterRows): number {
  return chapter.verseCount + (chapter.hasVerseZero ? 1 : 0);
}

export function lastVerse(chapter: ChapterRows): number {
  return chapter.firstVerse + rowCount(chapter) - 1;
}

export function chapterOfVerse(verse: number, chapters: readonly ChapterRows[]): ChapterRows | undefined {
  return chapters.find((c) => verse >= c.firstVerse && verse <= lastVerse(c));
}

/** Chapter:verse for an absolute verse number; a verse-0 Bismillah reads "2:0". */
export function verseReference(verse: number, chapters: readonly ChapterRows[]): string {
  const chapter = chapterOfVerse(verse, chapters);
  if (!chapter) return `${verse}`;
  const inChapter = verse - chapter.firstVerse + (chapter.hasVerseZero ? 0 : 1);
  return `${chapter.number}:${inChapter}`;
}

/** "2:255", "2:255–257" or "1:7 – 2:2" for a range. */
export function rangeReference(first: number, last: number, chapters: readonly ChapterRows[]): string {
  if (first === last) return verseReference(first, chapters);
  const a = verseReference(first, chapters);
  const b = verseReference(last, chapters);
  const [chapterA] = a.split(":");
  const [chapterB, verseB] = b.split(":");
  return chapterA === chapterB ? `${a}–${verseB}` : `${a} – ${b}`;
}
