using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;

public record GetAllDeliveryZonesQuery() : IRequest<Result<IReadOnlyList<DeliveryZoneSummaryDto>>>;