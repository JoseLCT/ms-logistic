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
		string name = "Soup";
		string description = "Delicious chicken soup";

		// Act
		var product = Product.Create(name, description);

		// Assert
		product.Should().NotBeNull();
		product.Id.Should().NotBe(Guid.Empty);
		product.Name.Should().Be(name);
		product.Description.Should().Be(description);
		product.ExternalId.Should().BeNull();
		product.UpdatedAt.Should().BeNull();
	}

	[Fact]
	public void Create_WithExternalId_ShouldSetExternalId() {
		// Arrange
		string name = "Soup";
		string description = "Delicious chicken soup";
		var externalId = Guid.NewGuid();

		// Act
		var product = Product.Create(name, description, externalId);

		// Assert
		product.ExternalId.Should().Be(externalId);
	}

	[Fact]
	public void Create_WithNullDescription_ShouldSucceed() {
		// Arrange
		string name = "Salad";

		// Act
		var product = Product.Create(name, null);

		// Assert
		product.Should().NotBeNull();
		product.Name.Should().Be(name);
		product.Description.Should().BeNull();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Create_WithInvalidName_ShouldThrowDomainException(string? invalidName) {
		// Act
		Action act = () => Product.Create(invalidName!, "Description");

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
		string newName = "New Product Name";

		// Act
		product.SetName(newName);

		// Assert
		product.Name.Should().Be(newName);
	}

	[Fact]
	public void SetName_WithValidName_ShouldMarkAsUpdated() {
		// Arrange
		var product = Product.Create("Old Name", "Description");

		// Act
		product.SetName("New Name");

		// Assert
		product.UpdatedAt.Should().NotBeNull();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void SetName_WithInvalidName_ShouldThrowDomainException(string? invalidName) {
		// Arrange
		var product = Product.Create("Valid Name", "Description");

		// Act
		Action act = () => product.SetName(invalidName!);

		// Assert
		act.Should().Throw<DomainException>()
			.Which.Error.Should().Be(ProductErrors.NameIsRequired);
	}

	[Fact]
	public void SetName_WithInvalidName_ShouldNotChangeNameOrMarkAsUpdated() {
		// Arrange
		var product = Product.Create("Original Name", "Description");

		// Act
		Action act = () => product.SetName("");

		// Assert
		act.Should().Throw<DomainException>();
		product.Name.Should().Be("Original Name");
		product.UpdatedAt.Should().BeNull();
	}

	#endregion

	#region SetDescription

	[Fact]
	public void SetDescription_WithValidDescription_ShouldUpdateDescription() {
		// Arrange
		var product = Product.Create("Product Name", "Old description");
		string newDescription = "Updated description";

		// Act
		product.SetDescription(newDescription);

		// Assert
		product.Description.Should().Be(newDescription);
	}

	[Fact]
	public void SetDescription_WithValidDescription_ShouldMarkAsUpdated() {
		// Arrange
		var product = Product.Create("Product Name", "Old description");

		// Act
		product.SetDescription("New description");

		// Assert
		product.UpdatedAt.Should().NotBeNull();
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
