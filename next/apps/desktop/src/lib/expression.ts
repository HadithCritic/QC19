// The expression calculator (Features.txt #30), for the Numbers view.
//
// The original compiled what was typed as C# at run time
// (Utilities/RadixEvaluator.cs). This is a parser instead, and it keeps whole
// numbers exact: 19^20 is 37589973457545958193355601, not a rounded double.
// It reads numbers in the chosen base, as the original does, and follows the
// original's conventions where they are deliberate:
//
//   + - * /   arithmetic; / gives a fraction when it does not divide
//   \         whole-number division, the original's integer divide
//   %         remainder
//   ^         power, right associative
//   !         factorial
//   P C       permutations and combinations, nPk and nCk, in base 10 only
//   pi e phi  constants
//   sqrt(x) and the other functions below, case-insensitive
//
// Where the original was a defect it is not reproduced: its ^ and ! worked
// only when they were the whole expression, and it rounded every result to a
// whole number. Here any result that is not whole is reported as such.
//
// Capital letters are digits in bases above 10, so names are written in
// lower case, as the original reserved them. A comma followed by exactly
// three digits groups thousands (118,123); write max(19, 200) with a space
// when an argument itself has three digits.

const DIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

/** An exact whole number, or a real one once a step leaves the integers. */
export type Value = { kind: "int"; value: bigint } | { kind: "real"; value: number };

export class ExpressionError extends Error {}

const CONSTANTS: Record<string, number> = { pi: Math.PI, e: Math.E, phi: (1 + Math.sqrt(5)) / 2 };

const FUNCTIONS: Record<string, { arity: number; apply: (...x: number[]) => number }> = {
  sqrt: { arity: 1, apply: Math.sqrt },
  cbrt: { arity: 1, apply: Math.cbrt },
  abs: { arity: 1, apply: Math.abs },
  ln: { arity: 1, apply: Math.log },
  log: { arity: 1, apply: Math.log },
  log10: { arity: 1, apply: Math.log10 },
  log2: { arity: 1, apply: Math.log2 },
  exp: { arity: 1, apply: Math.exp },
  floor: { arity: 1, apply: Math.floor },
  ceil: { arity: 1, apply: Math.ceil },
  round: { arity: 1, apply: Math.round },
  sin: { arity: 1, apply: Math.sin },
  cos: { arity: 1, apply: Math.cos },
  tan: { arity: 1, apply: Math.tan },
  pow: { arity: 2, apply: Math.pow },
  min: { arity: 2, apply: Math.min },
  max: { arity: 2, apply: Math.max },
};

/** Factorials beyond this are refused rather than computed for minutes. */
const MAX_FACTORIAL = 5000n;
/** Powers whose result would have more digits than this are refused. */
const MAX_DIGITS = 20000;

const int = (value: bigint): Value => ({ kind: "int", value });
const real = (value: number): Value => ({ kind: "real", value });
const toReal = (v: Value): number => (v.kind === "int" ? Number(v.value) : v.value);

/** A real that is whole and exactly representable becomes exact again. */
function settle(value: number): Value {
  if (!Number.isFinite(value)) throw new ExpressionError("The result is not a finite number.");
  return Number.isInteger(value) && Number.isSafeInteger(value) ? int(BigInt(value)) : real(value);
}

function requireInt(v: Value, what: string): bigint {
  if (v.kind === "int") return v.value;
  if (Number.isSafeInteger(v.value)) return BigInt(v.value);
  throw new ExpressionError(`${what} needs a whole number.`);
}

function factorial(n: bigint): bigint {
  if (n < 0n) throw new ExpressionError("A factorial needs a number that is not negative.");
  if (n > MAX_FACTORIAL) throw new ExpressionError(`Factorials go up to ${MAX_FACTORIAL}!.`);
  let result = 1n;
  for (let i = 2n; i <= n; i++) result *= i;
  return result;
}

function permutations(n: bigint, k: bigint): bigint {
  if (n < 0n || k < 0n || k > n) throw new ExpressionError("nPk needs 0 ≤ k ≤ n.");
  let result = 1n;
  for (let i = n - k + 1n; i <= n; i++) result *= i;
  return result;
}

