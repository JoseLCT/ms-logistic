using Joselct.Communication.Contracts.Messages;

namespace MsLogistic.Application.Integration.Events.Incoming;

public record OrderCreatedMessage : IntegrationMessage {
    public Guid CustomerId { get; init; }
    public DateTime DeliveryDate { get; init; }
    public string DeliveryAddress { get; init; } = string.Empty;
    public DeliveryLocationMessage DeliveryLocation { get; init; } = new();
    public IReadOnlyList<OrderItemMessage> Items { get; init; } = [];
}

public record OrderItemMessage {
    public Guid RecipeId { get; init; }
    public int Quantity { get; init; }
}

public record DeliveryLocationMessage {
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
