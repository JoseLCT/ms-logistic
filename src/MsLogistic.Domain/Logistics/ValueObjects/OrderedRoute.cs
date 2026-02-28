namespace MsLogistic.Domain.Logistics.ValueObjects;

public record OrderedRoute(
    IReadOnlyList<OrderedWaypoint> Waypoints
);
