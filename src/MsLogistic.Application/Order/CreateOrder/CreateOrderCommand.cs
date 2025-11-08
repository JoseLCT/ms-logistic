using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Order.CreateOrder;

public record CreateOrderCommand(
    Guid CustomerId,
    DateTime ScheduledDeliveryDate,
    string DeliveryAddress,
    CoordinateDto DeliveryLocation,
    ICollection<CreateOrderItemCommand> Items
) : IRequest<Result<Guid>>;

public record CreateOrderItemCommand(Guid ProductId, int Quantity);