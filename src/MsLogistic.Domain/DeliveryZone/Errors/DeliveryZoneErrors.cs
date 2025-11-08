using MsLogistic.Core.Results;

namespace MsLogistic.Domain.DeliveryZone.Errors;

public static class DeliveryZoneErrors
{
    public static Error NameIsRequired =>
        new Error(
            code: "delivery_zone_name_is_required",
            structuredMessage: "Delivery zone name is required.",
            type: ErrorType.Validation
        );

    public static Error CodeIsRequired =>
        new Error(
            code: "delivery_zone_code_is_required",
            structuredMessage: "Delivery zone code is required.",
            type: ErrorType.Validation
        );

    public static Error CodeFormatIsInvalid =>
        new Error(
            code: "delivery_zone_code_format_is_invalid",
            structuredMessage: "Delivery zone code format is invalid. It should be in the format ABC-123.",
            type: ErrorType.Validation
        );
}