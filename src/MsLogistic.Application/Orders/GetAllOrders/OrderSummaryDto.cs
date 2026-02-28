using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Orders.Enums;

namespace MsLogistic.Application.Orders.GetAllOrders;

public record OrderSummaryDto(
    Guid Id,
    int? DeliverySequence,
    OrderStatusEnum Status,
    DateTime ScheduledDeliveryDate,
    CoordinateDto DeliveryLocation
);
