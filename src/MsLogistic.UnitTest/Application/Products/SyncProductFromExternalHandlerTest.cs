using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Products.SyncProductFromExternal;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Products;

public class SyncProductFromExternalHandlerTest {
	private readonly Mock<IProductRepository> _productRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly SyncProductFromExternalHandler _handler;

	public SyncProductFromExternalHandlerTest() {
		_productRepositoryMock = new Mock<IProductRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<SyncProductFromExternalHandler>>();

		_handler = new SyncProductFromExternalHandler(
			_productRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WhenProductDoesNotExist_ShouldCreateAndReturnSuccess() {
		// Arrange
		var externalId = Guid.NewGuid();
		var command = new SyncProductFromExternalCommand(externalId, "Laptop", "Gaming laptop");
		Product? capturedProduct = null;

		_productRepositoryMock
			.Setup(x => x.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Product?)null);

		_productRepositoryMock
			.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
			.Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		capturedProduct.Should().NotBeNull();
		capturedProduct.Name.Should().Be(command.Name);
		capturedProduct.Description.Should().Be(command.Description);
		result.Value.Should().Be(capturedProduct.Id);
		_productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
		_productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenProductAlreadyExists_ShouldUpdateAndReturnSuccess() {
		// Arrange
		var externalId = Guid.NewGuid();
		var existingProduct = Product.Create("Old Laptop", "Old description", externalId);
		var command = new SyncProductFromExternalCommand(externalId, "New Laptop", "New description");

		_productRepositoryMock
			.Setup(x => x.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingProduct);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(existingProduct.Id);
		existingProduct.Name.Should().Be(command.Name);
		existingProduct.Description.Should().Be(command.Description);
		_productRepositoryMock.Verify(x => x.Update(existingProduct), Times.Once);
		_productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
