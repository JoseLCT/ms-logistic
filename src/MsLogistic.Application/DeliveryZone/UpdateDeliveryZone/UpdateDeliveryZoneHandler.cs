using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Repositories;
using MsLogistic.Domain.DeliveryZone.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZone.UpdateDeliveryZone;

internal class UpdateDeliveryZoneHandler : IRequestHandler<UpdateDeliveryZoneCommand, Result<Guid>>
{
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IDeliveryPersonRepository _deliveryPersonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeliveryZoneHandler(
        IDeliveryZoneRepository deliveryZoneRepository,
        IDeliveryPersonRepository deliveryPersonRepository,
        IUnitOfWork unitOfWork)
    {
        _deliveryZoneRepository = deliveryZoneRepository;
        _deliveryPersonRepository = deliveryPersonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdateDeliveryZoneCommand request, CancellationToken cancellationToken)
    {
        var deliveryZone = await _deliveryZoneRepository.GetByIdAsync(request.Id);
        if (deliveryZone is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(
                    code: "delivery_zone_not_found",
                    structuredMessage: $"Delivery zone with id {request.Id} was not found."
                )
            );
        }

        if (request.DeliveryPersonId.HasValue && request.DeliveryPersonId != deliveryZone.DeliveryPersonId)
        {
            var deliveryPerson = await _deliveryPersonRepository.GetByIdAsync(request.DeliveryPersonId.Value);
            if (deliveryPerson is null)
            {
                return Result.Failure<Guid>(
                    Error.NotFound(
                        code: "delivery_person_not_found",
                        structuredMessage: $"Delivery person with id {request.DeliveryPersonId.Value} was not found."
                    )
                );
            }
        }

        var geoPoints = request.Boundaries
            .Select(p => new GeoPointValue(p.Latitude, p.Longitude))
            .ToList();

        deliveryZone.SetDeliveryPersonId(request.DeliveryPersonId);
        deliveryZone.SetCode(request.Code);
        deliveryZone.SetName(request.Name);
        deliveryZone.SetBoundaries(ZoneBoundaryValue.Create(geoPoints));

        await _deliveryZoneRepository.UpdateAsync(deliveryZone);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(deliveryZone.Id);
    }
}