using Joselct.Communication.Contracts.Messages;

namespace MsLogistic.Application.Integration.Events.Incoming;

public record PatientCreatedMessage : IntegrationMessage {
    public Guid PatientId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string MiddleName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = "+591670000000";
}
