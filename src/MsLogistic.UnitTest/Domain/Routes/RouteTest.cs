using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Routes.Entities;
using MsLogistic.Domain.Routes.Enums;
using MsLogistic.Domain.Routes.Errors;
using MsLogistic.Domain.Routes.Events;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Routes;

public class RouteTest {
    private static GeoPointValue CreateValidGeoPoint()
        => GeoPointValue.Create(-17.7833, -63.1821);

    private static Route CreateValidRoute(Guid? driverId = null) {
        return Route.Create(
            batchId: Guid.NewGuid(),
            deliveryZoneId: Guid.NewGuid(),
            driverId: driverId,
            originLocation: CreateValidGeoPoint()
        );
    }

    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed() {
        // Arrange
        var batchId = Guid.NewGuid();
        var deliveryZoneId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var originLocation = CreateValidGeoPoint();

        // Act
        var route = Route.Create(batchId, deliveryZoneId, driverId, originLocation);

        // Assert
        route.Should().NotBeNull();
        route.Id.Should().NotBe(Guid.Empty);
        route.BatchId.Should().Be(batchId);
        route.DeliveryZoneId.Should().Be(deliveryZoneId);
        route.DriverId.Should().Be(driverId);
        route.OriginLocation.Should().Be(originLocation);
        route.Status.Should().Be(RouteStatusEnum.Pending);
        route.StartedAt.Should().BeNull();
        route.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutDriver_ShouldSucceed() {
        // Arrange
        var batchId = Guid.NewGuid();
        var deliveryZoneId = Guid.NewGuid();
        var originLocation = CreateValidGeoPoint();

        // Act
        var route = Route.Create(batchId, deliveryZoneId, null, originLocation);

        // Assert
        route.Should().NotBeNull();
        route.DriverId.Should().BeNull();
        route.Status.Should().Be(RouteStatusEnum.Pending);
    }

    #endregion

    #region AssignDriver

    [Fact]
    public void AssignDriver_WhenPending_ShouldAssignDriver() {
        // Arrange
        var route = CreateValidRoute(driverId: null);
        var driverId = Guid.NewGuid();

        // Act
        route.AssignDriver(driverId);

        // Assert
        route.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void AssignDriver_WhenAlreadyAssigned_ShouldReassignDriver() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());
        var newDriverId = Guid.NewGuid();

        // Act
        route.AssignDriver(newDriverId);

        // Assert
        route.DriverId.Should().Be(newDriverId);
    }

    [Theory]
    [InlineData(RouteStatusEnum.InProgress)]
    [InlineData(RouteStatusEnum.Completed)]
    [InlineData(RouteStatusEnum.Cancelled)]
    public void AssignDriver_WhenNotPending_ShouldThrowDomainException(RouteStatusEnum status) {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());

        // Change status based on the test case
        if (status == RouteStatusEnum.InProgress) {
            route.Start();
        } else if (status == RouteStatusEnum.Completed) {
            route.Start();
            route.Complete();
        } else if (status == RouteStatusEnum.Cancelled) {
            route.Cancel();
        }

        // Act
        Action act = () => route.AssignDriver(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(RouteErrors.CannotChangeDriverIfNotPending);
    }

    #endregion

    #region UnassignDriver

