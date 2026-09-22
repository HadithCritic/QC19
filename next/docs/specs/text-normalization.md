# Text normalization specification

Derived from the legacy implementation and verified against all 6,236 verses.
This is the specification the brief asks for in §45(B) and §24.

## The finding

**The legacy engine normalizes in two unrelated places, and both affect the
result.** Neither references the other, and neither is complete on its own.

```
raw Uthmani text
      |
      v
  [ stage 1 ]  Rules/<word-count-method>/<mode>.txt
               ordered find/replace, applied by SimplificationSystem.Simplify
               during BuildSimplifiedBook
      |
      +---> Verse.Text  (what the book stores, what word counts use,
      |                  diacritics intact)
      v
  [ stage 2 ]  hardcoded Simplify28..36 in Utilities/Extensions.cs
               applied by Server.CalculateValue
      |
      v
  value text  (diacritics stripped, letter forms folded)
```

This was found by golden comparison, not by reading the source. Implementing
only stage 1 gives Al-Fatiha **8283** instead of 8317. Implementing only stage 2
leaves **273 of 6,236** verses valued wrongly by small amounts. Applying both,
in this order, gives **0 mismatches across all 6,236 verses**.

## Stage 1: rules

Ordered find/replace pairs, TSV, `#` for comments. 16 files: 8 text modes across
2 word count methods (77878 and 77880). 832 rules total.

Order is significant. Later rules act on the output of earlier ones.

Mostly word segmentation and orthographic normalization, for example joining
`بعد ما` into `بعدما` so it counts as one word.

**Encoding is not consistent.** `Simplified36.txt` ships as UTF-8; the other
seven are UTF-16LE. Detect from the BOM; do not assume.

## Stage 2: letter normalization

Hardcoded, and defined by composition:

| Mode | Definition |
| --- | --- |
| `Simplify36` | strip removable characters, collapse runs of spaces |
| `Simplify31` | `Simplify36` + fold `إ أ ٱ آ → ا`, `ؤ → و`, `ئ → ي` |
| `Simplify30` | `Simplify31` + remove `ء` |
| `Simplify29` | `Simplify31` + `ة → ه`, `ى → ي` |
| `Simplify28` | `Simplify29` + remove `ء` |

Mode name to function:

| Text mode | Uses |
| --- | --- |
| `Simplified28` … `Simplified36` | the matching function |
| `SimplifiedDots` | `Simplify36` |
| **`Original`** | **`Simplify29`** |
| `SimplifiedMarks` | `Simplify29` |

### `Original` is not "original"

For valuation, `Original` maps to `Simplify29`. This explains a result in the
golden data that otherwise looks like coincidence: `Original` and
`Simplified29` produce the identical whole-book value **19,628,315**.

`Original` is a display mode. It preserves diacritics in `Verse.Text`, but the
moment a value is computed the text goes through `Simplify29`.

### Removed characters

65 characters, taken verbatim from `Utilities/Constants.cs`:

| Class | Count | Codepoints |
| --- | ---: | --- |
| Arabic digits | 10 | `0`–`9` |
| Indian digits | 10 | U+0660–U+0669 |
| Diacritics | 25 | U+0652, U+064E, U+0650, U+064F, U+0651, U+064B, U+064D, U+064C, U+0670, U+0654, U+06E5, U+0653, U+06DF, U+06E6, U+06E7, U+06ED, U+06E2, U+06DC, U+06E3, U+06E0, U+06E8, U+06EA, U+06EB, U+06EC, U+0640 |
| Stopmarks | 7 | U+06D9, U+06D6, U+06DA, U+06DB, U+06D7, U+06DC, U+06D8 |
| Quran marks | 3 | U+06DE, U+06E9, U+2302 |
| Ornate parentheses | 2 | U+FD3F, U+FD3E |

U+06DC appears in both the diacritics and stopmarks lists in the legacy source.
Harmless, since membership is what matters, but preserved as-is.

## Preserved quirks

Two legacy oddities are reproduced rather than corrected, because correcting
them would silently change results:

1. `SimplifiedDots` carries the comment `// BUT final ي --> ى` on its branch,
   and the code does not implement it. The comment is aspirational; the
   behavior is plain `Simplify36`.
2. `Simplify36` contains a commented-out `آ → ءا` replacement. The raw
   `quran-uthmani.txt` contains no U+0622 at all, so it is dead either way.

Both are documented here so that a future change is a decision rather than an
accident.

## Implementation

- Stage 1: `QuranCode.Core.Text.TextMode`
- Stage 2: `QuranCode.Core.Text.ArabicNormalizer`
- Both, in order: `QuranCode.Core.Text.TextPipeline`

Verification: `GoldenTests.AlFatihaValuesTo8317`,
`WholeBookValueMatchesLegacy`, `EveryChapterValueMatchesLegacy`.
