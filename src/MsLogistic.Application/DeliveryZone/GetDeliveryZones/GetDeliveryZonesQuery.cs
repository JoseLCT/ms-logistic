using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZone.GetDeliveryZones;

public record GetDeliveryZonesQuery() : IRequest<Result<ICollection<DeliveryZoneSummaryDto>>>;