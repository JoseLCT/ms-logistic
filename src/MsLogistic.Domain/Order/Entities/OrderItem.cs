using MsLogistic.Core.Abstractions;

namespace MsLogistic.Domain.Order.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem()
    {
    }

    public OrderItem(
        Guid orderId,
        Guid productId,
        int quantity) : base(Guid.NewGuid())
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        CreatedAt = DateTime.UtcNow;
    }
}