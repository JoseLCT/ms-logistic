using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Customers.GetAllCustomers;

public record GetAllCustomersQuery() : IRequest<Result<IReadOnlyList<CustomerSummaryDto>>>;
