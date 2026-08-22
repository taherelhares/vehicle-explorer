using Refit;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;

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
