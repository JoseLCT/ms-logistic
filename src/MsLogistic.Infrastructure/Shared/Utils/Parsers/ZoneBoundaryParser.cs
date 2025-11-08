using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class ZoneBoundaryParser
{
    public static Polygon ConvertToPolygon(ZoneBoundaryValue zoneBoundary)
    {
        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        var coordinates = zoneBoundary.Coordinates
            .Select(c => new Coordinate(c.Longitude, c.Latitude))
            .ToArray();

        var linearRing = geometryFactory.CreateLinearRing(coordinates);
        return geometryFactory.CreatePolygon(linearRing);
    }
}