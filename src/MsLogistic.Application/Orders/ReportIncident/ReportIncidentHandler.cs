using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Orders.ReportIncident;

public class ReportIncidentHandler : IRequestHandler<ReportIncidentCommand, Result> {
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportIncidentHandler> _logger;

    public ReportIncidentHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportIncidentHandler> logger
    ) {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ReportIncidentCommand request, CancellationToken ct) {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);

        if (order is null) {
            return Result.Failure(CommonErrors.NotFoundById("Order", request.OrderId));
        }

        order.ReportIncident(
            request.DriverId,
            request.IncidentType,
            request.Description
        );

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation($"Report Incident {request.IncidentType} has been reported.");

        return Result.Success();
    }
}
