using MediatR;
using MsLogistic.Application.Routes.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Orders.Events;

namespace MsLogistic.Application.Routes.DomainEventHandlers;

public class CompleteRouteOnOrderIncidentReportedHandler : INotificationHandler<OrderIncidentReported> {
    private readonly RouteCompletionService _routeCompletionService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteRouteOnOrderIncidentReportedHandler(
        RouteCompletionService routeCompletionService,
        IUnitOfWork unitOfWork
    ) {
        _routeCompletionService = routeCompletionService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderIncidentReported notification, CancellationToken ct) {
        var completed = await _routeCompletionService.TryCompleteRouteAsync(notification.RouteId, ct);
        if (completed) {
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
