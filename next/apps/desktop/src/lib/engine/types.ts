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

/** Counted verses before and after a selection, in its chapter and in the book. */
export interface Position {
  beforeInChapter: number;
  afterInChapter: number;
  beforeInBook: number;
  afterInBook: number;
}

export interface Stats {
  first: number;
  last: number;
  valueSystem: string;
  position: Position;
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

export interface ChapterStats {
  chapter: number;
  verses: number;
  words: number;
  letters: number;
  value: string;
  code: ClassCode;
}

export interface WordLocation {
  verse: number;
  /** 0-based display word within the verse. */
  word: number;
}

export interface Distance {
  chapters: number;
  verses: number;
  words: number;
  letters: number;
}

/** First and last are null when the stored chapter:verse does not exist in this edition. */
export interface Bookmark {
  id: number;
  reference: string;
  first: number | null;
  last: number | null;
  note: string;
  createdUtc: string;
  updatedUtc: string;
}

export type HistoryKind = "browse" | "find";

export interface HistoryEntry {
  id: number;
  kind: HistoryKind;
  reference: string | null;
  first: number | null;
  last: number | null;
  term: string | null;
  wordness: Wordness | null;
  atUtc: string;
}

/** How the text is counted: the original's Statistics-panel options. */
export interface CountingOptions {
  includeBasmalas: boolean;
  wawAsWord: boolean;
  shaddaAsLetter: boolean;
  hamzaAboveLine: boolean;
  elfAboveLine: boolean;
  yaaAboveLine: boolean;
  noonAboveLine: boolean;
}

export interface SearchVerse extends Verse {
  highlights: number[];
  /** False when words could not be mapped; mark the whole verse instead. */
  aligned: boolean;
  /** Bismillah words to highlight; the engine counts the header as part of verse 1. */
  bismillahHighlights: number[];
  matchCount: number;
  /** Similarity to the starting verse (0 to 1), for a similar-verse search by text. */
  score: number | null;
}

export type Grouping = "any" | "all" | "phrase";
export type SearchMethod =
  | "search.text"
  | "search.roots"
  | "search.harakat"
  | "search.related"
  | "search.relatedVerses"
  | "search.similar";
export type SimilarityMethod = "text" | "words" | "roots" | "values";

/** A verse range, or a list of verses; omitted means the whole book. */
export interface SearchScope {
  first?: number;
  last?: number;
  verses?: number[];
}

export interface RootTerm {
  term: string;
  kind: "plain" | "required" | "excluded";
  root: string | null;
}

export interface SearchResult {
  term: string;
  wordCount: number;
  verseCount: number;
  offset: number;
  verses: SearchVerse[];
  /** Matches per chapter across the whole result, for shading the chapter list. */
  chapterCounts: number[];
  /** Every found verse, for searching within the results. */
  verseNumbers: number[];
  roots: RootTerm[] | null;
}
