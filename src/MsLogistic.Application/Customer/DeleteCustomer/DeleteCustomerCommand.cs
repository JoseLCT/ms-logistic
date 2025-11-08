using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customer.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<Result<Guid>>;