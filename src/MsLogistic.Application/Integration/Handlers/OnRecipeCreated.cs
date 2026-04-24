using Joselct.Communication.Contracts.Services;
using MediatR;
using MsLogistic.Application.Integration.Events.Incoming;
using MsLogistic.Application.Products.SyncProductFromExternal;

namespace MsLogistic.Application.Integration.Handlers;

public class OnRecipeCreated : IIntegrationMessageConsumer<RecipeCreatedMessage> {
	private readonly IMediator _mediator;

	public OnRecipeCreated(IMediator mediator) {
		_mediator = mediator;
	}

	public async Task HandleAsync(RecipeCreatedMessage message, CancellationToken ct = default) {
		var command = new SyncProductFromExternalCommand(
			message.RecipeId,
			message.Name
		);
		await _mediator.Send(command, ct);
	}
}
