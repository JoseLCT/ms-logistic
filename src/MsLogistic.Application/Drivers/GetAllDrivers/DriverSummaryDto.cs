using MsLogistic.Domain.Drivers.Enums;

namespace MsLogistic.Application.Drivers.GetAllDrivers;

public record DriverSummaryDto(
    Guid Id,
    string FullName,
    bool IsActive,
    DriverStatusEnum Status
);