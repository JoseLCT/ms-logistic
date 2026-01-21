/*using MediatR;

namespace MsLogistic.Application.Orders.EventHandlers;

internal class ChangeStatusWhenRouteStarted : INotificationHandler<RouteStarted>
{
    private readonly IOrderRepository _orderRepository;

    public ChangeStatusWhenRouteStarted(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task Handle(RouteStarted notification, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByRouteIdAsync(notification.RouteId);
        foreach (var order in orders)
        {
            order.MarkAsInProgress();
            await _orderRepository.UpdateAsync(order);
        }
    }
}*/