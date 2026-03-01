using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.DeliveryZones.RemoveDeliveryZone;

public class RemoveDeliveryZoneHandler : IRequestHandler<RemoveDeliveryZoneCommand, Result<Guid>> {
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveDeliveryZoneHandler> _logger;

    public RemoveDeliveryZoneHandler(
        IDeliveryZoneRepository deliveryZoneRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveDeliveryZoneHandler> logger
    ) {
        _deliveryZoneRepository = deliveryZoneRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RemoveDeliveryZoneCommand request, CancellationToken ct) {
        var deliveryZone = await _deliveryZoneRepository.GetByIdAsync(request.Id, ct);

        if (deliveryZone is null) {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("DeliveryZone", request.Id)
            );
        }

        _deliveryZoneRepository.Remove(deliveryZone);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Delivery zone with id {DeliveryZoneId} removed successfully.", deliveryZone.Id);

        return Result.Success(deliveryZone.Id);
    }
}
