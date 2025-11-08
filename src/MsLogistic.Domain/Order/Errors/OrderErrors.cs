using MsLogistic.Core.Results;
using MsLogistic.Domain.Order.Types;

namespace MsLogistic.Domain.Order.Errors;

public static class OrderErrors
{
    public static Error DeliverySequenceMustBeGreaterThanZero =>
        new Error(
            code: "delivery_sequence_must_be_greater_than_zero",
            structuredMessage: "Delivery sequence must be greater than zero.",
            type: ErrorType.Validation
        );

    public static Error ScheduledDeliveryDateCannotBeInThePast =>
        new Error(
            code: "schedule_date_cannot_be_in_the_past",
            structuredMessage: "Schedule date cannot be in the past.",
            type: ErrorType.Validation
        );

    public static Error CannotModifyOrderThatIsNotPending =>
        new Error(
            code: "cannot_modify_order_that_is_not_pending",
            structuredMessage: "Cannot modify an order that is not pending.",
            type: ErrorType.Validation
        );

    public static Error DeliveryAddressIsRequired =>
        new Error(
            code: "delivery_address_is_required",
            structuredMessage: "Delivery address is required.",
            type: ErrorType.Validation
        );

    public static Error CannotAssignOrderWithNoItems =>
        new Error(
            code: "cannot_assign_order_with_no_items",
            structuredMessage: "Cannot assign an order with no items.",
            type: ErrorType.Validation
        );

    public static Error CannotChangeStatusFromTo(
        OrderStatusType from,
        OrderStatusType to
    ) =>
        new Error(
            code: "cannot_change_status_from_to",
            structuredMessage: "Cannot change status from {from} to {to}.",
            type: ErrorType.Validation,
            from,
            to
        );
}