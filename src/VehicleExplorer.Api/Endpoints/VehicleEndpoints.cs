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

        return endpoints;
    }
}
