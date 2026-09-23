import type { RatioOptions, RatioUnit } from "./engine/types";

// Ratio coloring (Features.txt #24): which part of its unit each displayed
// word falls in, and where a word the split runs through is cut.

export const RATIO_PRESETS: { label: string; value: number }[] = [
  { label: "1/π", value: 1 / Math.PI },
  { label: "1/e", value: 1 / Math.E },
  { label: "1/φ", value: 2 / (1 + Math.sqrt(5)) },
  // The original's "heart" ratio, 1.339, which it marks as needing verification.
  { label: "1/♥", value: 1 / 1.339 },
];

export const DEFAULT_RATIO: RatioOptions = {
  scope: "verse",
  ratio: 2 / (1 + Math.sqrt(5)),
  measure: "letters",
  length: "short",
  boundary: "letter",
};

export type WordPart = { part: "first" } | { part: "second" } | { part: "split"; letters: number };

/** The part of its colored unit a displayed word is in, or null when its unit is not colored. */
export function partOf(units: RatioUnit[], verse: number, word: number): WordPart | null {
  const unit = units.find((u) => u.colored && verse >= u.firstVerse && verse <= u.lastVerse);
  if (!unit) return null;
  if (verse < unit.splitVerse || (verse === unit.splitVerse && word < unit.splitWord)) return { part: "first" };
  if (verse > unit.splitVerse || word > unit.splitWord) return { part: "second" };
  return { part: "split", letters: unit.splitLetters };
}

const LETTER = /\p{L}/u;

/**
 * Cuts a displayed word after its first `letters` letters. Marks that follow
 * the last of those letters stay with it; tatweel is not a letter.
 */
export function cutWord(word: string, letters: number): [string, string] {
  if (letters <= 0) return ["", word];
  let seen = 0;
  const chars = [...word];
  for (let i = 0; i < chars.length; i++) {
    const c = chars[i]!;
    if (c !== "ـ" && LETTER.test(c)) {
      seen++;
      if (seen > letters) return [chars.slice(0, i).join(""), chars.slice(i).join("")];
    }
  }
  return [word, ""];
}

/** Letters and value of the first and second parts of the colored units. */
export function ratioTotals(units: RatioUnit[]): { firstLetters: number; secondLetters: number; firstValue: bigint; secondValue: bigint } {
  const colored = units.filter((u) => u.colored);
  return {
    firstLetters: colored.reduce((s, u) => s + u.firstLetters, 0),
    secondLetters: colored.reduce((s, u) => s + u.secondLetters, 0),
    firstValue: colored.reduce((s, u) => s + BigInt(u.firstValue), 0n),
    secondValue: colored.reduce((s, u) => s + BigInt(u.secondValue), 0n),
  };
}
