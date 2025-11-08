using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZone.UpdateDeliveryZone;

public record UpdateDeliveryZoneCommand(
    Guid Id,
    Guid? DeliveryPersonId,
    string Code,
    string Name,
    IEnumerable<CoordinateDto> Boundaries
) : IRequest<Result<Guid>>;