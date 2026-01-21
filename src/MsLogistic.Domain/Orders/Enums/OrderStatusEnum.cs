namespace MsLogistic.Domain.Orders.Enums;

public enum OrderStatusEnum
{
    Pending = 0,
    InTransit = 1,
    Delivered = 2,
    Cancelled = 3,
    Failed = 4
}