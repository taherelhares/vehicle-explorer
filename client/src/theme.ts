import { createTheme } from '@mui/material/styles';

/**
 * Vehicle Explorer is a reference instrument for a public vehicle registry, so it is
 * dressed like one: a zinc ground, white plates, graphite ink, hairline rules, and a
 * single signal amber that only ever appears where the interface is pointing at
 * something — the wordmark tick, the result plate, and the lamp under whichever control
 * currently has the keyboard.
 *
 * Barlow is drawn from highway signage and IBM Plex Mono carries every identifier and
 * numeral, so catalogue IDs and model years line up in columns the way they would on a
 * spec plate.
 */

export const sansFamily = "'Barlow', 'Segoe UI', system-ui, -apple-system, sans-serif";
export const condensedFamily = "'Barlow Condensed', 'Barlow', 'Segoe UI', sans-serif";
export const monoFamily = "'IBM Plex Mono', ui-monospace, 'Cascadia Mono', Consolas, monospace";

export const theme = createTheme({
  // The dark scheme below is only declared without this: `cssVariables` is what makes MUI
  // emit both schemes and switch between them on `prefers-color-scheme`.
  cssVariables: true,
  colorSchemes: {
    light: {
      palette: {
        primary: { main: '#1C2124', contrastText: '#FFFFFF' },
        secondary: { main: '#C67F09', contrastText: '#15181A' },
        error: { main: '#B3261E' },
        warning: { main: '#96601A' },
        info: { main: '#2C5C82' },
        background: { default: '#ECEEEB', paper: '#FFFFFF' },
        text: { primary: '#15181A', secondary: '#5B6469' },
        divider: '#D8DCD6',
      },
    },
    dark: {
      palette: {
        primary: { main: '#E7EAE6', contrastText: '#15181A' },
        secondary: { main: '#F0AE3F', contrastText: '#15181A' },
        error: { main: '#F0736A' },
        warning: { main: '#E0A44F' },
        info: { main: '#7FB2DA' },
        background: { default: '#101315', paper: '#181C1F' },
        text: { primary: '#E7EAE6', secondary: '#98A1A6' },
        divider: '#2A3034',
      },
    },
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: sansFamily,
    h1: {
      fontSize: '2rem',
      fontWeight: 600,
      letterSpacing: '-0.025em',
      lineHeight: 1.12,
    },
    h2: {
      fontSize: '1.125rem',
      fontWeight: 600,
      letterSpacing: '-0.01em',
      fontVariantNumeric: 'tabular-nums',
    },
    subtitle1: { fontSize: '1rem', fontWeight: 600, letterSpacing: '-0.005em' },
    body1: { fontSize: '0.9375rem', lineHeight: 1.5 },
    body2: { fontSize: '0.875rem', lineHeight: 1.55 },
    // The utility voice: condensed uppercase for the small labels that name a field or a
    // column, the way a spec sheet labels its rows.
    overline: {
      fontFamily: condensedFamily,
      fontSize: '0.75rem',
      fontWeight: 600,
      letterSpacing: '0.14em',
      lineHeight: 1.2,
      textTransform: 'uppercase',
    },
    // Every identifier and count is monospaced so digits align down the page.
    caption: {
      fontFamily: monoFamily,
      fontSize: '0.75rem',
      letterSpacing: '0.02em',
      lineHeight: 1.4,
    },
    button: { fontWeight: 600, letterSpacing: '0.01em', textTransform: 'none' },
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: (t) => ({
        body: {
          WebkitFontSmoothing: 'antialiased',
          MozOsxFontSmoothing: 'grayscale',
        },
        '::selection': {
          backgroundColor: t.palette.secondary.main,
          color: t.palette.secondary.contrastText,
        },
        // One focus treatment everywhere, in the signal colour.
        '*:focus-visible': {
          outline: `2px solid ${t.palette.secondary.main}`,
          outlineOffset: '2px',
        },
        '@media (prefers-reduced-motion: reduce)': {
          '*, *::before, *::after': {
            animationDuration: '0.01ms !important',
            animationIterationCount: '1 !important',
            transitionDuration: '0.01ms !important',
          },
        },
      }),
    },
    MuiPaper: {
      styleOverrides: { root: { backgroundImage: 'none' } },
    },
    // Notices are plates with a coloured rail rather than tinted blocks: the severity is
    // carried by one 3px edge, so an error and a warning sit at the same visual weight as
    // everything else on the page.
    MuiAlert: {
      styleOverrides: {
        root: ({ theme: t }) => ({
          alignItems: 'center',
          border: '1px solid',
          borderColor: t.palette.divider,
          borderLeftWidth: 3,
        }),
        standardError: ({ theme: t }) => ({ borderLeftColor: t.palette.error.main }),
        standardWarning: ({ theme: t }) => ({ borderLeftColor: t.palette.warning.main }),
        standardInfo: ({ theme: t }) => ({ borderLeftColor: t.palette.info.main }),
        standardSuccess: ({ theme: t }) => ({ borderLeftColor: t.palette.success.main }),
      },
    },
    MuiAlertTitle: {
      styleOverrides: { root: { fontWeight: 600, marginBottom: 2 } },
    },
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { borderRadius: 8 } },
    },
    MuiMenu: {
      styleOverrides: {
        paper: ({ theme: t }) => ({
          border: '1px solid',
          borderColor: t.palette.divider,
          boxShadow: '0 18px 44px -16px rgba(0, 0, 0, 0.32)',
        }),
      },
    },
    MuiMenuItem: {
      styleOverrides: { root: { fontSize: '0.9375rem', minHeight: 38 } },
    },
    MuiAutocomplete: {
      styleOverrides: {
        paper: ({ theme: t }) => ({
          border: '1px solid',
          borderColor: t.palette.divider,
          boxShadow: '0 18px 44px -16px rgba(0, 0, 0, 0.32)',
        }),
        option: { fontSize: '0.9375rem' },
      },
    },
    MuiInputBase: {
      styleOverrides: { root: { fontSize: '0.9375rem', fontWeight: 500 } },
    },
  },
});
