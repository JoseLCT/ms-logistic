using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Integration.Events.Incoming;
using MsLogistic.Application.Integration.Handlers;
using MsLogistic.Application.Orders.CreateOrder;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Application.Integration.Handlers;

public class OnOrderCreatedTest {
	private readonly Mock<IMediator> _mediatorMock;
	private readonly Mock<ICustomerRepository> _customerRepositoryMock;
	private readonly Mock<IProductRepository> _productRepositoryMock;
	private readonly OnOrderCreated _handler;

	public OnOrderCreatedTest() {
		_mediatorMock = new Mock<IMediator>();
		_customerRepositoryMock = new Mock<ICustomerRepository>();
		_productRepositoryMock = new Mock<IProductRepository>();
		var loggerMock = new Mock<ILogger<OnOrderCreated>>();

		_handler = new OnOrderCreated(
			_mediatorMock.Object,
			_customerRepositoryMock.Object,
			_productRepositoryMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task HandleAsync_WhenCustomerNotFound_ShouldLogWarningAndNotSendCommand() {
		// Arrange
		OrderCreatedMessage message = CreateValidMessage();

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(message.CustomerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Customer?)null);

		// Act
		await _handler.HandleAsync(message, CancellationToken.None);

		// Assert
		_productRepositoryMock.Verify(
			r => r.GetByExternalIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
			Times.Never
		);

		_mediatorMock.Verify(
			m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
			Times.Never
		);
	}

	[Fact]
	public async Task HandleAsync_WhenSomeProductsNotFound_ShouldLogWarningAndNotSendCommand() {
		// Arrange
		OrderCreatedMessage message = CreateValidMessage();
		Customer customer = CreateValidCustomer();

		Guid knownProductId = message.Items.First().RecipeId;
		Guid unknownProductId = message.Items.Last().RecipeId;

		var products = new List<Product> {
			CreateProductWithExternalId(knownProductId)
		};

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(message.CustomerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(customer);

		_productRepositoryMock
			.Setup(r => r.GetByExternalIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(products);

		// Act
		await _handler.HandleAsync(message, CancellationToken.None);

		// Assert
		_mediatorMock.Verify(
			m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
			Times.Never
		);
	}

	[Fact]
	public async Task HandleAsync_WhenAllDataIsValid_ShouldSendCreateOrderCommandWithCorrectData() {
		// Arrange
		OrderCreatedMessage message = CreateValidMessage();
		Customer customer = CreateValidCustomer();

		var products = message.Items
			.Select(i => CreateProductWithExternalId(i.RecipeId))
			.ToList();

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(message.CustomerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(customer);

		_productRepositoryMock
			.Setup(r => r.GetByExternalIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(products);

		CreateOrderCommand? capturedCommand = null;

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
			.Callback<IRequest<Result<Guid>>, CancellationToken>((cmd, _) => capturedCommand = (CreateOrderCommand)cmd)
			.ReturnsAsync(Result.Success(Guid.NewGuid()));

		// Act
		await _handler.HandleAsync(message, CancellationToken.None);

		// Assert
		_mediatorMock.Verify(
			m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
			Times.Once
		);

		capturedCommand.Should().NotBeNull();
		capturedCommand!.CustomerId.Should().Be(customer.Id);
		capturedCommand.ScheduledDeliveryDate.Should().Be(message.DeliveryDate);
		capturedCommand.DeliveryAddress.Should().Be(message.DeliveryAddress);
		capturedCommand.DeliveryLocation.Latitude.Should().Be(message.DeliveryLocation.Latitude);
		capturedCommand.DeliveryLocation.Longitude.Should().Be(message.DeliveryLocation.Longitude);
		capturedCommand.Items.Should().HaveCount(message.Items.Count);
	}

	[Fact]
	public async Task HandleAsync_WhenItemsHaveDuplicateRecipeIds_ShouldQueryProductsWithDistinctIds() {
		// Arrange
		var sharedRecipeId = Guid.NewGuid();

		var message = new OrderCreatedMessage {
			CustomerId = Guid.NewGuid(),
			DeliveryDate = DateTime.UtcNow.AddDays(1),
			DeliveryAddress = "Calle Principal 123",
			DeliveryLocation = new DeliveryLocationMessage { Latitude = -17.78, Longitude = -63.18 },
			Items = [
				new OrderItemMessage { RecipeId = sharedRecipeId, Quantity = 2 },
				new OrderItemMessage { RecipeId = sharedRecipeId, Quantity = 3 }
			]
		};

		Customer customer = CreateValidCustomer();
		var products = new List<Product> { CreateProductWithExternalId(sharedRecipeId) };

		_customerRepositoryMock
			.Setup(r => r.GetByExternalIdAsync(message.CustomerId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(customer);

		IReadOnlyCollection<Guid>? capturedIds = null;

		_productRepositoryMock
			.Setup(r => r.GetByExternalIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
			.Callback<IReadOnlyCollection<Guid>, CancellationToken>((ids, _) => capturedIds = ids)
			.ReturnsAsync(products);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Result.Success(Guid.NewGuid()));

		// Act
		await _handler.HandleAsync(message, CancellationToken.None);

		// Assert
		capturedIds.Should().NotBeNull();
		capturedIds.Should().HaveCount(1);
		capturedIds.Should().Contain(sharedRecipeId);
	}

	#region Helper Methods

	private static OrderCreatedMessage CreateValidMessage() {
		var recipeId1 = Guid.NewGuid();
		var recipeId2 = Guid.NewGuid();

		return new OrderCreatedMessage {
			CustomerId = Guid.NewGuid(),
			DeliveryDate = DateTime.UtcNow.AddDays(1),
			DeliveryAddress = "Calle Principal 123",
			DeliveryLocation = new DeliveryLocationMessage { Latitude = -17.78, Longitude = -63.18 },
			Items = [
				new OrderItemMessage { RecipeId = recipeId1, Quantity = 2 },
				new OrderItemMessage { RecipeId = recipeId2, Quantity = 1 }
			]
		};
	}

	private static Customer CreateValidCustomer() {
		return Customer.Create(
			fullName: "Juan Pérez",
			phoneNumber: PhoneNumberValue.Create("+59112345678"),
			externalId: Guid.NewGuid()
		);
	}

	private static Product CreateProductWithExternalId(Guid externalId) {
		var product = Product.Create(
			name: "Producto Test",
			description: "Descripción",
			externalId: externalId
		);
		return product;
	}

	#endregion
}
