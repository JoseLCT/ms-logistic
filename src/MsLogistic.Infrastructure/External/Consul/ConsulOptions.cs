namespace MsLogistic.Infrastructure.External.Consul;

public class ConsulOptions {
	public const string SectionName = "Consul";

	public string Host { get; set; } = "http://localhost:8500";
	public string ServiceName { get; set; } = "ms-logistic";
	public string ServiceAddress { get; set; } = "localhost";
	public int ServicePort { get; set; } = 80;
	public string[] Tags { get; set; } = ["dotnet", "api", "logistic", "metrics"];
	public string HealthCheckEndpoint { get; set; } = "/health/live";
}
