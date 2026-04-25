using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Routes.GetAllRoutes;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Routes;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Routes;

public class GetAllRoutesHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetAllRoutesHandler _handler;

	public GetAllRoutesHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetAllRoutesHandler(_dbContext);
	}

	public void Dispose() {
		_dbContext.Dispose();
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
			OriginLocation = originLocation ?? new Point(0, 0) { SRID = 4326 },
			Status = status,
			StartedAt = startedAt,
			CompletedAt = completedAt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithNoRoutes_ShouldReturnEmptyList() {
		// Arrange
		var query = new GetAllRoutesQuery();

		// Act
		Result<IReadOnlyList<RouteSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithSinglePendingRoute_ShouldReturnListWithOneRoute() {
		// Arrange
		RoutePersistenceModel route = CreateRoutePersistenceModel(
			status: RouteStatusEnum.Pending
		);

		await _dbContext.Routes.AddAsync(route);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllRoutesQuery();

		// Act
		Result<IReadOnlyList<RouteSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(1);
		result.Value[0].Id.Should().Be(route.Id);
		result.Value[0].Status.Should().Be(RouteStatusEnum.Pending);
		result.Value[0].StartedAt.Should().BeNull();
		result.Value[0].CompletedAt.Should().BeNull();
		result.Value[0].CreatedAt.Should().Be(route.CreatedAt);
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

		var query = new GetAllRoutesQuery();

		// Act
		Result<IReadOnlyList<RouteSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value[0].Status.Should().Be(RouteStatusEnum.Completed);
		result.Value[0].StartedAt.Should().Be(startedAt);
		result.Value[0].CompletedAt.Should().Be(completedAt);
	}

	[Fact]
	public async Task Handle_WithMultipleRoutes_ShouldReturnAllRoutes() {
		// Arrange
		RoutePersistenceModel route1 = CreateRoutePersistenceModel(
			status: RouteStatusEnum.Pending
		);
		RoutePersistenceModel route2 = CreateRoutePersistenceModel(
			status: RouteStatusEnum.InProgress,
			startedAt: DateTime.UtcNow.AddHours(-1)
		);
		RoutePersistenceModel route3 = CreateRoutePersistenceModel(
			status: RouteStatusEnum.Completed,
			startedAt: DateTime.UtcNow.AddHours(-3),
			completedAt: DateTime.UtcNow.AddHours(-1)
		);

		await _dbContext.Routes.AddRangeAsync(route1, route2, route3);
		await _dbContext.SaveChangesAsync();

		var query = new GetAllRoutesQuery();

		// Act
		Result<IReadOnlyList<RouteSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(3);
		result.Value.Select(r => r.Id).Should()
			.BeEquivalentTo([route1.Id, route2.Id, route3.Id]);
		result.Value.Select(r => r.Status).Should()
			.BeEquivalentTo([
				RouteStatusEnum.Pending,
				RouteStatusEnum.InProgress,
				RouteStatusEnum.Completed
			]);
	}
}
