using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Orders;

public class OrderItemTest
{
    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var quantity = 5;

        // Act
        var orderItem = OrderItem.Create(orderId, productId, quantity);

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.Id.Should().NotBe(Guid.Empty);
        orderItem.OrderId.Should().Be(orderId);
        orderItem.ProductId.Should().Be(productId);
        orderItem.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Create_WithDifferentValidQuantities_ShouldSucceed(int quantity)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act
        var orderItem = OrderItem.Create(orderId, productId, quantity);

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Create_WithInvalidQuantity_ShouldThrowDomainException(int invalidQuantity)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act
        Action act = () => OrderItem.Create(orderId, productId, invalidQuantity);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderItemErrors.QuantityMustBeGreaterThanZero);
    }

    #endregion

    #region IncreaseQuantity

    [Fact]
    public void IncreaseQuantity_WithValidQuantity_ShouldIncreaseQuantity()
    {
        // Arrange
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5);
        var initialQuantity = orderItem.Quantity;
        var increaseAmount = 3;

        // Act
        orderItem.IncreaseQuantity(increaseAmount);

        // Assert
        orderItem.Quantity.Should().Be(initialQuantity + increaseAmount);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 3, 8)]
    [InlineData(10, 15, 25)]
    [InlineData(100, 50, 150)]
    public void IncreaseQuantity_WithDifferentValues_ShouldCalculateCorrectly(
        int initialQuantity,
        int increaseAmount,
        int expectedQuantity)
    {
        // Arrange
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), initialQuantity);

        // Act
        orderItem.IncreaseQuantity(increaseAmount);

        // Assert
        orderItem.Quantity.Should().Be(expectedQuantity);
    }

    [Fact]
    public void IncreaseQuantity_MultipleTimes_ShouldAccumulateCorrectly()
    {
        // Arrange
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        // Act
        orderItem.IncreaseQuantity(3);
        orderItem.IncreaseQuantity(2);
        orderItem.IncreaseQuantity(5);

        // Assert
        orderItem.Quantity.Should().Be(15);
    }

    #endregion
}