<script lang="ts">
  import { describeError, engine, latest } from "../engine/client";
  import type { WordInfo } from "../engine/types";
  import { app } from "../state/app.svelte";

  // The clicked word's meaning, transliteration, roots and grammar
  // (Features.txt #62, #64). Grammar is the Quranic Arabic Corpus's analysis,
  // shown as the corpus gives it, with its tags named in English and Arabic.

  let info = $state<WordInfo | null>(null);
  let error = $state<string | null>(null);
  const load = latest(engine.wordInfo);

  $effect(() => {
    const word = app.currentWord;
    if (!word) {
      info = null;
      return;
    }
    load(word.verse, word.word)
      .then(({ current, value }) => {
        if (!current) return;
        info = value;
        error = null;
      })
      .catch((e: unknown) => (error = describeError(e)));
  });
</script>

{#if app.currentWord}
  <section class="word" aria-label="Word" aria-live="polite">
    <h2 class="eyebrow">Word</h2>
    {#if error}
      <p class="error">{error}</p>
    {:else if info}
      <p class="text arabic" lang="ar" dir="rtl">{info.text}</p>
      {#if info.meaning}<p class="meaning">{info.meaning}</p>{/if}
      {#if info.transliteration}<p class="translit" lang="ar-Latn">{info.transliteration}</p>{/if}
      {#if info.roots.length}
        <p class="roots">Roots <span class="arabic" lang="ar" dir="rtl">{info.roots.join(" · ")}</span></p>
      {/if}
      {#if info.parts.length}
        <ol class="parts">
          {#each info.parts as part (part.part)}
            <li>
              <span class="arabic" lang="ar" dir="rtl">{part.arabic}</span>
              <span class="tag" title={part.tagArabic ?? undefined}>{part.tagEnglish ?? part.tag}</span>
              <span class="features">
                {#each part.features.filter((f) => !["STEM", "PREFIX", "SUFFIX"].includes(f.text) && !f.text.startsWith("POS:")) as f (f.text)}
                  <span class="feature" title={f.arabic ?? f.text}>
                    {f.english ?? f.text}{#if f.script}&nbsp;<span class="arabic" lang="ar">{f.script}</span>{/if}
                  </span>
                {/each}
              </span>
            </li>
          {/each}
        </ol>
        <p class="credit">Grammar: Quranic Arabic Corpus, corpus.quran.com</p>
      {/if}
    {/if}
  </section>
{/if}

<style>
  .word {
    display: grid;
    gap: var(--space-1);
  }

  h2 {
    margin: 0;
  }

  .text {
    margin: 0;
    font-size: 1.6rem;
    text-align: right;
  }

  .meaning {
    margin: 0;
    font-size: var(--text-sm);
  }

  .translit {
    margin: 0;
    font-size: var(--text-sm);
    font-style: italic;
    color: var(--ink-muted);
  }

  .roots {
    margin: 0;
    font-size: var(--text-xs);
    color: var(--ink-muted);
  }

  .roots .arabic {
    font-size: 1.05rem;
    color: var(--ink);
  }

  .parts {
    display: grid;
    gap: var(--space-1);
    margin: var(--space-1) 0 0;
    padding: 0;
    list-style: none;
    font-size: var(--text-xs);
  }

  .parts li {
    display: grid;
    grid-template-columns: 3.5rem 1fr;
    gap: 0 var(--space-2);
    align-items: baseline;
  }

  .parts .arabic {
    grid-row: span 2;
    font-size: 1.15rem;
    text-align: right;
  }

  .tag {
    font-weight: 600;
  }

  .features {
    color: var(--ink-muted);
  }

  .feature + .feature::before {
    content: " · ";
  }

  .credit {
    margin: var(--space-1) 0 0;
    font-size: 0.68rem;
    color: var(--ink-faint);
  }

  .error {
    margin: 0;
    font-size: var(--text-xs);
    color: var(--danger);
  }
</style>
