import { describe, expect, it } from "vitest";
import type { ValueSystem } from "./engine/types";
import { humanize, partOptions, visibleSystems, withPart } from "./systems";

function system(name: string, researchOnly = false): ValueSystem {
  const [textMode = "", letterOrder = "", letterValue = ""] = name.split("_");
  return { name, textMode, letterOrder, letterValue, researchOnly };
}

const systems = [
  system("Original_Alphabet_Primes1"),
  system("Original_Abjad_Gematria"),
  system("Simplified29_Alphabet_Primes1"),
  system("Simplified29_Alphabet_Composites"),
  system("Simplified29_Abjad_Gematria"),
  system("Simplified29_Alphabet_RamanujanPrimes", true),
];

describe("visibleSystems", () => {
  it("hides research-only systems unless research mode is on", () => {
    expect(visibleSystems(systems, false)).toHaveLength(5);
    expect(visibleSystems(systems, true)).toHaveLength(6);
  });
});

describe("partOptions", () => {
  it("only offers combinations that exist", () => {
    const options = partOptions(systems, { textMode: "Original", letterOrder: "Alphabet", letterValue: "Primes1" });
    expect(options.textMode).toEqual(["Original", "Simplified29"]);
    expect(options.letterOrder).toEqual(["Abjad", "Alphabet"]);
    expect(options.letterValue).toEqual(["Primes1"]);
  });
});

describe("withPart", () => {
  it("keeps the other parts when the combination exists", () => {
    const current = { textMode: "Original", letterOrder: "Alphabet", letterValue: "Primes1" };
    expect(withPart(systems, current, "textMode", "Simplified29")?.name).toBe("Simplified29_Alphabet_Primes1");
  });

  it("falls back to the closest real system when it does not", () => {
    const current = { textMode: "Simplified29", letterOrder: "Alphabet", letterValue: "Composites" };
    // Original has no Alphabet_Composites; keep Alphabet, take its only value.
    expect(withPart(systems, current, "textMode", "Original")?.name).toBe("Original_Alphabet_Primes1");
  });

  it("returns undefined for a value no system has", () => {
    const current = { textMode: "Original", letterOrder: "Alphabet", letterValue: "Primes1" };
    expect(withPart(systems, current, "textMode", "Nope")).toBeUndefined();
  });
});

describe("humanize", () => {
  it("splits camel case and trailing digits", () => {
    expect(humanize("Simplified29")).toBe("Simplified 29");
    expect(humanize("AdditivePrimes1")).toBe("Additive primes 1");
    expect(humanize("Abjad")).toBe("Abjad");
  });
});
