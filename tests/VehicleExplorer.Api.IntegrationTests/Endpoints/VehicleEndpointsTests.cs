using System.Net;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;
using Xunit;

namespace VehicleExplorer.Api.IntegrationTests.Endpoints;

public sealed class VehicleEndpointsTests(VehicleApiFactory factory)
    : IClassFixture<VehicleApiFactory>
{
    [Fact]
    public async Task GetMakes_WhenTheCatalogueHasMakes_ReturnsThemAsCamelCasedJson()
    {
        factory.Nhtsa.Makes = () => [new MakeDto(448, "HONDA")];

        var response = await factory.CreateClient().GetAsync("/api/vehicles/makes", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The React client depends on this casing, so it is asserted rather than assumed.
        Assert.Contains("\"id\":448", json);
        Assert.Contains("\"name\":\"HONDA\"", json);
    }

    [Fact]
    public async Task GetVehicleTypes_WhenTheMakeHasTypes_ReturnsThem()
    {
        factory.Nhtsa.VehicleTypes = _ => [new VehicleTypeDto(2, "Passenger Car")];

        var response = await factory.CreateClient()
            .GetAsync("/api/vehicles/makes/448/vehicle-types", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"name\":\"Passenger Car\"", json);
    }

    [Fact]
    public async Task GetVehicleTypes_WhenTheMakeIsUnknown_ReturnsAnEmptyArrayRatherThanNotFound()
    {
        factory.Nhtsa.VehicleTypes = _ => [];

        var response = await factory.CreateClient()
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
        var response = await factory.CreateClient()
            .GetAsync($"/api/vehicles/makes/{makeId}/vehicle-types", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetModels_WhenGivenAMakeAndYear_ReturnsTheModels()
    {
        factory.Nhtsa.Models = (_, _, _) => [new ModelDto(1861, "Accord")];

        var response = await factory.CreateClient()
            .GetAsync("/api/vehicles/makes/474/models?year=2015", TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"name\":\"Accord\"", json);
    }

    [Fact]
    public async Task GetModels_WhenAVehicleTypeIsSupplied_PassesItThroughToTheCatalogue()
    {
        string? received = null;

        factory.Nhtsa.Models = (_, _, vehicleType) =>
        {
            received = vehicleType;
            return [];
        };

        await factory.CreateClient().GetAsync(
            "/api/vehicles/makes/474/models?year=2015&vehicleType=car",
            TestContext.Current.CancellationToken);

        Assert.Equal("car", received);

        factory.Nhtsa.Models = (_, _, _) => [];
    }

    [Fact]
    public async Task GetModels_WhenTheYearIsMissing_ReturnsBadRequest()
    {
        // year has no default, so model binding rejects the request before the handler runs.
        var response = await factory.CreateClient()
            .GetAsync("/api/vehicles/makes/474/models", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMakes_WhenTheUpstreamIsUnavailable_Returns503WithProblemDetails()
    {
        factory.Nhtsa.Makes = () => throw new NhtsaUnavailableException("vPIC is down.");

        var response = await factory.CreateClient().GetAsync("/api/vehicles/makes", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Vehicle data is temporarily unavailable", body);

        factory.Nhtsa.Makes = () => [];
    }
}
