using FluentAssertions;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Application.Orders.DeliverOrder;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Orders;

public class DeliverOrderHandlerTest {
	private readonly Mock<IOrderRepository> _orderRepositoryMock;
	private readonly Mock<IRouteRepository> _routeRepositoryMock;
	private readonly Mock<IImageStorageService> _imageStorageServiceMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
	private readonly DeliverOrderHandler _handler;

	public DeliverOrderHandlerTest() {
		_orderRepositoryMock = new Mock<IOrderRepository>();
		_routeRepositoryMock = new Mock<IRouteRepository>();
		_imageStorageServiceMock = new Mock<IImageStorageService>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_outboxRepositoryMock = new Mock<IOutboxRepository>();
		var loggerMock = new Mock<ILogger<DeliverOrderHandler>>();

		_handler = new DeliverOrderHandler(
			_orderRepositoryMock.Object,
			_routeRepositoryMock.Object,
			_imageStorageServiceMock.Object,
			_unitOfWorkMock.Object,
			_outboxRepositoryMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithValidOrder_ShouldDeliverAndReturnSuccess() {
		// Arrange
		Order order = CreateDeliverableOrder();
		DeliverOrderCommand command = CreateValidCommand(order.Id);
		string imageResource = "https://storage/image.jpg";

		_orderRepositoryMock
			.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(order);

		_routeRepositoryMock
			.Setup(x => x.GetByIdAsync(order.RouteId!.Value, It.IsAny<CancellationToken>()))
			.ReturnsAsync(
				Route.Create(
					batchId: Guid.NewGuid(),
					deliveryZoneId: Guid.NewGuid(),
					driverId: Guid.NewGuid(),
					originLocation: GeoPointValue.Create(-17.78, -63.18)
				)
			);

		_imageStorageServiceMock
			.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Result.Success(new ImageResourceValue {
				Url = imageResource,
				ExternalId = "external-id-123"
			}));

		_outboxRepositoryMock
			.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()),
			Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenOrderNotFound_ShouldReturnNotFoundErrorAndNotCommit() {
		// Arrange
		var orderId = Guid.NewGuid();
		DeliverOrderCommand command = CreateValidCommand(orderId);

		_orderRepositoryMock
			.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Order?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().NotBeNull();
		result.Error.Type.Should().Be(ErrorType.NotFound);
		result.Error.Code.Should().Contain("Order");
		result.Error.Message.Should().Contain(orderId.ToString());
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenOrderCannotBeDelivered_ShouldReturnFailureAndNotCommit() {
		// Arrange
		Order order = CreateNonDeliverableOrder();
		DeliverOrderCommand command = CreateValidCommand(order.Id);

		_orderRepositoryMock
			.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(order);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		_imageStorageServiceMock.Verify(
			x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never
		);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenImageUploadFails_ShouldReturnFailureAndNotCommit() {
		// Arrange
		Order order = CreateDeliverableOrder();
		DeliverOrderCommand command = CreateValidCommand(order.Id);
		var uploadError = Error.Failure("Storage.UploadFailed", "Image upload failed");

		_orderRepositoryMock
			.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(order);

		_imageStorageServiceMock
			.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Result.Failure<ImageResourceValue>(uploadError));

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().NotBeNull();
		_outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	#region Helper Methods

	private static Order CreateDeliverableOrder() {
		var order = Order.Create(
			batchId: Guid.NewGuid(),
			customerId: Guid.NewGuid(),
			scheduledDeliveryDate: DateTime.UtcNow.AddDays(1),
			deliveryAddress: "Calle Principal 123",
			deliveryLocation: GeoPointValue.Create(-17.78, -63.18)
		);
		order.AddItem(Guid.NewGuid(), 2);
		order.AssignToRoute(Guid.NewGuid(), 1);
		order.MarkAsInTransit();
		return order;
	}

	private static Order CreateNonDeliverableOrder() {
		return Order.Create(
			batchId: Guid.NewGuid(),
			customerId: Guid.NewGuid(),
			scheduledDeliveryDate: DateTime.UtcNow.AddDays(1),
			deliveryAddress: "Calle Principal 123",
			deliveryLocation: GeoPointValue.Create(-17.78, -63.18)
		);
	}

	private static DeliverOrderCommand CreateValidCommand(Guid orderId) {
		return new DeliverOrderCommand {
			OrderId = orderId,
			Location = new CoordinateDto(Latitude: -17.78, Longitude: -63.18),
			Comments = "Entregado en puerta",
			ImageStream = new MemoryStream([1, 2, 3]),
			ImageFileName = "delivery.jpg"
		};
	}

	#endregion
}
