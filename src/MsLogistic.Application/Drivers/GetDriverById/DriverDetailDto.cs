using MsLogistic.Domain.Drivers.Enums;

namespace MsLogistic.Application.Drivers.GetDriverById;

public record DriverDetailDto(
    Guid Id,
    string FullName,
    bool IsActive,
    DriverStatusEnum Status
);
