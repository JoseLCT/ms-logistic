using MsLogistic.Domain.Route.Types;

namespace MsLogistic.Application.Route.GetRoutes;

public record RouteSummaryDto
{
    public Guid Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public RouteStatusType Status { get; set; }
}