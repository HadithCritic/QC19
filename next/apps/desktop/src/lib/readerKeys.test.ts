import { describe, expect, it } from "vitest";
import { keySearch, nextBookmark } from "./readerKeys";

const word = { verse: 1, word: 2, text: "ٱلرَّحْمَٰنِ", label: "1:1" };
const verse = { number: 8, label: "2:1", text: "الٓمٓ" };

describe("keySearch", () => {
  it("uses the clicked word for F4, F7 and F8", () => {
    expect(keySearch("F4", word, verse)).toMatchObject({ kind: "related", verse: 1, word: 2 });
    expect(keySearch("F7", word, verse)).toEqual({ kind: "text", term: word.text, wordness: "whole", grouping: "any" });
    expect(keySearch("F8", word, verse)).toEqual({ kind: "harakat", term: word.text });
  });

  it("falls back to the verse text for F7 and F8", () => {
    expect(keySearch("F7", null, verse)).toEqual({ kind: "text", term: "الٓمٓ", wordness: "any", grouping: "phrase" });
    expect(keySearch("F8", null, verse)).toEqual({ kind: "harakat", term: "الٓمٓ" });
  });

  it("uses the verse for F5 and F6, at the original's 70%", () => {
    expect(keySearch("F5", word, verse)).toEqual({ kind: "relatedVerses", verse: 8, label: "2:1" });
    expect(keySearch("F6", null, verse)).toEqual({ kind: "similar", verse: 8, method: "text", threshold: 0.7, label: "2:1" });
  });

  it("does nothing without a target or for other keys", () => {
    expect(keySearch("F4", null, verse)).toBeNull();
    expect(keySearch("F5", word, null)).toBeNull();
    expect(keySearch("F2", word, verse)).toBeNull();
  });
});

describe("nextBookmark", () => {
  const marks = [{ first: 30 }, { first: null }, { first: 10 }, { first: 20 }];

  it("moves forward and back, wrapping", () => {
    expect(nextBookmark(marks, 10, false)).toEqual({ first: 20 });
    expect(nextBookmark(marks, 30, false)).toEqual({ first: 10 });
    expect(nextBookmark(marks, 20, true)).toEqual({ first: 10 });
    expect(nextBookmark(marks, 10, true)).toEqual({ first: 30 });
    expect(nextBookmark(marks, null, false)).toEqual({ first: 10 });
  });

  it("finds nothing without bookmarks", () => {
    expect(nextBookmark([{ first: null }], 5, false)).toBeNull();
  });
});
