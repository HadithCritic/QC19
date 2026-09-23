// The character palette beside the search box (Features.txt #46), after the
// original's MainForm.UpdateKeyboard. It offers only characters the active
// text mode keeps, so a search is never built from a letter the mode has
// already folded away: in Simplified29, ة has become ه, so offering it would
// guarantee no results.

/** The 28 letters every text mode keeps. */
export const BASE_LETTERS = [..."ابتثجحخدذرزسشصضطظعغفقكلمنهوي"];

/** The eight further characters, each kept only by some text modes. */
export const EXTRA = {
  hamza: "ء",
  taaMarbuta: "ة",
  alifMaqsura: "ى",
  alifWasl: "ٱ",
  hamzaAboveAlif: "أ",
  hamzaBelowAlif: "إ",
  hamzaAboveWaw: "ؤ",
  hamzaAboveYaa: "ئ",
} as const;

const ALL_EXTRAS = Object.values(EXTRA);

/** The extra characters a text mode keeps, in the original's order. */
export function extrasFor(textMode: string, roots = false): string[] {
  // Root search allows every letter, as the original does.
  if (roots) return ALL_EXTRAS;
  switch (textMode) {
    case "Simplified28":
      return [];
    case "Simplified29":
      return [EXTRA.hamza];
    case "Simplified30":
      return [EXTRA.taaMarbuta, EXTRA.alifMaqsura];
    case "Simplified31":
      return [EXTRA.hamza, EXTRA.taaMarbuta, EXTRA.alifMaqsura];
    case "Simplified36":
    case "SimplifiedDots":
    case "SimplifiedMarks":
    case "Original":
      return ALL_EXTRAS;
    default:
      // A text mode the original did not know offers only the base letters,
      // which every mode keeps.
      return [];
  }
}

/**
 * Inserts a character into a field's text at its selection, replacing any
 * selected text, and returns the new text and caret. A space is not doubled.
 */
export function insertAt(
  text: string,
  start: number,
  end: number,
  char: string,
): { text: string; caret: number } {
  if (char === " " && start === end && start > 0 && text[start - 1] === " ") return { text, caret: start };
  return { text: text.slice(0, start) + char + text.slice(end), caret: start + char.length };
}

/** Deletes the selection, or the character before the caret, as Backspace does. */
export function backspaceAt(text: string, start: number, end: number): { text: string; caret: number } {
  if (start !== end) return { text: text.slice(0, start) + text.slice(end), caret: start };
  if (start === 0) return { text, caret: 0 };
  // Step over a whole code point, so a surrogate pair is never split.
  const before = [...text.slice(0, start)];
  before.pop();
  const kept = before.join("");
  return { text: kept + text.slice(start), caret: kept.length };
}
