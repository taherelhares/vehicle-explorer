using System.Net;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;
using Xunit;

namespace VehicleExplorer.Api.IntegrationTests.Endpoints;

public sealed class VehicleEndpointsTests : IClassFixture<VehicleApiFactory>
{
    private readonly VehicleApiFactory _factory;

    public VehicleEndpointsTests(VehicleApiFactory factory)
    {
        _factory = factory;

        // xUnit builds a new instance of this class for every test, so this runs before
        // each one. The host — and with it the catalogue cache — is shared, so it is
        // returned to a known state here rather than cleaned up afterwards.
        _factory.Reset();
    }

    [Fact]
    public async Task GetMakes_WhenTheCatalogueHasMakes_ReturnsThemAsCamelCasedJson()
    {
        _factory.Nhtsa.Makes = () => [new MakeDto(448, "HONDA")];

        var response = await _factory.CreateClient().GetAsync("/api/vehicles/makes", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The React client depends on this casing, so it is asserted rather than assumed.
        Assert.Contains("\"id\":448", json);
        Assert.Contains("\"name\":\"HONDA\"", json);
    }

    [Fact]
    public async Task GetMakes_WhenRequestedTwice_ReachesTheUpstreamOnce()
    {
        var calls = 0;

        _factory.Nhtsa.Makes = () =>
        {
            calls++;
            return [new MakeDto(448, "HONDA")];
        };

        var client = _factory.CreateClient();
        await client.GetAsync("/api/vehicles/makes", TestContext.Current.CancellationToken);
        var response = await client.GetAsync("/api/vehicles/makes", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetVehicleTypes_WhenTheMakeHasTypes_ReturnsThem()
    {
        _factory.Nhtsa.VehicleTypes = _ => [new VehicleTypeDto(2, "Passenger Car")];

        var response = await _factory.CreateClient()
            .GetAsync("/api/vehicles/makes/448/vehicle-types", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"name\":\"Passenger Car\"", json);
    }

    [Fact]
    public async Task GetVehicleTypes_WhenTheMakeIsUnknown_ReturnsAnEmptyArrayRatherThanNotFound()
    {
        _factory.Nhtsa.VehicleTypes = _ => [];

        var response = await _factory.CreateClient()
            .GetAsync("/api/vehicles/makes/999999/vehicle-types", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.5")]
    public async Task GetVehicleTypes_WhenTheMakeIdIsNotAPositiveInteger_ReturnsNotFound(
        string makeId)
    {
        // The route constraint rejects these before any handler runs.
        var response = await _factory.CreateClient()
            .GetAsync($"/api/vehicles/makes/{makeId}/vehicle-types", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetModels_WhenGivenAMakeAndYear_ReturnsTheModels()
    {
        _factory.Nhtsa.Models = (_, _, _) => [new ModelDto(1861, "Accord")];

        var response = await _factory.CreateClient()
            .GetAsync("/api/vehicles/makes/474/models?year=2015", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"name\":\"Accord\"", json);
    }

    [Fact]
    public async Task GetModels_WhenAVehicleTypeIsSupplied_PassesItThroughToTheCatalogue()
    {
        string? received = null;

        _factory.Nhtsa.Models = (_, _, vehicleType) =>
        {
            received = vehicleType;
            return [];
        };

        await _factory.CreateClient().GetAsync(
            "/api/vehicles/makes/474/models?year=2015&vehicleType=car",
            TestContext.Current.CancellationToken);

        Assert.Equal("car", received);
    }

    [Fact]
    public async Task GetModels_WhenTheYearIsMissing_ReturnsBadRequest()
    {
        // year has no default, so model binding rejects the request before the handler runs.
        var response = await _factory.CreateClient()
            .GetAsync("/api/vehicles/makes/474/models", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMakes_WhenTheUpstreamIsUnavailable_Returns503WithProblemDetails()
    {
        _factory.Nhtsa.Makes = () => throw new NhtsaUnavailableException("vPIC is down.");

        var response = await _factory.CreateClient().GetAsync("/api/vehicles/makes", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Vehicle data is temporarily unavailable", body);
    }
}
