using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customers.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerDetailDto>>;
