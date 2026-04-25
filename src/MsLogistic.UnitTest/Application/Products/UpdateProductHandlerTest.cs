using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Products.UpdateProduct;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Products;

public class UpdateProductHandlerTest {
	private readonly Mock<IProductRepository> _productRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly UpdateProductHandler _handler;

	public UpdateProductHandlerTest() {
		_productRepositoryMock = new Mock<IProductRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<UpdateProductHandler>>();

		_handler = new UpdateProductHandler(
			_productRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithExistingProduct_ShouldUpdateAndReturnSuccess() {
		// Arrange
		var product = Product.Create("Laptop", "Gaming laptop");
		var command = new UpdateProductCommand(product.Id, "New Laptop", "New description");
		Product? capturedProduct = null;

		_productRepositoryMock
			.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(product);

		_productRepositoryMock
			.Setup(x => x.Update(It.IsAny<Product>()))
			.Callback<Product>(p => capturedProduct = p);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		capturedProduct.Should().NotBeNull();
		capturedProduct.Name.Should().Be(command.Name);
		capturedProduct.Description.Should().Be(command.Description);
		_productRepositoryMock.Verify(x => x.Update(product), Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithNonExistingProduct_ShouldReturnNotFoundErrorAndNotUpdateOrCommit() {
		// Arrange
		var productId = Guid.NewGuid();
		var command = new UpdateProductCommand(productId, "New Laptop", "New description");

		_productRepositoryMock
			.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Product?)null);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().NotBeNull();
		result.Error.Type.Should().Be(ErrorType.NotFound);
		result.Error.Code.Should().Contain("Product");
		result.Error.Message.Should().Contain(productId.ToString());
		_productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WithInvalidName_ShouldThrowDomainExceptionAndNotUpdateOrCommit() {
		// Arrange
		var product = Product.Create("Laptop", "Gaming laptop");
		var command = new UpdateProductCommand(product.Id, string.Empty, "New description");

		_productRepositoryMock
			.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(product);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DomainException>();
		_productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
