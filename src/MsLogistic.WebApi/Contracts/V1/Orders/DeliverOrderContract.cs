using System.Text.Json.Serialization;
using MsLogistic.WebApi.Contracts.Shared;

namespace MsLogistic.WebApi.Contracts.V1.Orders;

public record DeliverOrderContract {
    [property: JsonPropertyName("driver_id")]
    public required Guid DriverId { get; init; }

    [property: JsonPropertyName("location")]
    public required CoordinateContract Location { get; init; }

    [property: JsonPropertyName("comments")]
    public string? Comments { get; init; }
}
