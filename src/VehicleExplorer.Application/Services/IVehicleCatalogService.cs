using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Services;

public interface IVehicleCatalogService
{
    Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(
        int makeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ModelDto>> GetModelsAsync(
        int makeId,
        int year,
        string? vehicleType,
        CancellationToken cancellationToken);
}
