using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetAllDeliveryZones;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.DeliveryZones;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.DeliveryZones;

public class GetAllDeliveryZonesHandlerTest : IDisposable {
    private readonly PersistenceDbContext _dbContext;
    private readonly GetAllDeliveryZonesHandler _handler;

    public GetAllDeliveryZonesHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetAllDeliveryZonesHandler(_dbContext);
    }

    public void Dispose() {
        _dbContext.Dispose();
    }

    private static DeliveryZonePersistenceModel CreateDeliveryZonePersistenceModel(
        Guid? id = null,
        Guid? driverId = null,
        string code = "ABC-123",
        string name = "Zone A",
        Polygon? boundaries = null
    ) {
        return new DeliveryZonePersistenceModel {
            Id = id ?? Guid.NewGuid(),
            DriverId = driverId,
            Code = code,
            Name = name,
            Boundaries = boundaries ?? new Polygon(new LinearRing(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 1),
                new Coordinate(1, 1),
                new Coordinate(1, 0),
                new Coordinate(0, 0)
            })),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithNoDeliveryZones_ShouldReturnEmptyList() {
        // Arrange
        var query = new GetAllDeliveryZonesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleDeliveryZone_ShouldReturnListWithOneDeliveryZone() {
        // Arrange
        var deliveryZone = CreateDeliveryZonePersistenceModel();

        await _dbContext.DeliveryZones.AddAsync(deliveryZone);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllDeliveryZonesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(deliveryZone.Id);
        result.Value[0].Code.Should().Be(deliveryZone.Code);
    }

    [Fact]
    public async Task Handle_WithMultipleDeliveryZones_ShouldReturnAllDeliveryZones() {
        // Arrange
        var deliveryZone1 = CreateDeliveryZonePersistenceModel();
        var deliveryZone2 = CreateDeliveryZonePersistenceModel();

        await _dbContext.DeliveryZones.AddRangeAsync(deliveryZone1, deliveryZone2);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllDeliveryZonesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
