using MediatR;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Events;

namespace MsLogistic.Application.Orders.DomainEventHandlers;

public class CancelOrdersOnRouteCancelledHandler : INotificationHandler<RouteCancelled> {
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrdersOnRouteCancelledHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork
    ) {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RouteCancelled notification, CancellationToken ct) {
        var orders = await _orderRepository.GetByRouteIdAsync(notification.RouteId, ct);

        foreach (var order in orders) {
            order.Cancel();
            _orderRepository.Update(order);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
