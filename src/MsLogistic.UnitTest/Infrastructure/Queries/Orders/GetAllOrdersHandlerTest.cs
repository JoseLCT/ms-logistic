using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Orders.GetAllOrders;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Orders;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Orders;

public class GetAllOrdersHandlerTest : IDisposable {
	private readonly PersistenceDbContext _context;
	private readonly GetAllOrdersHandler _handler;

	public GetAllOrdersHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_context = new PersistenceDbContext(options);
		_handler = new GetAllOrdersHandler(_context);
	}

	public void Dispose() {
		_context.Dispose();
	}

	private static OrderPersistenceModel CreateOrderPersistenceModel(
		Guid? id = null,
		Guid? batchId = null,
		Guid? customerId = null,
		Guid? routeId = null,
		int? deliverySequence = null,
		OrderStatusEnum status = OrderStatusEnum.Pending,
		DateTime? scheduledDeliveryDate = null,
		string? deliveryAddress = null,
		Point? deliveryLocation = null
	) {
		return new OrderPersistenceModel {
			Id = id ?? Guid.NewGuid(),
			BatchId = batchId ?? Guid.NewGuid(),
			CustomerId = customerId ?? Guid.NewGuid(),
			RouteId = routeId,
			DeliverySequence = deliverySequence,
			Status = status,
			ScheduledDeliveryDate = scheduledDeliveryDate ?? DateTime.UtcNow.AddDays(1),
			DeliveryAddress = deliveryAddress ?? "123 Main St, Anytown, USA",
			DeliveryLocation = deliveryLocation ?? new Point(0, 0) { SRID = 4326 },
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithNoOrders_ShouldReturnSuccessWithEmptyList() {
		// Arrange
		var query = new GetAllOrdersQuery();

		// Act
		Result<IReadOnlyList<OrderSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithSingleOrder_ShouldMapAllFieldsToDto() {
		// Arrange
		DateTime scheduledDate = DateTime.UtcNow.AddDays(2);
		var location = new Point(-68.15, -16.5) { SRID = 4326 };

		OrderPersistenceModel order = CreateOrderPersistenceModel(
			deliverySequence: 5,
			status: OrderStatusEnum.Pending,
			scheduledDeliveryDate: scheduledDate,
			deliveryLocation: location
		);

		await _context.Orders.AddAsync(order);
		await _context.SaveChangesAsync();

		var query = new GetAllOrdersQuery();

		// Act
		Result<IReadOnlyList<OrderSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);

		OrderSummaryDto dto = result.Value[0];
		dto.Id.Should().Be(order.Id);
		dto.DeliverySequence.Should().Be(5);
		dto.Status.Should().Be(OrderStatusEnum.Pending);
		dto.ScheduledDeliveryDate.Should().BeCloseTo(scheduledDate, TimeSpan.FromSeconds(1));

		dto.DeliveryLocation.Should().NotBeNull();
		dto.DeliveryLocation.Latitude.Should().Be(-16.5);
		dto.DeliveryLocation.Longitude.Should().Be(-68.15);
	}

	[Fact]
	public async Task Handle_WithMultipleOrders_ShouldReturnAllOrders() {
		// Arrange
		OrderPersistenceModel order1 = CreateOrderPersistenceModel();
		OrderPersistenceModel order2 = CreateOrderPersistenceModel();
		OrderPersistenceModel order3 = CreateOrderPersistenceModel();

		await _context.Orders.AddRangeAsync(order1, order2, order3);
		await _context.SaveChangesAsync();

		var query = new GetAllOrdersQuery();

		// Act
		Result<IReadOnlyList<OrderSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Select(o => o.Id).Should().BeEquivalentTo([order1.Id, order2.Id, order3.Id]);
	}

	[Fact]
	public async Task Handle_WithNullDeliverySequence_ShouldMapAsNull() {
		// Arrange
		OrderPersistenceModel order = CreateOrderPersistenceModel(deliverySequence: null);

		await _context.Orders.AddAsync(order);
		await _context.SaveChangesAsync();

		var query = new GetAllOrdersQuery();

		// Act
		Result<IReadOnlyList<OrderSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value[0].DeliverySequence.Should().BeNull();
	}

	[Theory]
	[InlineData(OrderStatusEnum.Pending)]
	[InlineData(OrderStatusEnum.InTransit)]
	[InlineData(OrderStatusEnum.Delivered)]
	public async Task Handle_WithDifferentStatuses_ShouldPreserveStatusInDto(OrderStatusEnum status) {
		// Arrange
		OrderPersistenceModel order = CreateOrderPersistenceModel(status: status);

		await _context.Orders.AddAsync(order);
		await _context.SaveChangesAsync();

		var query = new GetAllOrdersQuery();

		// Act
		Result<IReadOnlyList<OrderSummaryDto>> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value[0].Status.Should().Be(status);
	}
}
