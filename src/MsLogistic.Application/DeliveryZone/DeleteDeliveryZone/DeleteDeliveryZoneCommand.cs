using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZone.DeleteDeliveryZone;

public record DeleteDeliveryZoneCommand(Guid Id) : IRequest<Result<Guid>>;