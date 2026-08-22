using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Models.Nhtsa;

/// <summary>
/// Every vPIC endpoint returns the same envelope, so one generic covers all of them.
/// </summary>
internal sealed class NhtsaResponse<T>
{
    [JsonPropertyName("Count")]
    public int Count { get; init; }

    [JsonPropertyName("Message")]
    public string? Message { get; init; }

    [JsonPropertyName("Results")]
    public IReadOnlyList<T> Results { get; init; } = [];
}
