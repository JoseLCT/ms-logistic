using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZones.CreateDeliveryZone;

public record CreateDeliveryZoneCommand(
    Guid? DriverId,
    string Code,
    string Name,
    IReadOnlyList<CoordinateDto> Boundaries
) : IRequest<Result<Guid>>;