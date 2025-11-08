using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Order.Types;

namespace MsLogistic.Application.Order.GetOrder;

public record OrderDetailDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? RouteId { get; set; }
    public int? DeliverySequence { get; set; }
    public OrderStatusType Status { get; set; }
    public DateTime ScheduledDeliveryDate { get; set; }
    public required string DeliveryAddress { get; set; }
    public required CoordinateDto DeliveryLocation { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public OrderDeliveryDto? Delivery { get; set; }
}

public record OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public record OrderDeliveryDto
{
    public Guid Id { get; set; }
    public Guid DeliveryPersonId { get; set; }
    public required CoordinateDto Location { get; set; }
    public DateTime DeliveredAt { get; set; }
    public string? Comments { get; set; }
    public string? ImageUrl { get; set; }
}