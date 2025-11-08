using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customer.Repositories;

namespace MsLogistic.Application.Customer.DeleteCustomer;

internal class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
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

        await _customerRepository.DeleteAsync(customer.Id);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(customer.Id);
    }
}