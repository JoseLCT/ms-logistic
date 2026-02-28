using System.Text.Json.Serialization;

namespace MsLogistic.WebApi.Contracts.V1.Customers;

public record UpdateCustomerContract {
    [property: JsonPropertyName("full_name")]
    public required string FullName { get; init; }

    [property: JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; init; }
}
