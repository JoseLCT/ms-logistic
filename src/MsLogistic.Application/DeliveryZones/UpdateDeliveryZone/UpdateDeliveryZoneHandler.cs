using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZones.UpdateDeliveryZone;

public class UpdateDeliveryZoneHandler : IRequestHandler<UpdateDeliveryZoneCommand, Result<Guid>> {
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDeliveryZoneHandler> _logger;

    public UpdateDeliveryZoneHandler(
        IDeliveryZoneRepository deliveryZoneRepository,
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateDeliveryZoneHandler> logger
    ) {
        _deliveryZoneRepository = deliveryZoneRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateDeliveryZoneCommand request, CancellationToken ct) {
        var deliveryZone = await _deliveryZoneRepository.GetByIdAsync(request.Id, ct);

        if (deliveryZone is null) {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("DeliveryZone", request.Id)
            );
        }

        if (request.DriverId.HasValue && request.DriverId != deliveryZone.DriverId) {
            var driverExistsResult = await DriverExists(request.DriverId.Value, ct);
            if (driverExistsResult.IsFailure) {
                return Result.Failure<Guid>(driverExistsResult.Error);
            }
        }

        var geoPoints = request.Boundaries
            .Select(p => GeoPointValue.Create(p.Latitude, p.Longitude))
            .ToList();

        var boundaries = BoundariesValue.Create(geoPoints);

        deliveryZone.SetDriverId(request.DriverId);
        deliveryZone.SetCode(request.Code);
        deliveryZone.SetName(request.Name);
        deliveryZone.SetBoundaries(boundaries);

        _deliveryZoneRepository.Update(deliveryZone);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Delivery zone with id {DeliveryZoneId} updated successfully.", deliveryZone.Id);

        return Result.Success(deliveryZone.Id);
    }

    private async Task<Result> DriverExists(Guid driverId, CancellationToken ct) {
        var driver = await _driverRepository.GetByIdAsync(driverId, ct);

        if (driver is null) {
            return Result.Failure(
                CommonErrors.NotFoundById("Driver", driverId)
            );
        }

        return Result.Success();
    }
}
