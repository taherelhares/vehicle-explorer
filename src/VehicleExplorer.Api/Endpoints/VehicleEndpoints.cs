using VehicleExplorer.Application.Services;

namespace VehicleExplorer.Api.Endpoints;

internal static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var vehicles = endpoints
            .MapGroup("/api/vehicles")
            .WithTags("Vehicles");

        vehicles.MapGet("/makes", async (
                IVehicleCatalogService catalog,
                CancellationToken cancellationToken) =>
                TypedResults.Ok(await catalog.GetMakesAsync(cancellationToken)))
            .WithName("GetMakes")
            .WithSummary("Every vehicle make known to vPIC.");

        // The route constraint rejects a malformed id before any of our code runs, so
        // the handler only ever deals with plausible input.
        vehicles.MapGet("/makes/{makeId:int:min(1)}/vehicle-types", async (
                int makeId,
                IVehicleCatalogService catalog,
                CancellationToken cancellationToken) =>
                TypedResults.Ok(await catalog.GetVehicleTypesAsync(makeId, cancellationToken)))
            .WithName("GetVehicleTypesForMake")
            .WithSummary("The vehicle types recorded for a given make.");

        return endpoints;
    }
}
