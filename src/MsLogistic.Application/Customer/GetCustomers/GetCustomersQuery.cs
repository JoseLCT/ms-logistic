using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customer.GetCustomers;

public record GetCustomersQuery() : IRequest<Result<ICollection<CustomerSummaryDto>>>;