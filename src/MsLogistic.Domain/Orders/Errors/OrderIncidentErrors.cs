using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Orders.Errors;

public static class OrderIncidentErrors
{
    public static Error DescriptionIsRequired =>
        Error.Failure(
            code: "OrderIncident.Description.Required",
            message: "Description is required."
        );
}