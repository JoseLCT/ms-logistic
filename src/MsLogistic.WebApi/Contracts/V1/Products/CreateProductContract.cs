using System.Text.Json.Serialization;

namespace MsLogistic.WebApi.Contracts.V1.Products;

public record CreateProductContract {
    [property: JsonPropertyName("name")]
    public required string Name { get; init; }

    [property: JsonPropertyName("description")]
    public string? Description { get; init; }
}
