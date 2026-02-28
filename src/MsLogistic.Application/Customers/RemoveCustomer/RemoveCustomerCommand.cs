using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customers.RemoveCustomer;

public record RemoveCustomerCommand(Guid Id) : IRequest<Result<Guid>>;
