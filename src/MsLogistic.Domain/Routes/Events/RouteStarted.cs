using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Routes.Events;

public record RouteStarted(
    Guid RouteId,
    DateTime StartedAt
) : DomainEvent;