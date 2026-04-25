using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Application.Products.GetProductById;
using MsLogistic.Core.Results;
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
		DbContextOptions<PersistenceDbContext> options = new DbContextOptionsBuilder<PersistenceDbContext>()
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
		Guid? externalId = null,
		string name = "Sample Product",
		string? description = "This is a sample product description."
	) {
		return new ProductPersistenceModel {
			Id = id ?? Guid.NewGuid(),
			ExternalId = externalId ?? Guid.NewGuid(),
			Name = name,
			Description = description,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = null
		};
	}

	[Fact]
	public async Task Handle_WithExistingProductId_ShouldReturnProductWithAllFieldsMapped() {
		// Arrange
		ProductPersistenceModel newProduct = CreateProductPersistenceModel(
			name: "Wireless Mouse",
			description: "Ergonomic wireless mouse with USB-C charging"
		);

		await _dbContext.Products.AddAsync(newProduct);
		await _dbContext.SaveChangesAsync();

		var query = new GetProductByIdQuery(newProduct.Id);

		// Act
		Result<ProductDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(newProduct.Id);
		result.Value.Name.Should().Be("Wireless Mouse");
		result.Value.Description.Should().Be("Ergonomic wireless mouse with USB-C charging");
	}

	[Fact]
	public async Task Handle_WithProductWithNullDescription_ShouldMapDescriptionAsNull() {
		// Arrange
		ProductPersistenceModel newProduct = CreateProductPersistenceModel(
			name: "Wireless Mouse",
			description: null
		);

		await _dbContext.Products.AddAsync(newProduct);
		await _dbContext.SaveChangesAsync();

		var query = new GetProductByIdQuery(newProduct.Id);

		// Act
		Result<ProductDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Name.Should().Be("Wireless Mouse");
		result.Value.Description.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithNonExistingProductId_ShouldReturnNotFoundError() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var query = new GetProductByIdQuery(nonExistingId);

		// Act
		Result<ProductDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Product", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithNonExistingIdAndOtherProductsInDb_ShouldReturnNotFoundError() {
		// Arrange
		ProductPersistenceModel product1 = CreateProductPersistenceModel();
		ProductPersistenceModel product2 = CreateProductPersistenceModel();

		await _dbContext.Products.AddRangeAsync(product1, product2);
		await _dbContext.SaveChangesAsync();

		var nonExistingId = Guid.NewGuid();
		var query = new GetProductByIdQuery(nonExistingId);

		// Act
		Result<ProductDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(CommonErrors.NotFoundById("Product", nonExistingId));
	}

	[Fact]
	public async Task Handle_WithMultipleProducts_ShouldReturnCorrectProduct() {
		// Arrange
		ProductPersistenceModel product1 = CreateProductPersistenceModel(
			name: "Wireless Mouse",
			description: "Mouse description"
		);
		ProductPersistenceModel product2 = CreateProductPersistenceModel(
			name: "Mechanical Keyboard",
			description: "Keyboard description"
		);
		ProductPersistenceModel product3 = CreateProductPersistenceModel(
			name: "USB-C Hub",
			description: "Hub description"
		);

		await _dbContext.Products.AddRangeAsync(product1, product2, product3);
		await _dbContext.SaveChangesAsync();

		var query = new GetProductByIdQuery(product2.Id);

		// Act
		Result<ProductDetailDto> result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(product2.Id);
		result.Value.Name.Should().Be("Mechanical Keyboard");
		result.Value.Description.Should().Be("Keyboard description");
	}
}
