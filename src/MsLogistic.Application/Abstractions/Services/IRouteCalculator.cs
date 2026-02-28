using MsLogistic.Core.Results;
using MsLogistic.Domain.Logistics.ValueObjects;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Abstractions.Services;

public interface IRouteCalculator {
    Task<Result<OrderedRoute>> CalculateOrderAsync(
        GeoPointValue origin,
        IReadOnlyCollection<Waypoint> waypoints,
        CancellationToken ct
    );
}
