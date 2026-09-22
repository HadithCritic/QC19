import type { Chapter, ChapterStats } from "./engine/types";

/** The original's chapter orderings (Features.txt #59). */
export type ChapterSort = "number" | "name" | "revelation" | "verses" | "words" | "letters" | "value";

export const CHAPTER_SORTS: { id: ChapterSort; label: string }[] = [
  { id: "number", label: "Number" },
  { id: "name", label: "Name" },
  { id: "revelation", label: "Revelation" },
  { id: "verses", label: "Verses" },
  { id: "words", label: "Words" },
  { id: "letters", label: "Letters" },
  { id: "value", label: "Value" },
];

/** Sorts chapters, ties broken by chapter number; stats may be missing while loading. */
export function sortChapters(
  chapters: readonly Chapter[],
  stats: ReadonlyMap<number, ChapterStats>,
  by: ChapterSort,
  descending: boolean,
): Chapter[] {
  const key = (c: Chapter): number | bigint | string => {
    const s = stats.get(c.number);
    switch (by) {
      case "name":
        return c.name;
      case "revelation":
        return c.revelationOrder;
      case "verses":
        return s?.verses ?? c.verseCount;
      case "words":
        return s?.words ?? 0;
      case "letters":
        return s?.letters ?? 0;
      case "value":
        // Values are decimal strings; compare exactly.
        return s ? BigInt(s.value) : 0n;
      default:
        return c.number;
    }
  };

  const compare = (a: Chapter, b: Chapter): number => {
    const ka = key(a);
    const kb = key(b);
    const order =
      typeof ka === "string" && typeof kb === "string"
        ? ka.localeCompare(kb, "ar")
        : ka < kb
          ? -1
          : ka > kb
            ? 1
            : 0;
    return (descending ? -order : order) || a.number - b.number;
  };

  return [...chapters].sort(compare);
}
