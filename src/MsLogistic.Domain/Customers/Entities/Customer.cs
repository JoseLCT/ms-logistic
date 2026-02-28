using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Customers.Entities;

public class Customer : AggregateRoot {
    public string FullName { get; private set; }
    public PhoneNumberValue? PhoneNumber { get; private set; }

    private Customer() {
    }

    private Customer(string fullName, PhoneNumberValue? phoneNumber)
        : base(Guid.NewGuid()) {
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public static Customer Create(string fullName, PhoneNumberValue? phoneNumber) {
        ValidateFullName(fullName);
        return new Customer(fullName, phoneNumber);
    }

    public void SetFullName(string fullName) {
        ValidateFullName(fullName);
        FullName = fullName;
        MarkAsUpdated();
    }

    public void SetPhoneNumber(PhoneNumberValue? phoneNumber) {
        PhoneNumber = phoneNumber;
        MarkAsUpdated();
    }

    private static void ValidateFullName(string fullName) {
        if (string.IsNullOrWhiteSpace(fullName)) {
            throw new DomainException(CustomerErrors.FullNameIsRequired);
        }
    }
}
