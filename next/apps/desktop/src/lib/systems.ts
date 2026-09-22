import type { ValueSystem } from "./engine/types";

/** A value system's three parts; its name is `textMode_letterOrder_letterValue`. */
export interface SystemParts {
  textMode: string;
  letterOrder: string;
  letterValue: string;
}

export type Part = keyof SystemParts;

export function visibleSystems(systems: readonly ValueSystem[], research: boolean): ValueSystem[] {
  return research ? [...systems] : systems.filter((s) => !s.researchOnly);
}

/**
 * The choices for each picker, given the current selection. Each list only
 * offers values that combine with the parts to its left, so every choice the
 * user can make names a real system.
 */
export function partOptions(systems: readonly ValueSystem[], current: SystemParts): Record<Part, string[]> {
  const unique = (values: string[]) => [...new Set(values)].sort((a, b) => a.localeCompare(b));
  const inMode = systems.filter((s) => s.textMode === current.textMode);
  const inOrder = inMode.filter((s) => s.letterOrder === current.letterOrder);
  return {
    textMode: unique(systems.map((s) => s.textMode)),
    letterOrder: unique(inMode.map((s) => s.letterOrder)),
    letterValue: unique(inOrder.map((s) => s.letterValue)),
  };
}

/**
 * The system to switch to when one part changes. Keeps as much of the
 * current choice as still exists: the changed part is fixed, then the parts
 * to its right are kept when possible, else the first available is taken.
 */
export function withPart(
  systems: readonly ValueSystem[],
  current: SystemParts,
  part: Part,
  value: string,
): ValueSystem | undefined {
  const wanted: SystemParts = { ...current, [part]: value };
  const candidates = systems.filter((s) => s[part] === value);
  const score = (s: ValueSystem) =>
    (s.textMode === wanted.textMode ? 4 : 0) + (s.letterOrder === wanted.letterOrder ? 2 : 0) + (s.letterValue === wanted.letterValue ? 1 : 0);

  return [...candidates].sort((a, b) => score(b) - score(a) || a.name.localeCompare(b.name))[0];
}

export function partsOf(system: ValueSystem): SystemParts {
  return { textMode: system.textMode, letterOrder: system.letterOrder, letterValue: system.letterValue };
}

/** "Simplified29" reads as "Simplified 29"; "AdditivePrimes1" as "Additive primes 1". */
export function humanize(part: string): string {
  const spaced = part
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/([A-Za-z])(\d)/g, "$1 $2")
    .toLowerCase();
  return spaced.charAt(0).toUpperCase() + spaced.slice(1);
}
