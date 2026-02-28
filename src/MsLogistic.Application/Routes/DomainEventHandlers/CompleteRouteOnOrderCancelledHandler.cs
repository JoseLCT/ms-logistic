using MediatR;
using MsLogistic.Application.Routes.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Orders.Events;

namespace MsLogistic.Application.Routes.DomainEventHandlers;

public class CompleteRouteOnOrderCancelledHandler : INotificationHandler<OrderCancelled> {
    private readonly RouteCompletionService _routeCompletionService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteRouteOnOrderCancelledHandler(
        RouteCompletionService routeCompletionService,
        IUnitOfWork unitOfWork
    ) {
        _routeCompletionService = routeCompletionService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderCancelled notification, CancellationToken ct) {
        var completed = await _routeCompletionService.TryCompleteRouteAsync(notification.RouteId, ct);
        if (completed) {
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
