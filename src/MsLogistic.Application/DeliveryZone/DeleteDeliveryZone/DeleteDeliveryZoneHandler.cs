using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZone.Repositories;

namespace MsLogistic.Application.DeliveryZone.DeleteDeliveryZone;

internal class DeleteDeliveryZoneHandler : IRequestHandler<DeleteDeliveryZoneCommand, Result<Guid>>
{
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDeliveryZoneHandler(IDeliveryZoneRepository deliveryZoneRepository, IUnitOfWork unitOfWork)
    {
        _deliveryZoneRepository = deliveryZoneRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteDeliveryZoneCommand request, CancellationToken cancellationToken)
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

        await _deliveryZoneRepository.DeleteAsync(deliveryZone.Id);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(deliveryZone.Id);
    }
}