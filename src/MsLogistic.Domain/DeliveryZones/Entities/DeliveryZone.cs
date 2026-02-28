using System.Text.RegularExpressions;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Errors;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Domain.DeliveryZones.Entities;

public partial class DeliveryZone : AggregateRoot {
    public Guid? DriverId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public BoundariesValue Boundaries { get; private set; }

    private DeliveryZone() {
    }

    private DeliveryZone(
        Guid? driverId,
        string code,
        string name,
        BoundariesValue boundaries
    ) : base(Guid.NewGuid()) {
        DriverId = driverId;
        Code = code;
        Name = name;
        Boundaries = boundaries;
    }

    public static DeliveryZone Create(
        Guid? driverId,
        string code,
        string name,
        BoundariesValue boundaries
    ) {
        ValidateCode(code);
        ValidateName(name);

        return new DeliveryZone(
            driverId,
            code,
            name,
            boundaries
        );
    }

    public void SetDriverId(Guid? driverId) {
        DriverId = driverId;
        MarkAsUpdated();
    }

    public void SetCode(string code) {
        ValidateCode(code);
        Code = code;
        MarkAsUpdated();
    }

    public void SetName(string name) {
        ValidateName(name);
        Name = name;
        MarkAsUpdated();
    }

    public void SetBoundaries(BoundariesValue boundaries) {
        Boundaries = boundaries;
        MarkAsUpdated();
    }

    private static void ValidateCode(string code) {
        if (string.IsNullOrWhiteSpace(code)) {
            throw new DomainException(DeliveryZoneErrors.CodeIsRequired);
        }

        if (!CodeRegex().IsMatch(code)) {
            throw new DomainException(DeliveryZoneErrors.CodeFormatIsInvalid);
        }
    }

    private static void ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new DomainException(DeliveryZoneErrors.NameIsRequired);
        }
    }

    [GeneratedRegex(@"^[A-Z]{3}-\d{3}$")]
    private static partial Regex CodeRegex();
}
