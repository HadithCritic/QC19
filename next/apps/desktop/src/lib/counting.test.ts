import { describe, expect, it } from "vitest";
import { changedCount, DEFAULT_COUNTING, parseCounting, unavailableReason } from "./counting";

describe("unavailableReason", () => {
  it("follows the original's enabled check boxes", () => {
    expect(unavailableReason("wawAsWord", "Original", true)).not.toBeNull();
    expect(unavailableReason("wawAsWord", "Simplified29", true)).toBeNull();
    expect(unavailableReason("hamzaAboveLine", "Simplified28", true)).not.toBeNull();
    expect(unavailableReason("hamzaAboveLine", "Simplified30", true)).not.toBeNull();
    expect(unavailableReason("hamzaAboveLine", "Simplified31", true)).toBeNull();
  });

  it("lets an edition with verse 0 leave the Bismillah out in any mode", () => {
    expect(unavailableReason("includeBasmalas", "Original", true)).toBeNull();
    expect(unavailableReason("includeBasmalas", "Original", false)).not.toBeNull();
    expect(unavailableReason("includeBasmalas", "Simplified29", false)).toBeNull();
  });
});

describe("parseCounting", () => {
  it("keeps known booleans and ignores everything else", () => {
    expect(parseCounting({ wawAsWord: true, junk: 1, hamzaAboveLine: "yes" })).toEqual({ ...DEFAULT_COUNTING, wawAsWord: true });
    expect(parseCounting(null)).toEqual(DEFAULT_COUNTING);
  });

  it("counts changes from the defaults", () => {
    expect(changedCount(DEFAULT_COUNTING)).toBe(0);
    expect(changedCount({ ...DEFAULT_COUNTING, includeBasmalas: false, shaddaAsLetter: true })).toBe(2);
  });
});
