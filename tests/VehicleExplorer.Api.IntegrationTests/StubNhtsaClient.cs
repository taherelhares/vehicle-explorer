using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Api.IntegrationTests;

/// <summary>
/// Replaces the real adapter so the tests exercise the host, not vPIC. Each test sets the
/// delegate it needs, including one that throws, which is how the failure path is reached
/// without an outage.
/// </summary>
public sealed class StubNhtsaClient : INhtsaClient
{
    public Func<IReadOnlyList<MakeDto>> Makes { get; set; } = () => [];

    public Func<int, IReadOnlyList<VehicleTypeDto>> VehicleTypes { get; set; } = _ => [];

    public Func<int, int, string?, IReadOnlyList<ModelDto>> Models { get; set; } = (_, _, _) => [];

    public Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Makes());

    public Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(
        int makeId,
        CancellationToken cancellationToken) =>
        Task.FromResult(VehicleTypes(makeId));

    public Task<IReadOnlyList<ModelDto>> GetModelsAsync(
        int makeId,
        int year,
        string? vehicleType,
        CancellationToken cancellationToken) =>
        Task.FromResult(Models(makeId, year, vehicleType));
}
