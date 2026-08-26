import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Skeleton from '@mui/material/Skeleton';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import type { Theme } from '@mui/material/styles';
import type { ApiError, CatalogItem } from '../services/vehicleService';
import { ErrorNotice } from './ErrorNotice';
import { Tick } from './TopBar';

export interface Query {
  make: string | null;
  year: number;
  vehicleType: string;
}

interface Props {
  models: CatalogItem[] | null;
  loading: boolean;
  error: ApiError | null;
  ready: boolean;
  query: Query;
  onRetry: () => void;
}

const SKELETON_WIDTHS = [58, 44, 67, 39, 52, 47];

export function ModelResults({ models, loading, error, ready, query, onRetry }: Props) {
  if (!ready) {
    return (
      <Placeholder title="Start with a make">
        Pick a manufacturer and every model vPIC records for that year and type is listed
        here.
      </Placeholder>
    );
  }

  if (error) {
    return <ErrorNotice error={error} onRetry={onRetry} />;
  }

  if (loading) {
    return (
      <Stack spacing={2} aria-busy="true" aria-label="Loading models">
        <Plate query={query}>
          <Typography variant="overline" sx={{ color: 'text.secondary' }}>
            Searching
          </Typography>
        </Plate>
        <Surface labelled>
          {SKELETON_WIDTHS.map((width) => (
            <Row key={width}>
              <Skeleton variant="text" width={`${width}%`} sx={{ fontSize: '0.9375rem' }} />
            </Row>
          ))}
        </Surface>
      </Stack>
    );
  }

  if (models && models.length === 0) {
    // vPIC answers an unknown combination with an empty list rather than an error, so
    // this is a real answer and is worded as one.
    return (
      <Placeholder title="No models recorded">
        vPIC has nothing for this make, year and type. Try another year, or set the type
        back to all types.
      </Placeholder>
    );
  }

  if (!models) {
    return null;
  }

  return (
    <Stack spacing={2}>
      <Plate query={query}>
        <Typography variant="h2" component="h2">
          {models.length} {models.length === 1 ? 'model' : 'models'}
        </Typography>
      </Plate>
      <Surface labelled>
        {models.map((model) => (
          <Row key={model.id} interactive>
            <Typography
              sx={{
                flex: 1,
                minWidth: 0,
                fontWeight: 500,
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
              }}
            >
              {model.name}
            </Typography>
            <Typography variant="caption" sx={{ color: 'text.secondary', flexShrink: 0 }}>
              {model.id}
            </Typography>
          </Row>
        ))}
      </Surface>
    </Stack>
  );
}

/**
 * The result plate: what was found on the left, what it was found for on the right, so
 * the list is never read without the query that produced it.
 */
function Plate({ query, children }: { query: Query; children: ReactNode }) {
  const stamp = [query.make ?? '', query.year, query.vehicleType || 'All types']
    .filter(Boolean)
    .join('  ·  ');

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25, flexWrap: 'wrap' }}>
      <Tick height={14} />
      {children}
      <Box sx={{ flex: 1, minWidth: 8 }} />
      <Typography variant="overline" sx={{ color: 'text.secondary' }}>
        {stamp}
      </Typography>
    </Box>
  );
}

/** The bordered plate the rows sit on, with the column names stated once. */
function Surface({ children, labelled = false }: { children: ReactNode; labelled?: boolean }) {
  return (
    <Paper variant="outlined" sx={{ overflow: 'hidden' }}>
      {labelled ? (
        <Box
          sx={{
            display: 'flex',
            gap: 2,
            px: 2,
            py: 0.75,
            borderBottom: '1px solid',
            borderColor: 'divider',
            backgroundColor: 'action.hover',
          }}
        >
          <Typography variant="overline" sx={{ flex: 1, color: 'text.secondary' }}>
            Model
          </Typography>
          <Typography variant="overline" sx={{ color: 'text.secondary' }}>
            vPIC ID
          </Typography>
        </Box>
      ) : null}
      <Box component="ul" sx={{ listStyle: 'none', m: 0, p: 0 }}>
        {children}
      </Box>
    </Paper>
  );
}

/**
 * Rows are separated by a single hairline drawn above every row but the first, so the
 * last row never doubles up against the plate's own border.
 */
function Row({ children, interactive = false }: { children: ReactNode; interactive?: boolean }) {
  return (
    <Box
      component="li"
      sx={{
        display: 'flex',
        alignItems: 'baseline',
        gap: 2,
        px: 2,
        py: 1.25,
        transition: 'background-color 120ms ease, box-shadow 120ms ease',
        '&:not(:first-of-type)': { borderTop: '1px solid', borderColor: 'divider' },
        ...(interactive
          ? {
              '&:hover': {
                backgroundColor: 'action.hover',
                boxShadow: (t: Theme) => `inset 3px 0 0 ${t.palette.secondary.main}`,
              },
            }
          : {}),
      }}
    >
      {children}
    </Box>
  );
}

/** Nothing to list yet, or nothing to list at all. Both say what to do next. */
function Placeholder({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Box
      sx={{
        border: '1px dashed',
        borderColor: 'divider',
        borderRadius: 1,
        px: 3,
        py: 6,
      }}
    >
      <Stack spacing={1.25} alignItems="center" textAlign="center">
        <Tick height={14} />
        <Typography variant="subtitle1">{title}</Typography>
        <Typography variant="body2" sx={{ color: 'text.secondary', maxWidth: 360 }}>
          {children}
        </Typography>
      </Stack>
    </Box>
  );
}
