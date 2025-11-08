using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Order.Events;

public record OrderCompleted : DomainEvent
{
    public Guid OrderId { get; init; }
    public DateTime CompletedAt { get; init; }

    private OrderCompleted()
    {
    }

    public OrderCompleted(Guid orderId, DateTime completedAt)
    {
        OrderId = orderId;
        CompletedAt = completedAt;
    }
}