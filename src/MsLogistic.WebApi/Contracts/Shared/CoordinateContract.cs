using System.Text.Json.Serialization;

namespace MsLogistic.WebApi.Contracts.Shared;

public record CoordinateContract
{
    [property: JsonPropertyName("latitude")]
    public required double Latitude { get; init; }

    [property: JsonPropertyName("longitude")]
    public required double Longitude { get; init; }
}