using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Products.GetProductById;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Products;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Products;

public class GetProductByIdHandlerTest : IDisposable {
    private readonly PersistenceDbContext _dbContext;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTest() {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetProductByIdHandler(_dbContext);
    }

    public void Dispose() {
        _dbContext.Dispose();
    }

    private static ProductPersistenceModel CreateProductPersistenceModel(
        Guid? id = null,
        string name = "Sample Product",
        string description = "This is a sample product description."
    ) {
        return new ProductPersistenceModel {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithExistingProductId_ShouldReturnProduct() {
        // Arrange
        var newProduct = CreateProductPersistenceModel();

        await _dbContext.Products.AddAsync(newProduct);
        await _dbContext.SaveChangesAsync();

        var query = new GetProductByIdQuery(newProduct.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(newProduct.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistingProductId_ShouldReturnNotFoundError() {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var query = new GetProductByIdQuery(nonExistingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommonErrors.NotFoundById("Product", nonExistingId));
    }

    [Fact]
    public async Task Handle_WithMultipleProducts_ShouldReturnCorrectProduct() {
        // Arrange
        var product1 = CreateProductPersistenceModel();
        var product2 = CreateProductPersistenceModel();

        await _dbContext.Products.AddRangeAsync(product1, product2);
        await _dbContext.SaveChangesAsync();

        var query = new GetProductByIdQuery(product2.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product2.Id);
    }
}
