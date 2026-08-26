import Alert from '@mui/material/Alert';
import AlertTitle from '@mui/material/AlertTitle';
import Button from '@mui/material/Button';
import type { ApiError } from '../services/vehicleService';

interface Props {
  error: ApiError;
  onRetry: () => void;
}

/**
 * The API distinguishes "the provider is down, try again" from everything else, so the
 * interface does too. Retrying a 503 is worth offering; retrying a bad request is not.
 */
export function ErrorNotice({ error, onRetry }: Props) {
  const upstream = error.isUpstreamUnavailable;

  return (
    <Alert
      severity={upstream ? 'warning' : 'error'}
      action={
        upstream ? (
          <Button color="inherit" size="small" onClick={onRetry}>
            Retry
          </Button>
        ) : undefined
      }
    >
      <AlertTitle>
        {upstream ? 'Vehicle data is temporarily unavailable' : 'Something went wrong'}
      </AlertTitle>
      {error.message}
    </Alert>
  );
}
