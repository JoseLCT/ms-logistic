using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customer.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Customer.UpdateCustomer;

internal class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id);
        if (customer is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(
                    code: "customer_not_found",
                    structuredMessage: $"Customer with id {request.Id} was not found."
                )
            );
        }

        customer.SetName(request.Name);
        customer.SetPhoneNumber(new PhoneNumberValue(request.PhoneNumber));

        await _customerRepository.UpdateAsync(customer);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(customer.Id);
    }
}