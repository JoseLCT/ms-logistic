using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Route.Events;

public record RouteStarted : DomainEvent
{
    public Guid RouteId { get; init; }
    public DateTime StartedAt { get; init; }

    public RouteStarted(Guid routeId, DateTime startedAt)
    {
        RouteId = routeId;
        StartedAt = startedAt;
    }

    private RouteStarted()
    {
    }
}