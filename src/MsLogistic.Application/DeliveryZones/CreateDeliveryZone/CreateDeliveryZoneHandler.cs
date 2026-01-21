using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZones.CreateDeliveryZone;

public class CreateDeliveryZoneHandler : IRequestHandler<CreateDeliveryZoneCommand, Result<Guid>>
{
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDeliveryZoneHandler> _logger;

    public CreateDeliveryZoneHandler(
        IDeliveryZoneRepository deliveryZoneRepository,
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateDeliveryZoneHandler> logger
    )
    {
        _deliveryZoneRepository = deliveryZoneRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateDeliveryZoneCommand request, CancellationToken ct)
    {
        if (request.DriverId.HasValue)
        {
            var driverExistsResult = await DriverExists(request.DriverId.Value, ct);
            if (driverExistsResult.IsFailure)
            {
                return Result.Failure<Guid>(driverExistsResult.Error);
            }
        }

        var geoPoints = request.Boundaries
            .Select(p => GeoPointValue.Create(p.Latitude, p.Longitude))
            .ToList();

        var boundaries = BoundariesValue.Create(geoPoints);

        var deliveryZone = DeliveryZone.Create(
            request.DriverId,
            request.Code,
            request.Name,
            boundaries
        );

        await _deliveryZoneRepository.AddAsync(deliveryZone, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Delivery zone with id {DeliveryZoneId} created successfully.", deliveryZone.Id);

        return Result.Success(deliveryZone.Id);
    }

    private async Task<Result> DriverExists(Guid driverId, CancellationToken ct)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId, ct);

        if (driver is null)
        {
            return Result.Failure(
                CommonErrors.NotFoundById("Driver", driverId)
            );
        }

        return Result.Success();
    }
}