using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Shared.Errors;

public static class PhoneNumberErrors {
    public static Error Empty =>
        Error.Validation(
            code: "PhoneNumber.Empty",
            message: "Phone number cannot be empty."
        );

    public static Error InvalidFormat =>
        Error.Validation(
            code: "PhoneNumber.InvalidFormat",
            message: "Phone number must be in E.164 format."
        );
}
