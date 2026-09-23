import { describe, expect, it } from "vitest";
import {
  digitalRootIn,
  digitSumIn,
  fromRadix,
  isDivisible,
  ordinal,
  powerMark,
  toRadix,
  wrapDivisor,
  wrapRadix,
} from "./numberDisplay";

describe("digit sums in a base", () => {
  it("add the digits as written in that base", () => {
    expect(digitSumIn("8317", 10)).toBe(19);
    expect(digitSumIn("19", 2)).toBe(3); // 10011
    expect(digitSumIn("8317", 16)).toBe(2 + 0 + 7 + 13); // 207D
    expect(digitalRootIn("8317", 10)).toBe(1);
    expect(digitalRootIn("8317", 16)).toBe(7); // 22 = 16 in base 16, then 7
  });
});

describe("ordinal", () => {
  it("uses English suffixes", () => {
    expect([1, 2, 3, 4, 11, 12, 13, 21, 102, 111].map(ordinal)).toEqual([
      "1st", "2nd", "3rd", "4th", "11th", "12th", "13th", "21st", "102nd", "111th",
    ]);
  });
});

describe("toRadix and fromRadix", () => {
  it("write and read numbers in bases 2 to 36", () => {
    expect(toRadix("8317", 16)).toBe("207D");
    expect(toRadix("19", 2)).toBe("10011");
    expect(toRadix("-35", 36)).toBe("-Z");
    expect(toRadix("0", 7)).toBe("0");
    expect(toRadix("8317", 10)).toBe("8317");
    expect(fromRadix("207d", 16)).toBe("8317");
    expect(fromRadix("102", 2)).toBeNull();
    expect(toRadix("12345678901234567890123", 36)).toBe("20DGOHX2W7BEK7F");
    expect(fromRadix("20DGOHX2W7BEK7F", 36)).toBe("12345678901234567890123");
  });
});

describe("the divisor", () => {
  it("marks non-zero multiples", () => {
    expect(isDivisible("114", 19)).toBe(true);
    expect(isDivisible("0", 19)).toBe(false);
    expect(isDivisible("-38", 19)).toBe(true);
    expect(isDivisible("115", 19)).toBe(false);
  });

  it("wraps past either end", () => {
    expect(wrapDivisor(10000)).toBe(2);
    expect(wrapDivisor(1)).toBe(9999);
    expect(wrapRadix(37)).toBe(2);
    expect(wrapRadix(1)).toBe(36);
  });
});

describe("powerMark", () => {
  it("follows the original's thresholds", () => {
    expect(powerMark("49")).toBe(2);
    expect(powerMark("36")).toBeNull();
    expect(powerMark("125")).toBe(3);
    expect(powerMark("64")).toBe(2);
    expect(powerMark("128")).toBe(7);
    expect(powerMark("243")).toBe(5);
    expect(powerMark("50")).toBeNull();
  });
});
