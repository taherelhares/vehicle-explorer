# Vehicle Explorer — design

The app is a reference instrument for a public vehicle registry, so it is dressed like
one: a zinc ground, white plates, graphite ink, hairline rules, and a single signal amber
that only appears where the interface is pointing at something.

## Owners

`src/theme.ts` owns every colour, type and shape decision. Components consume tokens
(`text.secondary`, `divider`, `secondary.main`, `borderRadius: 1`); they do not restate
values. `sx={{ borderRadius: n }}` multiplies `shape.borderRadius`, so `1` is the system
radius and `2` is double it.

## Colour

Both schemes are declared under `colorSchemes` and switch on `prefers-color-scheme`. That
only works because `cssVariables: true` is set — with it off, a declared dark scheme never
reaches the screen.

| Role | Light | Dark |
| --- | --- | --- |
| Ground (`background.default`) | `#ECEEEB` | `#101315` |
| Plate (`background.paper`) | `#FFFFFF` | `#181C1F` |
| Ink (`text.primary`) | `#15181A` | `#E7EAE6` |
| Quiet ink (`text.secondary`) | `#5B6469` | `#98A1A6` |
| Hairline (`divider`) | `#D8DCD6` | `#2A3034` |
| Signal (`secondary.main`) | `#C67F09` | `#F0AE3F` |

The signal amber is never body text. It is the wordmark tick, the tick on the result
plate and on empty states, the focus lamp under a filter cell, the rail on a hovered row,
and the `:focus-visible` outline. If it appears anywhere else, it has stopped meaning
"look here".

## Type

- **Barlow** (`sansFamily`) — everything the reader reads. Drawn from highway signage.
- **Barlow Condensed** (`condensedFamily`) — the `overline` variant only: field labels,
  column names, and the query stamp. Uppercase, tracked, always secondary ink.
- **IBM Plex Mono** (`monoFamily`) — the `caption` variant, model years, and vPIC IDs, so
  identifiers align down the page.

Faces load from Google Fonts in `index.html`; each stack has a real fallback.

## Structure

- **Top bar** — sticky, hairline bottom, tick + wordmark, source named on the right.
- **Filter cluster** — one outlined plate holds all three controls. The controls drop
  their own borders (`plainControlSx`); the cell reports focus with the amber lamp and is
  the only focus indicator inside the cluster. Every cell carries one helper line, so the
  row keeps a single baseline: what the field holds, or why it cannot be used.
- **Result plate** — count on the left, the query that produced it stamped on the right.
  A list is never read without the query behind it.
- **Rows** — name left, vPIC ID right in mono, columns named once in the header strip.
  Hairlines are drawn above every row but the first, so the last row never doubles up
  against the plate's border.

## Copy

An empty list is an answer and is worded as one; a failed request is not, and the two
never share wording. Empty states say what to do next. Errors say what happened and offer
a retry only when retrying can work (`ErrorNotice`).

## Floor

Visible keyboard focus everywhere, `prefers-reduced-motion` honoured globally in the
`MuiCssBaseline` override, and every control reachable and named. Motion is limited to the
focus lamp and row hover.
