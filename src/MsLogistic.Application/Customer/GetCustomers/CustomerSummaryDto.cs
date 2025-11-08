namespace MsLogistic.Application.Customer.GetCustomers;

public record CustomerSummaryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string PhoneNumber { get; init; }
}