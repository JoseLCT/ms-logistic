using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Orders.Events;

public record OrderDelivered(
    Guid OrderId,
    Guid? RouteId,
    DateTime DeliveredAt
) : DomainEvent;
