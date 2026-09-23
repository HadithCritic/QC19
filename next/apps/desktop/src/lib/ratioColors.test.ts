import { describe, expect, it } from "vitest";
import type { RatioUnit } from "./engine/types";
import { cutWord, partOf, ratioTotals } from "./ratioColors";

function unit(firstVerse: number, lastVerse: number, splitVerse: number, splitWord: number, splitLetters: number, colored = true): RatioUnit {
  return {
    firstVerse, lastVerse, colored, splitVerse, splitWord, splitLetters,
    firstLetters: 12, firstValue: "100", secondLetters: 7, secondValue: "50",
  };
}

describe("partOf", () => {
  const units = [unit(1, 1, 1, 2, 5), unit(2, 4, 3, 0, 3), unit(5, 5, 5, 1, 2, false)];

  it("places words before, at and after the split", () => {
    expect(partOf(units, 1, 0)).toEqual({ part: "first" });
    expect(partOf(units, 1, 2)).toEqual({ part: "split", letters: 5 });
    expect(partOf(units, 1, 3)).toEqual({ part: "second" });
    expect(partOf(units, 2, 9)).toEqual({ part: "first" });
    expect(partOf(units, 4, 0)).toEqual({ part: "second" });
  });

  it("leaves uncolored units alone", () => {
    expect(partOf(units, 5, 0)).toBeNull();
    expect(partOf(units, 9, 0)).toBeNull();
  });
});

describe("cutWord", () => {
  it("cuts after letters, keeping their marks", () => {
    // ٱلرَّحْمَٰنِ: after 5 letters (ٱ ل ر ح م) the rest is ٰنِ
    const word = "ٱلرَّحْمَٰنِ";
    const [a, b] = cutWord(word, 5);
    expect(a + b).toBe(word);
    expect(b).toBe("نِ");
    expect(cutWord(word, 0)).toEqual(["", word]);
    expect(cutWord(word, 99)).toEqual([word, ""]);
  });
});

describe("ratioTotals", () => {
  it("adds the colored units only", () => {
    const totals = ratioTotals([unit(1, 1, 1, 2, 5), unit(2, 2, 2, 0, 1, false)]);
    expect(totals).toEqual({ firstLetters: 12, secondLetters: 7, firstValue: 100n, secondValue: 50n });
  });
});
