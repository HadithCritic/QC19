<script lang="ts">
  import type { ClassCode } from "../engine/types";

  // The verse-end marker of a mushaf, carrying the verse's value class in its
  // ring. The number is written in Arabic-Indic digits, as a mushaf does.

  interface Props {
    number: number;
    code: ClassCode | null;
  }

  let { number, code }: Props = $props();

  const INDIC = "٠١٢٣٤٥٦٧٨٩";
  const digits = $derived([...String(number)].map((d) => INDIC[Number(d)]).join(""));
</script>

<span class="rosette" data-class={code ?? ""} class:pending={code === null} aria-hidden="true">
  <svg viewBox="0 0 40 40">
    <circle class="ring" cx="20" cy="20" r="16.5" />
    <circle class="petals" cx="20" cy="20" r="19" />
  </svg>
  <span class="digits">{digits}</span>
</span>

<style>
  .rosette {
    position: relative;
    display: inline-grid;
    place-items: center;
    width: 1.55em;
    height: 1.55em;
    margin-inline: 0.18em;
    vertical-align: -0.28em;
    font-size: 0.72em;
  }

  svg {
    position: absolute;
    inset: 0;
    overflow: visible;
  }

  .ring {
    fill: color-mix(in srgb, var(--class) 9%, var(--surface));
    stroke: var(--class);
    stroke-width: 2.6;
    transition: stroke var(--duration) var(--ease), fill var(--duration) var(--ease);
  }

  .petals {
    fill: none;
    stroke: var(--gilt);
    stroke-width: 1;
    stroke-dasharray: 2.2 2.8;
    opacity: 0.75;
  }

  .pending .ring {
    stroke: var(--rule-strong);
    fill: var(--surface);
  }

  .digits {
    position: relative;
    font-family: var(--font-arabic);
    font-size: 0.78em;
    line-height: 1;
    color: var(--ink);
    padding-top: 0.12em;
  }
</style>
