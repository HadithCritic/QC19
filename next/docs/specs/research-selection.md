# Research selection specification

One active selection, from a whole chapter down to a single letter, that the
reader highlights and every analysis view consumes. This records the design
and the rules a selection follows. The product brief it answers is
`QC19_FINE_GRAINED_RESEARCH_SELECTION_SPEC.md`.

## Two coordinate systems

A selection is written in **display coordinates**: what the reader sees.
Analysis runs over **counted coordinates**: the `Segmentation` of the text
under a text mode and counting options. The two differ, because rule stages
join words (بعدما, the 96:5 rule), waw-as-word splits them, and text modes and
counting options drop, fold or add letters.

| Part | Display coordinate | Notes |
| --- | --- | --- |
| Chapter | 1..114 | |
| Verse | number in chapter | 0 where the edition has a verse 0 |
| Word | 1-based in `VerseDisplay.Words` | the classic edition's Bismillah header is not a word |
| Letter | 1-based letter cluster in that word | a cluster is one Arabic letter (Unicode `Lo`, not tatweel) with the marks that follow it |

A selection never stores counted positions. They are derived each time, so
the same address stays attached to the same visible text when the value
system, text mode or counting options change.

## Address syntax

```text
2                   chapter
2:255               verse
2:255:w4            word
2:255:w4:l2         letter
2:255:w4-2:257:w8   range; each end is written in full
2-5                 chapter range
```

Arabic-Indic and Persian digits are accepted. An end that is a bare number is
a chapter only when the start is a chapter too: `2:255:w4-5` is rejected
rather than guessed at, since it could mean word 5 or chapter 5.

## Resolution

`SelectionResolver` turns a selection into an inclusive span of counted
letters, and from it the counted words and verses touched.

1. Endpoints are put in Quran order, so clicking backwards never gives an
   empty or negative range.
2. Each endpoint is validated against the display text. A chapter, verse,
   word or letter that does not exist is an error with a sentence the UI can
   show.
3. A start resolves to the first counted letter at or after its boundary, an
   end to the last counted letter at or before it. A verse the options leave
   out (a verse-0 Bismillah) is skipped inward; the address is not changed,
   and a note says the verse is not counted. A selection with nothing counted
   in it resolves to no span, with a note, not an error.
4. Words map through `DisplayWords.Align`. A display word selects every
   counted word derived from it (waw-as-word gives two). Display words joined
   into one counted word (96:5 ما لم) each select that counted word.
5. Letters map through `DisplayLetters`: the counted letters of cluster *k*
   are what normalizing the word up to cluster *k* adds to normalizing it up
   to cluster *k − 1*. The mapping is accepted only when every such prefix is
   a prefix of the counted letters, so a letter is never attributed by guess.
   A word that fails this is not addressable by letter under that text mode,
   and a letter endpoint in it is an error. A cluster may add two letters
   (shadda as a letter) or none (a letter the mode drops); a selection of only
   such a letter is empty and says so.
6. When `Align` cannot match a verse, word and letter endpoints in it are
   errors and verse endpoints still work.

In the classic edition, when Basmalahs are counted, the header's four counted
words sit before word 1 of verse 1. A word range that starts at word 1
excludes them; a range that crosses from the previous chapter includes them.

## Counting a span

- **Letters** and **value** cover exactly the selected letters.
- **Words** and **verses** are the counted units the span touches, so one
  letter inside a word is one word, and ما alone in 96:5 is one word.
- A span covering whole verses is marked verse-aligned and is analyzed by the
  existing verse-range code, so every legacy path and golden result still
  applies: one verse uses the single-verse path, one whole chapter the chapter
  path, anything else the aggregate path.
- A span with a partial word or verse is valued with the aggregate path
  clamped to the span. The legacy app could not select such a span, so there
  is no oracle; this is new behavior and is labeled as such.

## Protocol

- `selection.analyze` takes an exact selection and returns counts, value,
  classification, endpoints in display and counted coordinates, methodology
  and notes.
- `selection.breakdown` lists a selection by verse, word or letter, paged.
- The existing range methods accept an optional `selection` in place of
  `first` and `last`. Requests without it behave as before.

## Phases

| Phase | Scope |
| --- | --- |
| A | Model, address parser, `DisplayLetters`, resolver |
| B | Exact-selection statistics in Core and `selection.analyze` |
| C | Word selection in the reader, and the inspector |
| D | Shared selection across Read, Stats and Values |
| E | Letter selection |
| F | Breakdowns |
| G | Saved research selections, address copy and Go To |
| H | Multiple ranges, search within a selection, overlays |
