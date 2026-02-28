using FluentAssertions;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Orders;

public class OrderDeliveryTest {
    private static GeoPointValue CreateValidGeoPoint()
        => GeoPointValue.Create(-17.7833, -63.1821);

    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed() {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var location = CreateValidGeoPoint();
        var deliveredAt = DateTime.UtcNow;
        var comments = "Delivered successfully";
        var imageUrl = "image.jpg";

        // Act
        var orderDelivery = OrderDelivery.Create(
            orderId,
            driverId,
            location,
            deliveredAt,
            comments,
            imageUrl
        );

        // Assert
        orderDelivery.Should().NotBeNull();
        orderDelivery.Id.Should().NotBe(Guid.Empty);
        orderDelivery.OrderId.Should().Be(orderId);
        orderDelivery.DriverId.Should().Be(driverId);
        orderDelivery.Location.Should().Be(location);
        orderDelivery.DeliveredAt.Should().Be(deliveredAt);
        orderDelivery.Comments.Should().Be(comments);
        orderDelivery.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void Create_WithNullComments_ShouldSucceed() {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var location = CreateValidGeoPoint();
        var deliveredAt = DateTime.UtcNow;
        string? comments = null;
        var imageUrl = "image.jpg";

        // Act
        var orderDelivery = OrderDelivery.Create(
            orderId,
            driverId,
            location,
            deliveredAt,
            comments,
            imageUrl
        );

        // Assert
        orderDelivery.Should().NotBeNull();
        orderDelivery.Comments.Should().BeNull();
        orderDelivery.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void Create_WithNullImageUrl_ShouldSucceed() {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var location = CreateValidGeoPoint();
        var deliveredAt = DateTime.UtcNow;
        var comments = "Delivered successfully";
        string? imageUrl = null;

        // Act
        var orderDelivery = OrderDelivery.Create(
            orderId,
            driverId,
            location,
            deliveredAt,
            comments,
            imageUrl
        );

        // Assert
        orderDelivery.Should().NotBeNull();
        orderDelivery.Comments.Should().Be(comments);
        orderDelivery.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullCommentsAndImageUrl_ShouldSucceed() {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var location = CreateValidGeoPoint();
        var deliveredAt = DateTime.UtcNow;
        string? comments = null;
        string? imageUrl = null;

        // Act
        var orderDelivery = OrderDelivery.Create(
            orderId,
            driverId,
            location,
            deliveredAt,
            comments,
            imageUrl
        );

        // Assert
        orderDelivery.Should().NotBeNull();
        orderDelivery.OrderId.Should().Be(orderId);
        orderDelivery.DriverId.Should().Be(driverId);
        orderDelivery.Location.Should().Be(location);
        orderDelivery.DeliveredAt.Should().Be(deliveredAt);
        orderDelivery.Comments.Should().BeNull();
        orderDelivery.ImageUrl.Should().BeNull();
    }

    #endregion
}
