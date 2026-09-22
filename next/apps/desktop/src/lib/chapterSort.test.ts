import { describe, expect, it } from "vitest";
import type { Chapter, ChapterStats } from "./engine/types";
import { sortChapters } from "./chapterSort";

const chapter = (number: number, name: string, revelationOrder: number, verseCount: number): Chapter => ({
  number, name, transliteratedName: "", englishName: "", revelationOrder, revelationPlace: "", verseCount,
  firstVerse: number, hasVerseZero: false,
});

const chapters = [chapter(1, "الفاتحة", 5, 7), chapter(2, "البقرة", 87, 286), chapter(96, "العلق", 1, 19)];
const stats = new Map<number, ChapterStats>([
  [1, { chapter: 1, verses: 7, words: 29, letters: 139, value: "8317", code: "AP" }],
  [2, { chapter: 2, verses: 287, words: 6120, letters: 25632, value: "9007199254740993", code: "XC" }],
  [96, { chapter: 96, verses: 20, words: 76, letters: 304, value: "16742", code: "AC" }],
]);

const order = (by: Parameters<typeof sortChapters>[2], descending = false) =>
  sortChapters(chapters, stats, by, descending).map((c) => c.number);

describe("sortChapters", () => {
  it("sorts by each ordering", () => {
    expect(order("number")).toEqual([1, 2, 96]);
    expect(order("revelation")).toEqual([96, 1, 2]);
    expect(order("words")).toEqual([1, 96, 2]);
    expect(order("verses", true)).toEqual([2, 96, 1]);
  });

  it("compares values exactly, beyond JavaScript number precision", () => {
    expect(order("value", true)).toEqual([2, 96, 1]);
  });

  it("does not modify its input", () => {
    const before = chapters.map((c) => c.number);
    sortChapters(chapters, stats, "revelation", false);
    expect(chapters.map((c) => c.number)).toEqual(before);
  });
});
