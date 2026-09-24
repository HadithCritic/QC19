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
  /** Quranic initials: key (chapter 1), full, partial, double (42) or none. */
  initialization: "key" | "full" | "partial" | "double" | "none";
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

/** One find-and-replace rule of a reader's text mode. */
export interface TextRule {
  find: string;
  replace: string;
}

/** A text mode the reader defined on top of a stock one: its base's rules, then these. */
export interface TextMode {
  name: string;
  base: string;
  rules: TextRule[];
  description: string;
}

/** The reader's text modes, and the stock modes of this edition one can start from. */
export interface TextModes {
  bases: string[];
  modes: TextMode[];
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
  /** For a search in translations: the lines that matched, each match as [start, length]. */
  translations: { key: string; text: string; ranges: [number, number][] }[] | null;
}

export interface WordInfo {
  verse: number;
  word: number;
  text: string;
  meaning: string | null;
  transliteration: string | null;
  roots: string[];
  parts: {
    part: number;
    arabic: string;
    form: string;
    tag: string;
    tagEnglish: string | null;
    tagArabic: string | null;
    features: { text: string; english: string | null; arabic: string | null; script: string | null }[];
  }[];
}

export interface Translation {
  key: string;
  language: string;
  name: string;
  translator: string;
  kind: "translation" | "transliteration" | "emlaaei";
  rightToLeft: boolean;
}

/** One total of a selection, for the sweep. `value` is decimal text. */
export interface SweepTotal {
  group: "counts" | "value" | "numbers" | "letters";
  label: string;
  value: string;
}

/** One published Code 19 result and what the engine computes for it. */
export interface Finding {
  id: string;
  claim: string;
  expected: number;
  computed: number;
  /** The computed number is the published one. */
  holds: boolean;
  multipleOf19: boolean;
  /** computed / 19 when it divides, else null. */
  multiple: number | null;
  measure: string;
  scope: string;
  textMode: string;
  /** "stated" when the source gives the counting rule, "inferred" when it was derived. */
  basis: "stated" | "inferred";
  rule: string;
  /** The counting convention this finding holds under, in words. */
  convention: string;
  source: string;
  /** "gate" must reproduce; "open" is a known discrepancy whose cause is not settled. */
  check: "gate" | "open";
}

/** One of the 29 chapters that open with Quranic Initials. */
export interface InitialedChapter {
  chapter: number;
  name: string;
  /** Its initials in order, one character each. */
  letters: string;
  /** Opening verses carrying them: 1 everywhere except chapter 42. */
  verses: number;
  /** `published` is Khalifa's figure, or null when none is recorded. */
  counts: { letter: string; count: number; multipleOf19: boolean; published: number | null }[];
}

export interface TranslationText {
  key: string;
  verses: { verse: number; text: string }[];
}

export interface WordFrequencies {
  total: number;
  unique: number;
  words: { word: string; count: number }[];
}

export interface LetterStatistic {
  letter: string;
  order: number;
  count: number;
  positionSum: number;
  distanceSum: number;
}

export type LetterScope = "book" | "chapter" | "verse" | "word";

export interface QuantitySums {
  sum: number;
  odd: number;
  even: number;
  prime: number;
  composite: number;
  ratio: number | null;
}

export interface CvSums {
  count: number;
  c: QuantitySums;
  v: QuantitySums;
  plus: QuantitySums;
  minus: QuantitySums;
  times: QuantitySums;
  divided: QuantitySums;
}

export interface Maths {
  chapters: CvSums;
  verses: CvSums;
}

export type SymmetryKind = "wordLetters" | "verseWords" | "verseLetters";

export interface Symmetry {
  units: number;
  points: { position: number; total: number; positionSum: number; totalSum: number }[];
  percent: number;
}

export interface AllahSummary {
  allah: number;
  withAllah: number;
  withLillah: number;
  total: number;
}

export type ResearchMethod = "allah" | "nonAllah" | "all" | "double" | "repeated";

export interface ResearchTable {
  columns: string[];
  rowCount: number;
  offset: number;
  rows: string[][];
  tsv: string | null;
}

