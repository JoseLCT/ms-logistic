using Joselct.Communication.Contracts.Messages;

namespace MsLogistic.Application.Integration.Events.Outgoing;

public record OrderDeliveredMessage : IntegrationMessage {
    public Guid OrderId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime DeliveredAt { get; set; }
}
