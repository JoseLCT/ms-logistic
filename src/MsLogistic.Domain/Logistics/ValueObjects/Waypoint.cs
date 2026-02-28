using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Logistics.ValueObjects;

public record Waypoint(
    Guid Id,
    GeoPointValue Location
);
