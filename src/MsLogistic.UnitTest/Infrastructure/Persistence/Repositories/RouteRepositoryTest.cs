using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Domain.Shared.ValueObjects;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class RouteRepositoryTest : IDisposable
{
    private readonly DomainDbContext _dbContext;
    private readonly RouteRepository _repository;

    public RouteRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DomainDbContext(options);
        _repository = new RouteRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private static Route CreateValidRoute(
        Guid? driverId = null,
        GeoPointValue? originLocation = null
    )
    {
        return Route.Create(
            batchId: Guid.NewGuid(),
            deliveryZoneId: Guid.NewGuid(),
            driverId: driverId,
            originLocation: originLocation ?? GeoPointValue.Create(-17.7833, -63.1821)
        );
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenRouteExists_ShouldReturnRoute()
    {
        // Arrange
        var route = CreateValidRoute();
        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(route.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(route.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRouteDoesNotExist_ShouldReturnNull()
    {
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
    public async Task GetAllAsync_WhenRoutesExist_ShouldReturnAllRoutes()
    {
        // Arrange
        var route1 = CreateValidRoute();
        var route2 = CreateValidRoute();
        var route3 = CreateValidRoute();

        await _dbContext.Routes.AddRangeAsync(route1, route2, route3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Id == route1.Id);
        result.Should().Contain(r => r.Id == route2.Id);
        result.Should().Contain(r => r.Id == route3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoRoutes_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldAddRouteToDatabase()
    {
        // Arrange
        var route = CreateValidRoute();

        // Act
        await _repository.AddAsync(route);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedRoute = await _dbContext.Routes.FindAsync(route.Id);
        savedRoute.Should().NotBeNull();
        savedRoute.Id.Should().Be(route.Id);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateRouteInDatabase()
    {
        // Arrange
        var route = CreateValidRoute();
        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(route).State = EntityState.Detached;

        // Act
        var routeToUpdate = await _dbContext.Routes.FindAsync(route.Id);
        routeToUpdate!.Cancel();
        _repository.Update(routeToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedRoute = await _dbContext.Routes.FindAsync(route.Id);
        updatedRoute!.Status.Should().Be(RouteStatusEnum.Cancelled);
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldDeleteRouteFromDatabase()
    {
        // Arrange
        var route = CreateValidRoute();
        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(route);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedRoute = await _dbContext.Routes.FindAsync(route.Id);
        deletedRoute.Should().BeNull();
    }

    #endregion
}