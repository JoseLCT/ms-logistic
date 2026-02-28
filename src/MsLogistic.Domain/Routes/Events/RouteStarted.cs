using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Routes.Events;

public record RouteStarted(
    Guid RouteId,
    Guid BatchId,
    DateTime StartedAt
) : DomainEvent;