    [Fact]
    public void UnassignDriver_WhenPending_ShouldUnassignDriver() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());

        // Act
        route.UnassignDriver();

        // Assert
        route.DriverId.Should().BeNull();
    }

    [Theory]
    [InlineData(RouteStatusEnum.InProgress)]
    [InlineData(RouteStatusEnum.Completed)]
    [InlineData(RouteStatusEnum.Cancelled)]
    public void UnassignDriver_WhenNotPending_ShouldThrowDomainException(RouteStatusEnum status) {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());

        if (status == RouteStatusEnum.InProgress) {
            route.Start();
        } else if (status == RouteStatusEnum.Completed) {
            route.Start();
            route.Complete();
        } else if (status == RouteStatusEnum.Cancelled) {
            route.Cancel();
        }

        // Act
        Action act = () => route.UnassignDriver();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(RouteErrors.CannotChangeDriverIfNotPending);
    }

    #endregion

    #region Start

    [Fact]
    public void Start_WhenPendingWithDriver_ShouldStartAndRaiseEvent() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());
        var beforeStart = DateTime.UtcNow;

        // Act
        route.Start();

        // Assert
        route.Status.Should().Be(RouteStatusEnum.InProgress);
        route.StartedAt.Should().NotBeNull();
        route.StartedAt.Should().BeOnOrAfter(beforeStart);
        route.StartedAt.Should().BeOnOrBefore(DateTime.UtcNow);

        route.DomainEvents.Should().ContainSingle(e => e is RouteStarted);
        var startedEvent = route.DomainEvents.OfType<RouteStarted>().Single();
        startedEvent.RouteId.Should().Be(route.Id);
        startedEvent.StartedAt.Should().Be(route.StartedAt.Value);
    }

    [Fact]
    public void Start_WhenPendingWithoutDriver_ShouldThrowDomainException() {
        // Arrange
        var route = CreateValidRoute(driverId: null);

        // Act
        Action act = () => route.Start();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(RouteErrors.DriverIsRequired);
    }

    [Theory]
    [InlineData(RouteStatusEnum.InProgress)]
    [InlineData(RouteStatusEnum.Completed)]
    [InlineData(RouteStatusEnum.Cancelled)]
    public void Start_WhenNotPending_ShouldThrowDomainException(RouteStatusEnum status) {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());

        if (status == RouteStatusEnum.InProgress) {
            route.Start();
        } else if (status == RouteStatusEnum.Completed) {
            route.Start();
            route.Complete();
        } else if (status == RouteStatusEnum.Cancelled) {
            route.Cancel();
        }

        // Act
        Action act = () => route.Start();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(RouteErrors.CannotChangeStatusFromTo(status, RouteStatusEnum.InProgress));
    }

    #endregion

    #region Complete

    [Fact]
    public void Complete_WhenInProgress_ShouldComplete() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());
        route.Start();
        var beforeComplete = DateTime.UtcNow;

        // Act
        route.Complete();

        // Assert
        route.Status.Should().Be(RouteStatusEnum.Completed);
        route.CompletedAt.Should().NotBeNull();
        route.CompletedAt.Should().BeOnOrAfter(beforeComplete);
        route.CompletedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Theory]
    [InlineData(RouteStatusEnum.Pending)]
    [InlineData(RouteStatusEnum.Completed)]
    [InlineData(RouteStatusEnum.Cancelled)]
    public void Complete_WhenNotInProgress_ShouldThrowDomainException(RouteStatusEnum status) {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());

        if (status == RouteStatusEnum.Completed) {
            route.Start();
            route.Complete();
        } else if (status == RouteStatusEnum.Cancelled) {
            route.Cancel();
        }

        // Act
        Action act = () => route.Complete();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(RouteErrors.CannotChangeStatusFromTo(status, RouteStatusEnum.Completed));
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_WhenPending_ShouldCancelAndRaiseEvent() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());
        var beforeCancel = DateTime.UtcNow;

        // Act
        route.Cancel();

        // Assert
        route.Status.Should().Be(RouteStatusEnum.Cancelled);

        route.DomainEvents.Should().ContainSingle(e => e is RouteCancelled);
        var cancelledEvent = route.DomainEvents.OfType<RouteCancelled>().Single();
        cancelledEvent.RouteId.Should().Be(route.Id);
        cancelledEvent.CancelledAt.Should().BeOnOrAfter(beforeCancel);
        cancelledEvent.CancelledAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Cancel_WhenInProgress_ShouldCancelAndRaiseEvent() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());
        route.Start();

        // Act
        route.Cancel();

        // Assert
        route.Status.Should().Be(RouteStatusEnum.Cancelled);
        route.DomainEvents.Should().ContainSingle(e => e is RouteCancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowDomainException() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());
        route.Start();
        route.Complete();

        // Act
        Action act = () => route.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should()
            .Be(RouteErrors.CannotChangeStatusFromTo(RouteStatusEnum.Completed, RouteStatusEnum.Cancelled));
    }

    #endregion

    #region Workflow

    [Fact]
    public void Route_CompleteHappyPath_ShouldSucceed() {
        // Arrange
        var route = CreateValidRoute(driverId: null);

        // Act & Assert - Assign driver
        var driverId = Guid.NewGuid();
        route.AssignDriver(driverId);
        route.DriverId.Should().Be(driverId);

        // Act & Assert - Start route
        route.Start();
        route.Status.Should().Be(RouteStatusEnum.InProgress);
        route.StartedAt.Should().NotBeNull();
        route.DomainEvents.Should().ContainSingle(e => e is RouteStarted);

        // Act & Assert - Complete route
        route.Complete();
        route.Status.Should().Be(RouteStatusEnum.Completed);
        route.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Route_WorkflowWithCancellation_ShouldSucceed() {
        // Arrange
        var route = CreateValidRoute(driverId: Guid.NewGuid());

        // Act & Assert - Start and cancel
        route.Start();
        route.Status.Should().Be(RouteStatusEnum.InProgress);

        route.Cancel();
        route.Status.Should().Be(RouteStatusEnum.Cancelled);
        route.DomainEvents.Should().ContainSingle(e => e is RouteCancelled);
    }

    #endregion
}
