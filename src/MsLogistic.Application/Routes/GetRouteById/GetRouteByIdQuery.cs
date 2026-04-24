using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Routes.GetRouteById;

public record GetRouteByIdQuery(Guid Id) : IRequest<Result<RouteDetailDto>>;
