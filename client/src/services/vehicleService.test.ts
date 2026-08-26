import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, vehicleService } from './vehicleService';

function respondWith(body: unknown, init: ResponseInit = {}) {
  return vi.fn().mockResolvedValue(
    new Response(JSON.stringify(body), {
      status: 200,
      headers: { 'content-type': 'application/json' },
      ...init,
    }),
  );
}

function calledUrl(fetchMock: ReturnType<typeof vi.fn>): string {
  return String(fetchMock.mock.calls[0]?.[0]);
}

afterEach(() => vi.unstubAllGlobals());

describe('vehicleService', () => {
  it('omits the vehicle type filter when none is chosen', async () => {
    const fetchMock = respondWith([]);
    vi.stubGlobal('fetch', fetchMock);

    await vehicleService.getModels(474, 2015, null);

    expect(calledUrl(fetchMock)).toContain('/api/vehicles/makes/474/models?year=2015');
    expect(calledUrl(fetchMock)).not.toContain('vehicleType');
  });

  it('sends the vehicle type filter when one is chosen', async () => {
    const fetchMock = respondWith([]);
    vi.stubGlobal('fetch', fetchMock);

    await vehicleService.getModels(474, 2015, 'Passenger Car');

    expect(calledUrl(fetchMock)).toContain('vehicleType=Passenger+Car');
  });

  it('surfaces the problem details message when the upstream is unavailable', async () => {
    vi.stubGlobal(
      'fetch',
      respondWith(
        {
          title: 'Vehicle data is temporarily unavailable',
          detail: 'The vehicle data service did not respond. Please try again shortly.',
        },
        { status: 503, headers: { 'content-type': 'application/problem+json' } },
      ),
    );

    const failure = await vehicleService.getMakes().catch((error: unknown) => error);

    expect(failure).toBeInstanceOf(ApiError);
    expect((failure as ApiError).isUpstreamUnavailable).toBe(true);
    expect((failure as ApiError).message).toContain('did not respond');
  });

  it('reports a network failure without pretending it came from the API', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));

    const failure = (await vehicleService.getMakes().catch((error: unknown) => error)) as ApiError;

    expect(failure).toBeInstanceOf(ApiError);
    expect(failure.status).toBe(0);
    expect(failure.isUpstreamUnavailable).toBe(false);
  });
});
