using System.Text.Json.Serialization;

namespace MsLogistic.WebApi.Contracts.V1.Drivers;

public record UpdateDriverContract
{
    [property: JsonPropertyName("full_name")]
    public required string FullName { get; init; }
    
    [property: JsonPropertyName("is_active")]
    public bool IsActive { get; init; }
}