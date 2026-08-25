using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using VehicleExplorer.Application.Abstractions;

namespace VehicleExplorer.Api.IntegrationTests;

/// <summary>
/// Boots the real application in memory. Everything is genuine — dependency injection,
/// the middleware pipeline, routing, options validation and JSON serialisation — except
/// the vPIC adapter, which is stubbed so the tests never touch the network.
/// </summary>
public sealed class VehicleApiFactory : WebApplicationFactory<Program>
{
    public StubNhtsaClient Nhtsa { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Production keeps HTTPS redirection out of the pipeline, so the test client's
        // plain HTTP requests reach the endpoints instead of being redirected.
        builder.UseEnvironment("Production");

        // NhtsaOptions is validated on start; supply the one required value explicitly
        // rather than depending on which appsettings file the host happens to find.
        builder.UseSetting("Nhtsa:BaseAddress", "https://vpic.nhtsa.dot.gov/");

        builder.ConfigureTestServices(services =>
            services.AddScoped<INhtsaClient>(_ => Nhtsa));
    }

    /// <summary>
    /// Returns the host to a known state. The catalogue cache is a singleton and this
    /// factory is shared by every test in a class, so without this an answer cached by
    /// one test would be served to the next — and a test that expects the upstream to be
    /// called would never reach it.
    /// </summary>
    public void Reset()
    {
        Nhtsa.Reset();

        if (Services.GetRequiredService<IMemoryCache>() is MemoryCache cache)
        {
            cache.Clear();
        }
    }
}
