<script lang="ts">
  import type { Snippet } from "svelte";

  // One component for empty, loading and error states, so they read the same
  // everywhere: what happened, and what to do next.

  interface Props {
    tone?: "neutral" | "error" | "loading";
    title: string;
    detail?: string | undefined;
    action?: { label: string; run: () => void } | undefined;
    children?: Snippet;
  }

  let { tone = "neutral", title, detail, action, children }: Props = $props();
</script>

<div class="notice {tone}" role={tone === "error" ? "alert" : "status"} aria-live="polite">
  {#if tone === "loading"}<span class="spinner" aria-hidden="true"></span>{/if}
  <p class="title">{title}</p>
  {#if detail}<p class="detail">{detail}</p>{/if}
  {@render children?.()}
  {#if action}
    <button type="button" class="button" onclick={action.run}>{action.label}</button>
  {/if}
</div>

<style>
  .notice {
    display: grid;
    justify-items: center;
    gap: var(--space-2);
    padding: var(--space-6) var(--space-5);
    text-align: center;
    color: var(--ink-muted);
  }

  .title {
    margin: 0;
    font-weight: 600;
    color: var(--ink);
  }

  .detail {
    margin: 0;
    max-width: 38ch;
    font-size: var(--text-sm);
  }

  .error .title {
    color: var(--danger);
  }

  .spinner {
    width: 1.25rem;
    height: 1.25rem;
    border: 2px solid var(--rule);
    border-top-color: var(--lapis);
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
