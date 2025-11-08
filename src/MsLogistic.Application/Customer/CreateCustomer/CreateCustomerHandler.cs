using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customer.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Customer.CreateCustomer;

internal class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Domain.Customer.Entities.Customer(request.Name, new PhoneNumberValue(request.PhoneNumber));

        await _customerRepository.AddAsync(customer);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(customer.Id);
    }
}