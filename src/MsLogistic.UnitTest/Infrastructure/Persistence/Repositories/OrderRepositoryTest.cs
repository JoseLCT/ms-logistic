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
		DbContextOptions<DomainDbContext> options = new DbContextOptionsBuilder<DomainDbContext>()
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
		Guid? batchId = null,
		Guid? customerId = null,
		DateTime? scheduledDeliveryDate = null,
		string deliveryAddress = "123 Main St",
		GeoPointValue? deliveryLocation = null
	) {
		return Order.Create(
			batchId: batchId ?? Guid.NewGuid(),
			customerId: customerId ?? Guid.NewGuid(),
			scheduledDeliveryDate: scheduledDeliveryDate ?? DateTime.UtcNow.AddDays(2),
			deliveryAddress: deliveryAddress,
			deliveryLocation: deliveryLocation ?? GeoPointValue.Create(-17.7833, -63.1821)
		);
	}

	#region GetByIdAsync

	[Fact]
	public async Task GetByIdAsync_WhenOrderExists_ShouldReturnOrderWithRelatedData() {
		// Arrange
		Order order = CreateValidOrder();
		order.AddItem(Guid.NewGuid(), 2);
		order.AddItem(Guid.NewGuid(), 3);
		await _dbContext.Orders.AddAsync(order);
		await _dbContext.SaveChangesAsync();

		_dbContext.ChangeTracker.Clear();

		// Act
		Order? result = await _repository.GetByIdAsync(order.Id);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(order.Id);
		result.Items.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByIdAsync_WhenOrderDoesNotExist_ShouldReturnNull() {
		// Arrange
		var nonExistingId = Guid.NewGuid();

		// Act
		Order? result = await _repository.GetByIdAsync(nonExistingId);

		// Assert
		result.Should().BeNull();
	}

	#endregion

	#region GetByBatchIdAsync

	[Fact]
	public async Task GetByBatchIdAsync_WhenOrdersExist_ShouldReturnOrdersForBatch() {
		// Arrange
		var batchId = Guid.NewGuid();
		var otherBatchId = Guid.NewGuid();

		Order order1 = CreateValidOrder(batchId: batchId);
		Order order2 = CreateValidOrder(batchId: batchId);
		Order orderFromOtherBatch = CreateValidOrder(batchId: otherBatchId);

		await _dbContext.Orders.AddRangeAsync(order1, order2, orderFromOtherBatch);
		await _dbContext.SaveChangesAsync();

		// Act
		IReadOnlyList<Order> result = await _repository.GetByBatchIdAsync(batchId);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(o => o.Id == order1.Id);
		result.Should().Contain(o => o.Id == order2.Id);
		result.Should().NotContain(o => o.Id == orderFromOtherBatch.Id);
	}

	[Fact]
	public async Task GetByBatchIdAsync_WhenNoOrdersForBatch_ShouldReturnEmptyList() {
		// Act
		IReadOnlyList<Order> result = await _repository.GetByBatchIdAsync(Guid.NewGuid());

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GetByRouteIdAsync

	[Fact]
	public async Task GetByRouteIdAsync_WhenOrdersExist_ShouldReturnOrdersForRoute() {
		// Arrange
		var routeId = Guid.NewGuid();
		var otherRouteId = Guid.NewGuid();

		Order order1 = CreateValidOrder();
		order1.AddItem(Guid.NewGuid(), 1);
		order1.AssignToRoute(routeId, 1);

		Order order2 = CreateValidOrder();
		order2.AddItem(Guid.NewGuid(), 1);
		order2.AssignToRoute(routeId, 2);

		Order orderFromOtherRoute = CreateValidOrder();
		orderFromOtherRoute.AddItem(Guid.NewGuid(), 1);
		orderFromOtherRoute.AssignToRoute(otherRouteId, 1);

		await _dbContext.Orders.AddRangeAsync(order1, order2, orderFromOtherRoute);
		await _dbContext.SaveChangesAsync();

		// Act
		IReadOnlyList<Order> result = await _repository.GetByRouteIdAsync(routeId);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(o => o.Id == order1.Id);
		result.Should().Contain(o => o.Id == order2.Id);
		result.Should().NotContain(o => o.Id == orderFromOtherRoute.Id);
	}

	[Fact]
	public async Task GetByRouteIdAsync_WhenNoOrdersForRoute_ShouldReturnEmptyList() {
		// Act
		IReadOnlyList<Order> result = await _repository.GetByRouteIdAsync(Guid.NewGuid());

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GetAllAsync

	[Fact]
	public async Task GetAllAsync_WhenOrdersExist_ShouldReturnAllOrders() {
		// Arrange
		Order order1 = CreateValidOrder();
		Order order2 = CreateValidOrder();
		Order order3 = CreateValidOrder();

		await _dbContext.Orders.AddRangeAsync(order1, order2, order3);
		await _dbContext.SaveChangesAsync();

		// Act
		IReadOnlyList<Order> result = await _repository.GetAllAsync();

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(o => o.Id == order1.Id);
		result.Should().Contain(o => o.Id == order2.Id);
		result.Should().Contain(o => o.Id == order3.Id);
	}

	[Fact]
	public async Task GetAllAsync_WhenNoOrders_ShouldReturnEmptyList() {
		// Act
		IReadOnlyList<Order> result = await _repository.GetAllAsync();

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region AddAsync

	[Fact]
	public async Task AddAsync_ShouldAddOrderToDatabase() {
		// Arrange
		Order order = CreateValidOrder();

		// Act
		await _repository.AddAsync(order);
		await _dbContext.SaveChangesAsync();

		// Assert
		Order? savedOrder = await _dbContext.Orders.FindAsync(order.Id);
		savedOrder.Should().NotBeNull();
		savedOrder.Id.Should().Be(order.Id);
		savedOrder.DeliveryAddress.Should().Be(order.DeliveryAddress);
	}

	#endregion

	#region Update

	[Fact]
	public async Task Update_ShouldUpdateOrderInDatabase() {
		// Arrange
		Order order = CreateValidOrder();
		await _dbContext.Orders.AddAsync(order);
		await _dbContext.SaveChangesAsync();

		_dbContext.Entry(order).State = EntityState.Detached;

		// Act
		Order? orderToUpdate = await _dbContext.Orders.FindAsync(order.Id);
		orderToUpdate!.Cancel();
		_repository.Update(orderToUpdate);
		await _dbContext.SaveChangesAsync();

		// Assert
		Order? updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
		updatedOrder!.Status.Should().Be(OrderStatusEnum.Cancelled);
	}

	#endregion

	#region Remove

	[Fact]
	public async Task Remove_ShouldDeleteOrderFromDatabase() {
		// Arrange
		Order order = CreateValidOrder();
		await _dbContext.Orders.AddAsync(order);
		await _dbContext.SaveChangesAsync();

		// Act
		_repository.Remove(order);
		await _dbContext.SaveChangesAsync();

		// Assert
		Order? deletedOrder = await _dbContext.Orders.FindAsync(order.Id);
		deletedOrder.Should().BeNull();
	}

	#endregion
}
