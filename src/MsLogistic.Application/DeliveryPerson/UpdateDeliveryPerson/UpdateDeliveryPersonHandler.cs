using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Repositories;

namespace MsLogistic.Application.DeliveryPerson.UpdateDeliveryPerson;

internal class UpdateDeliveryPersonHandler : IRequestHandler<UpdateDeliveryPersonCommand, Result<Guid>>
{
    private readonly IDeliveryPersonRepository _deliveryPersonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeliveryPersonHandler(IDeliveryPersonRepository deliveryPersonRepository, IUnitOfWork unitOfWork)
    {
        _deliveryPersonRepository = deliveryPersonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdateDeliveryPersonCommand request, CancellationToken cancellationToken)
    {
        var deliveryPerson = await _deliveryPersonRepository.GetByIdAsync(request.Id);
        if (deliveryPerson is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(
                    code: "delivery_person_not_found",
                    structuredMessage: $"Delivery person with id {request.Id} was not found."
                )
            );
        }

        deliveryPerson.SetName(request.Name);
        deliveryPerson.SetIsActive(request.IsActive);
        deliveryPerson.SetStatus(request.Status);

        await _deliveryPersonRepository.UpdateAsync(deliveryPerson);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(deliveryPerson.Id);
    }
}