using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Models.Nhtsa;

internal sealed class NhtsaModel
{
    [JsonPropertyName("Make_ID")]
    public int MakeId { get; init; }

    [JsonPropertyName("Make_Name")]
    public string? MakeName { get; init; }

    [JsonPropertyName("Model_ID")]
    public int ModelId { get; init; }

    [JsonPropertyName("Model_Name")]
    public string? ModelName { get; init; }

    // Only populated on the vehicleType-filtered route.
    [JsonPropertyName("VehicleTypeId")]
    public int? VehicleTypeId { get; init; }

    [JsonPropertyName("VehicleTypeName")]
    public string? VehicleTypeName { get; init; }
}
