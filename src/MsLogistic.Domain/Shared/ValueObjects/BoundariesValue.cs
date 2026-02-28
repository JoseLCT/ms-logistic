using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Domain.Shared.ValueObjects;

public record BoundariesValue {
    private const double Tolerance = 0.000001;
    private const int MinimumPoints = 3;

    public IReadOnlyList<GeoPointValue> Coordinates { get; }

    private BoundariesValue(IReadOnlyList<GeoPointValue> coordinates) {
        Coordinates = coordinates;
    }

    public static BoundariesValue Create(IReadOnlyList<GeoPointValue> coordinates) {
        if (coordinates.Count < MinimumPoints) {
            throw new DomainException(BoundariesErrors.InsufficientPoints(MinimumPoints));
        }

        if (HasConsecutiveDuplicates(coordinates)) {
            throw new DomainException(BoundariesErrors.ConsecutiveDuplicatePoints);
        }

        if (HasDuplicatePoints(coordinates)) {
            throw new DomainException(BoundariesErrors.DuplicatePoints);
        }

        var closedCoordinates = EnsureClosedPolygon(coordinates);

        return new BoundariesValue(closedCoordinates);
    }

    public bool Contains(GeoPointValue point) {
        int crossings = 0;

        for (int i = 0; i < Coordinates.Count - 1; i++) {
            var p1 = Coordinates[i];
            var p2 = Coordinates[i + 1];

            if (RayCrossesSegment(point, p1, p2)) {
                crossings++;
            }
        }

        return crossings % 2 == 1;
    }

    public GeoPointValue GetCenter() {
        var avgLat = Coordinates.Take(Coordinates.Count - 1).Average(p => p.Latitude);
        var avgLon = Coordinates.Take(Coordinates.Count - 1).Average(p => p.Longitude);

        return GeoPointValue.Create(avgLat, avgLon);
    }

    private static IReadOnlyList<GeoPointValue> EnsureClosedPolygon(IReadOnlyList<GeoPointValue> coordinates) {
        var first = coordinates[0];
        var last = coordinates[^1];

        if (ArePointsEqual(first, last)) {
            return coordinates;
        }

        var list = new List<GeoPointValue>(coordinates.Count + 1);
        list.AddRange(coordinates);
        list.Add(first);

        return list.AsReadOnly();
    }

    private static bool HasConsecutiveDuplicates(IReadOnlyList<GeoPointValue> coordinates) {
        for (int i = 0; i < coordinates.Count - 1; i++) {
            if (ArePointsEqual(coordinates[i], coordinates[i + 1])) {
                return true;
            }
        }

        return false;
    }

    private static bool HasDuplicatePoints(IReadOnlyList<GeoPointValue> coordinates) {
        var uniquePoints = new HashSet<(double lat, double lon)>();

        for (int i = 0; i < coordinates.Count; i++) {
            var point = (coordinates[i].Latitude, coordinates[i].Longitude);

            if (i == coordinates.Count - 1 && ArePointsEqual(coordinates[i], coordinates[0])) {
                continue;
            }

            if (!uniquePoints.Add(point)) {
                return true;
            }
        }

        return false;
    }

    private static bool ArePointsEqual(GeoPointValue p1, GeoPointValue p2) {
        return Math.Abs(p1.Latitude - p2.Latitude) <= Tolerance &&
               Math.Abs(p1.Longitude - p2.Longitude) <= Tolerance;
    }

    private static bool RayCrossesSegment(GeoPointValue point, GeoPointValue p1, GeoPointValue p2) {
        return ((p1.Latitude <= point.Latitude && point.Latitude < p2.Latitude) ||
                (p2.Latitude <= point.Latitude && point.Latitude < p1.Latitude)) &&
               point.Longitude < (p2.Longitude - p1.Longitude) *
               (point.Latitude - p1.Latitude) /
               (p2.Latitude - p1.Latitude) + p1.Longitude;
    }
}
