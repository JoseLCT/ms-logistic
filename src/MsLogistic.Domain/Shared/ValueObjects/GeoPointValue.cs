using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Domain.Shared.ValueObjects;

public record GeoPointValue {
    public double Latitude { get; }
    public double Longitude { get; }

    private GeoPointValue(double latitude, double longitude) {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoPointValue Create(double latitude, double longitude) {
        if (latitude is < -90 or > 90)
            throw new DomainException(GeoPointErrors.LatitudeOutOfRange);

        if (longitude is < -180 or > 180)
            throw new DomainException(GeoPointErrors.LongitudeOutOfRange);

        return new GeoPointValue(latitude, longitude);
    }
}
