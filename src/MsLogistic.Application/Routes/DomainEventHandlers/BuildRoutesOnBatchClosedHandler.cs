using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsLogistic.Application.Abstractions.Options;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Batches.Events;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Logistics.ValueObjects;
using MsLogistic.Domain.Orders.Entities;
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
	private readonly IOptions<LogisticsOptions> _logisticsOptions;
	private readonly ILogger<BuildRoutesOnBatchClosedHandler> _logger;

	public BuildRoutesOnBatchClosedHandler(
		IOrderRepository orderRepository,
		IDeliveryZoneRepository deliveryZoneRepository,
		IRouteRepository routeRepository,
		IRouteCalculator routeCalculator,
		IUnitOfWork unitOfWork,
		IOptions<LogisticsOptions> logisticsOptions,
		ILogger<BuildRoutesOnBatchClosedHandler> logger
	) {
		_orderRepository = orderRepository;
		_deliveryZoneRepository = deliveryZoneRepository;
		_routeRepository = routeRepository;
		_routeCalculator = routeCalculator;
		_unitOfWork = unitOfWork;
		_logisticsOptions = logisticsOptions;
		_logger = logger;
	}

	public async Task Handle(BatchClosed notification, CancellationToken ct) {
		IReadOnlyList<Order> orders = await _orderRepository.GetByBatchIdAsync(notification.BatchId, ct);

		if (orders.Count == 0) {
			_logger.LogWarning("Batch {BatchId} closed with no orders.", notification.BatchId);
			return;
		}

		IReadOnlyList<DeliveryZone> deliveryZones = await _deliveryZoneRepository.GetAllAsync(ct);

		var origin = GeoPointValue.Create(
			_logisticsOptions.Value.Depot.Latitude,
			_logisticsOptions.Value.Depot.Longitude
		);

		var ordersByZone = orders
			.Select(order => new {
				Order = order,
				Zone = deliveryZones.FirstOrDefault(z => z.Boundaries.Contains(order.DeliveryLocation))
			})
			.Where(x => x.Zone != null)
			.GroupBy(x => x.Zone!.Id)
			.ToList();

		int unassignedCount = orders.Count - ordersByZone.Sum(g => g.Count());
		if (unassignedCount > 0) {
			_logger.LogWarning(
				"{Count} orders from batch {BatchId} could not be assigned to any delivery zone.",
				unassignedCount,
				notification.BatchId
			);
		}

		foreach (var zoneGroup in ordersByZone) {
			Guid zoneId = zoneGroup.Key;
			var zoneOrders = zoneGroup.Select(x => x.Order).ToList();
			DeliveryZone zone = deliveryZones.First(z => z.Id == zoneId);

			if (zoneOrders.Count == 1) {
				var singleRoute = Route.Create(notification.BatchId, zoneId, zone.DriverId, origin);
				await _routeRepository.AddAsync(singleRoute, ct);
				zoneOrders[0].AssignToRoute(singleRoute.Id, deliverySequence: 1);
				continue;
			}

			var waypoints = zoneOrders
				.Select(o => new Waypoint(o.Id, o.DeliveryLocation))
				.ToList();

			Result<OrderedRoute> routeResult = await _routeCalculator.CalculateOrderAsync(origin, waypoints, ct);

			if (routeResult.IsFailure || routeResult.Value is null) {
				_logger.LogError(
					"Failed to calculate route for zone {ZoneId} in batch {BatchId}: {Error}",
					zoneId,
					notification.BatchId,
					routeResult.Error?.Message
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

			foreach (OrderedWaypoint orderedWaypoint in routeResult.Value.Waypoints) {
				Order order = zoneOrders.First(o => o.Id == orderedWaypoint.WaypointId);
				order.AssignToRoute(route.Id, orderedWaypoint.Sequence);
			}
		}

		await _unitOfWork.CommitAsync(ct);
	}
}
