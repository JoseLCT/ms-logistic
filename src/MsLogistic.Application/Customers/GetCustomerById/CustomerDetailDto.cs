namespace MsLogistic.Application.Customers.GetCustomerById;

public record CustomerDetailDto(
    Guid Id,
    string FullName,
    string PhoneNumber
);