using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Infrastructure;
using Xunit;

namespace VehicleExplorer.Infrastructure.IntegrationTests.Clients;

/// <summary>
/// Exercises the whole infrastructure registration against a stubbed socket: configuration
/// binding, the Refit client, the resilience pipeline, JSON deserialisation and the
/// adapter's mapping. Unit tests hand-build contract objects and so cannot prove that the
/// route templates or the property name attributes are right; these can.
/// </summary>
public sealed class NhtsaClientTransportTests
{
    private const string BaseAddress = "https://vpic.nhtsa.dot.gov/";

    [Fact]
    public async Task GetMakesAsync_WhenVpicReturnsItsUsualEnvelope_DeserialisesAndMapsIt()
    {
        const string body = """
        {
          "Count": 2,
          "Message": "Response returned successfully",
          "SearchCriteria": null,
          "Results": [
            { "Make_ID": 448, "Make_Name": "HONDA" },
            { "Make_ID": 474, "Make_Name": "MERCEDES-BENZ" }
          ]
        }
        """;

        var (client, handler) = CreateClient(HttpStatusCode.OK, body);

        var makes = await client.GetMakesAsync(TestContext.Current.CancellationToken);

        Assert.Collection(
            makes,
            make => Assert.Equal((448, "HONDA"), (make.Id, make.Name)),
            make => Assert.Equal((474, "MERCEDES-BENZ"), (make.Id, make.Name)));

        Assert.Equal(
            "/api/vehicles/getallmakes?format=json",
            handler.LastRequestUri?.PathAndQuery);
    }

    [Fact]
    public async Task GetVehicleTypesAsync_WhenGivenAMakeId_RequestsThatMakeAndMapsTheResult()
    {
        const string body = """
        {
          "Count": 1,
          "Message": "Response returned successfully",
          "SearchCriteria": "Make ID: 448",
          "Results": [
            { "VehicleTypeId": 2, "VehicleTypeName": "Passenger Car" }
          ]
        }
        """;

        var (client, handler) = CreateClient(HttpStatusCode.OK, body);

        var types = await client.GetVehicleTypesAsync(448, TestContext.Current.CancellationToken);

        var type = Assert.Single(types);
        Assert.Equal((2, "Passenger Car"), (type.Id, type.Name));

        Assert.Equal(
            "/api/vehicles/GetVehicleTypesForMakeId/448?format=json",
            handler.LastRequestUri?.PathAndQuery);
    }

    [Fact]
    public async Task GetMakesAsync_WhenVpicReturnsAnErrorStatus_ThrowsNhtsaUnavailable()
    {
        var (client, _) = CreateClient(HttpStatusCode.InternalServerError, "{}");

        await Assert.ThrowsAsync<NhtsaUnavailableException>(
            () => client.GetMakesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddInfrastructure_WhenTheBaseAddressIsMissing_FailsOptionsValidation()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(new ConfigurationBuilder().Build());

        await using var provider = services.BuildServiceProvider();

        // Validation runs the first time the options are materialised, which happens while
        // the Refit client's HttpClient is being configured — so resolving the adapter is
        // already enough to fail. Both steps sit inside the assertion so the test does not
        // depend on exactly which one trips first.
        await Assert.ThrowsAsync<OptionsValidationException>(async () =>
        {
            var client = provider.GetRequiredService<INhtsaClient>();
            await client.GetMakesAsync(TestContext.Current.CancellationToken);
        });
    }

    private static (INhtsaClient Client, StubHttpMessageHandler Handler) CreateClient(
        HttpStatusCode statusCode,
        string body)
    {
        var handler = new StubHttpMessageHandler(statusCode, body);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Nhtsa:BaseAddress"] = BaseAddress
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => handler));

        var provider = services.BuildServiceProvider();

        return (provider.GetRequiredService<INhtsaClient>(), handler);
    }
}
