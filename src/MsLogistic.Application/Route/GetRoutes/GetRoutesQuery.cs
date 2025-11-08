using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Route.GetRoutes;

public record GetRoutesQuery() : IRequest<Result<ICollection<RouteSummaryDto>>>;