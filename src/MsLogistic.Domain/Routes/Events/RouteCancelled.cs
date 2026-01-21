using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Routes.Events;

public record RouteCancelled(
    Guid RouteId,
    DateTime CancelledAt
) : DomainEvent;