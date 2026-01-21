using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Orders.Enums;
using MsLogistic.Domain.Orders.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Orders;

public class OrderIncidentTest
{
    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var incidentType = OrderIncidentTypeEnum.IncorrectAddress;
        var description = "Wrong delivery address provided";

        // Act
        var orderIncident = OrderIncident.Create(
            orderId,
            driverId,
            incidentType,
            description
        );

        // Assert
        orderIncident.Should().NotBeNull();
        orderIncident.Id.Should().NotBe(Guid.Empty);
        orderIncident.OrderId.Should().Be(orderId);
        orderIncident.DriverId.Should().Be(driverId);
        orderIncident.IncidentType.Should().Be(incidentType);
        orderIncident.Description.Should().Be(description);
    }

    [Theory]
    [InlineData(OrderIncidentTypeEnum.IncorrectAddress)]
    [InlineData(OrderIncidentTypeEnum.AbsentRecipient)]
    [InlineData(OrderIncidentTypeEnum.DamagedPackage)]
    [InlineData(OrderIncidentTypeEnum.Other)]
    public void Create_WithDifferentIncidentTypes_ShouldSucceed(OrderIncidentTypeEnum incidentType)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var description = "Test incident description";

        // Act
        var orderIncident = OrderIncident.Create(
            orderId,
            driverId,
            incidentType,
            description
        );

        // Assert
        orderIncident.Should().NotBeNull();
        orderIncident.IncidentType.Should().Be(incidentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldThrowDomainException(string invalidDescription)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var incidentType = OrderIncidentTypeEnum.Other;

        // Act
        Action act = () => OrderIncident.Create(
            orderId,
            driverId,
            incidentType,
            invalidDescription
        );

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(OrderIncidentErrors.DescriptionIsRequired);
    }

    #endregion
}