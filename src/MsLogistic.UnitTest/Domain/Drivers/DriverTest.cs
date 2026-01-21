using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Drivers.Enums;
using MsLogistic.Domain.Drivers.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Drivers;

public class DriverTest
{
    #region Create

    [Fact]
    public void Create_WithValidFullName_ShouldSucceed()
    {
        // Arrange
        var fullName = "John Doe";

        // Act
        var driver = Driver.Create(fullName);

        // Assert
        driver.Should().NotBeNull();
        driver.Id.Should().NotBe(Guid.Empty);
        driver.FullName.Should().Be(fullName);
        driver.IsActive.Should().BeTrue();
        driver.Status.Should().Be(DriverStatusEnum.Available);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFullName_ShouldThrowDomainException(string invalidFullName)
    {
        // Act
        Action act = () => Driver.Create(invalidFullName);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DriverErrors.FullNameIsRequired);
    }

    #endregion

    #region SetFullName

    [Fact]
    public void SetFullName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var driver = Driver.Create("John Doe");
        var newFullName = "Jane Smith";

        // Act
        driver.SetFullName(newFullName);

        // Assert
        driver.FullName.Should().Be(newFullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetFullName_WithInvalidName_ShouldThrowDomainException(string invalidFullName)
    {
        // Arrange
        var driver = Driver.Create("John Doe");

        // Act
        Action act = () => driver.SetFullName(invalidFullName);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DriverErrors.FullNameIsRequired);
    }

    #endregion

    #region SetIsActive

    [Fact]
    public void SetIsActive_ToFalse_ShouldDeactivateDriver()
    {
        // Arrange
        var driver = Driver.Create("John Doe");

        // Act
        driver.SetIsActive(false);

        // Assert
        driver.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetIsActive_ToTrue_ShouldActivateDriver()
    {
        // Arrange
        var driver = Driver.Create("John Doe");
        driver.SetIsActive(false);

        // Act
        driver.SetIsActive(true);

        // Assert
        driver.IsActive.Should().BeTrue();
    }

    #endregion

    #region SetStatus

    [Theory]
    [InlineData(DriverStatusEnum.Available)]
    [InlineData(DriverStatusEnum.Unavailable)]
    public void SetStatus_WithValidStatus_ShouldUpdateStatus(DriverStatusEnum newStatus)
    {
        // Arrange
        var driver = Driver.Create("John Doe");

        // Act
        driver.SetStatus(newStatus);

        // Assert
        driver.Status.Should().Be(newStatus);
    }

    #endregion
}