import { describe, expect, it } from "vitest";
import { fileName, nextRepeat, pauseAfter, preamble, previousIndex, selectionSilence, type PlayVerse } from "./plan";

function v(chapter: number, verse: number, extra: Partial<PlayVerse> = {}): PlayVerse {
  return { number: 0, chapter, verse, lastInChapter: false, prostration: null, ...extra };
}

describe("fileName", () => {
  it("names verses as everyayah does, verse 0 as the Bismillah", () => {
    expect(fileName(2, 255)).toBe("002255");
    expect(fileName(114, 6)).toBe("114006");
    expect(fileName(2, 0)).toBe("001001");
  });
});

describe("preamble", () => {
  it("opens the session with the Audhubillah and the Bismillah", () => {
    expect(preamble(v(2, 5), null, true)).toEqual([{ kind: "audhubillah" }, { kind: "verse", name: "001001" }]);
    expect(preamble(v(1, 1), null, true)).toEqual([{ kind: "audhubillah" }]);
    expect(preamble(v(1, 2), null, true)).toEqual([{ kind: "audhubillah" }, { kind: "verse", name: "001001" }]);
    expect(preamble(v(9, 3), null, true)).toEqual([{ kind: "audhubillah" }]);
    expect(preamble(v(2, 0), null, true)).toEqual([{ kind: "audhubillah" }]);
  });

  it("puts the Bismillah before verse 1, unless its verse 0 was just played", () => {
    expect(preamble(v(3, 1), v(2, 286), false)).toEqual([{ kind: "verse", name: "001001" }]);
    expect(preamble(v(3, 1), v(3, 0), false)).toEqual([]);
    expect(preamble(v(9, 1), v(8, 75), false)).toEqual([{ kind: "audhubillah" }]);
    expect(preamble(v(1, 1), null, false)).toEqual([]);
    expect(preamble(v(2, 7), v(2, 6), false)).toEqual([]);
  });
});

describe("pauseAfter", () => {
  it("adds silence, the chapter end and prostrations", () => {
    expect(pauseAfter(v(2, 5), 4000, 0.5, true)).toBe(2000);
    expect(pauseAfter(v(1, 7, { lastInChapter: true }), 4000, 0, true)).toBe(3000);
    expect(pauseAfter(v(1, 7, { lastInChapter: true }), 4000, 0, false)).toBe(0);
    expect(pauseAfter(v(1, 7, { lastInChapter: true }), 4000, 0.5, true)).toBe(2000);
    expect(pauseAfter(v(32, 15, { prostration: "obligatory" }), 4000, 0, true)).toBe(10_000);
    expect(pauseAfter(v(7, 206, { prostration: "recommended", lastInChapter: true }), 1000, 0, true)).toBe(8000);
  });
});

describe("repeats and silences", () => {
  it("cycle unlimited, 2, 3, 5, 7", () => {
    expect(nextRepeat(Infinity)).toBe(2);
    expect(nextRepeat(7)).toBe(Infinity);
    expect(nextRepeat(Infinity, true)).toBe(7);
    expect(nextRepeat(2, true)).toBe(Infinity);
  });

  it("map the selection silence steps", () => {
    expect(selectionSilence(0)).toBe(0);
    expect(selectionSilence(2)).toBe(60_000);
    expect(selectionSilence(10, () => 0)).toBe(10_000);
    expect(selectionSilence(10, () => 1)).toBe(86_400_000);
  });

  it("go back a verse only near its start", () => {
    expect(previousIndex(5, 1200)).toBe(4);
    expect(previousIndex(5, 5000)).toBe(5);
    expect(previousIndex(0, 100)).toBe(0);
  });
});
