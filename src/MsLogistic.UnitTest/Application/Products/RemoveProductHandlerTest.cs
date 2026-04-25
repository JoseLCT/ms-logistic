using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Products.RemoveProduct;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Products;

public class RemoveProductHandlerTest {
	private readonly Mock<IProductRepository> _productRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly RemoveProductHandler _handler;

	public RemoveProductHandlerTest() {
		_productRepositoryMock = new Mock<IProductRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<RemoveProductHandler>>();

		_handler = new RemoveProductHandler(
			_productRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithExistingProduct_ShouldRemoveProductAndReturnSuccess() {
		// Arrange
		var product = Product.Create("Soup", "Chicken soup");
		var command = new RemoveProductCommand(product.Id);

		_productRepositoryMock
			.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(product);

		// Act
		Result result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_productRepositoryMock.Verify(x => x.Remove(product), Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithNonExistingProduct_ShouldReturnNotFoundErrorAndNotRemoveOrCommit() {
		// Arrange
		var productId = Guid.NewGuid();
		var command = new RemoveProductCommand(productId);

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
		_productRepositoryMock.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
