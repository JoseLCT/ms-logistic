using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Drivers.GetAllDrivers;
using MsLogistic.Domain.Drivers.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Drivers;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Drivers;

public class GetAllDriversHandlerTest : IDisposable {
    private readonly PersistenceDbContext _dbContext;
    private readonly GetAllDriversHandler _handler;

    public GetAllDriversHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetAllDriversHandler(_dbContext);
    }

    public void Dispose() {
        _dbContext.Dispose();
    }

    private static DriverPersistenceModel CreateDriverPersistenceModel(
        Guid? id = null,
        string fullName = "Jane Smith",
        bool isActive = true,
        DriverStatusEnum status = DriverStatusEnum.Available
    ) {
        return new DriverPersistenceModel {
            Id = id ?? Guid.NewGuid(),
            FullName = fullName,
            IsActive = isActive,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithNoDrivers_ShouldReturnEmptyList() {
        // Arrange
        var query = new GetAllDriversQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleDriver_ShouldReturnListWithOneDriver() {
        // Arrange
        var driver = CreateDriverPersistenceModel();

        await _dbContext.Drivers.AddAsync(driver);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllDriversQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(driver.Id);
        result.Value[0].FullName.Should().Be(driver.FullName);
    }

    [Fact]
    public async Task Handle_WithMultipleDrivers_ShouldReturnAllDrivers() {
        // Arrange
        var driver1 = CreateDriverPersistenceModel();
        var driver2 = CreateDriverPersistenceModel();

        await _dbContext.Drivers.AddRangeAsync(driver1, driver2);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllDriversQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }
}
