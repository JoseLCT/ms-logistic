using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryPerson.Errors;
using MsLogistic.Domain.DeliveryPerson.Types;

namespace MsLogistic.Domain.DeliveryPerson.Entities;

public class DeliveryPerson : AggregateRoot
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DeliveryPersonStatusType Status { get; private set; }

    private DeliveryPerson()
    {
    }

    public DeliveryPerson(string name) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(DeliveryPersonErrors.NameIsRequired);
        }

        Name = name;
        IsActive = true;
        Status = DeliveryPersonStatusType.Available;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(DeliveryPersonErrors.NameIsRequired);
        }

        Name = name;
        MarkAsUpdated();
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
        MarkAsUpdated();
    }

    public void SetStatus(DeliveryPersonStatusType status)
    {
        Status = status;
        MarkAsUpdated();
    }
}