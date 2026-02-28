using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Orders.Entities;

public class OrderDelivery : Entity {
    public Guid OrderId { get; private set; }
    public Guid DriverId { get; private set; }
    public GeoPointValue Location { get; private set; }
    public DateTime DeliveredAt { get; private set; }
    public string? Comments { get; private set; }
    public string? ImageUrl { get; private set; }

    private OrderDelivery() {
    }

    private OrderDelivery(
        Guid orderId,
        Guid driverId,
        GeoPointValue location,
        DateTime deliveredAt,
        string? comments,
        string? imageUrl
    ) : base(Guid.NewGuid()) {
        OrderId = orderId;
        DriverId = driverId;
        Location = location;
        DeliveredAt = deliveredAt;
        Comments = comments;
        ImageUrl = imageUrl;
    }

    public static OrderDelivery Create(
        Guid orderId,
        Guid driverId,
        GeoPointValue location,
        DateTime deliveredAt,
        string? comments,
        string? imageUrl
    ) {
        return new OrderDelivery(
            orderId,
            driverId,
            location,
            deliveredAt,
            comments,
            imageUrl
        );
    }
}
