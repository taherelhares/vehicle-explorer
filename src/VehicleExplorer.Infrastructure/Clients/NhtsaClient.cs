using Refit;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;
using VehicleExplorer.Infrastructure.Models.Nhtsa;

namespace VehicleExplorer.Infrastructure.Clients;

/// <summary>
/// The anti-corruption layer. Calls vPIC through <see cref="INhtsaApi"/> and hands back
/// this application's own types, so no vPIC vocabulary escapes this assembly.
/// </summary>
internal sealed class NhtsaClient(INhtsaApi api) : INhtsaClient
{
    public async Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken cancellationToken)
    {
        using var response = await api.GetAllMakesAsync(cancellationToken);

        // vPIC has been observed returning rows with a null name. They are useless to a
        // picker, so they are dropped here rather than defended against in every consumer.
        return [.. Unwrap(response).Results
            .Where(make => !string.IsNullOrWhiteSpace(make.MakeName))
            .Select(make => new MakeDto(make.MakeId, make.MakeName!))];
    }

    public async Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesAsync(
        int makeId,
        CancellationToken cancellationToken)
    {
        using var response = await api.GetVehicleTypesForMakeAsync(makeId, cancellationToken);

        // A make vPIC does not recognise comes back as a successful, empty envelope
        // rather than a 404, so an unknown id is indistinguishable from a make with no
        // recorded types. Both are reported to the caller as an empty list.
        return [.. Unwrap(response).Results
            .Where(type => !string.IsNullOrWhiteSpace(type.VehicleTypeName))
            .Select(type => new VehicleTypeDto(type.VehicleTypeId, type.VehicleTypeName!))];
    }

    public async Task<IReadOnlyList<ModelDto>> GetModelsAsync(
        int makeId,
        int year,
        string? vehicleType,
        CancellationToken cancellationToken)
    {
        // The filter is pushed upstream rather than applied to a larger result set here.
        // A blank value is normalised to null so Refit leaves the parameter off entirely.
        var filter = string.IsNullOrWhiteSpace(vehicleType) ? null : vehicleType.Trim();

        using var response = await api.GetModelsForMakeYearAsync(
            makeId, year, filter, cancellationToken);

        return [.. Unwrap(response).Results
            .Where(model => !string.IsNullOrWhiteSpace(model.ModelName))
            .Select(model => new ModelDto(model.ModelId, model.ModelName!))];
    }

    /// <summary>
    /// Returns the payload of a successful vPIC response, or raises the single
    /// application-level failure that the API layer knows how to present.
    /// </summary>
    private static T Unwrap<T>(IApiResponse<T> response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new NhtsaUnavailableException(
                $"The NHTSA vPIC service responded with {(int?)response.StatusCode}.",
                response.Error!);
        }

        return response.Content
            ?? throw new NhtsaUnavailableException(
                "The NHTSA vPIC service returned an empty body.");
    }
}
