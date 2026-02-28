using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.DeliveryZones.GetDeliveryZoneById;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.DeliveryZones;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.DeliveryZones;

public class GetDeliveryZoneByIdHandlerTest : IDisposable {
    private readonly PersistenceDbContext _dbContext;
    private readonly GetDeliveryZoneByIdHandler _handler;

    public GetDeliveryZoneByIdHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetDeliveryZoneByIdHandler(_dbContext);
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
    public async Task Handle_WithExistingDeliveryZoneId_ShouldReturnDeliveryZone() {
        // Arrange
        var newDeliveryZone = CreateDeliveryZonePersistenceModel();

        await _dbContext.DeliveryZones.AddAsync(newDeliveryZone);
        await _dbContext.SaveChangesAsync();

        var query = new GetDeliveryZoneByIdQuery(newDeliveryZone.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newDeliveryZone.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistingDeliveryZoneId_ShouldReturnNotFoundError() {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var query = new GetDeliveryZoneByIdQuery(nonExistingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommonErrors.NotFoundById("DeliveryZone", nonExistingId));
    }

    [Fact]
    public async Task Handle_WithMultipleDeliveryZones_ShouldReturnCorrectDeliveryZone() {
        // Arrange
        var deliveryZone1 = CreateDeliveryZonePersistenceModel();
        var deliveryZone2 = CreateDeliveryZonePersistenceModel();

        await _dbContext.DeliveryZones.AddRangeAsync(deliveryZone1, deliveryZone2);
        await _dbContext.SaveChangesAsync();

        var query = new GetDeliveryZoneByIdQuery(deliveryZone2.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(deliveryZone2.Id);
    }
}
