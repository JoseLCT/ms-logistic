using System.Text.Json.Serialization;

namespace MsLogistic.WebApi.Contracts.V1.Drivers;

public record CreateDriverContract
{
    [property: JsonPropertyName("full_name")]
    public required string FullName { get; init; }
}