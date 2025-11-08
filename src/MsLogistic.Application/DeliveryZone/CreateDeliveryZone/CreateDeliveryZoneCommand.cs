using MediatR;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.DeliveryZone.CreateDeliveryZone;

public record CreateDeliveryZoneCommand(
    Guid? DeliveryPersonId,
    string Code,
    string Name,
    IEnumerable<CoordinateDto> Boundaries
) : IRequest<Result<Guid>>;