import { describe, expect, it } from "vitest";
import { back, canGoBack, canGoForward, current, EMPTY_NAVIGATION, forward, NAVIGATION_LIMIT, visit } from "./navigation";

const r = (first: number, last = first) => ({ first, last });

describe("navigation", () => {
  it("goes back and forward through visited ranges", () => {
    let nav = visit(visit(visit(EMPTY_NAVIGATION, r(1)), r(2)), r(3));
    nav = back(back(nav));
    expect(current(nav)).toEqual(r(1));
    expect(canGoBack(nav)).toBe(false);
    nav = forward(nav);
    expect(current(nav)).toEqual(r(2));
    expect(canGoForward(nav)).toBe(true);
  });

  it("drops the forward entries when a new range is visited", () => {
    let nav = back(visit(visit(EMPTY_NAVIGATION, r(1)), r(2)));
    nav = visit(nav, r(9));
    expect(nav.entries).toEqual([r(1), r(9)]);
    expect(canGoForward(nav)).toBe(false);
  });

  it("ignores a repeat of the current range", () => {
    const nav = visit(visit(EMPTY_NAVIGATION, r(1, 7)), r(1, 7));
    expect(nav.entries).toHaveLength(1);
  });

  it("keeps a bounded history", () => {
    let nav = EMPTY_NAVIGATION;
    for (let i = 1; i <= NAVIGATION_LIMIT + 10; i++) nav = visit(nav, r(i));
    expect(nav.entries).toHaveLength(NAVIGATION_LIMIT);
    expect(current(nav)).toEqual(r(NAVIGATION_LIMIT + 10));
  });
});
