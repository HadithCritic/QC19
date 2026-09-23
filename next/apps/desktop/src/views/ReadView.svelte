<script lang="ts">
  import ChapterIndex from "./ChapterIndex.svelte";
  import Inspector from "./Inspector.svelte";
  import Reader from "./Reader.svelte";

  const DEFAULT_LEFT_WIDTH = 240;
  const DEFAULT_RIGHT_WIDTH = 336;
  const MIN_LEFT_WIDTH = 104;
  const MAX_LEFT_WIDTH = 460;
  const MIN_RIGHT_WIDTH = 200;
  const MAX_RIGHT_WIDTH = 560;

  function loadWidth(key: string, fallback: number): number {
    try {
      const val = localStorage.getItem(key);
      if (val) {
        const num = Number.parseInt(val, 10);
        if (!Number.isNaN(num) && num > 50 && num < 1200) return num;
      }
    } catch {}
    return fallback;
  }

  function saveWidth(key: string, width: number): void {
    try {
      localStorage.setItem(key, String(width));
    } catch {}
  }

  let leftWidth = $state(loadWidth("qc_left_sidebar_width", DEFAULT_LEFT_WIDTH));
  let rightWidth = $state(loadWidth("qc_right_sidebar_width", DEFAULT_RIGHT_WIDTH));

  let container: HTMLDivElement | undefined = $state();
  let dragging = $state<"left" | "right" | null>(null);

  function resetLeft(): void {
    leftWidth = DEFAULT_LEFT_WIDTH;
    saveWidth("qc_left_sidebar_width", leftWidth);
  }

  function resetRight(): void {
    rightWidth = DEFAULT_RIGHT_WIDTH;
    saveWidth("qc_right_sidebar_width", rightWidth);
  }

  function onPointerDown(side: "left" | "right", event: PointerEvent): void {
    if (event.button !== 0) return;
    event.preventDefault();
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    dragging = side;
  }

  function onPointerMove(event: PointerEvent): void {
    if (!dragging || !container) return;
    const rect = container.getBoundingClientRect();
    if (dragging === "left") {
      const w = Math.round(event.clientX - rect.left);
      leftWidth = Math.min(Math.max(w, MIN_LEFT_WIDTH), MAX_LEFT_WIDTH);
    } else if (dragging === "right") {
      const w = Math.round(rect.right - event.clientX);
      rightWidth = Math.min(Math.max(w, MIN_RIGHT_WIDTH), MAX_RIGHT_WIDTH);
    }
  }

  function onPointerUp(event: PointerEvent): void {
    if (!dragging) return;
    try {
      (event.currentTarget as HTMLElement).releasePointerCapture(event.pointerId);
    } catch {}
    if (dragging === "left") saveWidth("qc_left_sidebar_width", leftWidth);
    if (dragging === "right") saveWidth("qc_right_sidebar_width", rightWidth);
    dragging = null;
  }

  function onKeydown(side: "left" | "right", event: KeyboardEvent): void {
    const step = event.shiftKey ? 30 : 10;
    if (event.key === "Home" || event.key === "Enter") {
      event.preventDefault();
      if (side === "left") resetLeft();
      else resetRight();
    } else if (event.key === "ArrowLeft") {
      event.preventDefault();
      if (side === "left") {
        leftWidth = Math.max(leftWidth - step, MIN_LEFT_WIDTH);
        saveWidth("qc_left_sidebar_width", leftWidth);
      } else {
        rightWidth = Math.min(rightWidth + step, MAX_RIGHT_WIDTH);
        saveWidth("qc_right_sidebar_width", rightWidth);
      }
    } else if (event.key === "ArrowRight") {
      event.preventDefault();
      if (side === "left") {
        leftWidth = Math.min(leftWidth + step, MAX_LEFT_WIDTH);
        saveWidth("qc_left_sidebar_width", leftWidth);
      } else {
        rightWidth = Math.max(rightWidth - step, MIN_RIGHT_WIDTH);
        saveWidth("qc_right_sidebar_width", rightWidth);
      }
    }
  }
</script>

<div
  class="read"
  bind:this={container}
  class:is-dragging={dragging !== null}
  style:--index-width="{leftWidth}px"
  style:--inspector-width="{rightWidth}px"
>
  <ChapterIndex />

  <button
    type="button"
    class="splitter splitter-left"
    class:active={dragging === "left"}
    aria-label="Resize chapter index"
    title="Drag to resize, double-click to reset"
    onpointerdown={(e) => onPointerDown("left", e)}
    onpointermove={onPointerMove}
    onpointerup={onPointerUp}
    onpointercancel={onPointerUp}
    ondblclick={resetLeft}
    onkeydown={(e) => onKeydown("left", e)}
  >
    <div class="line" aria-hidden="true"></div>
  </button>

  <Reader />

  <button
    type="button"
    class="splitter splitter-right"
    class:active={dragging === "right"}
    aria-label="Resize inspector"
    title="Drag to resize, double-click to reset"
    onpointerdown={(e) => onPointerDown("right", e)}
    onpointermove={onPointerMove}
    onpointerup={onPointerUp}
    onpointercancel={onPointerUp}
    ondblclick={resetRight}
    onkeydown={(e) => onKeydown("right", e)}
  >
    <div class="line" aria-hidden="true"></div>
  </button>

  <Inspector />
</div>

<style>
  .read {
    display: grid;
    grid-template-columns: var(--index-width) 1px minmax(0, 1fr) 1px var(--inspector-width);
    min-height: 0;
    height: 100%;
    position: relative;
  }

  .read.is-dragging {
    user-select: none;
    cursor: col-resize;
  }

  .splitter {
    position: relative;
    width: 1px;
    height: 100%;
    padding: 0;
    margin: 0;
    border: 0;
    background: var(--rule);
    cursor: col-resize;
    z-index: 10;
    touch-action: none;
    outline: none;
  }

  /* Expanded interactive hit area so it's effortless to grab */
  .splitter::before {
    content: "";
    position: absolute;
    top: 0;
    bottom: 0;
    left: -4px;
    right: -4px;
    z-index: 1;
  }

  .splitter .line {
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    background: transparent;
    transition: background-color var(--duration) var(--ease);
  }

  .splitter:hover .line,
  .splitter.active .line,
  .splitter:focus-visible .line {
    background: var(--lapis);
    box-shadow: 0 0 6px var(--lapis);
  }

  /* Below 1200px the index narrows if user hasn't explicitly customized down */
  @media (max-width: 1200px) {
    .read {
      grid-template-columns: min(var(--index-width), 6.5rem) 1px minmax(0, 1fr) 1px min(var(--inspector-width), 18rem);
    }
  }

  /* Narrow screen collapse */
  @media (max-width: 900px) {
    .read {
      grid-template-columns: 6.5rem 1px minmax(0, 1fr);
      grid-template-rows: minmax(0, 1fr) minmax(0, 45%);
    }

    .splitter-right {
      display: none;
    }

    .read > :global(:last-child) {
      grid-column: 1 / -1;
      border-inline-start: 0;
      border-top: 1px solid var(--rule);
    }
  }
</style>
