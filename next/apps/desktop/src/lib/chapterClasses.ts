import type { Chapter } from "./engine/types";

// The original's chapter selections (Features.txt #5, #12): by initials,
// place of revelation, weight, and the kind of the chapter's number (C) and
// verse count (V), as its ChapterSelection list offers them.

export type NumberTest =
  | "any"
  | "even"
  | "odd"
  | "prime"
  | "composite"
  | "additivePrime"
  | "nonAdditivePrime"
  | "additiveComposite"
  | "nonAdditiveComposite";

export type ChapterPreset =
  | "all"
  | "initialized"
  | "full"
  | "partial"
  | "double"
  | "none"
  | "makkah"
  | "madinah"
  | "heavy"
  | "light"
  | "mersenne";

export const PRESETS: { value: ChapterPreset; label: string }[] = [
  { value: "all", label: "All chapters" },
  { value: "initialized", label: "Opening with initials" },
  { value: "full", label: "Initials on their own" },
  { value: "partial", label: "Initials within a verse" },
  { value: "double", label: "Initials in two verses (42)" },
  { value: "none", label: "Without initials" },
  { value: "makkah", label: "Revealed in Makkah" },
  { value: "madinah", label: "Revealed in Madinah" },
  { value: "heavy", label: "Heavy: number at most its verses" },
  { value: "light", label: "Light: number above its verses" },
  { value: "mersenne", label: "Number a Mersenne prime exponent" },
];

export const TESTS: { value: NumberTest; label: string }[] = [
  { value: "any", label: "any" },
  { value: "even", label: "even" },
  { value: "odd", label: "odd" },
  { value: "prime", label: "prime" },
  { value: "composite", label: "composite" },
  { value: "additivePrime", label: "additive prime" },
  { value: "nonAdditivePrime", label: "non-additive prime" },
  { value: "additiveComposite", label: "additive composite" },
  { value: "nonAdditiveComposite", label: "non-additive composite" },
];

/** Exponents p for which 2^p − 1 is prime, up to the last chapter. */
const MERSENNE_EXPONENTS = new Set([2, 3, 5, 7, 13, 17, 19, 31, 61, 89, 107]);

export function isPrime(n: number): boolean {
  if (n < 2) return false;
  for (let d = 2; d * d <= n; d++) if (n % d === 0) return false;
  return true;
}

function digitSum(n: number): number {
  let sum = 0;
  for (let rest = Math.abs(n); rest > 0; rest = Math.floor(rest / 10)) sum += rest % 10;
  return sum;
}

/** The original's tests; 1 is neither prime nor composite. */
export function passes(n: number, test: NumberTest): boolean {
  const prime = isPrime(n);
  const composite = n > 3 && !prime;
  switch (test) {
    case "any":
      return true;
    case "even":
      return n % 2 === 0;
    case "odd":
      return n % 2 === 1;
    case "prime":
      return prime;
    case "composite":
      return composite;
    case "additivePrime":
      return prime && isPrime(digitSum(n));
    case "nonAdditivePrime":
      return prime && !isPrime(digitSum(n));
    case "additiveComposite":
      return composite && !isPrime(digitSum(n)) && digitSum(n) > 3;
    case "nonAdditiveComposite":
      return composite && (isPrime(digitSum(n)) || digitSum(n) <= 3);
  }
}

/** A chapter's verse count as counted: with its verse 0 unless that is left out. */
export function countedVerses(chapter: Chapter, verseZeroCounted: boolean): number {
  return chapter.verseCount + (chapter.hasVerseZero && verseZeroCounted ? 1 : 0);
}

export function matchesPreset(chapter: Chapter, preset: ChapterPreset, verses: number): boolean {
  switch (preset) {
    case "all":
      return true;
    case "initialized":
      return chapter.initialization === "full" || chapter.initialization === "partial" || chapter.initialization === "double";
    // The original's "fully initialized" selection includes the doubly initialized chapter 42.
    case "full":
      return chapter.initialization === "full" || chapter.initialization === "double";
    case "partial":
      return chapter.initialization === "partial";
    case "double":
      return chapter.initialization === "double";
    // And its "non-initialized" selection includes the key chapter, 1.
    case "none":
      return chapter.initialization === "none" || chapter.initialization === "key";
    case "makkah":
      return chapter.revelationPlace.toLowerCase().startsWith("mak");
    case "madinah":
      return chapter.revelationPlace.toLowerCase().startsWith("mad") || chapter.revelationPlace.toLowerCase().startsWith("med");
    case "heavy":
      return chapter.number <= verses;
    case "light":
      return chapter.number > verses;
    case "mersenne":
      return MERSENNE_EXPONENTS.has(chapter.number);
  }
}

/** The chapters a selection keeps, in their order. */
export function selectChapters(
  chapters: Chapter[],
  preset: ChapterPreset,
  numberTest: NumberTest,
  versesTest: NumberTest,
  verseZeroCounted: boolean,
): Chapter[] {
  return chapters.filter((c) => {
    const verses = countedVerses(c, verseZeroCounted);
    return matchesPreset(c, preset, verses) && passes(c.number, numberTest) && passes(verses, versesTest);
  });
}
