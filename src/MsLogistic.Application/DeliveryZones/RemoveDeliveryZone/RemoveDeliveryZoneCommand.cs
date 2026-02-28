using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZones.RemoveDeliveryZone;

public record RemoveDeliveryZoneCommand(Guid Id) : IRequest<Result<Guid>>;
