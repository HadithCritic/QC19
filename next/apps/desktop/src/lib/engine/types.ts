// Mirror of apps/QuranCode.Engine/Protocol/Dtos.cs. The engine is the
// authority; change that file first and this one to match.

export type ClassCode = "" | "U" | "AP" | "XP" | "AC" | "XC";

export type BasmalaMode = "prefix" | "verse-zero";

export interface EngineInfo {
  version: string;
  edition: string;
  /** "verse-zero": the Bismillah is its own verse 0, which may be left uncounted. */
  basmala: BasmalaMode;
  chapterCount: number;
  /** Numbered verses, not counting verse-0 Bismillahs. */
  verseCount: number;
  /** Every verse row, verse-0 Bismillahs included. */
  rowCount: number;
  valueSystemCount: number;
  defaultValueSystem: string;
}

export interface Chapter {
  number: number;
  name: string;
  transliteratedName: string;
  englishName: string;
  revelationOrder: number;
  revelationPlace: string;
  /** Numbered verses, not counting a verse 0. */
  verseCount: number;
  /** Absolute number of the chapter's first row: its verse 0 when it has one. */
  firstVerse: number;
  hasVerseZero: boolean;
}

export interface ValueSystem {
  name: string;
  textMode: string;
  letterOrder: string;
  letterValue: string;
  researchOnly: boolean;
}

export interface Verse {
  number: number;
  chapter: number;
  numberInChapter: number;
  /** A verse-0 Bismillah, which the user may choose not to count. */
  isBasmala: boolean;
  bismillah: string | null;
  words: string[];
}

/** `value` is a decimal string: totals can exceed what a JS number holds exactly. */
export interface NumberInfo {
  value: string;
  class: string;
  code: ClassCode;
  digitSum: number;
  digitalRoot: number;
  familyOrdinal: number | null;
  classOrdinal: number | null;
  factors: number[] | null;
}

/** `value` and `code` are null for a Bismillah that is not being counted. */
export interface VerseValue {
  number: number;
  value: string | null;
  code: ClassCode | null;
}

export interface LetterCount {
  letter: string;
  count: number;
}

export interface Stats {
  first: number;
  last: number;
  valueSystem: string;
  chapters: NumberInfo;
  verses: NumberInfo;
  words: NumberInfo;
  letters: NumberInfo;
  distinctLetters: NumberInfo;
  value: NumberInfo;
  letterFrequencies: LetterCount[];
}

export interface VerseRange {
  first: number;
  last: number;
}

export interface SystemValue {
  valueSystem: string;
  letterCount: number;
  value: NumberInfo;
}

export type Wordness = "any" | "whole" | "part";

export interface SearchVerse extends Verse {
  highlights: number[];
  /** False when words could not be mapped; mark the whole verse instead. */
  aligned: boolean;
  /** Bismillah words to highlight; the engine counts the header as part of verse 1. */
  bismillahHighlights: number[];
  matchCount: number;
}

export interface SearchResult {
  term: string;
  wordCount: number;
  verseCount: number;
  offset: number;
  verses: SearchVerse[];
}
