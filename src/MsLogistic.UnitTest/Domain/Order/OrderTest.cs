using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Order;

public class OrderTest
{
    [Fact]
    public void Order_AddItem_Success()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var scheduledDeliveryDate = DateTime.UtcNow.AddDays(2);
        const string deliveryAddress = "123 Main St";
        var deliveryLocation = new GeoPointValue(
            40.7128,
            -74.0060
        );
        var productId = Guid.NewGuid();
        const int quantity = 5;

        var order = new MsLogistic.Domain.Order.Entities.Order(
            customerId,
            scheduledDeliveryDate,
            deliveryAddress,
            deliveryLocation
        );

        // Act
        order.AddItem(productId, quantity);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(productId, order.Items.First().ProductId);
    }
}