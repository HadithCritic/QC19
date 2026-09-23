import type { SearchRequest } from "./searchRequest";

/** A verse the keys act on, with the text a same-text search needs. */
export interface KeyVerse {
  number: number;
  label: string;
  text: string;
}

/** A word the keys act on: its verse, display index and text. */
export interface KeyWord {
  verse: number;
  word: number;
  text: string;
  label: string;
}

/**
 * The search a function key starts in the reader (Features.txt #39 to #43):
 * F4 related words, F5 related verses, F6 similar verses, F7 the same text,
 * F8 the same text with its marks. F4, F7 and F8 use the clicked word when
 * there is one, as the original uses the word at the caret; F7 and F8 fall
 * back to the whole verse, as the original does for a selection.
 */
export function keySearch(key: string, word: KeyWord | null, verse: KeyVerse | null): SearchRequest | null {
  switch (key) {
    case "F4":
      return word ? { kind: "related", verse: word.verse, word: word.word, label: `“${word.text}” (${word.label})` } : null;
    case "F5":
      return verse ? { kind: "relatedVerses", verse: verse.number, label: verse.label } : null;
    case "F6":
      return verse ? { kind: "similar", verse: verse.number, method: "text", threshold: 0.7, label: verse.label } : null;
    case "F7":
      if (word) return { kind: "text", term: word.text, wordness: "whole", grouping: "any" };
      return verse ? { kind: "text", term: verse.text, wordness: "any", grouping: "phrase" } : null;
    case "F8":
      if (word) return { kind: "harakat", term: word.text };
      return verse ? { kind: "harakat", term: verse.text } : null;
    default:
      return null;
  }
}

/**
 * The bookmark F3 (or Shift+F3) moves to from a verse: the first one after it,
 * or the last one before it, wrapping around. The original does this when no
 * search result is shown.
 */
export function nextBookmark<T extends { first: number | null }>(bookmarks: T[], from: number | null, backward: boolean): T | null {
  const usable = bookmarks.filter((b) => b.first !== null).sort((a, b) => a.first! - b.first!);
  if (usable.length === 0) return null;
  const at = from ?? 0;
  if (backward) return [...usable].reverse().find((b) => b.first! < at) ?? usable[usable.length - 1] ?? null;
  return usable.find((b) => b.first! > at) ?? usable[0] ?? null;
}
