using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Enums;
using MsLogistic.Domain.Drivers.Errors;

namespace MsLogistic.Domain.Drivers.Entities;

public class Driver : AggregateRoot {
    public string FullName { get; private set; }
    public bool IsActive { get; private set; }
    public DriverStatusEnum Status { get; private set; }

    private Driver() {
    }

    private Driver(string fullName)
        : base(Guid.NewGuid()) {
        FullName = fullName;
        IsActive = true;
        Status = DriverStatusEnum.Available;
    }

    public static Driver Create(string fullName) {
        ValidateFullName(fullName);
        return new Driver(fullName);
    }

    public void SetFullName(string fullName) {
        ValidateFullName(fullName);
        FullName = fullName;
        MarkAsUpdated();
    }

    public void SetIsActive(bool isActive) {
        IsActive = isActive;
        MarkAsUpdated();
    }

    public void SetStatus(DriverStatusEnum status) {
        Status = status;
        MarkAsUpdated();
    }

    private static void ValidateFullName(string fullName) {
        if (string.IsNullOrWhiteSpace(fullName)) {
            throw new DomainException(DriverErrors.FullNameIsRequired);
        }
    }
}
