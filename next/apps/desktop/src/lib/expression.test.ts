import { describe, expect, it } from "vitest";
import { ExpressionError, evaluate, isExpression, type Value } from "./expression";

const exact = (v: Value) => {
  expect(v.kind).toBe("int");
  return v.value.toString();
};

describe("expression calculator", () => {
  it("keeps whole numbers exact", () => {
    expect(exact(evaluate("19^20"))).toBe("37589973457545958193355601");
    expect(exact(evaluate("30!"))).toBe("265252859812191058636308480000000");
    expect(exact(evaluate("19*142"))).toBe("2698");
  });

  it("follows the usual precedence, with ^ right associative", () => {
    expect(exact(evaluate("2+3*4"))).toBe("14");
    expect(exact(evaluate("(2+3)*4"))).toBe("20");
    expect(exact(evaluate("2^3^2"))).toBe("512");
    expect(exact(evaluate("-2^2"))).toBe("-4");
    expect(exact(evaluate("2^-0"))).toBe("1");
  });

  it("divides exactly when it can, and reports a fraction when not", () => {
    expect(exact(evaluate("2698/19"))).toBe("142");
    const third = evaluate("1/3");
    expect(third.kind).toBe("real");
    expect(third.value).toBeCloseTo(0.3333, 4);
  });

  it("has the original's whole-number division and remainder", () => {
    expect(exact(evaluate("118123\\19"))).toBe("6217");
    expect(exact(evaluate("6236%19"))).toBe("4");
  });

  it("has nPk and nCk in base 10", () => {
    expect(exact(evaluate("5P2"))).toBe("20");
    expect(exact(evaluate("114C2"))).toBe("6441");
  });

  it("has functions and constants, in lower case", () => {
    expect(exact(evaluate("sqrt(361)"))).toBe("19");
    expect(exact(evaluate("pow(19,2)"))).toBe("361");
    expect(evaluate("pi").value).toBeCloseTo(Math.PI);
    expect(evaluate("2*phi").value).toBeCloseTo(3.23607, 4);
    expect(exact(evaluate("abs(-19)"))).toBe("19");
  });

  it("reads numbers in the chosen base, capital letters being digits", () => {
    expect(exact(evaluate("13+1", 16))).toBe("20"); // 0x13 + 1
    expect(exact(evaluate("FF", 16))).toBe("255");
    expect(exact(evaluate("J*2", 20))).toBe("38"); // J is 19 in base 20
    expect(() => evaluate("12", 2)).toThrow(ExpressionError);
  });

  it("allows thousands separators and × ÷", () => {
    expect(exact(evaluate("118,123 ÷ 19"))).toBe("6217");
    expect(exact(evaluate("19 × 334"))).toBe("6346");
    expect(exact(evaluate("1,000,000 + 1"))).toBe("1000001");
    expect(exact(evaluate("max(19,2)"))).toBe("19"); // one digit after the comma: an argument
  });

  it("explains what is wrong rather than guessing", () => {
    expect(() => evaluate("")).toThrow(/nothing/);
    expect(() => evaluate("(1+2")).toThrow(/not closed/);
    expect(() => evaluate("1+2)")).toThrow(/closing bracket/);
    expect(() => evaluate("1/0")).toThrow(/zero/);
    expect(() => evaluate("foo(2)")).toThrow(/not a function/);
    expect(() => evaluate("3.5!")).toThrow(/whole number/);
    expect(() => evaluate("9999!")).toThrow(/Factorials go up to/);
    expect(() => evaluate("10^100000")).toThrow(/digits/);
  });

  it("tells a plain number from an expression", () => {
    expect(isExpression("8317")).toBe(false);
    expect(isExpression("8,317")).toBe(false);
    expect(isExpression("-19")).toBe(false);
    expect(isExpression("19*2")).toBe(true);
    expect(isExpression("ff", 16)).toBe(false);
    expect(isExpression("sqrt(4)")).toBe(true);
  });
});
