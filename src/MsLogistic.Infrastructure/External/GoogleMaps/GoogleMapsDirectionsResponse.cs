using System.Text.Json.Serialization;

namespace MsLogistic.Infrastructure.External.GoogleMaps;

public class GoogleMapsDirectionsResponse {
	[JsonPropertyName("status")]
	public required string Status { get; set; }

	[JsonPropertyName("routes")]
	public List<Route> Routes { get; set; } = [];
}

public class Route {
	[JsonPropertyName("waypoint_order")]
	public List<int> WaypointOrder { get; set; } = [];

	[JsonPropertyName("legs")]
	public List<Leg> Legs { get; set; } = [];
}

public class Leg {
	[JsonPropertyName("distance")]
	public Distance? Distance { get; set; }

	[JsonPropertyName("duration")]
	public Duration? Duration { get; set; }
}

public class Distance {
	[JsonPropertyName("text")]
	public required string Text { get; set; }

	[JsonPropertyName("value")]
	public int Value { get; set; }
}

public class Duration {
	[JsonPropertyName("text")]
	public required string Text { get; set; }

	[JsonPropertyName("value")]
	public int Value { get; set; }
}
