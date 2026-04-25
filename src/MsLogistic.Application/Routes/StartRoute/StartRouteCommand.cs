using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Routes.StartRoute;

public record StartRouteCommand(Guid Id) : IRequest<Result>;
