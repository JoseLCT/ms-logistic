using MediatR;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Customer.UpdateCustomer;

public record UpdateCustomerCommand(Guid Id, string Name, string PhoneNumber) : IRequest<Result<Guid>>;