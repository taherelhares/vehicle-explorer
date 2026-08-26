import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import App from './App';

interface Route {
  match: (url: string) => boolean;
  body: unknown;
  status?: number;
  problem?: boolean;
}

/** Routes requests by URL so a test only states the responses it cares about. */
function stubApi(routes: Route[]) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
    const url = String(input);
    const route = routes.find((candidate) => candidate.match(url));

    if (!route) {
      return new Response('[]', {
        status: 200,
        headers: { 'content-type': 'application/json' },
      });
    }

    return new Response(JSON.stringify(route.body), {
      status: route.status ?? 200,
      headers: {
        'content-type': route.problem ? 'application/problem+json' : 'application/json',
      },
    });
  });

  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

const makesRoute = (body: unknown, extra: Partial<Route> = {}): Route => ({
  match: (url) => url.endsWith('/api/vehicles/makes'),
  body,
  ...extra,
});

afterEach(() => vi.unstubAllGlobals());

describe('App', () => {
  it('offers the makes returned by the API', async () => {
    stubApi([makesRoute([{ id: 474, name: 'HONDA' }])]);

    render(<App />);

    const make = screen.getByRole('combobox', { name: /make/i });
    await userEvent.click(make);
    await userEvent.type(make, 'hon');

    expect(await screen.findByText('HONDA')).toBeInTheDocument();
  });

  it('loads vehicle types and models for the chosen make', async () => {
    const fetchMock = stubApi([
      makesRoute([{ id: 474, name: 'HONDA' }]),
      {
        match: (url) => url.includes('/makes/474/vehicle-types'),
        body: [{ id: 2, name: 'Passenger Car' }],
      },
      {
        match: (url) => url.includes('/makes/474/models'),
        body: [{ id: 1861, name: 'Accord' }],
      },
    ]);

    render(<App />);

    const make = screen.getByRole('combobox', { name: /make/i });
    await userEvent.click(make);
    await userEvent.type(make, 'hon');
    await userEvent.click(await screen.findByText('HONDA'));

    expect(await screen.findByText('Accord')).toBeInTheDocument();
    expect(await screen.findByText(/1 model/i)).toBeInTheDocument();

    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some((call) => String(call[0]).includes('/vehicle-types')),
      ).toBe(true),
    );
  });

  it('shows the unavailable state, with a retry, when the upstream is down', async () => {
    stubApi([
      makesRoute(
        {
          title: 'Vehicle data is temporarily unavailable',
          detail: 'The vehicle data service did not respond. Please try again shortly.',
        },
        { status: 503, problem: true },
      ),
    ]);

    render(<App />);

    expect(
      await screen.findByText(/vehicle data is temporarily unavailable/i),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
  });
});
