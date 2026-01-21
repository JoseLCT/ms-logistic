using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Customers.UpdateCustomer;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCustomerHandler> logger
    )
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, ct);

        if (customer is null)
        {
            return Result.Failure<Guid>(
                CommonErrors.NotFoundById("Customer", request.Id)
            );
        }

        var phoneNumber = request.PhoneNumber != null
            ? PhoneNumberValue.Create(request.PhoneNumber)
            : null;

        customer.SetFullName(request.FullName);
        customer.SetPhoneNumber(phoneNumber);

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Customer with id {CustomerId} updated successfully.", customer.Id);

        return Result.Success(customer.Id);
    }
}