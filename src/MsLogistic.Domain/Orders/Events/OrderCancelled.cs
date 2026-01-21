using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Orders.Events;

public record OrderCancelled(
    Guid OrderId,
    DateTime CancelledAt
) : DomainEvent;