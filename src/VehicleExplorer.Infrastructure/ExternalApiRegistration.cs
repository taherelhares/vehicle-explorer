using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using VehicleExplorer.Infrastructure.Options;

namespace VehicleExplorer.Infrastructure;

/// <summary>
/// One place defining how this application talks to any third-party HTTP API:
/// serializer behaviour, address binding, timeouts, retries and circuit breaking.
/// Adding another upstream service is a single call, not another copy of this policy.
/// </summary>
internal static class ExternalApiRegistration
{
    /// <summary>Budget for a single attempt. Third-party APIs of this kind are slow.</summary>
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(20);

    /// <summary>Budget for the whole call including retries.</summary>
    private static readonly TimeSpan TotalRequestTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Circuit breaker sampling window. The resilience validator requires at least twice
    /// <see cref="AttemptTimeout"/>, so it is derived rather than stated independently.
    /// </summary>
    private static readonly TimeSpan CircuitBreakerSamplingDuration = AttemptTimeout * 3;

    private static readonly RefitSettings DefaultRefitSettings = new()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        })
    };

    /// <summary>
    /// Binds <typeparamref name="TOptions"/> from its own configuration section and
    /// registers <typeparamref name="TApi"/> as a resilient Refit client against it.
    /// </summary>
    /// <param name="configureResilience">
    /// Optional per-client adjustment, for an upstream service whose timing differs
    /// enough that the shared budget does not fit.
    /// </param>
    public static IHttpClientBuilder AddExternalApi<TApi, TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HttpStandardResilienceOptions>? configureResilience = null)
        where TApi : class
        where TOptions : ExternalApiOptions, IExternalApiSection
    {
        // Validated at startup rather than on first request: a missing address should
        // stop the process, not surface later as a confusing runtime failure.
        services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(TOptions.SectionName))
            .Validate(
                static options => options.BaseAddress is not null,
                $"{TOptions.SectionName}:BaseAddress must be configured.")
            .ValidateOnStart();

        var builder = services
            .AddRefitClient<TApi>(DefaultRefitSettings)
            .ConfigureHttpClient(static (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<TOptions>>().Value;

                client.BaseAddress = options.BaseAddress;

                // The resilience handler owns the timeout budget. Leaving HttpClient's
                // own 100 second default in place would silently cap TotalRequestTimeout.
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

        builder.AddStandardResilienceHandler(resilience =>
        {
            resilience.AttemptTimeout.Timeout = AttemptTimeout;
            resilience.TotalRequestTimeout.Timeout = TotalRequestTimeout;
            resilience.CircuitBreaker.SamplingDuration = CircuitBreakerSamplingDuration;

            configureResilience?.Invoke(resilience);
        });

        return builder;
    }
}
