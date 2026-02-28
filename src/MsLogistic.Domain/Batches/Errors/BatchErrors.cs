using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Batches.Errors;

public static class BatchErrors {
    public static Error TotalOrdersCannotBeNegative =>
        Error.Validation(
            code: "Batch.TotalOrders.Negative",
            message: "Total orders cannot be negative."
        );

    public static Error CannotAddOrdersToClosedBatch =>
        Error.Validation(
            code: "Batch.AddOrders.ClosedBatch",
            message: "Cannot add orders to a closed batch."
        );

    public static Error CannotAddNonPositiveQuantityOfOrders =>
        Error.Validation(
            code: "Batch.AddOrders.NonPositiveQuantity",
            message: "Cannot add a non-positive quantity of orders."
        );

    public static Error CannotCloseAlreadyClosedBatch =>
        Error.Validation(
            code: "Batch.Close.AlreadyClosed",
            message: "Cannot close an already closed batch."
        );
}
