using Microsoft.Extensions.DependencyInjection;
using VehicleExplorer.Application.Services;

namespace VehicleExplorer.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IVehicleCatalogService, VehicleCatalogService>();

        return services;
    }
}
