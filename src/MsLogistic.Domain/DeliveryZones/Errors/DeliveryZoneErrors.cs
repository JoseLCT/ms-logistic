using MsLogistic.Core.Results;

namespace MsLogistic.Domain.DeliveryZones.Errors;

public static class DeliveryZoneErrors
{
    public static Error NameIsRequired =>
        Error.Validation(
            code: "DeliveryZone.Name.Required",
            message: "Delivery zone name is required."
        );

    public static Error CodeIsRequired =>
        Error.Validation(
            code: "DeliveryZone.Code.Required",
            message: "Delivery zone code is required."
        );

    public static Error CodeFormatIsInvalid =>
        Error.Validation(
            code: "DeliveryZone.Code.InvalidFormat",
            message: "Delivery zone code format is invalid. It should be in the format ABC-123."
        );
}