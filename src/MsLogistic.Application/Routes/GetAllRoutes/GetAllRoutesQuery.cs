using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Routes.GetAllRoutes;

public record GetAllRoutesQuery() : IRequest<Result<IReadOnlyList<RouteSummaryDto>>>;