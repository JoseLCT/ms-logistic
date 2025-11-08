using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class PolygonParser
{
    public static ZoneBoundaryValue ConvertToZoneBoundaryValue(Polygon polygon)
    {
        var geoPoints = polygon.Coordinates
            .Select(c => new GeoPointValue(c.Y, c.X))
            .ToList();

        return ZoneBoundaryValue.Create(geoPoints);
    }
}