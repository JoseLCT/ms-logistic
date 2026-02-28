namespace MsLogistic.Infrastructure.External.GoogleMaps;

public class GoogleMapsOptions {
    public const string SectionName = "GoogleMaps";

    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }
    public int TimeoutSeconds { get; init; } = 10;
}
