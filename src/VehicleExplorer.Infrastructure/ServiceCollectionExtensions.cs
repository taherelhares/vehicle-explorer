using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Infrastructure.Clients;
using VehicleExplorer.Infrastructure.Options;

namespace VehicleExplorer.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExternalApi<INhtsaApi, NhtsaOptions>(configuration);

        services.AddScoped<INhtsaClient, NhtsaClient>();

        return services;
    }
}
