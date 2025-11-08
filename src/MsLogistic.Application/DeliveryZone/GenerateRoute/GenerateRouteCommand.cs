using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.DeliveryZone.GenerateRoute;

public record GenerateRouteCommand(Guid DeliveryZoneId) : IRequest<Result<Guid>>;