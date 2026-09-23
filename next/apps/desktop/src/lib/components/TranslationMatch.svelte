<script lang="ts">
  import { app } from "../state/app.svelte";

  // A translation line a search matched, with its matches marked.

  interface Props {
    line: { key: string; text: string; ranges: [number, number][] };
  }

  let { line }: Props = $props();

  const translation = $derived(app.allTranslations.find((t) => t.key === line.key));
  const pieces = $derived.by(() => {
    const out: { text: string; mark: boolean }[] = [];
    let at = 0;
    for (const [start, length] of [...line.ranges].sort((a, b) => a[0] - b[0])) {
      if (start < at) continue;
      if (start > at) out.push({ text: line.text.slice(at, start), mark: false });
      out.push({ text: line.text.slice(start, start + length), mark: true });
      at = start + length;
    }
    if (at < line.text.length) out.push({ text: line.text.slice(at), mark: false });
    // The export marks footnotes with ±; the footnotes themselves are not in it.
    return out.map((p) => ({ ...p, text: p.text.replaceAll("±", "*") }));
  });
</script>

<p class="line" lang={translation?.language} dir={translation?.rightToLeft ? "rtl" : "ltr"}>
  <span class="name" dir="ltr">{translation?.name ?? line.key}</span>
  {#each pieces as piece, i (i)}{#if piece.mark}<mark>{piece.text}</mark>{:else}{piece.text}{/if}{/each}
</p>

<style>
  .line {
    margin: var(--space-1) 0 0;
    font-size: var(--text-sm);
    line-height: 1.5;
    color: var(--ink-muted);
  }

  .line[dir="ltr"] {
    text-align: left;
  }

  .name {
    margin-inline-end: var(--space-2);
    font-size: var(--text-xs);
    color: var(--ink-faint);
  }

  mark {
    color: var(--ink);
    background: var(--gilt-soft);
    border-radius: 2px;
  }

  mark:global(.current) {
    background: var(--gilt);
    color: var(--surface);
  }
</style>
