using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Shared.Errors;

public static class CommonErrors
{
    public static Error NotFound(string entityName) =>
        Error.NotFound(
            code: $"{entityName}.NotFound",
            message: $"{entityName} was not found."
        );

    public static Error NotFoundById(string entityName, Guid id) =>
        Error.NotFound(
            code: $"{entityName}.NotFound",
            message: $"{entityName} with ID '{id}' was not found."
        );
}