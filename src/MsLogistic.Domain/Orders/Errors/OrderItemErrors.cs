using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Orders.Errors;

public static class OrderItemErrors
{
    public static Error QuantityMustBeGreaterThanZero =>
        Error.Validation(
            code: "OrderItem.Quantity.GreaterThanZero",
            message: "Quantity must be greater than zero."
        );
}