import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import type { SxProps, Theme } from '@mui/material/styles';

interface Props {
  /** Visible name of the control, also its accessible name. */
  label: string;
  /** Id given to the label element so a Select can point at it with `labelId`. */
  labelId: string;
  /** Id of the control, when the control is a real form element a label can target. */
  htmlFor?: string;
  /** One quiet line under the control: what it holds, or why it cannot be used. */
  helper?: ReactNode;
  children: ReactNode;
}

/**
 * One cell of the filter cluster: a condensed label, a borderless control, and a helper
 * line. The cells share a single plate, so the controls drop their own borders and the
 * cell itself reports focus — an amber lamp along its bottom edge, which is the only
 * focus indicator inside the cluster.
 */
export function FilterField({ label, labelId, htmlFor, helper, children }: Props) {
  return (
    <Box
      sx={{
        position: 'relative',
        display: 'flex',
        flexDirection: 'column',
        gap: 0.75,
        minWidth: 0,
        px: 2,
        py: 1.75,
        '&::after': {
          content: '""',
          position: 'absolute',
          insetInline: 0,
          bottom: 0,
          height: 2,
          backgroundColor: 'secondary.main',
          transform: 'scaleX(0)',
          transformOrigin: 'left',
          transition: 'transform 160ms ease',
        },
        '&:focus-within::after': { transform: 'scaleX(1)' },
        // The lamp is the indicator; a second outline around the input would double it.
        '& input:focus-visible, & .MuiSelect-select:focus-visible': { outline: 'none' },
      }}
    >
      <Typography
        component="label"
        variant="overline"
        id={labelId}
        htmlFor={htmlFor}
        sx={{ color: 'text.secondary', width: 'fit-content' }}
      >
        {label}
      </Typography>
      {children}
      <Typography
        variant="caption"
        sx={{ color: 'text.secondary', minHeight: 16, display: 'block' }}
      >
        {helper}
      </Typography>
    </Box>
  );
}

/**
 * Strips the standard-variant underline and its focus rule from an input or select, so
 * the cell's lamp is the only thing that moves.
 */
export const plainControlSx = {
  '&::before, &::after, & .MuiInput-root::before, & .MuiInput-root::after': {
    display: 'none',
  },
  '&.MuiInput-root, & .MuiInput-root': { marginTop: 0 },
  '& input, & .MuiSelect-select': { paddingTop: '2px', paddingBottom: '2px' },
} satisfies SxProps<Theme>;
