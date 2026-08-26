import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ApiError } from '../services/vehicleService';
import { useResource } from './useResource';

describe('useResource', () => {
  it('moves from loading to data', async () => {
    const { result } = renderHook(() => useResource(() => Promise.resolve(['a']), []));

    expect(result.current.loading).toBe(true);

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.data).toEqual(['a']);
    expect(result.current.error).toBeNull();
  });

  it('reports an ApiError and clears the stale data', async () => {
    const failure = new ApiError('vPIC is down.', 503, true);
    const { result } = renderHook(() => useResource(() => Promise.reject(failure), []));

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.error).toBe(failure);
    expect(result.current.data).toBeNull();
  });

  it('stays idle while disabled, so a dependent select never loads too early', async () => {
    const load = vi.fn().mockResolvedValue(['a']);

    const { result } = renderHook(() => useResource(load, [], false));

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(load).not.toHaveBeenCalled();
    expect(result.current.data).toBeNull();
  });
});
