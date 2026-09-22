import type { VerseRange } from "./engine/types";

/**
 * Browse history for Back and Forward (Features.txt #68): the ranges visited
 * this session and where the reader is among them.
 */
export interface Navigation {
  entries: VerseRange[];
  index: number;
}

export const EMPTY_NAVIGATION: Navigation = { entries: [], index: -1 };

/** How many steps Back can go. */
export const NAVIGATION_LIMIT = 200;

function same(a: VerseRange | undefined, b: VerseRange): boolean {
  return a !== undefined && a.first === b.first && a.last === b.last;
}

/** Visiting a range drops anything ahead of the current place, as a browser does. */
export function visit(nav: Navigation, range: VerseRange): Navigation {
  if (same(nav.entries[nav.index], range)) return nav;
  const entries = [...nav.entries.slice(0, nav.index + 1), range].slice(-NAVIGATION_LIMIT);
  return { entries, index: entries.length - 1 };
}

export function canGoBack(nav: Navigation): boolean {
  return nav.index > 0;
}

export function canGoForward(nav: Navigation): boolean {
  return nav.index < nav.entries.length - 1;
}

export function back(nav: Navigation): Navigation {
  return canGoBack(nav) ? { ...nav, index: nav.index - 1 } : nav;
}

export function forward(nav: Navigation): Navigation {
  return canGoForward(nav) ? { ...nav, index: nav.index + 1 } : nav;
}

export function current(nav: Navigation): VerseRange | null {
  return nav.entries[nav.index] ?? null;
}
