using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MsLogistic.Application.Products.CreateProduct;
using MsLogistic.Core.Interfaces;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Products.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Application.Products;

public class CreateProductHandlerTest {
	private readonly Mock<IProductRepository> _productRepositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly CreateProductHandler _handler;

	public CreateProductHandlerTest() {
		_productRepositoryMock = new Mock<IProductRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		var loggerMock = new Mock<ILogger<CreateProductHandler>>();

		_handler = new CreateProductHandler(
			_productRepositoryMock.Object,
			_unitOfWorkMock.Object,
			loggerMock.Object
		);
	}

	[Fact]
	public async Task Handle_WithValidCommand_ShouldCreateProductAndReturnSuccess() {
		// Arrange
		var command = new CreateProductCommand("Laptop", "Gaming laptop");
		Product? capturedProduct = null;

		_productRepositoryMock
			.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
			.Callback<Product, CancellationToken>((product, _) => capturedProduct = product)
			.Returns(Task.CompletedTask);

		// Act
		Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Error.Should().BeNull();
		capturedProduct.Should().NotBeNull();
		capturedProduct.Name.Should().Be(command.Name);
		capturedProduct.Description.Should().Be(command.Description);
		result.Value.Should().Be(capturedProduct.Id);
		_productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithInvalidCommand_ShouldThrowDomainExceptionAndNotAddOrCommit() {
		// Arrange
		var command = new CreateProductCommand(string.Empty, "Gaming laptop");

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DomainException>();
		_productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
		_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