export type RatioScope = "verse" | "chapter" | "page" | "station" | "part" | "group" | "half" | "quarter" | "bowing" | "book";
export type RatioBoundary = "letter" | "word" | "sentence" | "verse" | "chapter";

export interface RatioOptions {
  scope: RatioScope;
  ratio: number;
  measure: "letters" | "value";
  length: "short" | "long";
  boundary: RatioBoundary;
}

export interface RatioUnit {
  firstVerse: number;
  lastVerse: number;
  colored: boolean;
  splitVerse: number;
  splitWord: number;
  splitLetters: number;
  firstLetters: number;
  firstValue: string;
  secondLetters: number;
  secondValue: string;
}

export interface Pair {
  a: number;
  b: number;
}

export interface NumberDetails {
  number: NumberInfo;
  divisorCount: number | null;
  divisors: string[] | null;
  divisorSum: string | null;
  power: number | null;
  carmichael: boolean;
  fourN: { form: "4n+1" | "4n-1"; n: number; ordinal: number | null } | null;
  splits: { squareSums: Pair[]; squareDifferences: Pair[]; cubeSums: Pair[]; cubeDifferences: Pair[] } | null;
  chain: {
    text: string;
    length: number;
    sum: number;
    primesAsZero: number;
    primesAsZeroReversed: number;
    primesAsOne: number;
    primesAsOneReversed: number;
  } | null;
}

export type Grouping = "any" | "all" | "phrase";

export type UnitKind =
  | "words"
  | "verses"
  | "chapters"
  | "sentences"
  | "pages"
  | "stations"
  | "parts"
  | "groups"
  | "halves"
  | "quarters"
  | "bowings";
export type UnitShape = "single" | "range" | "set";
export type ComparisonOp = "eq" | "ne" | "lt" | "le" | "gt" | "ge" | "div" | "ndiv" | "sum";
export type NumberKind =
  | "none"
  | "natural"
  | "prime"
  | "additivePrime"
  | "nonAdditivePrime"
  | "composite"
  | "additiveComposite"
  | "nonAdditiveComposite"
  | "odd"
  | "even"
  | "fibonacci"
  | "square"
  | "cubic"
  | "quartic"
  | "quintic"
  | "sextic"
  | "septic"
  | "octic"
  | "nonic"
  | "decic";

/** One constraint of a number search; `value` is a decimal string and may be negative (from the end). */
export interface CriterionInput {
  value?: string | undefined;
  comparison?: ComparisonOp | undefined;
  type?: NumberKind | undefined;
  remainder?: number | undefined;
}

export type NumberField = "number" | "verses" | "words" | "letters" | "uniqueLetters" | "value" | "frequency" | "occurrence";

export interface NumberQueryInput {
  unit: UnitKind;
  shape: UnitShape;
  size?: number;
  numberScope?: "book" | "chapter" | "verse";
  criteria: Partial<Record<NumberField, CriterionInput>>;
}

export type LetterMatchKind = "all" | "any" | "only" | "none";

export interface FrequencyQueryInput {
  unit: "words" | "verses" | "chapters" | "sentences";
  phrase: string;
  shape: "single" | "range";
  size?: number;
  uniqueLetters: boolean;
  sum?: CriterionInput;
  match?: LetterMatchKind;
}

export interface FoundUnit {
  reference: string;
  firstVerse: number;
  lastVerse: number;
  verseNumbers: number[] | null;
  verses: number;
  words: number;
  letters: number;
  uniqueLetters: number;
  value: NumberInfo;
  letterFrequencySum: string | null;
  preview: SearchVerse[];
  morePreview: boolean;
}

export interface UnitSearchResult {
  unitCount: number;
  truncated: boolean;
  offset: number;
  units: FoundUnit[];
  chapterCounts: number[];
  verseNumbers: number[];
}
export type SearchMethod =
  | "search.text"
  | "search.roots"
  | "search.harakat"
  | "search.related"
  | "search.relatedVerses"
  | "search.similar";
export type UnitSearchMethod = "search.numbers" | "search.frequency";
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
  /** Where the verses were found when not in the Arabic text: in translations, or in the standard spelling. */
  foundIn: "translations" | "emlaaei" | null;
}
