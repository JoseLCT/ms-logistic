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
        try
        {
            var phoneNumber = new PhoneNumberValue(request.PhoneNumber);
            var customer = new Domain.Customer.Entities.Customer(request.Name, phoneNumber);
            await _customerRepository.AddAsync(customer);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(customer.Id);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(ex.Error);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(new Error(
                "InvalidPhoneNumber",
                ex.Message,
                ErrorType.Validation
            ));
        }
    }
}