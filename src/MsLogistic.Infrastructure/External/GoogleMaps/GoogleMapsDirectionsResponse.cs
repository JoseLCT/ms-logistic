namespace MsLogistic.Infrastructure.External.GoogleMaps;

public class GoogleMapsDirectionsResponse {
    public required string Status { get; set; }
    public List<Route> Routes { get; set; } = [];
}

public class Route {
    public List<int> WaypointOrder { get; set; } = [];
    public List<Leg>? Legs { get; set; }
}

public class Leg {
    public Distance? Distance { get; set; }
    public Duration? Duration { get; set; }
}

public class Distance {
    public required string Text { get; set; }
    public int Value { get; set; }
}

public class Duration {
    public required string Text { get; set; }
    public int Value { get; set; }
}