function combinations(n: bigint, k: bigint): bigint {
  if (n < 0n || k < 0n || k > n) throw new ExpressionError("nCk needs 0 ≤ k ≤ n.");
  const small = k < n - k ? k : n - k;
  let result = 1n;
  for (let i = 1n; i <= small; i++) result = (result * (n - small + i)) / i;
  return result;
}

function power(base: Value, exponent: Value): Value {
  if (base.kind === "int" && exponent.kind === "int" && exponent.value >= 0n) {
    const digits = base.value === 0n ? 1 : base.value.toString().replace("-", "").length;
    if (digits * Number(exponent.value) > MAX_DIGITS * 1.1 && base.value !== 1n && base.value !== -1n && base.value !== 0n)
      throw new ExpressionError(`The result would have more than ${MAX_DIGITS} digits.`);
    return int(base.value ** exponent.value);
  }
  return settle(Math.pow(toReal(base), toReal(exponent)));
}

function divide(a: Value, b: Value): Value {
  if (toReal(b) === 0) throw new ExpressionError("Division by zero.");
  if (a.kind === "int" && b.kind === "int" && a.value % b.value === 0n) return int(a.value / b.value);
  return settle(toReal(a) / toReal(b));
}

type Token =
  | { t: "num"; text: string }
  | { t: "name"; text: string }
  | { t: "op"; text: string }
  | { t: "(" }
  | { t: ")" }
  | { t: "," };

function tokenize(text: string, radix: number): Token[] {
  const tokens: Token[] = [];
  const digitsHere = DIGITS.slice(0, radix);
  let i = 0;
  while (i < text.length) {
    const c = text[i]!;
    if (/\s/.test(c)) {
      i++;
      continue;
    }
    // A comma that groups thousands is taken with its number below; any
    // other comma separates a function's arguments.
    if (c === ",") {
      tokens.push({ t: "," });
      i++;
      continue;
    }
    if (c === "(" || c === ")") {
      tokens.push({ t: c });
      i++;
      continue;
    }
    if ("+-*/\\%^!×÷−".includes(c)) {
      tokens.push({ t: "op", text: c === "×" ? "*" : c === "÷" ? "/" : c === "−" ? "-" : c });
      i++;
      continue;
    }
    // In base 10, P and C between two numbers are nPk and nCk.
    if (radix <= 10 && (c === "P" || c === "C")) {
      tokens.push({ t: "op", text: c });
      i++;
      continue;
    }
    if (/[a-z]/.test(c)) {
      let j = i;
      while (j < text.length && /[a-z0-9]/.test(text[j]!)) j++;
      tokens.push({ t: "name", text: text.slice(i, j) });
      i = j;
      continue;
    }
    if (digitsHere.includes(c) || (c === "." && radix === 10)) {
      let j = i;
      // A lone name char like "e" never starts here: lower case is handled above.
      while (j < text.length) {
        const d = text[j]!;
        if (digitsHere.includes(d) || (d === "." && radix === 10)) j++;
        else if (d === "," && radix === 10 && /^,\d{3}(?!\d)/.test(text.slice(j))) j++;
        else break;
      }
      tokens.push({ t: "num", text: text.slice(i, j).replace(/,/g, "") });
      i = j;
      continue;
    }
    throw new ExpressionError(
      /[A-Z]/.test(c) ? `"${c}" is not a digit in base ${radix}.` : `"${c}" is not part of an expression.`,
    );
  }
  return tokens;
}

function readNumber(text: string, radix: number): Value {
  if (radix === 10) {
    if (!/^(\d+\.?\d*|\.\d+)$/.test(text)) throw new ExpressionError(`"${text}" is not a number.`);
    return text.includes(".") ? settle(Number(text)) : int(BigInt(text));
  }
  let n = 0n;
  for (const c of text) n = n * BigInt(radix) + BigInt(DIGITS.indexOf(c));
  return int(n);
}

