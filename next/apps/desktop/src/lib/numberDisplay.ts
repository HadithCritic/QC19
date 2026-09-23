// How numbers are shown: in a base from 2 to 36 (Features.txt #71) and
// marked when divisible by the reader's divisor (#16). Values arrive as
// decimal strings because totals can exceed what a JavaScript number holds.

export const DEFAULT_DIVISOR = 19;
export const MIN_DIVISOR = 2;
export const MAX_DIVISOR = 9999;
export const DEFAULT_RADIX = 10;
export const MIN_RADIX = 2;
export const MAX_RADIX = 36;

const DIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

function toBigInt(value: string): bigint | null {
  const text = value.trim();
  if (!/^-?\d+$/.test(text)) return null;
  return BigInt(text);
}

/** A decimal string written in another base, digits 0-9 then A-Z, as the original's Radix.Encode. */
export function toRadix(value: string, radix: number): string {
  const n = toBigInt(value);
  if (n === null || radix === 10) return value;
  if (!Number.isInteger(radix) || radix < MIN_RADIX || radix > MAX_RADIX) throw new RangeError(`A base is from ${MIN_RADIX} to ${MAX_RADIX}.`);
  const negative = n < 0n;
  let rest = negative ? -n : n;
  if (rest === 0n) return "0";
  const base = BigInt(radix);
  let digits = "";
  while (rest > 0n) {
    digits = DIGITS[Number(rest % base)] + digits;
    rest /= base;
  }
  return negative ? `-${digits}` : digits;
}

/** A number typed in a base back to decimal; null when it is not a number in that base. */
export function fromRadix(text: string, radix: number): string | null {
  const trimmed = text.trim().toUpperCase();
  const negative = trimmed.startsWith("-");
  const body = negative ? trimmed.slice(1) : trimmed;
  if (body === "") return null;
  let n = 0n;
  for (const c of body) {
    const digit = DIGITS.indexOf(c);
    if (digit < 0 || digit >= radix) return null;
    n = n * BigInt(radix) + BigInt(digit);
  }
  return (negative ? -n : n).toString();
}

/** Keeps a divisor inside the original's range, wrapping past either end as its arrows do. */
export function wrapDivisor(divisor: number): number {
  if (divisor > MAX_DIVISOR) return MIN_DIVISOR;
  if (divisor < MIN_DIVISOR) return MAX_DIVISOR;
  return Math.round(divisor);
}

/** Wraps a base the same way: past 36 back to 2. */
export function wrapRadix(radix: number): number {
  if (radix > MAX_RADIX) return MIN_RADIX;
  if (radix < MIN_RADIX) return MAX_RADIX;
  return Math.round(radix);
}

/** The original's test: a non-zero value with no remainder. */
export function isDivisible(value: string, divisor: number): boolean {
  const n = toBigInt(value);
  if (n === null || n === 0n || divisor === 0) return false;
  return n % BigInt(divisor) === 0n;
}

/**
 * The power the original colors a number for: squares from 49, cubes from
 * 125, 5th powers from 243, 7th powers from 128; the first that applies.
 */
export function powerMark(value: string): 2 | 3 | 5 | 7 | null {
  const n = toBigInt(value);
  if (n === null) return null;
  const m = n < 0n ? -n : n;
  const checks: [2 | 3 | 5 | 7, bigint][] = [
    [2, 49n],
    [3, 125n],
    [5, 243n],
    [7, 128n],
  ];
  for (const [power, least] of checks) {
    if (m >= least && isPower(m, power)) return power;
  }
  return null;
}

function isPower(n: bigint, power: number): boolean {
  const guess = BigInt(Math.round(Number(n) ** (1 / power)));
  for (let r = guess - 1n; r <= guess + 1n; r++) {
    if (r > 0n && r ** BigInt(power) === n) return true;
  }
  return false;
}

/** 1st, 2nd, 3rd, 4th, 11th, 12th, 13th, 21st ... */
export function ordinal(n: number): string {
  const tens = n % 100;
  if (tens >= 11 && tens <= 13) return `${n}th`;
  const suffix = ["th", "st", "nd", "rd"][n % 10] ?? "th";
  return `${n}${suffix}`;
}

/** The sum of a number's digits written in a base (the original computes digit sums in the chosen base). */
export function digitSumIn(value: string, radix: number): number {
  const digits = toRadix(value.replace(/^-/, ""), radix);
  let sum = 0;
  for (const c of digits) sum += DIGITS.indexOf(c);
  return sum;
}

/** Repeated digit sums in a base until one digit remains. */
export function digitalRootIn(value: string, radix: number): number {
  let n = digitSumIn(value, radix);
  while (n >= radix) n = digitSumIn(String(n), radix);
  return n;
}
