using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customers.UpdateCustomer;

public record UpdateCustomerCommand(Guid Id, string FullName, string? PhoneNumber) : IRequest<Result<Guid>>;
