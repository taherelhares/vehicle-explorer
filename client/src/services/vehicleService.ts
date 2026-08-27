/**
 * Everything that knows the API exists. Hooks and components above this file deal in
 * catalogue records and errors, never in URLs, query strings or status codes.
 */

// Unset means same origin, which is how the container serves it: the API hosts the
// built client, so a relative path reaches the API it was served from. Development
// overrides this in .env.development, where the Vite server is a separate origin.
const baseUrl = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '');

/** Every catalogue endpoint returns the same shape. */
export interface CatalogItem {
  id: number;
  name: string;
}

/**
 * One error type for the UI to react to. `isUpstreamUnavailable` separates "the vehicle
 * data provider is down, trying again may work" from anything else, because that is the
 * only distinction the interface actually acts on.
 */
export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly isUpstreamUnavailable: boolean,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/** RFC 7807, which is what the API returns when it cannot reach vPIC. */
interface ProblemDetails {
  title?: string;
  detail?: string;
}

async function readProblem(response: Response): Promise<string | null> {
  if (!response.headers.get('content-type')?.includes('problem+json')) {
    return null;
  }

  try {
    const problem = (await response.json()) as ProblemDetails;
    return problem.detail ?? problem.title ?? null;
  } catch {
    return null;
  }
}

async function get<T>(path: string, signal?: AbortSignal): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`${baseUrl}${path}`, {
      signal,
      headers: { Accept: 'application/json' },
    });
  } catch (cause) {
    // An aborted request is a cancellation, not a failure, so it is rethrown untouched
    // for the caller to ignore.
    if (cause instanceof DOMException && cause.name === 'AbortError') {
      throw cause;
    }

    throw new ApiError('Could not reach the Vehicle Explorer API.', 0, false);
  }

  if (!response.ok) {
    const isUpstreamUnavailable = response.status === 503;
    const detail = await readProblem(response);

    throw new ApiError(
      detail ?? `The API responded with ${response.status}.`,
      response.status,
      isUpstreamUnavailable,
    );
  }

  return (await response.json()) as T;
}

export const vehicleService = {
  getMakes: (signal?: AbortSignal) => get<CatalogItem[]>('/api/vehicles/makes', signal),

  getVehicleTypes: (makeId: number, signal?: AbortSignal) =>
    get<CatalogItem[]>(`/api/vehicles/makes/${makeId}/vehicle-types`, signal),

  getModels: (makeId: number, year: number, vehicleType: string | null, signal?: AbortSignal) => {
    const query = new URLSearchParams({ year: String(year) });

    // The filter is omitted rather than sent empty, mirroring how the API treats it.
    if (vehicleType) {
      query.set('vehicleType', vehicleType);
    }

    return get<CatalogItem[]>(`/api/vehicles/makes/${makeId}/models?${query}`, signal);
  },
};
