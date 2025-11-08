using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Product.Errors;

public static class ProductErrors
{
    public static Error NameIsRequired =>
        new Error(
            code: "name_is_required",
            structuredMessage: "Product name is required.",
            type: ErrorType.Validation
        );
}