using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Repositories;

namespace MsLogistic.Application.DeliveryPerson.DeleteDeliveryPerson;

internal class DeleteDeliveryPersonHandler : IRequestHandler<DeleteDeliveryPersonCommand, Result<Guid>>
{
    private readonly IDeliveryPersonRepository _deliveryPersonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDeliveryPersonHandler(IDeliveryPersonRepository deliveryPersonRepository, IUnitOfWork unitOfWork)
    {
        _deliveryPersonRepository = deliveryPersonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteDeliveryPersonCommand request, CancellationToken cancellationToken)
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

        await _deliveryPersonRepository.DeleteAsync(deliveryPerson.Id);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(deliveryPerson.Id);
    }
}