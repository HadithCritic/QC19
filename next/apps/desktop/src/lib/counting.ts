import type { CountingOptions } from "./engine/types";

export type CountingKey = keyof CountingOptions;

export const DEFAULT_COUNTING: CountingOptions = {
  includeBasmalas: true,
  wawAsWord: false,
  shaddaAsLetter: false,
  hamzaAboveLine: false,
  elfAboveLine: false,
  yaaAboveLine: false,
  noonAboveLine: false,
};

export interface CountingChoice {
  key: CountingKey;
  mark: string;
  label: string;
  detail: string;
}

/** The original's Statistics-panel options, in its order. */
export const COUNTING_CHOICES: CountingChoice[] = [
  { key: "includeBasmalas", mark: "ب", label: "Count the Bismillah", detail: "" },
  { key: "wawAsWord", mark: "و", label: "Waw as a word", detail: "A leading و is its own word, except where it belongs to the word." },
  { key: "shaddaAsLetter", mark: "ـّ", label: "Shadda as a letter", detail: "A shadda counts as a second copy of its letter." },
  { key: "hamzaAboveLine", mark: "ـٔ", label: "Hamza above a line as a letter", detail: "Counts the small hamza on a line as the letter ء." },
  { key: "elfAboveLine", mark: "ـٰ", label: "Alif above a line as a letter", detail: "Counts the small (dagger) alif as the letter ا." },
  { key: "yaaAboveLine", mark: "ـۧ", label: "Yaa above a line as a letter", detail: "Counts the small yaa on a line as the letter ي." },
  { key: "noonAboveLine", mark: "ـۨ", label: "Noon above a line as a letter", detail: "Counts the small noon on a line as the letter ن." },
];

/**
 * Why an option does nothing in a text mode, or null when it applies. Mirrors
 * CountingOptions.For in the engine, which is what actually decides; this only
 * explains it in the interface.
 */
export function unavailableReason(key: CountingKey, textMode: string, verseZero: boolean): string | null {
  if (key === "includeBasmalas") {
    return verseZero || textMode !== "Original" ? null : "The Original text mode always counts the Bismillah.";
  }
  if (textMode === "Original") return "The Original text mode counts the text as written.";
  if (key === "hamzaAboveLine" && (textMode === "Simplified28" || textMode === "Simplified30")) {
    return "This text mode has no hamza letter.";
  }
  return null;
}

/** Parses stored options, keeping only known boolean fields. */
export function parseCounting(value: unknown): CountingOptions {
  const stored = typeof value === "object" && value !== null ? (value as Record<string, unknown>) : {};
  const result = { ...DEFAULT_COUNTING };
  for (const key of Object.keys(DEFAULT_COUNTING) as CountingKey[]) {
    if (typeof stored[key] === "boolean") result[key] = stored[key] as boolean;
  }
  return result;
}

/** How many options differ from the defaults. */
export function changedCount(options: CountingOptions): number {
  return (Object.keys(DEFAULT_COUNTING) as CountingKey[]).filter((k) => options[k] !== DEFAULT_COUNTING[k]).length;
}
