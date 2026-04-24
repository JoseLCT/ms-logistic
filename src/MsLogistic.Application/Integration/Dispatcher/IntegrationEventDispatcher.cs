using System.Text.Json;
using Joselct.Communication.Contracts.Messages;
using Joselct.Communication.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsLogistic.Application.Integration.Events.Incoming;

namespace MsLogistic.Application.Integration.Dispatcher;

public class IntegrationEventDispatcher : IIntegrationMessageConsumer<RawMessage> {
	private readonly IServiceProvider _sp;
	private readonly ILogger<IntegrationEventDispatcher> _logger;

	private static readonly Dictionary<string, Type> _routes = new() {
		["patient.created"] = typeof(PatientCreatedMessage),
		["order.created"] = typeof(OrderCreatedMessage),
		["order.completed"] = typeof(OrderBatchCompletedMessage)
	};

	private static readonly JsonSerializerOptions _jsonOptions = new() {
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public IntegrationEventDispatcher(
		IServiceProvider sp,
		ILogger<IntegrationEventDispatcher> logger
	) {
		_sp = sp;
		_logger = logger;
	}

	public async Task HandleAsync(RawMessage raw, CancellationToken ct) {
		if (!_routes.TryGetValue(raw.RoutingKey, out Type? messageType)) {
			_logger.LogWarning("No handler registered for routing key {RoutingKey}", raw.RoutingKey);
			return;
		}

		var message = (IntegrationMessage)JsonSerializer
			.Deserialize(raw.Body, messageType, _jsonOptions)!;

		Type handlerType = typeof(IIntegrationMessageConsumer<>)
			.MakeGenericType(messageType);

		object handler = _sp.GetRequiredService(handlerType);

		await (Task)handlerType
			.GetMethod(nameof(IIntegrationMessageConsumer<>.HandleAsync))!
			.Invoke(handler, [message, ct])!;
	}
}
