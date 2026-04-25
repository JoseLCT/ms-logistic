using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Routes.GetRouteById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Routes;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Routes;

public class GetRouteByIdHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetRouteByIdHandler _handler;

	public GetRouteByIdHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetRouteByIdHandler(_dbContext);
	}

	public void Dispose() {
		_dbContext.Dispose();
		GC.SuppressFinalize(this);
	}

	private static Point CreatePoint(double longitude, double latitude) {
		return new Point(longitude, latitude) { SRID = 4326 };
	}

	private static RoutePersistenceModel CreateRoutePersistenceModel(
		Guid? id = null,
		Guid? batchId = null,
		Guid? deliveryZoneId = null,
		Guid? driverId = null,
		Point? originLocation = null,
		RouteStatusEnum status = RouteStatusEnum.Pending,
		DateTime? startedAt = null,
		DateTime? completedAt = null
	) {
		return new RoutePersistenceModel {
			Id = id ?? Guid.NewGuid(),
			BatchId = batchId ?? Guid.NewGuid(),
			DeliveryZoneId = deliveryZoneId ?? Guid.NewGuid(),
			DriverId = driverId,
			OriginLocation = originLocation ?? CreatePoint(0, 0),
			Status = status,
			StartedAt = startedAt,
			CompletedAt = completedAt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	private static OrderPersistenceModel CreateOrderForRoute(
		Guid routeId,
		int? deliverySequence,
		OrderStatusEnum status = OrderStatusEnum.Pending,
		Point? deliveryLocation = null
	) {
		return new OrderPersistenceModel {
			Id = Guid.NewGuid(),
			BatchId = Guid.NewGuid(),
			CustomerId = Guid.NewGuid(),
			RouteId = routeId,
			DeliverySequence = deliverySequence,
			Status = status,
			ScheduledDeliveryDate = DateTime.UtcNow.AddDays(1),
			DeliveryAddress = "Test Address",
			DeliveryLocation = deliveryLocation ?? CreatePoint(-68.10, -16.50),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithExistingRouteIdWithoutStops_ShouldReturnRouteWithAllPrimaryFieldsMapped() {
		// Arrange
		var batchId = Guid.NewGuid();
		var deliveryZoneId = Guid.NewGuid();
		var driverId = Guid.NewGuid();

		RoutePersistenceModel route = CreateRoutePersistenceModel(
			batchId: batchId,
			deliveryZoneId: deliveryZoneId,
			driverId: driverId,
			originLocation: CreatePoint(-68.15, -16.50),
			status: RouteStatusEnum.Pending
		);

		await _dbContext.Routes.AddAsync(route);
		await _dbContext.SaveChangesAsync();

		var query = new GetRouteByIdQuery(route.Id);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(route.Id);
		result.Value.BatchId.Should().Be(batchId);
		result.Value.DeliveryZoneId.Should().Be(deliveryZoneId);
		result.Value.DriverId.Should().Be(driverId);
		result.Value.Status.Should().Be(RouteStatusEnum.Pending);
		result.Value.StartedAt.Should().BeNull();
		result.Value.CompletedAt.Should().BeNull();

		result.Value.OriginLocation.Latitude.Should().Be(-16.50);
		result.Value.OriginLocation.Longitude.Should().Be(-68.15);

		result.Value.Stops.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithRouteWithoutDriver_ShouldMapDriverIdAsNull() {
		// Arrange
		RoutePersistenceModel route = CreateRoutePersistenceModel(
			driverId: null
		);

		await _dbContext.Routes.AddAsync(route);
		await _dbContext.SaveChangesAsync();

		var query = new GetRouteByIdQuery(route.Id);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value?.DriverId.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithCompletedRoute_ShouldMapStartedAtAndCompletedAt() {
		// Arrange
		DateTime startedAt = DateTime.UtcNow.AddHours(-3);
		DateTime completedAt = DateTime.UtcNow.AddHours(-1);

		RoutePersistenceModel route = CreateRoutePersistenceModel(
			status: RouteStatusEnum.Completed,
			startedAt: startedAt,
			completedAt: completedAt
		);

		await _dbContext.Routes.AddAsync(route);
		await _dbContext.SaveChangesAsync();

		var query = new GetRouteByIdQuery(route.Id);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Status.Should().Be(RouteStatusEnum.Completed);
		result.Value.StartedAt.Should().Be(startedAt);
		result.Value.CompletedAt.Should().Be(completedAt);
	}

	[Fact]
	public async Task Handle_WithRouteWithStops_ShouldMapStopsWithCoordinateSwap() {
		// Arrange
		RoutePersistenceModel route = CreateRoutePersistenceModel(
			originLocation: CreatePoint(-68.15, -16.50)
		);

		OrderPersistenceModel stop1 = CreateOrderForRoute(
			routeId: route.Id,
			deliverySequence: 1,
			status: OrderStatusEnum.Pending,
			deliveryLocation: CreatePoint(-68.10, -16.45)
		);
		OrderPersistenceModel stop2 = CreateOrderForRoute(
			routeId: route.Id,
			deliverySequence: 2,
			status: OrderStatusEnum.Pending,
			deliveryLocation: CreatePoint(-68.05, -16.40)
		);

		await _dbContext.Routes.AddAsync(route);
		await _dbContext.Orders.AddRangeAsync(stop1, stop2);
		await _dbContext.SaveChangesAsync();

		var query = new GetRouteByIdQuery(route.Id);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Stops.Should().HaveCount(2);

		result.Value.Stops[0].Id.Should().Be(stop1.Id);
		result.Value.Stops[0].DeliverySequence.Should().Be(1);
		result.Value.Stops[0].Status.Should().Be(OrderStatusEnum.Pending);
		result.Value.Stops[0].DeliveryLocation.Latitude.Should().Be(-16.45);
		result.Value.Stops[0].DeliveryLocation.Longitude.Should().Be(-68.10);

		result.Value.Stops[1].Id.Should().Be(stop2.Id);
		result.Value.Stops[1].DeliverySequence.Should().Be(2);
		result.Value.Stops[1].DeliveryLocation.Latitude.Should().Be(-16.40);
		result.Value.Stops[1].DeliveryLocation.Longitude.Should().Be(-68.05);
	}

	[Fact]
	public async Task Handle_WithStopsAddedOutOfOrder_ShouldReturnStopsOrderedByDeliverySequence() {
		// Arrange
		RoutePersistenceModel route = CreateRoutePersistenceModel();

		// Insert stops out of order
		OrderPersistenceModel stop3 = CreateOrderForRoute(routeId: route.Id, deliverySequence: 3);
		OrderPersistenceModel stop1 = CreateOrderForRoute(routeId: route.Id, deliverySequence: 1);
		OrderPersistenceModel stop2 = CreateOrderForRoute(routeId: route.Id, deliverySequence: 2);

		await _dbContext.Routes.AddAsync(route);
		await _dbContext.Orders.AddRangeAsync(stop3, stop1, stop2);
		await _dbContext.SaveChangesAsync();

		var query = new GetRouteByIdQuery(route.Id);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Stops.Should().HaveCount(3);
		result.Value.Stops.Select(s => s.DeliverySequence).Should()
			.ContainInOrder(1, 2, 3);
		result.Value.Stops[0].Id.Should().Be(stop1.Id);
		result.Value.Stops[1].Id.Should().Be(stop2.Id);
		result.Value.Stops[2].Id.Should().Be(stop3.Id);
	}

	[Fact]
	public async Task Handle_WithMultipleRoutesAndCrossOrders_ShouldOnlyReturnStopsOfRequestedRoute() {
		// Arrange
		RoutePersistenceModel route1 = CreateRoutePersistenceModel();
		RoutePersistenceModel route2 = CreateRoutePersistenceModel();

		OrderPersistenceModel route1Stop = CreateOrderForRoute(
			routeId: route1.Id,
			deliverySequence: 1
		);
		OrderPersistenceModel route2Stop1 = CreateOrderForRoute(
			routeId: route2.Id,
			deliverySequence: 1
		);
		OrderPersistenceModel route2Stop2 = CreateOrderForRoute(
			routeId: route2.Id,
			deliverySequence: 2
		);

		await _dbContext.Routes.AddRangeAsync(route1, route2);
		await _dbContext.Orders.AddRangeAsync(route1Stop, route2Stop1, route2Stop2);
		await _dbContext.SaveChangesAsync();

		var query = new GetRouteByIdQuery(route2.Id);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(route2.Id);
		result.Value.Stops.Should().HaveCount(2);
		result.Value.Stops.Select(s => s.Id).Should()
			.BeEquivalentTo([route2Stop1.Id, route2Stop2.Id]);
		result.Value.Stops.Select(s => s.Id).Should()
			.NotContain(route1Stop.Id);
	}

	[Fact]
	public async Task Handle_WithNonExistingRouteId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetRouteByIdQuery(nonExistingId);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Route", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingIdAndOtherRoutesInDb_ShouldReturnNotFoundError() {
		// Arrange
		RoutePersistenceModel route1 = CreateRoutePersistenceModel();
		RoutePersistenceModel route2 = CreateRoutePersistenceModel();

		await _dbContext.Routes.AddRangeAsync(route1, route2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetRouteByIdQuery(nonExistingId);

		// Act
		Result<RouteDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Route", nonExistingId));
	}
}
