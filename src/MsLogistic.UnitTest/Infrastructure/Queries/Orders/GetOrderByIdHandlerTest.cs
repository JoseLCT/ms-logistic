using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Orders.GetOrderById;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Orders;
using NetTopologySuite.Geometries;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Orders;

public class GetOrderByIdHandlerTest : IDisposable {
	private readonly PersistenceDbContext _dbContext;
	private readonly GetOrderByIdHandler _handler;

	public GetOrderByIdHandlerTest() {
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_dbContext = new PersistenceDbContext(options);
		_handler = new GetOrderByIdHandler(_dbContext);
	}

	public void Dispose() {
		_dbContext.Dispose();
	}

	private static Point CreatePoint(double longitude, double latitude) {
		return new Point(longitude, latitude) { SRID = 4326 };
	}

	private static OrderPersistenceModel CreateOrderPersistenceModel(
		Guid? id = null,
		Guid? batchId = null,
		Guid? customerId = null,
		Guid? routeId = null,
		int? deliverySequence = null,
		OrderStatusEnum status = OrderStatusEnum.Pending,
		DateTime? scheduledDeliveryDate = null,
		string deliveryAddress = "Av. Siempre Viva 123",
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
			DeliveryAddress = deliveryAddress,
			DeliveryLocation = deliveryLocation ?? CreatePoint(-68.15, -16.50),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	private static ProductPersistenceModel CreateProductPersistenceModel(
		Guid? id = null,
		Guid? externalId = null,
		string name = "Test Product",
		string? description = null
	) {
		return new ProductPersistenceModel {
			Id = id ?? Guid.NewGuid(),
			ExternalId = externalId ?? Guid.NewGuid(),
			Name = name,
			Description = description,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithExistingOrderId_ShouldReturnOrderWithAllPrimaryFieldsMapped() {
		// Arrange
		var batchId = Guid.NewGuid();
		var customerId = Guid.NewGuid();
		var routeId = Guid.NewGuid();
		DateTime scheduledDate = DateTime.UtcNow.AddDays(2);

		OrderPersistenceModel newOrder = CreateOrderPersistenceModel(
			batchId: batchId,
			customerId: customerId,
			routeId: routeId,
			deliverySequence: 5,
			status: OrderStatusEnum.Pending,
			scheduledDeliveryDate: scheduledDate,
			deliveryAddress: "Calle Falsa 123",
			deliveryLocation: CreatePoint(-68.15, -16.50)
		);

		await _dbContext.Orders.AddAsync(newOrder);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(newOrder.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newOrder.Id);
		result.Value.BatchId.Should().Be(batchId);
		result.Value.CustomerId.Should().Be(customerId);
		result.Value.RouteId.Should().Be(routeId);
		result.Value.DeliverySequence.Should().Be(5);
		result.Value.Status.Should().Be(OrderStatusEnum.Pending);
		result.Value.ScheduledDeliveryDate.Should().Be(scheduledDate);
		result.Value.DeliveryAddress.Should().Be("Calle Falsa 123");

		result.Value.DeliveryLocation.Latitude.Should().Be(-16.50);
		result.Value.DeliveryLocation.Longitude.Should().Be(-68.15);

		result.Value.Delivery.Should().BeNull();
		result.Value.Incident.Should().BeNull();
		result.Value.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithOrderWithoutOptionalFields_ShouldMapNullableFieldsAsNull() {
		// Arrange
		OrderPersistenceModel newOrder = CreateOrderPersistenceModel(
			routeId: null,
			deliverySequence: null
		);

		await _dbContext.Orders.AddAsync(newOrder);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(newOrder.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.RouteId.Should().BeNull();
		result.Value.DeliverySequence.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithOrderWithDelivery_ShouldMapDeliveryDtoCorrectly() {
		// Arrange
		var driverId = Guid.NewGuid();
		var deliveryId = Guid.NewGuid();
		DateTime deliveredAt = DateTime.UtcNow;

		OrderPersistenceModel newOrder = CreateOrderPersistenceModel();
		newOrder.Delivery = new OrderDeliveryPersistenceModel {
			Id = deliveryId,
			OrderId = newOrder.Id,
			Order = newOrder,
			DriverId = driverId,
			Location = CreatePoint(-68.20, -16.55),
			DeliveredAt = deliveredAt,
			Comments = "Entregado al portero",
			ImageUrl = "https://example.com/proof.jpg",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};

		await _dbContext.Orders.AddAsync(newOrder);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(newOrder.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Delivery.Should().NotBeNull();
		result.Value.Delivery!.Id.Should().Be(deliveryId);
		result.Value.Delivery.DriverId.Should().Be(driverId);
		result.Value.Delivery.Location.Latitude.Should().Be(-16.55);
		result.Value.Delivery.Location.Longitude.Should().Be(-68.20);
		result.Value.Delivery.DeliveredAt.Should().Be(deliveredAt);
		result.Value.Delivery.Comments.Should().Be("Entregado al portero");
		result.Value.Delivery.ImageUrl.Should().Be("https://example.com/proof.jpg");
	}

	[Fact]
	public async Task Handle_WithOrderWithDeliveryWithNullableFieldsNull_ShouldMapAsNull() {
		// Arrange
		OrderPersistenceModel newOrder = CreateOrderPersistenceModel();
		newOrder.Delivery = new OrderDeliveryPersistenceModel {
			Id = Guid.NewGuid(),
			OrderId = newOrder.Id,
			Order = newOrder,
			DriverId = Guid.NewGuid(),
			Location = CreatePoint(-68.20, -16.55),
			DeliveredAt = DateTime.UtcNow,
			Comments = null,
			ImageUrl = null,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};

		await _dbContext.Orders.AddAsync(newOrder);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(newOrder.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Delivery.Should().NotBeNull();
		result.Value.Delivery!.Comments.Should().BeNull();
		result.Value.Delivery.ImageUrl.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithOrderWithIncident_ShouldMapIncidentDtoCorrectly() {
		// Arrange
		var driverId = Guid.NewGuid();
		var incidentId = Guid.NewGuid();

		OrderPersistenceModel newOrder = CreateOrderPersistenceModel();
		newOrder.Incident = new OrderIncidentPersistenceModel {
			Id = incidentId,
			OrderId = newOrder.Id,
			Order = newOrder,
			DriverId = driverId,
			IncidentType = OrderIncidentTypeEnum.IncorrectAddress,
			Description = "Dirección no encontrada en el barrio indicado",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};

		await _dbContext.Orders.AddAsync(newOrder);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(newOrder.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Incident.Should().NotBeNull();
		result.Value.Incident!.Id.Should().Be(incidentId);
		result.Value.Incident.DriverId.Should().Be(driverId);
		result.Value.Incident.IncidentType.Should().Be(OrderIncidentTypeEnum.IncorrectAddress);
		result.Value.Incident.Description.Should().Be("Dirección no encontrada en el barrio indicado");
	}

	[Fact]
	public async Task Handle_WithOrderWithItems_ShouldMapAllItems() {
		// Arrange
		var itemId1 = Guid.NewGuid();
		var itemId2 = Guid.NewGuid();
		ProductPersistenceModel product1 = CreateProductPersistenceModel();
		ProductPersistenceModel product2 = CreateProductPersistenceModel();

		OrderPersistenceModel newOrder = CreateOrderPersistenceModel();
		newOrder.Items = new List<OrderItemPersistenceModel> {
			new() {
				Id = itemId1,
				OrderId = newOrder.Id,
				Order = newOrder,
				ProductId = product1.Id,
				Product = product1,
				Quantity = 3,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null
			},
			new() {
				Id = itemId2,
				OrderId = newOrder.Id,
				Order = newOrder,
				ProductId = product2.Id,
				Product = product2,
				Quantity = 7,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null
			}
		};

		await _dbContext.Orders.AddAsync(newOrder);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(newOrder.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Items.Should().HaveCount(2);
		result.Value.Items.Should().BeEquivalentTo([
			new OrderItemDto(itemId1, product1.Id, 3),
			new OrderItemDto(itemId2, product2.Id, 7)
		]);
	}

	[Fact]
	public async Task Handle_WithNonExistingOrderId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetOrderByIdQuery(nonExistingId);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Order", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingIdAndOtherOrdersInDb_ShouldReturnNotFoundError() {
		// Arrange
		OrderPersistenceModel order1 = CreateOrderPersistenceModel();
		OrderPersistenceModel order2 = CreateOrderPersistenceModel();

		await _dbContext.Orders.AddRangeAsync(order1, order2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetOrderByIdQuery(nonExistingId);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Order", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithMultipleOrders_ShouldReturnCorrectOrder() {
		// Arrange
		OrderPersistenceModel order1 = CreateOrderPersistenceModel(
			deliveryAddress: "Address 1"
		);
		OrderPersistenceModel order2 = CreateOrderPersistenceModel(
			deliveryAddress: "Address 2"
		);
		OrderPersistenceModel order3 = CreateOrderPersistenceModel(
			deliveryAddress: "Address 3"
		);

		await _dbContext.Orders.AddRangeAsync(order1, order2, order3);
		await _dbContext.SaveChangesAsync();

		var query = new GetOrderByIdQuery(order2.Id);

		// Act
		Result<OrderDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(order2.Id);
		result.Value.DeliveryAddress.Should().Be("Address 2");
	}
}
