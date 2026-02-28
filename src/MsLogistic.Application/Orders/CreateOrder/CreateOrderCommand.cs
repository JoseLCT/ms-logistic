using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Orders.CreateOrder;

public record CreateOrderCommand(
    Guid CustomerId,
    DateTime ScheduledDeliveryDate,
    string DeliveryAddress,
    CoordinateDto DeliveryLocation,
    IReadOnlyCollection<CreateOrderItemDto> Items
) : IRequest<Result<Guid>>;

public record CreateOrderItemDto(Guid ProductId, int Quantity);
