using MediatR;
using Microsoft.Extensions.Logging;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Customers.SyncCustomerFromExternal;

public class SyncCustomerFromExternalHandler : IRequestHandler<SyncCustomerFromExternalCommand, Result<Guid>> {
	private readonly ICustomerRepository _customerRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<SyncCustomerFromExternalHandler> _logger;

	public SyncCustomerFromExternalHandler(
		ICustomerRepository customerRepository,
		IUnitOfWork unitOfWork,
		ILogger<SyncCustomerFromExternalHandler> logger
	) {
		_customerRepository = customerRepository;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<Result<Guid>> Handle(SyncCustomerFromExternalCommand request, CancellationToken ct) {
		Customer? customer = await _customerRepository.GetByExternalIdAsync(request.ExternalId, ct);

		PhoneNumberValue? phoneNumber = request.PhoneNumber != null
			? PhoneNumberValue.Create(request.PhoneNumber)
			: null;

		if (customer == null) {
			customer = Customer.Create(request.FullName, phoneNumber, request.ExternalId);

			await _customerRepository.AddAsync(customer, ct);

			_logger.LogInformation("Customer with external id {ExternalId} created successfully.", request.ExternalId);
		} else {
			customer.SetFullName(request.FullName);
			customer.SetPhoneNumber(phoneNumber);

			_customerRepository.Update(customer);

			_logger.LogInformation("Customer with external id {ExternalId} updated successfully.", request.ExternalId);
		}

		await _unitOfWork.CommitAsync(ct);
		return Result.Success(customer.Id);
	}
}
