using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Repositories;

namespace MsLogistic.Application.DeliveryPerson.CreateDeliveryPerson;

internal class CreateDeliveryPersonHandler : IRequestHandler<CreateDeliveryPersonCommand, Result<Guid>>
{
    private readonly IDeliveryPersonRepository _deliveryPersonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeliveryPersonHandler(IDeliveryPersonRepository deliveryPersonRepository, IUnitOfWork unitOfWork)
    {
        _deliveryPersonRepository = deliveryPersonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDeliveryPersonCommand request, CancellationToken cancellationToken)
    {
        var deliveryPerson = new Domain.DeliveryPerson.Entities.DeliveryPerson(request.Name);

        await _deliveryPersonRepository.AddAsync(deliveryPerson);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(deliveryPerson.Id);
    }
}