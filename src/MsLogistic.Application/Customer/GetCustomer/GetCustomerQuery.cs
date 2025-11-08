using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customer.GetCustomer;

public record GetCustomerQuery(Guid Id) : IRequest<Result<CustomerDetailDto>>;