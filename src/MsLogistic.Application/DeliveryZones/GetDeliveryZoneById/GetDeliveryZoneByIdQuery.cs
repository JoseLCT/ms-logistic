using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;

public record GetDeliveryZoneByIdQuery(Guid Id) : IRequest<Result<DeliveryZoneDetailDto>>;