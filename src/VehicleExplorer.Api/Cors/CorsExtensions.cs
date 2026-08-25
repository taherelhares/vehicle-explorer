namespace VehicleExplorer.Api.Cors;

/// <summary>
/// A browser client is the only reason this API needs a CORS policy, and which origins
/// are allowed genuinely differs between environments — so the list is bound from
/// configuration rather than compiled in.
/// </summary>
internal static class CorsExtensions
{
    public const string PolicyName = "VehicleExplorerClient";

    public static IServiceCollection AddClientCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        return services.AddCors(options => options.AddPolicy(
            PolicyName,
            policy => policy
                .WithOrigins(origins)
                // Every endpoint here is a read, so the policy is as narrow as the API
                // actually is. An unconfigured origin list denies rather than permits: a
                // missing deployment setting should fail visibly, not quietly open the
                // API to every site on the internet.
                .WithMethods(HttpMethods.Get)
                .AllowAnyHeader()));
    }
}
