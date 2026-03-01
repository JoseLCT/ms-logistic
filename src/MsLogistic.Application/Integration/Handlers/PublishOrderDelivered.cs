using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;
using MsLogistic.Application.Integration.Events.Outgoing;

namespace MsLogistic.Application.Integration.Handlers;

internal class PublishOrderDelivered : INotificationHandler<OutboxMessageNotification<OrderDeliveredMessage>> {
    private readonly IExternalPublisher _publisher;

    public PublishOrderDelivered(IExternalPublisher publisher) {
        _publisher = publisher;
    }

    public async Task Handle(OutboxMessageNotification<OrderDeliveredMessage> message, CancellationToken ct) {
        await _publisher.PublishAsync(
            message.Content,
            destination: "orders",
            routingKey: "order.delivered",
            ct: ct
        );
    }
}
