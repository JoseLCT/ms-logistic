using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using MsLogistic.Application.Routes.Services;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Routes;

public class RouteCompletionServiceTest {
	private readonly Mock<IOrderRepository> _orderRepositoryMock;
	private readonly Mock<IRouteRepository> _routeRepositoryMock;
	private readonly RouteCompletionService _service;

	public RouteCompletionServiceTest() {
		_orderRepositoryMock = new Mock<IOrderRepository>();
		_routeRepositoryMock = new Mock<IRouteRepository>();

		_service = new RouteCompletionService(
			_orderRepositoryMock.Object,
			_routeRepositoryMock.Object
		);
	}

	[Fact]
	public async Task TryCompleteRouteAsync_WhenRouteIdIsNull_ShouldReturnFalse() {
		// Act
		bool result = await _service.TryCompleteRouteAsync(null, CancellationToken.None);

		// Assert
		result.Should().BeFalse();

		_routeRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
		_routeRepositoryMock.Verify(r => r.Update(It.IsAny<Route>()), Times.Never);
	}

	[Fact]
	public async Task TryCompleteRouteAsync_WhenRouteDoesNotExist_ShouldReturnFalse() {
		// Arrange
		var routeId = Guid.NewGuid();

		_routeRepositoryMock
			.Setup(r => r.GetByIdAsync(routeId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Route?)null);

		// Act
		bool result = await _service.TryCompleteRouteAsync(routeId, CancellationToken.None);

		// Assert
		result.Should().BeFalse();

		_orderRepositoryMock.Verify(r => r.GetByRouteIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_routeRepositoryMock.Verify(r => r.Update(It.IsAny<Route>()), Times.Never);
	}

	[Fact]
	public async Task TryCompleteRouteAsync_WhenRouteHasNoOrders_ShouldReturnFalse() {
		// Arrange
		Route route = CreateValidRoute();

		_routeRepositoryMock
			.Setup(r => r.GetByIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(route);

		_orderRepositoryMock
			.Setup(r => r.GetByRouteIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Order>().AsReadOnly());

		// Act
		bool result = await _service.TryCompleteRouteAsync(route.Id, CancellationToken.None);

		// Assert
		result.Should().BeFalse();
		_routeRepositoryMock.Verify(r => r.Update(It.IsAny<Route>()), Times.Never);
	}

	[Theory]
	[InlineData(OrderStatusEnum.Pending)]
	[InlineData(OrderStatusEnum.InTransit)]
	public async Task TryCompleteRouteAsync_WhenNotAllOrdersAreTerminal_ShouldReturnFalse(
		OrderStatusEnum nonTerminalStatus) {
		// Arrange
		Route route = CreateValidRoute();

		ReadOnlyCollection<Order> orders = new List<Order> {
			CreateDeliverableOrder(),
			CreateOrderWithStatus(nonTerminalStatus)
		}.AsReadOnly();

		_routeRepositoryMock
			.Setup(r => r.GetByIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(route);

		_orderRepositoryMock
			.Setup(r => r.GetByRouteIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(orders);

		// Act
		bool result = await _service.TryCompleteRouteAsync(route.Id, CancellationToken.None);

		// Assert
		result.Should().BeFalse();

		_routeRepositoryMock.Verify(r => r.Update(It.IsAny<Route>()), Times.Never);
	}

	[Theory]
	[InlineData(OrderStatusEnum.Delivered)]
	[InlineData(OrderStatusEnum.Failed)]
	[InlineData(OrderStatusEnum.Cancelled)]
	public async Task TryCompleteRouteAsync_WhenAllOrdersHaveSingleTerminalStatus_ShouldCompleteRouteAndReturnTrue(
		OrderStatusEnum terminalStatus) {
		Route route = CreateValidRoute();

		ReadOnlyCollection<Order> orders = new List<Order> {
			CreateOrderWithStatus(terminalStatus),
			CreateOrderWithStatus(terminalStatus),
			CreateOrderWithStatus(terminalStatus)
		}.AsReadOnly();

		_routeRepositoryMock
			.Setup(r => r.GetByIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(route);

		_orderRepositoryMock
			.Setup(r => r.GetByRouteIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(orders);

		// Act
		bool result = await _service.TryCompleteRouteAsync(route.Id, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		route.Status.Should().Be(RouteStatusEnum.Completed);

		_routeRepositoryMock.Verify(r => r.Update(route), Times.Once);
	}

	[Fact]
	public async Task TryCompleteRouteAsync_WhenAllOrdersHaveMixedTerminalStatuses_ShouldCompleteRouteAndReturnTrue() {
		// Arrange
		Route route = CreateValidRoute();

		ReadOnlyCollection<Order> orders = new List<Order> {
			CreateOrderWithStatus(OrderStatusEnum.Delivered),
			CreateOrderWithStatus(OrderStatusEnum.Failed),
			CreateOrderWithStatus(OrderStatusEnum.Cancelled)
		}.AsReadOnly();

		_routeRepositoryMock
			.Setup(r => r.GetByIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(route);

		_orderRepositoryMock
			.Setup(r => r.GetByRouteIdAsync(route.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(orders);

		// Act
		bool result = await _service.TryCompleteRouteAsync(route.Id, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		route.Status.Should().Be(RouteStatusEnum.Completed);

		_routeRepositoryMock.Verify(r => r.Update(route), Times.Once);
	}

	#region Helper Methods

	private static Route CreateValidRoute(Guid? routeId = null) {
		var route = Route.Create(
			batchId: Guid.NewGuid(),
			deliveryZoneId: Guid.NewGuid(),
			driverId: Guid.NewGuid(),
			GeoPointValue.Create(latitude: 0, longitude: 0)
		);
		route.Start();
		return route;
	}

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

	private static Order CreateOrderWithStatus(OrderStatusEnum status) {
		Order order = CreateNonDeliverableOrder();

		if (status == OrderStatusEnum.Pending)
			return order;

		order.AddItem(Guid.NewGuid(), 2);
		order.AssignToRoute(Guid.NewGuid(), 1);
		order.MarkAsInTransit();

		if (status == OrderStatusEnum.InTransit)
			return order;

		if (status == OrderStatusEnum.Delivered)
			order.Deliver(
				driverId: Guid.NewGuid(),
				location: GeoPointValue.Create(-17.78, -63.18),
				comments: null,
				imageUrl: null
			);
		else if (status == OrderStatusEnum.Failed)
			order.ReportIncident(
				driverId: Guid.NewGuid(),
				incidentType: OrderIncidentTypeEnum.DamagedPackage,
				description: "Incidente reportado"
			);
		else if (status == OrderStatusEnum.Cancelled)
			order.Cancel();

		return order;
	}

	#endregion
}
