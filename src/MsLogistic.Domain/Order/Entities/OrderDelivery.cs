using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Order.Entities;

public class OrderDelivery : Entity
{
    public Guid OrderId { get; private set; }
    public Guid DeliveryPersonId { get; private set; }
    public GeoPointValue Location { get; private set; }
    public DateTime DeliveredAt { get; private set; }
    public string? Comments { get; private set; }
    public string? ImageUrl { get; private set; }

    private OrderDelivery()
    {
    }

    public OrderDelivery(
        Guid orderId,
        Guid deliveryPersonId,
        GeoPointValue location,
        DateTime deliveredAt,
        string? comments) : base(Guid.NewGuid())
    {
        OrderId = orderId;
        DeliveryPersonId = deliveryPersonId;
        Location = location;
        DeliveredAt = deliveredAt;
        Comments = comments;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddImage(string imageUrl)
    {
        ImageUrl = imageUrl;
        MarkAsUpdated();
    }
}