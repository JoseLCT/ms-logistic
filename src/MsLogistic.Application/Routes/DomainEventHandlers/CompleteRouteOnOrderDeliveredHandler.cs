using MediatR;
using MsLogistic.Application.Routes.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Orders.Events;

namespace MsLogistic.Application.Routes.DomainEventHandlers;

public class CompleteRouteOnOrderDeliveredHandler : INotificationHandler<OrderDelivered> {
    private readonly RouteCompletionService _routeCompletionService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteRouteOnOrderDeliveredHandler(
        RouteCompletionService routeCompletionService,
        IUnitOfWork unitOfWork
    ) {
        _routeCompletionService = routeCompletionService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderDelivered notification, CancellationToken ct) {
        var completed = await _routeCompletionService.TryCompleteRouteAsync(notification.RouteId, ct);
        if (completed) {
            await _unitOfWork.CommitAsync(ct);
        }
    }
}
