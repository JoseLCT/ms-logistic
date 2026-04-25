using System.Text.Json.Serialization;
using Joselct.Communication.Contracts.Messages;

namespace MsLogistic.Application.Integration.Events.Incoming;

public record RecipeCreatedMessage : IntegrationMessage {
	[JsonPropertyName("RecetaId")]
	public Guid RecipeId { get; init; }

	[JsonPropertyName("Nombre")]
	public string Name { get; set; } = string.Empty;
}
