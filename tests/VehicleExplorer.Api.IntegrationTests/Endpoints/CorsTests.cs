using VehicleExplorer.Application.Models;
using Xunit;

namespace VehicleExplorer.Api.IntegrationTests.Endpoints;

/// <summary>
/// A CORS policy is only worth having if it refuses as well as allows, so both are
/// asserted against the real pipeline.
/// </summary>
public sealed class CorsTests : IClassFixture<VehicleApiFactory>
{
    private const string CorsHeader = "Access-Control-Allow-Origin";

    private readonly VehicleApiFactory _factory;

    public CorsTests(VehicleApiFactory factory)
    {
        _factory = factory;
        _factory.Reset();
    }

    [Fact]
    public async Task GetMakes_WhenTheOriginIsAllowed_EchoesItBackToTheBrowser()
    {
        _factory.Nhtsa.Makes = () => [new MakeDto(448, "HONDA")];

        var response = await SendWithOriginAsync(VehicleApiFactory.AllowedOrigin);

        Assert.True(response.Headers.TryGetValues(CorsHeader, out var values));
        Assert.Equal(VehicleApiFactory.AllowedOrigin, Assert.Single(values!));
    }

    [Fact]
    public async Task GetMakes_WhenTheOriginIsNotAllowed_SendsNoCorsHeader()
    {
        _factory.Nhtsa.Makes = () => [new MakeDto(448, "HONDA")];

        var response = await SendWithOriginAsync("https://not-our-client.test");

        // Without the header the browser discards the response, which is the point: an
        // origin nobody configured is not permitted just because the API answered.
        Assert.False(response.Headers.Contains(CorsHeader));
    }

    private async Task<HttpResponseMessage> SendWithOriginAsync(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/vehicles/makes");
        request.Headers.Add("Origin", origin);

        return await _factory.CreateClient()
            .SendAsync(request, TestContext.Current.CancellationToken);
    }
}
