using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Errors;

namespace MsLogistic.Domain.Orders.Entities;

public class OrderItem : Entity {
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() {
    }

    private OrderItem(
        Guid orderId,
        Guid productId,
        int quantity
    ) : base(Guid.NewGuid()) {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
    }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        int quantity
    ) {
        ValidateQuantity(quantity);
        return new OrderItem(
            orderId,
            productId,
            quantity
        );
    }

    public void IncreaseQuantity(int quantity) {
        ValidateQuantity(quantity);
        Quantity += quantity;
        MarkAsUpdated();
    }

    private static void ValidateQuantity(int quantity) {
        if (quantity <= 0) {
            throw new DomainException(OrderItemErrors.QuantityMustBeGreaterThanZero);
        }
    }
}
