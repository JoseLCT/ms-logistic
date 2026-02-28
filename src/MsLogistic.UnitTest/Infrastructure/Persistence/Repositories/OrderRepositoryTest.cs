using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Shared.ValueObjects;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class OrderRepositoryTest : IDisposable {
    private readonly DomainDbContext _dbContext;
    private readonly OrderRepository _repository;

    public OrderRepositoryTest() {
        var options = new DbContextOptionsBuilder<DomainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DomainDbContext(options);
        _repository = new OrderRepository(_dbContext);
    }

    public void Dispose() {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private static Order CreateValidOrder(
        DateTime? scheduledDeliveryDate = null,
        string deliveryAddress = "123 Main St",
        GeoPointValue? deliveryLocation = null
    ) {
        return Order.Create(
            batchId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            scheduledDeliveryDate: scheduledDeliveryDate ?? DateTime.UtcNow.AddDays(2),
            deliveryAddress: deliveryAddress,
            deliveryLocation: deliveryLocation ?? GeoPointValue.Create(-17.7833, -63.1821)
        );
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ShouldReturnOrder() {
        // Arrange
        var order = CreateValidOrder();
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ShouldReturnNull() {
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
    public async Task GetAllAsync_WhenOrdersExist_ShouldReturnAllOrders() {
        // Arrange
        var order1 = CreateValidOrder();
        var order2 = CreateValidOrder();
        var order3 = CreateValidOrder();

        await _dbContext.Orders.AddRangeAsync(order1, order2, order3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().Contain(o => o.Id == order3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoOrders_ShouldReturnEmptyList() {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldAddOrderToDatabase() {
        // Arrange
        var order = CreateValidOrder();

        // Act
        await _repository.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedOrder = await _dbContext.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder.Id.Should().Be(order.Id);
        savedOrder.DeliveryAddress.Should().Be(order.DeliveryAddress);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateOrderInDatabase() {
        // Arrange
        var order = CreateValidOrder();
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(order).State = EntityState.Detached;

        // Act
        var orderToUpdate = await _dbContext.Orders.FindAsync(order.Id);
        orderToUpdate!.Cancel();
        _repository.Update(orderToUpdate);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatusEnum.Cancelled);
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldDeleteOrderFromDatabase() {
        // Arrange
        var order = CreateValidOrder();
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(order);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedOrder = await _dbContext.Orders.FindAsync(order.Id);
        deletedOrder.Should().BeNull();
    }

    #endregion
}
