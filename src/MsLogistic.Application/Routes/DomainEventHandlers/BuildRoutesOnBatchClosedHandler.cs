using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsLogistic.Application.Abstractions.Options;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Batches.Events;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Logistics.ValueObjects;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Routes.DomainEventHandlers;

internal class BuildRoutesOnBatchClosedHandler : INotificationHandler<BatchClosed> {
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryZoneRepository _deliveryZoneRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteCalculator _routeCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<DepotOptions> _depotOptions;
    private readonly ILogger<BuildRoutesOnBatchClosedHandler> _logger;

    public BuildRoutesOnBatchClosedHandler(
        IOrderRepository orderRepository,
        IDeliveryZoneRepository deliveryZoneRepository,
        IRouteRepository routeRepository,
        IRouteCalculator routeCalculator,
        IUnitOfWork unitOfWork,
        IOptions<DepotOptions> depotOptions,
        ILogger<BuildRoutesOnBatchClosedHandler> logger
    ) {
        _orderRepository = orderRepository;
        _deliveryZoneRepository = deliveryZoneRepository;
        _routeRepository = routeRepository;
        _routeCalculator = routeCalculator;
        _unitOfWork = unitOfWork;
        _depotOptions = depotOptions;
        _logger = logger;
    }

    public async Task Handle(BatchClosed notification, CancellationToken ct) {
        var orders = await _orderRepository.GetByBatchIdAsync(notification.BatchId, ct);

        if (orders.Count == 0) {
            _logger.LogWarning("Batch {BatchId} closed with no orders.", notification.BatchId);
            return;
        }

        var deliveryZones = await _deliveryZoneRepository.GetAllAsync(ct);

        var origin = GeoPointValue.Create(
            _depotOptions.Value.Latitude,
            _depotOptions.Value.Longitude
        );

        var ordersByZone = orders
            .Select(order => new {
                Order = order,
                Zone = deliveryZones.FirstOrDefault(z => z.Boundaries.Contains(order.DeliveryLocation))
            })
            .Where(x => x.Zone != null)
            .GroupBy(x => x.Zone!.Id)
            .ToList();

        var unassignedCount = orders.Count - ordersByZone.Sum(g => g.Count());
        if (unassignedCount > 0) {
            _logger.LogWarning(
                "{Count} orders from batch {BatchId} could not be assigned to any delivery zone.",
                unassignedCount,
                notification.BatchId
            );
        }

        foreach (var zoneGroup in ordersByZone) {
            var zoneId = zoneGroup.Key;
            var zoneOrders = zoneGroup.Select(x => x.Order).ToList();
            var zone = deliveryZones.First(z => z.Id == zoneId);

            var waypoints = zoneOrders
                .Select(o => new Waypoint(o.Id, o.DeliveryLocation))
                .ToList();

            var routeResult = await _routeCalculator.CalculateOrderAsync(origin, waypoints, ct);

            if (routeResult.IsFailure) {
                _logger.LogError(
                    "Failed to calculate route for zone {ZoneId} in batch {BatchId}: {Error}",
                    zoneId,
                    notification.BatchId,
                    routeResult.Error.Message
                );
                continue;
            }

            var route = Route.Create(
                notification.BatchId,
                zoneId,
                zone.DriverId,
                origin
            );

            await _routeRepository.AddAsync(route, ct);

            foreach (var orderedWaypoint in routeResult.Value.Waypoints) {
                var order = zoneOrders.First(o => o.Id == orderedWaypoint.WaypointId);
                order.AssignToRoute(route.Id, orderedWaypoint.Sequence);
                _orderRepository.Update(order);
            }
        }

        await _unitOfWork.CommitAsync(ct);
    }
}
