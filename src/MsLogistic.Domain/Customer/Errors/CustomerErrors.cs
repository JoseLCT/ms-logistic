using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Customer.Errors;

public static class CustomerErrors
{
    public static Error NameIsRequired =>
        new Error(
            code: "customer_name_is_required",
            structuredMessage: "Customer name is required.",
            type: ErrorType.Validation
        );
}