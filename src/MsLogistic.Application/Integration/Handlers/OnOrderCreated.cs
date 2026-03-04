using Joselct.Communication.Contracts.Services;
using MediatR;
using MsLogistic.Application.Integration.Events.Incoming;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Shared.DTOs;

namespace MsLogistic.Application.Integration.Handlers;

public class OnOrderCreated : IIntegrationMessageConsumer<OrderCreatedMessage> {
    private readonly IMediator _mediator;

    public OnOrderCreated(IMediator mediator) {
        _mediator = mediator;
    }

    public async Task HandleAsync(OrderCreatedMessage message, CancellationToken ct = default) {
        var location = new CoordinateDto(
            Latitude: message.DeliveryLocation.Latitude,
            Longitude: message.DeliveryLocation.Longitude
        );

        var command = new CreateOrderCommand(
            CustomerId: message.CustomerId,
            ScheduledDeliveryDate: message.DeliveryDate,
            DeliveryAddress: message.DeliveryAddress,
            DeliveryLocation: location,
            Items: message.Items.Select(i => new CreateOrderItemDto(
                ProductId: i.RecipeId,
                Quantity: i.Quantity
            )).ToList()
        );

        await _mediator.Send(command, ct);
    }
}
