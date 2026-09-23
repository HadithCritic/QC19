import type {
  CriterionInput,
  FrequencyQueryInput,
  Grouping,
  NumberQueryInput,
  SearchMethod,
  SearchScope,
  SimilarityMethod,
  UnitSearchMethod,
  Wordness,
} from "./engine/types";

/** What the reader asked to find. Verse and word numbers are absolute verse numbers and display word indexes. */
export type SearchRequest =
  | { kind: "text"; term: string; wordness: Wordness; grouping: Grouping; translations?: string[] }
  | { kind: "roots"; term: string; grouping: Grouping }
  | { kind: "harakat"; term: string }
  | { kind: "related"; verse: number; word: number; label: string }
  | { kind: "relatedVerses"; verse: number; label: string }
  | { kind: "similar"; verse: number; method: SimilarityMethod; threshold: number; label: string }
  | { kind: "numbers"; query: NumberQueryInput; label: string }
  | { kind: "frequency"; query: FrequencyQueryInput; label: string };

/** Whether a request finds units (words, verses, chapters, runs, sets) rather than verses with marked words. */
export function findsUnits(request: SearchRequest): boolean {
  return request.kind === "numbers" || request.kind === "frequency";
}

export type ScopeChoice = "book" | "selection" | "results";

const METHODS: Record<SearchRequest["kind"], SearchMethod | UnitSearchMethod> = {
  text: "search.text",
  roots: "search.roots",
  harakat: "search.harakat",
  related: "search.related",
  relatedVerses: "search.relatedVerses",
  similar: "search.similar",
  numbers: "search.numbers",
  frequency: "search.frequency",
};

/** A constraint counts when it has a value or a number kind. */
export function isSet(criterion: CriterionInput | undefined): criterion is CriterionInput {
  return criterion !== undefined && ((criterion.value ?? "").trim() !== "" || (criterion.type ?? "none") !== "none");
}

function criterionParam(criterion: CriterionInput): CriterionInput {
  const value = (criterion.value ?? "").trim();
  return {
    ...(value !== "" ? { value } : {}),
    ...(criterion.comparison && criterion.comparison !== "eq" ? { comparison: criterion.comparison } : {}),
    ...(criterion.type && criterion.type !== "none" ? { type: criterion.type } : {}),
    ...(criterion.comparison === "div" && criterion.remainder !== undefined ? { remainder: criterion.remainder } : {}),
  };
}

function numbersParams(query: NumberQueryInput): Record<string, unknown> {
  const criteria = Object.fromEntries(
    Object.entries(query.criteria)
      .filter(([, c]) => isSet(c))
      .map(([field, c]) => [field, criterionParam(c as CriterionInput)]),
  );
  return {
    unit: query.unit,
    shape: query.shape,
    ...(query.shape !== "single" && query.size ? { size: query.size } : {}),
    ...(query.numberScope ? { numberScope: query.numberScope } : {}),
    ...criteria,
  };
}

function frequencyParams(query: FrequencyQueryInput): Record<string, unknown> {
  return {
    unit: query.unit,
    phrase: query.phrase,
    shape: query.shape,
    uniqueLetters: query.uniqueLetters,
    ...(query.shape === "range" && query.size ? { size: query.size } : {}),
    ...(query.match ? { match: query.match } : query.sum && isSet(query.sum) ? { sum: criterionParam(query.sum) } : {}),
  };
}

/** The engine method and the request's own parameters (paging and context are added by the caller). */
export function toCall(request: SearchRequest): { method: SearchMethod | UnitSearchMethod; params: Record<string, unknown> } {
  const method = METHODS[request.kind];
  switch (request.kind) {
    case "text":
      return {
        method,
        params: {
          term: request.term,
          wordness: request.wordness,
          grouping: request.grouping,
          ...(request.translations?.length ? { translations: request.translations } : {}),
        },
      };
    case "roots":
      return { method, params: { term: request.term, grouping: request.grouping } };
    case "harakat":
      return { method, params: { term: request.term } };
    case "related":
      return { method, params: { verse: request.verse, word: request.word } };
    case "relatedVerses":
      return { method, params: { verse: request.verse } };
    case "similar":
      return { method, params: { verse: request.verse, method: request.method, threshold: request.threshold } };
    case "numbers":
      return { method, params: numbersParams(request.query) };
    case "frequency":
      return { method, params: frequencyParams(request.query) };
  }
}

/** How a request reads in a heading. */
export function describe(request: SearchRequest): string {
  switch (request.kind) {
    case "text":
      return request.term;
    case "roots":
      return `roots ${request.term}`;
    case "harakat":
      return `${request.term} with its marks`;
    case "related":
      return `words related to ${request.label}`;
    case "relatedVerses":
      return `verses related to ${request.label}`;
    case "similar":
      return `verses like ${request.label}, ${Math.round(request.threshold * 100)}% by ${request.method}`;
    case "numbers":
    case "frequency":
      return request.label;
  }
}

/**
 * The verses to search within. "selection" and "results" fall back to the
 * whole book when there is nothing to search within, and the caller says so.
 */
export function scopeFor(
  choice: ScopeChoice,
  selection: { first: number; last: number } | null,
  previous: number[] | null,
): SearchScope | undefined {
  if (choice === "selection" && selection) return { first: selection.first, last: selection.last };
  if (choice === "results" && previous && previous.length > 0) return { verses: previous };
  return undefined;
}

/** Next or previous mark for F3 and Shift+F3, wrapping at either end; -1 when there are none. */
export function step(current: number, count: number, backward: boolean): number {
  if (count <= 0) return -1;
  if (current < 0 || current >= count) return backward ? count - 1 : 0;
  return (current + (backward ? count - 1 : 1)) % count;
}

/**
 * Shading strength (0 to 1) for a chapter with n matches. The original fades
 * its color by 16 steps per match through green, red, then blue, reaching
 * black at 44 matches; this keeps that ramp as one strength.
 */
export function shade(matches: number): number {
  const DARKEST = 44;
  if (matches <= 0) return 0;
  return Math.min(matches, DARKEST) / DARKEST;
}
