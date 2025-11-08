using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Order.Events;

public record OrderCancelled : DomainEvent
{
    public Guid OrderId { get; init; }

    private OrderCancelled()
    {
    }

    public OrderCancelled(Guid orderId)
    {
        OrderId = orderId;
    }
}