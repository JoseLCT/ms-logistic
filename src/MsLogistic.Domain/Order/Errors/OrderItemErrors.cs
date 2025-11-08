using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Order.Errors;

public static class OrderItemErrors
{
    public static Error QuantityMustBeGreaterThanZero =>
        new Error(
            code: "quantity_must_be_greater_than_zero",
            structuredMessage: "Quantity must be greater than zero.",
            type: ErrorType.Validation
        );
}