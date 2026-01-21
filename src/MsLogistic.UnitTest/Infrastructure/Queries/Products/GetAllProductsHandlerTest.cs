using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Products.GetAllProducts;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.Infrastructure.Queries.Products;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Queries.Products;

public class GetAllProductsHandlerTest : IDisposable
{
    private readonly PersistenceDbContext _dbContext;
    private readonly GetAllProductsHandler _handler;

    public GetAllProductsHandlerTest()
    {
        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PersistenceDbContext(options);
        _handler = new GetAllProductsHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static ProductPersistenceModel CreateProductPersistenceModel(
        Guid? id = null,
        string name = "Sample Product",
        string description = "This is a sample product description."
    )
    {
        return new ProductPersistenceModel
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public async Task Handle_WithNoProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSingleProduct_ShouldReturnListWithOneProduct()
    {
        // Arrange
        var product = CreateProductPersistenceModel();

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(product.Id);
        result.Value[0].Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task Handle_WithMultipleProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var product1 = CreateProductPersistenceModel();
        var product2 = CreateProductPersistenceModel();

        await _dbContext.Products.AddRangeAsync(product1, product2);
        await _dbContext.SaveChangesAsync();

        var query = new GetAllProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }
}