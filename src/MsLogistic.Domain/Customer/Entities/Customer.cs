using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customer.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.Customer.Entities;

public class Customer : AggregateRoot
{
    public string Name { get; private set; }
    public PhoneNumberValue PhoneNumber { get; private set; }

    private Customer()
    {
    }

    public Customer(string name, PhoneNumberValue phoneNumber) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(CustomerErrors.NameIsRequired);
        }

        Name = name;
        PhoneNumber = phoneNumber;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(CustomerErrors.NameIsRequired);
        }

        Name = name;
        MarkAsUpdated();
    }

    public void SetPhoneNumber(PhoneNumberValue phoneNumber)
    {
        PhoneNumber = phoneNumber;
        MarkAsUpdated();
    }
}