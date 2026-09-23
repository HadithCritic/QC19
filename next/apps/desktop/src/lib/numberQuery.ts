import type { ComparisonOp, NumberField, NumberKind, UnitKind, UnitShape } from "./engine/types";

export const UNITS: { value: UnitKind; label: string }[] = [
  { value: "words", label: "words" },
  { value: "verses", label: "verses" },
  { value: "sentences", label: "sentences" },
  { value: "chapters", label: "chapters" },
  { value: "pages", label: "pages" },
  { value: "stations", label: "stations" },
  { value: "parts", label: "parts" },
  { value: "groups", label: "groups" },
  { value: "halves", label: "halves" },
  { value: "quarters", label: "quarters" },
  { value: "bowings", label: "bowings" },
];

export const SHAPES: { value: UnitShape; label: string }[] = [
  { value: "single", label: "one at a time" },
  { value: "range", label: "in runs of neighbors" },
  { value: "set", label: "in any combination" },
];

export const COMPARISONS: { value: ComparisonOp; label: string; title: string }[] = [
  { value: "eq", label: "=", title: "equal to" },
  { value: "ne", label: "≠", title: "not equal to" },
  { value: "lt", label: "<", title: "less than" },
  { value: "le", label: "≤", title: "at most" },
  { value: "gt", label: ">", title: "more than" },
  { value: "ge", label: "≥", title: "at least" },
  { value: "div", label: "÷", title: "divided by it leaves the remainder" },
  { value: "ndiv", label: "∤", title: "not divisible by" },
  { value: "sum", label: "Σ", title: "sum of positions equal to" },
];

export const KINDS: { value: NumberKind; label: string }[] = [
  { value: "none", label: "any number" },
  { value: "natural", label: "# its own number" },
  { value: "prime", label: "prime" },
  { value: "additivePrime", label: "additive prime" },
  { value: "nonAdditivePrime", label: "non-additive prime" },
  { value: "composite", label: "composite" },
  { value: "additiveComposite", label: "additive composite" },
  { value: "nonAdditiveComposite", label: "non-additive composite" },
  { value: "odd", label: "odd" },
  { value: "even", label: "even" },
  { value: "fibonacci", label: "Fibonacci" },
  { value: "square", label: "square" },
  { value: "cubic", label: "cube" },
  { value: "quartic", label: "4th power" },
  { value: "quintic", label: "5th power" },
  { value: "sextic", label: "6th power" },
  { value: "septic", label: "7th power" },
  { value: "octic", label: "8th power" },
  { value: "nonic", label: "9th power" },
  { value: "decic", label: "10th power" },
];

export const FIELD_LABELS: Record<NumberField, string> = {
  number: "Number",
  verses: "Verses",
  words: "Words",
  letters: "Letters",
  uniqueLetters: "Distinct letters",
  value: "Value",
  frequency: "Times its text occurs",
  occurrence: "Which occurrence",
};

const BLOCKS: UnitKind[] = ["chapters", "pages", "stations", "parts", "groups", "halves", "quarters", "bowings"];

/** The constraints that mean something for a kind of unit and shape, in the order the form shows them. */
export function fieldsFor(unit: UnitKind, shape: UnitShape): NumberField[] {
  const single = shape === "single";
  const fields: NumberField[] = [];
  if (unit !== "sentences") fields.push("number");
  if (BLOCKS.includes(unit) || (unit === "verses" && !single)) fields.push("verses");
  if (!(unit === "words" && single)) fields.push("words");
  fields.push("letters", "uniqueLetters", "value");
  if (single && (unit === "words" || unit === "verses")) fields.push("frequency", "occurrence");
  return fields;
}

/** Which numbers the Number constraint can read for a unit; empty when it has only one. */
export function numberScopesFor(unit: UnitKind): { value: "book" | "chapter" | "verse"; label: string }[] {
  if (unit === "words") {
    return [
      { value: "verse", label: "in its verse" },
      { value: "chapter", label: "in its chapter" },
      { value: "book", label: "in the book" },
    ];
  }
  if (unit === "verses") {
    return [
      { value: "chapter", label: "in its chapter" },
      { value: "book", label: "in the book" },
    ];
  }
  return [];
}

/** Units a letter frequency search can find. */
export const FREQUENCY_UNITS: { value: "words" | "verses" | "chapters" | "sentences"; label: string }[] = [
  { value: "words", label: "words" },
  { value: "verses", label: "verses" },
  { value: "sentences", label: "sentences" },
  { value: "chapters", label: "chapters" },
];
