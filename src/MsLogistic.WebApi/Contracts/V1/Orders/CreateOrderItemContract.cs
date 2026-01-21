using System.Text.Json.Serialization;

namespace MsLogistic.WebApi.Contracts.V1.Orders;

public record CreateOrderItemContract
{
    [property: JsonPropertyName("product_id")]
    public required Guid ProductId { get; init; }

    [property: JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
}