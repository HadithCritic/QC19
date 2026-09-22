<script lang="ts">
  import ChapterIndex from "./ChapterIndex.svelte";
  import Inspector from "./Inspector.svelte";
  import Reader from "./Reader.svelte";
</script>

<div class="read">
  <ChapterIndex />
  <Reader />
  <Inspector />
</div>

<style>
  .read {
    display: grid;
    grid-template-columns: var(--index-width) minmax(0, 1fr) var(--inspector-width);
    min-height: 0;
    height: 100%;
  }

  /* Below 1200px the index drops its Latin names (see ChapterIndex) and the
     inspector narrows, so the text keeps a readable measure. */
  @media (max-width: 1200px) {
    .read {
      grid-template-columns: 6.5rem minmax(0, 1fr) 18rem;
    }
  }

  /* Narrower than the desktop window allows; only reachable in a browser. */
  @media (max-width: 900px) {
    .read {
      grid-template-columns: 6.5rem minmax(0, 1fr);
      grid-template-rows: minmax(0, 1fr) minmax(0, 45%);
    }

    .read > :global(:last-child) {
      grid-column: 1 / -1;
      border-inline-start: 0;
      border-top: 1px solid var(--rule);
    }
  }
</style>
