using Refit;
using VehicleExplorer.Infrastructure.Models.Nhtsa;

namespace VehicleExplorer.Infrastructure.Clients;

/// <summary>
/// The vPIC service as it actually is: envelopes, snake-cased identifiers and an
/// explicit <c>format=json</c> on every route. Deliberately <c>internal</c> — nothing
/// outside this assembly should have to know this vocabulary exists.
/// </summary>
/// <remarks>
/// Every method returns <see cref="IApiResponse{T}"/> rather than the payload directly.
/// Refit then reports an unsuccessful status as data on the response instead of raising
/// an exception, so a 500 from vPIC is inspected rather than caught.
/// </remarks>
internal interface INhtsaApi
{
    [Get("/api/vehicles/getallmakes?format=json")]
    Task<IApiResponse<NhtsaResponse<NhtsaMake>>> GetAllMakesAsync(CancellationToken cancellationToken);

    [Get("/api/vehicles/GetVehicleTypesForMakeId/{makeId}?format=json")]
    Task<IApiResponse<NhtsaResponse<NhtsaVehicleType>>> GetVehicleTypesForMakeAsync(
        int makeId,
        CancellationToken cancellationToken);

    [Get("/api/vehicles/GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{year}?format=json")]
    Task<IApiResponse<NhtsaResponse<NhtsaModel>>> GetModelsForMakeYearAsync(
        int makeId,
        int year,
        CancellationToken cancellationToken);

    /// <summary>
    /// vPIC accepts an optional <c>vehicleType</c> segment on the models route, so the
    /// filter is pushed upstream rather than applied after the fact.
    /// </summary>
    [Get("/api/vehicles/GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{year}/vehicleType/{vehicleType}?format=json")]
    Task<IApiResponse<NhtsaResponse<NhtsaModel>>> GetModelsForMakeYearAndTypeAsync(
        int makeId,
        int year,
        string vehicleType,
        CancellationToken cancellationToken);
}
