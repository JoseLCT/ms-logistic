using MsLogistic.Core.Results;
using MsLogistic.Domain.Route.Types;

namespace MsLogistic.Domain.Route.Errors;

public static class RouteErrors
{
    public static Error ScheduleDateCannotBeInThePast =>
        new Error(
            code: "schedule_date_cannot_be_in_the_past",
            structuredMessage: "Schedule date cannot be in the past.",
            type: ErrorType.Validation
        );

    public static Error CannotChangeDeliveryPersonIfNotPending =>
        new Error(
            code: "cannot_change_delivery_person_if_not_pending",
            structuredMessage: "Cannot change delivery person if route status is not pending.",
            type: ErrorType.Validation
        );

    public static Error DeliveryPersonIsRequired =>
        new Error(
            code: "delivery_person_is_required",
            structuredMessage: "Delivery person is required for the route.",
            type: ErrorType.Validation
        );

    public static Error CannotChangeStatusFromTo(
        RouteStatusType from,
        RouteStatusType to
    ) =>
        new Error(
            code: "cannot_change_status_from_to",
            structuredMessage: "Cannot change route status from {from} to {to}.",
            type: ErrorType.Validation,
            from,
            to
        );
}