using VehicleExplorer.Application.Models;

namespace VehicleExplorer.Application.Abstractions;

/// <summary>
/// The port. Everything this application needs from the vehicle data provider, stated
/// without reference to how that provider actually behaves. The adapter implementing
/// this is the only place allowed to know about envelopes, snake-cased identifiers or
/// which query string turns JSON on.
/// </summary>
public interface INhtsaClient
{
    Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(
        int makeId,
        CancellationToken cancellationToken);
}
