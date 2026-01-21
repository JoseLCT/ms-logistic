using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZones.UpdateDeliveryZone;

public record UpdateDeliveryZoneCommand(
    Guid Id,
    Guid? DriverId,
    string Code,
    string Name,
    IEnumerable<CoordinateDto> Boundaries
) : IRequest<Result<Guid>>;