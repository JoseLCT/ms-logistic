namespace MsLogistic.Domain.Shared.ValueObjects;

public record GeoPointValue
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public GeoPointValue(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}