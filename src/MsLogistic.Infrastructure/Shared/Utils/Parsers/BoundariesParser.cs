using MsLogistic.Domain.Shared.ValueObjects;
using NetTopologySuite.Geometries;

namespace MsLogistic.Infrastructure.Shared.Utils.Parsers;

internal class BoundariesParser {
	public static Polygon ConvertToPolygon(BoundariesValue boundaries) {
		GeometryFactory geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

		Coordinate[] coordinates = boundaries.Coordinates
			.Select(c => new Coordinate(c.Longitude, c.Latitude))
			.ToArray();

		LinearRing linearRing = geometryFactory.CreateLinearRing(coordinates);
		return geometryFactory.CreatePolygon(linearRing);
	}
}
