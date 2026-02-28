using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class PointParser {
    public static GeoPointValue ConvertToGeoPointValue(Point point) {
        return GeoPointValue.Create(point.Y, point.X);
    }
}
