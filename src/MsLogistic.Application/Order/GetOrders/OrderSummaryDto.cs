using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Order.Types;

namespace MsLogistic.Application.Order.GetOrders;

public record OrderSummaryDto
{
    public Guid Id { get; set; }
    public int? DeliverySequence { get; set; }
    public OrderStatusType Status { get; set; }
    public required CoordinateDto DeliveryLocation { get; set; }
}