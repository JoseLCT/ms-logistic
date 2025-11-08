using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZone.GetDeliveryZone;

public record GetDeliveryZoneQuery(Guid Id) : IRequest<Result<DeliveryZoneDetailDto>>;