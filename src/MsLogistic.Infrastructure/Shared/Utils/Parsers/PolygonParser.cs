using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class PolygonParser {
    public static BoundariesValue ConvertToBoundariesValue(Polygon polygon) {
        var geoPoints = polygon.Coordinates
            .Select(c => GeoPointValue.Create(c.Y, c.X))
            .ToList();

        return BoundariesValue.Create(geoPoints);
    }
}
