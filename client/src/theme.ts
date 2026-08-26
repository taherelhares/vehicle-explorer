import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  colorSchemes: { light: true, dark: true },
  shape: { borderRadius: 10 },
  typography: {
    h1: { fontSize: '1.75rem', fontWeight: 600, letterSpacing: '-0.02em' },
    h2: { fontSize: '1rem', fontWeight: 600 },
  },
});
