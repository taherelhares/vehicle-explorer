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
}
