# Source of the Submission edition files

`ws_quran_chapters_rows.csv`, `ws_quran_index_rows.csv` and
`ws_quran_text_rows.csv` are exports of WikiSubmission's Quran tables
(`ws_quran_chapters`, `ws_quran_index`, `ws_quran_text`). The files are used
as they are; the importer reads only the `arabic` column of the text table.

| File | SHA-256 |
| --- | --- |
| ws_quran_chapters_rows.csv | 19bcd464cdace61b09e1662b256dc55a606b7082431f03e7a932a5dce79d165b |
| ws_quran_index_rows.csv | ae7493b44cd3476ac6627583e340c5763e199803c5b1d10293e22fc305e6c5a0 |
| ws_quran_text_rows.csv | 64ff35a7081fc9723c3e41e0310bf46d97aa221816c018a3b7ef0d0a053387ec |

## License findings (checked 2026-09-22)

- The same tables are published as SQL in WikiSubmission's public repository
  [`WikiSubmission/ws-backend`](https://github.com/WikiSubmission/ws-backend),
  under `assets/source_data/`. That repository is licensed **AGPL-3.0**, which
  is compatible with this project's GPL-3.0.
- [wikisubmission.org](https://wikisubmission.org) states: "Everything here is
  open to read, cite, and reuse", and that every dataset on the site is open
  source.
- The site's [Terms of Use](https://wikisubmission.org/legal/terms-of-use) are
  generic: they restrict downloading and derivative use of "the Service" unless
  WikiSubmission states otherwise. They say nothing specific about the Quran
  text or the translations.
- No statement covers the **translations** in the text table (Rashad Khalifa's
  English translation and the others). The app does not use them yet.

## Before a public release

Get written confirmation from WikiSubmission that the Arabic text and any
translation the app ships may be redistributed under GPL-3.0 or AGPL-3.0, and
keep attribution to WikiSubmission with a link to `ws-backend`.
