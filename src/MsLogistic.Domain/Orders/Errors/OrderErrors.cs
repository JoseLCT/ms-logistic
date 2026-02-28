using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.Domain.Orders.Errors;

public static class OrderErrors {
    public static Error DeliverySequenceMustBeGreaterThanZero =>
        Error.Validation(
            code: "Order.DeliverySequence.MustBeGreaterThanZero",
            message: "Delivery sequence must be greater than zero."
        );

    public static Error ScheduledDeliveryDateCannotBeInThePast =>
        Error.Validation(
            code: "Order.ScheduledDeliveryDate.CannotBeInThePast",
            message: "Schedule date cannot be in the past."
        );

    public static Error CannotModifyOrderThatIsNotPending =>
        Error.Validation(
            code: "Order.CannotModifyOrderThatIsNotPending",
            message: "Cannot modify an order that is not pending."
        );

    public static Error DeliveryAddressIsRequired =>
        Error.Validation(
            code: "Order.DeliveryAddress.Required",
            message: "Delivery address is required."
        );

    public static Error DeliveryAddressTooLong(int maxLength) =>
        Error.Validation(
            code: "Order.DeliveryAddress.TooLong",
            message: $"Delivery address cannot exceed {maxLength} characters."
        );

    public static Error CannotAssignOrderWithoutItems =>
        Error.Validation(
            code: "Order.CannotAssignOrderWithoutItems",
            message: "Cannot assign an order without items."
        );

    public static Error CannotAssignOrderThatIsNotPending =>
        Error.Validation(
            code: "Order.CannotAssignOrderThatIsNotPending",
            message: "Cannot assign an order that is not pending."
        );

    public static Error IncidentAlreadyReported =>
        Error.Validation(
            code: "Order.IncidentAlreadyReported",
            message: "An incident has already been reported for this order."
        );

    public static Error ItemsAreRequired =>
        Error.Validation(
            code: "Order.Items.Required",
            message: "Order items are required."
        );

    public static Error ProductsNotFound =>
        Error.NotFound(
            code: "Order.Products.NotFound",
            message: "One or more products were not found."
        );

    public static Error CannotReportIncidentForOrderWithStatus(OrderStatusEnum status) =>
        Error.Validation(
            code: "Order.CannotReportIncidentForOrderWithStatus",
            message: $"Cannot report incident for order with status {status}."
        );

    public static Error CannotDeliverOrderWithStatus(OrderStatusEnum status) =>
        Error.Validation(
            code: "Order.CannotDeliverOrderWithStatus",
            message: $"Cannot deliver order with status {status}."
        );

    public static Error CannotChangeStatusFromTo(
        OrderStatusEnum from,
        OrderStatusEnum to
    ) =>
        Error.Validation(
            code: "Order.CannotChangeStatusFromTo",
            message: $"Cannot change status from {from} to {to}."
        );
}
