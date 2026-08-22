using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Models.Nhtsa;

internal sealed class NhtsaVehicleType
{
    [JsonPropertyName("VehicleTypeId")]
    public int VehicleTypeId { get; init; }

    [JsonPropertyName("VehicleTypeName")]
    public string? VehicleTypeName { get; init; }
}
