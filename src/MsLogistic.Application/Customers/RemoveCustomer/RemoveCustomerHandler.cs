using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Customers.RemoveCustomer;

public class RemoveCustomerHandler : IRequestHandler<RemoveCustomerCommand, Result<Guid>> {
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveCustomerHandler> _logger;

    public RemoveCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveCustomerHandler> logger
    ) {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RemoveCustomerCommand request, CancellationToken ct) {
        var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

        if (customer is null) {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("Customer", request.Id)
            );
        }

        _customerRepository.Remove(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Customer with id {CustomerId} removed successfully.", customer.Id);

        return Result.Success(customer.Id);
    }
}
