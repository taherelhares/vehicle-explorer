using Microsoft.Extensions.Caching.Memory;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Services;

/// <summary>
/// Holds this application's freshness policy for the vehicle catalogue. The port supplies
/// the data; how long an answer stays usable is decided here, above the port, because it
/// is a statement about the catalogue rather than about HTTP or about vPIC.
/// </summary>
internal sealed class VehicleCatalogService(INhtsaClient client, IMemoryCache cache)
    : IVehicleCatalogService
{
    /// <summary>
    /// The catalogue changes when a manufacturer registers a make or files a model year,
    /// which is a matter of months. A day is well inside that and spares every visitor a
    /// slow upstream call.
    /// </summary>
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync(
            "catalog:makes",
            () => client.GetMakesAsync(cancellationToken));

    public Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(
        int makeId,
        CancellationToken cancellationToken) =>
        GetOrLoadAsync(
            $"catalog:vehicle-types:{makeId}",
            () => client.GetVehicleTypesAsync(makeId, cancellationToken));

    public Task<IReadOnlyList<ModelDto>> GetModelsAsync(
        int makeId,
        int year,
        string? vehicleType,
        CancellationToken cancellationToken) =>
        GetOrLoadAsync(
            // Every argument that changes the answer belongs in the key. Case and
            // surrounding whitespace do not change it, so they are normalised away and
            // "Car", "car" and " car " share a single entry.
            $"catalog:models:{makeId}:{year}:{vehicleType?.Trim().ToLowerInvariant()}",
            () => client.GetModelsAsync(makeId, year, vehicleType, cancellationToken));

    private async Task<IReadOnlyList<T>> GetOrLoadAsync<T>(
        string key,
        Func<Task<IReadOnlyList<T>>> load)
    {
        if (cache.TryGetValue(key, out IReadOnlyList<T>? cached) && cached is not null)
        {
            return cached;
        }

        var fresh = await load();

        // An empty result is never stored. vPIC intermittently answers with a well formed
        // envelope containing no rows, and caching that would turn a momentary upstream
        // glitch into a day of an empty dropdown.
        if (fresh.Count > 0)
        {
            cache.Set(key, fresh, Lifetime);
        }

        return fresh;
    }
}
