import { describe, expect, it } from "vitest";
import { BASE_LETTERS, EXTRA, backspaceAt, extrasFor, insertAt } from "./palette";

describe("palette", () => {
  it("has the 28 base letters, none repeated", () => {
    expect(BASE_LETTERS).toHaveLength(28);
    expect(new Set(BASE_LETTERS).size).toBe(28);
  });

  it("offers each text mode only the extras it keeps, as the original's UpdateKeyboard", () => {
    expect(extrasFor("Simplified28")).toEqual([]);
    expect(extrasFor("Simplified29")).toEqual(["ء"]);
    expect(extrasFor("Simplified30")).toEqual(["ة", "ى"]);
    expect(extrasFor("Simplified31")).toEqual(["ء", "ة", "ى"]);
    for (const mode of ["Simplified36", "SimplifiedDots", "SimplifiedMarks", "Original"]) {
      expect(extrasFor(mode)).toHaveLength(8);
    }
  });

  it("never offers ة where it has been folded into ه", () => {
    expect(extrasFor("Simplified29")).not.toContain(EXTRA.taaMarbuta);
  });

  it("offers every extra for a root search, whatever the mode", () => {
    expect(extrasFor("Simplified28", true)).toHaveLength(8);
  });

  it("offers only the base letters for a mode it does not know", () => {
    expect(extrasFor("MyOwnMode")).toEqual([]);
  });

  it("inserts at the caret and replaces a selection", () => {
    expect(insertAt("الله", 0, 0, "و")).toEqual({ text: "والله", caret: 1 });
    expect(insertAt("abcd", 1, 3, "x")).toEqual({ text: "axd", caret: 2 });
  });

  it("does not double a space", () => {
    expect(insertAt("ab ", 3, 3, " ")).toEqual({ text: "ab ", caret: 3 });
  });

  it("deletes the selection or the character before the caret", () => {
    expect(backspaceAt("abcd", 1, 3)).toEqual({ text: "ad", caret: 1 });
    expect(backspaceAt("abcd", 2, 2)).toEqual({ text: "acd", caret: 1 });
    expect(backspaceAt("abcd", 0, 0)).toEqual({ text: "abcd", caret: 0 });
  });
});
