using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class PointParser
{
    public static GeoPointValue ConvertToGeoPointValue(Point point)
    {
        return new GeoPointValue(point.Y, point.X);
    }
}