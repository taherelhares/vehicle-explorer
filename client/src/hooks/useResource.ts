import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../services/vehicleService';

export interface Resource<T> {
  data: T | null;
  loading: boolean;
  error: ApiError | null;
  /** Re-runs the load. Bound to the retry affordance on the error state. */
  reload: () => void;
}

/**
 * Loads one thing and reports the three states a caller can be in. Kept deliberately
 * small: the API already caches for a day server-side, so a client-side cache would be
 * re-solving a solved problem.
 *
 * @param load    Receives an AbortSignal; pass it through so a superseded request stops.
 * @param enabled When false the resource stays idle — used for the selects that cannot
 *                load anything until a make has been chosen.
 */
export function useResource<T>(
  load: (signal: AbortSignal) => Promise<T>,
  deps: readonly unknown[],
  enabled = true,
): Resource<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<ApiError | null>(null);
  const [attempt, setAttempt] = useState(0);

  const reload = useCallback(() => setAttempt((n) => n + 1), []);

  useEffect(() => {
    if (!enabled) {
      setData(null);
      setError(null);
      setLoading(false);
      return;
    }

    const controller = new AbortController();

    setLoading(true);
    setError(null);

    load(controller.signal)
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((cause: unknown) => {
        // The request was superseded by a newer one; its result is no longer wanted and
        // reporting it would overwrite fresher state.
        if (cause instanceof DOMException && cause.name === 'AbortError') {
          return;
        }

        setData(null);
        setError(
          cause instanceof ApiError
            ? cause
            : new ApiError('Something went wrong.', 0, false),
        );
        setLoading(false);
      });

    return () => controller.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, enabled, attempt]);

  return { data, loading, error, reload };
}
