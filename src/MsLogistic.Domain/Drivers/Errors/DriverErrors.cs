using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Drivers.Errors;

public static class DriverErrors {
    public static Error FullNameIsRequired =>
        Error.Validation(
            code: "Driver.FullName.Required",
            message: "Driver full name is required."
        );
}
