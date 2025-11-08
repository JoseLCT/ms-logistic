using MsLogistic.Core.Results;

namespace MsLogistic.Domain.DeliveryPerson.Errors;

public static class DeliveryPersonErrors
{
    public static Error NameIsRequired =>
        new Error(
            code: "delivery_person_name_is_required",
            structuredMessage: "Delivery person name is required.",
            type: ErrorType.Validation
        );
}