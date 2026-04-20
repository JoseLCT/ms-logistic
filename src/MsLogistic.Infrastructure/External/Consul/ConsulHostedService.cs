using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MsLogistic.Infrastructure.External.Consul;

internal sealed class ConsulHostedService(
	IConsulClient consulClient,
	IOptions<ConsulOptions> options,
	ILogger<ConsulHostedService> logger
) : IHostedService {
	private readonly ConsulOptions _options = options.Value;
	private string? _registrationId;

	public async Task StartAsync(CancellationToken cancellationToken) {
		_registrationId = $"{_options.ServiceName}-{_options.ServiceAddress}-{_options.ServicePort}";

		var registration = new AgentServiceRegistration {
			ID = _registrationId,
			Name = _options.ServiceName,
			Address = _options.ServiceAddress,
			Port = _options.ServicePort,
			Tags = _options.Tags,
			Check = new AgentServiceCheck {
				HTTP = $"http://{_options.ServiceAddress}:{_options.ServicePort}{_options.HealthCheckEndpoint}",
				Interval = TimeSpan.FromSeconds(10),
				Timeout = TimeSpan.FromSeconds(5),
				DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
			}
		};

		logger.LogInformation(
			"Registering service {ServiceName} at {Address}:{Port} in Consul",
			_options.ServiceName, _options.ServiceAddress, _options.ServicePort);

		try {
			await consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
			await consulClient.Agent.ServiceRegister(registration, cancellationToken);

			logger.LogInformation("Service registered successfully with ID {RegistrationId}", _registrationId);
		} catch (Exception ex) {
			logger.LogError(ex, "Failed to register service in Consul");
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken) {
		if (_registrationId is null) return;

		logger.LogInformation("Deregistering service {ServiceId} from Consul", _registrationId);

		try {
			await consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
		} catch (Exception ex) {
			logger.LogError(ex, "Failed to deregister service from Consul");
		}
	}
}
