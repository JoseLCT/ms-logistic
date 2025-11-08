using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Repositories;
using MsLogistic.Domain.DeliveryZone.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZone.CreateDeliveryZone;

internal class CreateDeliveryZoneHandler : IRequestHandler<CreateDeliveryZoneCommand, Result<Guid>>
{
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IDeliveryPersonRepository _deliveryPersonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeliveryZoneHandler(IDeliveryZoneRepository deliveryZoneRepository,
        IDeliveryPersonRepository deliveryPersonRepository, IUnitOfWork unitOfWork)
    {
        _deliveryZoneRepository = deliveryZoneRepository;
        _deliveryPersonRepository = deliveryPersonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDeliveryZoneCommand request, CancellationToken cancellationToken)
    {
        if (request.DeliveryPersonId.HasValue)
        {
            var deliveryPersonResult =
                await _deliveryPersonRepository.GetByIdAsync(request.DeliveryPersonId.Value, readOnly: true);
            if (deliveryPersonResult is null)
            {
                return Result.Failure<Guid>(
                    Error.NotFound(
                        code: "delivery_person_not_found",
                        structuredMessage: $"Delivery person with ID {request.DeliveryPersonId.Value} was not found."
                    )
                );
            }
        }

        try
        {
            var geoPoints = request.Boundaries
                .Select(p => new GeoPointValue(p.Latitude, p.Longitude))
                .ToList();

            var deliveryZone = new Domain.DeliveryZone.Entities.DeliveryZone(
                request.DeliveryPersonId,
                request.Code,
                request.Name,
                ZoneBoundaryValue.Create(geoPoints)
            );

            await _deliveryZoneRepository.AddAsync(deliveryZone);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(deliveryZone.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(
                Error.Problem(
                    code: "invalid_zone_boundary",
                    structuredMessage: ex.Message
                )
            );
        }
    }
}