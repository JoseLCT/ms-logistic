using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Routes.GetAllRoutes;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Routes;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Routes;

public class GetAllRoutesHandlerTest : IDisposable {
    private readonly PersistenceDbContext _context;
    private readonly GetAllRoutesHandler _handler;

    public GetAllRoutesHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new PersistenceDbContext(options);
        _handler = new GetAllRoutesHandler(_context);
    }

    public void Dispose() {
        _context.Dispose();
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
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleRoute_ShouldReturnListWithOneRoute() {
        // Arrange
        var route = CreateRoutePersistenceModel();

        await _context.Routes.AddAsync(route);
        await _context.SaveChangesAsync();

        var query = new GetAllRoutesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(route.Id);
    }

    [Fact]
    public async Task Handle_WithMultipleRoutes_ShouldReturnAllRoutes() {
        // Arrange
        var route1 = CreateRoutePersistenceModel();
        var route2 = CreateRoutePersistenceModel();

        await _context.Routes.AddRangeAsync(route1, route2);
        await _context.SaveChangesAsync();

        var query = new GetAllRoutesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }
}
