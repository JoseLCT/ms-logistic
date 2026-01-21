using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customers.CreateCustomer;

public record CreateCustomerCommand(string FullName, string? PhoneNumber) : IRequest<Result<Guid>>;