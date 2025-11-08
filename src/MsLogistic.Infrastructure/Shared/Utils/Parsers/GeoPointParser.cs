using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class GeoPointParser
{
    public static Point ConvertToPoint(GeoPointValue geoPoint)
    {
        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        return geometryFactory.CreatePoint(new Coordinate(geoPoint.Longitude, geoPoint.Latitude));
    }   
}