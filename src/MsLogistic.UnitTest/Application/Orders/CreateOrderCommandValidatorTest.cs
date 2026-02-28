using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Application.Shared.DTOs;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Orders.Errors;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Orders;

public class CreateOrderCommandValidatorTest {
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTest() {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _validator = new CreateOrderCommandValidator(
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object
        );
    }

    #region CustomerId Validation

    [Fact]
    public async Task Validate_WhenCustomerIdIsEmpty_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { CustomerId = Guid.Empty };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public async Task Validate_WhenCustomerDoesNotExist_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorCode(CommonErrors.NotFound("Customer").Code);
    }

    #endregion

    #region Items Validation

    [Fact]
    public async Task Validate_WhenItemsIsEmpty_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Items = new List<CreateOrderItemDto>() };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorCode(OrderErrors.ItemsAreRequired.Code);
    }

    [Fact]
    public async Task Validate_WhenItemsIsNull_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Items = null! };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public async Task Validate_WhenNotAllProductsExist_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        SetupValidCustomer(command.CustomerId);

        var productIds = command.Items.Select(i => i.ProductId).ToHashSet();
        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(productIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { Product.Create("Product 1", "Description") });

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorCode(OrderErrors.ProductsNotFound.Code);
    }

    [Fact]
    public async Task Validate_WhenAllProductsExist_ShouldNotHaveValidationErrorForItems() {
        // Arrange
        var command = CreateValidCommand();
        SetupValidCustomer(command.CustomerId);
        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Items);
    }

    #endregion

    #region Item.ProductId Validation

    [Fact]
    public async Task Validate_WhenItemProductIdIsEmpty_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        var items = command.Items.ToList();
        items[0] = items[0] with { ProductId = Guid.Empty };
        command = command with { Items = items };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    #endregion

    #region Item.Quantity Validation

    [Fact]
    public async Task Validate_WhenItemQuantityIsZero_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        var items = command.Items.ToList();
        items[0] = items[0] with { Quantity = 0 };
        command = command with { Items = items };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorCode(OrderItemErrors.QuantityMustBeGreaterThanZero.Code);
    }

    [Fact]
    public async Task Validate_WhenItemQuantityIsNegative_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        var items = command.Items.ToList();
        items[0] = items[0] with { Quantity = -5 };
        command = command with { Items = items };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorCode(OrderItemErrors.QuantityMustBeGreaterThanZero.Code);
    }

    [Fact]
    public async Task Validate_WhenItemQuantityIsPositive_ShouldNotHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        SetupValidCustomer(command.CustomerId);
        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Items[0].Quantity");
    }

    #endregion

    #region DeliveryAddress Validation

    [Fact]
    public async Task Validate_WhenDeliveryAddressIsEmpty_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { DeliveryAddress = "" };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress)
            .WithErrorCode(OrderErrors.DeliveryAddressIsRequired.Code);
    }

    [Fact]
    public async Task Validate_WhenDeliveryAddressIsNull_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { DeliveryAddress = null! };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress)
            .WithErrorCode(OrderErrors.DeliveryAddressIsRequired.Code);
    }

    [Fact]
    public async Task Validate_WhenDeliveryAddressExceedsMaxLength_ShouldHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { DeliveryAddress = new string('A', 501) };

        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress)
            .WithErrorCode(OrderErrors.DeliveryAddressTooLong(500).Code);
    }

    [Fact]
    public async Task Validate_WhenDeliveryAddressIsAtMaxLength_ShouldNotHaveValidationError() {
        // Arrange
        var command = CreateValidCommand();
        command = command with { DeliveryAddress = new string('A', 500) };

        SetupValidCustomer(command.CustomerId);
        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DeliveryAddress);
    }

    #endregion

    #region Complete Valid Command

    [Fact]
    public async Task Validate_WhenCommandIsCompletelyValid_ShouldNotHaveAnyValidationErrors() {
        // Arrange
        var command = CreateValidCommand();
        SetupValidCustomer(command.CustomerId);
        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Multiple Items Validation

    [Fact]
    public async Task Validate_WhenMultipleItemsHaveInvalidQuantity_ShouldHaveMultipleValidationErrors() {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            ScheduledDeliveryDate: DateTime.UtcNow.AddDays(3),
            DeliveryAddress: "Calle Principal 123, Santa Cruz",
            DeliveryLocation: new CoordinateDto(-17.7833, -63.1821),
            Items: new List<CreateOrderItemDto>
            {
                new(Guid.NewGuid(), 0),
                new(Guid.NewGuid(), -3),
                new(Guid.NewGuid(), 4)
            }
        );

        SetupValidCustomer(command.CustomerId);
        SetupValidProducts(command.Items);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        result.ShouldHaveValidationErrorFor("Items[1].Quantity");
        result.ShouldNotHaveValidationErrorFor("Items[2].Quantity");
    }

    #endregion

    #region Helper Methods

    private static CreateOrderCommand CreateValidCommand() {
        return new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            ScheduledDeliveryDate: DateTime.UtcNow.AddDays(3),
            DeliveryAddress: "Calle Principal 123, Santa Cruz",
            DeliveryLocation: new CoordinateDto(-17.7833, -63.1821),
            Items: new List<CreateOrderItemDto>
            {
                new(Guid.NewGuid(), 2),
                new(Guid.NewGuid(), 5)
            }
        );
    }

    private void SetupValidCustomer(Guid customerId) {
        var customer = Customer.Create("Test Customer", PhoneNumberValue.Create("+1234567890"));
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
    }

    private void SetupValidProducts(IReadOnlyCollection<CreateOrderItemDto> items) {
        var productIds = items.Select(i => i.ProductId).ToHashSet();
        var products = productIds.Select(id => Product.Create($"Product {id}", "Description")).ToList();

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(productIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
    }

    #endregion
}
