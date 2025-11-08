using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Order.GetOrders;

public record GetOrdersQuery() : IRequest<Result<ICollection<OrderSummaryDto>>>;