using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Orders.GetAllOrders;

public record GetAllOrdersQuery() : IRequest<Result<IReadOnlyList<OrderSummaryDto>>>;
