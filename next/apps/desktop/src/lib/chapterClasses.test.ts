import { describe, expect, it } from "vitest";
import type { Chapter } from "./engine/types";
import { passes, selectChapters } from "./chapterClasses";

function chapter(number: number, verseCount: number, initialization: Chapter["initialization"], place = "Makkah"): Chapter {
  return {
    number,
    name: "",
    transliteratedName: "",
    englishName: "",
    revelationOrder: number,
    revelationPlace: place,
    verseCount,
    firstVerse: 1,
    hasVerseZero: number !== 1 && number !== 9,
    initialization,
  };
}

const chapters = [
  chapter(1, 7, "key"),
  chapter(2, 286, "full", "Madinah"),
  chapter(10, 109, "partial"),
  chapter(42, 53, "double"),
  chapter(103, 3, "none"),
  chapter(108, 3, "none"),
];

describe("passes", () => {
  it("classifies as the original does", () => {
    expect(passes(1, "prime")).toBe(false);
    expect(passes(1, "composite")).toBe(false);
    expect(passes(23, "additivePrime")).toBe(true);
    expect(passes(19, "nonAdditivePrime")).toBe(true);
    expect(passes(22, "additiveComposite")).toBe(true); // 2 + 2 = 4
    expect(passes(10, "nonAdditiveComposite")).toBe(true); // 1 + 0 = 1
    expect(passes(12, "nonAdditiveComposite")).toBe(true); // 3 is prime
  });
});

describe("selectChapters", () => {
  const numbers = (list: Chapter[]): number[] => list.map((c) => c.number);

  it("selects by initials, counting 42 as fully initialized and 1 as without initials", () => {
    expect(numbers(selectChapters(chapters, "initialized", "any", "any", true))).toEqual([2, 10, 42]);
    expect(numbers(selectChapters(chapters, "full", "any", "any", true))).toEqual([2, 42]);
    expect(numbers(selectChapters(chapters, "none", "any", "any", true))).toEqual([1, 103, 108]);
  });

  it("selects by place and weight", () => {
    expect(numbers(selectChapters(chapters, "madinah", "any", "any", true))).toEqual([2]);
    // 103 has 3 verses and a verse 0: 4 when counted, so it is light either way.
    expect(numbers(selectChapters(chapters, "heavy", "any", "any", true))).toEqual([1, 2, 10, 42]);
  });

  it("tests the number and the counted verses", () => {
    expect(numbers(selectChapters(chapters, "all", "odd", "odd", false))).toEqual([1, 103]);
    expect(numbers(selectChapters(chapters, "all", "odd", "even", true))).toEqual([103]);
  });
});
