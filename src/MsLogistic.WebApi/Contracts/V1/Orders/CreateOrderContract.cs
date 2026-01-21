using System.Text.Json.Serialization;
using MsLogistic.WebApi.Contracts.Shared;

namespace MsLogistic.WebApi.Contracts.V1.Orders;

public record CreateOrderContract
{
    [property: JsonPropertyName("customer_id")]
    public Guid CustomerId { get; init; }
    
    [property: JsonPropertyName("scheduled_delivery_date")]
    public DateTime ScheduledDeliveryDate { get; init; }
    
    [property: JsonPropertyName("delivery_address")]
    public required string DeliveryAddress { get; init; }
    
    [property: JsonPropertyName("delivery_location")]
    public required CoordinateContract DeliveryLocation { get; init; }
    
    [property: JsonPropertyName("items")]
    public required IReadOnlyList<CreateOrderItemContract> Items { get; init; }
}