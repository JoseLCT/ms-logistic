using MsLogistic.Application.Shared.DTOs;

namespace MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;

public record DeliveryZoneDetailDto(
    Guid Id,
    Guid? DriverId,
    string Code,
    string Name,
    ICollection<CoordinateDto> Boundaries
);