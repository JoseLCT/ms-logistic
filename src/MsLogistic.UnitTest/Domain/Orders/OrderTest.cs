using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Errors;
using MsLogistic.Domain.Orders.Events;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Orders;

public class OrderTest
{
    private static GeoPointValue CreateValidGeoPoint()
        => GeoPointValue.Create(-17.7833, -63.1821);

    private static Order CreateValidOrder()
    {
        return Order.Create(
            batchId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            scheduledDeliveryDate: DateTime.UtcNow.Date.AddDays(1),
            deliveryAddress: "Av. San Martin 123",
            deliveryLocation: CreateValidGeoPoint()
        );
    }

    #region Create

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var batchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.Date.AddDays(1);
        var address = "Av. San Martin 123";
        var location = CreateValidGeoPoint();

        // Act
        var order = Order.Create(batchId, customerId, scheduledDate, address, location);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.BatchId.Should().Be(batchId);
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatusEnum.Pending);
        order.ScheduledDeliveryDate.Should().Be(scheduledDate);
        order.DeliveryAddress.Should().Be(address);
        order.DeliveryLocation.Should().Be(location);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithPastDate_ShouldThrowDomainException()
    {
        // Arrange
        var batchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var address = "Av. San Martin 123";
        var location = CreateValidGeoPoint();

        var pastDate = DateTime.UtcNow.Date.AddDays(-1);

        // Act
        Action act = () => Order.Create(
            batchId,
            customerId,
            pastDate,
            address,
            location
        );

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.ScheduledDeliveryDateCannotBeInThePast);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidAddress_ShouldThrowDomainException(string invalidAddress)
    {
        // Arrange
        var batchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.Date.AddDays(1);
        var location = CreateValidGeoPoint();

        // Act
        Action act = () => Order.Create(
            batchId,
            customerId,
            scheduledDate,
            invalidAddress,
            location
        );

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.DeliveryAddressIsRequired);
    }

    #endregion

    #region AddItem

    [Fact]
    public void AddItem_WithValidData_ShouldAddItem()
    {
        // Arrange
        var order = CreateValidOrder();
        var productId = Guid.NewGuid();

        // Act
        order.AddItem(productId, 5);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(productId);
        order.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_SameProductTwice_ShouldIncreaseQuantity()
    {
        // Arrange
        var order = CreateValidOrder();
        var productId = Guid.NewGuid();

        // Act
        order.AddItem(productId, 3);
        order.AddItem(productId, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_DifferentProducts_ShouldAddMultipleItems()
    {
        // Arrange
        var order = CreateValidOrder();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        // Act
        order.AddItem(productId1, 3);
        order.AddItem(productId2, 2);

        // Assert
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_WhenNotPending_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), 1);
        order.AssignToRoute(Guid.NewGuid(), 1);
        order.MarkAsInTransit();

        // Act
        Action act = () => order.AddItem(Guid.NewGuid(), 1);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.CannotModifyOrderThatIsNotPending);
    }

    #endregion

    #region AssignToRoute

    [Fact]
    public void AssignToRoute_WithValidData_ShouldAssignRoute()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), 5);
        var routeId = Guid.NewGuid();

        // Act
        order.AssignToRoute(routeId, 1);

        // Assert
        order.RouteId.Should().Be(routeId);
        order.DeliverySequence.Should().Be(1);
    }

    [Fact]
    public void AssignToRoute_WhenNotPending_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), 1);
        order.AssignToRoute(Guid.NewGuid(), 1);
        order.MarkAsInTransit();

        // Act
        Action act = () => order.AssignToRoute(Guid.NewGuid(), 2);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.CannotAssignOrderThatIsNotPending);
    }

    [Fact]
    public void AssignToRoute_WithInvalidSequence_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.AddItem(Guid.NewGuid(), 1);

        // Act
        Action act = () => order.AssignToRoute(Guid.NewGuid(), 0);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.DeliverySequenceMustBeGreaterThanZero);
    }

    [Fact]
    public void AssignToRoute_WithoutItems_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        Action act = () => order.AssignToRoute(Guid.NewGuid(), 1);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.CannotAssignOrderWithoutItems);
    }

    #endregion

    #region MarkAsInTransit

    [Fact]
    public void MarkAsInTransit_WhenPending_ShouldChangeStatus()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.MarkAsInTransit();

        // Assert
        order.Status.Should().Be(OrderStatusEnum.InTransit);
    }

    [Fact]
    public void MarkAsInTransit_WhenNotPending_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Cancel();

        // Act
        Action act = () => order.MarkAsInTransit();

        // Assert
        act.Should().Throw<DomainException>();
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_WhenPending_ShouldCancelAndRaiseEvent()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatusEnum.Cancelled);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelled);
    }

    [Fact]
    public void Cancel_WhenInTransit_ShouldCancelAndRaiseEvent()
    {
        // Arrange
        var order = CreateValidOrder();
        order.MarkAsInTransit();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatusEnum.Cancelled);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelled);
    }

    [Fact]
    public void Cancel_WhenDelivered_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.MarkAsInTransit();
        order.Deliver(Guid.NewGuid(), CreateValidGeoPoint(), null, null);

        // Act
        Action act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>();
    }

    #endregion

    #region ReportIncident

    [Fact]
    public void ReportIncident_WhenInTransit_ShouldReportAndRaiseEvent()
    {
        // Arrange
        var order = CreateValidOrder();
        order.MarkAsInTransit();
        var driverId = Guid.NewGuid();

        // Act
        order.ReportIncident(driverId, OrderIncidentTypeEnum.AbsentRecipient, "Recipient was not available");

        // Assert
        order.Status.Should().Be(OrderStatusEnum.Failed);
        order.Incident.Should().NotBeNull();
        order.DomainEvents.Should().ContainSingle(e => e is OrderIncidentReported);
    }

    [Fact]
    public void ReportIncident_WhenNotInTransit_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        Action act = () => order.ReportIncident(
            Guid.NewGuid(),
            OrderIncidentTypeEnum.Other,
            "Description"
        );

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ReportIncident_WhenAlreadyReported_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.MarkAsInTransit();
        order.ReportIncident(Guid.NewGuid(), OrderIncidentTypeEnum.Other, "First");

        // Act
        Action act = () => order.ReportIncident(
            Guid.NewGuid(),
            OrderIncidentTypeEnum.IncorrectAddress,
            "Second"
        );

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderErrors.IncidentAlreadyReported);
    }

    #endregion

    #region Deliver

    [Fact]
    public void Deliver_WhenInTransit_ShouldDeliverAndRaiseEvent()
    {
        // Arrange
        var order = CreateValidOrder();
        order.MarkAsInTransit();
        var driverId = Guid.NewGuid();
        var location = CreateValidGeoPoint();

        // Act
        order.Deliver(driverId, location, "Delivered successfully", "image.jpg");

        // Assert
        order.Status.Should().Be(OrderStatusEnum.Delivered);
        order.Delivery.Should().NotBeNull();
        order.Delivery!.DriverId.Should().Be(driverId);
        order.DomainEvents.Should().ContainSingle(e => e is OrderDelivered);
    }

    [Fact]
    public void Deliver_WhenNotInTransit_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        Action act = () => order.Deliver(Guid.NewGuid(), CreateValidGeoPoint(), null, null);

        // Assert
        act.Should().Throw<DomainException>();
    }

    #endregion

    #region Workflow

    [Fact]
    public void Order_CompleteHappyPath_ShouldSucceed()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act & Assert - Add items
        order.AddItem(Guid.NewGuid(), 3);
        order.AddItem(Guid.NewGuid(), 2);
        order.Items.Should().HaveCount(2);

        // Act & Assert - Assign to route
        order.AssignToRoute(Guid.NewGuid(), 1);
        order.RouteId.Should().NotBeNull();

        // Act & Assert - Start transit
        order.MarkAsInTransit();
        order.Status.Should().Be(OrderStatusEnum.InTransit);

        // Act & Assert - Deliver
        order.Deliver(Guid.NewGuid(), CreateValidGeoPoint(), "OK", null);
        order.Status.Should().Be(OrderStatusEnum.Delivered);
        order.DomainEvents.Should().ContainSingle(e => e is OrderDelivered);
    }

    #endregion
}