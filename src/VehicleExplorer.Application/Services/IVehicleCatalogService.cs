using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Services;

public interface IVehicleCatalogService
{
    Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken);
}
