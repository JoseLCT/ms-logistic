using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Products.Errors;

public static class ProductErrors {
    public static Error NameIsRequired =>
        Error.Validation(
            code: "Product.Name.Required",
            message: "Name is required."
        );
}
