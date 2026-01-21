using System.Text.Json.Serialization;
using MsLogistic.WebApi.Contracts.Shared;

namespace MsLogistic.WebApi.Contracts.V1.DeliveryZones;

public record UpdateDeliveryZoneContract
{
    [property: JsonPropertyName("driver_id")]
    public Guid? DriverId { get; init; }

    [property: JsonPropertyName("code")]
    public required string Code { get; init; }

    [property: JsonPropertyName("name")] public required string Name { get; init; }

    [property: JsonPropertyName("boundaries")]
    public required IReadOnlyList<CoordinateContract> Boundaries { get; init; }
}