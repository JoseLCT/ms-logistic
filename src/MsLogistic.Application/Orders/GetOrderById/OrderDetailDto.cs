using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.Application.Orders.GetOrderById;

public record OrderDetailDto(
    Guid Id,
    Guid BatchId,
    Guid CustomerId,
    Guid? RouteId,
    int? DeliverySequence,
    OrderStatusEnum Status,
    DateTime ScheduledDeliveryDate,
    string DeliveryAddress,
    CoordinateDto DeliveryLocation,
    OrderDeliveryDto? Delivery,
    OrderIncidentDto? Incident,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    int Quantity
);

public record OrderDeliveryDto(
    Guid Id,
    Guid DriverId,
    CoordinateDto Location,
    DateTime DeliveredAt,
    string? Comments,
    string? ImageUrl
);

public record OrderIncidentDto(
    Guid Id,
    Guid DriverId,
    OrderIncidentTypeEnum IncidentType,
    string Description
);
