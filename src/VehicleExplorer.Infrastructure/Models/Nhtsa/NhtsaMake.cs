using System.Text.Json.Serialization;

namespace VehicleExplorer.Infrastructure.Models.Nhtsa;

internal sealed class NhtsaMake
{
    [JsonPropertyName("Make_ID")]
    public int MakeId { get; init; }

    [JsonPropertyName("Make_Name")]
    public string? MakeName { get; init; }
}
