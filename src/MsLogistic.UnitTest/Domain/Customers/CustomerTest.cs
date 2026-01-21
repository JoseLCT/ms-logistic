using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Customers;

public class CustomerTest
{
    #region Create

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var fullName = "John Doe";
        var phoneNumber = PhoneNumberValue.Create("+59112345678");

        // Act
        var customer = Customer.Create(fullName, phoneNumber);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBe(Guid.Empty);
        customer.FullName.Should().Be(fullName);
        customer.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void Create_WithoutPhoneNumber_ShouldSucceed()
    {
        // Arrange
        var fullName = "Jane Smith";

        // Act
        var customer = Customer.Create(fullName, null);

        // Assert
        customer.Should().NotBeNull();
        customer.FullName.Should().Be(fullName);
        customer.PhoneNumber.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFullName_ShouldThrowDomainException(string invalidFullName)
    {
        // Act
        Action act = () => Customer.Create(invalidFullName, null);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(CustomerErrors.FullNameIsRequired);
    }

    #endregion

    #region SetFullName

    [Fact]
    public void SetFullName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var customer = Customer.Create("John Doe", null);
        var newFullName = "Jane Smith";

        // Act
        customer.SetFullName(newFullName);

        // Assert
        customer.FullName.Should().Be(newFullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetFullName_WithInvalidName_ShouldThrowDomainException(string invalidFullName)
    {
        // Arrange
        var customer = Customer.Create("John Doe", null);

        // Act
        Action act = () => customer.SetFullName(invalidFullName);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(CustomerErrors.FullNameIsRequired);
    }

    #endregion

    #region SetPhoneNumber

    [Fact]
    public void SetPhoneNumber_WithValidPhoneNumber_ShouldUpdatePhoneNumber()
    {
        // Arrange
        var customer = Customer.Create("John Doe", null);
        var phoneNumber = PhoneNumberValue.Create("+59112345678");

        // Act
        customer.SetPhoneNumber(phoneNumber);

        // Assert
        customer.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void SetPhoneNumber_WithNull_ShouldRemovePhoneNumber()
    {
        // Arrange
        var phoneNumber = PhoneNumberValue.Create("+59112345678");
        var customer = Customer.Create("John Doe", phoneNumber);

        // Act
        customer.SetPhoneNumber(null);

        // Assert
        customer.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void SetPhoneNumber_ChangingExistingPhoneNumber_ShouldUpdate()
    {
        // Arrange
        var oldPhone = PhoneNumberValue.Create("+59112345678");
        var customer = Customer.Create("John Doe", oldPhone);
        var newPhone = PhoneNumberValue.Create("+59187654321");

        // Act
        customer.SetPhoneNumber(newPhone);

        // Assert
        customer.PhoneNumber.Should().Be(newPhone);
        customer.PhoneNumber.Should().NotBe(oldPhone);
    }

    #endregion
}