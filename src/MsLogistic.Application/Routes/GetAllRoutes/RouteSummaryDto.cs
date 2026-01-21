using MsLogistic.Domain.Routes.Enums;

namespace MsLogistic.Application.Routes.GetAllRoutes;

public record RouteSummaryDto(
    Guid Id,
    RouteStatusEnum Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt
);