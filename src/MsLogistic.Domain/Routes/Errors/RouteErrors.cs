using MsLogistic.Core.Results;
using MsLogistic.Domain.Routes.Enums;

namespace MsLogistic.Domain.Routes.Errors;

public static class RouteErrors
{
    public static Error ScheduleDateCannotBeInThePast =>
        Error.Validation(
            code: "Route.ScheduleDate.CannotBeInThePast",
            message: "Schedule date cannot be in the past."
        );

    public static Error CannotChangeDriverIfNotPending =>
        Error.Validation(
            code: "Route.CannotChangeDriverIfNotPending",
            message: "Cannot change driver if route status is not pending."
        );

    public static Error DriverIsRequired =>
        Error.Validation(
            code: "Route.Driver.Required",
            message: "Driver is required for the route."
        );

    public static Error CannotChangeStatusFromTo(
        RouteStatusEnum from,
        RouteStatusEnum to
    ) =>
        Error.Validation(
            code: "Route.CannotChangeStatusFromTo",
            message: $"Cannot change route status from {from} to {to}."
        );
}