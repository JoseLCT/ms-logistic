using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Order.GetOrder;

public record GetOrderQuery(Guid Id) : IRequest<Result<OrderDetailDto>>;