using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Services;

internal sealed class VehicleCatalogService(INhtsaClient client) : IVehicleCatalogService
{
    public Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken) =>
        client.GetMakesAsync(cancellationToken);

    public Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(
        int makeId,
        CancellationToken cancellationToken) =>
        client.GetVehicleTypesAsync(makeId, cancellationToken);

    public Task<IReadOnlyList<ModelDto>> GetModelsAsync(
        int makeId,
        int year,
        string? vehicleType,
        CancellationToken cancellationToken) =>
        client.GetModelsAsync(makeId, year, vehicleType, cancellationToken);
}
