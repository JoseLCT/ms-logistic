using System.Text.Json.Serialization;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.WebApi.Contracts.V1.Orders;

public record ReportIncidentContract {
    [property: JsonPropertyName("driver_id")]
    public required Guid DriverId { get; init; }

    [property: JsonPropertyName("incident_type")]
    public required OrderIncidentTypeEnum IncidentType { get; init; }

    [property: JsonPropertyName("description")]
    public required string Description { get; init; }
}
