namespace MsLogistic.Application.Abstractions.Options;

public class LogisticsOptions {
    public const string SectionName = "Logistics";

    public DepotOptions Depot { get; init; } = new();
}

public class DepotOptions {
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
