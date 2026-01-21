using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Shared.Errors;

public static class GeoPointErrors
{
    public static Error LatitudeOutOfRange =>
        Error.Validation(
            code: "GeoPoint.LatitudeOutOfRange",
            message: "Latitude must be between -90 and 90 degrees."
        );

    public static Error LongitudeOutOfRange =>
        Error.Validation(
            code: "GeoPoint.LongitudeOutOfRange",
            message: "Longitude must be between -180 and 180 degrees."
        );
}