/** Evaluates an expression, reading its numbers in the given base. */
export function evaluate(text: string, radix = 10): Value {
  if (!Number.isInteger(radix) || radix < 2 || radix > 36) throw new ExpressionError("A base is from 2 to 36.");
  const tokens = tokenize(text, radix);
  if (tokens.length === 0) throw new ExpressionError("There is nothing to calculate.");
  let at = 0;

  const peek = () => tokens[at];
  const isOp = (...ops: string[]) => {
    const t = peek();
    return t?.t === "op" && ops.includes(t.text);
  };
  const next = () => tokens[at++];

  // expression := term (("+" | "-") term)*
  function expression(): Value {
    let left = term();
    while (isOp("+", "-")) {
      const op = (next() as { text: string }).text;
      const right = term();
      left =
        left.kind === "int" && right.kind === "int"
          ? int(op === "+" ? left.value + right.value : left.value - right.value)
          : settle(op === "+" ? toReal(left) + toReal(right) : toReal(left) - toReal(right));
    }
    return left;
  }

  // term := unary (("*" | "/" | "\" | "%" | "P" | "C") unary)*
  function term(): Value {
    let left = unary();
    while (isOp("*", "/", "\\", "%", "P", "C")) {
      const op = (next() as { text: string }).text;
      const right = unary();
      if (op === "*") {
        left = left.kind === "int" && right.kind === "int" ? int(left.value * right.value) : settle(toReal(left) * toReal(right));
      } else if (op === "/") {
        left = divide(left, right);
      } else if (op === "\\" || op === "%") {
        const a = requireInt(left, op === "\\" ? "Whole-number division" : "A remainder");
        const b = requireInt(right, op === "\\" ? "Whole-number division" : "A remainder");
        if (b === 0n) throw new ExpressionError("Division by zero.");
        left = int(op === "\\" ? a / b : a % b);
      } else {
        const n = requireInt(left, `n${op}k`);
        const k = requireInt(right, `n${op}k`);
        left = int(op === "P" ? permutations(n, k) : combinations(n, k));
      }
    }
    return left;
  }

  // unary := ("-" | "+") unary | power
  function unary(): Value {
    if (isOp("-")) {
      next();
      const v = unary();
      return v.kind === "int" ? int(-v.value) : real(-v.value);
    }
    if (isOp("+")) {
      next();
      return unary();
    }
    return powerOf();
  }

  // power := postfix ("^" unary)?   right associative, so 2^3^2 is 2^9
  function powerOf(): Value {
    const base = postfix();
    if (isOp("^")) {
      next();
      return power(base, unary());
    }
    return base;
  }

  // postfix := primary "!"*
  function postfix(): Value {
    let v = primary();
    while (isOp("!")) {
      next();
      v = int(factorial(requireInt(v, "A factorial")));
    }
    return v;
  }

  function primary(): Value {
    const t = next();
    if (!t) throw new ExpressionError("The expression ends too soon.");
    if (t.t === "num") return readNumber(t.text, radix);
    if (t.t === "(") {
      const v = expression();
      if (next()?.t !== ")") throw new ExpressionError("A bracket is not closed.");
      return v;
    }
    if (t.t === "name") {
      const name = t.text.toLowerCase();
      if (name in CONSTANTS && peek()?.t !== "(") return real(CONSTANTS[name]!);
      const fn = FUNCTIONS[name];
      if (!fn) throw new ExpressionError(`"${t.text}" is not a function or constant this calculator knows.`);
      if (next()?.t !== "(") throw new ExpressionError(`${name} needs its argument in brackets.`);
      const args: Value[] = [expression()];
      while (peek()?.t === ",") {
        next();
        args.push(expression());
      }
      if (next()?.t !== ")") throw new ExpressionError("A bracket is not closed.");
      if (args.length !== fn.arity) throw new ExpressionError(`${name} takes ${fn.arity} argument${fn.arity === 1 ? "" : "s"}.`);
      if (name === "abs" && args[0]!.kind === "int") return int(args[0]!.value < 0n ? -args[0]!.value : args[0]!.value);
      if (name === "pow") return power(args[0]!, args[1]!);
      return settle(fn.apply(...args.map(toReal)));
    }
    if (t.t === ")") throw new ExpressionError("A closing bracket has no opening one.");
    throw new ExpressionError("An operator is missing a number.");
  }

  const result = expression();
  if (at < tokens.length) {
    const t = tokens[at]!;
    throw new ExpressionError(t.t === ")" ? "A closing bracket has no opening one." : "There is something extra at the end.");
  }
  return result;
}

/** Whether text is more than a plain number, so it should be calculated first. */
export function isExpression(text: string, radix = 10): boolean {
  const plain = text.trim().replace(/,/g, "");
  const digits = DIGITS.slice(0, radix);
  const body = plain.startsWith("-") ? plain.slice(1) : plain;
  return body.length > 0 && ![...body.toUpperCase()].every((c) => digits.includes(c));
}
