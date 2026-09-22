import { describe, expect, it } from "vitest";
import type { NumberInfo } from "./engine/types";
import { classLabel, familyLabel, formatFactors, rangeReference, verseReference } from "./numbers";

const classic = [
  { number: 1, firstVerse: 1, verseCount: 7, hasVerseZero: false },
  { number: 2, firstVerse: 8, verseCount: 286, hasVerseZero: false },
];

// Submission edition: chapter 2 opens with its verse-0 Bismillah at row 8.
const submission = [
  { number: 1, firstVerse: 1, verseCount: 7, hasVerseZero: false },
  { number: 2, firstVerse: 8, verseCount: 286, hasVerseZero: true },
  { number: 3, firstVerse: 295, verseCount: 200, hasVerseZero: true },
];

function info(partial: Partial<NumberInfo>): NumberInfo {
  return {
    value: "0",
    class: "None",
    code: "",
    digitSum: 0,
    digitalRoot: 0,
    familyOrdinal: null,
    classOrdinal: null,
    factors: [],
    ...partial,
  };
}

describe("labels", () => {
  it("writes the family ordinal as the Readme does", () => {
    expect(familyLabel(info({ value: "619", code: "XP", familyOrdinal: 114 }))).toBe("P114");
    expect(familyLabel(info({ value: "621", code: "XC", familyOrdinal: 506 }))).toBe("C506");
    expect(familyLabel(info({ value: "1", code: "U" }))).toBeNull();
  });

  it("writes the class ordinal with its code", () => {
    expect(classLabel(info({ code: "AP", classOrdinal: 7 }))).toBe("AP7");
    expect(classLabel(info({ code: "", classOrdinal: null }))).toBeNull();
  });
});

describe("formatFactors", () => {
  it("folds repeats into powers", () => {
    expect(formatFactors([2, 3, 19])).toBe("2 × 3 × 19");
    expect(formatFactors([2, 2, 2, 3])).toBe("2³ × 3");
    expect(formatFactors([2, 2, 2, 2, 2, 2, 2, 2, 2, 2])).toBe("2¹⁰");
  });
});

describe("references", () => {
  it("formats single verses and ranges", () => {
    expect(verseReference(262, classic)).toBe("2:255");
    expect(rangeReference(262, 262, classic)).toBe("2:255");
    expect(rangeReference(262, 264, classic)).toBe("2:255–257");
    expect(rangeReference(7, 9, classic)).toBe("1:7 – 2:2");
  });

  it("numbers a verse-0 Bismillah as verse 0", () => {
    expect(verseReference(8, submission)).toBe("2:0");
    expect(verseReference(9, submission)).toBe("2:1");
    expect(verseReference(294, submission)).toBe("2:286");
    expect(verseReference(295, submission)).toBe("3:0");
    expect(rangeReference(8, 294, submission)).toBe("2:0–286");
  });
});
