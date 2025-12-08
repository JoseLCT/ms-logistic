using FluentAssertions;
using Moq;
using MsLogistic.Application.Product.CreateProduct;
using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Product.Repositories;
using Xunit;

namespace MsLogistic.Test.Application.Product;

public class CreateProductHandlerTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductHandler _handler;
    
    public CreateProductHandlerTest()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }
    
    [Fact]
    public async Task CreateProductHandler_Handle_ShouldCreateProductSuccessfully()
    {
        // Arrange
        const string productName = "Sample Product";
        const string productDescription = "This is a sample product.";
        var request = new CreateProductCommand(productName, productDescription);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<MsLogistic.Domain.Product.Entities.Product>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.CommitAsync(cancellationToken),
            Times.Once
        );
    }
}