namespace MsLogistic.Domain.Shared.ValueObjects;

public record ZoneBoundaryValue
{
    public IReadOnlyList<GeoPointValue> Coordinates { get; }

    private ZoneBoundaryValue(IReadOnlyList<GeoPointValue> coordinates)
    {
        if (coordinates == null || coordinates.Count < 3)
        {
            throw new ArgumentException("A polygon must have at least 3 points.", nameof(coordinates));
        }

        var first = coordinates[0];
        var last = coordinates[^1];

        if (first.Latitude != last.Latitude || first.Longitude != last.Longitude)
        {
            var list = coordinates.ToList();
            list.Add(first);
            coordinates = list.AsReadOnly();
        }

        Coordinates = coordinates;
    }

    public static ZoneBoundaryValue Create(IEnumerable<GeoPointValue> coordinates)
    {
        return new ZoneBoundaryValue(coordinates.ToList().AsReadOnly());
    }
}