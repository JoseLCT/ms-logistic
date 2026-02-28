using MediatR;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Events;

namespace MsLogistic.Application.Orders.DomainEventHandlers;

public class ChangeStatusOnRouteStartedHandler : INotificationHandler<RouteStarted> {
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeStatusOnRouteStartedHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork
    ) {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RouteStarted notification, CancellationToken ct) {
        var orders = await _orderRepository.GetByRouteIdAsync(notification.RouteId, ct);

        foreach (var order in orders) {
            order.MarkAsInTransit();
            _orderRepository.Update(order);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
