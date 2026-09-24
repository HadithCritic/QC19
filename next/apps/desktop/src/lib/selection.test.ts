import { describe, expect, it } from "vitest";
import {
  at,
  coverage,
  endpointOf,
  envelope,
  expand,
  extend,
  formatSelection,
  isExact,
  letterClusters,
  ordered,
  pick,
  verseSelection,
} from "./selection";

const submission = [
  { number: 1, firstVerse: 1, verseCount: 7, hasVerseZero: false },
  { number: 2, firstVerse: 8, verseCount: 286, hasVerseZero: true },
];

describe("addresses", () => {
  it("formats each level and ranges", () => {
    expect(formatSelection({ start: at(2), end: at(2) })).toBe("2");
    expect(formatSelection({ start: at(2, 255, 4, 2), end: at(2, 257, 8) })).toBe("2:255:w4:l2-2:257:w8");
    expect(formatSelection({ start: at(2, 0), end: at(2, 0) })).toBe("2:0");
  });
});

describe("ordering", () => {
  it("puts endpoints in Quran order", () => {
    const s = ordered({ start: at(2, 257, 8), end: at(2, 255, 4, 2) });
    expect(s.start).toEqual(at(2, 255, 4, 2));
    expect(s.end).toEqual(at(2, 257, 8));
  });

  it("orders a whole verse before its words", () => {
    expect(ordered({ start: at(2, 255, 3), end: at(2, 255) }).start).toEqual(at(2, 255));
  });
});

describe("picking in the reader", () => {
  it("starts on the first click and finishes on the second", () => {
    const first = pick(null, null, at(1, 1, 3), false);
    expect(first.selection).toEqual({ start: at(1, 1, 3), end: at(1, 1, 3) });
    expect(first.pending).toEqual(at(1, 1, 3));

    const second = pick(first.selection, first.pending, at(1, 2, 2), false);
    expect(second.selection).toEqual({ start: at(1, 1, 3), end: at(1, 2, 2) });
    expect(second.pending).toBeNull();
  });

  it("finishes backwards in order", () => {
    const first = pick(null, null, at(1, 2, 2), false);
    const second = pick(first.selection, first.pending, at(1, 1, 3), false);
    expect(second.selection).toEqual({ start: at(1, 1, 3), end: at(1, 2, 2) });
  });

  it("starts again after a finished selection", () => {
    const done = { start: at(1, 1, 1), end: at(1, 1, 3) };
    expect(pick(done, null, at(1, 4, 1), false).pending).toEqual(at(1, 4, 1));
  });

  it("extends with shift from either end", () => {
    const done = { start: at(1, 2, 1), end: at(1, 2, 3) };
    expect(pick(done, null, at(1, 3, 1), true).selection).toEqual({ start: at(1, 2, 1), end: at(1, 3, 1) });
    expect(pick(done, null, at(1, 1, 2), true).selection).toEqual({ start: at(1, 1, 2), end: at(1, 2, 3) });
    expect(extend(done, at(1, 3, 1))).toEqual({ start: at(1, 2, 1), end: at(1, 3, 1) });
  });
});

describe("coverage of a word", () => {
  const selection = { start: at(1, 1, 2, 2), end: at(1, 2, 1) };

  it("is none outside, full inside", () => {
    expect(coverage(selection, 1, 1, 1, "بِسْمِ")).toBe("none");
    expect(coverage(selection, 1, 1, 3, "ٱلرَّحْمَٰنِ")).toBe("full");
    expect(coverage(selection, 1, 2, 1, "ٱلْحَمْدُ")).toBe("full");
    expect(coverage(selection, 1, 2, 2, "لِلَّهِ")).toBe("none");
  });

  it("names the letters of a partly selected word", () => {
    expect(coverage(selection, 1, 1, 2, "ٱللَّهِ")).toEqual({ from: 2, to: 4 });
  });

  it("covers every word of a whole chapter or verse", () => {
    expect(coverage({ start: at(2), end: at(2) }, 2, 0, 1, "بِسْمِ")).toBe("full");
    expect(coverage({ start: at(1, 3), end: at(1, 3) }, 1, 3, 2, "ٱلرَّحِيمِ")).toBe("full");
  });

  it("knows the endpoint words", () => {
    expect(endpointOf(selection, 1, 1, 2)).toBe("start");
    expect(endpointOf(selection, 1, 2, 1)).toBe("end");
    expect(endpointOf({ start: at(1, 1, 2), end: at(1, 1, 2) }, 1, 1, 2)).toBe("both");
    expect(endpointOf(selection, 1, 1, 3)).toBeNull();
  });
});

describe("letters", () => {
  it("splits a word into letters with their marks", () => {
    expect(letterClusters("ٱللَّهِ")).toEqual(["ٱ", "ل", "لَّ", "هِ"]);
  });

  it("keeps tatweel and the small alif with their letter", () => {
    expect(letterClusters("ٱلرَّحْمَـٰنِ")).toHaveLength(6);
  });

  it("gives pause marks to the last letter and a leading mark to the first", () => {
    expect(letterClusters("رَيْبَ ۛ")).toEqual(["رَ", "يْ", "بَ ۛ"]);
    expect(letterClusters("۞ فَلَآ")[0]).toBe("۞ فَ");
  });

  it("puts every character in some letter", () => {
    const word = "إِبْرَٰهِـۧمَ";
    expect(letterClusters(word).join("")).toBe(word);
  });
});

describe("structure", () => {
  it("finds the verses a selection names", () => {
    expect(envelope({ start: at(2, 0), end: at(2, 1, 1) }, submission)).toEqual({ first: 8, last: 9 });
    expect(envelope({ start: at(1, 1, 2), end: at(2) }, submission)).toEqual({ first: 1, last: 294 });
  });

  it("expands to a coarser level", () => {
    const s = { start: at(1, 1, 2, 2), end: at(1, 2, 1) };
    expect(expand(s, "word")).toEqual({ start: at(1, 1, 2), end: at(1, 2, 1) });
    expect(expand(s, "verse")).toEqual({ start: at(1, 1), end: at(1, 2) });
    expect(expand(s, "chapter")).toEqual({ start: at(1), end: at(1) });
  });

  it("turns a verse range into a selection", () => {
    expect(verseSelection({ first: 8, last: 9 }, submission)).toEqual({ start: at(2, 0), end: at(2, 1) });
  });

  it("knows a selection finer than verses", () => {
    expect(isExact({ start: at(1, 1), end: at(1, 2) })).toBe(false);
    expect(isExact({ start: at(1, 1), end: at(1, 2, 1) })).toBe(true);
  });
});
