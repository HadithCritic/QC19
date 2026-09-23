import { describe as group, expect, it } from "vitest";
import { describe, scopeFor, shade, step, toCall } from "./searchRequest";

group("toCall", () => {
  it("maps each kind to its method and parameters", () => {
    expect(toCall({ kind: "text", term: "الله", wordness: "whole", grouping: "all" })).toEqual({
      method: "search.text",
      params: { term: "الله", wordness: "whole", grouping: "all" },
    });
    expect(toCall({ kind: "related", verse: 1, word: 2, label: "1:1" })).toEqual({
      method: "search.related",
      params: { verse: 1, word: 2 },
    });
    expect(toCall({ kind: "similar", verse: 9, method: "roots", threshold: 0.7, label: "2:1" }).params).toEqual({
      verse: 9,
      method: "roots",
      threshold: 0.7,
    });
  });

  it("describes a request for the heading", () => {
    expect(describe({ kind: "similar", verse: 9, method: "text", threshold: 0.7, label: "2:1" })).toBe(
      "verses like 2:1, 70% by text",
    );
  });
});

group("number queries", () => {
  it("sends only the constraints that are set", () => {
    const call = toCall({
      kind: "numbers",
      label: "",
      query: {
        unit: "verses",
        shape: "single",
        size: 4,
        criteria: {
          words: { value: " 7 ", comparison: "eq", type: "none" },
          letters: { value: "", type: "prime" },
          value: { value: "", type: "none" },
          verses: { value: "2", comparison: "div", remainder: -1 },
        },
      },
    });
    expect(call).toEqual({
      method: "search.numbers",
      params: {
        unit: "verses",
        shape: "single",
        words: { value: "7" },
        letters: { type: "prime" },
        verses: { value: "2", comparison: "div", remainder: -1 },
      },
    });
  });

  it("sends a letter match instead of a sum", () => {
    const call = toCall({
      kind: "frequency",
      label: "",
      query: { unit: "words", phrase: "ا", shape: "range", size: 3, uniqueLetters: true, sum: { value: "4" }, match: "none" },
    });
    expect(call.params).toEqual({ unit: "words", phrase: "ا", shape: "range", uniqueLetters: true, size: 3, match: "none" });
  });
});

group("scopeFor", () => {
  it("uses the selection or the previous results, else the book", () => {
    expect(scopeFor("selection", { first: 8, last: 20 }, null)).toEqual({ first: 8, last: 20 });
    expect(scopeFor("results", null, [3, 5])).toEqual({ verses: [3, 5] });
    expect(scopeFor("results", null, [])).toBeUndefined();
    expect(scopeFor("selection", null, [3])).toBeUndefined();
    expect(scopeFor("book", { first: 1, last: 2 }, [3])).toBeUndefined();
  });
});

group("step", () => {
  it("wraps forward and backward", () => {
    expect(step(-1, 3, false)).toBe(0);
    expect(step(-1, 3, true)).toBe(2);
    expect(step(2, 3, false)).toBe(0);
    expect(step(0, 3, true)).toBe(2);
    expect(step(1, 3, false)).toBe(2);
    expect(step(0, 0, false)).toBe(-1);
  });
});

group("shade", () => {
  it("grows with matches and saturates at 44", () => {
    expect(shade(0)).toBe(0);
    expect(shade(11)).toBeCloseTo(0.25);
    expect(shade(44)).toBe(1);
    expect(shade(500)).toBe(1);
  });
});
