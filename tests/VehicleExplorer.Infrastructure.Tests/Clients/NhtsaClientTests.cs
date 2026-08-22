using System.Net;
using Moq;
using Refit;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Infrastructure.Clients;
using VehicleExplorer.Infrastructure.Models.Nhtsa;
using Xunit;

namespace VehicleExplorer.Infrastructure.Tests.Clients;

/// <summary>
/// Covers the adapter's two responsibilities: translating vPIC's wire format into this
/// application's types, and collapsing every kind of upstream failure into one exception.
/// </summary>
public sealed class NhtsaClientTests
{
    private readonly Mock<INhtsaApi> _api = new(MockBehavior.Strict);

    private NhtsaClient CreateClient() => new(_api.Object);

    [Fact]
    public async Task GetMakesAsync_WhenVpicReturnsMakes_MapsThemToApplicationTypes()
    {
        SetupMakes(Success(new NhtsaResponse<NhtsaMake>
        {
            Results =
            [
                new NhtsaMake { MakeId = 448, MakeName = "HONDA" },
                new NhtsaMake { MakeId = 474, MakeName = "MERCEDES-BENZ" }
            ]
        }));

        var makes = await CreateClient().GetMakesAsync(CancellationToken.None);

        Assert.Collection(
            makes,
            make => Assert.Equal((448, "HONDA"), (make.Id, make.Name)),
            make => Assert.Equal((474, "MERCEDES-BENZ"), (make.Id, make.Name)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetMakesAsync_WhenAMakeHasNoUsableName_ExcludesItFromTheResult(string? name)
    {
        SetupMakes(Success(new NhtsaResponse<NhtsaMake>
        {
            Results =
            [
                new NhtsaMake { MakeId = 1, MakeName = name },
                new NhtsaMake { MakeId = 448, MakeName = "HONDA" }
            ]
        }));

        var makes = await CreateClient().GetMakesAsync(CancellationToken.None);

        var make = Assert.Single(makes);
        Assert.Equal(448, make.Id);
    }

    [Fact]
    public async Task GetMakesAsync_WhenVpicReturnsNoRows_ReturnsAnEmptyList()
    {
        SetupMakes(Success(new NhtsaResponse<NhtsaMake>()));

        var makes = await CreateClient().GetMakesAsync(CancellationToken.None);

        Assert.Empty(makes);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task GetMakesAsync_WhenVpicReturnsAnUnsuccessfulStatus_ThrowsNhtsaUnavailable(
        HttpStatusCode statusCode)
    {
        SetupMakes(Failure<NhtsaResponse<NhtsaMake>>(statusCode));

        var exception = await Assert.ThrowsAsync<NhtsaUnavailableException>(
            () => CreateClient().GetMakesAsync(CancellationToken.None));

        Assert.Contains(((int)statusCode).ToString(), exception.Message);
    }

    [Fact]
    public async Task GetMakesAsync_WhenVpicReturnsAnEmptyBody_ThrowsNhtsaUnavailable()
    {
        SetupMakes(Success<NhtsaResponse<NhtsaMake>>(content: null));

        await Assert.ThrowsAsync<NhtsaUnavailableException>(
            () => CreateClient().GetMakesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetVehicleTypesAsync_WhenVpicReturnsTypes_MapsThemToApplicationTypes()
    {
        SetupVehicleTypes(448, Success(new NhtsaResponse<NhtsaVehicleType>
        {
            Results =
            [
                new NhtsaVehicleType { VehicleTypeId = 2, VehicleTypeName = "Passenger Car" },
                new NhtsaVehicleType { VehicleTypeId = 3, VehicleTypeName = "Truck" }
            ]
        }));

        var types = await CreateClient().GetVehicleTypesAsync(448, CancellationToken.None);

        Assert.Collection(
            types,
            type => Assert.Equal((2, "Passenger Car"), (type.Id, type.Name)),
            type => Assert.Equal((3, "Truck"), (type.Id, type.Name)));
    }

    [Fact]
    public async Task GetVehicleTypesAsync_WhenTheMakeIsUnknownToVpic_ReturnsAnEmptyList()
    {
        // vPIC answers an unknown make with a successful, empty envelope rather than a
        // 404, so "no such make" and "make with no recorded types" are the same result.
        SetupVehicleTypes(999_999, Success(new NhtsaResponse<NhtsaVehicleType>()));

        var types = await CreateClient().GetVehicleTypesAsync(999_999, CancellationToken.None);

        Assert.Empty(types);
    }

    [Fact]
    public async Task GetVehicleTypesAsync_WhenCalled_PassesTheRequestedMakeUpstream()
    {
        SetupVehicleTypes(448, Success(new NhtsaResponse<NhtsaVehicleType>()));

        await CreateClient().GetVehicleTypesAsync(448, CancellationToken.None);

        _api.Verify(
            api => api.GetVehicleTypesForMakeAsync(448, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupMakes(IApiResponse<NhtsaResponse<NhtsaMake>> response) =>
        _api.Setup(api => api.GetAllMakesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    private void SetupVehicleTypes(int makeId, IApiResponse<NhtsaResponse<NhtsaVehicleType>> response) =>
        _api.Setup(api => api.GetVehicleTypesForMakeAsync(makeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    private static IApiResponse<T> Success<T>(T? content)
    {
        var response = new Mock<IApiResponse<T>>();
        response.SetupGet(r => r.IsSuccessStatusCode).Returns(true);
        response.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.OK);
        response.SetupGet(r => r.Content).Returns(content);
        response.Setup(r => r.Dispose());

        return response.Object;
    }

    private static IApiResponse<T> Failure<T>(HttpStatusCode statusCode)
    {
        var response = new Mock<IApiResponse<T>>();
        response.SetupGet(r => r.IsSuccessStatusCode).Returns(false);
        response.SetupGet(r => r.StatusCode).Returns(statusCode);
        response.SetupGet(r => r.Error).Returns((ApiException?)null);
        response.Setup(r => r.Dispose());

        return response.Object;
    }
}
