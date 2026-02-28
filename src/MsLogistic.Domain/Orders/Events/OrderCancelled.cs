using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Orders.Events;

public record OrderCancelled(
    Guid OrderId,
    Guid? RouteId,
    DateTime CancelledAt
) : DomainEvent;
