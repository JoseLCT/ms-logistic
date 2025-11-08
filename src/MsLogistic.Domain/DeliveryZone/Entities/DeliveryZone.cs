using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZone.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.DeliveryZone.Entities;

public partial class DeliveryZone : AggregateRoot
{
    public Guid? DeliveryPersonId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public ZoneBoundaryValue Boundaries { get; private set; }

    private DeliveryZone()
    {
    }

    public DeliveryZone(
        Guid? deliveryPersonId,
        string code,
        string name,
        ZoneBoundaryValue boundaries) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(DeliveryZoneErrors.CodeIsRequired);
        }

        if (!CodeRegex().IsMatch(code))
        {
            throw new DomainException(DeliveryZoneErrors.CodeFormatIsInvalid);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(DeliveryZoneErrors.NameIsRequired);
        }

        DeliveryPersonId = deliveryPersonId;
        Code = code;
        Name = name;
        Boundaries = boundaries;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetDeliveryPersonId(Guid? deliveryPersonId)
    {
        DeliveryPersonId = deliveryPersonId;
        MarkAsUpdated();
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(DeliveryZoneErrors.CodeIsRequired);
        }

        if (!CodeRegex().IsMatch(code))
        {
            throw new DomainException(DeliveryZoneErrors.CodeFormatIsInvalid);
        }

        Code = code;
        MarkAsUpdated();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(DeliveryZoneErrors.NameIsRequired);
        }

        Name = name;
        MarkAsUpdated();
    }

    public void SetBoundaries(ZoneBoundaryValue boundaries)
    {
        Boundaries = boundaries;
        MarkAsUpdated();
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^[A-Z]{3}-\d{3}$")]
    private static partial System.Text.RegularExpressions.Regex CodeRegex();
}