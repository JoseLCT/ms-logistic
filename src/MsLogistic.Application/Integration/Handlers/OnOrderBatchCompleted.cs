using Joselct.Communication.Contracts.Services;
using MediatR;
using MsLogistic.Application.Batches.CloseCurrentBatch;
using MsLogistic.Application.Integration.Events.Incoming;

namespace MsLogistic.Application.Integration.Handlers;

public class OnOrderBatchCompleted : IIntegrationMessageConsumer<OrderBatchCompletedMessage> {
	private readonly IMediator _mediator;

	public OnOrderBatchCompleted(IMediator mediator) {
		_mediator = mediator;
	}

	public async Task HandleAsync(OrderBatchCompletedMessage message, CancellationToken ct = default) {
		var command = new CloseCurrentBatchCommand();
		await _mediator.Send(command, ct);
	}
}
