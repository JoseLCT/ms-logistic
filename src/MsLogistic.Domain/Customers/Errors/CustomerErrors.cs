using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Customers.Errors;

public static class CustomerErrors
{
    public static Error FullNameIsRequired =>
        Error.Validation(
            code: "Customer.FullName.Required",
            message: "Customer full name is required."
        );
}