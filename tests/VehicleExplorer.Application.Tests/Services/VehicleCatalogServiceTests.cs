using Microsoft.Extensions.Caching.Memory;
using Moq;
using VehicleExplorer.Application.Abstractions;
using VehicleExplorer.Application.Models;
using VehicleExplorer.Application.Services;
using Xunit;

namespace VehicleExplorer.Application.Tests.Services;

/// <summary>
/// The service owns one decision: how long an answer from the catalogue stays usable.
/// These tests are about that decision, so the port is a strict mock and no HTTP is
/// involved anywhere.
/// </summary>
public sealed class VehicleCatalogServiceTests
{
    private readonly Mock<INhtsaClient> _client = new(MockBehavior.Strict);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private VehicleCatalogService CreateService() => new(_client.Object, _cache);

    [Fact]
    public async Task GetMakesAsync_WhenCalledTwice_ReachesThePortOnce()
    {
        _client.Setup(c => c.GetMakesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new MakeDto(474, "HONDA")]);

        var service = CreateService();
        await service.GetMakesAsync(TestContext.Current.CancellationToken);
        var second = await service.GetMakesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(474, Assert.Single(second).Id);
        _client.Verify(c => c.GetMakesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMakesAsync_WhenTheCatalogueComesBackEmpty_DoesNotCacheIt()
    {
        var responses = new Queue<IReadOnlyList<MakeDto>>([[], [new MakeDto(474, "HONDA")]]);

        _client.Setup(c => c.GetMakesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(responses.Dequeue);

        var service = CreateService();

        Assert.Empty(await service.GetMakesAsync(TestContext.Current.CancellationToken));

        // A momentary upstream glitch must not pin an empty catalogue in place for a day.
        Assert.Single(await service.GetMakesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetVehicleTypesAsync_WhenDifferentMakesAreRequested_CachesThemSeparately()
    {
        _client.Setup(c => c.GetVehicleTypesAsync(448, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new VehicleTypeDto(2, "Passenger Car")]);
        _client.Setup(c => c.GetVehicleTypesAsync(474, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new VehicleTypeDto(10, "Motorcycle")]);

        var service = CreateService();

        var toyota = await service.GetVehicleTypesAsync(448, TestContext.Current.CancellationToken);
        var honda = await service.GetVehicleTypesAsync(474, TestContext.Current.CancellationToken);
        await service.GetVehicleTypesAsync(448, TestContext.Current.CancellationToken);

        Assert.Equal("Passenger Car", Assert.Single(toyota).Name);
        Assert.Equal("Motorcycle", Assert.Single(honda).Name);
        _client.Verify(c => c.GetVehicleTypesAsync(448, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModelsAsync_WhenTheYearDiffers_TreatsItAsASeparateQuery()
    {
        _client.Setup(c => c.GetModelsAsync(474, 2015, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ModelDto(1861, "Accord")]);
        _client.Setup(c => c.GetModelsAsync(474, 2016, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ModelDto(1862, "Civic")]);

        var service = CreateService();

        var older = await service.GetModelsAsync(474, 2015, null, TestContext.Current.CancellationToken);
        var newer = await service.GetModelsAsync(474, 2016, null, TestContext.Current.CancellationToken);

        Assert.Equal("Accord", Assert.Single(older).Name);
        Assert.Equal("Civic", Assert.Single(newer).Name);
    }

    [Fact]
    public async Task GetModelsAsync_WhenTheVehicleTypeDiffers_TreatsItAsASeparateQuery()
    {
        _client.Setup(c => c.GetModelsAsync(474, 2015, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ModelDto(3235, "CB1100")]);
        _client.Setup(c => c.GetModelsAsync(474, 2015, "car", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ModelDto(1861, "Accord")]);

        var service = CreateService();

        var all = await service.GetModelsAsync(474, 2015, null, TestContext.Current.CancellationToken);
        var cars = await service.GetModelsAsync(474, 2015, "car", TestContext.Current.CancellationToken);

        // The filter is part of the key, so a filtered result never masks an unfiltered one.
        Assert.Equal("CB1100", Assert.Single(all).Name);
        Assert.Equal("Accord", Assert.Single(cars).Name);
    }

    [Fact]
    public async Task GetModelsAsync_WhenTheVehicleTypeDiffersOnlyByCase_ReusesTheCachedResult()
    {
        _client.Setup(c => c.GetModelsAsync(474, 2015, "Car", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ModelDto(1861, "Accord")]);

        var service = CreateService();
        await service.GetModelsAsync(474, 2015, "Car", TestContext.Current.CancellationToken);
        await service.GetModelsAsync(474, 2015, " car ", TestContext.Current.CancellationToken);

        _client.Verify(
            c => c.GetModelsAsync(474, 2015, It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
