using Microsoft.Extensions.DependencyInjection;
using VehicleExplorer.Application.Services;

namespace VehicleExplorer.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // The catalogue is identical for every visitor, so a single in-process cache is
        // shared by all of them rather than held per request.
        services.AddMemoryCache();
        services.AddScoped<IVehicleCatalogService, VehicleCatalogService>();

        return services;
    }
}
