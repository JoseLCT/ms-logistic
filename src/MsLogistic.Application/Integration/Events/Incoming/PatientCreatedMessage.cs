using Joselct.Communication.Contracts.Messages;

namespace MsLogistic.Application.Integration.Events.Incoming;

public record PatientCreatedMessage : IntegrationMessage {
    public Guid PatientId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
