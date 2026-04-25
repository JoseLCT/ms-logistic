using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Orders.ReportIncident;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Orders;

public class ReportIncidentHandlerTest {
	private readonly Mock<IOrderRepository> _orderRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly ReportIncidentHandler _handler;

	public ReportIncidentHandlerTest() {
		_orderRepositoryMock = new Mock<IOrderRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<ReportIncidentHandler>>();

		_handler = new ReportIncidentHandler(
			_orderRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithExistingOrder_ShouldReportIncidentAndReturnSuccess() {
		// Arrange
		Order order = CreateAssignedOrder();
		var command = new ReportIncidentCommand {
			OrderId = order.Id,
			DriverId = Guid.NewGuid(),
			IncidentType = OrderIncidentTypeEnum.AbsentRecipient,
			Description = "Recipient was not present at the delivery address"
		};

		_orderRepositoryMock
			.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(order);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenOrderNotFound_ShouldReturnNotFoundErrorAndNotCommit() {
		// Arrange
		var orderId = Guid.NewGuid();
		var command = new ReportIncidentCommand {
			OrderId = orderId,
			DriverId = Guid.NewGuid(),
			IncidentType = OrderIncidentTypeEnum.DamagedPackage,
			Description = "Package was damaged during delivery"
		};

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

	#region Helper Methods

	private static Order CreateAssignedOrder() {
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

	#endregion
}
