using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class DriverRepositoryTest : IDisposable {
    private readonly DomainDbContext _dbContext;
    private readonly DriverRepository _repository;

    public DriverRepositoryTest() {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DomainDbContext(options);
        _repository = new DriverRepository(_dbContext);
    }

    public void Dispose() {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private static Driver CreateValidDriver(string fullName = "Juan Perez") {
        return Driver.Create(fullName: fullName);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenDriverExists_ShouldReturnDriver() {
        // Arrange
        var driver = CreateValidDriver();
        await _dbContext.Drivers.AddAsync(driver);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(driver.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(driver.Id);
        result.FullName.Should().Be(driver.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDriverDoesNotExist_ShouldReturnNull() {
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
    public async Task GetAllAsync_WhenDriversExist_ShouldReturnAllDrivers() {
        // Arrange
        var driver1 = CreateValidDriver("Luis Rodríguez");
        var driver2 = CreateValidDriver("Ana Martínez");
        var driver3 = CreateValidDriver("Carlos Gómez");

        await _dbContext.Drivers.AddRangeAsync(driver1, driver2, driver3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(d => d.Id == driver1.Id);
        result.Should().Contain(d => d.Id == driver2.Id);
        result.Should().Contain(d => d.Id == driver3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoDrivers_ShouldReturnEmptyList() {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldAddDriverToDatabase() {
        // Arrange
        var driver = CreateValidDriver();

        // Act
        await _repository.AddAsync(driver);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedDriver = await _dbContext.Drivers.FindAsync(driver.Id);
        savedDriver.Should().NotBeNull();
        savedDriver.Id.Should().Be(driver.Id);
        savedDriver.FullName.Should().Be(driver.FullName);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateDriverInDatabase() {
        // Arrange
        var driver = CreateValidDriver();
        await _dbContext.Drivers.AddAsync(driver);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(driver).State = EntityState.Detached;

        // Act
        var driverToUpdate = await _dbContext.Drivers.FindAsync(driver.Id);
        driverToUpdate!.SetIsActive(false);
        driverToUpdate.SetFullName("Pedro Sanchez");
        _repository.Update(driverToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedDriver = await _dbContext.Drivers.FindAsync(driver.Id);
        updatedDriver!.IsActive.Should().BeFalse();
        updatedDriver.FullName.Should().Be("Pedro Sanchez");
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldDeleteDriverFromDatabase() {
        // Arrange
        var driver = CreateValidDriver();
        await _dbContext.Drivers.AddAsync(driver);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(driver);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedDriver = await _dbContext.Drivers.FindAsync(driver.Id);
        deletedDriver.Should().BeNull();
    }

    #endregion
}
