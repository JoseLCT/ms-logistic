using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Orders.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDetailDto>>;
