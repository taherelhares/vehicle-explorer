import Box from '@mui/material/Box';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import Skeleton from '@mui/material/Skeleton';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import type { ApiError, CatalogItem } from '../services/vehicleService';
import { ErrorNotice } from './ErrorNotice';

interface Props {
  models: CatalogItem[] | null;
  loading: boolean;
  error: ApiError | null;
  ready: boolean;
  onRetry: () => void;
}

export function ModelResults({ models, loading, error, ready, onRetry }: Props) {
  if (!ready) {
    return <Placeholder>Choose a make to see its models.</Placeholder>;
  }

  if (error) {
    return <ErrorNotice error={error} onRetry={onRetry} />;
  }

  if (loading) {
    return (
      <Stack spacing={1} aria-busy="true" aria-label="Loading models">
        {[0, 1, 2, 3].map((row) => (
          <Skeleton key={row} variant="rounded" height={48} />
        ))}
      </Stack>
    );
  }

  if (models && models.length === 0) {
    // vPIC answers an unknown combination with an empty list rather than an error, so
    // this is a real answer and is worded as one.
    return <Placeholder>No models recorded for this make, year and type.</Placeholder>;
  }

  if (!models) {
    return null;
  }

  return (
    <Stack spacing={1.5}>
      <Typography variant="h2" component="h2">
        {models.length} {models.length === 1 ? 'model' : 'models'}
      </Typography>
      <Paper variant="outlined">
        <List dense disablePadding>
          {models.map((model) => (
            <ListItem key={model.id} divider>
              <ListItemText primary={model.name} secondary={`ID ${model.id}`} />
            </ListItem>
          ))}
        </List>
      </Paper>
    </Stack>
  );
}

function Placeholder({ children }: { children: React.ReactNode }) {
  return (
    <Box
      sx={{
        p: 4,
        textAlign: 'center',
        color: 'text.secondary',
        border: '1px dashed',
        borderColor: 'divider',
        borderRadius: 2,
      }}
    >
      <Typography variant="body2">{children}</Typography>
    </Box>
  );
}
