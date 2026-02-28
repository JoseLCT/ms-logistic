using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.Shared.ValueObjects;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class DeliveryZoneRepositoryTest : IDisposable {
    private readonly DomainDbContext _dbContext;
    private readonly DeliveryZoneRepository _repository;

    public DeliveryZoneRepositoryTest() {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DomainDbContext(options);
        _repository = new DeliveryZoneRepository(_dbContext);
    }

    public void Dispose() {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private static DeliveryZone CreateValidDeliveryZone(
        Guid? driverId = null,
        string code = "ABC-123",
        string name = "North Zone"
    ) {
        var points = new List<GeoPointValue>
        {
            GeoPointValue.Create(-17.7833, -63.1821),
            GeoPointValue.Create(-17.7833, -63.1621),
            GeoPointValue.Create(-17.7633, -63.1621),
            GeoPointValue.Create(-17.7633, -63.1821)
        };
        var boundaries = BoundariesValue.Create(points);

        return DeliveryZone.Create(
            driverId: driverId,
            code: code,
            name: name,
            boundaries: boundaries
        );
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenDeliveryZoneExists_ShouldReturnDeliveryZone() {
        // Arrange
        var zone = CreateValidDeliveryZone();
        await _dbContext.DeliveryZones.AddAsync(zone);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(zone.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(zone.Id);
        result.Code.Should().Be(zone.Code);
        result.Name.Should().Be(zone.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDeliveryZoneDoesNotExist_ShouldReturnNull() {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WhenDeliveryZonesExist_ShouldReturnAllDeliveryZones() {
        // Arrange
        var zone1 = CreateValidDeliveryZone(code: "ABC-123", name: "North Zone");
        var zone2 = CreateValidDeliveryZone(code: "XYZ-456", name: "South Zone");
        var zone3 = CreateValidDeliveryZone(code: "DEF-789", name: "East Zone");

        await _dbContext.DeliveryZones.AddRangeAsync(zone1, zone2, zone3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(z => z.Id == zone1.Id);
        result.Should().Contain(z => z.Id == zone2.Id);
        result.Should().Contain(z => z.Id == zone3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoDeliveryZones_ShouldReturnEmptyList() {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldAddDeliveryZoneToDatabase() {
        // Arrange
        var zone = CreateValidDeliveryZone();

        // Act
        await _repository.AddAsync(zone);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedZone = await _dbContext.DeliveryZones.FindAsync(zone.Id);
        savedZone.Should().NotBeNull();
        savedZone.Id.Should().Be(zone.Id);
        savedZone.Code.Should().Be(zone.Code);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateDeliveryZoneInDatabase() {
        // Arrange
        var zone = CreateValidDeliveryZone();
        await _dbContext.DeliveryZones.AddAsync(zone);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(zone).State = EntityState.Detached;

        // Act
        var zoneToUpdate = await _dbContext.DeliveryZones.FindAsync(zone.Id);
        zoneToUpdate!.SetName("Updated Zone");
        _repository.Update(zoneToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedZone = await _dbContext.DeliveryZones.FindAsync(zone.Id);
        updatedZone!.Name.Should().Be("Updated Zone");
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldDeleteDeliveryZoneFromDatabase() {
        // Arrange
        var zone = CreateValidDeliveryZone();
        await _dbContext.DeliveryZones.AddAsync(zone);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(zone);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedZone = await _dbContext.DeliveryZones.FindAsync(zone.Id);
        deletedZone.Should().BeNull();
    }

    #endregion
}
