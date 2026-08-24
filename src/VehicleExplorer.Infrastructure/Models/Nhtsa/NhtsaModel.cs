using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Models.Nhtsa;

internal sealed class NhtsaModel
{
    [JsonPropertyName("Model_ID")]
    public int ModelId { get; init; }

    [JsonPropertyName("Model_Name")]
    public string? ModelName { get; init; }
}
