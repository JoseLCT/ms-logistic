using Joselct.Communication.Contracts.Messages;

namespace MsLogistic.Application.Integration.Events.Incoming;

public record OrderBatchCompletedMessage : IntegrationMessage {
	public string LastPackageCompletedId { get; init; } = string.Empty;
}
