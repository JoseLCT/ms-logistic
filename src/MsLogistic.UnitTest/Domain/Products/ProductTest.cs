using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Errors;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Products;

public class ProductTest {
    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed() {
        // Arrange
        var name = "Soup";
        var description = "Delicious chicken soup";

        // Act
        var product = Product.Create(name, description);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBe(Guid.Empty);
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed() {
        // Arrange
        var name = "Salad";

        // Act
        var product = Product.Create(name, null);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowDomainException(string invalidName) {
        // Act
        Action act = () => Product.Create(invalidName, "Description");

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(ProductErrors.NameIsRequired);
    }

    #endregion

    #region SetName

    [Fact]
    public void SetName_WithValidName_ShouldUpdateName() {
        // Arrange
        var product = Product.Create("Old Name", "Description");
        var newName = "New Product Name";

        // Act
        product.SetName(newName);

        // Assert
        product.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_WithInvalidName_ShouldThrowDomainException(string invalidName) {
        // Arrange
        var product = Product.Create("Valid Name", "Description");

        // Act
        Action act = () => product.SetName(invalidName);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(ProductErrors.NameIsRequired);
    }

    #endregion

    #region SetDescription

    [Fact]
    public void SetDescription_WithValidDescription_ShouldUpdateDescription() {
        // Arrange
        var product = Product.Create("Product Name", "Old description");
        var newDescription = "Updated description";

        // Act
        product.SetDescription(newDescription);

        // Assert
        product.Description.Should().Be(newDescription);
    }

    [Fact]
    public void SetDescription_WithNull_ShouldSetToNull() {
        // Arrange
        var product = Product.Create("Product Name", "Old description");

        // Act
        product.SetDescription(null);

        // Assert
        product.Description.Should().BeNull();
    }

    [Fact]
    public void SetDescription_WithEmptyString_ShouldSetToEmptyString() {
        // Arrange
        var product = Product.Create("Product Name", "Old description");

        // Act
        product.SetDescription("");

        // Assert
        product.Description.Should().Be("");
    }

    #endregion
}
