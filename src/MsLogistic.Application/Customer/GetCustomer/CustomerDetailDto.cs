namespace MsLogistic.Application.Customer.GetCustomer;

public record CustomerDetailDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string PhoneNumber { get; init; }
}